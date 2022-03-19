﻿using System.Collections.Generic;
using OpenTK.Mathematics;

namespace MikCAD;

public class CompositeObject: ParameterizedObject
{
    private List<ParameterizedObject> _objects = new List<ParameterizedObject>();
    private ParameterizedPoint _center = null; 
        
    public void ProcessObject(ParameterizedObject o)
    {
        if (!_objects.Contains(o))
            _objects.Add(o);
        else
            _objects.Remove(o);
        CalculateCenter();
    }

    public void DeleteObject(ParameterizedObject o)
    {
        // if (_objects.Contains(o))
        //     _objects.Remove(o);
        // CalculateCenter();
    }
    
    private void CalculateCenter()
    {
        if (_objects.Count == 0)
        {
            _center = null;
            return;
        }

        Point center = new Point() {X = 0, Y = 0, Z = 0};
        foreach (var o in _objects)
        {
            center.X += o.posX;
            center.Y += o.posY;
            center.Z += o.posZ;
        }

        center.X /= _objects.Count;
        center.Y /= _objects.Count;
        center.Z /= _objects.Count;

        _center = new ParameterizedPoint()
        {
            posX = center.X,
            posY = center.Y,
            posZ = center.Z,
        };
    }

    public CompositeObject(ParameterizedObject o) : base("composite")
    {
        ProcessObject(o);
    }

    public override uint[] lines { get; }
    public override void UpdateTranslationMatrix()
    {
    }

    public override void UpdateRotationMatrix(Axis axis)
    {
    }

    public override void UpdateScaleMatrix()
    {
    }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        if(_center!=null)
            _center.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
    }

    public override Matrix4 GetModelMatrix()
    {
        if(_center!=null)
            return _center.GetModelMatrix();
        return Matrix4.Identity;
    }
}