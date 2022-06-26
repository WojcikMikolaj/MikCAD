using System;
using System.Collections.Generic;
using System.Linq;
using MikCAD.Objects;
using MikCAD.Utilities;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using SharpSceneSerializer.DTOs.GeometryObjects;
using SharpSceneSerializer.DTOs.Types;

namespace MikCAD.BezierSurfaces;

public class BezierSurfaceC2 : CompositeObject, ISurface, IIntersectable
{
    private List<List<ParameterizedPoint>> points;
    private Patch[,] _patchesIdx;

    private uint _uPatches = 1;

    public uint UPatches
    {
        get => _uPatches;
        set
        {
            if (!_applied && value >= 1)
            {
                if (_isRolled && value < 4)
                    return;
                _uPatches = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(UPatches));
            }
        }
    }

    private uint _vPatches = 1;

    public uint VPatches
    {
        get => _vPatches;
        set
        {
            if (!_applied && value >= 1)
            {
                _vPatches = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(VPatches));
            }
        }
    }


    private uint _uDivisions = 4;

    public uint UDivisions
    {
        get => _uDivisions;
        set
        {
            if (value >= 4)
            {
                _uDivisions = value;
                OnPropertyChanged(nameof(UDivisions));
            }
        }
    }

    private uint _vDivisions = 4;

    public uint VDivisions
    {
        get => _vDivisions;
        set
        {
            if (value >= 4)
            {
                _vDivisions = value;
                OnPropertyChanged(nameof(VDivisions));
            }
        }
    }

    private bool _isRolled = false;

    public bool IsRolled
    {
        get => _isRolled;
        set
        {
            if (!_applied)
            {
                _isRolled = value;
                if (_isRolled)
                {
                    UPatches = 4;
                    OnPropertyChanged(nameof(UPatches));
                }

                UpdatePatchesCount();
                OnPropertyChanged(nameof(IsRolled));
            }
        }
    }

    private bool _applied = false;

    public bool Applied
    {
        get => _applied;
        set
        {
            if (value)
            {
                _applied = true;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(Applied));
                OnPropertyChanged(nameof(CanBeChanged));
            }
        }
    }

    public bool CanBeChanged => !Applied;

    private float _singlePatchWidth = 1;

    public float SinglePatchWidth
    {
        get => _singlePatchWidth;
        set
        {
            if (value > 0)
            {
                _singlePatchWidth = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(SinglePatchWidth));
            }
        }
    }

    private float _singlePatchHeight = 1;

    public float SinglePatchHeight
    {
        get => _singlePatchHeight;
        set
        {
            if (value > 0)
            {
                _singlePatchHeight = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(SinglePatchHeight));
            }
        }
    }

    private float _R = 1;

    public float R
    {
        get => _R;
        set
        {
            if (value > 0)
            {
                _R = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(R));
            }
        }
    }

    private float _cylinderHeight = 1;

    public float CylinderHeight
    {
        get => _cylinderHeight;
        set
        {
            if (value > 0)
            {
                _cylinderHeight = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(CylinderHeight));
            }
        }
    }

    private string _name;

    public virtual String Name
    {
        get => _name;
        set
        {
            int count = 1;
            var objname = value;
            while (Scene.CurrentScene.ObjectsController.IsNameTaken(objname))
            {
                objname = value + $"({count++})";
            }

            _name = objname;
            OnPropertyChanged(nameof(Name));
        }
    }

    private bool _drawPolygon = false;

    public bool DrawPolygon
    {
        get => _drawPolygon;
        set
        {
            _drawPolygon = value;
            OnPropertyChanged(nameof(DrawPolygon));
        }
    }

    private void UpdatePatchesCount()
    {
        posX = Scene.CurrentScene.ObjectsController._pointer.posX;
        posY = Scene.CurrentScene.ObjectsController._pointer.posY;
        posZ = Scene.CurrentScene.ObjectsController._pointer.posZ;
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < points[i].Count; j++)
            {
                points[i][j].Deleted = true;
                Scene.CurrentScene.ObjectsController.ParameterizedObjects.Remove(points[i][j]);
            }
        }

        _patchesIdx = new Patch[UPatches, VPatches];
        for (int i = 0; i < UPatches; i++)
        for (int j = 0; j < VPatches; j++)
            _patchesIdx[i, j] = new Patch();
        int counter = 0;
        if (!IsRolled)
        {
            var rowsCount = 4 + 1 * (VPatches - 1);
            var colsCount = 4 + 1 * (UPatches - 1);

            var startPoint = GetModelMatrix().ExtractTranslation();
            var dx = SinglePatchWidth / 3;
            var dz = SinglePatchHeight / 3;


            points.Clear();
            _objects.Clear();
            for (int i = 0; i < colsCount; i++)
            {
                points.Add(new List<ParameterizedPoint>());
                for (int j = 0; j < rowsCount; j++)
                {
                    var point = new ParameterizedPoint();
                    point.parents.Add(this);
                    point.CanBeDeleted = false;
                    point.posX = (startPoint + i * new Vector3(dx, 0, dz)).X;
                    point.posZ = (startPoint + j * new Vector3(dx, 0, dz)).Z;
                    Scene.CurrentScene.ObjectsController.AddObjectToScene(point);
                    _objects.Add(point);
                    points[i].Add(point);
                    for (int k = -3, wsp1 = 0; k <= 0; k++, wsp1++)
                    {
                        for (int l = -3, wsp2 = 0; l <= 0; l++, wsp2++)
                        {
                            if (i + k >= 0 && i + k < UPatches && j + l >= 0 && j + l < VPatches)
                            {
                                _patchesIdx[i + k, j + l].SetIdAtI(3 - wsp1, 3 - wsp2, (uint) counter);
                            }
                        }
                    }

                    counter++;
                }
            }
        }
        else
        {
            var rowsCount = 4 + 1 * (VPatches - 1);
            var colsCount = UPatches;

            var startPoint = GetModelMatrix().ExtractTranslation() + new Vector3(0, 0, 0);
            var dx = CylinderHeight / 3;
            var dalpha = MathHelper.DegreesToRadians(360f / colsCount);

            points.Clear();
            _objects.Clear();
            for (int i = 0; i < colsCount; i++)
            {
                points.Add(new List<ParameterizedPoint>());
                for (int j = 0; j < rowsCount; j++)
                {
                    var point = new ParameterizedPoint();
                    point.parents.Add(this);
                    point.posX = (startPoint + j * new Vector3(dx, 0, 0)).X;
                    point.posY = (startPoint + new Vector3((float) Math.Sin(i * dalpha)) * R).Y;
                    point.posZ = (startPoint + new Vector3((float) Math.Cos(i * dalpha)) * R).Z;
                    point.CanBeDeleted = false;
                    Scene.CurrentScene.ObjectsController.AddObjectToScene(point);
                    _objects.Add(point);
                    points[i].Add(point);
                    for (int k = -3, wsp1 = 0; k <= 0; k++, wsp1++)
                    {
                        for (int l = -3, wsp2 = 0; l <= 0; l++, wsp2++)
                        {
                            var m_first = i + k;
                            if (m_first < 0)
                                m_first += (int) UPatches;
                            if (j + l >= 0 && j + l < VPatches && m_first < UPatches)
                            {
                                _patchesIdx[m_first, j + l].SetIdAtI(3 - wsp1, 3 - wsp2, (uint) counter);
                            }
                        }
                    }

                    counter++;
                }
            }
        }

        _UpdateBuffers = true;
    }

    public override void ProcessObject(ParameterizedObject o)
    {
    }

    public BezierSurfaceC2() : base("SurfaceC2")
    {
        points = new List<List<ParameterizedPoint>>();
        Name = "SurfaceC2";
        UpdatePatchesCount();
    }

    public BezierSurfaceC2(bool b) : base("SurfaceC2")
    {
        Name = "SurfaceC2";
    }

    private bool _UpdateBuffers = true;

    public override uint[] lines => _lines;
    public uint[] _lines;

    private void GenerateLines()
    {
        if (!_UpdateBuffers)
            return;
        //nie zawijany
        int size = 0;
        if (!IsRolled)
        {
            size = (int) (2 * (3 + UPatches) * (3 + VPatches) * (3 + UPatches) * (3 + VPatches));
        }
        else
        {
            size = (int) (2 * UPatches * 4 * 3 * VPatches * 3 * 3);
        }

        if (_lines == null || _lines.Length != size)
            _lines = new uint[size];
        uint it = 0;
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < points[0].Count; j++)
            {
                if (j < points[0].Count - 1)
                {
                    lines[it++] = (uint) (i * points[0].Count + j);
                    lines[it++] = (uint) (i * points[0].Count + j + 1);
                }


                if (i < points.Count - 1)
                {
                    lines[it++] = (uint) (i * points[0].Count + j);
                    lines[it++] = (uint) (i * points[0].Count + j + points[0].Count);
                }
            }
        }

        if (IsRolled)
        {
            for (int j = 0; j < points[0].Count; j++)
            {
                _lines[it++] = (uint) (j);
                _lines[it++] = (uint) ((points.Count - 1) * points[0].Count + j);
            }
        }
    }

    public uint[] patches => _patches;
    public uint[] _patches;

    private void GeneratePatches()
    {
        if (!_UpdateBuffers)
            return;
        int patchesCount = (int) (UPatches * VPatches);
        if (_patches == null || _patches.Length != patchesCount * 16)
            _patches = new uint[patchesCount * 16];
        int it = 0;
        for (int i = 0; i < UPatches; i++)
        {
            for (int j = 0; j < VPatches; j++)
            {
                for (int k = 0; k < 16; k++)
                {
                    _patches[it++] = _patchesIdx[i, j].GetIdAtI(k);
                }
            }
        }
    }

    private float[] _vertices;

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        //nie zawijany
        int rowsCount = 0;
        int colsCount = 0;
        if (!IsRolled)
        {
            rowsCount = 4 + 1 * ((int) VPatches - 1);
            colsCount = 4 + 1 * ((int) UPatches - 1);
        }
        else
        {
            rowsCount = 4 + 1 * ((int) VPatches - 1);
            colsCount = 3 * (int) UPatches;
        }

        if (_vertices == null || _vertices.Length != rowsCount * colsCount * 4)
        {
            _vertices = new float[rowsCount * colsCount * 4];
            GC.Collect();
        }


        int it = 0;
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < points[i].Count; j++)
            {
                var posVector = points[i][j].GetModelMatrix().ExtractTranslation();
                _vertices[4 * it] = posVector.X;
                _vertices[4 * it + 1] = posVector.Y;
                _vertices[4 * it + 2] = posVector.Z;
                _vertices[4 * it + 3] = 1;
                it++;
            }
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, rowsCount * colsCount * 4 * sizeof(float), _vertices,
            BufferUsageHint.StaticDraw);

        GL.BindVertexArray(_vao);

        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GenerateLines();
        GeneratePatches();
        _UpdateBuffers = false;
    }

    public override void OnDelete()
    {
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < points[i].Count;)
            {
                points[i][j].CanBeDeleted = true;
                Scene.CurrentScene.ObjectsController.ParameterizedObjects.Remove(points[i][j]);
                points[i].RemoveAt(j);
            }
        }

        base.OnDelete();
    }

    public override void PassToDrawProcessor(DrawProcessor drawProcessor, EyeEnum eye, uint vertexAttributeLocation,
        uint normalAttributeLocation)
    {
        drawProcessor.ProcessObject(this, eye, vertexAttributeLocation, normalAttributeLocation);
    }

    public static explicit operator SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2(BezierSurfaceC2 surfaceC2)
    {
        var c2Patches = new List<List<BezierPatchC2>>();
        var resultPatches = new List<BezierPatchC2>();
        var controlPoints = new List<PointRef>();
        uint it = 0;
        if (!surfaceC2.IsRolled)
        {
            for (int i = 0; i < surfaceC2.UPatches; i++)
            {
                c2Patches.Add(new List<BezierPatchC2>());
                for (int j = 0; j < surfaceC2.VPatches; j++)
                {
                    controlPoints.Clear();
                    for (int k = 0; k < 4; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][j + k].Id});
                        }
                    }

                    var patchP = new BezierPatchC2()
                    {
                        Id = 100000 * (surfaceC2.Id + 1) + it,
                        controlPoints = controlPoints.ToArray(),
                        Samples = new Uint2(surfaceC2.UDivisions, surfaceC2.VDivisions)
                    };
                    c2Patches[i].Add(patchP);
                    it++;
                }
            }

            for (int i = 0; i < surfaceC2.VPatches; i++)
            {
                for (int j = 0; j < surfaceC2.UPatches; j++)
                {
                    resultPatches.Add(c2Patches[j][i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < surfaceC2.UPatches - 3; i++)
            {
                c2Patches.Add(new List<BezierPatchC2>());
                for (int j = 0; j < surfaceC2.VPatches; j++)
                {
                    controlPoints.Clear();
                    for (int k = 0; k < 4; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][j + k].Id});
                        }
                    }

                    var patchP = new BezierPatchC2()
                    {
                        Id = 100000 * (surfaceC2.Id + 1) + it,
                        controlPoints = controlPoints.ToArray(),
                        Samples = new Uint2(surfaceC2.UDivisions, surfaceC2.VDivisions)
                    };
                    c2Patches[i].Add(patchP);
                    it++;
                }
            }

            for (int i = 0; i < 3; i++)
                c2Patches.Add(new List<BezierPatchC2>());

            for (int j = 0; j < surfaceC2.VPatches; j++)
            {
                //3 od końca
                controlPoints.Clear();
                for (int k = 0; k < 4; k++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[^3][j + k].Id});
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[^2][j + k].Id});
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[^1][j + k].Id});
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[0][j + k].Id});
                }


                var patch3 = new BezierPatchC2()
                {
                    Id = 100000 * (surfaceC2.Id + 1) + it,
                    controlPoints = controlPoints.ToArray(),
                    Samples = new Uint2(surfaceC2.UDivisions, surfaceC2.VDivisions)
                };
                c2Patches[^3].Add(patch3);
                it++;

                //2 od końca
                controlPoints.Clear();
                for (int k = 0; k < 4; k++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[^2][j + k].Id});
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[^1][j + k].Id});
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[0][j + k].Id});
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[1][j + k].Id});
                }

                var patch2 = new BezierPatchC2()
                {
                    Id = 100000 * (surfaceC2.Id + 1) + it,
                    controlPoints = controlPoints.ToArray(),
                    Samples = new Uint2(surfaceC2.UDivisions, surfaceC2.VDivisions)
                };
                c2Patches[^2].Add(patch2);
                it++;

                //ostatni
                controlPoints.Clear();
                for (int k = 0; k < 4; k++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[^1][j + k].Id});
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[0][j + k].Id});
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[1][j + k].Id});
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[2][j + k].Id});
                }

                var patch1 = new BezierPatchC2()
                {
                    Id = 100000 * (surfaceC2.Id + 1) + it,
                    controlPoints = controlPoints.ToArray(),
                    Samples = new Uint2(surfaceC2.UDivisions, surfaceC2.VDivisions)
                };
                c2Patches[^1].Add(patch1);
                it++;
            }

            for (int i = 0; i < surfaceC2.VPatches; i++)
            {
                for (int j = 0; j < surfaceC2.UPatches; j++)
                {
                    resultPatches.Add(c2Patches[j][i]);
                }
            }
        }

        SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2 ret =
            new SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2()
            {
                Id = surfaceC2.Id,
                Name = surfaceC2.Name,
                Patches = resultPatches.ToArray(),
                Size = new Uint2(surfaceC2.UPatches, surfaceC2.VPatches),
                ParameterWrapped = new Bool2(surfaceC2.IsRolled, false)
            };

        return ret;
    }

    public static explicit operator BezierSurfaceC2(SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2 surfaceC2)
    {
        BezierSurfaceC2 ret = new BezierSurfaceC2(false);
        ret._uPatches = surfaceC2.Size.X;
        ret._vPatches = surfaceC2.Size.Y;
        ret._isRolled = surfaceC2.ParameterWrapped.U || surfaceC2.ParameterWrapped.V;
        ret._patchesIdx = new Patch[ret._uPatches, ret._vPatches];
        ret.points = new List<List<ParameterizedPoint>>();
        ret._applied = true;

        var patches = new List<List<BezierPatchC2>>();
        var sortedPatches = new List<BezierPatchC2>();

        var it = 0;
        for (int ii = 0; ii < ret.VPatches; ii++)
        {
            patches.Add(new List<BezierPatchC2>());
            for (int jj = 0; jj < ret.UPatches; jj++)
            {
                patches[ii].Add(surfaceC2.Patches[it++]);
            }
        }

        for (int ii = 0; ii < ret.UPatches; ii++)
        {
            for (int jj = 0; jj < ret.VPatches; jj++)
            {
                sortedPatches.Add(patches[jj][ii]);
            }
        }

        if (!ret.IsRolled)
        {
            var rowsCount = 4 + 1 * (ret.VPatches - 1);
            var colsCount = 4 + 1 * (ret.UPatches - 1);
            for (int i = 0; i < colsCount; i++)
            {
                ret.points.Add(new List<ParameterizedPoint>((int) rowsCount));
                for (int j = 0; j < rowsCount; j++)
                    ret.points[i].Add(new ParameterizedPoint());
            }

            for (int i = 0; i < ret.UPatches; i++)
            {
                for (int j = 0; j < ret.VPatches; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            ret.points[i + l][j + k] =
                                (ParameterizedPoint) Scene.CurrentScene.ObjectsController.ParameterizedObjects.First(
                                    x =>
                                        x.Id == sortedPatches[(int) (i * ret.VPatches + j)].controlPoints[k * 4 + l]
                                            .Id);
                            ret.points[i + l][j + k].parents.Add(ret);
                            //TODO: ładniej
                            if (!ret._objects.Contains(ret.points[i + l][j + k]))
                                ret._objects.Add(ret.points[i + l][j + k]);
                        }
                    }
                }
            }
        }
        else
        {
            var rowsCount = 4 + 1 * (ret.VPatches - 1);
            var colsCount = ret.UPatches;

            for (int i = 0; i < colsCount; i++)
            {
                ret.points.Add(new List<ParameterizedPoint>((int) rowsCount));
                for (int j = 0; j < rowsCount; j++)
                    ret.points[i].Add(new ParameterizedPoint());
            }

            for (int i = 0; i < ret.UPatches - 3; i++)
            {
                for (int j = 0; j < ret.VPatches; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            ret.points[i + l][j + k] =
                                (ParameterizedPoint) Scene.CurrentScene.ObjectsController.ParameterizedObjects.First(
                                    x =>
                                        x.Id == sortedPatches[(int) (i * ret.VPatches + j)].controlPoints[k * 4 + l]
                                            .Id);
                            ret.points[i + l][j + k].parents.Add(ret);
                            //TODO: ładniej
                            if (!ret._objects.Contains(ret.points[i + l][j + k]))
                                ret._objects.Add(ret.points[i + l][j + k]);
                        }
                    }
                }
            }
        }

        SetPatchesStruct(ret);

        ret._UpdateBuffers = true;
        ret.GenerateLines();
        return ret;
    }

    private static void SetPatchesStruct(BezierSurfaceC2 ret)
    {
        ret._patchesIdx = new Patch[ret.UPatches, ret.VPatches];
        for (int i = 0; i < ret.UPatches; i++)
        for (int j = 0; j < ret.VPatches; j++)
            ret._patchesIdx[i, j] = new Patch();
        int counter = 0;
        if (!ret.IsRolled)
        {
            var rowsCount = 4 + 1 * (ret.VPatches - 1);
            var colsCount = 4 + 1 * (ret.UPatches - 1);

            for (int i = 0; i < colsCount; i++)
            {
                for (int j = 0; j < rowsCount; j++)
                {
                    for (int k = -3, wsp1 = 0; k <= 0; k++, wsp1++)
                    {
                        for (int l = -3, wsp2 = 0; l <= 0; l++, wsp2++)
                        {
                            if (i + k >= 0 && i + k < ret.UPatches && j + l >= 0 && j + l < ret.VPatches)
                            {
                                ret._patchesIdx[i + k, j + l].SetIdAtI(3 - wsp1, 3 - wsp2, (uint) counter);
                            }
                        }
                    }

                    counter++;
                }
            }
        }
        else
        {
            var rowsCount = 4 + 1 * (ret.VPatches - 1);
            var colsCount = ret.UPatches;

            for (int i = 0; i < colsCount; i++)
            {
                for (int j = 0; j < rowsCount; j++)
                {
                    for (int k = -3, wsp1 = 0; k <= 0; k++, wsp1++)
                    {
                        for (int l = -3, wsp2 = 0; l <= 0; l++, wsp2++)
                        {
                            var m_first = i + k;
                            if (m_first < 0)
                                m_first += (int) ret.UPatches;
                            if (j + l >= 0 && j + l < ret.VPatches && m_first < ret.UPatches)
                            {
                                ret._patchesIdx[m_first, j + l].SetIdAtI(3 - wsp1, 3 - wsp2, (uint) counter);
                            }
                        }
                    }

                    counter++;
                }
            }
        }
    }

    public void SubstitutePoints(ParameterizedPoint oldPoint, ParameterizedPoint newPoint)
    {
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < points[0].Count; j++)
            {
                if (points[i][j].Id == oldPoint.Id)
                    points[i][j] = newPoint;
            }
        }
    }

    public Vector3 GetValueAt(float u, float v)
    {
        return GetPositionAndGradient(u, v).pos;
    }

    public Vector3 GetUDerivativeAt(float u, float v)
    {
        return GetPositionAndGradient(u, v).dU;
    }

    public Vector3 GetVDerivativeAt(float u, float v)
    {
        return GetPositionAndGradient(u, v).dV;
    }

    public (Vector3 pos, Vector3 dU, Vector3 dV) GetPositionAndGradient(float u, float v)
    {
        int UPatchNum = (int) Math.Floor(u);
        int VPatchNum = (int) Math.Floor(v);

        if (UPatchNum == UPatches)
            UPatchNum = (int) UPatches - 1;
        
        if (VPatchNum == VPatches)
            VPatchNum = (int) VPatches - 1;

        if (u < 0)
        {
            u = 0;
            UPatchNum = 0;
        }

        if (u >= UPatches)
        {
            u = UPatches;
            UPatchNum = (int) UPatches - 1;
        }

        if (v < 0)
        {
            v = 0;
            VPatchNum = 0;
        }

        if (v >= VPatches)
        {
            v = VPatches;
            VPatchNum = (int) VPatches - 1;
        }

        u -= MathF.Floor(u); 
        v -= MathF.Floor(v); 

        ParameterizedPoint[,] patchPoints = new ParameterizedPoint[4, 4];
        Vector3[,] patchPosdVPoints = new Vector3[4, 4];
        Vector3[,] patchdUPoints = new Vector3[4, 4];


        for (int i = 0; i < 4; i++)
        {
            if (IsRolled)
            {
                patchPoints[i, 0] = points[(UPatchNum) % (int) (UPatches)][VPatchNum + i];
                patchPoints[i, 1] = points[(UPatchNum + 1) % (int) (UPatches)][VPatchNum + i];
                patchPoints[i, 2] = points[(UPatchNum + 2) % (int) (UPatches)][VPatchNum + i];
                patchPoints[i, 3] = points[(UPatchNum + 3) % (int) (UPatches)][VPatchNum + i];
            }
            else
            {
                patchPoints[i, 0] = points[UPatchNum][VPatchNum + i];
                patchPoints[i, 1] = points[UPatchNum + 1][VPatchNum + i];
                patchPoints[i, 2] = points[UPatchNum + 2][VPatchNum + i];
                patchPoints[i, 3] = points[UPatchNum + 3][VPatchNum + i];
            }

            var bezierPoints = (this as ISurface).ConvertBSplineToBezier(
                patchPoints[i, 0].pos,
                patchPoints[i, 1].pos,
                patchPoints[i, 2].pos,
                patchPoints[i, 3].pos);

            patchPosdVPoints[i, 0] = bezierPoints[0];
            patchPosdVPoints[i, 1] = bezierPoints[1];
            patchPosdVPoints[i, 2] = bezierPoints[2];
            patchPosdVPoints[i, 3] = bezierPoints[3];
        }

        Vector3[] interArray = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            interArray[i] = (this as ISurface).EvaluateCurveAtT(u,
                patchPosdVPoints[i, 0],
                patchPosdVPoints[i, 1],
                patchPosdVPoints[i, 2],
                patchPosdVPoints[i, 3]);
        }

        var interBezierPoints =
            (this as ISurface).ConvertBSplineToBezier(interArray[0], interArray[1], interArray[2], interArray[3]);

        var pos = (this as ISurface).EvaluateCurveAtT(v,
            interBezierPoints[0],
            interBezierPoints[1],
            interBezierPoints[2],
            interBezierPoints[3]);

        var dV = (this as ISurface).EvaluateCurveAtT(v,
            3 * interBezierPoints[1] - 3 * interBezierPoints[0],
            3 * interBezierPoints[2] - 3 * interBezierPoints[1],
            3 * interBezierPoints[3] - 3 * interBezierPoints[2],
            interBezierPoints[3],
            3);


        for (int i = 0; i < 4; i++)
        {
            var bezierPoints = (this as ISurface).ConvertBSplineToBezier(
                patchPoints[0, i].pos,
                patchPoints[1, i].pos,
                patchPoints[2, i].pos,
                patchPoints[3, i].pos);

            patchdUPoints[0, i] = bezierPoints[0];
            patchdUPoints[1, i] = bezierPoints[1];
            patchdUPoints[2, i] = bezierPoints[2];
            patchdUPoints[3, i] = bezierPoints[3];
        }

        Vector3[] interUArray = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            interUArray[i] = (this as ISurface).EvaluateCurveAtT(v,
                patchdUPoints[0, i],
                patchdUPoints[1, i],
                patchdUPoints[2, i],
                patchdUPoints[3, i]);
        }

        var interUBezierArray =
            (this as ISurface).ConvertBSplineToBezier(interUArray[0], interUArray[1], interUArray[2], interUArray[3]);

        var dU = (this as ISurface).EvaluateCurveAtT(u,
            3 * interUBezierArray[1] - 3 * interUBezierArray[0],
            3 * interUBezierArray[2] - 3 * interUBezierArray[1],
            3 * interUBezierArray[3] - 3 * interUBezierArray[2],
            interUBezierArray[3],
            3);

        return (pos, dU, dV);
    }

    public bool IsUWrapped => IsRolled;
    public bool IsVWrapped => false;
    public float USize => UPatches;
    public float VSize => VPatches;
}