using System;
using System.Collections.Generic;
using MikCAD.Utilities;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

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
                UpdatePatchesCount();
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
                UpdatePatchesCount();
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
                    point.posX = (startPoint + i * new Vector3(dx, 0, dz)).X;
                    point.posZ = (startPoint + j * new Vector3(dx, 0, dz)).Z;
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
        }
        else
        {
            var rowsCount = 4 + 3 * (VPatches - 1) - 1;
            var colsCount = 4 + 3 * (UPatches - 1);

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
            for (int i = 0; i < UPatches; i++)
            {
                _patchesIdx[i, VPatches - 1].SetIdAtI(0, 3, _patchesIdx[i, 0].GetIdAtI(0, 0));
                _patchesIdx[i, VPatches - 1].SetIdAtI(1, 3, _patchesIdx[i, 0].GetIdAtI(1, 0));
                _patchesIdx[i, VPatches - 1].SetIdAtI(2, 3, _patchesIdx[i, 0].GetIdAtI(2, 0));
                _patchesIdx[i, VPatches - 1].SetIdAtI(3, 3, _patchesIdx[i, 0].GetIdAtI(3, 0));
            }
        }
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

    public override uint[] lines => _lines;
    public uint[] _lines;

    private uint[] GenerateLines()
    {
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
            rowsCount = 3 * (int) VPatches;
            colsCount = 4 + 3 * ((int) UPatches - 1);
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
}

internal class Patch
{
    uint[] idx = new uint[16];

    internal uint GetIdAtI(int i) => idx[i];
    internal uint GetIdAtI(int i, int j) => idx[i * 4 + j];
    internal void SetIdAtI(int i, uint id) => idx[i] = id;
    internal void SetIdAtI(int i, int j, uint id) => idx[i * 4 + j] = id;
}