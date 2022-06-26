using System.Drawing;

namespace MikCAD.Utilities;

public class LineDrawer
{
    private static int margines = 0;

    public static bool InBox(Point a, DirectBitmap direct, int dx = 0, int dy = 0)
    {
        return a.X + dx > margines && a.X + dx < direct.Width - margines && a.Y + dy > margines &&
               a.Y + dy < direct.Height - margines;
    }

    public static void Line((int X, int Y) a, (int X, int Y) b, DirectBitmap direct, Color color)
    {
        int x1 = 0, x2 = 02, y1 = 0, y2 = 0, dx = 0, dy = 0, d = 0, incrE = 0, incrNE = 0, x = 0, y = 0, incrY = 0;
        //podział wzdłuż OY
        if (b.X < a.X)
        {
            (a, b) = (b, a);
        }

        //Te same wartości dla kazdego przypadku
        x1 = (int) a.X;
        x2 = (int) b.X;
        y1 = (int) a.Y;
        y2 = (int) b.Y;
        dx = x2 - x1;
        x = x1;
        y = y1;
        //315-360
        if (b.Y >= a.Y && b.Y - a.Y <= b.X - a.X)
        {
            dy = y2 - y1;
            d = 2 * dy - dx;
            incrE = 2 * dy;
            incrNE = 2 * (dy - dx);
            incrY = 1;
        }
        //0-45
        else if (b.Y < a.Y && a.Y - b.Y <= b.X - a.X)
        {
            dy = y1 - y2;
            d = 2 * dy - dx;
            incrE = 2 * dy;
            incrNE = 2 * (dy - dx);
            incrY = -1;
        }
        //270-315
        else if (b.Y >= a.Y)
        {
            dy = y2 - y1;
            d = 2 * dx - dy;
            incrE = 2 * dx;
            incrNE = 2 * (dx - dy);
        }
        //45-90
        else
        {
            dy = y1 - y2;
            d = 2 * dx - dy;
            incrE = 2 * dx;
            incrNE = 2 * (dx - dy);
        }

        direct.SetPixel(x, y, color);
        //315-45
        if (dx > dy)
        {
            while (x < x2)
            {
                if (d < 0)
                    //chooseE 
                {
                    d += incrE;
                    x++;
                }
                else
                    //chooseNE
                {
                    d += incrNE;
                    x++;
                    y += incrY;
                }

                direct.SetPixel(x, y, color);
            }
        }
        //270-315
        else if (y2 > y)
        {
            while (y < y2)
            {
                if (d < 0)
                    //chooseE 
                {
                    d += incrE;
                    y++;
                }
                else
                    //chooseNE
                {
                    d += incrNE;
                    y++;
                    x++;
                }

                direct.SetPixel(x, y, color);
            }
        }
        //45-90
        else
        {
            while (y > y2)
            {
                if (d < 0)
                    //chooseE 
                {
                    d += incrE;
                    y--;
                }
                else
                    //chooseNE
                {
                    d += incrNE;
                    y--;
                    x++;
                }

                direct.SetPixel(x, y, color);
            }
        }
    }
}