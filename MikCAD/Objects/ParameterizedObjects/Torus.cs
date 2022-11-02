using System;
using System.Collections.Generic;
using System.Data;
using MikCAD.Objects;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Types;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD
{
    public class Torus : ParameterizedObject, IIntersectable
    {
        public float R
        {
            get => _R;
            set
            {
                _R = MH.Max(value, 0.0f);
                CalculateVertices();
                OnPropertyChanged(nameof(R));
            }
        }

        public float r
        {
            get => _r;
            set
            {
                _r = MH.Max(value, 0.0f);
                CalculateVertices();
                OnPropertyChanged(nameof(r));
            }
        }

        public int SectorsCount
        {
            get => _sectorsCount;
            set
            {
                _sectorsCount = MH.Max(value, 3);
                CalculateVertices();
                OnPropertyChanged(nameof(SectorsCount));
            }
        }

        public int CirclesCount
        {
            get => _circlesCount;
            set
            {
                _circlesCount = MH.Max(value, 3);
                CalculateVertices();
                OnPropertyChanged(nameof(CirclesCount));
            }
        }

        private float _R;
        private float _r;
        private float _thetaStep;
        private float _phiStep;
        private int _sectorsCount;
        private int _circlesCount;

        private TexPoint[] _vertices;
        public int VerticesCount => _vertices.Length;

        public override uint[] lines => _lines;
        public uint[] _lines;

        public Torus() : base("Torus")
        {
            _R = 1;
            _r = 0.5f;
            _sectorsCount = 6;
            _circlesCount = 6;
            CalculateVertices();
            UpdateTranslationMatrix();
        }

        private void CalculateVertices()
        {
            _phiStep = MathHelper.DegreesToRadians(360.0f / _sectorsCount);
            _thetaStep = MathHelper.DegreesToRadians(360.0f / _circlesCount);

            _vertices = new TexPoint[(_sectorsCount + 1) * (_circlesCount + 1)];

            for (int i = 0; i < _sectorsCount + 1; i++)
            {
                for (int j = 0; j < _circlesCount + 1; j++)
                {
                    _vertices[i * (_circlesCount + 1) + j] = new TexPoint()
                    {
                        X = (R + r * (float) MathHelper.Cos(j * _thetaStep)) * (float) MathHelper.Cos(i * _phiStep),
                        Y = r * (float) MathHelper.Sin(j * _thetaStep),
                        Z = (R + r * (float) MathHelper.Cos(j * _thetaStep)) * (float) MathHelper.Sin(i * _phiStep),

                        TexX = (float) i / SectorsCount,
                        TexY = (float) j / CirclesCount
                    };
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
                new Quaternion(MH.DegreesToRadians(_rotation[0]),
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
            GL.BufferData(BufferTarget.ElementArrayBuffer, _lines.Length * sizeof(uint), _lines,
                BufferUsageHint.StaticDraw);
        }

        private void GenerateLines()
        {
            //2 - ends of each line
            //2 - same number of big and small circles
            int size = 2 * _sectorsCount * 2 * _circlesCount;
            if (_lines == null || _lines.Length != size)
                _lines = new uint[size];
            uint it = 0;
            for (int i = 0; i < _sectorsCount; i++)
            {
                for (int j = 0; j < _circlesCount; j++)
                {
                    //duże okręgi
                    if (i != _sectorsCount)
                    {
                        _lines[it++] = (uint) (i * (_circlesCount+1) + j);
                        _lines[it++] = (uint) ((i + 1) * (_circlesCount+1) + j);
                    }

                    //małe okręgi
                    if (j != _circlesCount)
                    {
                        _lines[it++] = (uint) (i * (_circlesCount+1) + j);
                        _lines[it++] = (uint) (i * (_circlesCount+1) + j + 1);
                    }
                }
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

        public static explicit operator SharpSceneSerializer.DTOs.GeometryObjects.Torus(Torus torus)
        {
            SharpSceneSerializer.DTOs.GeometryObjects.Torus ret = new SharpSceneSerializer.DTOs.GeometryObjects.Torus()
            {
                Id = torus.Id,
                Name = torus.Name,
                Position = new Float3(torus.posX, torus.posY, torus.posZ),
                Rotation = new Float3(torus.rotX, torus.rotY, torus.rotZ),
                Scale = new Float3(torus.scaleX, torus.scaleY, torus.scaleZ),
                LargeRadius = torus.R,
                SmallRadius = torus.r,
                Samples = new Uint2((uint) torus.CirclesCount, (uint) torus.SectorsCount)
            };

            return ret;
        }

        public static explicit operator Torus(SharpSceneSerializer.DTOs.GeometryObjects.Torus torus)
        {
            Torus ret = new Torus()
            {
                Id = torus.Id,
                Name = torus.Name,
                posX = torus.Position.X,
                posY = torus.Position.Y,
                posZ = torus.Position.Z,
                rotX = torus.Rotation.X,
                rotY = torus.Rotation.Y,
                rotZ = torus.Rotation.Z,
                scaleX = torus.Scale.X,
                scaleY = torus.Scale.Y,
                scaleZ = torus.Scale.Z,
                R = torus.LargeRadius,
                r = torus.SmallRadius,
                CirclesCount = (int) torus.Samples.X,
                SectorsCount = (int) torus.Samples.Y,
            };

            return ret;
        }

        //TODO: uwzględnić macierz
        public Vector3 GetValueAt(float u, float v)
        {
            var phi = u;
            var theta = v;


            var X = (R + r * (float) MathHelper.Cos(theta)) * (float) MathHelper.Cos(phi);
            var Y = r * (float) MathHelper.Sin(theta);
            var Z = (R + r * (float) MathHelper.Cos(theta)) * (float) MathHelper.Sin(phi);
            return (new Vector4(X, Y, Z, 1) * GetModelMatrix()).Xyz;
        }

        public Vector3 GetUDerivativeAt(float u, float v)
        {
            var phi = u;
            var theta = v;


            var X = -(R + r * (float) MathHelper.Cos(theta)) * (float) MathHelper.Sin(phi);
            var Y = 0;
            var Z = (R + r * (float) MathHelper.Cos(theta)) * (float) MathHelper.Cos(phi);
            return (new Vector4(X, Y, Z, 0) * GetModelMatrix()).Xyz;
        }

        public Vector3 GetVDerivativeAt(float u, float v)
        {
            var phi = u;
            var theta = v;


            var X = -r * (float) MathHelper.Sin(theta) * (float) MathHelper.Cos(phi);
            var Y = r * (float) MathHelper.Cos(theta);
            var Z = -r * (float) MathHelper.Sin(theta) * (float) MathHelper.Sin(phi);
            return (new Vector4(X, Y, Z, 0) * GetModelMatrix()).Xyz;
        }

        public (Vector3 pos, Vector3 dU, Vector3 dV) GetPositionAndGradient(float u, float v)
        {
            return (GetValueAt(u, v), GetUDerivativeAt(u, v), GetVDerivativeAt(u, v));
        }

        public bool IsUWrapped => true;
        public bool IsVWrapped => true;
        public float USize => 2 * MathF.PI;
        public float VSize => 2 * MathF.PI;

        private Intersection _intersection;

        public Intersection Intersection
        {
            get => _intersection;
            set
            {
                _intersection = value;
                var height = TexHeight = _intersection._firstObj == this
                    ? _intersection.firstBmp.Height
                    : _intersection.secondBmp.Height;
                var width = TexWidth = _intersection._firstObj == this
                    ? _intersection.firstBmp.Width
                    : _intersection.secondBmp.Width;

                var bmp = _intersection._firstObj == this
                    ? _intersection.firstBmp
                    : _intersection.secondBmp;

                var texture = new List<byte>(4 * width * height);
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        var color = bmp.GetPixel(i, j);
                        texture.Add(color.R);
                        texture.Add(color.G);
                        texture.Add(color.B);
                        texture.Add(color.A);
                    }
                }

                _texture = texture.ToArray();
                UpdateTexture = true;
                OnPropertyChanged(nameof(Intersection));
            }
        }

        public override void SetTexture()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texHandle);
            if (UpdateTexture)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int) TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int) TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Nearest);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TexWidth, TexHeight, 0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte, Texture);
                UpdateTexture = false;
            }
        }

        private byte[] _texture;
        public byte[] Texture => _texture;

        public int TexWidth { get; private set; }
        public int TexHeight { get; private set; }
        public bool IgnoreBlack { get; set; }
    }
}