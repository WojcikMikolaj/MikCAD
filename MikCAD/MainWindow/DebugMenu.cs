using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using MikCAD.Annotations;
using MikCAD.BezierCurves;
using MikCAD.Utilities;

namespace MikCAD;

public partial class MainWindow
{
    //TODO: Sprawdzić do czego to służy
    private bool _clear = false;

    private void SetClear(object sender, RoutedEventArgs e)
    {
        _clear = !_clear;
        scene.ObjectsController.ClearScene();
    }
    
    private void AddThreePoints(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posY = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posZ = 1});
    }

    private void AddThreeToruses(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new Torus() {posX = 3});
        scene.ObjectsController.AddObjectToScene(new Torus() {posY = 3});
        scene.ObjectsController.AddObjectToScene(new Torus() {posZ = 3});
    }

    private void RotTest(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new Torus() {posX = -3});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 3});
    }

    private void BezierCurveC0Test(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint());
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 2, posY = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 2, posY = 1, posZ = 1});
    }

    private void BezierCurveC2Test(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint());
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 2, posY = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 2, posY = 1, posZ = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 2, posY = 6, posZ = 2});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 0, posY = 1, posZ = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 0, posY = 3, posZ = 0});
    }

    private void BezierCurveC2BernsteinTest(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint());
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 2, posY = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 2, posY = 1, posZ = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 2, posY = 6, posZ = 2});
    }

    private void InterpolationTest(object sender, RoutedEventArgs e)
    {
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint());
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 1, posY = 1});
        scene.ObjectsController.AddObjectToScene(new ParameterizedPoint() {posX = 2, posY = 1});
    }
}