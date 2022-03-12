using OpenTK.Mathematics;

namespace MikCAD
{
    public class Camera
    {
        public float posX
        {
            get => _position.X;
            set => _position.X = value;
        }
        public float posY
        {
            get => _position.Y;
            set => _position.Y = value;
        }
        public float posZ
        {
            get => _position.Z;
            set => _position.Z = value;
        }
        public float rotX
        {
            get => _pitch;
            set => _pitch = value;
        }
        public float rotY
        {
            get => _yaw;
            set => _yaw = value;
        }
        public float rotZ
        {
            get => _roll;
            set => _roll = value;
        }
        public float fov
        {
            get => _fov;
            set => _fov = value;
        }
        public float near
        {
            get => _near;
            set => _near = value;
        }
        public float far
        {
            get => _far;
            set => _far = value;
        }
        
        private Vector3 _position = new Vector3(0, 0, 5);
        private Vector3 _front = new Vector3(0, 0, -1);
        private Vector3 _up = new Vector3(0, 1, 0);
        
        private float _pitch;
        private float _yaw;
        private float _roll;

        private float _fov = 60;
        private float _width = 400;
        private float _height = 400;
        private float _near = 0.1f;
        private float _far = 100;

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(_position, _front, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov), _width / _height, _near,
                _far);
        }
    }
}