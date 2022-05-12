using System;
using System.Collections.Generic;

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
        
    }


    public BezierSurfaceC0() : base("SurfaceC0")
    {
        Name = "SurfaceC0";
        UpdatePatchesCount();
    }
}