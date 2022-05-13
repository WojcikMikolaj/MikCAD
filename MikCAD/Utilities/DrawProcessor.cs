using MikCAD.BezierCurves;
using MikCAD.BezierSurfaces;
using MikCAD.Objects;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD;

public class DrawProcessor
{
    private ObjectsController _controller;

    public DrawProcessor(ObjectsController controller)
    {
        _controller = controller;
    }

    public void ProcessObject(ParameterizedPoint point,EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        if (point.Draw)
        {
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
            GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
        }
    }

    public void ProcessObject(Torus torus,EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        GL.DrawElements(PrimitiveType.Lines, torus.lines.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void ProcessObject(InterpolatingBezierCurveC2 curveC2,EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        curveC2.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
        int indexBufferObject;
        Scene.CurrentScene._shader = _controller._standardObjectShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);

        if (curveC2.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
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
        Scene.CurrentScene._shader = _controller._interpolatingBezierCurveC2Shader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

        indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, curveC2._patches.Length * sizeof(uint),
            curveC2._patches, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        GL.DrawElements(PrimitiveType.Patches, curveC2.patches.Length, DrawElementsType.UnsignedInt,
            0);
    }
    
    public void ProcessObject(BezierCurveC2 curveC2,EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        curveC2.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
        int indexBufferObject;
        Scene.CurrentScene._shader = _controller._standardObjectShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
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
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
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
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
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
                    Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
                    point.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
                    Scene.CurrentScene._shader.SetMatrix4("modelMatrix", point.GetModelMatrix());
                    point.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
                    GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
                }
            }
        }
    }

    public void ProcessObject(BezierCurveC0 curveC0, EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        int indexBufferObject;
        if (curveC0.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
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
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

        indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, curveC0._patches.Length * sizeof(uint), curveC0._patches,
            BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        GL.DrawElements(PrimitiveType.Patches, curveC0.patches.Length, DrawElementsType.UnsignedInt, 0);
    }
    
    public void ProcessObject(BezierSurfaceC0 surfaceC0, EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        int indexBufferObject;
        if (surfaceC0.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
            Scene.CurrentScene._shader.SetVector4("color", Vector4.One);
            indexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, surfaceC0._lines.Length * sizeof(uint),
                surfaceC0._lines, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.DrawElements(PrimitiveType.Lines, surfaceC0.lines.Length, DrawElementsType.UnsignedInt, 0);
        }

        Scene.CurrentScene._shader = _controller._bezierSurfaceC0Shader;
        Scene.CurrentScene._shader.SetInt("UTessLevels", (int)surfaceC0.UDivisions);
        Scene.CurrentScene._shader.SetInt("VTessLevels", (int)surfaceC0.VDivisions);
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        GL.PatchParameter(PatchParameterInt.PatchVertices, 16);
        
        indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, surfaceC0._patches.Length * sizeof(uint), surfaceC0._patches, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Line);
        GL.DrawElements(PrimitiveType.Patches, surfaceC0.patches.Length, DrawElementsType.UnsignedInt, 0);
        GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Fill);
    }

    public void ProcessObject(CompositeObject compositeObject,EyeEnum eye, uint vertexAttributeLocation,
        uint normalAttributeLocation)
    {
        var _modelMatrix = compositeObject.GetModelMatrix();
        Scene.CurrentScene._shader = _controller._centerObjectShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
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

    public void ProcessObject(Pointer3D pointer3D,EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        Scene.CurrentScene._shader = _controller._pointerShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", pointer3D.GetModelMatrix());
        pointer3D.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
        GL.DrawElements(PrimitiveType.Lines, pointer3D.lines.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void DrawAxis(Axis selectedAxis,EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        var vertices = new float[6];
        var lines = new uint[] {0, 1};
        var selected = Scene.CurrentScene.ObjectsController.SelectedObject;
        Vector4 color = default;
        if (selected != null)
        {
            vertices[0] = vertices[3] = selected.posX;
            vertices[1] = vertices[4] = selected.posY;
            vertices[2] = vertices[5] = selected.posZ;
        }

        switch (selectedAxis)
        {
            case Axis.None:
                return;
            case Axis.X:
                vertices[0] += 100;
                vertices[3] -= 100;
                color = new Vector4(1, 0, 0, 1);
                break;
            case Axis.Y:
                vertices[1] += 100;
                vertices[4] -= 100;
                color = new Vector4(0, 1, 0, 1);
                break;
            case Axis.Z:
                vertices[2] += 100;
                vertices[5] -= 100;
                color = new Vector4(0, 1, 1, 1);
                break;
        }

        Scene.CurrentScene._shader = _controller._colorShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
        Scene.CurrentScene._shader.SetVector4("color", color);
        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        var indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, lines.Length * sizeof(uint), lines,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.DrawElements(PrimitiveType.Lines, lines.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void DrawGrid(Grid grid,EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        Scene.CurrentScene._shader = _controller._colorShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
        Scene.CurrentScene._shader.SetVector4("color", new Vector4(0.5f, 0.5f, 0.5f, 1));
        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, grid.vertices.Length * sizeof(float), grid.vertices,
            BufferUsageHint.StaticDraw);
        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        var indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, grid.lines.Length * sizeof(uint), grid.lines,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.DrawElements(PrimitiveType.Lines, grid.lines.Length, DrawElementsType.UnsignedInt, 0);
    }
}