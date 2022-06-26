using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace MikCAD.Utilities;

public class DirectBitmap : IDisposable
{
    public Bitmap Bitmap { get; set; }
    public Int32[] Bits { get; private set; }
    public bool Disposed { get; private set; }
    public int Height { get; private set; }
    public int Width { get; private set; }

    protected GCHandle BitsHandle { get; private set; }

    public DirectBitmap(int width, int height)
    {
        Width = width;
        Height = height;
        Bits = new Int32[width * height];
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
    }

    public void SetPixel(int x, int y, Color color)
    {
        if (x < Width && x >= 0 && y < Height && y >= 0)
        {
            int index = x + (y * Width);
            int col = color.ToArgb();
            if (index < Width * Height && index >= 0)
            {
                Bits[index] = col;
            }
        }
    }

    public Color GetPixel(int x, int y)
    {
        if (y == Height)
        {
            y = Height - 1;
        }

        if (x == Width)
        {
            x = Width - 1;
        }

        int index = x + (y * Width);
        int col = Bits[index];
        Color result = Color.FromArgb(col);

        return result;
    }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        Bitmap.Dispose();
        BitsHandle.Free();
    }
    
    public BitmapImage BitmapToImageSource()
    {
        using (MemoryStream memory = new MemoryStream())
        {
            Bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();

            return bitmapimage;
        }
    }
}