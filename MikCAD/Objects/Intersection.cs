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
        }
    }

    private object FindClosestPoints(List<(Vector3 pos, float u, float v)> startingPointsFirst, List<(Vector3 pos, float u, float v)> startingPointsSecond)
    {
        ((Vector3 pos, float u, float v) first, (Vector3 pos, float u, float v) second) closest=
            (startingPointsFirst[0], startingPointsSecond[0]);
        var minDist = MathM.Distance(closest.first.pos, closest.second.pos);
        
        for (int i = 0; i < startingPointsFirst.Count; i++)
        {
            for (int j = 0; j < startingPointsSecond.Count; j++)
            {
                var currDist = MathM.Distance(startingPointsFirst[i].pos, startingPointsSecond[j].pos);
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