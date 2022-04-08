using System.Diagnostics;
using MikCAD.BezierCurves;

namespace MikCAD;

[DebuggerDisplay("BSpline {BSplinePointToMove}")]
public class FakePoint : ParameterizedPoint
{
    public FakePoint() : base("FakePoint")
    {
    }

    public override void OnPositionUpdate()
    {
        foreach (var parent in parents)
        {
            (parent as BezierCurveC2)?.UpdatePoints(this);    
        }
    }

    public int ID
    {
        get;
        init;
    }
    
    public int BSplinePointToMove
    {
        get;
        init;
    }
}