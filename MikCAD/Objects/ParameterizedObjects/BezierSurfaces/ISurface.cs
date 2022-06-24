using OpenTK.Mathematics;

namespace MikCAD.BezierSurfaces;

public interface ISurface
{
    void SubstitutePoints(ParameterizedPoint oldPoint, ParameterizedPoint newPoint);

    Vector3 EvaluateCurveAtT(float u, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int size = 4)
    {
        //t = 1 - t;
        int i = 0;
        int j = 0;
        Vector3[] arr = new Vector3[] {p0, p1, p2, p3};
        if (size > 4)
            size = 4;
        for (i = 1; i < size; i++)
        {
            for (j = 0; j < size - i; j++)
            {
                arr[j] = arr[j] * (1.0f - u) + arr[j + 1] * u;
            }
        }

        return arr[0];
    }

    Vector3[] ConvertBSplineToBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var points = new Vector3[4];
        points[1] = p1 + (p2 - p1) / 3;
        points[2] = p1 + 2 * (p2 - p1) / 3;

        var pom1 = p0 + 2 * (p1 - p0) / 3;
        points[0] = pom1 + (points[1] - pom1) / 2;

        var pom2 = p2 + (p3 - p2) / 3;
        points[3] = points[2] + (pom2 - points[2]) / 2;
        return points;
    }
}