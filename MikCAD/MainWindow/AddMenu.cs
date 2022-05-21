using System.Windows;
using MikCAD.BezierCurves;
using MikCAD.BezierSurfaces;

namespace MikCAD;

public partial class MainWindow 
{
    private void AddTorus(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new Torus());
    }

    private void AddPoint(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint());
    }
        
    private void AddBezierCurveC0(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new BezierCurveC0());
        bezierCurveC0Control.PointsList.Items.Refresh();
    }

    private void AddBezierCurveC2(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new BezierCurveC2());
        bezierCurveC2Control.PointsList.Items.Refresh();
    }
    
    private void AddBezierSurfaceC0(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new BezierSurfaceC0());
    }

    private void AddBezierSurfaceC2(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new BezierSurfaceC2());
    }

    private void AddInterpolatingBezierCurveC2(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new InterpolatingBezierCurveC2());
        interpolatingBezierCurveC2Control.PointsList.Items.Refresh();
    }
}