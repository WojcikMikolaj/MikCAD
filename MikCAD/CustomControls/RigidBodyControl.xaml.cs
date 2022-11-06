using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MikCAD.Symulacje.RigidBody;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.Forms.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace MikCAD.CustomControls;

public partial class RigidBodyControl : UserControl
{
    public RigidBodyControl()
    {
        InitializeComponent();
        
    }

    private void StartSimulation(object sender, RoutedEventArgs e)
    {
        RigidBody.RB._rigidBodyControl ??= this;
        RigidBody.RB.IsSimulationRunning = true;
        RigidBody.RB.StartSimulation();
    }

    private void StopSimulation(object sender, RoutedEventArgs e)
    {
        RigidBody.RB._rigidBodyControl ??= this;
        RigidBody.RB.IsSimulationRunning = false;
        RigidBody.RB.PauseSimulation();
    }

    private void ResetSimulation(object sender, RoutedEventArgs e)
    {
        RigidBody.RB._rigidBodyControl ??= this;
        RigidBody.RB.ResetSimulation();
    }
}