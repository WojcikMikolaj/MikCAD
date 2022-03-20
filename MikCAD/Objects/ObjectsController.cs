﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MikCAD.Annotations;
using OpenTK.Graphics.OpenGL;

namespace MikCAD;

public class ObjectsController : INotifyPropertyChanged
{
    private ParameterizedObject _selectedObject;
    internal Pointer3D _pointer;
    private Shader _selectedObjectShader = new Shader("Shaders/SelectedObject.vert", "Shaders/SelectedObject.frag");
    private Shader _standardObjectShader = new Shader("Shaders/Shader.vert", "Shaders/Shader.frag");
    private Shader _pointerShader = new Shader("Shaders/PointerShader.vert", "Shaders/PointerShader.frag");
    private Shader _pickingShader = new Shader("Shaders/PickingShader.vert", "Shaders/PickingShader.frag");

    public ParameterizedObject SelectedObject
    {
        get => _selectedObject;
        set
        {
            _selectedObject = value;
            if (_selectedObject is null)
            {
                MainWindow.current.torusControl.Visibility = Visibility.Hidden;
                MainWindow.current.pointControl.Visibility = Visibility.Hidden;
                return;
            }
            switch (_selectedObject)
            {
                case Torus torus:
                    MainWindow.current.torusControl.Visibility = Visibility.Visible;
                    MainWindow.current.pointControl.Visibility = Visibility.Hidden;
                    break;
                case ParameterizedPoint point:
                case Pointer3D pointer3D:
                case CompositeObject compositeObject:
                    MainWindow.current.torusControl.Visibility = Visibility.Hidden;
                    MainWindow.current.pointControl.Visibility = Visibility.Visible;
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
        if(parameterizedObject is ParameterizedPoint point)
            _parameterizedPoints.Add(point);
        SelectedObject ??= parameterizedObject;
        return true;
    }

    public bool DeleteSelectedObjects()
    {
        if (_selectedObject is Pointer3D)
            return false;
        if (_selectedObject is ParameterizedPoint point)
            _parameterizedPoints.Remove(point);
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
                default:
                    GL.DrawElements(PrimitiveType.Lines, obj.lines.Length, DrawElementsType.UnsignedInt, 0);
                    break;
            }
        }

        if (SelectedObject is CompositeObject o)
        {
            var _modelMatrix = o.GetModelMatrix();
            Scene.CurrentScene._shader = _selectedObjectShader;
            Scene.CurrentScene.UpdatePVM();
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);
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