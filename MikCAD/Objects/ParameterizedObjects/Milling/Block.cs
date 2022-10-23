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

    public uint XVerticesCount => 1024;
    public uint YVerticesCount => 1024;

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

        if (_updatedRegionOfHeightMap)
        {
            var sub = _updatedRegion;
            GL.TexSubImage2D(TextureTarget.Texture2D,
                0,
                sub.min.X,
                sub.min.Y,
                sub.max.X - sub.min.X + 1,
                sub.max.Y - sub.min.Y + 1,
                PixelFormat.Red,
                PixelType.Float,
                GetSubTexture(sub));
            _updatedRegionOfHeightMap = false;
        }

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

    private float[] GetSubTexture(((int X, int Y) min, (int X, int Y) max) region)
    {
        int xSize = region.max.X - region.min.X + 1;
        int ySize = region.max.Y - region.min.Y + 1;
        float[] pixels = new float[xSize * ySize];
        var it = 0;
        var itt = 0;
        for (int j = region.min.Y; j <= region.max.Y; j++)
        {
            itt = 0;
            for (int i = region.min.X; i <= region.max.X; i++)
            {
                pixels[it * xSize + itt] = _heightMap[j * HeightMapWidth + i];
                itt++;
            }

            it++;
        }

        return pixels;
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
    private ((int X, int Y) min, (int X, int Y) max) _updatedRegion;
    private bool _updatedRegionOfHeightMap;

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

    private float _rInUnits = 0;

    public float UpdateHeightMap(Vector3 currPosInUnits, Vector3 dirInUnits, float distLeft,
        bool skipTextureUpdate)
    {
        return UpdateHeightMap(currPosInUnits, currPosInUnits + dirInUnits * distLeft, skipTextureUpdate);
    }

    public float UpdateHeightMap(Vector3 currPosInUnits, Vector3 endPosInUnits, bool skipTextureUpdate)
    {
        Vector2 currTexPos = GetTexPos(currPosInUnits);
        Vector2 endTexPos = GetTexPos(endPosInUnits);

        return Line(((int) (currTexPos.X), (int) (currTexPos.Y), currPosInUnits.Z),
            ((int) (endTexPos.X), (int) (endTexPos.Y), endPosInUnits.Z), skipTextureUpdate);
    }

    public void UpdateHeightMapInPoint(Vector3 posInUnits, bool skipTextureUpdate)
    {
        Vector2 posInTex = GetTexPos(posInUnits);

        var milledAmount = SetZValue((int) (posInTex.X), (int) (posInTex.Y), posInUnits.Z, true);
        //updatedHeightMap = true;
        if (!skipTextureUpdate)
        {
            if (milledAmount > Single.Epsilon)
            {
                _updatedRegionOfHeightMap = true;
            }

            var minX = Math.Min(Math.Max((int) posInTex.X - _rX, 0), HeightMapWidth -1);
            var maxX = Math.Max(Math.Min((int) posInTex.X + _rX, HeightMapWidth - 1), 0);
            var minY = Math.Min(Math.Max((int) posInTex.Y - _rY, 0), HeightMapHeight -1);
            var maxY = Math.Max(Math.Min((int) posInTex.Y + _rY, HeightMapHeight - 1),0);
            _updatedRegion = ((minX, minY), (maxX, maxY));
        }
    }


    private Vector2 GetTexPos(Vector3 currPosInUnits)
    {
        var xTex = HeightMapWidth * (0.5f + currPosInUnits.X / Simulator3C.Simulator.XGridSizeInUnits);
        var yTex = HeightMapHeight * (0.5f + currPosInUnits.Y / Simulator3C.Simulator.YGridSizeInUnits);
        return new Vector2(xTex, yTex);
    }

    bool InBox(Point a, int dx = 0, int dy = 0)
    {
        return a.X + dx > 0 && a.X + dx < HeightMapWidth && a.Y + dy > 0 && a.Y + dy < HeightMapHeight;
    }

    float Line((int X, int Y, float Z) a, (int X, int Y, float Z) b, bool skipTextureUpdate)
    {
        float totalMilledMaterial = 0;

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

        totalMilledMaterial += SetZValue(x, y, currZ);
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
                totalMilledMaterial += SetZValue(x, y, currZ);
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
                totalMilledMaterial += SetZValue(x, y, currZ);
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
                totalMilledMaterial += SetZValue(x, y, currZ);
            }
        }

        //updatedHeightMap = true;
        if (!skipTextureUpdate)
        {
            if (totalMilledMaterial > Single.Epsilon)
            {
                _updatedRegionOfHeightMap = true;
            }

            var minX = Math.Min(Math.Max(Math.Min(a.X, b.X) - _rX, 0), HeightMapWidth -1);
            var maxX = Math.Max(Math.Min(Math.Max(a.X, b.X) + _rX, HeightMapWidth - 1), 0);
            var minY = Math.Min(Math.Max(Math.Min(a.Y, b.Y) - _rY, 0), HeightMapHeight -1);
            var maxY = Math.Max(Math.Min(Math.Max(a.Y, b.Y) + _rY, HeightMapHeight - 1),0);
            _updatedRegion = ((minX, minY), (maxX, maxY));
        }

        return totalMilledMaterial;
    }

    private float SetZValue(int x, int y, float z, bool circle = false)
    {
        float totalMilledMaterial = 0;

        if (!circle)
        {
            int it = 0;
            for (int i = -_rY + 1; i < _rY; i++)
            {
                int j = 0;
                if ((y + i) * HeightMapWidth + x + j > 0
                    && (y + i) * HeightMapWidth + x + j < HeightMapWidth * HeightMapHeight)
                {
                    var newZ = z;
                    if (Simulator3C.Simulator.SphericalSelected)
                    {
                        newZ = z + _yZArray[it++];
                    }

                    if (_heightMap[(y + i) * HeightMapWidth + x + j] > newZ)
                    {
                        totalMilledMaterial += _heightMap[(y + i) * HeightMapWidth + x + j] - newZ;
                        _heightMap[(y + i) * HeightMapWidth + x + j] = newZ;
                    }
                }
            }

            it = 0;
            for (int j = -_rX + 1; j < _rX; j++)
            {
                int i = 0;
                if ((y + i) * HeightMapWidth + x + j > 0
                    && (y + i) * HeightMapWidth + x + j < HeightMapWidth * HeightMapHeight)
                {
                    var newZ = z;
                    if (Simulator3C.Simulator.SphericalSelected)
                    {
                        newZ = z + _xZArray[it++];
                    }

                    if (_heightMap[(y + i) * HeightMapWidth + x + j] > newZ)
                    {
                        totalMilledMaterial += _heightMap[(y + i) * HeightMapWidth + x + j] - newZ;
                        _heightMap[(y + i) * HeightMapWidth + x + j] = newZ;
                    }
                }
            }
        }
        else
        {
            for (int i = -_rY + 1; i < _rY; i++)
            {
                for (int j = -_rX + 1; j < _rX; j++)
                {
                    //var newZ = GetZForYXInTex(i, j, y, x, z);
                    var newZ = z;

                    if ((y + i) * HeightMapWidth + x + j > 0
                        && (y + i) * HeightMapWidth + x + j < HeightMapWidth * HeightMapHeight)
                    {
                        if (_heightMap[(y + i) * HeightMapWidth + x + j] > newZ)
                        {
                            totalMilledMaterial += _heightMap[(y + i) * HeightMapWidth + x + j] - newZ;
                            _heightMap[(y + i) * HeightMapWidth + x + j] = newZ;
                        }
                    }
                }
            }
        }


        return totalMilledMaterial;
    }

    private float GetZForYXInTex(int i, int j, int centerY, int centerX, float z)
    {
        var unitPosY = ConvertFromTexYToUnitsY(i);
        var unitPosX = ConvertFromTexXToUnitsX(j);
        var unitCenterY = ConvertFromTexYToUnitsY(centerY);
        var unitCenterX = ConvertFromTexXToUnitsX(centerX);

        if ((unitPosX - unitCenterX) * (unitPosX - unitCenterX) + (unitPosY - unitCenterY) * (unitPosY - unitCenterY) -
            (_rInUnits + 2) * (_rInUnits + 2) < Single.Epsilon)
        {
            // if (Simulator3C.Simulator.SphericalSelected)
            // {
            //     return z + _rInUnits - MathF.Sqrt(_rInUnits * _rInUnits -
            //                                       (unitPosX - unitCenterX) * (unitPosX - unitCenterX) -
            //                                       (unitPosY - unitCenterY) * (unitPosY - unitCenterY));
            // }
            // else
            {
                return z;
            }
        }
        else
        {
            return float.MaxValue;
        }
    }

    private int ConvertXUnitsToTexX(float rInUnits)
    {
        return (int) (rInUnits * (HeightMapWidth / (float) Simulator3C.Simulator.XGridSizeInUnits));
    }

    private int ConvertYUnitsToTexY(float rInUnits)
    {
        return (int) (rInUnits * (HeightMapHeight / (float) Simulator3C.Simulator.YGridSizeInUnits));
    }

    private float ConvertFromTexXToUnitsX(float value)
    {
        return value * (Simulator3C.Simulator.XGridSizeInUnits / (float) HeightMapWidth);
    }

    private float ConvertFromTexYToUnitsY(float value)
    {
        return value * (Simulator3C.Simulator.YGridSizeInUnits / (float) HeightMapHeight);
    }

    public void UpdateTexture()
    {
        updatedHeightMap = true;
    }

    private int _rX, _rY;
    private float[] _yZArray;
    private float[] _xZArray;

    public void CalculateSimulationParams(float rInUnits)
    {
        _rInUnits = rInUnits;
        _rX = ConvertXUnitsToTexX(rInUnits);
        _rY = ConvertYUnitsToTexY(rInUnits);

        _yZArray = new float[(_rY - 1) * 2 + 1];
        int it = 0;
        for (int i = -_rY + 1; i < _rY; i++)
        {
            _yZArray[it++] = ConvertFromTexXToUnitsX(_rX) - ConvertFromTexXToUnitsX(MathF.Sqrt(_rY * _rY - i * i));
            //_yZArray[it++] = ConvertXUnitsToTexX(0.5f*(1.0f/_rY * i*i));
        }

        _xZArray = new float[(_rX - 1) * 2 + 1];
        it = 0;
        for (int i = -_rX + 1; i < _rX; i++)
        {
            _xZArray[it++] = ConvertFromTexYToUnitsY(_rY) - ConvertFromTexYToUnitsY(MathF.Sqrt(_rX * _rX - i * i));
            //_xZArray[it++] = ConvertYUnitsToTexY(0.5f*(1.0f/_rX * i*i));
        }
    }
}