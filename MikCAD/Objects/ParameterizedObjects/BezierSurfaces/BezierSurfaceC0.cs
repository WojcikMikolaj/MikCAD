using System;
using System.Collections.Generic;
using System.Linq;
using MikCAD.BezierCurves;
using MikCAD.Utilities;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using SharpSceneSerializer.DTOs.GeometryObjects;
using SharpSceneSerializer.DTOs.Types;

namespace MikCAD.BezierSurfaces;

public class BezierSurfaceC0 : CompositeObject, ISurface, I2DObject
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
            var rowsCount = 4 + 3 * (VPatches - 1);
            var colsCount = 4 + 3 * (UPatches - 1);

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
                    int uPatchId = i / 3;
                    int vPatchId = j / 3;
                    if (uPatchId < UPatches && vPatchId < VPatches)
                        _patchesIdx[uPatchId, vPatchId].SetIdAtI(i % 3, j % 3, (uint) counter);
                    if (i % 3 == 0 && i != 0 && vPatchId < VPatches)
                        _patchesIdx[uPatchId - 1, vPatchId].SetIdAtI(3, j % 3, (uint) counter);
                    if (j % 3 == 0 && j != 0 && uPatchId < UPatches)
                        _patchesIdx[uPatchId, vPatchId - 1].SetIdAtI(i % 3, 3, (uint) counter);
                    if (i % 3 == 0 && j % 3 == 0 && i != 0 && j != 0)
                        _patchesIdx[uPatchId - 1, vPatchId - 1].SetIdAtI(3, 3, (uint) counter);

                    counter++;
                }
            }
        }
        else
        {
            var rowsCount = 4 + 3 * (VPatches - 1);
            var colsCount = 4 + 3 * (UPatches - 1) - 1;

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

                    int uPatchId = i / 3;
                    int vPatchId = j / 3;


                    if (uPatchId < UPatches && vPatchId < VPatches)
                        _patchesIdx[uPatchId, vPatchId].SetIdAtI(i % 3, j % 3, (uint) counter);

                    if (i % 3 == 0 && i != 0 && vPatchId < VPatches)
                        _patchesIdx[uPatchId - 1, vPatchId].SetIdAtI(3, j % 3, (uint) counter);

                    if (j % 3 == 0 && j != 0 && uPatchId < UPatches)
                        _patchesIdx[uPatchId, vPatchId - 1].SetIdAtI(i % 3, 3, (uint) counter);

                    if (i % 3 == 0 && j % 3 == 0 && i != 0 && j != 0)
                        _patchesIdx[uPatchId - 1, vPatchId - 1].SetIdAtI(3, 3, (uint) counter);
                    counter++;
                }
            }

            //jakby zawinięte w drugą stronę
            for (int i = 0; i < VPatches; i++)
            {
                _patchesIdx[UPatches - 1, i].SetIdAtI(3, 0, _patchesIdx[0, i].GetIdAtI(0, 0));
                _patchesIdx[UPatches - 1, i].SetIdAtI(3, 1, _patchesIdx[0, i].GetIdAtI(0, 1));
                _patchesIdx[UPatches - 1, i].SetIdAtI(3, 2, _patchesIdx[0, i].GetIdAtI(0, 2));
                _patchesIdx[UPatches - 1, i].SetIdAtI(3, 3, _patchesIdx[0, i].GetIdAtI(0, 3));
            }
        }

        _UpdateBuffers = true;
    }

    public override void ProcessObject(ParameterizedObject o)
    {
    }

    public BezierSurfaceC0() : base("SurfaceC0")
    {
        points = new List<List<ParameterizedPoint>>();
        Name = "SurfaceC0";
        UpdatePatchesCount();
    }

    public BezierSurfaceC0(bool b) : base("SurfaceC0")
    {
        Name = "SurfaceC0";
    }

    private bool _UpdateBuffers = true;

    public override uint[] lines => _lines;
    public uint[] _lines;

    private uint[] GenerateLines()
    {
        if (!_UpdateBuffers)
            return _lines;
        //nie zawijany
        int size = 0;
        if (!IsRolled)
        {
            size = (int) (2 * UPatches * 4 * 3 * VPatches * 4 * 3);
        }
        else
        {
            size = (int) (2 * UPatches * 4 * 3 * VPatches * 3 * 3);
        }

        uint[] lines = new uint[size];
        uint it = 0;
        for (int i = 0; i < points.Count - 1; i++)
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
                lines[it++] = (uint) (j);
                lines[it++] = (uint) ((points.Count-1) * points[0].Count + j);
            }
        }
        
        return lines;
    }

    public uint[] patches => _patches;
    public uint[] _patches;

    private uint[] GeneratePatches()
    {
        if (!_UpdateBuffers)
            return _patches;
        int patchesCount = (int) (UPatches * VPatches);
        uint[] patches = new uint[patchesCount * 16];
        int it = 0;
        for (int i = 0; i < UPatches; i++)
        {
            for (int j = 0; j < VPatches; j++)
            {
                for (int k = 0; k < 16; k++)
                {
                    patches[it++] = _patchesIdx[i, j].GetIdAtI(k);
                }
            }
        }

        return patches;
    }
    
    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        //nie zawijany
        int rowsCount = 0;
        int colsCount = 0;
        if (!IsRolled)
        {
            rowsCount = 4 + 3 * ((int) VPatches - 1);
            colsCount = 4 + 3 * ((int) UPatches - 1);
        }
        else
        {
            rowsCount = 4 + 3 * ((int) VPatches - 1);
            colsCount = 3 * (int) UPatches;
        }

        var vertices = new float[rowsCount * colsCount * 4];
        int it = 0;
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < points[i].Count; j++)
            {
                var posVector = points[i][j].GetModelMatrix().ExtractTranslation();
                vertices[4 * it] = posVector.X;
                vertices[4 * it + 1] = posVector.Y;
                vertices[4 * it + 2] = posVector.Z;
                vertices[4 * it + 3] = 1;
                it++;
            }
        }
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, rowsCount * colsCount * 4 * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);
        
        GL.BindVertexArray(_vao);

        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        _lines = GenerateLines();
        _patches = GeneratePatches();
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

    public static explicit operator SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC0(BezierSurfaceC0 surfaceC0)
    {
        var c0Patches = new List<List<BezierPatchC0>>();
        var resultPatches = new List<BezierPatchC0>();
        var controlPoints = new List<PointRef>();
        uint it = 0;
        if (!surfaceC0.IsRolled)
        {
            for (int i = 0; i < surfaceC0.UPatches; i++)
            {
                c0Patches.Add(new List<BezierPatchC0>());
                for (int j = 0; j < surfaceC0.VPatches; j++)
                {
                    controlPoints.Clear();
                    for (int k = 0; k < 4; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            controlPoints.Add(new PointRef() {Id = surfaceC0.points[i * 3 + l][j * 3 + k].Id});
                        }
                    }

                    var patchP = new BezierPatchC0()
                    {
                        Id = 200000 * (surfaceC0.Id + 1) + it,
                        controlPoints = controlPoints.ToArray(),
                        Samples = new Uint2(surfaceC0.UDivisions, surfaceC0.VDivisions)
                    };
                    c0Patches[i].Add(patchP);
                    it++;
                }
            }

            for (int i = 0; i < surfaceC0.VPatches; i++)
            {
                for (int j = 0; j < surfaceC0.UPatches; j++)
                {
                    resultPatches.Add(c0Patches[j][i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < surfaceC0.UPatches - 1; i++)
            {
                c0Patches.Add(new List<BezierPatchC0>());
                for (int j = 0; j < surfaceC0.VPatches; j++)
                {
                    controlPoints.Clear();
                    for (int k = 0; k < 4; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            controlPoints.Add(new PointRef() {Id = surfaceC0.points[i * 3 + l][j * 3 + k].Id});
                        }
                    }

                    var patchP = new BezierPatchC0()
                    {
                        Id = 200000 * (surfaceC0.Id + 1) + it,
                        controlPoints = controlPoints.ToArray(),
                        Samples = new Uint2(surfaceC0.UDivisions, surfaceC0.VDivisions)
                    };
                    c0Patches[i].Add(patchP);
                    it++;
                }
            }
            c0Patches.Add(new List<BezierPatchC0>());
            for (int j = 0; j < surfaceC0.VPatches; j++)
            {
                controlPoints.Clear();
                for (int k = 0; k < 4; k++)
                {
                    for (int l = 0; l < 3; l++)
                    {
                        controlPoints.Add(new PointRef()
                            {Id = surfaceC0.points[(int)(surfaceC0.UPatches-1) * 3 + l][j * 3 + k].Id});
                    }
                    controlPoints.Add(new PointRef() 
                        {Id = surfaceC0.points[0][j * 3 + k].Id});
                }
                var patchZ = new BezierPatchC0()
                {
                    Id = 200000 * (surfaceC0.Id + 1) + it,
                    controlPoints = controlPoints.ToArray(),
                    Samples = new Uint2(surfaceC0.UDivisions, surfaceC0.VDivisions)
                };
                c0Patches[(int)surfaceC0.UPatches-1].Add(patchZ);
                it++;
            }

            for (int i = 0; i < surfaceC0.VPatches; i++)
            {
                for (int j = 0; j < surfaceC0.UPatches; j++)
                {
                    resultPatches.Add(c0Patches[j][i]);
                }
            }
        }

        SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC0 ret =
            new SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC0()
            {
                Id = surfaceC0.Id,
                Name = surfaceC0.Name,
                Patches = resultPatches.ToArray(),
                Size = new Uint2(surfaceC0.UPatches, surfaceC0.VPatches),
                ParameterWrapped = new Bool2(surfaceC0.IsRolled, false)
            };

        return ret;
    }

    public static explicit operator BezierSurfaceC0(SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC0 surfaceC0)
    {
        BezierSurfaceC0 ret = new BezierSurfaceC0(false);
        ret._uPatches = surfaceC0.Size.X;
        ret._vPatches = surfaceC0.Size.Y;
        ret._isRolled = surfaceC0.ParameterWrapped.U || surfaceC0.ParameterWrapped.V;
        ret._patchesIdx = new Patch[ret._uPatches, ret._vPatches];
        ret.points = new List<List<ParameterizedPoint>>();
        ret._applied = true;

        var patches = new List<List<BezierPatchC0>>();
        var sortedPatches = new List<BezierPatchC0>();

        var it = 0;
        for (int ii = 0; ii < ret.VPatches; ii++)
        {
            patches.Add(new List<BezierPatchC0>());
            for (int jj = 0; jj < ret.UPatches; jj++)
            {
                patches[ii].Add(surfaceC0.Patches[it++]);
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
            var rowsCount = 4 + 3 * (ret.VPatches - 1);
            var colsCount = 4 + 3 * (ret.UPatches - 1);
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
                            ret.points[i * 3 + l][j * 3 + k] =
                                (ParameterizedPoint) Scene.CurrentScene.ObjectsController.ParameterizedObjects.First(
                                    x =>
                                        x.Id == sortedPatches[(int) (i * ret.VPatches + j)].controlPoints[k * 4 + l]
                                            .Id);
                            ret.points[i * 3 + l][j * 3 + k].parents.Add(ret);
                        }
                    }
                }
            }
        }
        else
        {
            var rowsCount = 4 + 3 * (ret.VPatches - 1);
            var colsCount = 4 + 3 * (ret.UPatches - 1) -1;

            for (int i = 0; i < colsCount; i++)
            {
                ret.points.Add(new List<ParameterizedPoint>((int) rowsCount));
                for (int j = 0; j < rowsCount; j++)
                    ret.points[i].Add(new ParameterizedPoint());
            }

            for (int i = 0; i < ret.UPatches-1; i++)
            {
                for (int j = 0; j < ret.VPatches; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            ret.points[i * 3 + l][j * 3 + k] =
                                (ParameterizedPoint) Scene.CurrentScene.ObjectsController.ParameterizedObjects.First(
                                    x =>
                                        x.Id == sortedPatches[(int) (i * ret.VPatches + j)].controlPoints[k * 4 + l]
                                            .Id);
                            ret.points[i * 3 + l][j * 3 + k].parents.Add(ret);
                        }
                    }
                }
            }

            for (int j = 0; j < ret.VPatches; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    for (int l = 0; l < 3; l++)
                    {
                        ret.points[(int) (ret.UPatches-1) * 3 + l][j*3+k] =
                            (ParameterizedPoint) Scene.CurrentScene.ObjectsController.ParameterizedObjects.First(x =>
                                x.Id == sortedPatches[
                                        (int) ((ret.UPatches - 1) * ret.VPatches + j)]
                                    .controlPoints[k * 4 + l].Id);
                        ret.points[(int) (ret.UPatches-1) * 3 + l][j*3+k].parents.Add(ret);
                    }
                }
            }
        }

        SetPatchesStruct(ret);

        ret._UpdateBuffers = true;
        ret.GenerateLines();
        return ret;
    }

    private static void SetPatchesStruct(BezierSurfaceC0 ret)
    {
        ret._patchesIdx = new Patch[ret.UPatches, ret.VPatches];
        for (int i = 0; i < ret.UPatches; i++)
        for (int j = 0; j < ret.VPatches; j++)
            ret._patchesIdx[i, j] = new Patch();
        int counter = 0;
        if (!ret.IsRolled)
        {
            var rowsCount = 4 + 3 * (ret.VPatches - 1);
            var colsCount = 4 + 3 * (ret.UPatches - 1);

            for (int i = 0; i < colsCount; i++)
            {
                for (int j = 0; j < rowsCount; j++)
                {
                    int uPatchId = i / 3;
                    int vPatchId = j / 3;
                    if (uPatchId < ret.UPatches && vPatchId < ret.VPatches)
                        ret._patchesIdx[uPatchId, vPatchId].SetIdAtI(i % 3, j % 3, (uint) counter);
                    if (i % 3 == 0 && i != 0 && vPatchId < ret.VPatches)
                        ret._patchesIdx[uPatchId - 1, vPatchId].SetIdAtI(3, j % 3, (uint) counter);
                    if (j % 3 == 0 && j != 0 && uPatchId < ret.UPatches)
                        ret._patchesIdx[uPatchId, vPatchId - 1].SetIdAtI(i % 3, 3, (uint) counter);
                    if (i % 3 == 0 && j % 3 == 0 && i != 0 && j != 0)
                        ret._patchesIdx[uPatchId - 1, vPatchId - 1].SetIdAtI(3, 3, (uint) counter);

                    counter++;
                }
            }
        }
        else
        {
            var rowsCount = 4 + 3 * (ret.VPatches - 1);
            var colsCount = 4 + 3 * (ret.UPatches - 1) - 1;

            for (int i = 0; i < colsCount; i++)
            {
                for (int j = 0; j < rowsCount; j++)
                {
                    int uPatchId = i / 3;
                    int vPatchId = j / 3;

                    if (uPatchId < ret.UPatches && vPatchId < ret.VPatches)
                        ret._patchesIdx[uPatchId, vPatchId].SetIdAtI(i % 3, j % 3, (uint) counter);

                    if (i % 3 == 0 && i != 0 && vPatchId < ret.VPatches)
                        ret._patchesIdx[uPatchId - 1, vPatchId].SetIdAtI(3, j % 3, (uint) counter);

                    if (j % 3 == 0 && j != 0 && uPatchId < ret.UPatches)
                        ret._patchesIdx[uPatchId, vPatchId - 1].SetIdAtI(i % 3, 3, (uint) counter);

                    if (i % 3 == 0 && j % 3 == 0 && i != 0 && j != 0)
                        ret._patchesIdx[uPatchId - 1, vPatchId - 1].SetIdAtI(3, 3, (uint) counter);
                    counter++;
                }
            }

            //jakby zawinięte w drugą stronę
            for (int i = 0; i < ret.VPatches; i++)
            {
                ret._patchesIdx[ret.UPatches - 1, i].SetIdAtI(3, 0, ret._patchesIdx[0, i].GetIdAtI(0, 0));
                ret._patchesIdx[ret.UPatches - 1, i].SetIdAtI(3, 1, ret._patchesIdx[0, i].GetIdAtI(0, 1));
                ret._patchesIdx[ret.UPatches - 1, i].SetIdAtI(3, 2, ret._patchesIdx[0, i].GetIdAtI(0, 2));
                ret._patchesIdx[ret.UPatches - 1, i].SetIdAtI(3, 3, ret._patchesIdx[0, i].GetIdAtI(0, 3));
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

    public List<List<ParameterizedPoint>> GetPoints()
    {
        return points;
    }

    public (int i, int j) FindPointIndices(ParameterizedPoint point)
    {
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < points[0].Count; j++)
            {
                if (points[i][j].Id == point.Id)
                    return (i, j);
            }
        }

        return (-1, -1);
    }
}