using System;
using System.Collections.Specialized;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MikCAD.BezierCurves;

public class BezierCurveC0 : CompositeObject
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

    public BezierCurveC0() : base("BezierCurveC0")
    {
        Name = "BezierCurveC0";
    }

    public BezierCurveC0(CompositeObject compositeObject) : this()
    {
        foreach (var o in compositeObject._objects)
        {
            if (o is ParameterizedPoint p)
                ProcessPoint(p);
        }
    }

    public BezierCurveC0(ParameterizedObject o) : this()
    {
        if (o is ParameterizedPoint p)
            ProcessPoint(p);
    }

    public void ProcessPoint(ParameterizedPoint point)
    {
        base.ProcessObject(point);
    }

    public override void ProcessObject(ParameterizedObject o)
    {
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
        uint[] patches = new uint[patchesCount*4];
        int it = 0;
        for (int i = 0; i < patchesCount; i++)
        {
            patches[it++] = (uint) (3*i);
            patches[it++] = (uint) (3*i+1);
            patches[it++] = (uint) (3*i+2);
            patches[it++] = (uint) (3*i+3);
        }
        return patches;
        //return new uint[]{0, 1, 2, 3};
    }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        var vertices = new float[(_objects.Count ) * 4 ];

        for (int i = 0; i < (_objects.Count ); i++)
        {
            var posVector = _objects[i]._position;
            vertices[4 * i] = posVector.X;
            vertices[4 * i + 1] = posVector.Y;
            vertices[4 * i + 2] = posVector.Z;
            vertices[4 * i + 3] = 1;
        }

        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, (_objects.Count ) * 4 * sizeof(float), vertices,
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
}