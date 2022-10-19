using System;
using MikCAD.Utilities;
using OpenTK.Mathematics;

namespace MikCAD;

public struct CuttingLinePoint
{
    public CuttingLinePoint()
    {
    }

    public float XPosInMm { get; init; } = Single.NaN;
    public float YPosInMm { get; init; } = Single.NaN;
    public float ZPosInMm { get; init; } = Single.NaN;
    
    public float XPosInUnits { get; set; } = Single.NaN;
    public float YPosInUnits { get; set; } = Single.NaN;
    public float ZPosInUnits { get; set; } = Single.NaN;

    public int InstructionNumber { get; init; } = -1;

    public (float X, float Y, float Z) GetPosInUnits()
    {
        return (XPosInUnits, YPosInUnits, ZPosInUnits);
    }
    public (float X, float Y, float Z) GetPosInUnitsYZSwitched()
    {
        return (XPosInUnits, ZPosInUnits, YPosInUnits);
    }
}

public class CuttingLines
{
    public CuttingLinePoint[] points { get; init; }
}