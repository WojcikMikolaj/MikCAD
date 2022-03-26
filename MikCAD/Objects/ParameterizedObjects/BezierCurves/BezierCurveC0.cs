﻿using System;
using System.Collections.Specialized;
using OpenTK.Mathematics;

namespace MikCAD.BezierCurves;

public class BezierCurveC0 : CompositeObject
{
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
    
    public override uint[] lines { get; }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
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