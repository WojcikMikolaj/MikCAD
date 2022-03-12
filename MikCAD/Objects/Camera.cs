using OpenTK.Mathematics;

namespace MikCAD
{
    public class Camera
    {
        private Vector3 _position = new Vector3(0, 0, 5);
        private Vector3 _front = new Vector3(0, 0, -1);
        private Vector3 _up = new Vector3(0, 1, 0);

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