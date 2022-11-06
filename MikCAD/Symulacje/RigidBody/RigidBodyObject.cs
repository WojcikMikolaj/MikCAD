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
    #region UserInteractionsVariablesAndProperties

    public static RigidBody RB;
    public RigidBodyControl _rigidBodyControl;

    private bool _isSimulationRunning = false;

    public bool IsSimulationRunning
    {
        get => _isSimulationRunning;
        set
        {
            _isSimulationRunning = value;
            SetGuiIsEnabled(!_isSimulationRunning);
            OnPropertyChanged();
        }
    }

    public bool Enabled { get; set; }

    private double _cubeEdgeLength = 1;

    public double CubeEdgeLength
    {
        get => _cubeEdgeLength;
        set
        {
            _cubeEdgeLength = value;
            GenerateCube();
            OnPropertyChanged();
        }
    }

    public double CubeDensity { get; set; } = 1;

    private double _cubeDeviation = 0;

    public double CubeDeviation
    {
        get => _cubeDeviation;
        set
        {
            _cubeDeviation = value;
            var rot = InitialRotation;
            rot.Z -= (float) _cubeDeviation;
            _rotation = rot;
            UpdateRotationMatrix();
        }
    }

    public double AngularVelocity { get; set; } = 1;

    public float IntegrationStep { get; set; } = 0.001f;

    public bool DrawCube { get; set; } = true;

    public bool DrawDiagonal { get; set; } = true;

    public bool DrawPath { get; set; }

    public bool DrawGravityVector { get; set; }

    public bool DrawPlane { get; set; }

    #endregion


    private Timer _timer = new Timer();
    
    private void SetUpModel()
    {
        _vbo = GL.GenBuffer();
        _vao = GL.GenVertexArray();
        _ibo = GL.GenBuffer();
        GenerateCube(true);
    }

    public void SetGuiIsEnabled(bool value)
    {
        _rigidBodyControl.initialConditionsGroupBox.IsEnabled = value;
        _rigidBodyControl.visualisationGroupBox.IsEnabled = value;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    private Vector3 _rotation = InitialRotation;
    private Vector3 _scale = new Vector3(1, 1, 1);

    private Matrix4 _modelMatrix = Matrix4.Identity;
    private Matrix4 _scaleMatrix = Matrix4.Identity;
    private Matrix4 _rotationMatrixX = Matrix4.Identity;
    private Matrix4 _rotationMatrixY = Matrix4.Identity;
    private Matrix4 _rotationMatrixZ = Matrix4.Identity;
    private Matrix4 _rigidBodyRotation = Matrix4.Identity;
    private Matrix4 _translationMatrix = Matrix4.Identity;

    public void UpdateRigidBodyRotationMatrix()
    {
        _rigidBodyRotation = Matrix4.CreateFromQuaternion(Q);
        UpdateModelMatrix();
    }
    
    public void UpdateModelMatrix()
    {
        _modelMatrix = _scaleMatrix * _rotationMatrixX * _rotationMatrixY * _rotationMatrixZ * _rigidBodyRotation * _translationMatrix;
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

    private int _vbo;
    private int _vao;
    private int _ibo;

    public Vector3 DiagonalizedInertiaTensor { get; private set; }
    public Vector3 InversedDaigonalizedInertiaTensor { get; private set; }

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
        }

        CalculateInertiaTensor();
    }

    private void CalculateInertiaTensor()
    {
        var faceLength5 = Math.Pow(_cubeEdgeLength, 5);

        var I1 = (float) (11.0f / 12 * faceLength5 * CubeDensity); //wektor własny {-1,0,1}
        var I2 = (float) (11.0f / 12 * faceLength5 * CubeDensity); //wektor własny {-1,1,0} 
        var I3 = (float) (faceLength5 * CubeDensity / 6.0f); //wektor własny {1,1,1} - nasza przekątna

        DiagonalizedInertiaTensor = (I1, I2, I3);
        InversedDaigonalizedInertiaTensor = (1.0f / I1, 1.0f / I2, 1.0f / I3);
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

    public void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
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

    
    private void ResetPath()
    {
        currentVert = 0;
        _pathVertices = new TexPoint[PathLength];
        for (uint i = 0; i < PathLength; i++)
        {
            _pathVertices[i] = new TexPoint();
        }
        _pathLinesIndices = new uint[2*(PathLength-1)];
        uint k = 0;
        for (uint j = 0; j < 2*(PathLength-1); j+=2)
        {
            _pathLinesIndices[j] = k;
            _pathLinesIndices[j+1] = k+1;
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

        var point =  new Vector4(1, 1, 1,1) * _modelMatrix ;
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
    

    public void GeneratePathVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _pathVertices.Length * TexPoint.Size * sizeof(float), _pathVerticesDraw,
            BufferUsageHint.StaticDraw);

        GL.BindVertexArray(_vao);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, TexPoint.Size * sizeof(float),
            3 * sizeof(float));
        GL.EnableVertexAttribArray(1);


        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _pathLinesIndices.Length * sizeof(uint), _pathLinesIndices,
            BufferUsageHint.StaticDraw);
    }
    
    public void PassToDrawProcessor(DrawProcessor drawProcessor, EyeEnum eye, uint vertexAttributeLocation,
        uint normalAttributeLocation)
    {
        drawProcessor.ProcessObject(this, eye, vertexAttributeLocation, normalAttributeLocation);
    }

    public void GenerateIndicesForDiagonal()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _diagonalLineIndices.Length * sizeof(uint), _diagonalLineIndices,
            BufferUsageHint.StaticDraw);
    }

    public void GeneratePlaneIndices()
    {
    }

    public void GenerateGravityVectorIndices()
    {
    }

    // ReSharper disable once InconsistentNaming
    private Quaternion Q, Q_t, Q1, Q_t1;

    // ReSharper disable once InconsistentNaming
    private Vector3 W, W_t, W1, W_t1; //prędkość kątowa
    private float t = 0;

    // ReSharper disable once InconsistentNaming
    private readonly Matrix3 ChangeBaseMatrixT0 = new Matrix3(
        (-1.0f / 3.0f, -1.0f / 3.0f, 2.0f / 3.0f),
        (-1.0f / 3.0f, 2.0f / 3.0f, -1.0f / 3.0f),
        (1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f));

    // ReSharper disable once InconsistentNaming
    private readonly Vector3 GravityVector = new Vector3(0, 0, -9.807f);

    // ReSharper disable once InconsistentNaming
    private Vector3 StartingGravityVectorInChangedBase;
    private Vector3 GravityTorque;

    private Vector3 CalculateTorque(Quaternion Q)
    {
        return Q * StartingGravityVectorInChangedBase;
    }

    private Vector3 f(Quaternion Q, Vector3 W)
    {
        return Vector3.Multiply(InversedDaigonalizedInertiaTensor,
            (CalculateTorque(Q) + Vector3.Cross(Vector3.Multiply(DiagonalizedInertiaTensor, W), W)));
    }

    private Quaternion g(Quaternion Q, Vector3 W)
    {
        Matrix4x3 qMatrix4x3 = new Matrix4x3((-Q.X, -Q.Y, -Q.Z), (Q.W, -Q.Z, Q.Y), (Q.Z, Q.W, -Q.X), (-Q.Y, Q.X, Q.W));

        return new Quaternion(
            w: qMatrix4x3[0, 0] * W[0] + qMatrix4x3[0, 1] * W[1] + qMatrix4x3[0, 2] * W[2],
            x: qMatrix4x3[1, 0] * W[0] + qMatrix4x3[1, 1] * W[1] + qMatrix4x3[1, 2] * W[2],
            y: qMatrix4x3[2, 0] * W[0] + qMatrix4x3[2, 1] * W[1] + qMatrix4x3[2, 2] * W[2],
            z: qMatrix4x3[3, 0] * W[0] + qMatrix4x3[3, 1] * W[1] + qMatrix4x3[3, 2] * W[2]
        );
    }


    public void SimulateNextStep()
    {
        /*
         Jak to policzyć?
         funkcja1  f(t)=I^(-1)(N+(IW)*W)
         funkcja2  g(t)=QxW/2
        //Z poprzedniego kroku
        W(t), Wt(t)
        Q(t), Qt(t)
        //Inne
        I - stałe
        N - liczymy na bieżąco*/


        var f_k1 = f(Q, W);
        var g_k1 = g(Q, W);

        var f_k2 = f(Q + IntegrationStep / 2.0f * g_k1, W + IntegrationStep * f_k1 / 2.0f);
        var g_k2 = g(Q + IntegrationStep / 2.0f * g_k1, W + IntegrationStep * f_k1 / 2.0f);

        var f_k3 = f(Q + IntegrationStep / 2.0f * g_k2, W + IntegrationStep * f_k2 / 2.0f);
        var g_k3 = g(Q + IntegrationStep / 2.0f * g_k2, W + IntegrationStep * f_k2 / 2.0f);

        var f_k4 = f(Q + IntegrationStep / 2.0f * g_k2, W + IntegrationStep * f_k3);
        var g_k4 = g(Q + IntegrationStep / 2.0f * g_k2, W + IntegrationStep * f_k3);

        W1 = W + 1.0f / 6.0f * (f_k1 + 2 * f_k2 + 2 * f_k3 + f_k4) * IntegrationStep;
        Q1 = Q + 1.0f / 6.0f * (g_k1 + 2 * g_k2 + 2 * g_k3 + g_k4) * IntegrationStep;
        //W_t1 = ;


        Q1.Normalize();

        W = W1;
        Q = Q1;
        /*
         Jak to narysować?
         Q(t+delta) - nasza obecna rotacja w układzie bączka
         teraz chcemy to za pomocą "macierz" B obrócić do układu sceny i zastosować na startowym obiekcie
         */

        //Główne pytanie, jak policzyć B?
        //B to nasz kwaternion
    }

    public void StartSimulation()
    {
        W = (0, 0, (float)AngularVelocity);
        Q = Quaternion.FromAxisAngle((0,0,1),0);
        
        
        _timer.Interval = (int)(IntegrationStep * 1000);
        _timer.Enabled = true;
        _timer.Start();
    }

    public void StopSimulation()
    {
        _timer.Enabled = false;
        _timer.Stop();
    }

    public void ResetSimulation()
    {
        _timer.Stop();
        _rigidBodyRotation = Matrix4.Identity;
        ResetPath();
        StartSimulation();
    }
}