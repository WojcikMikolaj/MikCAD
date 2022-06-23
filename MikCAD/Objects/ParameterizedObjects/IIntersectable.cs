using System.Collections.Generic;
using OpenTK.Mathematics;

namespace MikCAD;

public interface IIntersectable
{
    List<(Vector3 pos,float u, float v)> GetStartingPoints()
    {
        var result = new List<(Vector3 pos,float u, float v)>();
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                result.Add((GetValueAt(i*0.05f,j*0.05f), i*0.05f,j*0.05f));
            }
        }

        return result;
    }
    Vector3 GetValueAt(float u, float v);
    Vector3 GetUDerivativeAt(float u, float v);
    Vector3 GetVDerivativeAt(float u, float v);
    (Vector3 pos, Vector3 dU, Vector3 dV) GetPositionAndGradient(float u, float v);
}