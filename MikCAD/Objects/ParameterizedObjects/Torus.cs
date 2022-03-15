using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD
{
    public class Torus : ParameterizedObject
    {
        public float posX
        {
            get => _position.X;
            set
            {
                _position.X = value;
                OnPropertyChanged(nameof(posX));
            }
        }

        public float posY
        {
            get => _position.Y;
            set
            {
                _position.Y = value;
                OnPropertyChanged(nameof(posY));
            }
        }

        public float posZ
        {
            get => _position.Z;
            set
            {
                _position.Z = value;
                OnPropertyChanged(nameof(posZ));
            }
        }

        public float rotX
        {
            get => _rotation.X;
            set
            {
                _rotation.X = value;
                OnPropertyChanged(nameof(rotX));
            }
        }

        public float rotY
        {
            get => _rotation.Y;
            set
            {
                _rotation.Y = value;
                OnPropertyChanged(nameof(rotY));
            }
        }

        public float rotZ
        {
            get => _rotation.Z;
            set
            {
                _rotation.Z = value;
                OnPropertyChanged(nameof(rotZ));
            }
        }

        public float scaleX
        {
            get => _scale.X;
            set
            {
                _scale.X = value;
                OnPropertyChanged(nameof(scaleX));
            }
        }

        public float scaleY
        {
            get => _scale.Y;
            set
            {
                _scale.Y = value;
                OnPropertyChanged(nameof(scaleY));
            }
        }

        public float scaleZ
        {
            get => _scale.Z;
            set
            {
                _scale.Z = value;
                OnPropertyChanged(nameof(scaleZ));
            }
        }

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
        
        private Vector3 _position = new Vector3();
        private Vector3 _rotation = new Vector3();
        private Vector3 _scale = new Vector3(1, 1, 1);


        private float _R;
        private float _r;
        private float _thetaStep;
        private float _phiStep;
        private int _sectorsCount;
        private int _circlesCount;

        private Point[] _vertices;
        public int VerticesCount => _vertices.Length;
        public uint[] lines;

        public Torus()
        {
            _R = 1;
            _r = 0.5f;
            _sectorsCount = 6;
            _circlesCount = 6;
            CalculateVertices();
        }

        private void CalculateVertices()
        {
            _phiStep = MathHelper.DegreesToRadians(360.0f / _sectorsCount);
            _thetaStep = MathHelper.DegreesToRadians(360.0f / _circlesCount);

            _vertices = new Point[_sectorsCount * _circlesCount];

            for (int i = 0; i < _sectorsCount; i++)
            {
                for (int j = 0; j < _circlesCount; j++)
                {
                    _vertices[i * _circlesCount + j] = new Point()
                    {
                        X = (R + r * (float) MathHelper.Cos(j * _thetaStep)) * (float) MathHelper.Cos(i * _phiStep),
                        Y = (R + r * (float) MathHelper.Cos(j * _thetaStep)) * (float) MathHelper.Sin(i * _phiStep),
                        Z = r * (float) MathHelper.Sin(j * _thetaStep)
                    };
                }
            }
        }

        public Matrix4 GetModelMatrix()
        {
            return Matrix4.CreateScale(_scale)
                   * Matrix4.CreateRotationX(MH.DegreesToRadians(_rotation[0]))
                   * Matrix4.CreateRotationY(MH.DegreesToRadians(_rotation[1]))
                   * Matrix4.CreateRotationZ(MH.DegreesToRadians(_rotation[2]))
                   * Matrix4.CreateTranslation(_position);
        }

        public void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation,
            out int _vertexBufferObject, out int _vertexArrayObject)
        {
            var vertices = new float[_vertices.Length * Point.Size];
            var colors = new float[_vertices.Length * Point.Size];

            for (int i = 0; i < _vertices.Length; i++)
            {
                vertices[Point.Size * i] = _vertices[i].X;
                vertices[Point.Size * i + 1] = _vertices[i].Y;
                vertices[Point.Size * i + 2] = _vertices[i].Z;
            }

            var vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * 3 * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);
            _vertexBufferObject = vertexBufferObject;

            var vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);
            _vertexArrayObject = vertexArrayObject;

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            var indexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
            lines = GenerateLines();
            GL.BufferData(BufferTarget.ElementArrayBuffer, lines.Length * sizeof(uint), lines,
                BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
            GL.EnableVertexAttribArray(1);
        }

        private uint[] GenerateLines()
        {
            //2 - ends of each line
            //2 - same number of big and small circles
            uint[] lines = new uint[2 * _sectorsCount * 2 * _circlesCount];
            uint it = 0;
            for (int i = 0; i < _sectorsCount; i++)
            {
                for (int j = 0; j < _circlesCount; j++)
                {
                    //duże okręgi
                    if (i != _sectorsCount - 1)
                    {
                        lines[it++] = (uint) (i * _circlesCount + j);
                        lines[it++] = (uint) ((i + 1) * _circlesCount + j);
                    }
                    else
                    {
                        lines[it++] = (uint) ((_sectorsCount - 1) * _circlesCount + j);
                        lines[it++] = (uint) j;
                    }

                    //małe okręgi
                    if (j != _circlesCount - 1)
                    {
                        lines[it++] = (uint) (i * _circlesCount + j);
                        lines[it++] = (uint) (i * _circlesCount + j + 1);
                    }
                    else
                    {
                        lines[it++] = (uint) (i * _circlesCount + _circlesCount - 1);
                        lines[it++] = (uint) (i * _circlesCount);
                    }
                }
            }

            return lines;
        }

        public float[] GetVertices()
        {
            var vertices = new float[_vertices.Length * 3];
            var colors = new float[_vertices.Length * 3];

            for (int i = 0; i < _vertices.Length; i++)
            {
                vertices[i * Point.Size] = _vertices[i].X;
                vertices[i * Point.Size + 1] = _vertices[i].Y;
                vertices[i * Point.Size + 2] = _vertices[i].Z;

                colors[i * Point.Size] = 1.0f;
                colors[i * Point.Size + 1] = 0.0f;
                colors[i * Point.Size + 2] = 0.0f;
            }

            return vertices;
        }
    }
}