using System.Globalization;
using System.Windows;
using System.Windows.Input;
using MikCAD.Annotations;

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

        private void AddTorus(object sender, RoutedEventArgs e)
        {
            scene.ObjectsController.AddObjectToScene(new Torus());
        }
        
        private void AddPoint(object sender, RoutedEventArgs e)
        {
            scene.ObjectsController.AddObjectToScene(new ParameterizedPoint());
        }
    }
}