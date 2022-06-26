using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using MikCAD.BezierCurves;
using MikCAD.Objects;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

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