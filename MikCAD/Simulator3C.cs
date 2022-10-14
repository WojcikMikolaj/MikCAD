﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikCAD;

public class Simulator3C : INotifyPropertyChanged
{
    public static Simulator3C Simulator;

    public bool Enabled { get; set; }

    private uint _xGridSizeInUnits = 10;

    public uint XGridSizeInUnits
    {
        get => _xGridSizeInUnits;
        set
        {
            if (value > 0)
            {
                _xGridSizeInUnits = value;
            }
        }
    }

    private uint _yGridSizeInUnits = 10;

    public uint YGridSizeInUnits
    {
        get => _yGridSizeInUnits;
        set
        {
            if (value > 0)
            {
                _yGridSizeInUnits = value;
            }
        }
    }

    private uint _xGridDivisions = 8000;

    public uint XGridDivisions
    {
        get => _xGridDivisions;
        set
        {
            if (value > 0)
            {
                _xGridDivisions = value;
            }
        }
    }

    private uint _yGridDivisions = 8000;

    public uint YGridDivisions
    {
        get => _yGridDivisions;
        set
        {
            if (value > 0)
            {
                _yGridDivisions = value;
            }
        }
    }

    private uint _maxCutterImmersionInMm = 20;
    public uint MaxCutterImmersionInMm
    {
        get => _maxCutterImmersionInMm;
        set
        {
            _maxCutterImmersionInMm = value;
        }
    }

    public CutterType CutterType { get; set; }

    public bool SphericalSelected
    {
        get => CutterType == CutterType.Spherical;
        set => CutterType = value ? CutterType.Spherical : CutterType.Flat;
    } 
    
    public bool FlatSelected
    {
        get => CutterType == CutterType.Flat;
        set => CutterType = value ? CutterType.Flat : CutterType.Spherical;
    } 
    
    private uint _cutterDiameterInMm = 15;
    public uint CutterDiameterInMm
    {
        get => _cutterDiameterInMm;
        set
        {
            _cutterDiameterInMm = value;
        }
    }

    private uint _simulationSpeed = 1;
    public uint SimulationSpeed
    {
        get => _simulationSpeed;
        set
        {
            if (value > 0 && value <= 100)
            {
                _simulationSpeed = value;
            }
        }
    }

    public Simulator3C()
    {
        Simulator = this;
    }

    public (bool, SimulatorErrorCode) ParsePathFile(string diagFileName, string[] lines)
    {
        var indexOfDot = diagFileName.IndexOf('.');
        if (indexOfDot + 1 >= diagFileName.Length)
        {
            return (false, SimulatorErrorCode.WrongFileName);
        }

        var cutterTypeChar = diagFileName[indexOfDot + 1];
        switch (cutterTypeChar)
        {
            case 'k':
                CutterType = CutterType.Spherical;
                break;
            case 'f':
                CutterType = CutterType.Flat;
                break;
            default:
                return (false, SimulatorErrorCode.WrongCutterType);
        }
        OnPropertyChanged(nameof(CutterType));
        OnPropertyChanged(nameof(SphericalSelected));
        OnPropertyChanged(nameof(FlatSelected));
        
        var cutterSizeStr = diagFileName.Substring(indexOfDot+2,2);
        UInt32 cutterSize;
        if (UInt32.TryParse(cutterSizeStr, out cutterSize))
        {
            CutterDiameterInMm = cutterSize;
        }
        else
        {
            return (false, SimulatorErrorCode.WrongCutterSize);
        }
        OnPropertyChanged(nameof(CutterDiameterInMm));

        return (true, SimulatorErrorCode.None);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}