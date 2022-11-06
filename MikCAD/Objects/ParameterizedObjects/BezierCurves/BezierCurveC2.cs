using System;
using System.Collections.Generic;
using System.Linq;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.GeometryObjects;

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
        CurveColor = new Vector4(1, 186.0f / 255, 0, 1);
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
        int count = _bernsteinPoints.Count;
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

    public override void GenerateVertices()
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


    public void GenerateVerticesBase()
    {
        base.GenerateVertices();
    }

    public void ConvertBSplineToBernstein(bool update = false)
    {
        Point lastPoint = null;
        var points = new List<(Point, int)>();
        for (int i = 0; i < _objects.Count - 1; i++)
        {
            var posA = _objects[i].GetModelMatrix().ExtractTranslation();
            var posB = _objects[i + 1].GetModelMatrix().ExtractTranslation();
            var dXYZ = posB - posA;
            var first = new Point(posA);
            var second = new Point(posA + dXYZ / 3);
            var third = new Point(posA + 2 * dXYZ / 3);

            if (i != 0)
            {
                points.Add((new Point(lastPoint.XYZ + (second.XYZ - lastPoint.XYZ) / 2), i));
                if (i != _objects.Count - 2)
                {
                    points.Add((second, i));
                    points.Add((third, i + 1));
                }
            }

            lastPoint = third;
        }

        int id = 0;
        if (!update)
        {
            _bernsteinPoints.Clear();
            foreach (var (point, pointToMove) in points)
            {
                var p = new FakePoint()
                {
                    posX = point.X,
                    posY = point.Y,
                    posZ = point.Z,
                    ID = id++,
                    BSplinePointToMove = pointToMove,
                };
                p.parents.Add(this);
                _bernsteinPoints.Add(p);
            }
        }
        else
        {
            foreach (var (point, _) in points)
            {
                _bernsteinPoints[id]._position.X = point.X;
                _bernsteinPoints[id]._position.Y = point.Y;
                _bernsteinPoints[id]._position.Z = point.Z;
                id++;
            }
        }
    }

    public void UpdatePoints(FakePoint point)
    {
        if (_objects.Count < 4)
            return;
        //pierwszy punkt
        if (point.ID == 0)
        {
            var t = _bernsteinPoints[0].GetModelMatrix().ExtractTranslation();
            var second = _bernsteinPoints[1].GetModelMatrix().ExtractTranslation();
            var II = _objects[1].GetModelMatrix().ExtractTranslation();
            var pom = t + t - second;
            var pos = II + 3 * (pom - II);
            _objects[0].posX = pos.X;
            _objects[0].posY = pos.Y;
            _objects[0].posZ = pos.Z;
        }

        //ostatni
        else if (point.ID == _bernsteinPoints.Count - 1)
        {
            var t = _bernsteinPoints[^1].GetModelMatrix().ExtractTranslation();
            var second = _bernsteinPoints[^2].GetModelMatrix().ExtractTranslation();
            var LII = _objects[^2].GetModelMatrix().ExtractTranslation();
            var pom = t + t - second;
            var pos = LII + 3 * (pom - LII);
            _objects[^1].posX = pos.X;
            _objects[^1].posY = pos.Y;
            _objects[^1].posZ = pos.Z;
        }

        // na prostej pomiędzy kolejnymi punktami De Boora
        else if (_bernsteinPoints[point.ID - 1].BSplinePointToMove != _bernsteinPoints[point.ID + 1].BSplinePointToMove)
        {
            //kolejny punkt też pomiędzy kolejnymi punktami De Boora
            if (point.BSplinePointToMove < _bernsteinPoints[point.ID + 1].BSplinePointToMove)
            {
                var t = point.GetModelMatrix().ExtractTranslation();
                var db = _objects[point.BSplinePointToMove + 1].GetModelMatrix().ExtractTranslation();
                var dist = (t - db)/2.0f;
                var pos = t + dist;
                _objects[point.BSplinePointToMove].posX = pos.X;
                _objects[point.BSplinePointToMove].posY = pos.Y;
                _objects[point.BSplinePointToMove].posZ = pos.Z;
            }
            else
            {
                var t = point.GetModelMatrix().ExtractTranslation();
                var db = _objects[point.BSplinePointToMove - 1].GetModelMatrix().ExtractTranslation();
                var dist = (t - db)/2.0f;
                var pos = t + dist;
                _objects[point.BSplinePointToMove].posX = pos.X;
                _objects[point.BSplinePointToMove].posY = pos.Y;
                _objects[point.BSplinePointToMove].posZ = pos.Z;
            }
        }

        // "pod" punktem De Boora
        else
        {
            var prevB = _bernsteinPoints[point.ID - 1].GetModelMatrix().ExtractTranslation();
            var nextB = _bernsteinPoints[point.ID + 1].GetModelMatrix().ExtractTranslation();
            var lastPos = ( prevB + nextB) / 2.0f;
            var dpos = point.GetModelMatrix().ExtractTranslation() - lastPos;
            nextB += dpos;
            prevB += dpos;

            var pos = nextB + (nextB - _objects[point.BSplinePointToMove + 1].GetModelMatrix().ExtractTranslation()) / 2.0f;
            _objects[point.BSplinePointToMove].posX = pos.X;
            _objects[point.BSplinePointToMove].posY = pos.Y;
            _objects[point.BSplinePointToMove].posZ = pos.Z;

            var prevPos = prevB + 2*(prevB - pos);
            _objects[point.BSplinePointToMove-1].posX = prevPos.X;
            _objects[point.BSplinePointToMove-1].posY = prevPos.Y;
            _objects[point.BSplinePointToMove-1].posZ = prevPos.Z;
        }
        
        ConvertBSplineToBernstein();
        Scene.CurrentScene.ObjectsController.SelectObject(_bernsteinPoints[point.ID]);
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

    public void GenerateVerticesForBernsteinPoints()
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
            indexArr[i] = (uint) i;
        }

        var indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, points.Count * sizeof(uint), indexArr,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
    }

    public void GenerateVerticesForBSplinePoints()
    {
        var vertices = new float[(_objects.Count) * 4];
        for (int i = 0; i < (_objects.Count); i++)
        {
            var posVector = _objects[i].GetModelMatrix().ExtractTranslation();
            vertices[4 * i] = posVector.X;
            vertices[4 * i + 1] = posVector.Y;
            vertices[4 * i + 2] = posVector.Z;
            vertices[4 * i + 3] = 1;
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, (_objects.Count) * 4 * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);

        GL.BindVertexArray(_vao);

        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        //TODO: poprawić pod kątem wydajności
        var indexArr = new uint[_objects.Count];
        for (int i = 0; i < _objects.Count; i++)
        {
            indexArr[i] = (uint) i;
        }

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _objects.Count * sizeof(uint), indexArr,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
    }
    
    public override void PassToDrawProcessor(DrawProcessor drawProcessor,EyeEnum eye)
    {
        drawProcessor.ProcessObject(this, eye);
    }
    
    public static explicit operator BezierC2(BezierCurveC2 curveC2)
    {
        var referencePoints = new List<PointRef>();
        foreach (var o in curveC2._objects)
        {
            referencePoints.Add(new PointRef()
            {
                Id = o.Id
            });
        }
        
        BezierC2 ret = new BezierC2()
        {
            Id = curveC2.Id,
            Name = curveC2.Name,
            DeBoorPoints = referencePoints.ToArray()
        };

        return ret;
    }
    
    public static explicit operator BezierCurveC2(BezierC2 curveC2)
    {
        BezierCurveC2 ret = new BezierCurveC2()
        {
            Id = curveC2.Id,
            Name = curveC2.Name,
        };
        foreach (var point in curveC2.DeBoorPoints)
        {
            ret.ProcessObject(Scene.CurrentScene.ObjectsController.ParameterizedObjects.Where(x => x.Id == point.Id).ToArray()[0]);
        }
        return ret;
    }
}