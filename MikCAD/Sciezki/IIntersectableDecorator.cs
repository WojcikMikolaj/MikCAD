﻿using MikCAD.BezierSurfaces;
using MikCAD.Objects;
using OpenTK.Mathematics;

namespace MikCAD.Sciezki;

public class IntersectableDecorator : IIntersectable
{
    public IIntersectable Intersectable { get; }

    public float DistanceFromSurface { get; set; }
    public float Delta { get; set; } = 0.001f;

    public IntersectableDecorator(BezierSurfaceC2 surfaceC2)
    {
        Intersectable = surfaceC2;
    }

    public Vector3 GetValueAt(float u, float v)
    {
        (var pos, var dU, var dV) = Intersectable.GetPositionAndGradient(u, v);
        var normal = Vector3.Cross(dU.Normalized(), dV.Normalized()).Normalized();
        return pos + DistanceFromSurface * (-normal);
    }

    public Vector3 GetUDerivativeAt(float u, float v)
    {
        (_, var dU_m_du, _) = Intersectable.GetPositionAndGradient(u - Delta, v);
        (_, var dU_p_du, _) = Intersectable.GetPositionAndGradient(u + Delta, v);
        return Intersectable.GetUDerivativeAt(u, v);
    }

    public Vector3 GetVDerivativeAt(float u, float v)
    {
        (_, _, var dV_m_dv) = Intersectable.GetPositionAndGradient(u, v - Delta);
        (_, _, var dV_p_dv) = Intersectable.GetPositionAndGradient(u, v + Delta);
        return Intersectable.GetVDerivativeAt(u, v);
    }

    public (Vector3 pos, Vector3 dU, Vector3 dV) GetPositionAndGradient(float u, float v)
    {
        (_, var dU_m_du, var dV_m_du) = Intersectable.GetPositionAndGradient(u - Delta, v);
        (_, var dU_m_dv, var dV_m_dv) = Intersectable.GetPositionAndGradient(u, v - Delta);

        (_, var dU_p_du, var dV_p_du) = Intersectable.GetPositionAndGradient(u + Delta, v);
        (_, var dU_p_dv, var dV_p_dv) = Intersectable.GetPositionAndGradient(u, v + Delta);

        (var pos, var dU, var dV) = Intersectable.GetPositionAndGradient(u, v);

        var normal = Vector3.Cross(dU.Normalized(), dV.Normalized()).Normalized();
        return (pos * DistanceFromSurface * normal, dU, dV);
    }

    public bool IsUWrapped => Intersectable.IsUWrapped;
    public bool IsVWrapped => Intersectable.IsVWrapped;
    public float USize => Intersectable.USize;
    public float VSize => Intersectable.VSize;
    public Intersection Intersection { get; set; }
    public bool IgnoreBlack { get; set; }
}