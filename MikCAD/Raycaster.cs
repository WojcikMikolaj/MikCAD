using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MikCAD;

public class Raycaster
{
    //Copied from: https://antongerdelan.net/opengl/raycasting.html
    public static ParameterizedObject FindIntersectingPoint(float mouse_x, float mouse_y)
    {
        float x = (2.0f * mouse_x) / (float)MainWindow.current.OpenTkControl.ActualWidth - 1.0f;
        float y = 1.0f - (2.0f * mouse_y) / (float)MainWindow.current.OpenTkControl.ActualHeight;
        float z = 1.0f;

        var ndc = new Vector4(x, y, z, 1);
        var unprojected = ndc * Scene.CurrentScene.camera.GetProjectionMatrix().Inverted();
        var unview = unprojected * Scene.CurrentScene.camera.GetViewMatrix().Inverted();
        var raycastDir = unview;
        raycastDir.W = 0;
        
        var raycastSource = new Vector4(Scene.CurrentScene.camera.WorldPosition);
        raycastSource.W = 1;

        //dotąd jest ok
        ParameterizedPoint currPoint = null;
        float tmin = float.MaxValue;
        foreach (var point in Scene.CurrentScene.ObjectsController.ParameterizedPoints)
        {
            var mat = Matrix4.Identity;
            mat.M44 = -point.BB.radius * point.BB.radius;

            var model = point.GetOnlyModelMatrix();
            var finalMatrix = model.Inverted() * mat * Matrix4.Transpose(model.Inverted());

            var a = Vector4.Dot(raycastDir * finalMatrix, raycastDir);
            var b = Vector4.Dot(2 * raycastDir * finalMatrix, raycastSource);
            var c = Vector4.Dot(raycastSource * finalMatrix, raycastSource);
            
            var delta = b * b - 4 * a * c;

           // MainWindow.current.Title = $"Delta: {delta}";
            if (delta > 0)
            {
                //działa do tego miejsca
                var sqd = (float)MathHelper.Sqrt(delta);
                var t = Math.Min(-b - delta / 2, -b + delta / 2);
                if (t < tmin)
                {
                    tmin = t;
                    currPoint = point;
                }
            }
        }
        
        return currPoint;
    }

    public static Vector4 FindPointOnCameraPlain(float mouse_x, float mouse_y)
    {
        float x = (2.0f * mouse_x) / ((float)MainWindow.current.OpenTkControl.ActualWidth - 1) - 1.0f;
        float y = 1.0f - (2.0f * mouse_y) / ((float)MainWindow.current.OpenTkControl.ActualHeight - 1);
        float z = 1.0f;

        var ndc = new Vector4(x, y, z, 1);
        var unprojected = ndc * Scene.CurrentScene.camera.GetProjectionMatrix().Inverted();
        var unview = unprojected * Scene.CurrentScene.camera.GetViewMatrix().Inverted();
        var raycastDir = unview;
        raycastDir.W = 0;
        
        var raycastSource = new Vector4(Scene.CurrentScene.camera.WorldPosition);
        raycastSource.W = 1;

        //dotąd jest ok
        var cameraVec = Scene.CurrentScene.camera.ActForward;
        var cameraTarget = Scene.CurrentScene.camera._position;
        var D = Vector3.Dot(cameraVec, -cameraTarget);

        var p = new Vector4(cameraVec);
        p.W = D;
        var t = -Vector4.Dot(p, raycastSource) / Vector4.Dot(p, raycastDir);

        var worldPoint = raycastDir * t + raycastSource;
        return worldPoint;
    }
}