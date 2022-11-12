using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
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
}

public class CuttingLines
{
    public CuttingLinePoint[] points { get; init; }

    public void SaveFile(CutterType frez, uint radius)
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
        foreach (var point in points)
        {
            lines.Add(
                $"N{firstInstructionNumber++}G01" +
                $"X{point.XPosInMm.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)}" +
                $"Y{point.YPosInMm.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)}" +
                $"Z{point.ZPosInMm.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)}");
        }

        SaveFileDialog diag = new SaveFileDialog()
        {
            Filter = $"{fileExtensionBuilder}|*{fileExtensionBuilder}"
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