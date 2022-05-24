using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MikCAD.Annotations;
using MikCAD.BezierCurves;
using MikCAD.Utilities;
using SharpSceneSerializer;
using SharpSceneSerializer.DTOs.GeometryObjects;
using SharpSceneSerializer.DTOs.Interfaces;
using BezierSurfaceC0 = MikCAD.BezierSurfaces.BezierSurfaceC0;
using BezierSurfaceC2 = MikCAD.BezierSurfaces.BezierSurfaceC2;

namespace MikCAD;

public partial class MainWindow
{
    private void OnSaveCommand(object sender, RoutedEventArgs e)
    {
        FileDialog diag = new SaveFileDialog();
        if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var objects = scene.ObjectsController.ParameterizedObjects;

            var points = new List<SharpSceneSerializer.DTOs.GeometryObjects.Point>();
            var geometry = new List<IGeometryObject>();
            
            foreach (var o in objects)
            {
                switch (o)
                {
                    case ParameterizedPoint p:
                        points.Add((SharpSceneSerializer.DTOs.GeometryObjects.Point)p);
                        break;
                    case Torus t:
                        geometry.Add((SharpSceneSerializer.DTOs.GeometryObjects.Torus)t);
                        break;
                    case BezierCurveC0 c0:
                        geometry.Add((SharpSceneSerializer.DTOs.GeometryObjects.BezierC0)c0);
                        break;
                    case BezierCurveC2 c2:
                        geometry.Add((SharpSceneSerializer.DTOs.GeometryObjects.BezierC2)c2);
                        break;
                    case InterpolatingBezierCurveC2 i2:
                        geometry.Add((SharpSceneSerializer.DTOs.GeometryObjects.InterpolatedC2)i2);
                        break;
                    case BezierSurfaceC0 s0:
                        geometry.Add((SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC0)s0);
                        break;
                    case BezierSurfaceC2 s2:
                        geometry.Add((SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2)s2);
                        break;
                }
            }
            SharpSceneSerializer.DTOs.Scene sceneToSave = new SharpSceneSerializer.DTOs.Scene(points, geometry);
            SharpSceneSerializer.SceneSerializer.Serialize(sceneToSave, diag.FileName);
        }
    }
    
    private void OnLoadCommand(object sender, RoutedEventArgs e)
    {
        FileDialog diag = new OpenFileDialog();
        if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var loadResult= SharpSceneSerializer.SceneSerializer.Deserialize( diag.FileName,@"D:\MG1\Aplikacja\MikCAD\MikCAD\libs\schema.json");
            if (loadResult.succeded)
            {
                scene.ObjectsController.ClearScene();
                foreach (var p in loadResult.scene.Points)
                {
                    scene.ObjectsController.AddObjectToScene((ParameterizedPoint) p);
                }

                foreach (var geometryObject in loadResult.scene.Geometry)
                {
                    scene.ObjectsController.SelectedObject = null;
                    switch (geometryObject)
                    {
                        case BezierC0 bezierC0:
                            scene.ObjectsController.AddObjectToScene((BezierCurveC0)bezierC0);
                            break;
                        case BezierC2 bezierC2:
                            scene.ObjectsController.AddObjectToScene((BezierCurveC2)bezierC2);
                            break;
                        case BezierPatchC0 bezierPatchC0:
                            break;
                        case BezierPatchC2 bezierPatchC2:
                            break;
                        case SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC0 bezierSurfaceC0:
                            break;
                        case SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2 bezierSurfaceC2:
                            break;
                        case InterpolatedC2 interpolatedC2:
                            break;
                        case SharpSceneSerializer.DTOs.GeometryObjects.Torus torus:
                            break;
                    }
                }
            }
        }
    }
}