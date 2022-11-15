using MikCAD.BezierSurfaces;
using MikCAD.Objects;
using OpenTK.Mathematics;

namespace MikCAD.Sciezki;

// ReSharper disable once InconsistentNaming
public class IIntersectableDecoratorStage3 : IIntersectable
{
    public IIntersectable Intersectable { get; }

    public float DistanceFromSurface { get; set; }
    public float Delta { get; set; } = 0.001f;

    public IIntersectableDecoratorStage3(BezierSurfaceC2 surfaceC2)
    {
        Intersectable = surfaceC2;
    }

    public Vector3 GetValueAt(float u, float v)
    {
        (var pos, var dU, var dV) = Intersectable.GetPositionAndGradient(u, v);
        var normal = Vector3.Cross(dU.Normalized(), dV.Normalized()).Normalized();
        if (float.IsNaN(normal.X)
            || float.IsInfinity(normal.X)
            || float.IsNaN(normal.Y)
            || float.IsInfinity(normal.Y)
            || float.IsNaN(normal.Z)
            || float.IsInfinity(normal.Z))
        {
            normal = Vector3.Zero;
        }
        return pos + DistanceFromSurface * (-normal);
    }

    public Vector3 GetUDerivativeAt(float u, float v)
    {
        var pos = GetValueAt(u, v);
        var posUDelta = GetValueAt(u + Delta, v);
        return (posUDelta - pos) / (Delta * USize);
    }

    public Vector3 GetVDerivativeAt(float u, float v)
    {
        var pos = GetValueAt(u, v);
        var posVDelta = GetValueAt(u, v + Delta);
        return (posVDelta - pos) / (Delta * VSize);
    }

    public (Vector3 pos, Vector3 dU, Vector3 dV) GetPositionAndGradient(float u, float v)
    {
        var pos = GetValueAt(u, v);
        var posUDelta = GetValueAt(u + Delta, v);
        var posVDelta = GetValueAt(u, v + Delta);

        return (pos, (posUDelta - pos) / (Delta * USize), (posVDelta - pos) / (Delta * VSize));
    }

    public bool IsUWrapped => Intersectable.IsUWrapped;
    public bool IsVWrapped => Intersectable.IsVWrapped;
    public float USize => Intersectable.USize;
    public float VSize => Intersectable.VSize;
    public Intersection Intersection { get; set; }
    public bool IgnoreBlack { get; set; }

    public Vector3 GetNormal(float u, float v)
    {
        (var pos, var dU, var dV) = Intersectable.GetPositionAndGradient(u, v);

        var normal = Vector3.Cross(dU.Normalized(), dV.Normalized()).Normalized();
        if (float.IsNaN(normal.X)
            || float.IsInfinity(normal.X)
            || float.IsNaN(normal.Y)
            || float.IsInfinity(normal.Y)
            || float.IsNaN(normal.Z)
            || float.IsInfinity(normal.Z))
        {
            normal = Vector3.Zero;
        }

        //return (pos + DistanceFromSurface * (-normal), GetUDerivativeAt(u,v), GetVDerivativeAt(u,v));
        return -normal;
    }
}