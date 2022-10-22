using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikCAD.CustomControls;
using MikCAD.Objects.ParameterizedObjects.Milling;
using MikCAD.Utilities;
using OpenTK.Mathematics;

namespace MikCAD;

public class Simulator3C : INotifyPropertyChanged
{
    public static Simulator3C Simulator;

    public bool IsAnimationRunning { get; private set; } = false;

    public bool Enabled { get; set; }
    public bool IgnoreDepth { get; set; }
    public bool ShowLines { get; set; } = true;

    #region Light

    public float LightPosX { get; set; }
    public float LightPosY { get; set; }
    public float LightPosZ { get; set; }

    public float ka { get; set; } = 0.2f;
    public float ks { get; set; } = 0.2f;
    public float kd { get; set; } = 0.5f;
    public float m { get; set; } = 100;

    #endregion

    private uint _xGridSizeInUnits = 18;

    public uint XGridSizeInUnits
    {
        get => _xGridSizeInUnits;
        set
        {
            if (value > 0 && !IsAnimationRunning)
            {
                _xGridSizeInUnits = value;
                Scene.CurrentScene.ObjectsController.Block.UpdateVertices();
            }
        }
    }

    private uint _yGridSizeInUnits = 18;

    public uint YGridSizeInUnits
    {
        get => _yGridSizeInUnits;
        set
        {
            if (value > 0 && !IsAnimationRunning)
            {
                _yGridSizeInUnits = value;
                Scene.CurrentScene.ObjectsController.Block.UpdateVertices();
            }
        }
    }

    private uint _zGridSizeInUnits = 5;

    public uint ZGridSizeInUnits
    {
        get => _zGridSizeInUnits;
        set
        {
            if (value > 0 && !IsAnimationRunning)
            {
                _zGridSizeInUnits = value;
                Scene.CurrentScene.ObjectsController.Block.UpdateVertices();
            }
        }
    }

    private uint _xGridDivisions = 8000;

    public uint XGridDivisions
    {
        get => _xGridDivisions;
        set
        {
            if (value > 0 && !IsAnimationRunning)
            {
                _xGridDivisions = value;
                Scene.CurrentScene.ObjectsController.Block.UpdateVertices();
                UpdateBlockHeightMap();
            }
        }
    }

    private uint _yGridDivisions = 8000;

    public uint YGridDivisions
    {
        get => _yGridDivisions;
        set
        {
            if (value > 0 && !IsAnimationRunning)
            {
                _yGridDivisions = value;
                Scene.CurrentScene.ObjectsController.Block.UpdateVertices();
                UpdateBlockHeightMap();
            }
        }
    }

    private uint _maxCutterImmersionInMm = 10;

    public uint MaxCutterImmersionInMm
    {
        get => _maxCutterImmersionInMm;
        set { _maxCutterImmersionInMm = value; }
    }

    public CutterType CutterType { get; set; }

    public bool SphericalSelected
    {
        get => CutterType == CutterType.Spherical;
        set
        {
            if (!IsAnimationRunning)
            {
                CutterType = value ? CutterType.Spherical : CutterType.Flat;
            }
        }
    }

    public bool FlatSelected
    {
        get => CutterType == CutterType.Flat;
        set
        {
            if (!IsAnimationRunning)
            {
                CutterType = value ? CutterType.Flat : CutterType.Spherical;
            }
        }
    }

    private uint _cutterDiameterInMm = 15;

    public uint CutterDiameterInMm
    {
        get => _cutterDiameterInMm;
        set
        {
            if (!IsAnimationRunning)
            {
                _cutterDiameterInMm = value;
            }
        }
    }

    private uint _simulationSpeed = 1;

    public uint SimulationSpeed
    {
        get => _simulationSpeed;
        set
        {
            if (value > 0 && value <= 5 && !IsAnimationRunning)
            {
                _simulationSpeed = value;
                speedInUnitsPerSecond = (float) _simulationSpeed / 5 * maxSpeedInMm * MmToUnits;
            }
        }
    }

    private String _fileName = "Brak pliku";

    public String FileName
    {
        get => _fileName;
        set
        {
            if (!IsAnimationRunning)
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }
    }

    public Simulator3C()
    {
        Simulator = this;
    }

    private Regex _moveLineRegex = new Regex(@"(N\d+)G01(X-?\d+.\d{3})?(Y-?\d+.\d{3})?(Z-?\d+.\d{3})?");

    public (bool, SimulatorErrorCode) ParsePathFile(string diagFileName, string[] lines)
    {
        var indexOfDot = diagFileName.IndexOf('.');
        if (indexOfDot + 1 >= diagFileName.Length)
        {
            return (false, SimulatorErrorCode.WrongFileName);
        }

        var cutterTypeChar = diagFileName[indexOfDot + 1];
        switch (cutterTypeChar)
        {
            case 'k':
                CutterType = CutterType.Spherical;
                break;
            case 'f':
                CutterType = CutterType.Flat;
                break;
            default:
                return (false, SimulatorErrorCode.WrongCutterType);
        }

        OnPropertyChanged(nameof(CutterType));
        OnPropertyChanged(nameof(SphericalSelected));
        OnPropertyChanged(nameof(FlatSelected));

        var cutterSizeStr = diagFileName.Substring(indexOfDot + 2, 2);
        UInt32 cutterSize;
        if (UInt32.TryParse(cutterSizeStr, out cutterSize))
        {
            CutterDiameterInMm = cutterSize;
        }
        else
        {
            return (false, SimulatorErrorCode.WrongCutterSize);
        }

        OnPropertyChanged(nameof(CutterDiameterInMm));


        int lineNumber = 0;
        List<CuttingLinePoint> points = new List<CuttingLinePoint>(capacity: lines.Length);
        float XLastPosInMm = 0;
        float YLastPosInMm = 0;
        float ZLastPosInMm = 0;
        int LastInstructionNum = -1;

        foreach (var line in lines)
        {
            var matchCollection = _moveLineRegex.Matches(line, 0);
            if (matchCollection.Count == 1 && matchCollection[0].Length == line.Length)
            {
                foreach (var group in matchCollection[0].Groups)
                {
                    var groupStr = group.ToString();
                    float posValue;

                    if (groupStr.Length < 1 || !float.TryParse(groupStr.Substring(1), NumberStyles.Float,
                            CultureInfo.InvariantCulture, out posValue))
                    {
                        continue;
                    }

                    switch (groupStr[0])
                    {
                        case 'N':
                            var instructionNumber = (int) posValue;
                            if (LastInstructionNum != -1)
                            {
                                if (instructionNumber != LastInstructionNum + 1)
                                {
                                    return (false, SimulatorErrorCode.MissingInstructions);
                                }
                            }

                            LastInstructionNum = instructionNumber;
                            break;
                        case 'X':
                            XLastPosInMm = posValue;
                            break;
                        case 'Y':
                            YLastPosInMm = posValue;
                            break;
                        case 'Z':
                            ZLastPosInMm = posValue;
                            break;
                    }
                }

                points.Add(new CuttingLinePoint()
                {
                    InstructionNumber = LastInstructionNum,
                    XPosInMm = XLastPosInMm,
                    YPosInMm = YLastPosInMm,
                    ZPosInMm = ZLastPosInMm
                });
            }
            else
            {
                return (false, SimulatorErrorCode.UnsupportedCommand);
            }

            lineNumber++;
        }


        FileName = diagFileName.Substring(diagFileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);
        Scene.CurrentScene.ObjectsController.Path.CuttingLines = new CuttingLines()
        {
            points = points.ToArray()
        };
        var pathPoints = Scene.CurrentScene.ObjectsController.Path.CuttingLines.points;
        Scene.CurrentScene.ObjectsController.Cutter.posX = pathPoints[0].XPosInUnits;
        Scene.CurrentScene.ObjectsController.Cutter.posY = pathPoints[0].ZPosInUnits;
        Scene.CurrentScene.ObjectsController.Cutter.posZ = -pathPoints[0].YPosInUnits;

        return (true, SimulatorErrorCode.None);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }


    private float maxSpeedInMm = 1f;
    private float MmToUnits = 0.1f;
    private float UnitsToMm = 10f;
    private float dt = 1 / 5.0f;
    private float speedInUnitsPerSecond = 0;
    private Torus cutter;
    private Block block;
    private BackgroundWorker cutterThread;

    public Simulator3CControl _simulator3CControl;

    public void StartMilling()
    {
        if (Scene.CurrentScene.ObjectsController.Path is {CuttingLines: {points: { }}})
        {
            _simulator3CControl.MaterialSettings.IsEnabled = false;
            _simulator3CControl.CutterSettings.IsEnabled = false;
            IsAnimationRunning = true;

            cutter = Scene.CurrentScene.ObjectsController.Cutter;
            var pathPoints = Scene.CurrentScene.ObjectsController.Path.CuttingLines.points;
            cutter.posX = pathPoints[0].XPosInUnits;
            cutter.posY = pathPoints[0].ZPosInUnits;
            cutter.posZ = -pathPoints[0].YPosInUnits;

            block = Scene.CurrentScene.ObjectsController.Block;

            speedInUnitsPerSecond = (float) _simulationSpeed / 5 * maxSpeedInMm * MmToUnits;

            cutterThread = new BackgroundWorker();
            cutterThread.WorkerReportsProgress = true;
            cutterThread.WorkerSupportsCancellation = true;
            cutterThread.DoWork += (sender, args) => MoveCutter(sender, args, pathPoints);
            cutterThread.ProgressChanged +=
                (sender, args) => _simulator3CControl.UpdateProgressBar(args.ProgressPercentage);
            cutterThread.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Result is SimulatorErrorCode code)
                {
                    switch (code)
                    {
                        case SimulatorErrorCode.FlatHeadMoveDownWhileMilling:
                        case SimulatorErrorCode.MoveBelowSafeLimit:
                            MessageBox.Show($"Błąd podczas frezowania\nKod błędu: {code}", "Błąd",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        default:
                            break;
                    }
                }
                IsAnimationRunning = false;
                _simulator3CControl.MaterialSettings.IsEnabled = true;
                _simulator3CControl.CutterSettings.IsEnabled = true;
            };
            cutterThread.RunWorkerAsync();
        }
    }

    public void StopSimulation()
    {
        cutterThread?.CancelAsync();
    }
    
    private void MoveCutter(object sender, DoWorkEventArgs e, CuttingLinePoint[] points)
    {
        int i = 1;
        var startPos = new Vector3(points[0].GetPosInUnitsYZSwitched());
        var currPos = cutter.pos;
        currPos.Z = -currPos.Z;
        var endPos = new Vector3(points[1].GetPosInUnitsYZSwitched());
        var dir = new Vector3(endPos.X - startPos.X, endPos.Y - startPos.Y, endPos.Z - startPos.Z).Normalized();

        var lastXDiffSign = endPos.X - startPos.X > 0;
        var lastYDiffSign = endPos.Y - startPos.Y > 0;
        var lastZDiffSign = endPos.Z - startPos.Z > 0;

        float rInUnits = (CutterDiameterInMm / 2 * 0.1f);

        while (i < points.Length)
        {
            if (sender is BackgroundWorker b)
            {
                if (b.CancellationPending)
                {
                    return;
                }
            }
            var lenToNextPoint = MathM.Distance(currPos, endPos);
            var distLeft = speedInUnitsPerSecond * dt;


            while (distLeft > Single.Epsilon)
            {
                var currXDiffSign = endPos.X - currPos.X > 0;
                var currYDiffSign = endPos.Y - currPos.Y > 0;
                var currZDiffSign = endPos.Z - currPos.Z > 0;

                if (lenToNextPoint > distLeft
                    && lenToNextPoint > 0.01f
                    && currXDiffSign == lastXDiffSign
                    && currYDiffSign == lastYDiffSign
                    && currZDiffSign == lastZDiffSign)
                {
                    var totalMilled = block.UpdateHeightMap(Unswap(currPos), dir, distLeft, rInUnits);
                    if (totalMilled > Single.Epsilon && FlatSelected && dir.Y < -Single.Epsilon)
                    {
                        e.Result = SimulatorErrorCode.FlatHeadMoveDownWhileMilling;
                        return;
                    }

                    cutter.posX += dir.X * distLeft;
                    cutter.posY += dir.Y * distLeft;
                    cutter.posZ -= dir.Z * distLeft;
                    currPos = cutter.pos;
                    currPos.Z = -currPos.Z;
                    //Trace.WriteLine(currPos);
                    break;
                }
                else
                {
                    i++;
                    (sender as BackgroundWorker).ReportProgress((int) ((float) i / points.Length * 100));
                    if (i >= points.Length)
                    {
                        cutter.posX = points[^1].XPosInUnits;
                        cutter.posY = points[^1].ZPosInUnits;
                        cutter.posZ = -points[^1].YPosInUnits;
                        IsAnimationRunning = false;
                        return;
                    }

                    block.UpdateHeightMapInPoint(Unswap(currPos), rInUnits);

                    distLeft -= MathM.Distance(currPos, endPos);
                    currPos = startPos = endPos;
                    cutter.posX = currPos.X;
                    cutter.posY = currPos.Y;
                    cutter.posZ = -currPos.Z;

                    endPos = points[i].GetPosInUnitsYZSwitched();
                    if (endPos.Y * UnitsToMm < _maxCutterImmersionInMm)
                    {
                        e.Result = SimulatorErrorCode.MoveBelowSafeLimit;
                        return;
                    }

                    dir = new Vector3(endPos.X - startPos.X, endPos.Y - startPos.Y, endPos.Z - startPos.Z).Normalized();
                    lenToNextPoint = MathM.Distance(currPos, endPos);
                    lastXDiffSign = endPos.X - currPos.X > 0;
                    lastYDiffSign = endPos.Y - currPos.Y > 0;
                    lastZDiffSign = endPos.Z - currPos.Z > 0;
                }
            }

            Task.Delay(TimeSpan.FromSeconds(dt));
        }

        IsAnimationRunning = false;
    }

    private Vector3 Unswap(Vector3 currPos)
    {
        (currPos.Y, currPos.Z) = (currPos.Z, currPos.Y);
        return currPos;
    }

    public void UpdateBlockHeightMap()
    {
        var texture1 = new List<float>((int) XGridDivisions * (int) YGridDivisions);

        for (int j = 0; j < YGridDivisions; j++)
        {
            for (int i = 0; i < XGridDivisions; i++)
            {
                var color = 5.0f;
                // if (i > Simulator3C.XGridDivisions / 3.0f && i < 2.0f / 3 * Simulator3C.XGridDivisions)
                // {
                //     color = 0.5f;
                // }


                texture1.Add(color);
            }
        }

        Scene.CurrentScene.ObjectsController.Block.HeightMap = texture1.ToArray();
        Scene.CurrentScene.ObjectsController.Block.HeightMapWidth = (int) XGridDivisions;
        Scene.CurrentScene.ObjectsController.Block.HeightMapHeight = (int) YGridDivisions;
    }

    public void ResetBlock()
    {
        UpdateBlockHeightMap();
    }
}