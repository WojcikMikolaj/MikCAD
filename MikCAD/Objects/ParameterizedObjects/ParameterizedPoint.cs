using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD;

public class ParameterizedPoint : ParameterizedObject
{
    public override bool RotationEnabled => false;
    public override bool ScaleEnabled => false;

    public ParameterizedPoint() : base("Punkt")
    {
        UpdateTranslationMatrix();
    }

    public override uint[] lines { get; }

    private Matrix4 _translationMatrix = Matrix4.Identity;

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

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        var vertices = new float[1 * Point.Size];
        var colors = new float[1 * Point.Size];

        vertices[0] = 0;
        vertices[1] = 0;
        vertices[2] = 0;


        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 3 * sizeof(float), vertices,
            BufferUsageHint.StaticDraw);

        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        var indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, 1 * sizeof(uint), new uint[] {0}, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
    }

    public override Matrix4 GetModelMatrix()
    {
        return _translationMatrix;
    }
}