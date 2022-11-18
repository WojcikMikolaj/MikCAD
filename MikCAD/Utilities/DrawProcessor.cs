using System;
using MikCAD.BezierCurves;
using MikCAD.BezierSurfaces;
using MikCAD.Objects;
using MikCAD.Objects.ParameterizedObjects;
using MikCAD.Objects.ParameterizedObjects.Milling;
using MikCAD.Symulacje.RigidBody;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD;

public class DrawProcessor
{
    private ObjectsController _controller;
    private int pointerVBO = GL.GenBuffer();
    private int interpolatingCurveC2IBO = GL.GenBuffer();
    private int curveC2IBO = GL.GenBuffer();
    private int curveC0IBO = GL.GenBuffer();
    private int intersectionCurveIBO = GL.GenBuffer();
    private int surfaceC0IBO = GL.GenBuffer();
    private int surfaceC2IBO = GL.GenBuffer();
    private int gregoryIBO = GL.GenBuffer();

    private int pointerVAO = GL.GenVertexArray();
    private int pointerIBO = GL.GenBuffer();

    private int gridVBO = GL.GenBuffer();
    private int gridVAO = GL.GenVertexArray();
    private int gridIBO = GL.GenBuffer();

    private int selectionVBO = GL.GenBuffer();
    private int selectionVAO = GL.GenVertexArray();
    private int selectionIBO = GL.GenBuffer();


    private int pathsIBO = GL.GenBuffer();

    private int blockIBO = GL.GenBuffer();

    public DrawProcessor(ObjectsController controller)
    {
        _controller = controller;
    }

    public void ProcessObject(ParameterizedPoint point, EyeEnum eye)
    {
        if (point.Draw)
        {
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
            GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
        }
    }

    public void ProcessObject(Torus torus, EyeEnum eye)
    {
        Scene.CurrentScene._shader = _controller._torusShader;
        if (torus.Intersection != null)
        {
            Scene.CurrentScene._shader.SetFloat("useTexture", 1);
            Scene.CurrentScene._shader.SetFloat("ignoreBlack", torus.IgnoreBlack ? 1 : 0);
        }
        else
        {
            Scene.CurrentScene._shader.SetFloat("useTexture", 0);
        }

        torus.GenerateVertices();
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", torus.GetModelMatrix());
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        GL.DrawElements(PrimitiveType.Lines, torus.lines.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void ProcessObject(InterpolatingBezierCurveC2 curveC2, EyeEnum eye)
    {
        curveC2.GenerateVertices();
        Scene.CurrentScene._shader = _controller._standardObjectShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);

        if (curveC2.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
            Scene.CurrentScene._shader.SetVector4("color", curveC2.CurveColor);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, interpolatingCurveC2IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, curveC2._lines.Length * sizeof(uint),
                curveC2._lines, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.DrawElements(PrimitiveType.Lines, curveC2.lines.Length, DrawElementsType.UnsignedInt, 0);
        }

        curveC2.GenerateVertices();
        Scene.CurrentScene._shader = _controller._interpolatingBezierCurveC2Shader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, interpolatingCurveC2IBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, curveC2._patches.Length * sizeof(uint),
            curveC2._patches, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        GL.DrawElements(PrimitiveType.Patches, curveC2.patches.Length, DrawElementsType.UnsignedInt,
            0);
    }

    public void ProcessObject(BezierCurveC2 curveC2, EyeEnum eye)
    {
        curveC2.GenerateVertices();
        Scene.CurrentScene._shader = _controller._standardObjectShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
        if (curveC2.Bernstein)
        {
            curveC2.GenerateVerticesForBernsteinPoints();
            GL.DrawElements(PrimitiveType.Points, curveC2.BernsteinPoints.Count,
                DrawElementsType.UnsignedInt, 0);
        }

        if (curveC2.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
            Scene.CurrentScene._shader.SetVector4("color", curveC2.CurveColor);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, curveC2IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, curveC2._lines.Length * sizeof(uint),
                curveC2._lines, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.DrawElements(PrimitiveType.Lines, curveC2.lines.Length, DrawElementsType.UnsignedInt, 0);
        }

        curveC2.GenerateVertices();
        Scene.CurrentScene._shader = _controller._bezierCurveC0Shader;
        Scene.CurrentScene._shader.SetInt("tessLevels", curveC2.tessLevel);
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, curveC2IBO);
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
                    point.GenerateVertices();
                    Scene.CurrentScene._shader.SetMatrix4("modelMatrix", point.GetModelMatrix());
                    point.GenerateVertices();
                    GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
                }
            }
        }
    }

    public void ProcessObject(BezierCurveC0 curveC0, EyeEnum eye)
    {
        if (curveC0.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
            Scene.CurrentScene._shader.SetVector4("color", curveC0.CurveColor);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, curveC0IBO);
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

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, curveC0IBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, curveC0._patches.Length * sizeof(uint), curveC0._patches,
            BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        GL.DrawElements(PrimitiveType.Patches, curveC0.patches.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void ProcessObject(IntersectionCurve intersectionCurve, EyeEnum eye)
    {
        Scene.CurrentScene._shader = _controller._colorShader;
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetVector4("color", intersectionCurve.CurveColor);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, intersectionCurveIBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, intersectionCurve._lines.Length * sizeof(uint),
            intersectionCurve._lines, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.DrawElements(PrimitiveType.Lines, intersectionCurve.lines.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void ProcessObject(BezierSurfaceC0 surfaceC0, EyeEnum eye)
    {
        if (surfaceC0.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
            Scene.CurrentScene._shader.SetVector4("color", new Vector4(0, 1, 1, 1));
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, surfaceC0IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, surfaceC0._lines.Length * sizeof(uint),
                surfaceC0._lines, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.DrawElements(PrimitiveType.Lines, surfaceC0.lines.Length, DrawElementsType.UnsignedInt, 0);
        }

        Scene.CurrentScene._shader = _controller._bezierSurfaceC0Shader;
        Scene.CurrentScene._shader.SetInt("UTessLevels", (int) surfaceC0.UDivisions);
        Scene.CurrentScene._shader.SetInt("VTessLevels", (int) surfaceC0.VDivisions);
        Scene.CurrentScene._shader.SetInt("HorizontalPatchesCount", (int) surfaceC0.UPatches);
        Scene.CurrentScene._shader.SetInt("VerticalPatchesCount", (int) surfaceC0.VPatches);
        if (surfaceC0.Intersection != null)
        {
            Scene.CurrentScene._shader.SetFloat("useTexture", 1);
            Scene.CurrentScene._shader.SetFloat("ignoreBlack", surfaceC0.IgnoreBlack ? 1 : 0);
        }
        else
        {
            Scene.CurrentScene._shader.SetFloat("useTexture", 0);
        }

        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        GL.PatchParameter(PatchParameterInt.PatchVertices, 16);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, surfaceC0IBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, surfaceC0._patches.Length * sizeof(uint), surfaceC0._patches,
            BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        GL.DrawElements(PrimitiveType.Patches, surfaceC0.patches.Length, DrawElementsType.UnsignedInt, 0);
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    }

    public void ProcessObject(BezierSurfaceC2 surfaceC2, EyeEnum eye)
    {
        if (surfaceC2.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
            Scene.CurrentScene._shader.SetVector4("color", new Vector4(0, 1, 1, 1));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, surfaceC2IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, surfaceC2._lines.Length * sizeof(uint),
                surfaceC2._lines, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.DrawElements(PrimitiveType.Lines, surfaceC2.lines.Length, DrawElementsType.UnsignedInt, 0);
        }

        Scene.CurrentScene._shader = _controller._bezierSurfaceC2Shader;
        Scene.CurrentScene._shader.SetInt("UTessLevels", (int) surfaceC2.UDivisions);
        Scene.CurrentScene._shader.SetInt("VTessLevels", (int) surfaceC2.VDivisions);
        Scene.CurrentScene._shader.SetInt("HorizontalPatchesCount", (int) surfaceC2.UPatches);
        Scene.CurrentScene._shader.SetInt("VerticalPatchesCount", (int) surfaceC2.VPatches);
        if (surfaceC2.Intersection != null)
        {
            Scene.CurrentScene._shader.SetFloat("useTexture", 1);
            Scene.CurrentScene._shader.SetFloat("ignoreBlack", surfaceC2.IgnoreBlack ? 1 : 0);
        }
        else
        {
            Scene.CurrentScene._shader.SetFloat("useTexture", 0);
        }

        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        GL.PatchParameter(PatchParameterInt.PatchVertices, 16);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, surfaceC2IBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, surfaceC2._patches.Length * sizeof(uint), surfaceC2._patches,
            BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        GL.DrawElements(PrimitiveType.Patches, surfaceC2.patches.Length, DrawElementsType.UnsignedInt, 0);
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    }

    public void ProcessObject(GregoryPatch gregoriusz, EyeEnum eye)
    {
        if (gregoriusz.DrawPolygon)
        {
            Scene.CurrentScene._shader = _controller._colorShader;
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
            Scene.CurrentScene._shader.SetVector4("color", new Vector4(0, 1, 1, 1));
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, gregoryIBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, gregoriusz.lines.Length * sizeof(uint),
                gregoriusz.lines, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.DrawElements(PrimitiveType.Lines, gregoriusz.lines.Length, DrawElementsType.UnsignedInt, 0);
        }

        Scene.CurrentScene._shader = _controller._gregoryPatchShader;
        Scene.CurrentScene._shader.SetInt("UTessLevels", (int) gregoriusz.UDivisions);
        Scene.CurrentScene._shader.SetInt("VTessLevels", (int) gregoriusz.VDivisions);
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        GL.PatchParameter(PatchParameterInt.PatchVertices, 20);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, gregoryIBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, gregoriusz.patches.Length * sizeof(uint), gregoriusz.patches,
            BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        GL.DrawElements(PrimitiveType.Patches, gregoriusz.patches.Length, DrawElementsType.UnsignedInt, 0);
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    }

    public void ProcessObject(Path path, EyeEnum eye)
    {
        var _modelMatrix = path.GetModelMatrix();
        Scene.CurrentScene._shader = _controller._colorShader;
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetVector4("color", new Vector4(1, 0, 0, 1));

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, pathsIBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, path._lines.Length * sizeof(uint),
            path._lines, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.DrawElements(PrimitiveType.Lines, path.lines.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void ProcessObject(Block block, EyeEnum eye)
    {
        GL.Disable(EnableCap.CullFace);
        var _modelMatrix = block.GetModelMatrix();
        Scene.CurrentScene._shader = _controller._blockShader;
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetVector4("color", new Vector4(1, 0, 0, 1));
        Scene.CurrentScene._shader.SetVector4("cameraPos", new Vector4(
            Scene.CurrentScene.camera.posX,
            Scene.CurrentScene.camera.posY,
            Scene.CurrentScene.camera.posZ, 1));

        Scene.CurrentScene._shader.SetVector4("LightPos", new Vector4(
            Simulator3C.Simulator.LightPosX,
            Simulator3C.Simulator.LightPosY,
            Simulator3C.Simulator.LightPosZ,
            1));
        
        Scene.CurrentScene._shader.SetVector3("LightColor", new Vector3(
            Simulator3C.Simulator.LightColorR,
            Simulator3C.Simulator.LightColorG,
            Simulator3C.Simulator.LightColorB));

        Scene.CurrentScene._shader.SetVector3("LightAmbient", new Vector3(
            Simulator3C.Simulator.AmbientColorR,
            Simulator3C.Simulator.AmbientColorG,
            Simulator3C.Simulator.AmbientColorB));
        
        Scene.CurrentScene._shader.SetFloat("showNormals", Simulator3C.Simulator.ShowNormals ? 1: 0);

        Scene.CurrentScene._shader.SetFloat("ka", Simulator3C.Simulator.ka);
        Scene.CurrentScene._shader.SetFloat("ks", Simulator3C.Simulator.ks);
        Scene.CurrentScene._shader.SetFloat("kd", Simulator3C.Simulator.kd);
        Scene.CurrentScene._shader.SetFloat("m", Simulator3C.Simulator.m);

        Scene.CurrentScene._shader.SetInt("texture0", 0);
        Scene.CurrentScene._shader.SetInt("texture1", 1);

        if (block.Texture0 != null)
        {
            Scene.CurrentScene._shader.SetFloat("useTexture", 1);
        }

        // GL.BindBuffer(BufferTarget.ElementArrayBuffer, blockIBO);
        // GL.BufferData(BufferTarget.ElementArrayBuffer, block._lines.Length * sizeof(uint),
        //     block._lines, BufferUsageHint.StaticDraw);
        // GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        // GL.EnableVertexAttribArray(1);
        GL.DrawElements(PrimitiveType.Triangles, block.TrianglesIndices.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void ProcessObject(CompositeObject compositeObject, EyeEnum eye)
    {
        var _modelMatrix = compositeObject.GetModelMatrix();
        Scene.CurrentScene._shader = _controller._centerObjectShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);

        #region bezierCurve

        if (compositeObject is BezierCurveC0 c)
            c.GenerateVerticesBase();
        else if (compositeObject is BezierCurveC2 c2)
            c2.GenerateVerticesBase();

        #endregion

        else
            compositeObject.GenerateVertices();

        GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
    }

    public void ProcessObject(Pointer3D pointer3D, EyeEnum eye)
    {
        Scene.CurrentScene._shader = _controller._pointerShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", pointer3D.GetModelMatrix());
        pointer3D.GenerateVertices();
        GL.DrawElements(PrimitiveType.Lines, pointer3D.lines.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void DrawAxis(Axis selectedAxis, EyeEnum eye)
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
        GL.BindBuffer(BufferTarget.ArrayBuffer, pointerVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        GL.BindVertexArray(pointerVAO);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, pointerIBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, lines.Length * sizeof(uint), lines,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.DrawElements(PrimitiveType.Lines, lines.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void DrawGrid(Grid grid, EyeEnum eye)
    {
        Scene.CurrentScene._shader = _controller._colorShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
        Scene.CurrentScene._shader.SetVector4("color", new Vector4(0.5f, 0.5f, 0.5f, 1));
        GL.BindBuffer(BufferTarget.ArrayBuffer, gridVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, grid.vertices.Length * sizeof(float), grid.vertices,
            BufferUsageHint.StaticDraw);
        GL.BindVertexArray(gridVAO);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, gridIBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, grid.lines.Length * sizeof(uint), grid.lines,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.DrawElements(PrimitiveType.Lines, grid.lines.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void DrawSelectionBox(SelectionBox selectionBox)
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Scene.CurrentScene._shader = _controller._selectionBoxShader;
        Scene.CurrentScene._shader.Use();
        GL.BindBuffer(BufferTarget.ArrayBuffer, selectionVBO);
        var quad = new float[]
        {
            MathF.Min(selectionBox.X1, selectionBox.X2), MathF.Min(selectionBox.Y1, selectionBox.Y2), 0.1f,
            MathF.Max(selectionBox.X1, selectionBox.X2), MathF.Min(selectionBox.Y1, selectionBox.Y2), 0.1f,
            MathF.Max(selectionBox.X1, selectionBox.X2), MathF.Max(selectionBox.Y1, selectionBox.Y2), 0.1f,
            MathF.Min(selectionBox.X1, selectionBox.X2), MathF.Max(selectionBox.Y1, selectionBox.Y2), 0.1f,
        };
        GL.BufferData(BufferTarget.ArrayBuffer, quad.Length * sizeof(float), quad, BufferUsageHint.StaticDraw);
        GL.BindVertexArray(selectionVAO);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, selectionIBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, 4 * sizeof(uint), new[] {0, 1, 2, 3},
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
        GL.DrawElements(PrimitiveType.Quads, 4, DrawElementsType.UnsignedInt, 0);
    }

    public void ProcessObject(RigidBody rigidBody, EyeEnum eye)
    {
        GL.Disable(EnableCap.CullFace);
        var _modelMatrix = rigidBody.GetModelMatrix();
        Scene.CurrentScene._shader = _controller._colorShader;
        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
        Scene.CurrentScene._shader.SetVector4("color", new Vector4(1, 0, 0, 0.7f));

        if (rigidBody.DrawCube)
        {
            GL.DrawElements(PrimitiveType.Triangles, rigidBody.TrianglesIndices.Length, DrawElementsType.UnsignedInt,
                0);
        }

        if (rigidBody.DrawDiagonal)
        {
            rigidBody.GenerateIndicesForDiagonal();
            Scene.CurrentScene._shader.SetVector4("color", new Vector4(0, 1, 1, 1));
            GL.DrawElements(PrimitiveType.Lines, rigidBody.DiagonalLineIndices.Length, DrawElementsType.UnsignedInt,
                0);
        }

        if (rigidBody.DrawPath)
        {
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene._shader.SetVector4("color", new Vector4(0, 1, 1, 1));
            rigidBody.GeneratePathVertices();
            GL.DrawElements(PrimitiveType.Lines, rigidBody.PathLinesIndices.Length, DrawElementsType.UnsignedInt,
                0);
        }

        if (rigidBody.DrawPlane)
        {
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene._shader.SetVector4("color", new Vector4(1, 1, 1, 0.5f));
            rigidBody.GeneratePlaneVerticesAndIndices();
            GL.DrawElements(PrimitiveType.Triangles, rigidBody.PlaneTrianglesIndices.Length, DrawElementsType.UnsignedInt,
                0);
        }
        
        if (rigidBody.DrawGravityVector)
        {
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
            Scene.CurrentScene._shader.SetVector4("color", new Vector4(1, 1, 0, 1));
            rigidBody.GenerateGravityVectorVertices();
            GL.DrawElements(PrimitiveType.Lines, rigidBody.PlaneTrianglesIndices.Length, DrawElementsType.UnsignedInt,
                0);
        }
    }
}