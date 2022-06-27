using System;
using System.Collections.Generic;
using System.Linq;
using MikCAD.BezierCurves;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.GeometryObjects;

namespace MikCAD.Objects.ParameterizedObjects;

public class IntersectionCurve : CompositeObject, IBezierCurve
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

    public IntersectionCurve() : base("IntersectionCurve")
    {
        Name = "IntersectionCurve";
        CurveColor = new Vector4(1, 186.0f / 255, 0, 1);
    }

    public IntersectionCurve(CompositeObject compositeObject) : this()
    {
        foreach (var o in compositeObject._objects)
        {
            if (o is ParameterizedPoint p)
                ProcessPoint(p);
        }
    }

    public IntersectionCurve(ParameterizedObject o) : this()
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

        OnPropertyChanged(nameof(Objects));
    }

    public int tessLevel;

    public override uint[] lines => _lines;
    public uint[] _lines;

    public uint[] patches => _patches;
    public uint[] _patches;

    private uint[] GenerateLines()
    {
        if (_objects.Count == 0)
            return new uint[0];
        //2 - ends of each line
        uint[] lines = new uint[2 * (_objects.Count - 1)];
        uint it = 0;
        for (int i = 0; i < _objects.Count - 1; i++)
        {
            lines[it++] = (uint) i;
            lines[it++] = (uint) i + 1;
        }

        return lines;
    }

    private uint[] GeneratePatches()
    {
        if (_objects.Count == 0)
            return new uint[0];
        int patchesCount = (int) Math.Ceiling(_objects.Count / 4.0f);
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
        //return new uint[]{0, 1, 2, 3};
    }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        float minX = 1;
        float maxX = -1;
        float minY = 1;
        float maxY = -1;

        var vertices = new float[(_objects.Count) * 4];
        for (int i = 0; i < (_objects.Count); i++)
        {
            var posVector = _objects[i].GetModelMatrix().ExtractTranslation();
            vertices[4 * i] = posVector.X;
            vertices[4 * i + 1] = posVector.Y;
            vertices[4 * i + 2] = posVector.Z;
            vertices[4 * i + 3] = 1;

            var posNDC = new Vector4(posVector, 1) * Scene.CurrentScene.camera.GetViewMatrix() *
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

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, (_objects.Count) * 4 * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);

        GL.BindVertexArray(_vao);

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

    public override void PassToDrawProcessor(DrawProcessor drawProcessor, EyeEnum eye, uint vertexAttributeLocation,
        uint normalAttributeLocation)
    {
        drawProcessor.ProcessObject(this, eye, vertexAttributeLocation, normalAttributeLocation);
    }
}