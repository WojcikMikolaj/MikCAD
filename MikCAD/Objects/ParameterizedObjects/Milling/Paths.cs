using System;
using System.Collections.Generic;
using System.Data;
using MikCAD.Objects;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Types;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD.Objects.ParameterizedObjects.Milling;

public class Paths : ParameterizedObject
{
    private int _xUnitSize=1;
    public int XUnitSize
    {
        get => _xUnitSize;
        set
        {
            _xUnitSize = MH.Max(1, value);
            CalculateVertices();
            OnPropertyChanged();
        }
    }

    public float XSizeOfMm => XUnitSize / 10f;

    private int _yUnitSize=1;

    public int YUnitSize
    {
        get => _yUnitSize;
        set
        {
            _yUnitSize = MH.Max(1, value);
            CalculateVertices();
            OnPropertyChanged();
        }
    }

    public float YSizeOfMm => YUnitSize / 10f;

    private int _zUnitSize=1;

    public int ZUnitSize
    {
        get => _zUnitSize;
        set
        {
            _zUnitSize = MH.Max(1, value);
            CalculateVertices();
            OnPropertyChanged();
        }
    }

    public float ZSizeOfMm => ZUnitSize / 10f;

    private TexPoint[] _vertices;
    public int VerticesCount => _vertices.Length;

    public override uint[] lines => _lines;
    public uint[] _lines;

    private CuttingLines _cuttingLines;

    public CuttingLines CuttingLines
    {
        get => _cuttingLines;
        set
        {
            _cuttingLines = value;
            CalculateVertices();
        }
    }

    public Paths() : base("Paths")
    {
        CalculateVertices();
        UpdateTranslationMatrix();
    }

    private void CalculateVertices()
    {
        var size = 0;
        if (_cuttingLines is {points: { }})
        {
            size = _cuttingLines.points.Length;
        }

        _vertices = new TexPoint[size];

        for (int i = 0; i < size; i++)
        {
            var point = _cuttingLines.points[i];
            _vertices[i] = new TexPoint()
            {
                X = point.XPosInMm * XSizeOfMm,
                Y = point.YPosInMm * YSizeOfMm,
                Z = point.ZPosInMm * ZSizeOfMm,

                TexX = 0,
                TexY = 0
            };
        }

        _verticesDraw = new float[_vertices.Length * 3];
        for (int i = 0; i < _vertices.Length; i++)
        {
            _verticesDraw[3 * i] = _vertices[i].X;
            _verticesDraw[3 * i + 1] = _vertices[i].Y;
            _verticesDraw[3 * i + 2] = _vertices[i].Z;
        }

        GenerateLines();
    }

    private float[] _verticesDraw;

    private Matrix4 _modelMatrix = Matrix4.Identity;
    private Matrix4 _scaleMatrix = Matrix4.Identity;
    private Matrix4 _rotationMatrix = Matrix4.Identity;
    private Matrix4 _translationMatrix = Matrix4.Identity;

    public override void UpdateScaleMatrix()
    {
        _scaleMatrix = Matrix4.CreateScale(_scale);
        _modelMatrix = _scaleMatrix * _rotationMatrix * _translationMatrix;
    }

    public override void UpdateRotationMatrix(Axis axis)
    {
        _rotationMatrix = Matrix4.CreateFromQuaternion(
            new Quaternion(
                MH.DegreesToRadians(_rotation[0]),
                MH.DegreesToRadians(_rotation[1]),
                MH.DegreesToRadians(_rotation[2])));

        _modelMatrix = _scaleMatrix * _rotationMatrix * _translationMatrix;
    }

    public override void UpdateTranslationMatrix()
    {
        _translationMatrix = Matrix4.CreateTranslation(_position);
        _modelMatrix = _scaleMatrix * _rotationMatrix * _translationMatrix;
    }

    public override Matrix4 GetModelMatrix()
    {
        return _modelMatrix * CompositeOperationMatrix;
    }

    public override Matrix4 GetOnlyModelMatrix()
    {
        return _modelMatrix;
    }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * 3 * sizeof(float), _verticesDraw,
            BufferUsageHint.StaticDraw);

        GL.BindVertexArray(_vao);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _lines.Length * sizeof(uint), _lines,
            BufferUsageHint.StaticDraw);
    }

    private void GenerateLines()
    {
        //2 - ends of each line
        int size1 = 0;
        int size = 0;
        if (_cuttingLines is {points: { }})
        {
            size1 = _cuttingLines.points.Length - 1;
            size = 2 * (_cuttingLines.points.Length - 1);
        }

        if (_lines == null || _lines.Length != size)
            _lines = new uint[size];
        uint it = 0;
        for (int i = 0; i < size1; i++)
        {
            _lines[it++] = (uint) (i);
            _lines[it++] = (uint) (i + 1);
        }
    }

    public float[] GetVertices()
    {
        var vertices = new float[_vertices.Length * 3];
        var colors = new float[_vertices.Length * 3];

        for (int i = 0; i < _vertices.Length; i++)
        {
            vertices[i * TexPoint.Size] = _vertices[i].X;
            vertices[i * TexPoint.Size + 1] = _vertices[i].Y;
            vertices[i * TexPoint.Size + 2] = _vertices[i].Z;
            vertices[i * TexPoint.Size + 3] = _vertices[i].Z;
            vertices[i * TexPoint.Size + 4] = _vertices[i].Z;
            //
            // colors[i * Point.Size] = 1.0f;
            // colors[i * Point.Size + 1] = 0.0f;
            // colors[i * Point.Size + 2] = 0.0f;
        }

        return vertices;
    }

    public override void PassToDrawProcessor(DrawProcessor drawProcessor, EyeEnum eye, uint vertexAttributeLocation,
        uint normalAttributeLocation)
    {
        drawProcessor.ProcessObject(this, eye, vertexAttributeLocation, normalAttributeLocation);
    }
}