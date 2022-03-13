using System.ComponentModel;
using System.Runtime.CompilerServices;
using MikCAD.Annotations;
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
                OnPropertyChanged(nameof(posX));
            }
        }

        public float posY
        {
            get => _position.Y;
            set
            {
                _position.Y = value;
                OnPropertyChanged(nameof(posY));
            }
        }

        public float posZ
        {
            get => _position.Z;
            set
            {
                _position.Z = value;
                OnPropertyChanged(nameof(posZ));
            }
        }

        public float rotX
        {
            get => _pitch;
            set
            {
                _pitch = value;
                OnPropertyChanged(nameof(rotX));
            }
        }

        public float rotY
        {
            get => _yaw;
            set
            {
                _yaw = value; 
                OnPropertyChanged(nameof(rotY));
            }
        }

        public float rotZ
        {
            get => _roll;
            set
            {
                _roll = value;
                OnPropertyChanged(nameof(rotZ));
            }
        }

        public float fov
        {
            get => _fov;
            set
            {
                _fov = MH.Max(MH.Min(value,179.9f), 60.0f);
                OnPropertyChanged(nameof(fov));
            }
        }

        public float near
        {
            get => _near;
            set
            {
                _near = MH.Max(MH.Min(value, _far-0.1f), 0.1f);
                OnPropertyChanged(nameof(near));
            }
        }

        public float far
        {
            get => _far;
            set
            {
                _far = MH.Max(value, _near+0.1f);
                OnPropertyChanged(nameof(far));
            }
        }

        private Vector3 _position = new Vector3(0, 0, 5);
        private Vector3 _front = new Vector3(0, 0, -1);
        private Vector3 _up = new Vector3(0, 1, 0);
        
        private float _pitch;
        private float _yaw = -90;
        private float _roll;

        private float _fov = 60;
        private float _width = 400;
        private float _height = 400;
        private float _near = 0.1f;
        private float _far = 100;

        public Matrix4 GetViewMatrix()
        {
            _front.X = (float)MH.Cos(MH.DegreesToRadians(_yaw)) * (float)MH.Cos(MH.DegreesToRadians(_pitch));
            _front.Y = (float)MH.Sin(MH.DegreesToRadians(_pitch));
            _front.Z = (float)MH.Sin(MH.DegreesToRadians(_yaw)) * (float)MH.Cos(MH.DegreesToRadians(_pitch));
            
            return Matrix4.LookAt(_position, _front + _position, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MH.DegreesToRadians(_fov), _width / _height, _near,
                _far);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}