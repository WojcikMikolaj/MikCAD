using OpenTK.Mathematics;

namespace MikCAD.Objects;

public class TexPoint
{
    public TexPoint()
    {
    }

    public TexPoint(Vector3 position, Vector2 texCoord)
    {
        X = position.X;
        Y = position.Y;
        Z = position.Z;

        TexX = texCoord.X;
        TexY = texCoord.Y;
    }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    
    public float TexX { get; set; }
    public float TexY { get; set; }

    public Vector3 XYZ => new Vector3(X, Y, Z);
    public Vector2 TexXY => new Vector2(TexX, TexY);
    
    public static int Size => 5;
}