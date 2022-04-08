
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using MikCAD.BezierCurves;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD;

[DebuggerDisplay("{_position}")]
public class ParameterizedPoint : ParameterizedObject
{
    private static int _nextId;
    private static Random random = new Random(0);
    public readonly int PointId;
    private BoundingSphere _bb = new BoundingSphere()
    {
        radius = 0.25f
    };
    public BoundingSphere BB => _bb;
    public override bool RotationEnabled => false;
    public override bool ScaleEnabled => false;

    public ParameterizedPoint(string name="Punkt") : base(name)
    {
        parents = new List<CompositeObject>();
        UpdateTranslationMatrix();
        PointId = random.Next();
        _pickingColor = new Vector3((PointId & 0xFF )/255.0f, ((PointId >> 8) &  0xFF)/255.0f, ((PointId >>16 ) & 0xFF)/255.0f);
    }

    public override uint[] lines { get; }
    public Vector3 PickingColor => _pickingColor;

    private Matrix4 _translationMatrix = Matrix4.Identity;
    private Vector3 _pickingColor;

    public override void UpdateTranslationMatrix()
    {
        _translationMatrix = Matrix4.CreateTranslation(_position);
        _bb.position = _position;
    }

    public override void UpdateRotationMatrix(Axis axis)
    {
        _rotation[0] = 1;
        _rotation[1] = 1;
        _rotation[2] = 1;
    }

    public override void UpdateScaleMatrix()
    {
        _scale[0] = 1;
        _scale[1] = 1;
        _scale[2] = 1;
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
        return _translationMatrix * CompositeOperationMatrix;
    }
    
    public override Matrix4 GetOnlyModelMatrix()
    {
        return _translationMatrix;
    }
    
    public bool Draw { get; set; } = true;
    
    public List<CompositeObject> parents { get; set; }

    public override void OnPositionUpdate()
    {
        foreach (var parent in parents)   
        {
            (parent as BezierCurveC2)?.ConvertBSplineToBernstein(false);    
        }
    }

    public override void OnDelete()
    {
        foreach (var parent in parents)
        {
            parent?.ProcessObject(this);
        }
    }
    
    public override void Rasterize(Rasterizer rasterizer, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        rasterizer.RasterizeObject(this, vertexAttributeLocation, normalAttributeLocation);
    }
}