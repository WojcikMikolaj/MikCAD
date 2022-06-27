using System.Windows;
using System.Windows.Controls;
using MikCAD.BezierCurves;

namespace MikCAD.CustomControls;

public partial class InterpolatingBezierCurveC2Control : UserControl
{
    public InterpolatingBezierCurveC2Control()
    {
        InitializeComponent();
    }
    
    private void MoveItemUp(object sender, RoutedEventArgs e)
    {
        var item = sender as Button;
        var obj = (item.Parent as Grid).DataContext as ParameterizedPoint;
        ((InterpolatingBezierCurveC2)Scene.CurrentScene.ObjectsController.SelectedObject).MoveUp(obj);
        MainWindow.current.interpolatingBezierCurveC2Control.PointsList.Items.Refresh();
    }

    private void MoveItemDown(object sender, RoutedEventArgs e)
    {
        var item = sender as Button;
        var obj = (item.Parent as Grid).DataContext as ParameterizedPoint;
        ((InterpolatingBezierCurveC2)Scene.CurrentScene.ObjectsController.SelectedObject).MoveDown(obj);
        MainWindow.current.interpolatingBezierCurveC2Control.PointsList.Items.Refresh();
    }

    private void DeleteItem(object sender, RoutedEventArgs e)
    {
        var item = sender as Button;
        var obj = (item.Parent as Grid).DataContext as ParameterizedPoint;
        ((InterpolatingBezierCurveC2)Scene.CurrentScene.ObjectsController.SelectedObject).ProcessObject(obj);
        MainWindow.current.interpolatingBezierCurveC2Control.PointsList.Items.Refresh();
    }
}