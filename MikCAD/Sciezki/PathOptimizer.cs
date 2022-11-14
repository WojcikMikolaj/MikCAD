using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace MikCAD.Sciezki;

public static class PathOptimizer
{
    public static List<CuttingLinePoint> OptimizePaths(List<CuttingLinePoint> points, float epsilon, bool fullOptimize)
    {
        var punktyPoPierwszymPrzebiegu = FirstPass(points, epsilon);
        if (!fullOptimize)
        {
            return punktyPoPierwszymPrzebiegu;
        }
        var punktyPoDrugimPrzebiegu = SecondPass(punktyPoPierwszymPrzebiegu, epsilon);
        return punktyPoDrugimPrzebiegu;
    }

    private static List<CuttingLinePoint> FirstPass(List<CuttingLinePoint> points, float epsilon)
    {
        if (points.Count == 0)
        {
            return points;
        }
        
        List<CuttingLinePoint> punktyWynikowe = new List<CuttingLinePoint>();
        if (points.Count > 1)
        {
            
            var lastPoint = points[0];
            punktyWynikowe.Add(points[0]);
            
            for (int i = 0; i < points.Count;)
            {
                if (points[i].DistanceSquaredTo(lastPoint) < epsilon)
                {
                    points.RemoveAt(i);
                    continue;
                }

                var pkt = points[i];
                
                if (MathF.Abs(pkt.XPosInMm - lastPoint.XPosInMm) < epsilon)
                {
                    pkt.WriteX = false;
                }
                
                if (MathF.Abs(pkt.YPosInMm - lastPoint.YPosInMm) < epsilon)
                {
                    pkt.WriteY = false;
                }
                
                if (MathF.Abs(pkt.ZPosInMm - lastPoint.ZPosInMm) < epsilon)
                {
                    pkt.WriteZ = false;
                }

                if (!pkt.WriteX && !pkt.WriteY && !pkt.WriteZ)
                {
                    points.RemoveAt(i);
                    continue;
                }
                
                punktyWynikowe.Add(pkt);
                
                lastPoint = pkt;
                i++;
            }
        }
        return punktyWynikowe;
    }

    private static List<CuttingLinePoint> SecondPass(List<CuttingLinePoint> points, float epsilon)
    {
        if (points.Count == 0)
        {
            return points;
        }
        
        List<CuttingLinePoint> punktyWynikowe = new List<CuttingLinePoint>();
        if (points.Count > 1)
        {
            
            var lastPoint = points[0];
            punktyWynikowe.Add(points[0]);
            
            for (int i = 0; i < points.Count-1;)
            {
                var pkt = points[i];

                if (pkt.GetWritePos() == CuttingLinePoint.WritePos.None ||
                    (pkt.GetWritePos() == lastPoint.GetWritePos() && pkt.GetWritePos() == points[i + 1].GetWritePos()))
                {
                    points.RemoveAt(i);
                    continue;
                }
                
                
                punktyWynikowe.Add(pkt);
                
                lastPoint = pkt;
                i++;
            }
        }
        return punktyWynikowe;
    }
}