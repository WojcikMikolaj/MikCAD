using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MikCAD.Annotations;
using MikCAD.BezierCurves;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MikCAD;

public class ObjectsController : INotifyPropertyChanged
{
    private ParameterizedObject _selectedObject;
    internal Pointer3D _pointer;
    private Shader _selectedObjectShader = new Shader("Shaders/SelectedObject.vert", "Shaders/SelectedObject.frag");
    private Shader _standardObjectShader = new Shader("Shaders/Shader.vert", "Shaders/Shader.frag");
    private Shader _pointerShader = new Shader("Shaders/PointerShader.vert", "Shaders/PointerShader.frag");
    private Shader _pickingShader = new Shader("Shaders/PickingShader.vert", "Shaders/PickingShader.frag");
    private Shader _colorShader = new Shader("Shaders/ColorShader.vert", "Shaders/ColorShader.frag");

    private Shader _centerObjectShader =
        new Shader("Shaders/CenterObjectShader.vert", "Shaders/CenterObjectShader.frag");

    private Shader _bezierCurveC0Shader = new Shader(
        "Shaders/BezierShader.vert",
        "Shaders/BezierShader.frag",
        "Shaders/BezierCurveC0TessControlShader.tesc",
        "Shaders/BezierCurveC0TessEvaluationShader.tese");

    private Vector4 curveColor = new Vector4(0.3f, 1f, 0.6f, 1f);

    public ParameterizedObject SelectedObject
    {
        get => _selectedObject;
        set
        {
            MainWindow.current.torusControl.Visibility = Visibility.Hidden;
            MainWindow.current.pointControl.Visibility = Visibility.Hidden;
            MainWindow.current.pointerControl.Visibility = Visibility.Hidden;
            MainWindow.current.bezierCurveC0Control.Visibility = Visibility.Hidden;
            MainWindow.current.bezierCurveC2Control.Visibility = Visibility.Hidden;
            _selectedObject = value;
            if (_selectedObject is null)
            {
                return;
            }

            switch (_selectedObject)
            {
                case Torus torus:
                    MainWindow.current.torusControl.Visibility = Visibility.Visible;
                    break;
                //must be before CompositeObject
                case BezierCurveC0 bezierCurveC0:
                    MainWindow.current.bezierCurveC0Control.Visibility = Visibility.Visible;
                    break;
                case BezierCurveC2 bezierCurveC2:
                    MainWindow.current.bezierCurveC2Control.Visibility = Visibility.Visible;
                    break;
                case ParameterizedPoint point:
                case CompositeObject compositeObject:
                    MainWindow.current.pointControl.Visibility = Visibility.Visible;
                    break;
                case Pointer3D pointer3D:
                    MainWindow.current.pointerControl.Visibility = Visibility.Visible;
                    break;
            }

            OnPropertyChanged(nameof(SelectedObject));
        }
    }

    public ObservableCollection<ParameterizedObject> ParameterizedObjects { get; private set; } =
        new ObservableCollection<ParameterizedObject>();

    private List<ParameterizedPoint> _parameterizedPoints = new List<ParameterizedPoint>();
    public List<ParameterizedPoint> ParameterizedPoints => _parameterizedPoints;


    public bool AddObjectToScene(ParameterizedObject parameterizedObject)
    {
        ParameterizedObjects.Add(parameterizedObject);
        switch (parameterizedObject)
        {
            case ParameterizedPoint point:
            {
                _parameterizedPoints.Add(point);

                #region bezierCurve

                if (SelectedObject is BezierCurveC0 curveC0)
                {
                    curveC0.ProcessObject(parameterizedObject);
                    MainWindow.current.bezierCurveC0Control.PointsList.Items.Refresh();
                }
                else if (SelectedObject is BezierCurveC2 curveC2)
                {
                    curveC2.ProcessObject(parameterizedObject);
                    MainWindow.current.bezierCurveC2Control.PointsList.Items.Refresh();
                }

                #endregion

                break;
            }

            #region bezierCurve

            case BezierCurveC0 curveC0:
            {
                switch (SelectedObject)
                {
                    case ParameterizedPoint point:
                        curveC0.ProcessPoint(point);
                        break;
                    case CompositeObject compositeObject:
                        compositeObject.Selected = false;
                        curveC0.ProcessObject(compositeObject);
                        break;
                }

                SelectedObject = parameterizedObject;
                break;
            }
            case BezierCurveC2 curveC2:
            {
                switch (SelectedObject)
                {
                    case ParameterizedPoint point:
                        curveC2.ProcessPoint(point);
                        break;
                    case CompositeObject compositeObject:
                        compositeObject.Selected = false;
                        curveC2.ProcessObject(compositeObject);
                        break;
                }

                SelectedObject = parameterizedObject;
                break;
            }

            #endregion
        }

        SelectedObject ??= parameterizedObject;
        return true;
    }

    public bool DeleteSelectedObjects()
    {
        if (_selectedObject is Pointer3D)
            return false;
        if (_selectedObject is ParameterizedPoint point)
            _parameterizedPoints.Remove(point);

        #region bezierCurve

        if (_selectedObject is BezierCurveC0 curve)
        {
            foreach (var o in curve._objects)
            {
                o.Selected = false;
            }

            curve.Selected = false;
            ParameterizedObjects.Remove(curve);
            SelectedObject = null;
            return true;
        }

        if (_selectedObject is BezierCurveC2 curve2)
        {
            foreach (var o in curve2._objects)
            {
                o.Selected = false;
            }

            curve2.Selected = false;
            ParameterizedObjects.Remove(curve2);
            SelectedObject = null;
            return true;
        }

        #endregion

        List<ParameterizedObject> objectsToDelete = new List<ParameterizedObject>();
        foreach (var o in ParameterizedObjects)
        {
            if (o.Selected)
                objectsToDelete.Add(o);
            o.Selected = false;
        }

        foreach (var o in objectsToDelete)
        {
            ParameterizedObjects.Remove(o);
        }

        SelectedObject = null;
        return true;
    }

    public void SelectObject(ParameterizedObject o)
    {
        #region bezierCurve

        if (MainWindow.AddToSelected && SelectedObject is BezierCurveC0 curveC0)
        {
            curveC0.ProcessObject(o);
            MainWindow.current.bezierCurveC0Control.PointsList.Items.Refresh();
            return;
        }

        if (MainWindow.AddToSelected && SelectedObject is BezierCurveC2 curveC2)
        {
            curveC2.ProcessObject(o);
            MainWindow.current.bezierCurveC2Control.PointsList.Items.Refresh();
            return;
        }

        #endregion

        if (!MainWindow.IsMultiSelectEnabled || o is Pointer3D)
        {
            if (_selectedObject != null)
                SelectedObject.Selected = false;
            SelectedObject = o;
            o.Selected = true;
        }
        else
        {
            if (SelectedObject is CompositeObject compositeObject)
            {
                compositeObject.ProcessObject(o);
            }
            else
            {
                var cmp = new CompositeObject(SelectedObject);
                cmp.ProcessObject(o);
                SelectedObject = cmp;
                o.Selected = true;
            }
        }
    }

    public void DrawObjects(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        foreach (var obj in ParameterizedObjects)
        {
            if (obj.Selected)
                Scene.CurrentScene._shader = _selectedObjectShader;
            else
                Scene.CurrentScene._shader = _standardObjectShader;
            Scene.CurrentScene.UpdatePVM();
            var _modelMatrix = obj.GetModelMatrix();
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);
            obj.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
            switch (obj)
            {
                case ParameterizedPoint point:
                    GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
                    break;

                #region bezierCurve

                case BezierCurveC0 bezierCurveC0:
                {
                    int indexBufferObject;
                    if (bezierCurveC0.DrawPolygon)
                    {
                        Scene.CurrentScene._shader = _colorShader;
                        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
                        Scene.CurrentScene.UpdatePVM();
                        Scene.CurrentScene._shader.SetVector4("color", curveColor);
                        indexBufferObject = GL.GenBuffer();
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
                        GL.BufferData(BufferTarget.ElementArrayBuffer, bezierCurveC0._lines.Length * sizeof(uint),
                            bezierCurveC0._lines, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
                        GL.EnableVertexAttribArray(1);
                        GL.DrawElements(PrimitiveType.Lines, obj.lines.Length, DrawElementsType.UnsignedInt, 0);
                    }

                    Scene.CurrentScene._shader = _bezierCurveC0Shader;
                    Scene.CurrentScene._shader.SetInt("tessLevels", bezierCurveC0.tessLevel);
                    Scene.CurrentScene.UpdatePVM();
                    GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

                    indexBufferObject = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, bezierCurveC0._patches.Length * sizeof(uint),
                        bezierCurveC0._patches, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
                    GL.EnableVertexAttribArray(1);

                    GL.DrawElements(PrimitiveType.Patches, bezierCurveC0.patches.Length, DrawElementsType.UnsignedInt,
                        0);
                    break;
                }

                case BezierCurveC2 bezierCurveC2:
                {
                    int indexBufferObject;
                    if (bezierCurveC2.DrawPolygon)
                    {
                        Scene.CurrentScene._shader = _colorShader;
                        Scene.CurrentScene._shader.SetMatrix4("modelMatrix", Matrix4.Identity);
                        Scene.CurrentScene.UpdatePVM();
                        Scene.CurrentScene._shader.SetVector4("color", curveColor);
                        indexBufferObject = GL.GenBuffer();
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
                        GL.BufferData(BufferTarget.ElementArrayBuffer, bezierCurveC2._lines.Length * sizeof(uint),
                            bezierCurveC2._lines, BufferUsageHint.StaticDraw);
                        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
                        GL.EnableVertexAttribArray(1);
                        GL.DrawElements(PrimitiveType.Lines, obj.lines.Length, DrawElementsType.UnsignedInt, 0);
                    }

                    Scene.CurrentScene._shader = _bezierCurveC0Shader;
                    Scene.CurrentScene._shader.SetInt("tessLevels", bezierCurveC2.tessLevel);
                    Scene.CurrentScene.UpdatePVM();
                    GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

                    indexBufferObject = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, bezierCurveC2._patches.Length * sizeof(uint),
                        bezierCurveC2._patches, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
                    GL.EnableVertexAttribArray(1);

                    GL.DrawElements(PrimitiveType.Patches, bezierCurveC2.patches.Length, DrawElementsType.UnsignedInt,
                        0);
                    break;
                }

                #endregion

                default:
                    GL.DrawElements(PrimitiveType.Lines, obj.lines.Length, DrawElementsType.UnsignedInt, 0);
                    break;
            }
        }

        if (SelectedObject is not IBezierCurve)
            if (SelectedObject is CompositeObject o)
            {
                var _modelMatrix = o.GetModelMatrix();
                Scene.CurrentScene._shader = _centerObjectShader;
                Scene.CurrentScene.UpdatePVM();
                Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);

                #region bezierCurve

                if (o is BezierCurveC0 c)
                    c.GenerateVerticesBase(vertexAttributeLocation, normalAttributeLocation);
                else if (o is BezierCurveC2 c2)
                    c2.GenerateVerticesBase(vertexAttributeLocation, normalAttributeLocation);

                #endregion

                else
                    o.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);

                GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
            }

        {
            var _modelMatrix = _pointer.GetModelMatrix();
            Scene.CurrentScene._shader = _pointerShader;
            Scene.CurrentScene.UpdatePVM();
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);
            _pointer.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
            GL.DrawElements(PrimitiveType.Lines, _pointer.lines.Length, DrawElementsType.UnsignedInt, 0);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool IsNameTaken(string objname)
    {
        foreach (var o in ParameterizedObjects)
        {
            if (o.Name == objname)
                return true;
        }

        return false;
    }

    public void UnselectAll()
    {
        foreach (var o in ParameterizedObjects)
        {
            o.Selected = false;
            _selectedObject = null;
        }
    }

    public void DrawPoints()
    {
        // GL.ClearColor(0f, 0f, 0f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                 ClearBufferMask.StencilBufferBit);
        Scene.CurrentScene._shader = _pickingShader;
        Scene.CurrentScene.UpdatePVM();
        foreach (var point in _parameterizedPoints)
        {
            var _modelMatrix = point.GetModelMatrix();
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);
            Scene.CurrentScene._shader.SetVector3("PickingColor", point.PickingColor);
            point.GenerateVertices(0, 0);
            GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
        }

        GL.Flush();
        GL.Finish();
    }
}