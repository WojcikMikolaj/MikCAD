﻿using System;
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
    
    public static float Distance(Vector4 a, Vector4 b)
    {
        return (b - a).Length;
    }
    
    public static float DistanceSquared((float u, float v) a, (float u, float v) b)
    {
        return (new Vector2(b.u, b.v) - new Vector2(a.u, a.v)).LengthSquared;
    }
    
    public static float DistanceSquared(Vector3 a, Vector3 b)
    {
        return (b - a).LengthSquared;
    }
    
    public static float DistanceSquared(Vector4 a, Vector4 b)
    {
        return (b - a).LengthSquared;
    }

    public static float Length((float x, float y, float z) A, (float x, float y, float z) B)
    {
        return MathF.Sqrt((A.x - B.x) * (A.x - B.x) + (A.y - B.y) * (A.y - B.y) + (A.z - B.z) * (A.z - B.z));
    }
}