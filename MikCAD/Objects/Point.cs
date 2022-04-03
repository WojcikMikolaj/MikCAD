using OpenTK.Mathematics;

namespace MikCAD
{
    public class Point
    {
        public Point()
        {
            
        }
        public Point(Vector3 position)
        {
            X = position.X;
            Y = position.Y;
            Z = position.Z;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3 XYZ => new Vector3(X, Y, Z);
        public static int Size => 3;
    }
}