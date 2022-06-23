using System;
using OpenTK.Mathematics;

namespace MikCAD.Utilities;

public static class MathM
{
    public static readonly float Eps = 0.0001f;
    public static float AbsMax(float a, float b)
    {
        return MathF.Abs(a) > MathF.Abs(b) ? a : b;
    }

    public static float Distance(ParameterizedObject a, ParameterizedObject b)
    {
        return (b.GetModelMatrix().ExtractTranslation() - a.GetModelMatrix().ExtractTranslation()).Length;
    }
    
    public static float Distance(Vector3 a, Vector3 b)
    {
        return (b - a).Length;
    }
    
    public static float DistanceSquared(Vector3 a, Vector3 b)
    {
        return (b - a).LengthSquared;
    }
}