using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using MikCAD.Annotations;
using MikCAD.BezierCurves;

namespace MikCAD
{
    public partial class MainWindow
    {
        private bool _rightPressed = false;
        private bool _leftPressed = false;
        private System.Windows.Point? _last;

        struct mouse_position
        {
            public double X;
            public double Y;
        }

        private void Image_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                disabled = true;
                checkCollisions = true;
                m_x = (int) e.GetPosition(OpenTkControl).X;
                m_y = (int) e.GetPosition(OpenTkControl).Y;
                var point = Raycaster.FindIntersectingPoint((float) e.GetPosition(OpenTkControl).X,
                    (float) e.GetPosition(OpenTkControl).Y);
                // ParameterizedPoint point = null;
                if (point != null)
                {
                    IsMultiSelectEnabled = false;
                    if (AddToSelected)
                    {
                        #region bezierCurve

                        if (Scene.CurrentScene.ObjectsController.SelectedObject is BezierCurveC0 curveC0)
                        {
                            curveC0.ProcessObject(point);
                            bezierCurveC0Control.PointsList.Items.Refresh();
                        }
                        else if (Scene.CurrentScene.ObjectsController.SelectedObject is BezierCurveC2 curveC2)
                        {
                            curveC2.ProcessObject(point);
                            bezierCurveC2Control.PointsList.Items.Refresh();
                        }

                        #endregion

                        else if (Scene.CurrentScene.ObjectsController.SelectedObject is CompositeObject compositeObject)
                        {
                            compositeObject.ProcessObject(point);
                        }
                        else
                        {
                            Scene.CurrentScene.ObjectsController.SelectObject(point);
                        }
                    }
                    else
                    {
                        Scene.CurrentScene.ObjectsController.SelectObject(point);
                    }
                }
                else
                {
                    Scene.CurrentScene.ObjectsController.UnselectAll();
                }
            }

            if (!_rightPressed)
            {
                _leftPressed = true;
            }
        }

        private void Image_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _leftPressed = false;
            _last = null;
        }

        private void Image_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_leftPressed | _rightPressed)
            {
                var pos = e.GetPosition(this);
                if (_last.HasValue)
                {
                    var dx = (float) (pos.X - _last.Value.X);
                    var dy = (float) (pos.Y - _last.Value.Y);

                    //Title = $"{dx}, {dy}";

                    if (_leftPressed)
                    {
                        var rotX = scene.camera.rotX;
                        var rotY = scene.camera.rotY;

                        scene.camera.rotX = rotX + dy * 0.1f;
                        scene.camera.rotY = rotY + dx * 0.1f;
                    }
                    else
                    {
                        var posX = scene.camera.posX;
                        var posY = scene.camera.posY;

                        scene.camera.posX = posX + dx * 0.05f;
                        scene.camera.posY = posY + dy * 0.05f;
                    }
                }

                _last = pos;
            }
        }

        private void Window_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _rightPressed = false;
            _leftPressed = false;
            _last = null;
        }

        private void Image_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_leftPressed)
            {
                _rightPressed = true;
            }
        }

        private void Image_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _rightPressed = false;
        }

        private void Image_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //var value = scene.camera.fov;
            var value = scene.camera.Scale;
            value -= e.Delta * 0.005f;
            if (value < 0.1f)
                value = 0.1f;
            //scene.camera.fov = value;
            scene.camera.Scale = value;
        }

        public static bool IsMultiSelectEnabled { get; private set; }
        public static bool AddToSelected { get; private set; }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    AddToSelected = true;
                    break;
                case Key.LeftCtrl:
                    IsMultiSelectEnabled = true;
                    break;
                case Key.Escape:
                    Scene.CurrentScene.ObjectsController.UnselectAll();
                    break;
                case Key.Delete:
                    Scene.CurrentScene.ObjectsController.DeleteSelectedObjects();
                    break;
            }
        }


        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                IsMultiSelectEnabled = false;
            }

            if (e.Key == Key.A)
            {
                AddToSelected = false;
            }
        }

        private void AddTorus(object sender, RoutedEventArgs e)
        {
            scene.ObjectsController.AddObjectToScene(new Torus());
        }

        private void AddPoint(object sender, RoutedEventArgs e)
        {
            scene.ObjectsController.AddObjectToScene(new ParameterizedPoint());
        }

        private bool _clear = false;

        private void SetClear(object sender, RoutedEventArgs e)
        {
            _clear = !_clear;
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

        private void AddBezierCurveC0(object sender, RoutedEventArgs e)
        {
            scene.ObjectsController.AddObjectToScene(new BezierCurveC0());
            bezierCurveC0Control.PointsList.Items.Refresh();
        }

        private void AddBezierCurveC2(object sender, RoutedEventArgs e)
        {
            scene.ObjectsController.AddObjectToScene(new BezierCurveC2());
            bezierCurveC2Control.PointsList.Items.Refresh();
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
    }
}