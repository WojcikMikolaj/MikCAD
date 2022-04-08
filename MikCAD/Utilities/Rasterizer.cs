using MikCAD.BezierCurves;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MikCAD;

public class Rasterizer
{
    private ObjectsController _controller;

    public Rasterizer(ObjectsController controller)
    {
        _controller = controller;
    }

    public void RasterizeObject(ParameterizedPoint point, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        if (point.Draw)
            GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
    }

    public void RasterizeObject(Torus torus, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        GL.DrawElements(PrimitiveType.Lines, torus.lines.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void RasterizeObject(BezierCurveC2 curveC2, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        curveC2.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
        int indexBufferObject;
        Scene.CurrentScene._shader = _controller._standardObjectShader;
        Scene.CurrentScene.UpdatePVM();
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
        if (curveC2.Bernstein)
        {
            curveC2.GenerateVerticesForBernsteinPoints(vertexAttributeLocation,
                normalAttributeLocation);
            GL.DrawElements(PrimitiveType.Points, curveC2.BernsteinPoints.Count,
                DrawElementsType.UnsignedInt, 0);
        }

        if (curveC2.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVM();
            Scene.CurrentScene._shader.SetVector4("color", curveC2.CurveColor);
            indexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, curveC2._lines.Length * sizeof(uint),
                curveC2._lines, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.DrawElements(PrimitiveType.Lines, curveC2.lines.Length, DrawElementsType.UnsignedInt, 0);
        }
        
        curveC2.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
        Scene.CurrentScene._shader = _controller._bezierCurveC0Shader;
        Scene.CurrentScene._shader.SetInt("tessLevels", curveC2.tessLevel);
        Scene.CurrentScene.UpdatePVM();
        GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

        indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, curveC2._patches.Length * sizeof(uint),
            curveC2._patches, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        GL.DrawElements(PrimitiveType.Patches, curveC2.patches.Length, DrawElementsType.UnsignedInt,
            0);
        if (curveC2.Bernstein)
        {
            foreach (var point in curveC2.BernsteinPoints)
            {
                if (point.Selected)
                {
                    Scene.CurrentScene._shader = _controller._selectedObjectShader;
                    Scene.CurrentScene.UpdatePVM();
                    point.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
                    Scene.CurrentScene._shader.SetMatrix4("modelMatrix", point.GetModelMatrix());
                    point.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
                    GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
                }
            }
        }
    }

    public void RasterizeObject(BezierCurveC0 curveC0, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        int indexBufferObject;
        if (curveC0.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVM();
            Scene.CurrentScene._shader.SetVector4("color", curveC0.CurveColor);
            indexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, curveC0._lines.Length * sizeof(uint),
                curveC0._lines, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.DrawElements(PrimitiveType.Lines, curveC0.lines.Length, DrawElementsType.UnsignedInt, 0);
        }

        Scene.CurrentScene._shader = _controller._bezierCurveC0Shader;
        Scene.CurrentScene._shader.SetInt("tessLevels", curveC0.tessLevel);
        Scene.CurrentScene.UpdatePVM();
        GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

        indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, curveC0._patches.Length * sizeof(uint), curveC0._patches,
            BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        GL.DrawElements(PrimitiveType.Patches, curveC0.patches.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void RasterizeObject(CompositeObject compositeObject, uint vertexAttributeLocation,
        uint normalAttributeLocation)
    {
        var _modelMatrix = compositeObject.GetModelMatrix();
        Scene.CurrentScene._shader = _controller._centerObjectShader;
        Scene.CurrentScene.UpdatePVM();
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);

        #region bezierCurve

        if (compositeObject is BezierCurveC0 c)
            c.GenerateVerticesBase(vertexAttributeLocation, normalAttributeLocation);
        else if (compositeObject is BezierCurveC2 c2)
            c2.GenerateVerticesBase(vertexAttributeLocation, normalAttributeLocation);

        #endregion

        else
            compositeObject.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);

        GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
    }

    public void RasterizeObject(Pointer3D pointer3D, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        Scene.CurrentScene._shader = _controller._pointerShader;
        Scene.CurrentScene.UpdatePVM();
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", pointer3D.GetModelMatrix());
        pointer3D.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
        GL.DrawElements(PrimitiveType.Lines, pointer3D.lines.Length, DrawElementsType.UnsignedInt, 0);
    }
}