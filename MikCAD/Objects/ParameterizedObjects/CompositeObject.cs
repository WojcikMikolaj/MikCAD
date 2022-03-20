using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;
namespace MikCAD;

public class CompositeObject: ParameterizedObject
{
    private List<ParameterizedObject> _objects = new List<ParameterizedObject>();
    private ParameterizedPoint _center = null;
    internal int objectsCount => _objects.Count;
    internal ParameterizedObject first => _objects.Count == 1 ? _objects[0] : null;
    private bool _selected = false;
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

    public void ProcessObject(ParameterizedObject o)
    {
        if(o == null)
            return;
        if (!_objects.Contains(o))
        {
            _objects.Add(o);
            o.Selected = true;
        }
        else
        {
            _objects.Remove(o);
            o.Selected = false;
        }
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
        _oldPos = _position;
    }

    public CompositeObject(ParameterizedObject o) : base("composite")
    {
        ProcessObject(o);
    }

    public override uint[] lines { get; }
    public override void UpdateTranslationMatrix()
    {
        var dpos = _position - _oldPos;
        foreach (var o in _objects)
        {
            o.posX += dpos.X;
            o.posY += dpos.Y;
            o.posZ += dpos.Z;
        }
        CalculateCenter();
    }

    private Vector3 _oldPos;
    
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
        var rotationX = Matrix4.CreateRotationX(MH.DegreesToRadians(rotX));
        var rotationY = Matrix4.CreateRotationY(MH.DegreesToRadians(rotY));
        var rotationZ = Matrix4.CreateRotationZ(MH.DegreesToRadians(rotZ));
        var scale = Matrix4.CreateScale(scaleX, scaleY, scaleZ);
        foreach (var o in _objects)
        {
            var mat = o.GetOnlyModelMatrix();
            var tr = new Vector3(_center.posX, _center.posY, _center.posZ) - o._position;
            var trMat = Matrix4.CreateTranslation(tr);
            var mtrMat = Matrix4.CreateTranslation(-tr);
            o.CompositeOperationMatrix = (mtrMat * scale * rotationX * rotationY * rotationZ * trMat);
        }        
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
    
    public override Matrix4 GetOnlyModelMatrix()
    {
        return GetModelMatrix();
    }
}