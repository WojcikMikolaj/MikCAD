using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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

    private String _fileName= "Brak pliku";

    public String FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
            OnPropertyChanged(nameof(FileName));
        }
    } 
    
    public Simulator3C()
    {
        Simulator = this;
    }

    private Regex _moveLineRegex = new Regex(@"N(\d+)G01(X-?\d{1,2}.\d{3})?(Y-?\d{1,2}.\d{3})?(Z-?\d{1,2}.\d{3})?");
    
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


        int lineNumber = 0;
        foreach (var line in lines)
        {
            var matchCollection = _moveLineRegex.Matches(line, 0);
            if (matchCollection.Count == 1 && matchCollection[0].Length == line.Length)
            {
                
            }
            else
            {
                return (false, SimulatorErrorCode.UnsupportedCommand);
            }
            lineNumber++;
        }
        
        
        FileName = diagFileName.Substring(diagFileName.LastIndexOf("\\", StringComparison.Ordinal)+1);
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