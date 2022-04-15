using OpenTK.Mathematics;

namespace MikCAD.BezierCurves;

public interface IBezierCurve
{
    void ProcessPoint(ParameterizedPoint point);
    void MoveUp(ParameterizedObject parameterizedObject);
    void MoveDown(ParameterizedObject parameterizedObject);
    Vector4 CurveColor { get; set; }
}