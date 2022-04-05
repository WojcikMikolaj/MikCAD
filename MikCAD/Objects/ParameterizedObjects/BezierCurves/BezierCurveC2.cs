using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MikCAD.BezierCurves;

public class BezierCurveC2 : CompositeObject, IBezierCurve
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

    public BezierCurveC2() : base("BezierCurveC2")
    {
        Name = "BezierCurveC2";
    }

    public BezierCurveC2(CompositeObject compositeObject) : this()
    {
        foreach (var o in compositeObject._objects)
        {
            if (o is ParameterizedPoint p)
                ProcessPoint(p);
        }
    }

    public BezierCurveC2(ParameterizedObject o) : this()
    {
        if (o is ParameterizedPoint p)
            ProcessPoint(p);
    }

    public void ProcessPoint(ParameterizedPoint point)
    {
        //point.Draw = !_bernstein;
        base.ProcessObject(point);
    }

    public override void ProcessObject(ParameterizedObject o)
    {
        if (o is BezierCurveC0 or BezierCurveC2)
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
        ConvertBSplineToBernstein();
        OnPropertyChanged(nameof(Objects));
    }

    public int tessLevel;

    public override uint[] lines => _lines;
    public uint[] _lines;

    public uint[] patches => _patches;

    private bool _bernstein;
    public bool Bernstein
    {
        get => _bernstein;
        set
        {
            // foreach (var o in _objects)
            // {
            //     (o as ParameterizedPoint).Draw = !value;
            // }

            _bernstein = value;
        }
    }

    public uint[] _patches;
    
    public List<FakePoint> BernsteinPoints => _bernsteinPoints;
    private List<FakePoint> _bernsteinPoints = new List<FakePoint>();
    
    private uint[] GenerateLines()
    {
        int count = _bernstein ? _bernsteinPoints.Count : _objects.Count;
        if (count == 0)
            return new uint[0];
        //2 - ends of each line
        uint[] lines = new uint[2 * (count - 1)];
        uint it = 0;
        for (int i = 0; i < count - 1; i++)
        {
            lines[it++] = (uint) i;
            lines[it++] = (uint) i + 1;
        }

        return lines;
    }

    private uint[] GeneratePatches()
    {
        if (_bernsteinPoints.Count == 0)
            return new uint[0];
        int patchesCount = (int) Math.Ceiling(_bernsteinPoints.Count / 4.0f);
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

        var points = _bernsteinPoints;
        
        var vertices = new float[(points.Count) * 4];
        for (int i = 0; i < (points.Count); i++)
        {
            var posVector = points[i].GetModelMatrix().ExtractTranslation();
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

        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, (points.Count) * 4 * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);

        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
#if DEBUG
        MainWindow.current.Title = $"X:{minX}, {maxX}; Y:{minY}, {maxY}";
#endif
        tessLevel = (int) Math.Max(32, 256 * (maxX - minX) * (maxY - minY) / 4);
        _lines = GenerateLines();
        _patches = GeneratePatches();
    }

    
    public void GenerateVerticesBase(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        base.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
    }

    public void ConvertBSplineToBernstein()
    {
        Point lastPoint = null;
        var points = new List<Point>();
        for (int i = 0; i < _objects.Count-1; i++)
        {
            var posA = _objects[i].GetModelMatrix().ExtractTranslation();
            var posB = _objects[i + 1].GetModelMatrix().ExtractTranslation();
            var dXYZ = posB - posA;
            var first = new Point(posA);
            var second = new Point(posA + dXYZ / 3);
            var third = new Point(posA + 2 * dXYZ / 3);
            
            if (i != 0)
            {
                points.Add(new Point(lastPoint.XYZ + (second.XYZ - lastPoint.XYZ) / 2));
                if (i != _objects.Count - 2)
                {
                    points.Add(second);
                    points.Add(third);
                }
            }
            lastPoint = third;
        }

        _bernsteinPoints.Clear();
        foreach (var point in points)
        {
            _bernsteinPoints.Add(new FakePoint()
            {
                posX = point.X,
                posY = point.Y,
                posZ = point.Z,
                parent = this
            });
        }
    }

    public void UpdatePoints()
    {
        //źle
        return;
        if(Objects.Count<4)
            return;
        for (int i = 2; i < BernsteinPoints.Count-1; i+=3)
        {
            var posi1 = BernsteinPoints[i + 1].GetModelMatrix().ExtractTranslation();
            var posi = BernsteinPoints[i].GetModelMatrix().ExtractTranslation();
            var dxyz = posi1 - posi;
            var npos = (posi - dxyz);
            var nposp = (posi1 + dxyz);
            _objects[i - 1].posX = npos.X;
            _objects[i - 1].posY = npos.Y;
            _objects[i - 1].posZ = npos.Z;

            if (i == 2)
            {
                var dxyz0 = npos - BernsteinPoints[0].GetModelMatrix().ExtractTranslation();
                var npos0 = BernsteinPoints[0].GetModelMatrix().ExtractTranslation() - 2*dxyz0; 
                _objects[0].posX = npos0.X;
                _objects[0].posY = npos0.Y;
                _objects[0].posZ = npos0.Z;
            }
            
            _objects[i].posX = nposp.X;
            _objects[i].posY = nposp.Y;
            _objects[i].posZ = nposp.Z;
        }

        var dxyzl = BernsteinPoints[^1].GetModelMatrix().ExtractTranslation() - _objects[^2].GetModelMatrix().ExtractTranslation();
        var nposl = BernsteinPoints[^1].GetModelMatrix().ExtractTranslation() + 2*dxyzl;
        _objects[^1].posX = nposl.X;
        _objects[^1].posY = nposl.Y;
        _objects[^1].posZ = nposl.Z;
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

    public void GenerateVerticesForBernsteinPoints(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        var points = _bernsteinPoints;
        var vertices = new float[(points.Count) * 4];
        for (int i = 0; i < (points.Count); i++)
        {
            var posVector = points[i].GetModelMatrix().ExtractTranslation();
            vertices[4 * i] = posVector.X;
            vertices[4 * i + 1] = posVector.Y;
            vertices[4 * i + 2] = posVector.Z;
            vertices[4 * i + 3] = 1;
        }

        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, (points.Count) * 4 * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);

        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        var indexArr = new uint[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            indexArr[i] = (uint)i;
        }
        var indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, points.Count * sizeof(uint), indexArr, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
    }

    public void GenerateVerticesForBSplinePoints(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        var vertices = new float[(_objects.Count ) * 4 ];
        for (int i = 0; i < (_objects.Count ); i++)
        {
            var posVector = _objects[i].GetModelMatrix().ExtractTranslation();
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

        var indexArr = new uint[_objects.Count];
        for (int i = 0; i < _objects.Count; i++)
        {
            indexArr[i] = (uint)i;
        }
        var indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _objects.Count * sizeof(uint), indexArr, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
    }
}