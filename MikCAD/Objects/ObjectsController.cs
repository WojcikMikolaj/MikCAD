using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MikCAD.Annotations;
using MikCAD.BezierCurves;
using MikCAD.BezierSurfaces;
using MikCAD.Objects;
using MikCAD.Utilities;
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
        "Shaders/BezierCurveC0/BezierCurveC0TessControlShader.tesc",
        "Shaders/BezierCurveC0/BezierCurveC0TessEvaluationShader.tese");

    internal Shader _bezierSurfaceC0Shader = new Shader(
        "Shaders/BezierShader.vert",
        "Shaders/BezierShader.frag",
        "Shaders/BezierSurfaceC0/BezierSurfaceC0TessControlShader.tesc",
        "Shaders/BezierSurfaceC0/BezierSurfaceC0TessEvaluationShader.tese");

    internal Shader _bezierSurfaceC2Shader = new Shader(
        "Shaders/BezierShader.vert",
        "Shaders/BezierShader.frag",
        "Shaders/BezierSurfaceC2/BezierSurfaceC2TessControlShader.tesc",
        "Shaders/BezierSurfaceC2/BezierSurfaceC2TessEvaluationShader.tese");

    internal Shader _bezierCurveC2Shader = new Shader(
        "Shaders/BezierShader.vert",
        "Shaders/BezierShader.frag",
        "Shaders/BezierCurveC2/BezierCurveC2TessControlShader.tesc",
        "Shaders/BezierCurveC2/BezierCurveC2TessEvaluationShader.tese");

    internal Shader _gregoryPatchShader = new Shader(
        "Shaders/BezierShader.vert",
        "Shaders/BezierShader.frag",
        "Shaders/GregoryPatch/GregoryPatchTessControlShader.tesc",
        "Shaders/GregoryPatch/GregoryPatchTessEvaluationShader.tese");

    public Shader _interpolatingBezierCurveC2Shader = new Shader(
        "Shaders/BezierShader.vert",
        "Shaders/BezierShader.frag",
        "Shaders/InterpolatingBezierCurveC2/InterpolatingBezierCurveC2TessControlShader.tesc",
        "Shaders/InterpolatingBezierCurveC2/InterpolatingBezierCurveC2TessEvaluationShader.tese");

    public Shader _selectionBoxShader = new Shader("Shaders/SelectionBox/SelectionBoxShader.vert",
        "Shaders/SelectionBox/SelectionBoxShader.frag");

    private DrawProcessor _drawProcessor;
    public readonly SelectionBox SelectionBox = new SelectionBox();

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
            MainWindow.current.bezierSurfaceC0Control.Visibility = Visibility.Hidden;
            MainWindow.current.bezierSurfaceC2Control.Visibility = Visibility.Hidden;
            MainWindow.current.gregoryPatchControl.Visibility = Visibility.Hidden;
            MainWindow.current.compositeControl.Visibility = Visibility.Hidden;
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
                case GregoryPatch:
                    MainWindow.current.gregoryPatchControl.Visibility = Visibility.Visible;
                    break;
                //must be before CompositeObject
                case BezierSurfaceC0:
                    MainWindow.current.bezierSurfaceC0Control.Visibility = Visibility.Visible;
                    break;
                case BezierSurfaceC2:
                    MainWindow.current.bezierSurfaceC2Control.Visibility = Visibility.Visible;
                    break;
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
                    MainWindow.current.pointControl.Visibility = Visibility.Visible;
                    break;
                case CompositeObject compositeObject:
                    MainWindow.current.compositeControl.Visibility = Visibility.Visible;
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
            if (o is ParameterizedPoint {CanBeDeleted: false})
                continue;
            ParameterizedObjects.Remove(o);
        }

        SelectedObject = null;
        obj.OnDelete();
        return true;
    }

    public void SelectObject(ParameterizedObject o)
    {
        if (o is null)
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
            if (SelectedObject is IIntersectable intersectable)
            {
                var cmp = new CompositeObject(null)
                {
                    IsPointComposite = false
                };
                cmp.ProcessObject((ParameterizedObject)intersectable);
                cmp.ProcessObject(o);
                SelectedObject = cmp;
                o.Selected = true;
                return;
            }

            if (SelectedObject is ISurface)
                return;
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

    public void DrawObjects(EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        _drawProcessor.DrawGrid(Scene.CurrentScene.camera.grid, eye, vertexAttributeLocation, normalAttributeLocation);
        foreach (var obj in ParameterizedObjects)
        {
            Scene.CurrentScene._shader = obj.Selected ? _selectedObjectShader : _standardObjectShader;
            Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", obj.GetModelMatrix());
            Scene.CurrentScene._shader.SetFloat("overrideEnabled", eye == EyeEnum.Both ? 0 : 1);
            switch (eye)
            {
                case EyeEnum.Left:
                    Scene.CurrentScene._shader.SetVector4("overrideColor", Scene.CurrentScene.camera.leftColor);
                    break;
                case EyeEnum.Right:
                    Scene.CurrentScene._shader.SetVector4("overrideColor", Scene.CurrentScene.camera.rightColor);
                    break;
            }

            obj.GenerateVertices(vertexAttributeLocation, normalAttributeLocation);
            obj.SetTexture();
            obj.PassToDrawProcessor(_drawProcessor, eye, vertexAttributeLocation, normalAttributeLocation);
        }

        if (SelectedObject is not IBezierCurve && SelectedObject is not ISurface)
            if (SelectedObject is CompositeObject o)
            {
                _drawProcessor.ProcessObject(o, eye, vertexAttributeLocation, normalAttributeLocation);
            }

        _drawProcessor.ProcessObject(_pointer, eye, vertexAttributeLocation, normalAttributeLocation);
        _drawProcessor.DrawAxis(MainWindow.current.ActiveAxis, eye, vertexAttributeLocation, normalAttributeLocation);
        if (SelectionBox.Draw)
            _drawProcessor.DrawSelectionBox(SelectionBox);
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

    public void DrawPoints(EyeEnum eye)
    {
        // GL.ClearColor(0f, 0f, 0f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                 ClearBufferMask.StencilBufferBit);
        Scene.CurrentScene._shader = _pickingShader;
        Scene.CurrentScene.UpdatePVMAndStereoscopics(eye);
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

    public void SelectPointsInSelectionBox()
    {
        Scene.CurrentScene.camera.UpdateProjectionMatrices();
        Scene.CurrentScene.camera.UpdateViewMatrices();
        List<ParameterizedObject> selectedPoints = new();
        foreach (var point in ParameterizedObjects)
        {
            if (point is ParameterizedPoint p && SelectionBox.InBox(p))
                selectedPoints.Add(point);
        }

        CompositeObject comp;
        if (selectedPoints.Count > 0)
        {
            if (selectedPoints.Count > 1)
            {
                comp = new CompositeObject(selectedPoints[0]);
                for (int i = 1; i < selectedPoints.Count; i++)
                    comp.ProcessObject(selectedPoints[i]);
                SelectedObject = comp;
            }
            else
            {
                SelectedObject = selectedPoints[0];
            }
        }
    }

    public void ClearScene()
    {
        ParameterizedObjects.Clear();
        _pointer = new Pointer3D();
        ParameterizedObjects.Add(_pointer);
        ParameterizedPoints.Clear();
        SelectedObject = null;
    }

    public void CollapsePoints()
    {
        if (_selectedObject is CompositeObject compositeObject)
        {
            
            if (compositeObject._objects.Count == 2
                && compositeObject._objects[0] is ParameterizedPoint p1
                && compositeObject._objects[1] is ParameterizedPoint p2)
            {
                SelectedObject = null;
                var middlePoint = new ParameterizedPoint()
                {
                    posX = (p1.posX + p2.posX) / 2,
                    posY = (p1.posY + p2.posY) / 2,
                    posZ = (p1.posZ + p2.posZ) / 2,
                    IsCollapsedPoint = true,
                };
                var set = new HashSet<CompositeObject>();
                foreach (var parent in p1.parents)
                {
                    if(parent is CompositeObject comp)
                        set.Add(comp);
                }

                foreach (var parent in p2.parents)
                {
                    if(parent is CompositeObject comp)
                        set.Add(comp);
                }

                foreach (var parent in set)
                {
                    middlePoint.parents.Add(parent);
                    if (parent is ISurface)
                        middlePoint.CanBeDeleted = false;
                }

                foreach (var parent in set)
                {
                    if (parent is ISurface surf)
                    {
                        surf.SubstitutePoints(p1, middlePoint);
                        surf.SubstitutePoints(p2, middlePoint);
                    }

                    while (true)
                    {
                        var index = parent._objects.FindIndex(x => x.Id == p1.Id);
                        if (index == -1)
                            break;
                        parent._objects[index] = middlePoint;
                    }

                    while (true)
                    {
                        var index = parent._objects.FindIndex(x => x.Id == p2.Id);
                        if (index == -1)
                            break;
                        parent._objects[index] = middlePoint;
                    }
                }

                p1.CanBeDeleted = true;
                p2.CanBeDeleted = true;
                ParameterizedObjects.Remove(p1);
                ParameterizedObjects.Remove(p2);
                p1.Deleted = true;
                p2.Deleted = true;

                ParameterizedObjects.Add(middlePoint);
                _parameterizedPoints.Add(middlePoint);
            }
        }
    }

    public bool PatchHole()
    {
        if (_selectedObject is CompositeObject compositeObject)
        {
            if (compositeObject._objects.Count == 3
                && compositeObject._objects[0] is BezierSurfaceC0 surf1
                && compositeObject._objects[1] is BezierSurfaceC0 surf2
                && compositeObject._objects[2] is BezierSurfaceC0 surf3)
            {
                var points1 = surf1.GetPoints();
                var points2 = surf2.GetPoints();
                var points3 = surf3.GetPoints();

                List<ParameterizedPoint> innerRing = new();
                List<ParameterizedPoint> outerRing = new();

                // ReSharper disable once NotAccessedVariable
                ParameterizedPoint first = null;
                // ReSharper disable once TooWideLocalVariableScope
                ParameterizedPoint second = null;
                // ReSharper disable once TooWideLocalVariableScope
                ParameterizedPoint third = null;

                for (int i = 0; i < points1.Count; i++)
                {
                    for (int j = 0; j < points1[0].Count; j++)
                    {
                        if (points1[i][j].parents.Contains(surf3))
                        {
                            first = points1[i][j];
                            innerRing.Add(first);

                            if (i + 3 < points1.Count && points1[i + 3][j].parents.Contains(surf2))
                            {
                                innerRing.Add(points1[i + 1][j]);
                                innerRing.Add(points1[i + 2][j]);

                                second = points1[i + 3][j];
                                innerRing.Add(second);

                                //Handle outer ring
                                HandleOuterRing(outerRing, points1, i, j, ascending: true, secondRow: true);

                                (int k, int l) = surf2.FindPointIndices(second);

                                if (HandleSurf2(points2, k, l, surf3, innerRing, outerRing, points3, first, out third))
                                    goto PatchAssemblyStage;

                                ClearRingsAfterTry(innerRing, outerRing);
                            }

                            if (i - 3 >= 0 && points1[i - 3][j].parents.Contains(surf2))
                            {
                                innerRing.Add(points1[i - 1][j]);
                                innerRing.Add(points1[i - 2][j]);

                                second = points1[i - 3][j];
                                innerRing.Add(second);

                                //Handle outer ring
                                HandleOuterRing(outerRing, points1, i, j, ascending: false, secondRow: true);

                                (int k, int l) = surf2.FindPointIndices(second);

                                if (HandleSurf2(points2, k, l, surf3, innerRing, outerRing, points3, first, out third))
                                    goto PatchAssemblyStage;

                                ClearRingsAfterTry(innerRing, outerRing);
                            }

                            if (j + 3 < points1[0].Count && points1[i][j + 3].parents.Contains(surf2))
                            {
                                innerRing.Add(points1[i][j + 1]);
                                innerRing.Add(points1[i][j + 2]);

                                second = points1[i][j + 3];
                                innerRing.Add(second);

                                //Handle outer ring
                                HandleOuterRing(outerRing, points1, i, j, ascending: true, secondRow: false);

                                (int k, int l) = surf2.FindPointIndices(second);

                                if (HandleSurf2(points2, k, l, surf3, innerRing, outerRing, points3, first, out third))
                                    goto PatchAssemblyStage;

                                ClearRingsAfterTry(innerRing, outerRing);
                            }

                            if (j - 3 >= 0 && points1[i][j - 3].parents.Contains(surf2))
                            {
                                innerRing.Add(points1[i][j - 1]);
                                innerRing.Add(points1[i][j - 2]);

                                second = points1[i][j - 3];
                                innerRing.Add(second);

                                //Handle outer ring
                                HandleOuterRing(outerRing, points1, i, j, ascending: false, secondRow: false);

                                (int k, int l) = surf2.FindPointIndices(second);

                                if (HandleSurf2(points2, k, l, surf3, innerRing, outerRing, points3, first, out third))
                                    goto PatchAssemblyStage;

                                ClearRingsAfterTry(innerRing, outerRing);
                            }
                        }
                    }
                }

                PatchAssemblyStage:
                if (innerRing.Count > 0)
                {
                    var gregory = new GregoryPatch(innerRing, outerRing);
                    AddObjectToScene(gregory);
                    
                    foreach (var point in innerRing)
                    {
                        point.parents.Add(gregory);            
                    }
                    
                    foreach (var point in outerRing)
                    {
                        point.parents.Add(gregory);  
                    }
                    return true;
                }
            }
        }

        return false;
    }

    private bool HandleSurf2(List<List<ParameterizedPoint>> points2, int k, int l, BezierSurfaceC0 surf3,
        List<ParameterizedPoint> innerRing, List<ParameterizedPoint> outerRing, List<List<ParameterizedPoint>> points3,
        ParameterizedPoint first, out ParameterizedPoint third)
    {
        if (k + 3 < points2.Count && points2[k + 3][l].parents.Contains(surf3))
        {
            innerRing.Add(points2[k + 1][l]);
            innerRing.Add(points2[k + 2][l]);

            third = points2[k + 3][l];
            innerRing.Add(third);

            (int m, int n) = surf3.FindPointIndices(third);

            //Handle outer ring
            HandleOuterRing(outerRing, points2, k, l, ascending: true, secondRow: true);

            if (HandleSurf3(points3, m, n, first, innerRing, outerRing))
                return true;

            ClearRingsAfterTry(innerRing, outerRing);
        }

        if (k - 3 >= 0 && points2[k - 3][l].parents.Contains(surf3))
        {
            innerRing.Add(points2[k - 1][l]);
            innerRing.Add(points2[k - 2][l]);

            third = points2[k - 3][l];
            innerRing.Add(third);

            (int m, int n) = surf3.FindPointIndices(third);

            //Handle outer ring
            HandleOuterRing(outerRing, points2, k, l, ascending: false, secondRow: true);

            if (HandleSurf3(points3, m, n, first, innerRing, outerRing))
                return true;

            ClearRingsAfterTry(innerRing, outerRing);
        }

        if (l + 3 < points2[0].Count && points2[k][l + 3].parents.Contains(surf3))
        {
            innerRing.Add(points2[k][l + 1]);
            innerRing.Add(points2[k][l + 2]);

            third = points2[k][l + 3];
            innerRing.Add(third);

            (int m, int n) = surf3.FindPointIndices(third);

            //Handle outer ring
            HandleOuterRing(outerRing, points2, k, l, ascending: true, secondRow: false);

            if (HandleSurf3(points3, m, n, first, innerRing, outerRing))
                return true;

            ClearRingsAfterTry(innerRing, outerRing);
        }

        if (l - 3 >= 0 && points2[k][l - 3].parents.Contains(surf3))
        {
            innerRing.Add(points2[k][l - 1]);
            innerRing.Add(points2[k][l - 2]);

            third = points2[k][l - 3];
            innerRing.Add(third);

            (int m, int n) = surf3.FindPointIndices(third);

            //Handle outer ring
            HandleOuterRing(outerRing, points2, k, l, ascending: false, secondRow: false);

            if (HandleSurf3(points3, m, n, first, innerRing, outerRing))
                return true;

            ClearRingsAfterTry(innerRing, outerRing);
        }

        third = null;
        return false;
    }

    private bool HandleSurf3(List<List<ParameterizedPoint>> points3, int m, int n, ParameterizedPoint first,
        List<ParameterizedPoint> innerRing, List<ParameterizedPoint> outerRing)
    {
        if (m + 3 < points3.Count && points3[m + 3][n].Id == first.Id)
        {
            innerRing.Add(points3[m + 1][n]);
            innerRing.Add(points3[m + 2][n]);

            //Handle outer ring
            HandleOuterRing(outerRing, points3, m, n, ascending: true, secondRow: true);

            return true;
        }

        if (m - 3 >= 0 && points3[m - 3][n].Id == first.Id)
        {
            innerRing.Add(points3[m - 1][n]);
            innerRing.Add(points3[m - 2][n]);

            //Handle outer ring
            HandleOuterRing(outerRing, points3, m, n, ascending: false, secondRow: true);

            return true;
        }

        if (n + 3 < points3[0].Count && points3[m][n + 3].Id == first.Id)
        {
            innerRing.Add(points3[m][n + 1]);
            innerRing.Add(points3[m][n + 2]);

            //Handle outer ring
            HandleOuterRing(outerRing, points3, m, n, ascending: true, secondRow: false);

            return true;
        }

        if (n - 3 >= 0 && points3[m][n - 3].Id == first.Id)
        {
            innerRing.Add(points3[m][n - 3]);
            innerRing.Add(points3[m][n - 3]);

            //Handle outer ring
            HandleOuterRing(outerRing, points3, m, n, ascending: false, secondRow: false);

            return true;
        }

        return false;
    }

    private void HandleOuterRing(List<ParameterizedPoint> outerRing, List<List<ParameterizedPoint>> points, int i,
        int j, bool ascending, bool secondRow)
    {
        if (secondRow)
        {
            if (ascending)
            {
                if (j + 1 < points[0].Count)
                {
                    outerRing.Add(points[i][j + 1]);
                    outerRing.Add(points[i + 1][j + 1]);
                    outerRing.Add(points[i + 2][j + 1]);
                    outerRing.Add(points[i + 3][j + 1]);
                }
                else
                {
                    outerRing.Add(points[i][j - 1]);
                    outerRing.Add(points[i + 1][j - 1]);
                    outerRing.Add(points[i + 2][j - 1]);
                    outerRing.Add(points[i + 3][j - 1]);
                }
            }
            else
            {
                if (j + 1 < points[0].Count)
                {
                    outerRing.Add(points[i][j + 1]);
                    outerRing.Add(points[i - 1][j + 1]);
                    outerRing.Add(points[i - 2][j + 1]);
                    outerRing.Add(points[i - 3][j + 1]);
                }
                else
                {
                    outerRing.Add(points[i][j - 1]);
                    outerRing.Add(points[i - 1][j - 1]);
                    outerRing.Add(points[i - 2][j - 1]);
                    outerRing.Add(points[i - 3][j - 1]);
                }
            }
        }
        else
        {
            if (ascending)
            {
                if (i + 1 < points.Count)
                {
                    outerRing.Add(points[i + 1][j]);
                    outerRing.Add(points[i + 1][j + 1]);
                    outerRing.Add(points[i + 1][j + 2]);
                    outerRing.Add(points[i + 1][j + 3]);
                }
                else
                {
                    outerRing.Add(points[i - 1][j]);
                    outerRing.Add(points[i - 1][j + 1]);
                    outerRing.Add(points[i - 1][j + 2]);
                    outerRing.Add(points[i - 1][j + 3]);
                }
            }
            else
            {
                if (i + 1 < points.Count)
                {
                    outerRing.Add(points[i + 1][j]);
                    outerRing.Add(points[i + 1][j - 1]);
                    outerRing.Add(points[i + 1][j - 2]);
                    outerRing.Add(points[i + 1][j - 3]);
                }
                else
                {
                    outerRing.Add(points[i - 1][j]);
                    outerRing.Add(points[i - 1][j - 1]);
                    outerRing.Add(points[i - 1][j - 2]);
                    outerRing.Add(points[i - 1][j - 3]);
                }
            }
        }
    }

    private void ClearRingsAfterTry(List<ParameterizedPoint> innerRing, List<ParameterizedPoint> outerRing)
    {
        innerRing.RemoveAt(innerRing.Count - 1);
        innerRing.RemoveAt(innerRing.Count - 1);
        innerRing.RemoveAt(innerRing.Count - 1);

        //Handle outer ring
        outerRing.RemoveAt(outerRing.Count - 1);
        outerRing.RemoveAt(outerRing.Count - 1);
        outerRing.RemoveAt(outerRing.Count - 1);
        outerRing.RemoveAt(outerRing.Count - 1);
    }

    public Intersection GetNewIntersectionObject()
    {
        if (_selectedObject is CompositeObject {CanIntersectObjects: true} c)
        {
            return c.GetNewIntersectionObject();
        }

        return null;
    }
}