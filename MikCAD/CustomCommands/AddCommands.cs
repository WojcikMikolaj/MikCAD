using System.Windows.Input;

namespace MikCAD.CustomCommands;

public static class AddCommands
{
    public static RoutedCommand AddPoint = new RoutedCommand("AddPointCommand", typeof(MainWindow));
    public static RoutedCommand AddTorus = new RoutedCommand("AddTorusCommand", typeof(MainWindow));
    public static RoutedCommand AddBezierCurveC0 = new RoutedCommand("AddBezierCurveC0", typeof(MainWindow));
    public static RoutedCommand AddBezierCurveC2 = new RoutedCommand("AddBezierCurveC2", typeof(MainWindow));
    public static RoutedCommand AddInterpolatingBezierCurveC2 = new RoutedCommand("AddInterpolatingBezierCurveC2", typeof(MainWindow));
    public static RoutedCommand AddBezierSurfaceC0 = new RoutedCommand("AddBezierSurfaceC0", typeof(MainWindow));
}