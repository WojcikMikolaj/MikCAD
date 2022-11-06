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

    #region PhysicsProperties

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

    private double _angularVelocity = 1;
    public double AngularVelocity
    {
        get => _angularVelocity;
        set
        {
            _angularVelocity = value;
            W = (0, 0, (float) _angularVelocity);
        }
    }

    public float IntegrationStep { get; set; } = 0.001f;


    #endregion

    #region DrawProperties

    public bool DrawCube { get; set; } = true;
    public bool DrawDiagonal { get; set; } = true;
    public bool DrawPath { get; set; }
    public bool DrawGravityVector { get; set; }
    public bool DrawPlane { get; set; }

    #endregion
    
    private Timer _timer = new Timer();
    
    public RigidBody()
    {
        RB = this;
        SetUpModel();
        SetUpPhysics();

        UpdateRotationMatrix();
        _timer.Tick += (sender, args) => SimulateNextStep();

        ResetPath();
    }
    
    public void SetGuiIsEnabled(bool value)
    {
        _rigidBodyControl.initialConditionsGroupBox.IsEnabled = value;
    }
    
    public void StartSimulation()
    {
        _timer.Interval = (int)(IntegrationStep * 1000);
        _timer.Enabled = true;
        _timer.Start();
        IsSimulationRunning = true;
    }

    public void StopSimulation()
    {
        _timer.Enabled = false;
        _timer.Stop();
        IsSimulationRunning = false;
    }

    public void ResetSimulation()
    {
        StopSimulation();
        ResetPhysics();
        ResetPath();
        StartSimulation();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}