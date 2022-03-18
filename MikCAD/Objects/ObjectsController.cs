﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MikCAD.Annotations;
using OpenTK.Graphics.OpenGL;

namespace MikCAD;

public class ObjectsController: INotifyPropertyChanged
{

    private ParameterizedObject _selectedObject;

    public ParameterizedObject SelectedObject
    {
        get => _selectedObject;
        private set
        {
            _selectedObject = value;
            OnPropertyChanged(nameof(SelectedObject));
        }
    }
    
    public ObservableCollection<ParameterizedObject> ParameterizedObjects { get; private set; } =
        new ObservableCollection<ParameterizedObject>();

    public bool AddObjectToScene(ParameterizedObject parameterizedObject)
    {
        ParameterizedObjects.Add(parameterizedObject);
        return true;
    }

    public bool DeleteObjectFromScene(ParameterizedObject parameterizedObject)
    {
        return ParameterizedObjects.Remove(parameterizedObject);
    }

    public void SelectObject(string name)
    {
        var item = ParameterizedObjects.FirstOrDefault(x => x.Name == name);
        SelectedObject = item;
    }
    
    public void DrawObjects(uint vertexAttributeLocation, uint normalAttributeLocation, int _vertexBufferObject, int _vertexArrayObject)
    {
        foreach (var obj in ParameterizedObjects)
        {
            var _modelMatrix = obj.GetModelMatrix();
            Scene.CurrentScene._shader.SetMatrix4("modelMatrix", _modelMatrix);
            obj.GenerateVertices(vertexAttributeLocation, normalAttributeLocation, out _vertexBufferObject, out _vertexArrayObject);
            
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Lines, obj.lines.Length, DrawElementsType.UnsignedInt, 0);
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
}