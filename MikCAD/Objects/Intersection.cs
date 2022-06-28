using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MikCAD.Annotations;
using MikCAD.BezierCurves;
using MikCAD.Objects.ParameterizedObjects;
using MikCAD.Utilities;
using OpenTK.Mathematics;

namespace MikCAD.Objects;

public class Intersection : INotifyPropertyChanged
{
    public Intersection(IIntersectable first, IIntersectable second)
    {
        this._firstObj = first;
        this._secondObj = second;

        if (first == second)
            _selfIntersection = true;
    }

    private readonly bool _selfIntersection;

    public ParameterizedObject FirstObject => (ParameterizedObject) _firstObj;
    internal IIntersectable _firstObj;

    public ParameterizedObject SecondObject => (ParameterizedObject) _secondObj;
    internal IIntersectable _secondObj;

    public bool UseCursor { get; set; }

    public float StartingGradientStepLength { get; set; } = 0.1f;
    public float GradientEps { get; set; } = 0.001f;

    public float PointsDist { get; set; } = 0.05f;
    public float NewtonEps { get; set; } = 0.001f;
    public int NewtonMaxIterations { get; set; } = 5000;
    public float MinDistParameterSpace { get; set; } = 0.1f;

    public List<IntersectionPoint> points;

    private bool _looped = false;
    public DirectBitmap secondBmp;
    public DirectBitmap firstBmp;
    public bool Looped => _looped;

    public bool Intersect()
    {
        _looped = false;

        // var startingPointsFirst = _firstObj.GetStartingPoints();
        // var startingPointsSecond = _secondObj.GetStartingPoints();
        
        var startingPointsFirst = _firstObj.GetRandomStartingPoints();
        var startingPointsSecond = _secondObj.GetRandomStartingPoints();
        
        var closestPoints = FindClosestPoints(startingPointsFirst, startingPointsSecond, _selfIntersection);

        // foreach (var p in startingPointsSecond)
        // {
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint("p")
        //     {
        //         posX = p.pos.X,
        //         posY = p.pos.Y,
        //         posZ = p.pos.Z,
        //     });    
        // }

        Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint("first")
        {
            posX = closestPoints.first.pos.X,
            posY = closestPoints.first.pos.Y,
            posZ = closestPoints.first.pos.Z,
        });

        Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint("second")
        {
            posX = closestPoints.second.pos.X,
            posY = closestPoints.second.pos.Y,
            posZ = closestPoints.second.pos.Z,
        });


        var firstIntersectionPoint = FindFirstIntersectionPoint(closestPoints);
        if (MathM.DistanceSquared(
                _firstObj.GetValueAt(firstIntersectionPoint.u, firstIntersectionPoint.v),
                _secondObj.GetValueAt(firstIntersectionPoint.s, firstIntersectionPoint.t)) > GradientEps * 10)
        {
            return false;
        }

        //Newton
        var points = FindOtherPoints(firstIntersectionPoint);

        this.points = points;
        return true;
    }

    public class IntersectionPoint
    {
        public Vector3 pos { get; set; }
        public float u { get; set; }
        public float v { get; set; }
        public float s { get; set; }
        public float t { get; set; }

        public (float u, float v, float s, float t) parameters => (u, v, s, t);
    }


    private List<IntersectionPoint> FindOtherPoints(IntersectionPoint firstIntersectionPoint)
    {
        var lastPoint = firstIntersectionPoint;
        var finalPoints = new LinkedList<IntersectionPoint>();

        //finalPoints.AddFirst(firstIntersectionPoint);

        while (true)
        {
            lastPoint = FindNextPoint(lastPoint);
            if (lastPoint == null)
                break;
            finalPoints.AddLast(lastPoint);

            if (HandleWrapping(lastPoint))
                break;
            if (MathM.Distance(lastPoint.pos, firstIntersectionPoint.pos) < PointsDist * 2 / 3)
            {
                _looped = true;
                break;
            }
        }

        lastPoint = firstIntersectionPoint;
        while (!_looped)
        {
            lastPoint = FindNextPoint(lastPoint, false);
            if (lastPoint == null)
                break;
            finalPoints.AddFirst(lastPoint);

            if (HandleWrapping(lastPoint))
                break;
            if (MathM.Distance(lastPoint.pos, firstIntersectionPoint.pos) < PointsDist / 2)
                break;
        }

        if (_looped)
            finalPoints.AddFirst(finalPoints.Last.Value);
        return finalPoints.ToList();
    }

    private bool HandleWrapping(IntersectionPoint lastPoint)
    {
        if (lastPoint.u > _firstObj.USize && _firstObj.IsUWrapped)
            lastPoint.u -= MathF.Floor(lastPoint.u);
        if (lastPoint.u < 0 && _firstObj.IsUWrapped)
            lastPoint.u = _firstObj.USize + lastPoint.u;

        if (lastPoint.v > _firstObj.VSize && _firstObj.IsVWrapped)
            lastPoint.v -= MathF.Floor(lastPoint.v);
        if (lastPoint.v < 0 && _firstObj.IsVWrapped)
            lastPoint.v = _firstObj.VSize + lastPoint.v;


        if (lastPoint.s > _secondObj.USize && _secondObj.IsUWrapped)
            lastPoint.s -= MathF.Floor(lastPoint.s);
        if (lastPoint.s < 0 && _secondObj.IsUWrapped)
            lastPoint.s = _secondObj.USize + lastPoint.s;

        if (lastPoint.t > _secondObj.VSize && _secondObj.IsVWrapped)
            lastPoint.t -= MathF.Floor(lastPoint.t);
        if (lastPoint.t < 0 && _secondObj.IsVWrapped)
            lastPoint.t = _secondObj.VSize + lastPoint.t;


        if (lastPoint.u > _firstObj.USize || lastPoint.u < 0)
            return true;
        if (lastPoint.v > _firstObj.VSize || lastPoint.v < 0)
            return true;
        if (lastPoint.s > _secondObj.USize || lastPoint.s < 0)
            return true;
        if (lastPoint.t > _secondObj.VSize || lastPoint.t < 0)
            return true;
        return false;
    }

    private IntersectionPoint FindNextPoint(IntersectionPoint lastPoint, bool right = true)
    {
        var lastPointP = _firstObj.GetPositionAndGradient(lastPoint.u, lastPoint.v);
        var lastPointQ = _secondObj.GetPositionAndGradient(lastPoint.s, lastPoint.t);

        var NP = Vector3.Cross(lastPointP.dU, lastPointP.dV).Normalized();
        var NQ = Vector3.Cross(lastPointQ.dU, lastPointQ.dV).Normalized();

        var cross = right ? Vector3.Cross(NP, NQ) : Vector3.Cross(NQ, NP);
        cross.Normalize();

        var F = (Vector4 vec) =>
        {
            float u = vec.X;
            float v = vec.Y;
            float s = vec.Z;
            float t = vec.W;
            var P = _firstObj.GetPositionAndGradient(u, v);
            var Q = _secondObj.GetPositionAndGradient(s, t);

            return new Vector4(
                P.pos.X - Q.pos.X,
                P.pos.Y - Q.pos.Y,
                P.pos.Z - Q.pos.Z,
                Vector3.Dot(P.pos - lastPointP.pos, cross) - PointsDist);
        };

        var dF = (Vector4 vec) =>
        {
            float u = vec.X;
            float v = vec.Y;
            float s = vec.Z;
            float t = vec.W;
            var valuesForP = _firstObj.GetPositionAndGradient(u, v);
            var valuesForQ = _secondObj.GetPositionAndGradient(s, t);

            var dPdU = valuesForP.dU;
            var dPdV = valuesForP.dV;

            var dQdS = -valuesForQ.dU;
            var dQdT = -valuesForQ.dV;

            var mat = new Matrix4()
            {
                M11 = dPdU.X,
                M12 = dPdV.X,
                M13 = dQdS.X,
                M14 = dQdT.X,
                M21 = dPdU.Y,
                M22 = dPdV.Y,
                M23 = dQdS.Y,
                M24 = dQdT.Y,
                M31 = dPdU.Z,
                M32 = dPdV.Z,
                M33 = dQdS.Z,
                M34 = dQdT.Z,
                M41 = Vector3.Dot(dPdU, cross),
                M42 = Vector3.Dot(dPdV, cross),
                M43 = 0,
                M44 = 0,
            };
            //mat.Transpose();
            return mat;
        };

        Vector4 xk, xk1;
        xk = lastPoint.parameters;
        xk1 = (-1, -1, -1, -1);

        int it = 0;
        do
        {
            if (it > NewtonMaxIterations)
                return null;
            if (it > 0)
            {
                xk = xk1;
            }

            it++;

            // if (Math.Abs(dF(xk).Determinant) > 1e-6)
            //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint("g"));

            xk1 = xk - dF(xk).Inverted() * F(xk);

            xk1 = Wrap(xk1);

            //Print(right, xk1, it);

            if (xk1.X > _firstObj.USize || xk1.Y > _firstObj.VSize || xk1.Z > _secondObj.USize ||
                xk1.W > _secondObj.VSize)
                break;
            if (xk1.X < 0 || xk1.Y < 0 || xk1.Z < 0 || xk1.W < 0)
                break;
        } while (MathM.DistanceSquared(_firstObj.GetValueAt(xk1.X, xk1.Y), _firstObj.GetValueAt(xk.X, xk.Y)) >
                 NewtonEps);

        return new IntersectionPoint()
        {
            pos = _firstObj.GetValueAt(xk1.X, xk1.Y),
            u = xk1.X,
            v = xk1.Y,
            s = xk1.Z,
            t = xk1.W,
        };
    }

    private Vector4 Wrap(Vector4 xk1)
    {
        if (xk1.X > _firstObj.USize && _firstObj.IsUWrapped)
            xk1.X -= MathF.Floor(xk1.X);
        if (xk1.X < 0 && _firstObj.IsUWrapped)
            xk1.X = _firstObj.USize + xk1.X;

        if (xk1.Y > _firstObj.VSize && _firstObj.IsVWrapped)
            xk1.Y -= MathF.Floor(xk1.Y);
        if (xk1.Y < 0 && _firstObj.IsVWrapped)
            xk1.Y = _firstObj.VSize + xk1.Y;


        if (xk1.Z > _secondObj.USize && _secondObj.IsUWrapped)
            xk1.Z -= MathF.Floor(xk1.Z);
        if (xk1.Z < 0 && _secondObj.IsUWrapped)
            xk1.Z = _secondObj.USize + xk1.Z;

        if (xk1.W > _secondObj.VSize && _secondObj.IsVWrapped)
            xk1.W -= MathF.Floor(xk1.W);
        if (xk1.W < 0 && _secondObj.IsVWrapped)
            xk1.W = _secondObj.VSize + xk1.W;

        return xk1;
    }

    private IntersectionPoint FindFirstIntersectionPoint(
        ((Vector3 pos, float u, float v) first, (Vector3 pos, float u, float v) second) closestPoints)
    {
        var stepLength = StartingGradientStepLength;
        var F = (float u, float v, float s, float t) =>
        {
            return (_firstObj.GetValueAt(u, v) - _secondObj.GetValueAt(s, t)).LengthSquared;
        };

        var gradF = (float u, float v, float s, float t) =>
        {
            var valuesForA = _firstObj.GetPositionAndGradient(u, v);
            var valuesForB = _secondObj.GetPositionAndGradient(s, t);
            var AmB = valuesForA.pos - valuesForB.pos;
            var dAdU = 2 * valuesForA.dU;
            var dAdV = 2 * valuesForA.dV;
            var dBdS = -2 * valuesForB.dU;
            var dBdT = -2 * valuesForB.dV;
            return new Vector4()
            {
                X = Vector3.Dot(dAdU, AmB),
                Y = Vector3.Dot(dAdV, AmB),
                Z = Vector3.Dot(dBdS, AmB),
                W = Vector3.Dot(dBdT, AmB),
            };
        };

        Vector4 uvstI = new Vector4()
        {
            X = closestPoints.first.u,
            Y = closestPoints.first.v,
            Z = closestPoints.second.u, //s
            W = closestPoints.second.v, //t
        };

        Vector4 uvstI1 = new Vector4();

        int it = 0;
        do
        {
            if (it > 0)
            {
                uvstI = uvstI1;
            }

            it++;

            uvstI1 = uvstI - stepLength * gradF(uvstI.X, uvstI.Y, uvstI.Z, uvstI.W);
            if (F(uvstI1.X, uvstI1.Y, uvstI1.Z, uvstI1.W) >= F(uvstI.X, uvstI.Y, uvstI.Z, uvstI.W))
            {
                stepLength /= 1.5f;
            }

            //Vector3 pos = _firstObj.GetValueAt(uvstI1.X, uvstI1.Y);
            // Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            // {
            //     posX = pos.X,
            //     posY = pos.Y,
            //     posZ = pos.Z
            // });
        } while (MathM.DistanceSquared(_firstObj.GetValueAt(uvstI.X, uvstI.Y),
                     _firstObj.GetValueAt(uvstI1.X, uvstI1.Y)) > GradientEps);

        float u = uvstI1.X;
        float v = uvstI1.Y;
        float s = uvstI1.Z;
        float t = uvstI1.W;
        return new() {pos = _firstObj.GetValueAt(u, v), u = u, v = v, s = s, t = t};
    }

    private ((Vector3 pos, float u, float v) first, (Vector3 pos, float u, float v) second) FindClosestPoints(
        List<(Vector3 pos, float u, float v)> startingPointsFirst,
        List<(Vector3 pos, float u, float v)> startingPointsSecond,
        bool selfIntersection)
    {
        ((Vector3 pos, float u, float v) first, (Vector3 pos, float u, float v) second) closest =
            (startingPointsFirst[0], startingPointsSecond[0]);
        var minDist = selfIntersection? float.MaxValue: MathM.Distance(closest.first.pos, closest.second.pos);

        if (!UseCursor)
        {
            for (int i = 0; i < startingPointsFirst.Count; i++)
            {
                for (int j = 0; j < startingPointsSecond.Count; j++)
                {
                    var currDist = MathM.DistanceSquared(startingPointsFirst[i].pos, startingPointsSecond[j].pos);
                    if(selfIntersection && MathM.DistanceSquared((startingPointsFirst[i].u, startingPointsFirst[i].v),(startingPointsSecond[j].u, startingPointsSecond[j].v))< MinDistParameterSpace)
                           continue;
                    if (currDist < minDist)
                    {
                        minDist = currDist;
                        closest = (startingPointsFirst[i], startingPointsSecond[j]);
                    }
                }
            }
        }
        else
        {
            var pointerPos = Scene.CurrentScene.ObjectsController._pointer.pos;
            minDist = MathM.Distance(pointerPos, startingPointsFirst[0].pos);
            var first = startingPointsFirst[0];

            for (int i = 1; i < startingPointsFirst.Count; i++)
            {
                var currDist = MathM.DistanceSquared(pointerPos, startingPointsFirst[i].pos);
                if (currDist < minDist)
                {
                    minDist = currDist;
                    first = startingPointsFirst[i];
                }
            }

            minDist = MathM.Distance(pointerPos, startingPointsSecond[0].pos);
            var second = startingPointsSecond[0];

            for (int i = 1; i < startingPointsSecond.Count; i++)
            {
                var currDist = MathM.DistanceSquared(pointerPos, startingPointsSecond[i].pos);
                if(selfIntersection && MathM.DistanceSquared((first.u, first.v),(startingPointsSecond[i].u, startingPointsSecond[i].v))< MinDistParameterSpace)
                    continue;
                if (currDist < minDist)
                {
                    minDist = currDist;
                    second = startingPointsSecond[i];
                }
            }

            closest = (first, second);
        }

        return closest;
    }

    public void ConvertToInterpolating()
    {
        Scene.CurrentScene.ObjectsController.SelectedObject = null;
        var interpolating = new InterpolatingBezierCurveC2();
        Scene.CurrentScene.ObjectsController.AddObjectToScene(interpolating);
        Scene.CurrentScene.ObjectsController.SelectedObject = null;

        foreach (var point in points)
        {
            var pos = _firstObj.GetValueAt(point.u, point.v);
            var p = new ParameterizedPoint($"u:{point.u}; v:{point.v}; s:{point.s}; t:{point.t}")
            {
                posX = pos.X,
                posY = pos.Y,
                posZ = pos.Z,
            };
            Scene.CurrentScene.ObjectsController.AddObjectToScene(p);
            interpolating.ProcessObject(p);
        }

        // Scene.CurrentScene.ObjectsController.SelectedObject = null;
        // var interpolating2 = new InterpolatingBezierCurveC2();
        // Scene.CurrentScene.ObjectsController.AddObjectToScene(interpolating2);
        // Scene.CurrentScene.ObjectsController.SelectedObject = null;
        //
        // foreach (var point in points)
        // {
        //     var pos = _secondObj.GetValueAt(point.s, point.t);
        //     var p = new ParameterizedPoint($"u:{point.u}; v:{point.v}; s:{point.s}; t:{point.t}")
        //     {
        //         posX = pos.X,
        //         posY = pos.Y,
        //         posZ = pos.Z,
        //     };
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(p);
        //     interpolating2.ProcessObject(p);
        // }
    }

    public void ShowC0()
    {
        Scene.CurrentScene.ObjectsController.SelectedObject = null;
        var intersectionC0 = new IntersectionCurve();
        intersectionC0.Name = "intersectionC0";
        Scene.CurrentScene.ObjectsController.AddObjectToScene(intersectionC0);
        Scene.CurrentScene.ObjectsController.SelectedObject = null;

        foreach (var point in points)
        {
            var pos = _firstObj.GetValueAt(point.u, point.v);
            var p = new ParameterizedPoint($"u:{point.u}; v:{point.v}; s:{point.s}; t:{point.t}")
            {
                posX = pos.X,
                posY = pos.Y,
                posZ = pos.Z,
            };
            //Scene.CurrentScene.ObjectsController.AddObjectToScene(p);
            intersectionC0.ProcessObject(p);
        }
    }

    #region Misc

    private void Print(bool right, Vector4 xk1, int it)
    {
        if (right)
        {
            var f = _firstObj.GetPositionAndGradient(xk1.X, xk1.Y);
            var fdU = new BezierCurveC0()
            {
                Name = $"fdU{it}"
            };
            var fdV = new BezierCurveC0()
            {
                Name = $"fdV{it}"
            };
            var fPos = new ParameterizedPoint($"f {it}")
            {
                posX = f.pos.X,
                posY = f.pos.Y,
                posZ = f.pos.Z,
            };
            var fPosdU = new ParameterizedPoint()
            {
                posX = f.pos.X + f.dU.X,
                posY = f.pos.Y + f.dU.Y,
                posZ = f.pos.Z + f.dU.Z,
            };
            var fPosdV = new ParameterizedPoint()
            {
                posX = f.pos.X + f.dV.X,
                posY = f.pos.Y + f.dV.Y,
                posZ = f.pos.Z + f.dV.Z,
            };
            Scene.CurrentScene.ObjectsController.SelectedObject = null;
            Scene.CurrentScene.ObjectsController.AddObjectToScene(fdU);
            Scene.CurrentScene.ObjectsController.SelectedObject = null;
            Scene.CurrentScene.ObjectsController.AddObjectToScene(fdV);
            Scene.CurrentScene.ObjectsController.SelectedObject = null;
            Scene.CurrentScene.ObjectsController.AddObjectToScene(fPos);
            Scene.CurrentScene.ObjectsController.AddObjectToScene(fPosdU);
            Scene.CurrentScene.ObjectsController.AddObjectToScene(fPosdV);
            fdU.ProcessObject(fPos);
            fdU.ProcessObject(fPosdU);
            fdV.ProcessObject(fPos);
            fdV.ProcessObject(fPosdV);

            var s = _secondObj.GetPositionAndGradient(xk1.Z, xk1.W);
            var sdU = new BezierCurveC0()
            {
                Name = $"sdU{it}"
            };
            var sdV = new BezierCurveC0()
            {
                Name = $"sdV{it}"
            };
            var sPos = new ParameterizedPoint($"s {it}; s: {xk1.Z} t: {xk1.W}")
            {
                posX = s.pos.X,
                posY = s.pos.Y,
                posZ = s.pos.Z,
            };
            var sPosdU = new ParameterizedPoint()
            {
                posX = s.pos.X + s.dU.X,
                posY = s.pos.Y + s.dU.Y,
                posZ = s.pos.Z + s.dU.Z,
            };
            var sPosdV = new ParameterizedPoint()
            {
                posX = s.pos.X + s.dV.X,
                posY = s.pos.Y + s.dV.Y,
                posZ = s.pos.Z + s.dV.Z,
            };
            Scene.CurrentScene.ObjectsController.SelectedObject = null;
            Scene.CurrentScene.ObjectsController.AddObjectToScene(sdU);
            Scene.CurrentScene.ObjectsController.SelectedObject = null;
            Scene.CurrentScene.ObjectsController.AddObjectToScene(sdV);
            Scene.CurrentScene.ObjectsController.SelectedObject = null;
            Scene.CurrentScene.ObjectsController.AddObjectToScene(sPos);
            Scene.CurrentScene.ObjectsController.AddObjectToScene(sPosdU);
            Scene.CurrentScene.ObjectsController.AddObjectToScene(sPosdV);
            sdU.ProcessObject(sPos);
            sdU.ProcessObject(sPosdU);
            sdV.ProcessObject(sPos);
            sdV.ProcessObject(sPosdV);
            // var s = _firstObj.GetPositionAndGradient(xk1.Z, xk1.W);
            // Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"s {it}")
            // {
            //     posX = s.pos.X,
            //     posY = s.pos.Y,
            //     posZ = s.pos.Z,
            // });
        }
    }

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}