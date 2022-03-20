using System;
using System.Data;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD;

public class Pointer3D : ParameterizedObject
{
    public Pointer3D() : base("3D Pointer")
    {
    }

    public override string Name
    {
        get => "3D Pointer";
        set{}
    }

    private Matrix4 _translationMatrix = Matrix4.Identity;

    public override bool RotationEnabled => false;
    public override bool ScaleEnabled => false;
    public override uint[] lines => _lines;
    public override void UpdateTranslationMatrix()
    {
        _translationMatrix = Matrix4.CreateTranslation(_position);
    }

    public override void UpdateRotationMatrix(Axis axis)
    {
        
    }

    public override void UpdateScaleMatrix()
    {
        
    }
    
    private float[] _vertices = new float[]
    {
        0,0,0,
        1,0,0,
        0,1,0,
        0,0,1,
    };

    private uint[] _lines = new uint[]
    {
        0, 1,
        0, 2,
        0, 3,
    };
    
    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {

        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * 3 * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        var indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _lines.Length * sizeof(uint), _lines, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
    }

    public override Matrix4 GetModelMatrix()
    {
        return _translationMatrix;
    }

    public override Matrix4 GetOnlyModelMatrix()
    {
        return GetModelMatrix();
    }
}