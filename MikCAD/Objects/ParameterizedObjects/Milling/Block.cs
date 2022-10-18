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

public class Block : ParameterizedObject
{
    private int _xUnitSize;

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

    private int _yUnitSize;

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

    private int _zUnitSize;

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

    public uint XVerticesCount => 512;
    public uint YVerticesCount => 512;

    private TexPoint[] _vertices;
    private uint[] _vertIndices;
    public int VerticesCount => _vertices.Length;

    public override uint[] lines => _lines;
    public uint[] _lines;

    public uint[] TrianglesIndices => _trianglesIndices;
    private uint[] _trianglesIndices;

    public Block() : base("Block")
    {
        CalculateVertices();
        UpdateTranslationMatrix();
    }

    public void UpdateVertices() => CalculateVertices();

    private void CalculateVertices()
    {
        _vertices = new TexPoint[XVerticesCount * YVerticesCount * 2];
        _vertIndices = new uint[XVerticesCount * YVerticesCount * 2];

        var stepX = (float) Simulator3C.Simulator.XGridSizeInUnits / XVerticesCount;
        var stepY = (float) Simulator3C.Simulator.YGridSizeInUnits / YVerticesCount;
        var stepZ = (float) Simulator3C.Simulator.ZGridSizeInUnits;

        var startX = -(float) Simulator3C.Simulator.XGridSizeInUnits / 2;
        var startY = -(float) Simulator3C.Simulator.YGridSizeInUnits / 2;

        uint it = 0;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < XVerticesCount; j++)
            {
                for (int k = 0; k < YVerticesCount; k++)
                {
                    _vertices[i * (XVerticesCount * YVerticesCount) + j * YVerticesCount + k] = new TexPoint()
                    {
                        X = startX + j * stepX,
                        Y = startY + k * stepY,
                        Z = i * stepZ,

                        TexX = (float)j/XVerticesCount,
                        TexY = (float)k/YVerticesCount
                    };
                    _vertIndices[it] = it;
                    it++;
                }
            }
        }

        _verticesDraw = new float[_vertices.Length * TexPoint.Size];
        for (int i = 0; i < _vertices.Length; i++)
        {
            _verticesDraw[TexPoint.Size * i] = _vertices[i].X;
            _verticesDraw[TexPoint.Size * i + 1] = _vertices[i].Y;
            _verticesDraw[TexPoint.Size * i + 2] = _vertices[i].Z;
            _verticesDraw[TexPoint.Size * i + 3] = _vertices[i].TexX;
            _verticesDraw[TexPoint.Size * i + 4] = _vertices[i].TexY;
        }

        GenerateLines();
        GenerateTriangles();
    }

    public void GenerateTriangles()
    {
        var triangleList = new List<uint>();

        uint triangleStart = 0;
        //Górna ściana
        triangleStart = XVerticesCount * YVerticesCount;
        for (int i = 0; i < XVerticesCount - 1; i++)
        {
            for (int j = 0; j < YVerticesCount - 1; j++)
            {
                triangleList.Add(triangleStart);
                triangleList.Add(triangleStart + 1);
                triangleList.Add(triangleStart + YVerticesCount);

                triangleList.Add(triangleStart + 1);
                triangleList.Add(triangleStart + YVerticesCount);
                triangleList.Add(triangleStart + YVerticesCount + 1);

                triangleStart++;
            }

            triangleStart++;
        }

        //Ściana x==0
        triangleStart = 0;
        for (int j = 0; j < YVerticesCount - 1; j++)
        {
            triangleList.Add(triangleStart);
            triangleList.Add(triangleStart + 1);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount);
        
            triangleList.Add(triangleStart + 1);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount + 1);
        
            triangleStart++;
        }
        
        //Ściana x==max
        triangleStart = (XVerticesCount-1)*YVerticesCount;
        for (int j = 0; j < YVerticesCount - 1; j++)
        {
            triangleList.Add(triangleStart);
            triangleList.Add(triangleStart + 1);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount);
        
            triangleList.Add(triangleStart + 1);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount + 1);
        
            triangleStart++;
        }
        
        //Ściana y==0
        triangleStart = 0;
        for (int j = 0; j < XVerticesCount - 1; j++)
        {
            triangleList.Add(triangleStart);
            triangleList.Add(triangleStart + YVerticesCount);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount);
        
            triangleList.Add(triangleStart + YVerticesCount);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount);
            triangleList.Add(triangleStart + YVerticesCount + XVerticesCount * YVerticesCount);
        
            triangleStart+= YVerticesCount;
        }
        
        //Ściana y==max
        triangleStart = YVerticesCount -1;
        for (int j = 0; j < XVerticesCount - 1; j++)
        {
            triangleList.Add(triangleStart);
            triangleList.Add(triangleStart + YVerticesCount);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount);
        
            triangleList.Add(triangleStart + YVerticesCount);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount);
            triangleList.Add(triangleStart + YVerticesCount + XVerticesCount * YVerticesCount);
        
            triangleStart+= YVerticesCount;
        }
        
        //Dolna ściana
        triangleStart = 0;
        for (int i = 0; i < XVerticesCount - 1; i++)
        {
            for (int j = 0; j < YVerticesCount - 1; j++)
            {
                triangleList.Add(triangleStart);
                triangleList.Add(triangleStart + 1);
                triangleList.Add(triangleStart + YVerticesCount);
        
                triangleList.Add(triangleStart + 1);
                triangleList.Add(triangleStart + YVerticesCount);
                triangleList.Add(triangleStart + YVerticesCount + 1);
        
                triangleStart++;
            }
        
            triangleStart++;
        }
        
        _trianglesIndices = triangleList.ToArray();
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
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * TexPoint.Size * sizeof(float), _verticesDraw,
            BufferUsageHint.StaticDraw);

        GL.BindVertexArray(_vao);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float),
            3 * sizeof(float));
        GL.EnableVertexAttribArray(1);


        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _trianglesIndices.Length * sizeof(uint), _trianglesIndices,
            BufferUsageHint.StaticDraw);
    }

    private void GenerateLines()
    {
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

    public override void SetTexture()
    {
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int) TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int) TextureMagFilter.Nearest);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TexWidth, TexHeight, 0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte, Texture);
    }

    private byte[] _texture;
    public byte[] Texture
    {
        get => _texture;
        set {_texture = value; }
    }

    public int TexWidth { get; set; }
    public int TexHeight { get; set; }
}