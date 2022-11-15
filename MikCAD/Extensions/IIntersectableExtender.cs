namespace MikCAD.Extensions;

public static class IIntersectableExtender
{
    public static void Sample(this IIntersectable intersectable, int uSamplesCount, int vSamplesCount)
    {
        float u = 0;
        float v = 0;
        
        float dU = intersectable.USize/uSamplesCount;
        float dV = intersectable.VSize/vSamplesCount;

        for (int i = 0; i < uSamplesCount; i++)
        {
            v = 0;
            for (int j = 0; j < vSamplesCount; j++)
            {
                var pos = intersectable.GetValueAt(u, v);
                Scene.CurrentScene.ObjectsController.AddObjectToScene(
                    new FakePoint()
                    {
                        posX = pos.X,
                        posY = pos.Y,
                        posZ = pos.Z
                    });
                v += dV;
            }
            u += dU;
        }
    }
}