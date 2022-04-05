using MikCAD.BezierCurves;

namespace MikCAD;

public class FakePoint : ParameterizedPoint
{
    public FakePoint(): base("FakePoint")
    {
        
    }

    public BezierCurveC2 parent { get; set; }
}