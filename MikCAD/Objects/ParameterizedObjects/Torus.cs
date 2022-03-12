using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MikCAD
{
    public class Torus : IParameterizedObject
    {
        public float R
        {
            get => _R;
            set
            {
                _R = value;
                CalculateVertices();
            }
        }

        public float r
        {
            get => _r;
            set
            {
                _r = value;
                CalculateVertices();
            }
        }

        public float Theta
        {
            get => _theta;
            set
            {
                _theta = value;
                CalculateVertices();
            }
        }

        public float Phi
        {
            get => _phi;
            set
            {
                _phi = value;
                CalculateVertices();
            }
        }

        private Point[] _vertices;
        public int VerticesCount => _vertices.Length;
        private float _R;
        private float _r;
        private float _theta;
        private float _thetaStep;
        private float _phi;
        private float _phiStep;
        private int _circleStepsCount;
        private int _planeStepsCount;

        public uint[] lines;

        public Torus()
        {
            _R = 1;
            _r = 0.5f;
            _theta = 60;
            _phi = 60;
            CalculateVertices();
        }

        private void CalculateVertices()
        {
            _planeStepsCount = (int) (360 / _phi);
            _phiStep = MathHelper.DegreesToRadians(360.0f / _planeStepsCount);

            _circleStepsCount = (int) (360 / _theta);
            _thetaStep = MathHelper.DegreesToRadians(360.0f / _circleStepsCount);

            _vertices = new Point[_planeStepsCount * _circleStepsCount];

            for (int i = 0; i < _planeStepsCount; i++)
            {
                for (int j = 0; j < _circleStepsCount; j++)
                {
                    _vertices[i * _circleStepsCount + j] = new Point()
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
            return Matrix4.Identity;
            //return modelMatrix;
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
            uint[] lines = new uint[2* _planeStepsCount * 2 * _circleStepsCount];
            uint it = 0;
            for (int i = 0; i < _planeStepsCount; i++)
            {
                for (int j = 0; j < _circleStepsCount; j++)
                {
                    //duże okręgi
                    if (i != _planeStepsCount - 1)
                    {
                        lines[it++] = (uint) (i * _circleStepsCount + j);
                        lines[it++] = (uint) ((i + 1) * _circleStepsCount + j);
                    }
                    else
                    {
                        lines[it++] = (uint) ((_planeStepsCount - 1) * _circleStepsCount + j);
                        lines[it++] = (uint) j;
                    }

                    //małe okręgi
                    if (j != _circleStepsCount - 1)
                    {
                        lines[it++] = (uint) (i * _circleStepsCount + j);
                        lines[it++] = (uint) (i * _circleStepsCount + j + 1);
                    }
                    else
                    {
                        lines[it++] = (uint) (i * _circleStepsCount + _circleStepsCount - 1);
                        lines[it++] = (uint) (i * _circleStepsCount);
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