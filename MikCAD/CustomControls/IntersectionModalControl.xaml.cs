using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using MikCAD.Objects;
using MikCAD.Utilities;

namespace MikCAD.CustomControls;

public partial class IntersectionModalControl : UserControl
{
    public Intersection intersection { get; set; }

    public IntersectionModalControl()
    {
        InitializeComponent();
    }

    private void CalculateIntersection(object sender, RoutedEventArgs e)
    {
        //intersection.NumberOfPoints = (int) PointsNum.Value;
        intersection.Steps = StepSize.Value;
        intersection.UseCursor = UseCursor.IsChecked.Value;
        if (intersection.Intersect())
        {
            var firstBmp = new DirectBitmap(400, 400);
            var secondBmp = new DirectBitmap(400, 400);
            if (intersection.points.Count > 2)
            {
                var first = intersection.points[0];
                var second = intersection.points[0];

                var firstUSize = intersection._firstObj.USize;
                var firstVSize = intersection._firstObj.VSize;

                var secondUSize = intersection._secondObj.USize;
                var secondVSize = intersection._secondObj.VSize;

                LineDrawer.Line(
                    ((int) (first.u / firstUSize * firstBmp.Width), (int) (first.v / firstVSize * firstBmp.Height)),
                    ((int) (second.u / firstUSize * firstBmp.Width), (int) (second.v / firstVSize * firstBmp.Height)),
                    firstBmp,
                    Color.Lime);
                LineDrawer.Line(
                    ((int) (first.s / secondUSize * firstBmp.Width), (int) (first.t / secondVSize * firstBmp.Height)),
                    ((int) (second.s / secondUSize * firstBmp.Width), (int) (second.t / secondVSize * firstBmp.Height)),
                    secondBmp,
                    Color.Lime);

                for (int i = 2; i < intersection.points.Count; i++)
                {
                    first = second;
                    second = intersection.points[i];
                    LineDrawer.Line(
                        ((int) (first.u / firstUSize * firstBmp.Width), (int) (first.v / firstVSize * firstBmp.Height)),
                        ((int) (second.u / firstUSize * firstBmp.Width), (int) (second.v / firstVSize * firstBmp.Height)),
                        firstBmp,
                        Color.Lime);
                    LineDrawer.Line(
                        ((int) (first.s / secondUSize * firstBmp.Width), (int) (first.t / secondVSize * firstBmp.Height)),
                        ((int) (second.s/ secondUSize * firstBmp.Width), (int) (second.t/ secondVSize * firstBmp.Height)),
                        secondBmp,
                        Color.Lime);
                }

                if (intersection.Looped)
                {
                    first = intersection.points[^1];
                    second = intersection.points[0];
                    LineDrawer.Line(
                        ((int) (first.s / secondUSize * firstBmp.Width), (int) (first.t / secondVSize * firstBmp.Height)),
                        ((int) (second.s/ secondUSize * firstBmp.Width), (int) (second.t/ secondVSize * firstBmp.Height)),
                        secondBmp,
                        Color.Lime);
                }
            }

            FillingToolset.FloodFill(firstBmp, Color.Lime, Color.Gray);
            FillingToolset.FloodFill(secondBmp, Color.Lime, Color.Gray);
            
            firstBmp.Bitmap.Save("firstBitmap.bmp");
            secondBmp.Bitmap.Save("secondBitmap.bmp");
            MainWindow.current.firstImage.Source = firstBmp.BitmapToImageSource();
            MainWindow.current.secondImage.Source = secondBmp.BitmapToImageSource();
        }

        (Parent as Window)?.Close();
    }
}