using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace MikCAD.Utilities;

public class FillingToolset
{
    public static void FloodFill(DirectBitmap directBmp, Color lineColor, Color fillingColor)
    {
        (int x, int y) startingPoint = (1, 1);
        if (directBmp.GetPixel(startingPoint.x, startingPoint.y) == lineColor)
        {
            for (int i = 1; i < directBmp.Width; i++)
            {
                for (int j = 1; j < directBmp.Height; j++)
                {
                    if (directBmp.GetPixel(i, j) != lineColor)
                    {
                        startingPoint = (i, j);
                        goto startFloodFill;
                    }
                }
            }
        }

        startFloodFill:
        Stack<(int x, int y)> pointsToFill = new Stack<(int x, int y)>();
        pointsToFill.Push(startingPoint);

        while (pointsToFill.Count > 0)
        {
            var point = pointsToFill.Pop();
            directBmp.SetPixel(point.x, point.y, fillingColor);
            
            TryAdd((point.x - 1, point.y), directBmp, pointsToFill, lineColor, fillingColor);
            TryAdd((point.x + 1, point.y), directBmp, pointsToFill, lineColor, fillingColor);
            TryAdd((point.x, point.y - 1), directBmp, pointsToFill, lineColor, fillingColor);
            TryAdd((point.x, point.y + 1), directBmp, pointsToFill, lineColor, fillingColor);
        }
    }

    private static void TryAdd((int x, int y) point, DirectBitmap directBmp, Stack<(int x, int y)> pointsToFill, Color lineColor, Color fillingColor)
    {
        if(point.x<0 || point.x >= directBmp.Width)
            return;
        if(point.y<0 || point.y >= directBmp.Height)
            return;
        var color = directBmp.GetPixel(point.x, point.y);
        if(color.ToArgb()!=lineColor.ToArgb() && color.ToArgb() !=fillingColor.ToArgb())
        {
            pointsToFill.Push(point);
        }
    }
}