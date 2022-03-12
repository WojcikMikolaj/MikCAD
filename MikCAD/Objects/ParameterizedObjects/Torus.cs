using System;
using GlmNet;
using SharpGL;
using SharpGL.VertexBuffers;

namespace MikCAD
{
    public class Torus : IParameterizedObject
    {
        private Point[] _vertices;
        private float _R;

        public int VerticesCount => _vertices.Length;

        public Torus()
        {
            _R = 1;
            _r = 0.5f;
            _theta = 360;
            _phi = 45;
            CalculateVertices();
        }
        
        public float R
        {
            get => _R;
            set
            {
                _R = value;
                CalculateVertices();
            }
        }

        private float _r;

        public float r
        {
            get => _r;
            set
            {
                _r = value;
                CalculateVertices();
            }
        }

        private float _theta;

        public float Theta
        {
            get => _theta;
            set
            {
                _theta = value;
                CalculateVertices();
            }
        }

        private float _phi;

        public float Phi
        {
            get => _phi;
            set
            {
                _phi = value;
                CalculateVertices();
            }
        }

        private void CalculateVertices()
        {
            int planeStepsCount = (int) (360 / _phi);
            float phiStep = glm.radians(360.0f / planeStepsCount);

            int circleStepsCount = (int) (360 / _theta);
            float thetaStep = glm.radians(360.0f / circleStepsCount);

            _vertices = new Point[planeStepsCount * circleStepsCount];

            for (int i = 0; i < planeStepsCount; i++)
            {
                for (int j = 0; j < circleStepsCount; j++)
                {
                    _vertices[i * circleStepsCount + j] = new Point()
                    {
                        X = (R + r * glm.cos(j * thetaStep)) * glm.cos(i * phiStep),
                        Y = (R + r * glm.cos(j * thetaStep)) * glm.sin(i * phiStep),
                        Z = r * glm.sin(j * thetaStep)
                    };
                }
            }
        }


        public mat4 GetModelMatrix()
        {
            return mat4.identity();
            //return modelMatrix;
        }

        public void GenerateVertices(OpenGL gl, uint vertexAttributeLocation, uint normalAttributeLocation, out VertexBufferArray vertexBufferArray)
        {
            var vertices = new float[_vertices.Length * 3];
            var colors = new float[_vertices.Length * 3];
            
            for (int i = 0; i < _vertices.Length; i++)
            {
                vertices[i] = _vertices[i].X;
                vertices[i + 1] = _vertices[i].Y;
                vertices[i + 2] = _vertices[i].Z;

                colors[i] = 1.0f;
                colors[i + 1] = 0.0f;
                colors[i + 2] = 0.0f;
            }
            
            //  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);

            //  Create a vertex buffer for the vertex data.
            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(gl);
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, vertices, false, 3);

            //  Now do the same for the colour data.
            var colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(gl);
            colourDataBuffer.Bind(gl);
            colourDataBuffer.SetData(gl, 1, colors, false, 3);

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(gl);
        }

        public void CreateVertexNormalBuffer(OpenGL gl, uint vertexAttributeLocation, uint normalAttributeLocation)
        {
            throw new NotImplementedException();
        }

        public void CreateIndexBuffer(OpenGL gl)
        {
            throw new NotImplementedException();
        }
    }
}