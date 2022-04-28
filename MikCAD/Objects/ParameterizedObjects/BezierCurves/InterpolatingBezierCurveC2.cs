using System;
using System.Collections.Generic;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MikCAD.BezierCurves;

public class InterpolatingBezierCurveC2 : CompositeObject, IBezierCurve
{
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

    private string _name = "";

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

    public InterpolatingBezierCurveC2() : base("InterpolatingBezierCurveC2")
    {
        Name = "InterpolatingBezierCurveC2";
        CurveColor = new Vector4(1, 186.0f / 255, 0, 1);
    }

    public InterpolatingBezierCurveC2(CompositeObject compositeObject) : this()
    {
        foreach (var o in compositeObject._objects)
        {
            if (o is ParameterizedPoint p)
                ProcessPoint(p);
        }
    }

    public InterpolatingBezierCurveC2(ParameterizedObject o) : this()
    {
        if (o is ParameterizedPoint p)
            ProcessPoint(p);
    }

    public void ProcessPoint(ParameterizedPoint point)
    {
        base.ProcessObject(point);
    }

    public Vector4 CurveColor { get; set; }

    public override void ProcessObject(ParameterizedObject o)
    {
        if (o is IBezierCurve)
            return;
        int size = _objects.Count;
        if (o is ParameterizedPoint p)
            ProcessPoint(p);
        if (o is CompositeObject cmp)
        {
            foreach (var obj in cmp._objects)
            {
                ProcessObject(obj);
            }
        }

        if (size != _objects.Count)
        {
            RecalculatePoints();
        }

        OnPropertyChanged(nameof(Objects));
    }

    internal void RecalculatePoints()
    {
        int number = 0;
        _points.Clear();
        if (_objects.Count > 0)
        {
            number = 1;
            _points.Add(_objects[0]);
        }

        for (int i = 1; i < _objects.Count; i++)
        {
            if (MathM.Distance(_objects[i - 1], _objects[i]) > Double.Epsilon)
            {
                number++;
                _points.Add(_objects[i]);
            }
        }

        _chordLengths = new float[number]; //di ze wzoru
        _alpha = new float[number];
        _beta = new float[number];
        _mid = new float[number];
        _r = new Vector3[number];
        _a = new Vector3[number];
        _b = new Vector3[number];
        _c = new Vector3[number];
        _d = new Vector3[number];
        _vertices = new Vector4[4 * (number - 1 >= 0 ? number - 1 : 0)];
        CalculateBezierCoefficients();
    }

    public int tessLevel;

    public override uint[] lines => _lines;
    public uint[] _lines;

    public uint[] patches => _patches;
    public uint[] _patches;


    private uint[] GenerateLines()
    {
        int count = _points.Count;
        if (count < 2)
            return new uint[0];
        //2 - ends of each line
        int patchesCount = (int) Math.Ceiling(_vertices.Length / 4.0f);
        uint[] lines = new uint[4 * 2 * patchesCount - 2];
        uint it = 0;
        for (int i = 0; i < 4 * patchesCount - 1; i++)
        {
            lines[it++] = (uint) i;
            lines[it++] = (uint) i + 1;
        }

        return lines;
    }

    private uint[] GeneratePatches()
    {
        if (_vertices.Length == 0)
            return new uint[0];
        int patchesCount = (int) Math.Ceiling(_vertices.Length / 4.0f);
        uint[] patches = new uint[patchesCount * 4];
        int it = 0;
        for (int i = 0; i < patchesCount; i++)
        {
            patches[it++] = (uint) (4 * i);
            patches[it++] = (uint) (4 * i + 1);
            patches[it++] = (uint) (4 * i + 2);
            patches[it++] = (uint) (4 * i + 3);
        }

        return patches;
    }

    private float[] _chordLengths = Array.Empty<float>();
    private float[] _mid = Array.Empty<float>();
    private float[] _alpha = Array.Empty<float>();
    private float[] _beta = Array.Empty<float>();
    
    private List<ParameterizedObject> _points = new List<ParameterizedObject>();
    private Vector3[] _r = Array.Empty<Vector3>();
    private Vector3[] _a = Array.Empty<Vector3>();
    private Vector3[] _b = Array.Empty<Vector3>();
    private Vector3[] _c = Array.Empty<Vector3>();
    private Vector3[] _d = Array.Empty<Vector3>();
    private Vector4[] _vertices = Array.Empty<Vector4>();

    public void CalculateBezierCoefficients()
    {
        if (_points.Count < 2)
            return;
        int firstRowId = 1;
        int lastRowId = _points.Count - 2;
        for (int i = 0; i < _points.Count - 1; i++)
        {
            _chordLengths[i] = MathM.Distance(_points[i], _points[i + 1]);
        }

        for (int i = firstRowId; i <= lastRowId; i++)
        {
            _mid[i] = 2;
            _alpha[i] = _chordLengths[i - 1] / (_chordLengths[i - 1] + _chordLengths[i]);
            _beta[i] = _chordLengths[i] / (_chordLengths[i - 1] + _chordLengths[i]);


            var pmPos = _points[i - 1].GetModelMatrix().ExtractTranslation();
            var pPos = _points[i].GetModelMatrix().ExtractTranslation();
            var ppPos = _points[i + 1].GetModelMatrix().ExtractTranslation();

            _r[i] = 3 * ((ppPos - pPos) / _chordLengths[i] - (pPos - pmPos) / _chordLengths[i - 1]) /
                    (_chordLengths[i - 1] + _chordLengths[i]);
        }

        for (int i = firstRowId + 1; i <= lastRowId; i++)
        {
            var w = _alpha[i] / _mid[i - 1]; //dolna diagonala przez środkową (z przesunięciem)
            _mid[i] = _mid[i] - w * _beta[i - 1];
            _r[i] = _r[i] - w * _r[i - 1];
        }

        _c[lastRowId] = _r[lastRowId] / _mid[lastRowId];
        for (int i = lastRowId - 1; i >= firstRowId; i--)
        {
            _c[i] = (_r[i] - _beta[i] * _c[i + 1]) / _mid[i];
        }

        //koniec układu równań
        //początek wyliczania a,b i d

        _c[0] = _c[^1] = Vector3.Zero;

        for (int i = 0; i < _points.Count - 1; i++)
        {
            _a[i] = _points[i].GetModelMatrix().ExtractTranslation();

            _b[i] = (_points[i + 1].GetModelMatrix().ExtractTranslation() -
                     _points[i].GetModelMatrix().ExtractTranslation()) / _chordLengths[i] -
                    (_c[i + 1] + 2 * _c[i]) / 3 * _chordLengths[i];
            _d[i] = (_c[i + 1] - _c[i]) / (3 * _chordLengths[i]);
        }

        _b[^1] = _b[^2] + _c[^2] * _chordLengths[^2];
        _a[^1] = _points[^1].GetModelMatrix().ExtractTranslation();

        for (int i = 0; i <= lastRowId; i++)
        {
            _vertices[4 * i] = new Vector4(_a[i], _chordLengths[i]);
            _vertices[4 * i + 1] = new Vector4(_a[i] + _b[i]/3 * _chordLengths[i], 1);
            _vertices[4 * i + 2] = new Vector4(_a[i+1] - _b[i+1]/3 * _chordLengths[i], 1);
            _vertices[4 * i + 3] = new Vector4(_a[i+1], 1);
        }
    }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        CalculateBezierCoefficients();

        var points = _vertices;

        var vertices = new float[(points.Length) * 4];
        for (int i = 0; i < (points.Length); i++)
        {
            var posVector = points[i];
            vertices[4 * i] = posVector.X;
            vertices[4 * i + 1] = posVector.Y;
            vertices[4 * i + 2] = posVector.Z;
            vertices[4 * i + 3] = posVector.W;
        }

        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, (points.Length) * 4 * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);

        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        _lines = GenerateLines();
        _patches = GeneratePatches();
    }


    public void GenerateVerticesBase(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        base.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
    }

    public void MoveUp(ParameterizedObject parameterizedObject)
    {
        if (parameterizedObject is ParameterizedPoint p)
        {
            if (_objects.Contains(p))
            {
                int i = _objects.FindIndex(x => x == p);
                if (i == 0)
                    return;
                (_objects[i - 1], _objects[i]) = (_objects[i], _objects[i - 1]);
                RecalculatePoints();
            }
        }
    }

    public void MoveDown(ParameterizedObject parameterizedObject)
    {
        if (parameterizedObject is ParameterizedPoint p)
        {
            if (_objects.Contains(p))
            {
                int i = _objects.FindIndex(x => x == p);
                if (i == _objects.Count - 1)
                    return;
                (_objects[i + 1], _objects[i]) = (_objects[i], _objects[i + 1]);
                RecalculatePoints();
            }
        }
    }

    public override void PassToDrawProcessor(DrawProcessor drawProcessor, uint vertexAttributeLocation,
        uint normalAttributeLocation)
    {
        drawProcessor.ProcessObject(this, vertexAttributeLocation, normalAttributeLocation);
    }
}