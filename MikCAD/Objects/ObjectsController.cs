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
    internal Shader _selectedObjectShader = new Shader("Shaders/SelectedObject.vert", "Shaders/SelectedObject.frag");
    internal Shader _standardObjectShader = new Shader("Shaders/Shader.vert", "Shaders/Shader.frag");
    internal Shader _pointerShader = new Shader("Shaders/PointerShader.vert", "Shaders/PointerShader.frag");
    internal Shader _pickingShader = new Shader("Shaders/PickingShader.vert", "Shaders/PickingShader.frag");
    internal Shader _colorShader = new Shader("Shaders/ColorShader.vert", "Shaders/ColorShader.frag");
    internal Shader _centerObjectShader =
        new Shader("Shaders/CenterObjectShader.vert", "Shaders/CenterObjectShader.frag");
    internal Shader _bezierCurveC0Shader = new Shader(
        "Shaders/BezierShader.vert",
        "Shaders/BezierShader.frag",
        "Shaders/BezierCurveC0TessControlShader.tesc",
        "Shaders/BezierCurveC0TessEvaluationShader.tese");
    internal Shader _bezierCurveC2Shader = new Shader(
        "Shaders/BezierShader.vert",
        "Shaders/BezierShader.frag",
        "Shaders/BezierCurveC2/BezierCurveC2TessControlShader.tesc",
        "Shaders/BezierCurveC2/BezierCurveC2TessEvaluationShader.tese");
    public Shader _interpolatingBezierCurveC2Shader = new Shader(
        "Shaders/BezierShader.vert",
        "Shaders/BezierShader.frag",
        "Shaders/InterpolatingBezierCurveC2/InterpolatingBezierCurveC2TessControlShader.tesc",
        "Shaders/InterpolatingBezierCurveC2/InterpolatingBezierCurveC2TessEvaluationShader.tese");

    private DrawProcessor _drawProcessor;

    public ObjectsController()
    {
        _drawProcessor = new DrawProcessor(this);
    }
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
            MainWindow.current.interpolatingBezierCurveC2Control.Visibility = Visibility.Hidden;
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
                case InterpolatingBezierCurveC2 interpolatingBezierCurveC2:
                    MainWindow.current.interpolatingBezierCurveC2Control.Visibility = Visibility.Visible;
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

    public IEnumerable<ParameterizedPoint> AllPoints
    {
        get
        {
            List<ParameterizedPoint> points = new List<ParameterizedPoint>(ParameterizedPoints);
            foreach (var parameterizedObject in ParameterizedObjects)
            {
                if (parameterizedObject is BezierCurveC2 curve)
                {
                    if (curve.Bernstein)
                    {
                        points.AddRange(curve.BernsteinPoints);
                    }
                }
            }

            return points;
        }
    }


    public bool AddObjectToScene(ParameterizedObject parameterizedObject)
    {
        ParameterizedObjects.Add(parameterizedObject);
        switch (parameterizedObject)
        {
            case ParameterizedPoint point:
            {
                _parameterizedPoints.Add(point);

                #region bezierCurve

                if (SelectedObject is IBezierCurve)
                    if (SelectedObject is CompositeObject curve)
                    {
                        curve.ProcessObject(parameterizedObject);
                        MainWindow.current.bezierCurveC0Control.PointsList.Items.Refresh();
                        MainWindow.current.bezierCurveC2Control.PointsList.Items.Refresh();
                        MainWindow.current.interpolatingBezierCurveC2Control.PointsList.Items.Refresh();
                    }

                #endregion

                break;
            }

            #region bezierCurve

            case IBezierCurve curve:
            {
                switch (SelectedObject)
                {
                    case ParameterizedPoint point:
                        curve.ProcessPoint(point);
                        break;
                    case CompositeObject compositeObject:
                        compositeObject.Selected = false;
                        (curve as CompositeObject).ProcessObject(compositeObject);
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
        var obj = _selectedObject;
        if (obj is null)
            return false;
        if (_selectedObject is Pointer3D)
            return false;
        if (_selectedObject is ParameterizedPoint point)
            _parameterizedPoints.Remove(point);

        #region bezierCurve

        if (_selectedObject is IBezierCurve)
        {
            var curve = _selectedObject as CompositeObject;
            foreach (var o in curve._objects)
            {
                o.Selected = false;
            }

            curve.Selected = false;
            ParameterizedObjects.Remove(curve);
            SelectedObject = null;
            return true;
        }

        #endregion

        List<ParameterizedObject> objectsToDelete = new List<ParameterizedObject>();
        foreach (var o in ParameterizedObjects)
        {
            if (o.Selected || o == obj)
                objectsToDelete.Add(o);
            o.Selected = false;
        }

        foreach (var o in objectsToDelete)
        {
            ParameterizedObjects.Remove(o);
        }

        SelectedObject = null;
        obj.OnDelete();
        return true;
    }

    public void SelectObject(ParameterizedObject o)
    {
        if(o is null)
            return;
        
        #region bezierCurve

        if (MainWindow.AddToSelected && SelectedObject is IBezierCurve)
        {
            (SelectedObject as CompositeObject).ProcessObject(o);
            MainWindow.current.bezierCurveC0Control.PointsList.Items.Refresh();
            MainWindow.current.bezierCurveC2Control.PointsList.Items.Refresh();
            MainWindow.current.interpolatingBezierCurveC2Control.PointsList.Items.Refresh();
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
        _drawProcessor.DrawGrid(Scene.CurrentScene.camera.grid, vertexAttributeLocation, normalAttributeLocation);
        foreach (var obj in ParameterizedObjects)
        {
            Scene.CurrentScene._shader = obj.Selected ? _selectedObjectShader : _standardObjectShader;
            Scene.CurrentScene.UpdatePVM();
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", obj.GetModelMatrix());
            obj.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
            obj.PassToDrawProcessor(_drawProcessor, vertexAttributeLocation, normalAttributeLocation);
        }

        if (SelectedObject is not IBezierCurve)
            if (SelectedObject is CompositeObject o)
            {
                _drawProcessor.ProcessObject(o, vertexAttributeLocation, normalAttributeLocation);
            }

        _drawProcessor.ProcessObject(_pointer, vertexAttributeLocation, normalAttributeLocation);
        _drawProcessor.DrawAxis(MainWindow.current.ActiveAxis, vertexAttributeLocation, normalAttributeLocation);
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
            SelectedObject = null;
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