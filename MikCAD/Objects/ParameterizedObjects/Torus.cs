﻿using System;
using System.Data;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD
{
    public class Torus : ParameterizedObject
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

        private Point[] _vertices;
        public int VerticesCount => _vertices.Length;

        public override uint[] lines => _lines;
        public uint[] _lines;

        public Torus(): base("Torus")
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

            _vertices = new Point[_sectorsCount * _circlesCount];

            for (int i = 0; i < _sectorsCount; i++)
            {
                for (int j = 0; j < _circlesCount; j++)
                {
                    _vertices[i * _circlesCount + j] = new Point()
                    {
                        X = (R + r * (float) MathHelper.Cos(j * _thetaStep)) * (float) MathHelper.Cos(i * _phiStep),
                        Y = r * (float) MathHelper.Sin(j * _thetaStep),
                        Z = (R + r * (float) MathHelper.Cos(j * _thetaStep)) * (float) MathHelper.Sin(i * _phiStep),
                    };
                }
            }
        }

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

            var vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            var indexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
            _lines = GenerateLines();
            GL.BufferData(BufferTarget.ElementArrayBuffer, _lines.Length * sizeof(uint), _lines,
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

        public override void PassToDrawProcessor(DrawProcessor drawProcessor,EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
        {
            drawProcessor.ProcessObject(this, eye, vertexAttributeLocation, normalAttributeLocation);
        }
    }
}