// using System;
// using System.Collections.Generic;
// using System.Data;
// using MikCAD.Objects;
// using MikCAD.Utilities;
// using OpenTK.Graphics.OpenGL;
// using OpenTK.Mathematics;
// using SharpSceneSerializer.DTOs.Types;
// using MH = OpenTK.Mathematics.MathHelper;
//
// namespace MikCAD.Objects.ParameterizedObjects.Milling;
//
// public class Block : ParameterizedObject
// {
//     private int _xUnitSize;
//     public int XUnitSize
//     {
//         get => _xUnitSize;
//         set
//         {
//             _xUnitSize = MH.Max(1, value);
//             CalculateVertices();
//             OnPropertyChanged();
//         }
//     }
//
//     private int _yUnitSize;
//     public int YUnitSize
//     {
//         get => _yUnitSize;
//         set
//         {
//             _yUnitSize = MH.Max(1, value);
//             CalculateVertices();
//             OnPropertyChanged();
//         }
//     }
//
//     private int _zUnitSize;
//     public int ZUnitSize
//     {
//         get => _zUnitSize;
//         set
//         {
//             _zUnitSize = MH.Max(1, value);
//             CalculateVertices();
//             OnPropertyChanged();
//         }
//     }
//     
//     public int XDivisions
//     {
//         get => _xDivisions;
//         set
//         {
//             _xDivisions = MH.Max(value, 3);
//             CalculateVertices();
//             OnPropertyChanged(nameof(XDivisions));
//         }
//     }
//
//     public int YDivisions
//     {
//         get => _yDivisions;
//         set
//         {
//             _yDivisions = MH.Max(value, 3);
//             CalculateVertices();
//             OnPropertyChanged(nameof(YDivisions));
//         }
//     }
//     
//     private int _xDivisions;
//     private int _yDivisions;
//
//     private TexPoint[] _vertices;
//     public int VerticesCount => _vertices.Length;
//
//     public override uint[] lines => _lines;
//     public uint[] _lines;
//
//     public Block() : base("Block")
//     {
//         CalculateVertices();
//         UpdateTranslationMatrix();
//     }
//
//     private void CalculateVertices()
//     {
//         _vertices = new TexPoint[];
//
//         for(int i=0; i<2; i++)
//         {
//             for (int j = 0; j < 2; j++)
//             {
//                 for (int k = 0; k < 2; k++)
//                 {
//                     _vertices[i * (_yDivisions + 1) + j] = new TexPoint()
//                     {
//                         X = ,
//                         Y = ,
//                         Z = ,
//
//                         TexX = ,
//                         TexY = 
//                     };
//                 }
//             }
//         }
//
//         _verticesDraw = new float[_vertices.Length * TexPoint.Size];
//         for (int i = 0; i < _vertices.Length; i++)
//         {
//             _verticesDraw[TexPoint.Size * i] = _vertices[i].X;
//             _verticesDraw[TexPoint.Size * i + 1] = _vertices[i].Y;
//             _verticesDraw[TexPoint.Size * i + 2] = _vertices[i].Z;
//             _verticesDraw[TexPoint.Size * i + 3] = _vertices[i].TexX;
//             _verticesDraw[TexPoint.Size * i + 4] = _vertices[i].TexY;
//         }
//
//         GenerateLines();
//     }
//
//     private float[] _verticesDraw;
//
//     private Matrix4 _modelMatrix = Matrix4.Identity;
//     private Matrix4 _scaleMatrix = Matrix4.Identity;
//     private Matrix4 _rotationMatrix = Matrix4.Identity;
//     private Matrix4 _translationMatrix = Matrix4.Identity;
//
//     public override void UpdateScaleMatrix()
//     {
//         _scaleMatrix = Matrix4.CreateScale(_scale);
//         _modelMatrix = _scaleMatrix * _rotationMatrix * _translationMatrix;
//     }
//
//     public override void UpdateRotationMatrix(Axis axis)
//     {
//         _rotationMatrix = Matrix4.CreateFromQuaternion(
//             new Quaternion(
//                 MH.DegreesToRadians(_rotation[0]),
//                 MH.DegreesToRadians(_rotation[1]),
//                 MH.DegreesToRadians(_rotation[2])));
//
//         _modelMatrix = _scaleMatrix * _rotationMatrix * _translationMatrix;
//     }
//
//     public override void UpdateTranslationMatrix()
//     {
//         _translationMatrix = Matrix4.CreateTranslation(_position);
//         _modelMatrix = _scaleMatrix * _rotationMatrix * _translationMatrix;
//     }
//
//     public override Matrix4 GetModelMatrix()
//     {
//         return _modelMatrix * CompositeOperationMatrix;
//     }
//
//     public override Matrix4 GetOnlyModelMatrix()
//     {
//         return _modelMatrix;
//     }
//
//     public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
//     {
//         GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
//         GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * TexPoint.Size * sizeof(float), _verticesDraw,
//             BufferUsageHint.StaticDraw);
//
//         GL.BindVertexArray(_vao);
//         GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float), 0);
//         GL.EnableVertexAttribArray(0);
//
//         GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float),
//             3 * sizeof(float));
//         GL.EnableVertexAttribArray(1);
//
//
//         GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
//         GL.BufferData(BufferTarget.ElementArrayBuffer, _lines.Length * sizeof(uint), _lines,
//             BufferUsageHint.StaticDraw);
//     }
//
//     private void GenerateLines()
//     {
//         //2 - ends of each line
//         //2 - same number of big and small circles
//         int size = 2 * _xDivisions * 2 * _yDivisions;
//         if (_lines == null || _lines.Length != size)
//             _lines = new uint[size];
//         uint it = 0;
//         for (int i = 0; i < _xDivisions; i++)
//         {
//             for (int j = 0; j < _yDivisions; j++)
//             {
//                 //duże okręgi
//                 if (i != _xDivisions)
//                 {
//                     _lines[it++] = (uint) (i * (_yDivisions + 1) + j);
//                     _lines[it++] = (uint) ((i + 1) * (_yDivisions + 1) + j);
//                 }
//
//                 //małe okręgi
//                 if (j != _yDivisions)
//                 {
//                     _lines[it++] = (uint) (i * (_yDivisions + 1) + j);
//                     _lines[it++] = (uint) (i * (_yDivisions + 1) + j + 1);
//                 }
//             }
//         }
//     }
//
//     public float[] GetVertices()
//     {
//         var vertices = new float[_vertices.Length * 3];
//         var colors = new float[_vertices.Length * 3];
//
//         for (int i = 0; i < _vertices.Length; i++)
//         {
//             vertices[i * TexPoint.Size] = _vertices[i].X;
//             vertices[i * TexPoint.Size + 1] = _vertices[i].Y;
//             vertices[i * TexPoint.Size + 2] = _vertices[i].Z;
//             vertices[i * TexPoint.Size + 3] = _vertices[i].Z;
//             vertices[i * TexPoint.Size + 4] = _vertices[i].Z;
//             //
//             // colors[i * Point.Size] = 1.0f;
//             // colors[i * Point.Size + 1] = 0.0f;
//             // colors[i * Point.Size + 2] = 0.0f;
//         }
//
//         return vertices;
//     }
//
//     public override void PassToDrawProcessor(DrawProcessor drawProcessor, EyeEnum eye, uint vertexAttributeLocation,
//         uint normalAttributeLocation)
//     {
//         drawProcessor.ProcessObject(this, eye, vertexAttributeLocation, normalAttributeLocation);
//     }
//
//     public bool IsUWrapped => true;
//     public bool IsVWrapped => true;
//     public float USize => 2 * MathF.PI;
//     public float VSize => 2 * MathF.PI;
//
//     private Intersection _intersection;
//
//     public Intersection Intersection
//     {
//         get => _intersection;
//         set
//         {
//             _intersection = value;
//             var height = TexHeight = _intersection._firstObj == this
//                 ? _intersection.firstBmp.Height
//                 : _intersection.secondBmp.Height;
//             var width = TexWidth = _intersection._firstObj == this
//                 ? _intersection.firstBmp.Width
//                 : _intersection.secondBmp.Width;
//
//             var bmp = _intersection._firstObj == this
//                 ? _intersection.firstBmp
//                 : _intersection.secondBmp;
//
//             var texture = new List<byte>(4 * width * height);
//             for (int j = 0; j < height; j++)
//             {
//                 for (int i = 0; i < width; i++)
//                 {
//                     var color = bmp.GetPixel(i, j);
//                     texture.Add(color.R);
//                     texture.Add(color.G);
//                     texture.Add(color.B);
//                     texture.Add(color.A);
//                 }
//             }
//
//             _texture = texture.ToArray();
//             OnPropertyChanged(nameof(Intersection));
//         }
//     }
//
//     public override void SetTexture()
//     {
//         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
//         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
//         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
//             (int) TextureMinFilter.Nearest);
//         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
//             (int) TextureMagFilter.Nearest);
//         GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TexWidth, TexHeight, 0,
//             PixelFormat.Rgba,
//             PixelType.UnsignedByte, Texture);
//     }
//
//     private byte[] _texture;
//     public byte[] Texture => _texture;
//
//     public int TexWidth { get; private set; }
//     public int TexHeight { get; private set; }
// }