using System;
using System.Collections.Generic;
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
        
        OnPropertyChanged(nameof(Objects));
    }

    public int tessLevel;

    public override uint[] lines => _lines;
    public uint[] _lines;

    public uint[] patches => _patches;
    public uint[] _patches;
    
    
    private uint[] GenerateLines()
    {
        return _lines;
    }

    private uint[] GeneratePatches()
    {
        return _patches;
    }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        
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

    public override void PassToDrawProcessor(DrawProcessor drawProcessor, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        drawProcessor.ProcessObject(this, vertexAttributeLocation, normalAttributeLocation);
    }
}