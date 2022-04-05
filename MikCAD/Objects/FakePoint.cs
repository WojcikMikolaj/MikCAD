using MikCAD.BezierCurves;

namespace MikCAD;

public class FakePoint : ParameterizedPoint
{
    public FakePoint() : base("FakePoint")
    {
    }

    public override void OnPositionUpdate()
    {
        (parent as BezierCurveC2)?.UpdatePoints();
    }
}