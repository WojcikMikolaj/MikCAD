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
            _chordLengths = new float[_objects.Count]; //di ze wzoru
            _alpha = new float[_objects.Count];
            _beta = new float[_objects.Count];
            _mid = new float[_objects.Count];
            _r = new Vector3[_objects.Count];
            _a = new Vector3[_objects.Count];
            _b = new Vector3[_objects.Count];
            _c = new Vector3[_objects.Count];
            _d = new Vector3[_objects.Count];
            _vertices = new Vector4[4 * (_objects.Count-1 >=0?_objects.Count-1:0)];
            CalculateBezierCoefficients();
        }

        OnPropertyChanged(nameof(Objects));
    }

    public int tessLevel;

    public override uint[] lines => _lines;
    public uint[] _lines;

    public uint[] patches => _patches;
    public uint[] _patches;


    private uint[] GenerateLines()
    {
        int count = _objects.Count;
        if (count == 0)
            return new uint[0];
        //2 - ends of each line
        uint[] lines = new uint[2 * (count - 1)];
        uint it = 0;
        for (int i = 0; i < count - 1; i++)
        {
            lines[it++] = (uint) (4*i);
            lines[it++] = (uint) (4*i + 4);
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

    //d - wyraz wolny
    private Vector3[] _r = Array.Empty<Vector3>();
    private Vector3[] _a = Array.Empty<Vector3>();
    private Vector3[] _b = Array.Empty<Vector3>();
    private Vector3[] _c = Array.Empty<Vector3>();
    private Vector3[] _d = Array.Empty<Vector3>();
    private Vector4[] _vertices = Array.Empty<Vector4>();

    public void CalculateBezierCoefficients()
    {
        if (_objects.Count < 2)
            return;
        int firstRowId = 1;
        int lastRowId = _objects.Count - 2;
        for (int i = 0; i < _objects.Count - 1; i++)
        {
            _chordLengths[i] = MathM.Distance(_objects[i], _objects[i + 1]);
        }

        for (int i = firstRowId; i <= lastRowId; i++)
        {
            _mid[i] = 2;
            _alpha[i] = _chordLengths[i-1] / (_chordLengths[i-1] + _chordLengths[i]);
            _beta[i] = _chordLengths[i] / (_chordLengths[i-1] + _chordLengths[i]);
            
            var pmPos = _objects[i - 1].GetModelMatrix().ExtractTranslation();
            var pPos = _objects[i].GetModelMatrix().ExtractTranslation();
            var ppPos = _objects[i + 1].GetModelMatrix().ExtractTranslation();
            
            _r[i] = 3*((ppPos - pPos)/_chordLengths[i] - (pPos - pmPos)/_chordLengths[i-1] ) / (_chordLengths[i-1] + _chordLengths[i]);
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

        for (int i = 0; i < _objects.Count-1; i++)
        {
            _a[i] = _objects[i].GetModelMatrix().ExtractTranslation();
            _b[i] = (_objects[i + 1].GetModelMatrix().ExtractTranslation() -
                     _objects[i].GetModelMatrix().ExtractTranslation()) / _chordLengths[i] -
                    (_c[i + 1] + 2 * _c[i]) / 3 * _chordLengths[i];
            _d[i] = (_c[i + 1] -  _c[i]) / (3 * _chordLengths[i]);
        }

        _a[^1] = _objects[^1].GetModelMatrix().ExtractTranslation();
        
        for (int i = 0; i <= lastRowId; i++)
        {
            _vertices[4 * i] = new Vector4(_a[i], _chordLengths[i]);
            _vertices[4 * i + 1] = new Vector4(_b[i], 1);
            _vertices[4 * i + 2] = new Vector4(_c[i], 1);
            _vertices[4 * i + 3] = new Vector4(_d[i], 1);
        }
    }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        CalculateBezierCoefficients();
        float minX = 1;
        float maxX = -1;
        float minY = 1;
        float maxY = -1;

        var points = _vertices;

        var vertices = new float[(points.Length) * 4];
        for (int i = 0; i < (points.Length); i++)
        {
            var posVector = points[i];
            vertices[4 * i] = posVector.X;
            vertices[4 * i + 1] = posVector.Y;
            vertices[4 * i + 2] = posVector.Z;
            vertices[4 * i + 3] = posVector.W;

            var posNDC = new Vector4(posVector.Xyz,1) * Scene.CurrentScene.camera.GetViewMatrix() *
                         Scene.CurrentScene.camera.GetProjectionMatrix();
            posNDC /= posNDC.W;
            if (posNDC.X < minX)
                minX = posNDC.X;
            if (posNDC.Y < minY)
                minY = posNDC.Y;
            if (posNDC.X > maxX)
                maxX = posNDC.X;
            if (posNDC.Y > maxY)
                maxY = posNDC.Y;
        }

        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, (points.Length) * 4 * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);

        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        tessLevel = (int) Math.Max(32, 256 * (maxX - minX) * (maxY - minY) / 4);
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
            }
        }
    }

    public override void PassToDrawProcessor(DrawProcessor drawProcessor, uint vertexAttributeLocation,
        uint normalAttributeLocation)
    {
        drawProcessor.ProcessObject(this, vertexAttributeLocation, normalAttributeLocation);
    }
}