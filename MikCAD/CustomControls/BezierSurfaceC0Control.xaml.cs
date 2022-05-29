using System.Windows;
using System.Windows.Controls;
using MikCAD.BezierSurfaces;

namespace MikCAD;

public partial class BezierSurfaceC0Control : UserControl
{
    public BezierSurfaceC0Control()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var obj = Scene.CurrentScene.ObjectsController.SelectedObject;
        if (obj is BezierSurfaceC0 surf0)
        {
            surf0.Applied = true;
        }
        if (obj is BezierSurfaceC2 surf2)
        {
            surf2.Applied = true;
        }
    }
}