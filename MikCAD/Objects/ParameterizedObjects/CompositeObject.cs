﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MikCAD.BezierCurves;
using MikCAD.BezierSurfaces;
using MikCAD.Objects;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD;

public class CompositeObject : ParameterizedObject, INotifyCollectionChanged
{
    public virtual string Name
    {
        get => "composite";
        set { }
    }

    public List<ParameterizedObject> Objects => _objects;

    internal List<ParameterizedObject> _objects = new List<ParameterizedObject>();
    private ParameterizedPoint _centerP = null;
    private ParameterizedPoint _centerRender = null;

    internal int objectsCount => _objects.Count;
    internal ParameterizedObject first => _objects.Count == 1 ? _objects[0] : null;
    private bool _selected = false;

    public bool CanCollapse => _objects.Count == 2 && _objects[0] is ParameterizedPoint && _objects[1] is ParameterizedPoint;
    public bool CanPatchHole => _objects.Count == 3 && _objects[0] is BezierSurfaceC0 && _objects[1] is BezierSurfaceC0 && _objects[2] is BezierSurfaceC0;
    public bool CanIntersectObjects => (_objects.Count == 1 && _objects[0] is IIntersectable) ||
                                       (_objects.Count == 2 && _objects[0] is IIntersectable &&
                                        _objects[1] is IIntersectable); 

    public virtual bool IsPointComposite { get; init; } = true;

    public override bool Selected
    {
        get => _selected;
        internal set
        {
            _selected = value;
            foreach (var o in _objects)
            {
                o.Selected = value;
            }
        }
    }

    public virtual void ProcessObject(ParameterizedObject o)
    {
        if (o == null)
            return;
        if(o is IBezierCurve or FakePoint)
            return;
        if(IsPointComposite && o is IIntersectable)
            return;
        if(!IsPointComposite && o is not IIntersectable)
            return;
        if (!_objects.Contains(o))
        {
            _objects.Add(o);
            if (o is ParameterizedPoint p)
                p.parents.Add(this);
            o.Selected = true;
        }
        else
        {
            _objects.Remove(o);
            if (o is ParameterizedPoint p)
                p.parents.Remove(this);
            o.Selected = false;
        }

        CalculateCenter();
        OnPropertyChanged(nameof(CanPatchHole));
        OnPropertyChanged(nameof(CanCollapse));
        OnPropertyChanged(nameof(CanIntersectObjects));
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
            _centerP = null;
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

        _centerP = new ParameterizedPoint()
        {
            posX = center.X,
            posY = center.Y,
            posZ = center.Z,
        };

        addedPoints = true;
        posX = _centerP.posX;
        addedPoints = true;
        posY = _centerP.posY;
        addedPoints = true;
        posZ = _centerP.posZ;
        _centerRender = new ParameterizedPoint
        {
            _position = _centerP._position
        };
        _centerRender.UpdateTranslationMatrix();
        orgPos = _position;
        lastPos = _position;
        //addedPoints = true;
    }

    private bool addedPoints = false;
    private Vector3 orgPos;
    private Vector3 lastPos;

    public CompositeObject(ParameterizedObject o) : base("composite")
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        ProcessObject(o);
    }

    protected CompositeObject(string name) : base(name)
    {
        
    }

    public override uint[] lines { get; }

    public override void UpdateTranslationMatrix()
    {
        if(!addedPoints)
            foreach (var o in _objects)
            {
                o._position += _position - lastPos;
                o.UpdateTranslationMatrix();
            }
        else
        {
            addedPoints = false;
        }

        lastPos = _position;
        ApplyOnChilds();
        if (_centerRender != null)
        {
            _centerRender._position = _position;
            _centerRender.UpdateTranslationMatrix();
        }
    }

    public override void UpdateRotationMatrix(Axis axis)
    {
        ApplyOnChilds();
    }

    public override void UpdateScaleMatrix()
    {
        ApplyOnChilds();
    }

    public void ApplyOnChilds()
    {
        var rot = Matrix4.CreateFromQuaternion(new Quaternion(MH.DegreesToRadians(rotX), MH.DegreesToRadians(rotY), MH.DegreesToRadians(rotZ)));
        var scale = Matrix4.CreateScale(scaleX, scaleY, scaleZ);
        foreach (var o in _objects)
        {
            var tr = _position;
            var trMat = Matrix4.CreateTranslation(tr);
            var mtrMat = Matrix4.CreateTranslation(-_position);
            o.CompositeOperationMatrix = (mtrMat * scale * rot * trMat);
            (o as ParameterizedPoint)?.OnPositionUpdate();
        }
    }

    public override void GenerateVertices()
    {
        if (_centerRender != null)
            _centerRender.GenerateVertices();
    }

    public override Matrix4 GetModelMatrix()
    {
        if (_centerRender != null)
            return _centerRender.GetModelMatrix();
        return Matrix4.Identity;
    }

    public override Matrix4 GetOnlyModelMatrix()
    {
        return GetModelMatrix();
    }
    
    protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
    }
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    
    public override void PassToDrawProcessor(DrawProcessor drawProcessor,EyeEnum eye)
    {
        drawProcessor.ProcessObject(this,eye);
    }

    public Intersection GetNewIntersectionObject()
    {
        if (CanIntersectObjects)
        {
            if(objectsCount>1)
                return new Intersection(_objects[0] as IIntersectable, _objects[1] as IIntersectable);
            else
                return new Intersection(_objects[0] as IIntersectable, _objects[0] as IIntersectable);
        }

        return null;
    }
}