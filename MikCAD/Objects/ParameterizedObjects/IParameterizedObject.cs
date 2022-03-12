using System;
using SharpGL;
using SharpGL.VertexBuffers;

namespace MikCAD
{
    public interface IParameterizedObject
    {
        void GenerateVertices(OpenGL gl, uint vertexAttributeLocation, uint normalAttributeLocation, out VertexBufferArray vertexBufferArray);
        void CreateVertexNormalBuffer(OpenGL gl, uint vertexAttributeLocation, uint normalAttributeLocation);
        void CreateIndexBuffer(OpenGL gl);
    }
}