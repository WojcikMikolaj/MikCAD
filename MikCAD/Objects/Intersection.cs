﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MikCAD.Annotations;
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

    private IIntersectable _firstObj;
    private IIntersectable _secondObj;

    public int NumberOfPoints { get; set; }
    public float Steps { get; set; }
    public bool UseCursor { get; set; }

    public float StartingGradientStepLength { get; set; } = 0.1f;
    public float GradientEps { get; set; } = 0.001f;

    public float PointsDist { get; set; } = 0.025f;
    public float NewtonEps { get; set; } = 0.001f;

    public void Intersect()
    {
        if (!_selfIntersection)
        {
            List<IntersectionPoint> intersectionPoints = new List<IntersectionPoint>();

            var startingPointsFirst = _firstObj.GetStartingPoints();
            var startingPointsSecond = _secondObj.GetStartingPoints();
            var closestPoints = FindClosestPoints(startingPointsFirst, startingPointsSecond);

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
                    _secondObj.GetValueAt(firstIntersectionPoint.s, firstIntersectionPoint.t)) > GradientEps*10)
            {
                return;
            }

            intersectionPoints.Add(firstIntersectionPoint);
            var points = FindOtherPoints(firstIntersectionPoint);
            foreach (var point in points)
            {
                Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
                {
                    posX = point.pos.X,
                    posY = point.pos.Y,
                    posZ = point.pos.Z,
                });
            }
        }
    }

    class IntersectionPoint
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
        List<IntersectionPoint> leftPoints = new List<IntersectionPoint>();
        List<IntersectionPoint> rightPoints = new List<IntersectionPoint>();


        while (true)
        {
            lastPoint = FindNextPoint(lastPoint);
            if (lastPoint == null)
                break;
            rightPoints.Add(lastPoint);
            if (lastPoint.u > 1 || lastPoint.u < 0)
                break;
            if (lastPoint.v > 1 || lastPoint.v < 0)
                break;
            if (lastPoint.s > 1 || lastPoint.s < 0)
                break;
            if (lastPoint.t > 1 || lastPoint.t < 0)
                break;
        }

        lastPoint = firstIntersectionPoint;
        while (true)
        {
            lastPoint = FindNextPoint(lastPoint, false);
            if (lastPoint == null)
                break;
            leftPoints.Add(lastPoint);
            if (lastPoint.u > 1 || lastPoint.u < 0)
                break;
            if (lastPoint.v > 1 || lastPoint.v < 0)
                break;
            if (lastPoint.s > 1 || lastPoint.s < 0)
                break;
            if (lastPoint.t > 1 || lastPoint.t < 0)
                break;
        }

        leftPoints.Reverse();
        leftPoints.Add(firstIntersectionPoint);
        foreach (var point in rightPoints)
        {
            leftPoints.Add(point);
        }

        return leftPoints;
    }

    private IntersectionPoint FindNextPoint(IntersectionPoint lastPoint, bool right = true)
    {
        var lastPointP = _firstObj.GetPositionAndGradient(lastPoint.u, lastPoint.v);
        var lastPointQ = _secondObj.GetPositionAndGradient(lastPoint.s, lastPoint.t);

        var NP = Vector3.Cross(lastPointP.dU, lastPointP.dV);
        var NQ = Vector3.Cross(lastPointQ.dU, lastPointQ.dV);

        var cross = right ? Vector3.Cross(NP, NQ) : Vector3.Cross(NQ, NP);

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
            // mat.Transpose();
            return mat;
        };
        List<IntersectionPoint> points = new List<IntersectionPoint>();

        Vector4 xk, xk1;

        xk = lastPoint.parameters;
        xk1 = (-1, -1, -1, -1);

        int it = 0;
        do
        {
            if (it != 0)
            {
                xk = xk1;
            }

            it++;

            xk1 = xk - dF(xk).Inverted() * F(xk);
            if (it > 10)
                return null;
        } while (MathM.DistanceSquared(_firstObj.GetValueAt(xk1.X, xk1.Y), _firstObj.GetValueAt(xk.X, xk.Y)) >
                 NewtonEps);

        if (MathM.Distance(_firstObj.GetValueAt(xk1.X, xk1.Y), lastPoint.pos) > PointsDist * 1.5f)
            return null;
        return new IntersectionPoint()
        {
            pos = _firstObj.GetValueAt(xk1.X, xk1.Y),
            u = xk1.X,
            v = xk1.Y,
            s = xk1.Z,
            t = xk1.W,
        };
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
            Z = closestPoints.second.u,
            W = closestPoints.second.v,
        };

        Vector4 uvstI1 = new Vector4();

        int it = 0;
        do
        {
            if (it != 0)
            {
                uvstI = uvstI1;
            }
            else
            {
                it++;
            }

            uvstI1 = uvstI - stepLength * gradF(uvstI.X, uvstI.Y, uvstI.Z, uvstI.W);
            if (F(uvstI1.X, uvstI1.Y, uvstI1.Z, uvstI1.W) >= F(uvstI.X, uvstI.Y, uvstI.Z, uvstI.W))
            {
                stepLength /= 2f;
            }

            Vector3 pos = _firstObj.GetValueAt(uvstI1.X, uvstI1.Y);

            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = pos.X,
                posY = pos.Y,
                posZ = pos.Z
            });
        } while (MathM.DistanceSquared(_firstObj.GetValueAt(uvstI.X, uvstI.Y), _firstObj.GetValueAt(uvstI1.X, uvstI1.Y)) > GradientEps);

        float u = uvstI1.X;
        float v = uvstI1.Y;
        float s = uvstI1.Z;
        float t = uvstI1.W;
        return new() {pos = _firstObj.GetValueAt(u, v), u = u, v = v, s = s, t = t};
    }

    private ((Vector3 pos, float u, float v) first, (Vector3 pos, float u, float v) second) FindClosestPoints(
        List<(Vector3 pos, float u, float v)> startingPointsFirst,
        List<(Vector3 pos, float u, float v)> startingPointsSecond)
    {
        ((Vector3 pos, float u, float v) first, (Vector3 pos, float u, float v) second) closest =
            (startingPointsFirst[0], startingPointsSecond[0]);
        var minDist = MathM.Distance(closest.first.pos, closest.second.pos);

        for (int i = 0; i < startingPointsFirst.Count; i++)
        {
            for (int j = 0; j < startingPointsSecond.Count; j++)
            {
                var currDist = MathM.DistanceSquared(startingPointsFirst[i].pos, startingPointsSecond[j].pos);
                if (currDist < minDist)
                {
                    minDist = currDist;
                    closest = (startingPointsFirst[i], startingPointsSecond[j]);
                }
            }
        }

        return closest;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}