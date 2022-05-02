using System;
using OpenTK.Mathematics;

namespace MikCAD.Utilities;

public static class MathM
{
    public static float AbsMax(float a, float b)
    {
        return MathF.Abs(a) > MathF.Abs(b) ? a : b;
    }

    public static float Distance(ParameterizedObject a, ParameterizedObject b)
    {
        return (b.GetModelMatrix().ExtractTranslation() - a.GetModelMatrix().ExtractTranslation()).Length;
    }
}