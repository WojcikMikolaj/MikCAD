﻿using System;
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
            _chordLengths = new float[_objects.Count];
            _alpha = new float[_objects.Count];
            _beta = new float[_objects.Count];
            _r = new Vector3[_objects.Count];
            _a = new Vector3[_objects.Count];
            _b = new Vector3[_objects.Count];
            _c = new Vector3[_objects.Count];
            _db = new Vector3[_objects.Count];
            _vertices = new Vector4[4 * _objects.Count];
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
        int count = _vertices.Length;
        if (count == 0)
            return new uint[0];
        //2 - ends of each line
        uint[] lines = new uint[2 * (count - 1)];
        uint it = 0;
        for (int i = 0; i < count - 1; i++)
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
            patches[it++] = (uint) (3 * i);
            patches[it++] = (uint) (3 * i + 1);
            patches[it++] = (uint) (3 * i + 2);
            patches[it++] = (uint) (3 * i + 3);
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
    private Vector3[] _db = Array.Empty<Vector3>();
    private Vector4[] _vertices = Array.Empty<Vector4>();

    public void CalculateBezierCoefficients()
    {
        if (_objects.Count < 2)
            return;
        for (int i = 0; i < _objects.Count - 1; i++)
        {
            _chordLengths[i] = MathM.Distance(_objects[i], _objects[i + 1]);
        }

        for (int i = 0; i < _objects.Count; i++)
        {
            _mid[i] = 2;
        }

        for (int i = 1; i < _objects.Count; i++)
        {
            var w = _alpha[i] / _mid[i-1]; //dolna diagonala przez środkową (z przesunięciem)
            _mid[i] = _mid[i] - w * _beta[i - 1];
            _r[i] = _r[i] - w * _r[i - 1];
        }

        _c[^1] = _r[^1] / _mid[^1];
        for (int i = _objects.Count - 2; i >= 0; i++)
        {
            _c[i] = (_r[i] - _beta[i]*_r[i+1])/ _mid[i];
        }

        //koniec układu równań
        //początek wyliczania a,b i d
        
        
        
        
        for (int i = 0; i < _objects.Count; i++)
        {
            _vertices[4 * i] = new Vector4(_a[i], _chordLengths[i]);
            _vertices[4 * i + 1] = new Vector4(_b[i], 1);
            _vertices[4 * i + 2] = new Vector4(_c[i], 1);
            _vertices[4 * i + 3] = new Vector4(_db[i], 1);
        }
    }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
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

            var posNDC = new Vector4(posVector) * Scene.CurrentScene.camera.GetViewMatrix() *
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