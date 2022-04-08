using OpenTK.Mathematics;

namespace MikCAD.BezierCurves;

public interface IBezierCurve
{
    void ProcessPoint(ParameterizedPoint point);
    Vector4 CurveColor { get; set; }
}