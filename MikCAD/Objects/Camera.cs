using GlmNet;

namespace MikCAD
{
    public class Camera
    {
        private vec3 _position = new vec3(0, 0, 5);
        private vec3 _front = new vec3(0,0, -1);
        private vec3 _up = new vec3(0,1, 0);

        private float _fov = 60;
        private float _width = 400;
        private float _height = 400;
        private float _near = 0.1f ;
        private float _far = 100;

        public mat4 GetViewMatrix()
        {
            return glm.lookAt(_position, _front, _up);
        }

        public mat4 GetProjectionMatrix()
        {
            return glm.perspectiveFov(glm.radians(_fov), _width, _height, _near, _far);
        }
    }
}