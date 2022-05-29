using System;
using System.Collections.Generic;
using MikCAD.Utilities;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using SharpSceneSerializer.DTOs.GeometryObjects;
using SharpSceneSerializer.DTOs.Types;

namespace MikCAD.BezierSurfaces;

public class BezierSurfaceC2 : CompositeObject, ISurface, I2DObject
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
                if (_isRolled && value < 4)
                    return;
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
                    VPatches = 4;
                    OnPropertyChanged(nameof(VPatches));
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
            //var rowsCount = VDivisions + (VDivisions - 1) * (VPatches - 1);
            //var colsCount = UDivisions + (UDivisions - 1) * (UPatches - 1);

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
            var rowsCount = VPatches;
            var colsCount = 4 + 1 * (UPatches - 1);

            var startPoint = GetModelMatrix().ExtractTranslation() + new Vector3(0, 0, 0);
            var dx = CylinderHeight / 3;
            var dalpha = MathHelper.DegreesToRadians(360f / rowsCount);

            points.Clear();
            _objects.Clear();
            for (int i = 0; i < colsCount; i++)
            {
                points.Add(new List<ParameterizedPoint>());
                for (int j = 0; j < rowsCount; j++)
                {
                    var point = new ParameterizedPoint();
                    point.parents.Add(this);
                    point.posX = (startPoint + i * new Vector3(dx, 0, 0)).X;
                    point.posY = (startPoint + new Vector3((float) Math.Sin(j * dalpha)) * R).Y;
                    point.posZ = (startPoint + new Vector3((float) Math.Cos(j * dalpha)) * R).Z;
                    point.CanBeDeleted = false;
                    Scene.CurrentScene.ObjectsController.AddObjectToScene(point);
                    _objects.Add(point);
                    points[i].Add(point);

                    for (int k = -3, wsp1 = 0; k <= 0; k++, wsp1++)
                    {
                        for (int l = -3, wsp2 = 0; l <= 0; l++, wsp2++)
                        {
                            var second = j + l;
                            if (second < 0)
                                second += (int) VPatches;
                            if (i + k >= 0 && i + k < UPatches && second < VPatches)
                            {
                                _patchesIdx[i + k, second].SetIdAtI(3 - wsp1, 3 - wsp2, (uint) counter);
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
            size = (int) (2 * (3 + UPatches) * (3 + VPatches) * (3 + UPatches) * (3 + VPatches));
        }
        else
        {
            size = (int) (2 * UPatches * 4 * 3 * VPatches * 3 * 3);
        }

        uint[] lines = new uint[size];
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

                if (j == points[0].Count - 1 && IsRolled)
                {
                    lines[it++] = (uint) (i * points[0].Count + j);
                    lines[it++] = (uint) (i * points[0].Count);
                }

                if (i < points.Count - 1)
                {
                    lines[it++] = (uint) (i * points[0].Count + j);
                    lines[it++] = (uint) (i * points[0].Count + j + points[0].Count);
                }
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
            rowsCount = 4 + 1 * ((int) VPatches - 1);
            colsCount = 4 + 1 * ((int) UPatches - 1);
        }
        else
        {
            rowsCount = 3 * (int) VPatches;
            colsCount = 4 + 1 * ((int) UPatches - 1);
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

        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, rowsCount * colsCount * 4 * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);

        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

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

    // public static explicit operator SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2(BezierSurfaceC2 surfaceC2)
    // {
    //     var c2Patches = new List<BezierPatchC2>();
    //     var controlPoints = new List<PointRef>();
    //     uint it=0;
    //     foreach (var patch in surfaceC2._patchesIdx)
    //     {
    //         controlPoints.Clear();
    //         for (int i = 0; i < 16; i++)
    //         {
    //             controlPoints.Add(new PointRef(){Id = surfaceC2.points[i/surfaceC2.points[0].Count][i%surfaceC2.points[0].Count].Id});
    //         }
    //
    //         var patchP = new BezierPatchC2()
    //         {
    //             Id = 1000 * (surfaceC2.Id + 1) + it,
    //             controlPoints = controlPoints.ToArray(),
    //             Samples = new Uint2(surfaceC2.UDivisions, surfaceC2.VDivisions)
    //         };
    //         c2Patches.Add(patchP);
    //         it++;
    //     }
    //     SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2 ret = new SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2()
    //     {
    //         Id = surfaceC2.Id,
    //         Name = surfaceC2.Name,
    //         Patches = c2Patches.ToArray(),
    //         Size = new Uint2(surfaceC2.UPatches, surfaceC2.VPatches),
    //         ParameterWrapped = new Bool2(false, surfaceC2.IsRolled)
    //     };
    //     
    //     return ret;
    // }

    public static explicit operator SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2(BezierSurfaceC2 surfaceC2)
    {
        var c2Patches = new List<BezierPatchC2>();
        var controlPoints = new List<PointRef>();
        uint it = 0;
        if (!surfaceC2.IsRolled)
        {
            for (int i = 0; i < surfaceC2.UPatches; i++)
            {
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
                    c2Patches.Add(patchP);
                    it++;
                }
            }
        }
        else
        {
            for (int i = 0; i < surfaceC2.UPatches; i++)
            {
                for (int j = 0; j < surfaceC2.VPatches - 3; j++)
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
                    c2Patches.Add(patchP);
                    it++;
                }


                //3 od końca
                controlPoints.Clear();
                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][^3].Id});
                }

                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][^2].Id});
                }

                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][^1].Id});
                }

                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][0].Id});
                }
                var patch3 = new BezierPatchC2()
                {
                    Id = 100000 * (surfaceC2.Id + 1) + it,
                    controlPoints = controlPoints.ToArray(),
                    Samples = new Uint2(surfaceC2.UDivisions, surfaceC2.VDivisions)
                };
                c2Patches.Add(patch3);
                it++;
                
                //2 od końca
                controlPoints.Clear();
                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][^2].Id});
                }

                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][^1].Id});
                }

                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][0].Id});
                }

                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][1].Id});
                }
                var patch2 = new BezierPatchC2()
                {
                    Id = 100000 * (surfaceC2.Id + 1) + it,
                    controlPoints = controlPoints.ToArray(),
                    Samples = new Uint2(surfaceC2.UDivisions, surfaceC2.VDivisions)
                };
                c2Patches.Add(patch2);
                it++;
                
                //ostatni
                controlPoints.Clear();
                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][^1].Id});
                }

                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][0].Id});
                }

                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][1].Id});
                }

                for (int l = 0; l < 4; l++)
                {
                    controlPoints.Add(new PointRef() {Id = surfaceC2.points[i + l][2].Id});
                }
                var patch1 = new BezierPatchC2()
                {
                    Id = 100000 * (surfaceC2.Id + 1) + it,
                    controlPoints = controlPoints.ToArray(),
                    Samples = new Uint2(surfaceC2.UDivisions, surfaceC2.VDivisions)
                };
                c2Patches.Add(patch1);
                it++;
            }
        }

        SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2 ret =
            new SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2()
            {
                Id = surfaceC2.Id,
                Name = surfaceC2.Name,
                Patches = c2Patches.ToArray(),
                Size = new Uint2(surfaceC2.UPatches, surfaceC2.VPatches),
                ParameterWrapped = new Bool2(false, surfaceC2.IsRolled)
            };

        return ret;
    }

    // public static explicit operator BezierSurfaceC0(SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC0 surfaceC0)
    // {
    //     BezierSurfaceC0 ret = new BezierSurfaceC0(false);
    //     ret._uPatches = surfaceC0.Size.X;
    //     ret._vPatches = surfaceC0.Size.Y;
    //     ret._isRolled = surfaceC0.ParameterWrapped.U || surfaceC0.ParameterWrapped.V;
    //     ret._patchesIdx = new Patch[ret._uPatches, ret._vPatches];
    //     ret.points = new List<List<ParameterizedPoint>>();
    //     ret._applied = true;
    //     
    //     if (!ret.IsRolled)
    //     {
    //         var rowsCount = 4 + 3 * (ret.VPatches - 1);
    //         var colsCount = 4 + 3 * (ret.UPatches - 1);
    //         for (int i = 0; i < colsCount; i++)
    //         {
    //             ret.points.Add(new List<ParameterizedPoint>((int)rowsCount));
    //             for(int j=0; j<rowsCount; j++)
    //                 ret.points[i].Add(new ParameterizedPoint());
    //         }
    //
    //         for (int i = 0; i < ret.UPatches; i++)
    //         {
    //             for (int j = 0; j < ret.VPatches; j++)
    //             {
    //                 for (int k = 0; k < 4; k++)
    //                 {
    //                     for (int l = 0; l < 4; l++)
    //                     {
    //                         ret.points[i*3+l][j*3+k] = (ParameterizedPoint)Scene.CurrentScene.ObjectsController.ParameterizedObjects.First(x =>
    //                                 x.Id == surfaceC0.Patches[i * ret.VPatches + j].controlPoints[k*4+l].Id);
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     else
    //     {
    //         var rowsCount = 4 + 3 * (ret.VPatches - 1) - 1;
    //         var colsCount = 4 + 3 * (ret.UPatches - 1);
    //         
    //         for (int i = 0; i < colsCount; i++)
    //         {
    //             ret.points.Add(new List<ParameterizedPoint>((int)rowsCount));
    //             for(int j=0; j<rowsCount; j++)
    //                 ret.points[i].Add(new ParameterizedPoint());
    //         }
    //
    //         for (int i = 0; i < ret.UPatches; i++)
    //         {
    //             for (int j = 0; j < ret.VPatches-1; j++)
    //             {
    //                 for (int k = 0; k < 4; k++)
    //                 {
    //                     for (int l = 0; l < 4; l++)
    //                     {
    //                         ret.points[i*3+l][j*3+k] = (ParameterizedPoint)Scene.CurrentScene.ObjectsController.ParameterizedObjects.First(x =>
    //                             x.Id == surfaceC0.Patches[i * ret.VPatches + j].controlPoints[k*4+l].Id);
    //                     }
    //                 }
    //             }
    //             for (int k = 0; k < 3; k++)
    //             {
    //                 for (int l = 0; l < 4; l++)
    //                 {
    //                     ret.points[i*3+l][(int)(ret.VPatches-1)*3+k] = (ParameterizedPoint)Scene.CurrentScene.ObjectsController.ParameterizedObjects.First(x =>
    //                         x.Id == surfaceC0.Patches[i * ret.VPatches + (int)(ret.VPatches-1)].controlPoints[k*4+l].Id);
    //                 }
    //             }
    //         }
    //     }
    //
    //     SetPatchesStruct(ret);
    //
    //     ret._UpdateBuffers = true;
    //     ret.GenerateLines();
    //     return ret;
    // }
}