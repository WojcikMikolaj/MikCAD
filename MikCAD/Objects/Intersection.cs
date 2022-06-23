using System;
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

    public float GradientStepLength { get; set; } = 0.1f;
    public int NumberOfPoints { get; set; }
    public float Steps { get; set; }
    public bool UseCursor { get; set; }

    public void Intersect()
    {
        if (!_selfIntersection)
        {
            var startingPointsFirst = _firstObj.GetStartingPoints();
            var startingPointsSecond = _secondObj.GetStartingPoints();
            var closestPoints = FindClosestPoints(startingPointsFirst, startingPointsSecond);

            var firstIntersectionPoint = FindFirstIntersectionPoint(closestPoints);
        }
    }

    private (Vector3 pos, float u, float v, float s, float t) FindFirstIntersectionPoint(((Vector3 pos, float u, float v) first, (Vector3 pos, float u, float v) second) closestPoints)
    {
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
            return new Vector4(){
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
        
        while ()
        {
            uvstI1 = uvstI - GradientStepLength * 
        }

    }

    private ((Vector3 pos, float u, float v) first, (Vector3 pos, float u, float v) second) FindClosestPoints(List<(Vector3 pos, float u, float v)> startingPointsFirst, List<(Vector3 pos, float u, float v)> startingPointsSecond)
    {
        ((Vector3 pos, float u, float v) first, (Vector3 pos, float u, float v) second) closest=
            (startingPointsFirst[0], startingPointsSecond[0]);
        var minDist = MathM.Distance(closest.first.pos, closest.second.pos);
        
        for (int i = 0; i < startingPointsFirst.Count; i++)
        {
            for (int j = 0; j < startingPointsSecond.Count; j++)
            {
                var currDist = MathM.DistanceSquared(startingPointsFirst[i].pos, startingPointsSecond[j].pos);
                if (currDist < minDist )
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