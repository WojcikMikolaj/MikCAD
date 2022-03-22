using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD;

public class CompositeObject : ParameterizedObject
{
    public override string Name
    {
        get => "composite";
        set { }
    }

    private List<ParameterizedObject> _objects = new List<ParameterizedObject>();
    private ParameterizedPoint _centerP = null;
    private ParameterizedPoint _centerRender = null;

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
        if (o == null)
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
        ProcessObject(o);
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
        MainWindow.current.Title = $"{_position.X - lastPos.X}, {_position.Y - lastPos.Y}, {_position.Z - lastPos.Z}";
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
        var rotationX = Matrix4.CreateRotationX(MH.DegreesToRadians(rotX));
        var rotationY = Matrix4.CreateRotationY(MH.DegreesToRadians(rotY));
        var rotationZ = Matrix4.CreateRotationZ(MH.DegreesToRadians(rotZ));
        var scale = Matrix4.CreateScale(scaleX, scaleY, scaleZ);
        foreach (var o in _objects)
        {
            var mat = o.GetOnlyModelMatrix();
            var tr = _position;
            var trMat = Matrix4.CreateTranslation(tr);
            var mtrMat = Matrix4.CreateTranslation(-_position);
            o.CompositeOperationMatrix = (mtrMat * scale * rotationX * rotationY * rotationZ * trMat);
        }
    }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        if (_centerRender != null)
            _centerRender.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
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
}