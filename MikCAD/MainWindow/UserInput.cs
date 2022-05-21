using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using MikCAD.Annotations;
using MikCAD.BezierCurves;
using MikCAD.Utilities;
using OpenTK.Mathematics;

namespace MikCAD
{
    public partial class MainWindow
    {
        private bool _rightPressed = false;
        private bool _leftPressed = false;
        private System.Windows.Point? _last;
        private bool _boxSelection = false;
        private (int x, int y) _firstCorner;
        private bool _shiftPressed;
        public Axis ActiveAxis { get; private set; } = 0;

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
                ParameterizedObject point = null;
                if (IsObjectFreeMoveEnabled)
                {
                    var pos = Raycaster.FindPointOnCameraPlain((float) e.GetPosition(OpenTkControl).X,
                        (float) e.GetPosition(OpenTkControl).Y);
                    scene.ObjectsController.AddObjectToScene(point = new ParameterizedPoint()
                    {
                        posX = pos.X,
                        posY = pos.Y,
                        posZ = pos.Z,
                    });
                    return;
                }
                else
                {
                    point = Raycaster.FindIntersectingPoint((float) e.GetPosition(OpenTkControl).X,
                        (float) e.GetPosition(OpenTkControl).Y);
                }

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
                        else if (Scene.CurrentScene.ObjectsController.SelectedObject is InterpolatingBezierCurveC2
                                 interpolatingBezierCurveC2)
                        {
                            interpolatingBezierCurveC2.ProcessObject(point);
                            interpolatingBezierCurveC2Control.PointsList.Items.Refresh();
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


            if (!_rightPressed && !_shiftPressed)
            {
                _leftPressed = true;
            }

            if (e.LeftButton == MouseButtonState.Pressed && _shiftPressed)
            {
                _boxSelection = true;
                _firstCorner = ((int) e.GetPosition(OpenTkControl).X, (int) e.GetPosition(OpenTkControl).Y);

                Scene.CurrentScene.ObjectsController.SelectionBox.Draw = true;
                Scene.CurrentScene.ObjectsController.SelectionBox.X1 =
                    ((float) (_firstCorner.x / OpenTkControl.ActualWidth) - 0.5f) * 2;
                Scene.CurrentScene.ObjectsController.SelectionBox.Y1 =
                    -((float) (_firstCorner.y / OpenTkControl.ActualHeight) - 0.5f) * 2;

                (int x, int y) lastCorner = ((int) e.GetPosition(OpenTkControl).X,
                    (int) e.GetPosition(OpenTkControl).Y);
                Scene.CurrentScene.ObjectsController.SelectionBox.X2 =
                    ((float) (lastCorner.x / OpenTkControl.ActualWidth) - 0.5f) * 2;
                Scene.CurrentScene.ObjectsController.SelectionBox.Y2 =
                    -((float) (lastCorner.y / OpenTkControl.ActualHeight) - 0.5f) * 2;
            }
        }

        private void Image_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_boxSelection && _shiftPressed)
            {
                (int x, int y) lastCorner = ((int) e.GetPosition(OpenTkControl).X,
                    (int) e.GetPosition(OpenTkControl).Y);
                Scene.CurrentScene.ObjectsController.SelectionBox.X2 =
                    ((float) (lastCorner.x / OpenTkControl.ActualWidth) - 0.5f) * 2;
                Scene.CurrentScene.ObjectsController.SelectionBox.Y2 =
                    -((float) (lastCorner.y / OpenTkControl.ActualHeight) - 0.5f) * 2;
                Scene.CurrentScene.ObjectsController.SelectPointsInSelectionBox();
                Title = $"{_firstCorner},    ,{lastCorner}";
                Scene.CurrentScene.ObjectsController.SelectionBox.Draw = false;
            }

            _boxSelection = false;
            _leftPressed = false;
            _last = null;
        }

        private void Image_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_shiftPressed && (_leftPressed || _rightPressed))
            {
                var pos = e.GetPosition(this);
                if (_last.HasValue)
                {
                    var dx = (float) (pos.X - _last.Value.X);
                    var dy = (float) (pos.Y - _last.Value.Y);

                    //Title = $"{dx}, {dy}";

                    switch (ActiveAxis)
                    {
                        case Axis.None:
                            if (IsObjectFreeMoveEnabled)
                            {
                                var obj = Scene.CurrentScene.ObjectsController.SelectedObject;
                                if (obj is not IBezierCurve)
                                {
                                    var yVec = Scene.CurrentScene.camera.Up;
                                    var xVec = (Scene.CurrentScene.camera.GetViewMatrix() * new Vector4(1, 0, 0, 0))
                                        .Xyz;
#if DEBUG
                                    Title = $"Vec1: {yVec.X}, {yVec.Y}, {yVec.Z}. Vec3: {xVec.X}, {xVec.Y}, {xVec.Z}.";
#endif
                                    var vec = -yVec * dy + xVec * dx;
                                    obj.posX += vec.X * 0.005f;
                                    obj.posY += vec.Y * 0.005f;
                                    obj.posZ += vec.Z * 0.005f;
                                }
                            }
                            else if (_leftPressed)
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

                            break;
                        case Axis.X:
                            if (Scene.CurrentScene.ObjectsController.SelectedObject != null)
                                Scene.CurrentScene.ObjectsController.SelectedObject.posX +=
                                    MathM.AbsMax(dx, -dy) * 0.01f;
                            break;
                        case Axis.Y:
                            if (Scene.CurrentScene.ObjectsController.SelectedObject != null)
                                Scene.CurrentScene.ObjectsController.SelectedObject.posY +=
                                    MathM.AbsMax(dx, -dy) * 0.01f;
                            break;
                        case Axis.Z:
                            if (Scene.CurrentScene.ObjectsController.SelectedObject != null)
                                Scene.CurrentScene.ObjectsController.SelectedObject.posZ +=
                                    MathM.AbsMax(dx, -dy) * 0.01f;
                            break;
                    }
                }

                _last = pos;
            }

            if (e.LeftButton == MouseButtonState.Pressed && _shiftPressed)
            {
                _boxSelection = true;
                (int x, int y) lastCorner = ((int) e.GetPosition(OpenTkControl).X,
                    (int) e.GetPosition(OpenTkControl).Y);
                Title =
                    $"{(float) (_firstCorner.y / OpenTkControl.ActualHeight)} {(float) (_firstCorner.y / OpenTkControl.ActualHeight)},    ,{(float) (lastCorner.x / OpenTkControl.ActualWidth)} {(float) (lastCorner.y / OpenTkControl.ActualHeight)}";
                Scene.CurrentScene.ObjectsController.SelectionBox.Draw = true;
                Scene.CurrentScene.ObjectsController.SelectionBox.X2 =
                    ((float) (lastCorner.x / OpenTkControl.ActualWidth) - 0.5f) * 2;
                Scene.CurrentScene.ObjectsController.SelectionBox.Y2 =
                    -((float) (lastCorner.y / OpenTkControl.ActualHeight) - 0.5f) * 2;
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
        public static bool IsObjectFreeMoveEnabled { get; private set; }
        public static bool AddToSelected { get; private set; }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    AddToSelected = true;
                    break;
                case Key.Q:
                    ActiveAxis = Axis.X;
                    break;
                case Key.W:
                    ActiveAxis = Axis.Y;
                    break;
                case Key.E:
                    ActiveAxis = Axis.Z;
                    break;
                case Key.LeftCtrl:
                    IsMultiSelectEnabled = true;
                    break;
                case Key.System:
                    if (e.SystemKey == Key.LeftAlt)
                        IsObjectFreeMoveEnabled = true;
                    if (e.SystemKey == Key.Escape)
                        Scene.CurrentScene.ObjectsController.UnselectAll();
                    break;
                case Key.Escape:
                    Scene.CurrentScene.ObjectsController.UnselectAll();
                    break;
                case Key.Delete:
                    Scene.CurrentScene.ObjectsController.DeleteSelectedObjects();
                    break;
                case Key.LeftShift:
                    _shiftPressed = true;
                    break;
            }
        }

        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftCtrl:
                    IsMultiSelectEnabled = false;
                    break;
                case Key.System:
                    if (e.SystemKey == Key.LeftAlt)
                        IsObjectFreeMoveEnabled = false;
                    break;
                case Key.LeftAlt:
                    IsObjectFreeMoveEnabled = false;
                    break;
                case Key.A:
                    AddToSelected = false;
                    break;
                case Key.Q:
                case Key.W:
                case Key.E:
                    ActiveAxis = Axis.None;
                    break;
                case Key.LeftShift:
                    _shiftPressed = false;
                    break;
            }
        }
    }
}