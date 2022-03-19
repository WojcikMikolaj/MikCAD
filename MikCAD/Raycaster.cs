using System;
using OpenTK.Mathematics;

namespace MikCAD;

public class Raycaster
{
    public static ParameterizedObject FindIntersectingPoint(float mouse_x, float mouse_y)
    {
        float x = (2.0f * mouse_x) / (float)MainWindow.current.OpenTkControl.ActualWidth - 1.0f;
        float y = 1.0f - (2.0f * mouse_y) / (float)MainWindow.current.OpenTkControl.ActualHeight;
        float z = 1.0f;
        MainWindow.current.Title = $"x:{x}, y:{y}";
        Vector3 ray_nds = new Vector3(x, y, z);
        Vector4 ray_clip = new Vector4(ray_nds.X, ray_nds.Y, -1.0f, 1.0f);
        Vector4 ray_eye = Scene.CurrentScene.camera.GetProjectionMatrix().Inverted() * ray_clip;
        ray_eye = new Vector4(ray_eye.X, ray_eye.Y, -1.0f, 0.0f);
        Vector3 ray_wor = (Scene.CurrentScene.camera.GetViewMatrix().Inverted() * ray_eye).Xyz;
        // don't forget to normalise the vector at some point
        ray_wor = ray_wor.Normalized();
        var t = float.MaxValue;
        ParameterizedPoint obj = null;
        var O = Scene.CurrentScene.camera.WorldPosition;
        foreach (var point in Scene.CurrentScene.ObjectsController.ParameterizedPoints)
        {
            var omp = O - point.BB.position;
            var b = Vector3.Dot(ray_wor, omp);
            var c = Vector3.Dot(omp, omp) - point.BB.radius * point.BB.radius;
            var delta = b * b - c;
            if(delta<0)
                continue;
            else
            {
                var sd = Math.Sqrt(delta);
                var pom = Math.Min(-b - sd, -b + sd);
                if (pom < t)
                {
                    t = (float) pom;
                    obj = point;
                }
            }
        }
        
        return obj;
    }
}