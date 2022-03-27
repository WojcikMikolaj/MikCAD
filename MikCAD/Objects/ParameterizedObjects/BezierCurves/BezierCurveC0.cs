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

    public BezierCurveC0(CompositeObject compositeObject): this()
    {
        foreach (var o in compositeObject._objects)
        {
            if(o is ParameterizedPoint p)
                ProcessPoint(p);
        }
    }
    
    public BezierCurveC0(ParameterizedObject o): this()
    {
        if(o is ParameterizedPoint p)
            ProcessPoint(p);
    }

    public void ProcessPoint(ParameterizedPoint point)
    {
        base.ProcessObject(point);
    }

    public override void ProcessObject(ParameterizedObject o)
    {
        int size = _objects.Count;
        if(o is ParameterizedPoint p)
            ProcessPoint(p);
        OnPropertyChanged(nameof(Objects));
    }
    
    public override uint[] lines => _lines;
    public uint[] _lines;
    
    private uint[] GenerateLines()
    {
        if (_objects.Count == 0)
            return new uint[0];
        //2 - ends of each line
        uint[] lines = new uint[2 * (_objects.Count-1)];
        uint it = 0;
        for (int i = 0; i < _objects.Count-1; i++)
        {
            lines[it++] = (uint)i;
            lines[it++] = (uint)i+1;
        }
        return lines;
    }
    
    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        var vertices = new float[_objects.Count * Point.Size];
        
        for (int i = 0; i < _objects.Count; i++)
        {
            var posVector = _objects[i].GetModelMatrix() * new Vector4(_objects[i]._position);
            vertices[Point.Size * i] = posVector.X;
            vertices[Point.Size * i + 1] = posVector.Y;
            vertices[Point.Size * i + 2] = posVector.Z;
        }
        
        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _objects.Count * 3 * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);
        
        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        var indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        _lines = GenerateLines();
        GL.BufferData(BufferTarget.ElementArrayBuffer, _lines.Length * sizeof(uint), _lines,
            BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
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
                int i = _objects.FindIndex(x=>x==p);
                if(i==0)
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
                int i = _objects.FindIndex(x=>x==p);
                if(i==_objects.Count-1)
                    return;
                (_objects[i + 1], _objects[i]) = (_objects[i], _objects[i + 1]);
            }
        }
    }
}