using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

    private readonly int _textureHandle0 = GL.GenTexture();
    private readonly int _textureHandle1 = GL.GenTexture();

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

                        TexX = (float) j / XVerticesCount,
                        TexY = (float) k / YVerticesCount
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
        triangleStart = (XVerticesCount - 1) * YVerticesCount;
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

            triangleStart += YVerticesCount;
        }

        //Ściana y==max
        triangleStart = YVerticesCount - 1;
        for (int j = 0; j < XVerticesCount - 1; j++)
        {
            triangleList.Add(triangleStart);
            triangleList.Add(triangleStart + YVerticesCount);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount);

            triangleList.Add(triangleStart + YVerticesCount);
            triangleList.Add(triangleStart + XVerticesCount * YVerticesCount);
            triangleList.Add(triangleStart + YVerticesCount + XVerticesCount * YVerticesCount);

            triangleStart += YVerticesCount;
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
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _textureHandle0);
        if (updated)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Nearest);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Tex0Width, Tex0Height, 0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte, Texture0);
            updated = false;
        }


        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _textureHandle1);
        if (updatedHeightMap)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Nearest);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, HeightMapWidth, HeightMapHeight, 0,
                PixelFormat.Red,
                PixelType.Float, HeightMap);
            updatedHeightMap = false;
        }
    }

    private bool updated = false;
    private bool updatedHeightMap = false;

    private byte[] _texture0;

    public byte[] Texture0
    {
        get => _texture0;
        set
        {
            _texture0 = value;
            updated = true;
        }
    }

    public int Tex0Width { get; set; }
    public int Tex0Height { get; set; }


    private float[] _heightMap;

    public float[] HeightMap
    {
        get => _heightMap;
        set
        {
            _heightMap = value;
            updatedHeightMap = true;
        }
    }

    public int HeightMapWidth { get; set; }
    public int HeightMapHeight { get; set; }

    public void UpdateHeightMap(Vector3 currPosInUnits, Vector3 dirInUnits, float distLeft, float rInUnits)
    {
        UpdateHeightMap(currPosInUnits, currPosInUnits + dirInUnits * distLeft, rInUnits);
    }

    public void UpdateHeightMap(Vector3 currPosInUnits, Vector3 endPosInUnits, float rInUnits)
    {
        Vector2 currTexPos = GetTexPos(currPosInUnits);
        Vector2 endTexPos = GetTexPos(endPosInUnits);
        int rInTex = ConvertXUnitsToTexX(rInUnits);

        Line(((int) (currTexPos.X), (int) (currTexPos.Y), currPosInUnits.Z),
            ((int) (endTexPos.X), (int) (endTexPos.Y), endPosInUnits.Z),
            rInTex);
    }

    public void UpdateHeightMapInPoint(Vector3 posInUnits, float rInUnits)
    {
        Vector2 posInTex = GetTexPos(posInUnits);
        int rInTex = ConvertXUnitsToTexX(rInUnits);
        
        SetZValue((int) (posInTex.X), (int) (posInTex.Y), posInUnits.Z, rInTex, true);
    }
    
    private int ConvertXUnitsToTexX(float rInUnits)
    {
        return (int) (rInUnits * ((HeightMapWidth / 2.0f) / (Simulator3C.Simulator.XGridSizeInUnits / 2.0f)));
    }

    private Vector2 GetTexPos(Vector3 currPosInUnits)
    {
        var xTex = (HeightMapWidth / 2.0f * (1 + currPosInUnits.X / (Simulator3C.Simulator.XGridSizeInUnits / 2.0f)));
        var yTex = (HeightMapHeight / 2.0f * (1 + currPosInUnits.Y / (Simulator3C.Simulator.YGridSizeInUnits / 2.0f)));
        return new Vector2(xTex, yTex);
    }

    bool InBox(Point a, int dx = 0, int dy = 0)
    {
        return a.X + dx > 0 && a.X + dx < HeightMapWidth && a.Y + dy > 0 && a.Y + dy < HeightMapHeight;
    }

    void Line((int X, int Y, float Z) a, (int X, int Y, float Z) b, int r)
    {
        int x1 = 0, x2 = 02, y1 = 0, y2 = 0, dx = 0, dy = 0, d = 0, incrE = 0, incrNE = 0, x = 0, y = 0, incrY = 0;
        float currZ = a.Z;
        float dz = b.Z - a.Z;
        //podział wzdłuż OY
        if (b.X < a.X)
        {
            (a, b) = (b, a);
        }

        //Te same wartości dla kazdego przypadku
        x1 = (int) a.X;
        x2 = (int) b.X;
        y1 = (int) a.Y;
        y2 = (int) b.Y;
        dx = x2 - x1;
        x = x1;
        y = y1;
        //315-360
        if (b.Y >= a.Y && b.Y - a.Y <= b.X - a.X)
        {
            dy = y2 - y1;
            d = 2 * dy - dx;
            incrE = 2 * dy;
            incrNE = 2 * (dy - dx);
            incrY = 1;
        }
        //0-45
        else if (b.Y < a.Y && a.Y - b.Y <= b.X - a.X)
        {
            dy = y1 - y2;
            d = 2 * dy - dx;
            incrE = 2 * dy;
            incrNE = 2 * (dy - dx);
            incrY = -1;
        }
        //270-315
        else if (b.Y >= a.Y)
        {
            dy = y2 - y1;
            d = 2 * dx - dy;
            incrE = 2 * dx;
            incrNE = 2 * (dx - dy);
        }
        //45-90
        else
        {
            dy = y1 - y2;
            d = 2 * dx - dy;
            incrE = 2 * dx;
            incrNE = 2 * (dx - dy);
        }

        SetZValue(x, y, currZ, r);
        //315-45
        if (dx > dy)
        {
            while (x < x2)
            {
                if (d < 0)
                    //chooseE 
                {
                    d += incrE;
                    x++;
                }
                else
                    //chooseNE
                {
                    d += incrNE;
                    x++;
                    y += incrY;
                }

                currZ += dz;
                SetZValue(x, y, currZ, r);
            }
        }
        //270-315
        else if (y2 > y)
        {
            while (y < y2)
            {
                if (d < 0)
                    //chooseE 
                {
                    d += incrE;
                    y++;
                }
                else
                    //chooseNE
                {
                    d += incrNE;
                    y++;
                    x++;
                }

                currZ += dz;
                SetZValue(x, y, currZ, r);
            }
        }
        //45-90
        else
        {
            while (y > y2)
            {
                if (d < 0)
                    //chooseE 
                {
                    d += incrE;
                    y--;
                }
                else
                    //chooseNE
                {
                    d += incrNE;
                    y--;
                    x++;
                }

                currZ += dz;
                SetZValue(x, y, currZ, r);
            }
        }
    }

    private void SetZValue(int x, int y, float z, int r, bool circle = false)
    {
        if (x >= 0
            && x < HeightMapWidth
            && y >= 0
            && y < HeightMapHeight)
        {
            if (!circle)
            {
                if (_heightMap[y * HeightMapWidth + x] > z)
                {
                    _heightMap[y * HeightMapWidth + x] = z;
                }

                for (int i = -r + 1; i < r; i++)
                {
                    int j = 0;
                    if ((y + i) * HeightMapWidth + x + j > 0
                        && (y + i) * HeightMapWidth + x + j < HeightMapWidth * HeightMapHeight)
                    {
                        if (_heightMap[(y + i) * HeightMapWidth + x + j] > z)
                        {
                            _heightMap[(y + i) * HeightMapWidth + x + j] = z;
                        }
                    }
                    // j = -r+1;
                    // if ((y + i) * HeightMapWidth + x + j > 0
                    //     && (y + i) * HeightMapWidth + x + j < HeightMapWidth * HeightMapHeight)
                    // {
                    //     if (_heightMap[(y + i) * HeightMapWidth + x + j] > z)
                    //     {
                    //         _heightMap[(y + i) * HeightMapWidth + x + j] = z;
                    //     }
                    // }
                }

                for (int j = -r + 1; j < r; j++)
                {
                    int i = 0;
                    if ((y + i) * HeightMapWidth + x + j > 0
                        && (y + i) * HeightMapWidth + x + j < HeightMapWidth * HeightMapHeight)
                    {
                        if (_heightMap[(y + i) * HeightMapWidth + x + j] > z)
                        {
                            _heightMap[(y + i) * HeightMapWidth + x + j] = z;
                        }
                    }
                    // i = -r+1;
                    // if ((y + i) * HeightMapWidth + x + j > 0
                    //     && (y + i) * HeightMapWidth + x + j < HeightMapWidth * HeightMapHeight)
                    // {
                    //     if (_heightMap[(y + i) * HeightMapWidth + x + j] > z)
                    //     {
                    //         _heightMap[(y + i) * HeightMapWidth + x + j] = z;
                    //     }
                    // }
                }
            }
            else
            {
                for (int i = -r + 1; i < r; i++)
                {
                    for (int j = -r + 1; j < r; j++)
                    {
                        if ((y + i) * HeightMapWidth + x + j > 0
                            && (y + i) * HeightMapWidth + x + j < HeightMapWidth * HeightMapHeight)
                        {
                            if (_heightMap[(y + i) * HeightMapWidth + x + j] > z)
                            {
                                _heightMap[(y + i) * HeightMapWidth + x + j] = z;
                            }
                        }
                    }
                }
            }

           // updatedHeightMap = true;
        }
    }
}