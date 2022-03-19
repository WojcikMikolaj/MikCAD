using System;
using System.Collections.Generic;
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
            camera.InitializeCamera();
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.PointSmooth);
            _shader = new Shader("Shaders/Shader.vert", "Shaders/Shader.frag");
            UpdatePVM();
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

        public void OnRenderFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);
            UpdatePVM();
            ObjectsController.DrawObjects(0,0);
            //torus.GenerateVertices(0,0,out _vertexBufferObject , out _vertexArrayObject);
            
            //GL.BindVertexArray(_vertexArrayObject);
            //GL.DrawElements(PrimitiveType.Lines, torus._lines.Length, DrawElementsType.UnsignedInt, 0);
            //GL.DrawArrays(PrimitiveType.Lines, 0, _torus.VerticesCount);


            // OpenTK windows are what's known as "double-buffered". In essence, the window manages two buffers.
            // One is rendered to while the other is currently displayed by the window.
            // This avoids screen tearing, a visual artifact that can happen if the buffer is modified while being displayed.
            // After drawing, call this function to swap the buffers. If you don't, it won't display what you've rendered.
            //SwapBuffers();

            // And that's all you have to do for rendering! You should now see a yellow triangle on a black screen.
        }
        
    }
}