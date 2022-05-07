using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MikCAD.Annotations;
using MikCAD.Objects;
using MikCAD.Utilities;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD
{
    public class Camera : INotifyPropertyChanged
    {
        public float posX
        {
            get => _position.X;
            set
            {
                _position.X = value;
                UpdateViewMatrices();
                OnPropertyChanged(nameof(posX));
            }
        }

        public float posY
        {
            get => _position.Y;
            set
            {
                _position.Y = value;
                UpdateViewMatrices();
                OnPropertyChanged(nameof(posY));
            }
        }

        public float posZ
        {
            get => _position.Z;
            set
            {
                _position.Z = value;
                UpdateViewMatrices();
                OnPropertyChanged(nameof(posZ));
            }
        }

        public float rotX
        {
            get => _pitch;
            set
            {
                _pitch = MH.Min(MH.Max(value, -89.9f), 89.9f);
                UpdateViewMatrices();
                OnPropertyChanged(nameof(rotX));
            }
        }

        public float rotY
        {
            get => _yaw;
            set
            {
                _yaw = value;
                UpdateViewMatrices();
                OnPropertyChanged(nameof(rotY));
            }
        }

        public float rotZ
        {
            get => _roll;
            set
            {
                _roll = value;
                UpdateViewMatrices();
                OnPropertyChanged(nameof(rotZ));
            }
        }

        public float fov
        {
            get => _fov;
            set
            {
                _fov = MH.Max(MH.Min(value, 179.9f), 60.0f);
                UpdateProjectionMatrices();
                OnPropertyChanged(nameof(fov));
            }
        }

        public float near
        {
            get => _near;
            set
            {
                _near = MH.Max(MH.Min(value, _far - 0.1f), 0.1f);
                UpdateProjectionMatrices();
                OnPropertyChanged(nameof(near));
            }
        }

        public float far
        {
            get => _far;
            set
            {
                _far = MH.Max(value, _near + 0.1f);
                UpdateProjectionMatrices();
                OnPropertyChanged(nameof(far));
            }
        }

        public float Scale
        {
            get => _scale;
            set
            {
                _scale = MH.Max(value, 0.1f);
                UpdateViewMatrices();
                OnPropertyChanged(nameof(Scale));
            }
        }

        public float IPD
        {
            get => _IPD;
            set
            {
                _IPD = value > 0.1f? value:0.1f;
                UpdateViewMatrices();
                UpdateProjectionMatrices();
            }
        }

        public float focusDistance
        {
            get => _focusDistance;
            set 
            { 
                _focusDistance = value > 0.1f? value:0.1f;
                UpdateViewMatrices();
                UpdateProjectionMatrices();
            }
        }

        public bool IsStereoEnabled { get; set; }

        public Vector3 WorldPosition => _position - ActForward * _scale;

        public void InitializeCamera()
        {
            UpdateProjectionMatrices();
            UpdateViewMatrices();
            if (grid == null)
                _grid = new Grid(this);
        }

        private Grid _grid = null;
        public Grid grid => _grid;

        internal Vector3 _position = new Vector3(0, 0, 0);
        private Vector3 _front = new Vector3(0, 0, -1);
        private Vector3 _up = new Vector3(0, 1, 0);
        public Vector3 Up => _up;
        internal Vector3 ActForward = new Vector3();

        private float _pitch;
        private float _yaw = -90;
        private float _roll;

        private float _fov = 60;
        internal float _width = 400;
        internal float _height = 400;
        private float _near = 0.1f;
        private float _far = 100;

        private float _scale = 5.0f;

        private float _IPD = 0.5f;
        private float _focusDistance = 10f;
        internal Vector4 leftColor = new Vector4(1,0,0,1);
        internal Vector4 rightColor = new Vector4(0,1,1,1);

        private Matrix4 _viewMatrix;
        private Matrix4 _projectionMatrix;

        private Matrix4 _leftViewMatrix;
        private Matrix4 _leftProjectionMatrix;

        private Matrix4 _rightViewMatrix;
        private Matrix4 _rightProjectionMatrix;

        public void UpdateViewMatrices()
        {
            UpdateViewMatrix();
            UpdateLeftViewMatrix();
            UpdateRightViewMatrix();
        }
        
        public void UpdateProjectionMatrices()
        {
            UpdateProjectionMatrix();
            UpdateLeftProjectionMatrix();
            UpdateRightProjectionMatrix();
        }
        
        public void UpdateViewMatrix()
        {
            _front.X = (float) MH.Cos(MH.DegreesToRadians(_yaw)) * (float) MH.Cos(MH.DegreesToRadians(_pitch));
            _front.Y = (float) MH.Sin(MH.DegreesToRadians(_pitch));
            _front.Z = (float) MH.Sin(MH.DegreesToRadians(_yaw)) * (float) MH.Cos(MH.DegreesToRadians(_pitch));
            ActForward = _front;
            _viewMatrix = Matrix4.LookAt(_position - _front * _scale, _position, _up);
            Scene.CurrentScene.ObjectsController?._pointer?.UpdateTranslationMatrix();
            _grid?.UpdateGrid();
        }

        public Matrix4 GetViewMatrix()
        {
            return _viewMatrix;
        }

        public void UpdateProjectionMatrix()
        {
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MH.DegreesToRadians(_fov), _width / _height, _near,
                _far);
            Scene.CurrentScene.ObjectsController?._pointer?.UpdateTranslationMatrix();
        }

        public Matrix4 GetProjectionMatrix()
        {
            return _projectionMatrix;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateLeftViewMatrix()
        {
            _front.X = (float) MH.Cos(MH.DegreesToRadians(_yaw)) * (float) MH.Cos(MH.DegreesToRadians(_pitch));
            _front.Y = (float) MH.Sin(MH.DegreesToRadians(_pitch));
            _front.Z = (float) MH.Sin(MH.DegreesToRadians(_yaw)) * (float) MH.Cos(MH.DegreesToRadians(_pitch));
            ActForward = _front;
            var right = Vector3.Cross(_up, _front).Normalized();
            _leftViewMatrix = Matrix4.LookAt(_position - _front * _scale + right * IPD / 2, _position + right * IPD / 2,
                _up);
            Scene.CurrentScene.ObjectsController?._pointer?.UpdateTranslationMatrix();
            _grid?.UpdateGrid();
        }

        public void UpdateLeftProjectionMatrix()
        {
            float left_right_direction = 1.0f;
            float aspect_ratio = _width / _height;
            float frustumshift = (IPD/2)*_near/_focusDistance;
            float top = (float)Math.Tan(MH.DegreesToRadians(_fov)/2)*_near;
            float right = aspect_ratio*top+frustumshift*left_right_direction;
            float left = -aspect_ratio*top+frustumshift*left_right_direction;
            float bottom = -top;
            _leftProjectionMatrix = Matrix4.CreatePerspectiveOffCenter(left, right, bottom, top, _near, _far);
            Scene.CurrentScene.ObjectsController?._pointer?.UpdateTranslationMatrix();
        }

        public Matrix4 GetLeftEyeViewMatrix()
        {
            return _leftViewMatrix;
        }

        public Matrix4 GetLeftEyeProjectionMatrix()
        {
            return _leftProjectionMatrix;
        }

        public void UpdateRightViewMatrix()
        {
            _front.X = (float) MH.Cos(MH.DegreesToRadians(_yaw)) * (float) MH.Cos(MH.DegreesToRadians(_pitch));
            _front.Y = (float) MH.Sin(MH.DegreesToRadians(_pitch));
            _front.Z = (float) MH.Sin(MH.DegreesToRadians(_yaw)) * (float) MH.Cos(MH.DegreesToRadians(_pitch));
            ActForward = _front;
            var right = Vector3.Cross(_up, _front).Normalized();
            _rightViewMatrix = Matrix4.LookAt(_position - _front * _scale - right * IPD / 2,
                _position - right * IPD / 2, _up);
            Scene.CurrentScene.ObjectsController?._pointer?.UpdateTranslationMatrix();
            _grid?.UpdateGrid();
        }

        public void UpdateRightProjectionMatrix()
        {
            float left_right_direction = -1.0f;
            float aspect_ratio = _width / _height;
            float frustumshift = (IPD/2)*_near/_focusDistance;
            float top = (float)Math.Tan(MH.DegreesToRadians(_fov)/2)*_near;
            float right = aspect_ratio*top+frustumshift*left_right_direction;
            float left = -aspect_ratio*top+frustumshift*left_right_direction;
            float bottom = -top;
            _rightProjectionMatrix = Matrix4.CreatePerspectiveOffCenter(left, right, bottom, top, _near, _far);
            Scene.CurrentScene.ObjectsController?._pointer?.UpdateTranslationMatrix();
        }

        public Matrix4 GetRightEyeViewMatrix()
        {
            return _rightViewMatrix;
        }

        public Matrix4 GetRightProjectionMatrix()
        {
            return _rightProjectionMatrix;
        }
    }
}