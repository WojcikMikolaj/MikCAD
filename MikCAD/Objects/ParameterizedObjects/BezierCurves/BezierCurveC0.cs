using System;
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
        if(o is ParameterizedPoint p)
            ProcessPoint(p);
    }
    
    public override uint[] lines { get; }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
    }
}