using MikCAD.Utilities;
using OpenTK.Mathematics;

namespace MikCAD;

public struct CuttingLinePoint
{
    public float XPosInMm { get; init; }
    public float YPosInMm { get; init; }
    public float ZPosInMm { get; init; }
    
    public int InstructionNumber { get; init; }
}

public class CuttingLines
{
    public CuttingLinePoint[] points { get; init; }
}