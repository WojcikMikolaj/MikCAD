using OpenTK.Mathematics;

namespace MikCAD.Extensions;

public static class Vector3Extender
{
    public static bool isFinite(this Vector3 vec)
    {
        return float.IsFinite(vec.X) && float.IsFinite(vec.Y) && float.IsFinite(vec.Z);
    }
}