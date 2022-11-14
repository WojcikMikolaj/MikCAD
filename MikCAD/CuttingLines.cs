using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using MikCAD.Sciezki;
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

    public bool WriteX { get; set; } = true;
    public bool WriteY { get; set; } = true;
    public bool WriteZ { get; set; } = true;

    public int InstructionNumber { get; init; } = -1;

    public (float X, float Y, float Z) GetPosInUnits()
    {
        return (XPosInUnits, YPosInUnits, ZPosInUnits);
    }

    public (float X, float Y, float Z) GetPosInUnitsYZSwitched()
    {
        return (XPosInUnits, ZPosInUnits, YPosInUnits);
    }

    public Vector3 ToVector3()
    {
        return new Vector3(XPosInUnits, YPosInUnits, ZPosInUnits);
    }

    public static implicit operator CuttingLinePoint(Vector3 posInMm)
    {
        return new CuttingLinePoint()
        {
            XPosInMm = posInMm.X,
            YPosInMm = posInMm.Y,
            ZPosInMm = posInMm.Z,
        };
    }

    public float DistanceSquaredTo(CuttingLinePoint other)
    {
        return Vector3.DistanceSquared(this.ToVector3(), other.ToVector3());
    }

    public enum WritePos
    {
        None = -1,
        X = 0,
        Y = 1,
        Z = 2,
        Xy = 3,
        Xz = 4,
        Yz = 5,
        Xyz = 6,
    }

    public WritePos GetWritePos()
    {
        if (WriteX && WriteY && WriteZ)
        {
            return WritePos.Xyz;
        }

        if (WriteX && WriteY)
        {
            return WritePos.Xy;
        }
        
        if (WriteX && WriteZ)
        {
            return WritePos.Xz;
        }
        
        if (WriteY && WriteZ)
        {
            return WritePos.Yz;
        }

        if (WriteX)
        {
            return WritePos.X;
        }
        
        if (WriteY)
        {
            return WritePos.Y;
        }
        
        if (WriteZ)
        {
            return WritePos.Z;
        }

        return WritePos.None;
    }
}

public class CuttingLines
{
    public CuttingLinePoint[] points { get; init; }

    public void SaveFile(CutterType frez, uint radius, bool optimize = true)
    {
        var fileExtensionBuilder = new StringBuilder();
        var diameter = radius * 2;
        fileExtensionBuilder.Append(".");
        if (frez == CutterType.Flat)
        {
            fileExtensionBuilder.Append("f");
        }
        else
        {
            fileExtensionBuilder.Append("k");
        }

        if (diameter < 10)
        {
            fileExtensionBuilder.Append("0");
        }

        fileExtensionBuilder.Append(diameter);

        var firstInstructionNumber = 3;
        List<string> lines = new List<string>();

        var punktyWynikowe = PathOptimizer.OptimizePaths(points.ToList(), 0.001f, optimize);

        StringBuilder instruction = new StringBuilder();
        foreach (var point in punktyWynikowe)
        {
            instruction.Clear();

            instruction.Append($"N{firstInstructionNumber++}G01");
            if (point.WriteX)
            {
                instruction.Append(
                    $"X{point.XPosInMm.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)}");
            }

            if (point.WriteY)
            {
                instruction.Append(
                    $"Y{point.YPosInMm.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)}");
            }

            if (point.WriteZ)
            {
                instruction.Append(
                    $"Z{point.ZPosInMm.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)}");
            }

            lines.Add(instruction.ToString());
        }

        SaveFileDialog diag = new SaveFileDialog()
        {
            Filter = $@"{fileExtensionBuilder}|*{fileExtensionBuilder}"
        };
        try
        {
            if (diag.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllLines($"{diag.FileName}", lines);
            }
        }
        catch (Exception _)
        {
            // ignored
        }
    }
}