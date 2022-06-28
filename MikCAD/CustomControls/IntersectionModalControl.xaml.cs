using System;
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
            var firstBmp = new DirectBitmap(Constants.TextureWidth, Constants.TextureHeight);
            var secondBmp = new DirectBitmap(Constants.TextureWidth, Constants.TextureHeight);
            if (intersection.points.Count > 2)
            {
                var first = intersection.points[0];
                var second = intersection.points[0];

                var firstUSize = intersection._firstObj.USize;
                var firstVSize = intersection._firstObj.VSize;

                var secondUSize = intersection._secondObj.USize;
                var secondVSize = intersection._secondObj.VSize;

                DrawLine(first.u, second.u, first.v, second.v, firstUSize, firstVSize, intersection._firstObj.IsUWrapped, intersection._firstObj.IsVWrapped, firstBmp);
                DrawLine(first.s, second.s, first.t, second.t, secondUSize, secondVSize, intersection._secondObj.IsUWrapped, intersection._secondObj.IsVWrapped, secondBmp);
                

                for (int i = 2; i < intersection.points.Count; i++)
                {
                    first = second;
                    second = intersection.points[i];

                    DrawLine(first.u, second.u, first.v, second.v, firstUSize, firstVSize, intersection._firstObj.IsUWrapped, intersection._firstObj.IsVWrapped, firstBmp);
                    DrawLine(first.s, second.s, first.t, second.t, secondUSize, secondVSize, intersection._secondObj.IsUWrapped, intersection._secondObj.IsVWrapped, secondBmp);
                }

                if (intersection.Looped)
                {
                    first = intersection.points[^1];
                    second = intersection.points[0];
                    
                    DrawLine(first.u, second.u, first.v, second.v, firstUSize, firstVSize, intersection._firstObj.IsUWrapped, intersection._firstObj.IsVWrapped, firstBmp);
                    DrawLine(first.s, second.s, first.t, second.t, secondUSize, secondVSize, intersection._secondObj.IsUWrapped, intersection._secondObj.IsVWrapped, secondBmp);
                }
            }

            FillingToolset.FloodFill(firstBmp, Color.Lime, Color.Gray, intersection._firstObj.IsUWrapped,
                intersection._firstObj.IsVWrapped);
            FillingToolset.FloodFill(secondBmp, Color.Lime, Color.Gray, intersection._secondObj.IsUWrapped,
                intersection._secondObj.IsVWrapped);

            firstBmp.Bitmap.Save("firstBitmap.bmp");
            secondBmp.Bitmap.Save("secondBitmap.bmp");

            intersection.firstBmp = firstBmp;
            intersection.secondBmp = secondBmp;

            MainWindow.current.firstImage.Source = intersection.firstBmp.BitmapToImageSource();
            MainWindow.current.secondImage.Source = intersection.secondBmp.BitmapToImageSource();

            intersection._firstObj.Intersection = intersection;
            intersection._secondObj.Intersection = intersection;
        }

        (Parent as Window)?.Close();
    }

    void DrawLine(float u1, float u2, float v1, float v2, float USize, float VSize, bool IsUWrapped, bool IsVWrapped,
        DirectBitmap bmp)
    {
        bool wrapX = false, wrapY = false;

        if (MathF.Abs(u1 - u2) > USize / 2 && IsUWrapped)
        {
            wrapX = true;
        }

        if (MathF.Abs(v1 - v2) > VSize / 2 && IsVWrapped)
        {
            wrapY = true;
        }

        if (wrapX)
        {
            float secondUp, firstUp;
            if (u1 > USize / 2)
            {
                secondUp = Math.Clamp(u2 + USize, 0, USize);
                firstUp = Math.Clamp(u1 - USize, 0, USize);
            }
            else
            {
                secondUp = Math.Clamp(u2 - USize, 0, USize);
                firstUp = Math.Clamp(u1 + USize, 0, USize);
            }

            LineDrawer.Line(
                ((int) (u1 / USize * bmp.Width),
                    (int) (v1 / VSize * bmp.Height)),
                ((int) (secondUp / USize * bmp.Width),
                    (int) (v2 / VSize * bmp.Height)),
                bmp,
                Color.Lime);

            LineDrawer.Line(
                ((int) (firstUp / USize * bmp.Width),
                    (int) (v1 / VSize * bmp.Height)),
                ((int) (u2 / USize * bmp.Width),
                    (int) (v2 / VSize * bmp.Height)),
                bmp,
                Color.Lime);
        }

        if (wrapY)
        {
            float secondVp, firstVp;
            if (v1 > VSize / 2)
            {
                secondVp = Math.Clamp(v2 + VSize, 0, VSize);
                firstVp = Math.Clamp(v1 - VSize, 0, VSize);
            }
            else
            {
                secondVp = Math.Clamp(v2 + -VSize, 0, VSize);
                firstVp = Math.Clamp(v1 + VSize, 0, VSize);
            }

            LineDrawer.Line(
                ((int) (u1 / USize * bmp.Width),
                    (int) (v1 / VSize * bmp.Height)),
                ((int) (u2 / USize * bmp.Width),
                    (int) (secondVp / VSize * bmp.Height)),
                bmp,
                Color.Lime);

            LineDrawer.Line(
                ((int) (u1 / USize * bmp.Width),
                    (int) (firstVp / VSize * bmp.Height)),
                ((int) (u2 / USize * bmp.Width),
                    (int) (v2 / VSize * bmp.Height)),
                bmp,
                Color.Lime);
        }

        if (!wrapX && !wrapY)
        {
            LineDrawer.Line(
                ((int) (u1 / USize * bmp.Width),
                    (int) (v1 / VSize * bmp.Height)),
                ((int) (u2 / USize * bmp.Width),
                    (int) (v2 / VSize * bmp.Height)),
                bmp,
                Color.Lime);
        }
    }
}