using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace MikCAD
{
    public class Scene
    {
        public Camera camera { get; set; }= new Camera();
        public Torus torus { get; set; } = new Torus()
        {
            //theta = 120
        };
        
        public float Scale
        {
            get
            {
                return _scale;
            }

            set
            {
                _scale = value;
                _sceneScaleMatrix = Matrix4.CreateScale(_scale);
            }
        }
        
        private float _scale = 1;
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private Shader _shader;

        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _modelMatrix;
        private Matrix4 _sceneScaleMatrix = Matrix4.Identity;

        public void Initialise(float width, float height)
        {
            camera.InitializeCamera();
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            
            
            torus.GenerateVertices(0,0,out _vertexBufferObject , out _vertexArrayObject);
            
            // _vertices = _torus.GetVertices();
            // _vertexBufferObject = GL.GenBuffer();
            // GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            // GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
            //     BufferUsageHint.StaticDraw);
            //
            // _vertexArrayObject = GL.GenVertexArray();
            // GL.BindVertexArray(_vertexArrayObject);
            //
            // GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            // GL.EnableVertexAttribArray(0);


            _shader = new Shader("Shaders/Shader.vert", "Shaders/Shader.frag");
            UpdatePVM();
        }

        public void UpdatePVM()
        {
            _projectionMatrix = camera.GetProjectionMatrix();
            _viewMatrix = camera.GetViewMatrix();
            _modelMatrix = torus.GetModelMatrix();

            _shader.SetMatrix4("projectionMatrix", _projectionMatrix);
            _shader.SetMatrix4("viewMatrix", _viewMatrix);
            _shader.SetMatrix4("modelMatrix", _modelMatrix);
            _shader.SetMatrix4("sceneScaleMatrix", _sceneScaleMatrix);
            
            _shader.Use();
        }

        public void OnRenderFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            torus.GenerateVertices(0,0,out _vertexBufferObject , out _vertexArrayObject);
            UpdatePVM();
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Lines, torus.lines.Length, DrawElementsType.UnsignedInt, 0);
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