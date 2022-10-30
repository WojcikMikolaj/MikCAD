using MikCAD.CustomControls;

namespace MikCAD.Symulacje.RigidBody;

public class RigidBody
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
        }
    }

    public bool Enabled
    {
        get;
        set;
    }

    public double CubeEdgeLength
    {
        get;
        set;
    }

    public double CubeDensity
    {
        get;
        set;
    }

    public double AngularVelocity
    {
        get;
        set;
    }

    public double IntegrationStep
    {
        get;
        set;
    }

    public bool DrawCube
    {
        get;
        set;
    }

    public bool DrawDiagonal
    {
        get;
        set;
    }

    public bool DrawPath
    {
        get;
        set;
    }

    public bool DrawGravityVector
    {
        get;
        set;
    }

    public bool DrawPlane
    {
        get;
        set;
    }

    public RigidBody()
    {
        RB = this;
    }
    
    public void SetGuiIsEnabled(bool value)
    {
        _rigidBodyControl.initialConditionsGroupBox.IsEnabled = value;
        _rigidBodyControl.visualisationGroupBox.IsEnabled = value;
    }
}