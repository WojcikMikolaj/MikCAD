namespace MikCAD.BezierSurfaces;

public interface ISurface
{
    void SubstitutePoints(ParameterizedPoint oldPoint, ParameterizedPoint newPoint);
}