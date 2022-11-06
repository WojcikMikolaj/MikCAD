using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using MikCAD.Annotations;
using MikCAD.CustomControls;
using MikCAD.Objects;
using MikCAD.Utilities;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD.Symulacje.RigidBody;

public partial class RigidBody
{
    private void SetUpModel()
    {
        _cubeVbo = GL.GenBuffer();
        _cubeVao = GL.GenVertexArray();
        _cubeIbo = GL.GenBuffer();

        _planeVbo = GL.GenBuffer();
        _planeVao = GL.GenVertexArray();
        _planeIbo = GL.GenBuffer();

        _pathVbo = GL.GenBuffer();
        _pathVao = GL.GenVertexArray();
        _pathIbo = GL.GenBuffer();
        GenerateCube(true);
        GeneratePlane();
    }

    #region PosRotScaleMatrices

    public float posX
    {
        get => _position.X;
        set
        {
            _position.X = value;
            UpdateTranslationMatrix();
            OnPropertyChanged(nameof(posX));
        }
    }

    public float posY
    {
        get => _position.Y;
        set
        {
            _position.Y = value;
            UpdateTranslationMatrix();
            OnPropertyChanged(nameof(posY));
        }
    }

    public float posZ
    {
        get => _position.Z;
        set
        {
            _position.Z = value;
            UpdateTranslationMatrix();
            OnPropertyChanged(nameof(posZ));
        }
    }

    public Vector3 pos => new Vector3(posX, posY, posZ);

    public float rotX
    {
        get => _rotation.X;
        set
        {
            _rotation.X = value;
            UpdateRotationMatrix();
            OnPropertyChanged(nameof(rotX));
        }
    }

    public float rotY
    {
        get => _rotation.Y;
        set
        {
            _rotation.Y = value;
            UpdateRotationMatrix();
            OnPropertyChanged(nameof(rotY));
        }
    }

    public float rotZ
    {
        get => _rotation.Z;
        set
        {
            _rotation.Z = value;
            UpdateRotationMatrix();
            OnPropertyChanged(nameof(rotZ));
        }
    }

    public float scaleX
    {
        get => _scale.X;
        set
        {
            _scale.X = value;
            UpdateScaleMatrix();
            OnPropertyChanged(nameof(scaleX));
        }
    }

    public float scaleY
    {
        get => _scale.Y;
        set
        {
            _scale.Y = value;
            UpdateScaleMatrix();
            OnPropertyChanged(nameof(scaleY));
        }
    }

    public float scaleZ
    {
        get => _scale.Z;
        set
        {
            _scale.Z = value;
            UpdateScaleMatrix();
            OnPropertyChanged(nameof(scaleZ));
        }
    }

    private Vector3 _position = new Vector3();

    private static readonly Vector3 InitialRotation =
        new Vector3(0, 45, (float) MH.RadiansToDegrees(MH.Atan(MH.Sqrt(2))));
    //new Vector3(0, 0, 0);

    private Vector3 _rotation = InitialRotation;
    private Vector3 _scale = new Vector3(1, 1, 1);
    private Matrix4 _rotationMatrixX = Matrix4.Identity;
    private Matrix4 _rotationMatrixY = Matrix4.Identity;
    private Matrix4 _rotationMatrixZ = Matrix4.Identity;
    private Matrix4 _modelMatrix = Matrix4.Identity;
    private Matrix4 _scaleMatrix = Matrix4.Identity;
    private Matrix4 _rigidBodyRotation = Matrix4.Identity;
    private Matrix4 _translationMatrix = Matrix4.Identity;

    public void UpdateRigidBodyRotationMatrix()
    {
        if (IsSimulationRunning)
        {
            _rigidBodyRotation = Matrix4.CreateFromQuaternion(Q);
        }

        UpdateModelMatrix();
    }

    public void UpdateModelMatrix()
    {
        _modelMatrix = _scaleMatrix * _rigidBodyRotation * _translationMatrix;
    }

    public void UpdateScaleMatrix()
    {
        _scaleMatrix = Matrix4.CreateScale(_scale);
        UpdateModelMatrix();
    }

    public void UpdateRotationMatrix()
    {
        _rotationMatrixX = Matrix4.CreateFromQuaternion(new Quaternion(MH.DegreesToRadians(_rotation[0]), 0, 0));
        _rotationMatrixY = Matrix4.CreateFromQuaternion(new Quaternion(0, MH.DegreesToRadians(_rotation[1]), 0));
        _rotationMatrixZ = Matrix4.CreateFromQuaternion(new Quaternion(0, 0, MH.DegreesToRadians(_rotation[2])));
        _rigidBodyRotation = _rotationMatrixX * _rotationMatrixY * _rotationMatrixZ;
        UpdateModelMatrix();
    }

    public void UpdateTranslationMatrix()
    {
        _translationMatrix = Matrix4.CreateTranslation(_position);
        UpdateModelMatrix();
    }

    public Matrix4 GetModelMatrix()
    {
        return _modelMatrix;
    }

    #endregion

    private TexPoint[] _vertices;
    private uint[] _vertIndices;
    public int VerticesCount => _vertices.Length;
    private float[] _verticesDraw;

    private uint PathLength = 5000;
    private uint currentVert = 1;
    private TexPoint[] _pathVertices;
    private uint[] _pathLinesIndices;
    public uint[] PathLinesIndices => _pathLinesIndices;
    private float[] _pathVerticesDraw;

    public uint[] Lines => _lines;
    private uint[] _lines;

    public uint[] TrianglesIndices => _trianglesIndices;
    private uint[] _trianglesIndices;

    public uint[] DiagonalLineIndices => _diagonalLineIndices;
    private uint[] _diagonalLineIndices;

    private int _cubeVbo;
    private int _cubeVao;
    private int _cubeIbo;

    private int _pathVbo;
    private int _pathVao;
    private int _pathIbo;

    private int _planeVbo;
    private int _planeVao;
    private int _planeIbo;


    private TexPoint[] _planeVertices;
    private uint[] _planeVertIndices;
    public int PlaneVerticesCount => _planeVertices.Length;
    private float[] _planeVerticesDraw;
    public uint[] PlaneTrianglesIndices => _planeTrianglesIndices;
    private uint[] _planeTrianglesIndices;


    private void GenerateCube(bool generateNew = false)
    {
        _vertices = new TexPoint[8];
        _vertIndices = new uint[8];
        Vector3 dirToPoint0 = (-(float) CubeEdgeLength / 2, -(float) CubeEdgeLength / 2, -(float) CubeEdgeLength / 2);
        dirToPoint0 = (0, 0, 0);
        _vertices[0] = new TexPoint(dirToPoint0 + (0, 0, 0), (0, 0));
        _vertices[1] = new TexPoint(dirToPoint0 + (0, 0, (float) CubeEdgeLength), (0, 0));
        _vertices[2] = new TexPoint(dirToPoint0 + (0, (float) CubeEdgeLength, (float) CubeEdgeLength), (0, 0));
        _vertices[3] = new TexPoint(dirToPoint0 + (0, (float) CubeEdgeLength, 0), (0, 0));
        _vertices[4] = new TexPoint(dirToPoint0 + ((float) CubeEdgeLength, 0, 0), (0, 0));
        _vertices[5] = new TexPoint(dirToPoint0 + ((float) CubeEdgeLength, 0, (float) CubeEdgeLength), (0, 0));
        _vertices[6] =
            new TexPoint(dirToPoint0 + ((float) CubeEdgeLength, (float) CubeEdgeLength, (float) CubeEdgeLength),
                (0, 0));
        _vertices[7] = new TexPoint(dirToPoint0 + ((float) CubeEdgeLength, (float) CubeEdgeLength, 0), (0, 0));

        _verticesDraw = new float[_vertices.Length * TexPoint.Size];
        for (int i = 0; i < _vertices.Length; i++)
        {
            _verticesDraw[TexPoint.Size * i] = _vertices[i].X;
            _verticesDraw[TexPoint.Size * i + 1] = _vertices[i].Y;
            _verticesDraw[TexPoint.Size * i + 2] = _vertices[i].Z;
            _verticesDraw[TexPoint.Size * i + 3] = _vertices[i].TexX;
            _verticesDraw[TexPoint.Size * i + 4] = _vertices[i].TexY;
        }

        if (generateNew)
        {
            GenerateCubeLines();
            GenerateCubeTriangles();
            GenerateDiagonalLine();
            _cubeFirstIter = true;
        }

        CalculateDiagonalizedInertiaTensor();
    }

    private void GenerateCubeLines()
    {
        _lines = new uint[]
        {
            0, 1,
            1, 2,
            2, 3,
            3, 0,

            1, 5,
            2, 6,
            3, 7,
            0, 4,

            4, 5,
            5, 6,
            6, 7,
            7, 4
        };
    }

    private void GenerateCubeTriangles()
    {
        _trianglesIndices = new uint[]
        {
            0, 1, 2,
            0, 2, 3,

            1, 5, 6,
            1, 6, 2,

            5, 4, 7,
            5, 7, 6,

            4, 0, 3,
            4, 3, 7,

            3, 2, 6,
            3, 6, 7,

            0, 1, 5,
            0, 5, 4
        };
    }

    private void GenerateDiagonalLine()
    {
        _diagonalLineIndices = new uint[]
        {
            0, 6
        };
    }

    private bool _cubeFirstIter = true;

    public void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _cubeVbo);
        if (_cubeFirstIter)
        {
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * TexPoint.Size * sizeof(float), _verticesDraw,
                BufferUsageHint.StaticDraw);
        }

        GL.BindVertexArray(_cubeVao);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float),
            3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _cubeIbo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _trianglesIndices.Length * sizeof(uint), _trianglesIndices,
            BufferUsageHint.StaticDraw);
        
        _cubeFirstIter = false;
    }


    private void ResetPath()
    {
        currentVert = 0;
        _pathVertices = new TexPoint[PathLength];
        for (uint i = 0; i < PathLength; i++)
        {
            _pathVertices[i] = new TexPoint();
        }

        _pathLinesIndices = new uint[2 * (PathLength - 1)];
        uint k = 0;
        for (uint j = 0; j < 2 * (PathLength - 1); j += 2)
        {
            _pathLinesIndices[j] = k;
            _pathLinesIndices[j + 1] = k + 1;
            k++;
        }

        _pathVerticesDraw = new float[_pathVertices.Length * TexPoint.Size];
        for (int i = 0; i < _pathVertices.Length; i++)
        {
            _pathVerticesDraw[TexPoint.Size * i] = _pathVertices[i].X;
            _pathVerticesDraw[TexPoint.Size * i + 1] = _pathVertices[i].Y;
            _pathVerticesDraw[TexPoint.Size * i + 2] = _pathVertices[i].Z;
            _pathVerticesDraw[TexPoint.Size * i + 3] = _pathVertices[i].TexX;
            _pathVerticesDraw[TexPoint.Size * i + 4] = _pathVertices[i].TexY;
        }
    }

    public void AddVertexToPath()
    {
        if (!IsSimulationRunning)
        {
            return;
        }

        var point = new Vector4((float) CubeEdgeLength, (float) CubeEdgeLength, (float) CubeEdgeLength, 1) *
                    _modelMatrix;
        _pathVertices[currentVert] = new TexPoint()
        {
            X = point.X,
            Y = point.Y,
            Z = point.Z,
            TexX = 0,
            TexY = 0
        };

        _pathVerticesDraw[TexPoint.Size * currentVert] = _pathVertices[currentVert].X;
        _pathVerticesDraw[TexPoint.Size * currentVert + 1] = _pathVertices[currentVert].Y;
        _pathVerticesDraw[TexPoint.Size * currentVert + 2] = _pathVertices[currentVert].Z;
        _pathVerticesDraw[TexPoint.Size * currentVert + 3] = _pathVertices[currentVert].TexX;
        _pathVerticesDraw[TexPoint.Size * currentVert + 4] = _pathVertices[currentVert].TexY;

        currentVert++;
        currentVert %= PathLength;
    }


    private bool _pathFirstIter = true;

    public void GeneratePathVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _pathVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _pathVertices.Length * TexPoint.Size * sizeof(float),
            _pathVerticesDraw,
            BufferUsageHint.StaticDraw);


        GL.BindVertexArray(_pathVao);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float),
            3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _pathIbo);
        if (_pathFirstIter)
        {
            GL.BufferData(BufferTarget.ElementArrayBuffer, _pathLinesIndices.Length * sizeof(uint), _pathLinesIndices,
                BufferUsageHint.StaticDraw);
        }

        _pathFirstIter = false;
    }

    public void PassToDrawProcessor(DrawProcessor drawProcessor, EyeEnum eye, uint vertexAttributeLocation,
        uint normalAttributeLocation)
    {
        drawProcessor.ProcessObject(this, eye, vertexAttributeLocation, normalAttributeLocation);
    }

    public void GenerateIndicesForDiagonal()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _cubeIbo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _diagonalLineIndices.Length * sizeof(uint), _diagonalLineIndices,
            BufferUsageHint.StaticDraw);
    }

    private void GeneratePlane()
    {
        _planeVertices = new TexPoint[4];
        _planeVertIndices = new uint[4];
        _planeVertices[0] = new TexPoint((-3, 0, 3), (0, 0));
        _planeVertices[1] = new TexPoint((3, 0, 3), (0, 0));
        _planeVertices[2] = new TexPoint((3, 0, -3), (0, 0));
        _planeVertices[3] = new TexPoint((-3, 0, -3), (0, 0));

        _planeVerticesDraw = new float[_planeVertices.Length * TexPoint.Size];
        for (int i = 0; i < _planeVertices.Length; i++)
        {
            _planeVerticesDraw[TexPoint.Size * i] = _planeVertices[i].X;
            _planeVerticesDraw[TexPoint.Size * i + 1] = _planeVertices[i].Y;
            _planeVerticesDraw[TexPoint.Size * i + 2] = _planeVertices[i].Z;
            _planeVerticesDraw[TexPoint.Size * i + 3] = _planeVertices[i].TexX;
            _planeVerticesDraw[TexPoint.Size * i + 4] = _planeVertices[i].TexY;
        }

        GeneratePlaneTriangles();
    }

    private void GeneratePlaneTriangles()
    {
        _planeTrianglesIndices = new uint[]
        {
            0, 1, 2,
            0, 2, 3,
        };
    }

    private bool _planeFirstIter = true;

    public void GeneratePlaneVerticesAndIndices()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _planeVbo);
        if (_planeFirstIter)
        {
            GL.BufferData(BufferTarget.ArrayBuffer, _planeVertices.Length * TexPoint.Size * sizeof(float),
                _planeVerticesDraw,
                BufferUsageHint.StaticDraw);
        }

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float),
            3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _planeIbo);
        if (_planeFirstIter)
        {
            GL.BufferData(BufferTarget.ElementArrayBuffer, _planeTrianglesIndices.Length * sizeof(uint),
                _planeTrianglesIndices,
                BufferUsageHint.StaticDraw);
        }

        _planeFirstIter = false;
    }

    private void GenerateGravityVector()
    {
    }


    public void GenerateGravityVectorIndices()
    {
    }
}