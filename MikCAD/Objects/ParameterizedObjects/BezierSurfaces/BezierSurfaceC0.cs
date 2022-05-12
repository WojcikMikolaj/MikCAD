using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace MikCAD.BezierSurfaces;

public class BezierSurfaceC0 : CompositeObject, ISurface, I2DObject
{
    private List<List<ParameterizedPoint>> points;


    private uint _uPatches = 1;
    public uint UPatches
    {
        get => _uPatches;
        set
        {
            if (!_applied && value >= 1)
            {
                _uPatches = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(UPatches));
            }
        }
    }

    private uint _vPatches = 1;
    public uint VPatches
    {
        get => _vPatches;
        set
        {
            if (!_applied && value >= 1)
            {
                _vPatches = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(VPatches));
            }
        }
    }


    private uint _uDivisions = 1;
    public uint UDivisions
    {
        get => _uDivisions;
        set
        {
            if (value >= 1)
            {
                _uDivisions = value;
                OnPropertyChanged(nameof(UDivisions));
            }
        }
    }

    private uint _vDivisions = 1;
    public uint VDivisions
    {
        get => _vDivisions;
        set
        {
            if (value >= 1)
            {
                _vDivisions = value;
                OnPropertyChanged(nameof(VDivisions));
            }
        }
    }

    private bool _isRolled = false;
    public bool IsRolled
    {
        get => _isRolled;
        set
        {
            if (!_applied)
            {
                _isRolled = value;
            }
        }
    }
    
    private bool _applied = false;
    public bool Applied
    {
        get => _applied;
        set
        {
            if (value)
            {
                _applied = true;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(Applied));
                OnPropertyChanged(nameof(CanBeChanged));
            }
        }
    }

    public bool CanBeChanged => !Applied;

    private string _name;
    public virtual String Name
    {
        get => _name;
        set
        {
            int count = 1;
            var objname = value;
            while (Scene.CurrentScene.ObjectsController.IsNameTaken(objname))
            {
                objname = value + $"({count++})";
            }

            _name = objname;
            OnPropertyChanged(nameof(Name));
        }
    }

    private void UpdatePatchesCount()
    {
        var rowsCount = 4 + 3 * (VPatches - 1);
        var colsCount = 4 + 3 * (UPatches - 1);

        var startPoint = GetModelMatrix().ExtractTranslation();
        var dx = 1 / 4f;
        var dy = 1 / 4f;
        
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < points[i].Count; j++)
            {
                points[i][j].Deleted = true;
                Scene.CurrentScene.ObjectsController.ParameterizedObjects.Remove(points[i][j]);
            }
        }

        points = new List<List<ParameterizedPoint>>();
        for (int i = 0; i < colsCount; i++)
        {
            points.Add(new List<ParameterizedPoint>());
            for (int j = 0; j < rowsCount; j++)
            {
                var point = new ParameterizedPoint();
                point.parents.Add(this);
                point.posX = (startPoint + j*new Vector3(dx, dy, 0)).X;
                point.posY = (startPoint + i*new Vector3(dx, dy, 0)).Y;
                Scene.CurrentScene.ObjectsController.AddObjectToScene(point);
                points[i].Add(point);
            }
        }
    }

    public override void ProcessObject(ParameterizedObject o)
    {
        
    }

    public BezierSurfaceC0() : base("SurfaceC0")
    {
        points = new List<List<ParameterizedPoint>>();
        Name = "SurfaceC0";
        UpdatePatchesCount();
    }
}