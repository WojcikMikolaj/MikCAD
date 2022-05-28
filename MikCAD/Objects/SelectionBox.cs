using System;
using System.Numerics;

namespace MikCAD.Objects;

public class SelectionBox
{
    public bool Draw { get; set; } = false;
    public float X1 { get; set; }
    public float X2 { get; set; }
    public float Y1 { get; set; }
    public float Y2 { get; set; }

    public bool InBox(ParameterizedPoint point)
    {
        var projectionMatrix = Scene.CurrentScene.camera.GetProjectionMatrix();
        var viewMatrix = Scene.CurrentScene.camera.GetViewMatrix();
        var modelMatrix = point.GetModelMatrix();

        var pvm =  modelMatrix * viewMatrix * projectionMatrix ;
        var pos = new OpenTK.Mathematics.Vector4(0,0, 0 ,1f) * pvm;
        pos /= pos.W;

        return (pos.X > MathF.Min(X1, X2)) && (pos.X < MathF.Max(X1, X2)) && (pos.Y > MathF.Min(Y1, Y2)) &&
               (pos.Y < MathF.Max(Y1, Y2));
    }
}