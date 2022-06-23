using OpenTK.Mathematics;

namespace MikCAD;

public interface IIntersectable
{
    Vector3 GetValueAt(float u, float v);
    Vector3 GetUDerivativeAt(float u, float v);
    Vector3 GetVDerivativeAt(float u, float v);
    (Vector3 pos, Vector3 dU, Vector3 dV) GetPositionAndGradient(float u, float v);
}