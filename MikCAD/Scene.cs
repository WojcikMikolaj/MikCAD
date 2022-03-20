using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;


namespace MikCAD
{
    public class Scene
    {
        public static Scene CurrentScene;
        public ObjectsController ObjectsController { get; private set; } = new ObjectsController();
        public Camera camera { get; set; }= new Camera();

        public Torus torus { get; set; }
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        internal Shader _shader;

        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _modelMatrix;

        public Scene()
        {
            CurrentScene = this;
        }
        public void Initialise(float width, float height)
        {
            GL.DepthRange(1.0f, 0.0f);
            camera.InitializeCamera();
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.PointSmooth);
            ObjectsController.AddObjectToScene(ObjectsController._pointer = new Pointer3D());
            //UpdatePVM();
        }

        public void UpdatePVM()
        {
            _projectionMatrix = camera.GetProjectionMatrix();
            _viewMatrix = camera.GetViewMatrix();
           // _modelMatrix = torus.GetModelMatrix();

            _shader.SetMatrix4("projectionMatrix", _projectionMatrix);
            _shader.SetMatrix4("viewMatrix", _viewMatrix);
           // _shader.SetMatrix4("modelMatrix", _modelMatrix);
            _shader.Use();
        }

        public void OnRenderFrame(bool raycast, int x, int y, bool clear = true)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);
            // GL.Flush();
            ObjectsController.DrawObjects(0,0);
            return;

            if(clear)
                ObjectsController.DrawObjects(0,0);
            if (raycast)
            {
                //CurrentScene.ObjectsController.DrawPoints();
                GL.Flush();
                GL.Finish();
                byte r = 0, g = 0, b = 0, r1=0, g1=0,b1=0;
                GL.ReadBuffer(ReadBufferMode.Back);
                GL.PixelStore(PixelStoreParameter.UnpackAlignment,1);
                GL.ReadPixels(x, y, 1, 1, PixelFormat.Red, PixelType.UnsignedByte, ref r);
                GL.ReadPixels(x, y, 1, 1, PixelFormat.Green, PixelType.UnsignedByte, ref g);
                GL.ReadPixels(x, y, 1, 1, PixelFormat.Blue, PixelType.UnsignedByte, ref b);
                GL.ReadBuffer(ReadBufferMode.Front);
                GL.PixelStore(PixelStoreParameter.UnpackAlignment,1);
                GL.ReadPixels(x, y, 1, 1, PixelFormat.Red, PixelType.UnsignedByte, ref r1);
                GL.ReadPixels(x, y, 1, 1, PixelFormat.Green, PixelType.UnsignedByte, ref g1);
                GL.ReadPixels(x, y, 1, 1, PixelFormat.Blue, PixelType.UnsignedByte, ref b1);
                MainWindow.current.Title = $"{r},{g},{b}    {r1},{g1},{b1}";
                raycast = false;
                // if (clear)
                //     GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                //              ClearBufferMask.StencilBufferBit);
            }


        }
        }
        
    
}