using System.Windows;
using System.Windows.Controls;
using MikCAD.BezierCurves;

namespace MikCAD;

public partial class BezierCurveC2Control : UserControl
{
    public BezierCurveC2Control()
    {
        InitializeComponent();
    }

    private void MoveItemUp(object sender, RoutedEventArgs e)
    {
        var item = sender as Button;
        var obj = (item.Parent as Grid).DataContext as ParameterizedPoint;
        ((BezierCurveC2)Scene.CurrentScene.ObjectsController.SelectedObject).MoveUp(obj);
        MainWindow.current.bezierCurveC2Control.PointsList.Items.Refresh();
    }

    private void MoveItemDown(object sender, RoutedEventArgs e)
    {
        var item = sender as Button;
        var obj = (item.Parent as Grid).DataContext as ParameterizedPoint;
        ((BezierCurveC2)Scene.CurrentScene.ObjectsController.SelectedObject).MoveDown(obj);
        MainWindow.current.bezierCurveC2Control.PointsList.Items.Refresh();
    }

    private void DeleteItem(object sender, RoutedEventArgs e)
    {
        var item = sender as Button;
        var obj = (item.Parent as Grid).DataContext as ParameterizedPoint;
        ((BezierCurveC2)Scene.CurrentScene.ObjectsController.SelectedObject).ProcessObject(obj);
        MainWindow.current.bezierCurveC2Control.PointsList.Items.Refresh();
    }

    private void Update_BSpline_Points(object sender, RoutedEventArgs e)
    {
        ((BezierCurveC2)Scene.CurrentScene.ObjectsController.SelectedObject).ConvertBSplineToBernstein();
    }
}