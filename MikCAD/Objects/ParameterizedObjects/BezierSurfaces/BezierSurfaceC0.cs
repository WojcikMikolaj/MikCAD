using System;
using System.Collections.Generic;
using MikCAD.Utilities;
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


    private uint _uDivisions = 4;
    public uint UDivisions
    {
        get => _uDivisions;
        set
        {
            if (value >= 4)
            {
                _uDivisions = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(UDivisions));
            }
        }
    }

    private uint _vDivisions = 4;
    public uint VDivisions
    {
        get => _vDivisions;
        set
        {
            if (value >= 4)
            {
                _vDivisions = value;
                UpdatePatchesCount();
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
                UpdatePatchesCount();
                OnPropertyChanged(nameof(IsRolled));
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

    private float _singlePatchWidth = 1;
    public float SinglePatchWidth
    {
        get => _singlePatchWidth;
        set
        {
            if (value > 0)
            {
                _singlePatchWidth = value;
                OnPropertyChanged(nameof(SinglePatchWidth));
            }
        }
    }
    
    private float _singlePatchHeight = 1;
    public float SinglePatchHeight
    {
        get => _singlePatchHeight;
        set
        {
            if (value > 0)
            {
                _singlePatchWidth = value;
                OnPropertyChanged(nameof(SinglePatchHeight));
            }
        }
    }
    
    private float _R = 1;
    public float R
    {
        get => _R;
        set
        {
            if (value > 0)
            {
                _R = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(R));
            }
        }
    }
    
    private float _cylinderHeight = 1;
    public float CylinderHeight
    {
        get => _cylinderHeight;
        set
        {
            if (value > 0)
            {
                _cylinderHeight = value;
                UpdatePatchesCount();
                OnPropertyChanged(nameof(CylinderHeight));
            }
        }
    }
    
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
    
    private bool _drawPolygon = false;
    public bool DrawPolygon
    {
        get => _drawPolygon;
        set
        {
            _drawPolygon = value;
            OnPropertyChanged(nameof(DrawPolygon));
        }
    }

    private void UpdatePatchesCount()
    {
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < points[i].Count; j++)
            {
                points[i][j].Deleted = true;
                Scene.CurrentScene.ObjectsController.ParameterizedObjects.Remove(points[i][j]);
            }
        }
        
        if (!IsRolled)
        {
            //var rowsCount = VDivisions + (VDivisions - 1) * (VPatches - 1);
            //var colsCount = UDivisions + (UDivisions - 1) * (UPatches - 1);
            
            var rowsCount = 4 + 3 * (VPatches - 1);
            var colsCount = 4 + 3 * (UPatches - 1);

            var startPoint = GetModelMatrix().ExtractTranslation();
            var dx = SinglePatchWidth / 3;
            var dz = SinglePatchHeight / 3;

           

            points.Clear();
            for (int i = 0; i < colsCount; i++)
            {
                points.Add(new List<ParameterizedPoint>());
                for (int j = 0; j < rowsCount; j++)
                {
                    var point = new ParameterizedPoint();
                    point.parents.Add(this);
                    point.posX = (startPoint + j * new Vector3(dx, 0, dz)).X;
                    point.posZ = (startPoint + i * new Vector3(dx, 0, dz)).Z;
                    Scene.CurrentScene.ObjectsController.AddObjectToScene(point);
                    points[i].Add(point);
                }
            }
        }
        else
        {
            var rowsCount = 4 + 3 * (VPatches - 1) - 1;
            var colsCount = 4 + 3 * (UPatches - 1);
            
            var startPoint = GetModelMatrix().ExtractTranslation() + new Vector3(0,R,0);
            var dx = CylinderHeight / 3;
            var dalpha = MathHelper.DegreesToRadians(360f / rowsCount);

            points.Clear();
            for (int i = 0; i < colsCount; i++)
            {
                points.Add(new List<ParameterizedPoint>());
                for (int j = 0; j < rowsCount; j++)
                {
                    var point = new ParameterizedPoint();
                    point.parents.Add(this);
                    point.posX = (startPoint +  i * new Vector3(dx,0,0)).X;
                    point.posY = (startPoint + new Vector3((float)Math.Sin(j*dalpha)) * R).Y;
                    point.posZ = (startPoint + new Vector3((float)Math.Cos(j*dalpha)) * R).Z;
                    Scene.CurrentScene.ObjectsController.AddObjectToScene(point);
                    points[i].Add(point);
                }
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