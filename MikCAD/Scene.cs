using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using MikCAD.Objects.ParameterizedObjects.Milling;
using MikCAD.Symulacje.RigidBody;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;


namespace MikCAD
{
    public class Scene
    {
        public static Scene CurrentScene;
        public Simulator3C Simulator3C { get; private set; } = new Simulator3C();
        public ObjectsController ObjectsController { get; private set; } = new ObjectsController();
        public Camera camera { get; set; } = new Camera();

        public Torus torus { get; set; }
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        internal Shader _shader;

        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _modelMatrix;

        public RigidBody RigidBody { get; private set; } = new RigidBody();
        
        public Scene()
        {
            CurrentScene = this;
        }

        public void Initialise(float width, float height)
        {
            //GL.DepthRange(1.0f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            camera.InitializeCamera();
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.PointSmooth);
            ObjectsController.AddObjectToScene(ObjectsController._pointer = new Pointer3D());
            ObjectsController.Path = new Path();
            ObjectsController.Path.rotX = -90;
            
            
            ObjectsController.Block = new Block();
            ObjectsController.Block.rotX = -90;

            var woodBmp = Bitmap.FromFile("./Textures/wood.jpg") as Bitmap;
            var texture = new List<byte>(4 * woodBmp.Width * woodBmp.Height);

            for (int j = 0; j < woodBmp.Height; j++)
            {
                for (int i = 0; i < woodBmp.Width; i++)
                {
                    var color = woodBmp.GetPixel(i, j);
                    texture.Add(color.R);
                    texture.Add(color.G);
                    texture.Add(color.B);
                    texture.Add(color.A);
                }
            }
            ObjectsController.Block.Texture0 = texture.ToArray();
            ObjectsController.Block.Tex0Width = woodBmp.Width;
            ObjectsController.Block.Tex0Height = woodBmp.Height;

            Simulator3C.UpdateBlockHeightMap();

            ObjectsController.Cutter = new Torus()
            {
                scaleY = 3,
                R = 0.1f,
                r = 0.1f,
                CirclesCount = 6,
                SectorsCount = 90,
            };
        }

        public void UpdatePVMAndStereoscopics(EyeEnum eye)
        {
            switch (eye)
            {
                case EyeEnum.Both:
                    _projectionMatrix = camera.GetProjectionMatrix();
                    _viewMatrix = camera.GetViewMatrix();
                    break;
                case EyeEnum.Left:
                    _projectionMatrix = camera.GetLeftEyeProjectionMatrix();
                    _viewMatrix = camera.GetLeftEyeViewMatrix();
                    break;
                case EyeEnum.Right:
                    _projectionMatrix = camera.GetRightProjectionMatrix();
                    _viewMatrix = camera.GetRightEyeViewMatrix();
                    break;
            }

            _shader.SetMatrix4("projectionMatrix", _projectionMatrix);
            _shader.SetMatrix4("viewMatrix", _viewMatrix);
            _shader.SetFloat("overrideEnabled", eye==EyeEnum.Both?0:1);
            switch (eye)
            {
                case EyeEnum.Left:
                    _shader.SetVector4("overrideColor", camera.leftColor);
                    break;
                case EyeEnum.Right:
                    _shader.SetVector4("overrideColor", camera.rightColor);
                    break;
            }
            _shader.Use();
        }

        public void OnRenderFrame(bool raycast, int x, int y, bool clear = true)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);
            if (camera.IsStereoEnabled)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                //GL.ColorMask(true, false,false,true);
                ObjectsController.DrawObjects(EyeEnum.Left,0,  0);
                //GL.ColorMask(true, true,true,true);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                //GL.ColorMask(false, true,true,true);
                ObjectsController.DrawObjects(EyeEnum.Right,0,  0);
                //GL.ColorMask(true, true,true,true);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
                //GL.ColorMask(true, true,true,true);
                ObjectsController.DrawObjects(EyeEnum.Both,0,  0);
            }
        }
    }
}