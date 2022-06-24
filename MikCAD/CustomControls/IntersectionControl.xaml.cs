using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using MikCAD.BezierCurves;
using MikCAD.Objects;

namespace MikCAD;

public partial class IntersectionControl : UserControl
{
    public Intersection intersection { get; set; }
    public IntersectionControl()
    {
        InitializeComponent();
    }

    private void CalculateIntersection(object sender, RoutedEventArgs e)
    {
        //intersection.NumberOfPoints = (int) PointsNum.Value;
        intersection.Steps = StepSize.Value;
        intersection.UseCursor = UseCursor.IsChecked.Value;
        intersection.Intersect();
        (this.Parent as Window)?.Close();
    }
}