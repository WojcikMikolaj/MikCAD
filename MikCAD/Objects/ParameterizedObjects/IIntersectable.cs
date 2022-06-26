using System.Collections.Generic;
using OpenTK.Mathematics;

namespace MikCAD;

public interface IIntersectable
{
    List<(Vector3 pos,float u, float v)> GetStartingPoints(int uDivs =20, int vDivs=20 )
    {
        var result = new List<(Vector3 pos,float u, float v)>();
        for (int i = 0; i < uDivs; i++)
        {
            for (int j = 0; j < vDivs; j++)
            {
                result.Add((GetValueAt(i*USize/uDivs,j*VSize/vDivs), i*USize/uDivs,j*VSize/vDivs));
            }
        }
        return result;
    }
    Vector3 GetValueAt(float u, float v);
    Vector3 GetUDerivativeAt(float u, float v);
    Vector3 GetVDerivativeAt(float u, float v);
    (Vector3 pos, Vector3 dU, Vector3 dV) GetPositionAndGradient(float u, float v);
    bool IsUWrapped { get; }
    bool IsVWrapped { get; }
    
    float USize { get; }
    float VSize { get; }
}