
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Windows;
using MikCAD.BezierCurves;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.GeometryObjects;
using SharpSceneSerializer.DTOs.Types;
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

    public bool IsCollapsedPoint
    {
        get;
        init;
    }
    public ParameterizedPoint(string name="Punkt") : base(name)
    {
        parents = new List<ParameterizedObject>();
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

    private float[] _vertices = new float[3];

    private uint[] _indexArray = {0};
    public override void GenerateVertices()
    {
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * 3 * sizeof(float), _vertices,
            BufferUsageHint.StaticDraw);

        GL.BindVertexArray(_vao);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, 1 * sizeof(uint), _indexArray, BufferUsageHint.StaticDraw);

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
    
    public List<ParameterizedObject> parents { get; set; }
    public bool Deleted { get; set; } = false;
    public bool CanBeDeleted { get; set; } = true;

    public override void OnPositionUpdate()
    {
        foreach (var parent in parents)   
        {
            (parent as BezierCurveC2)?.ConvertBSplineToBernstein(false);
            (parent as InterpolatingBezierCurveC2)?.RecalculatePoints();
            (parent as GregoryPatch)?.ReconstructPatch();
        }
    }

    public override void OnDelete()
    {
        if (CanBeDeleted)
        {
            for (int i = 0; i < parents.Count;)
            {
                if(parents[i] is CompositeObject parent)
                    parent?.ProcessObject(this);
            }
        }
    }
    
    public override void PassToDrawProcessor(DrawProcessor drawProcessor,EyeEnum eye)
    {
        drawProcessor.ProcessObject(this, eye);
    }
    
    public static explicit operator SharpSceneSerializer.DTOs.GeometryObjects.Point(ParameterizedPoint point)
    {
        SharpSceneSerializer.DTOs.GeometryObjects.Point ret = new SharpSceneSerializer.DTOs.GeometryObjects.Point()
        {
            Id = point.Id,
            Name = point.Name,
            Position = new Float3(point.posX, point.posY, point.posZ)
        };
        
        return ret;
    }
    
    public static explicit operator ParameterizedPoint(SharpSceneSerializer.DTOs.GeometryObjects.Point point)
    {
        ParameterizedPoint ret = new ParameterizedPoint() {
            Name = point.Name,
            posX = point.Position.X,
            posY = point.Position.Y,
            posZ = point.Position.Z,
        };
        ret.Id = point.Id;
        
        return ret;
    }
}