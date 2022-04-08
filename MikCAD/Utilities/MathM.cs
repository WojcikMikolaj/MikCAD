using System;

namespace MikCAD.Utilities;

public static class MathM
{
    public static float AbsMax(float a, float b)
    {
        return MathF.Abs(a) > MathF.Abs(b) ? a : b;
    }
}