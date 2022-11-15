//#define DRAW
//#define SHOW_POINTS
//#define RACZKA
//#define CZUBEK
#define SLOIK


using System.Collections.Generic;
using System.Numerics;
using MikCAD.BezierSurfaces;
using MikCAD.Extensions;
using MikCAD.Objects;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace MikCAD.Sciezki;

public partial class PathsGenerator
{
    public void GenerateDetailed(CutterType frez, uint radius)
    {
        var surfaces = new List<BezierSurfaceC2>();
        foreach (var obj in Scene.CurrentScene.ObjectsController.ParameterizedObjects)
        {
            if (obj is BezierSurfaceC2 surf)
            {
                surfaces.Add(surf);
            }
        }

        #region raczka

#if RACZKA
        {
            var finalPoints = new List<Vector3>();
            var lines = new List<List<Vector3>>();

            var detailed = new IIntersectableDecoratorStage2(surfaces[0])
            {
                DistanceFromSurface = radius / CmToMm
            };
            var samplesPerParam = 100;

            var u = 0.0f;
            var v = 0.0f;

            var dU = detailed.USize / samplesPerParam;
            var dV = detailed.VSize / samplesPerParam;


            for (int i = 0; i < samplesPerParam; i++)
            {
                v = 0.0f;
                var points = new List<Vector3>();
                for (int j = 0; j < samplesPerParam; j++)
                {
                    var point = detailed.GetValueAt(u, v);
                    if (point.isFinite())
                    {
                        (point.Y, point.Z) = (-point.Z, point.Y);
                        point *= CmToMm;
                        
                        if (point.Z >= SupportSize*CmToMm - 0.1)
                        {
                            points.Add(point);
                        }
                        else
                        {
                            
                        }
                    }

                    v += dV;
                }

                u += dU;
                if (i % 2 == 1)
                {
                    points.Reverse();
                }
                finalPoints.AddRange(points);
                nextIter: ;
            }

            SavePath(frez, radius, finalPoints);
        }
#endif

        #endregion
        
        #region sloik

#if SLOIK
        {
            var finalPoints = new List<Vector3>();
            var lines = new List<List<Vector3>>();

            var detailed = new IIntersectableDecoratorStage3(surfaces[1])
            {
                //DistanceFromSurface = radius / CmToMm
            };
            var detailedR = new IIntersectableDecoratorStage3(surfaces[0])
            {
                //DistanceFromSurface = radius / CmToMm
            };
            {
                int uSamplesCount = 100;
                int vSamplesCount = 100;
                
                float uu = 0;
                float vv = 0;

                float ddU = surfaces[1].USize / uSamplesCount;
                float ddV = surfaces[1].VSize / vSamplesCount;

                for (int i = 0; i < uSamplesCount; i++)
                {
                    vv = 0;
                    for (int j = 0; j < vSamplesCount; j++)
                    {
                        var org = surfaces[1].GetPositionAndGradient(uu, vv);
                        var ext = detailed.GetPositionAndGradient(uu, vv);
                        var deltaPos = (org.pos - ext.pos).Length;
                        var deltadU = (org.dU - ext.dU).Length;
                        var deltadV = (org.dV - ext.dV).Length;
                        vv += ddV;
                    }

                    uu += ddU;
                }
            }
            //detailed.Sample(20,20);
            //detailedR.Sample(20,20);
            
            var intersectN = new Intersection(surfaces[1], surfaces[0])
            {
                UseCursor = true,
                MaxPointsNumber = 10000,
                StartingPointsNumber = 1000,
            };
            var intersect = new Intersection(detailed, detailedR)
            {
                UseCursor = true,
                MaxPointsNumber = 10000,
                StartingPointsNumber = 10000,
                NewtonMaxIterations = 10000
            };
            if (intersect.Intersect())
            {
                intersect.ShowC0();
                intersect.ConvertToInterpolating();
            }
            if (intersectN.Intersect())
            {
                //intersectN.ShowC0();
                //intersectN.ConvertToInterpolating();
            }
            var samplesPerParam = 100;

            var u = 0.0f;
            var v = 0.0f;

            var dU = detailed.USize / samplesPerParam;
            var dV = detailed.VSize / samplesPerParam;


            for (int i = 0; i < samplesPerParam; i++)
            {
                v = 0.0f;
                var points = new List<Vector3>();
                for (int j = 0; j < samplesPerParam; j++)
                {
                    var point = detailed.GetValueAt(u, v);
                    if (point.isFinite())
                    {
                        (point.Y, point.Z) = (-point.Z, point.Y);
                        point *= CmToMm;
                        
                        if (point.Z >= SupportSize*CmToMm - 0.1)
                        {
                            points.Add(point);
                        }
                        else
                        {
                            
                        }
                    }

                    v += dV;
                }

                u += dU;
                if (i % 2 == 1)
                {
                    points.Reverse();
                }
                finalPoints.AddRange(points);
                nextIter: ;
            }

            SavePath(frez, radius, finalPoints, false);
        }
#endif

        #endregion
        
        #region sloik

#if CZUBEK
        {
            var finalPoints = new List<Vector3>();
            var lines = new List<List<Vector3>>();

            var detailed = new IIntersectableDecoratorStage2(surfaces[2])
            {
                DistanceFromSurface = radius / CmToMm
            };
            var samplesPerParam = 100;

            var u = 0.0f;
            var v = 0.0f;

            var dU = detailed.USize / samplesPerParam;
            var dV = detailed.VSize / samplesPerParam;


            for (int i = 0; i < samplesPerParam; i++)
            {
                v = 0.0f;
                var points = new List<Vector3>();
                for (int j = 0; j < samplesPerParam; j++)
                {
                    var point = detailed.GetValueAt(u, v);
                    if (point.isFinite())
                    {
                        (point.Y, point.Z) = (-point.Z, point.Y);
                        point *= CmToMm;
                        
                        if (point.Z >= SupportSize*CmToMm - 0.1)
                        {
                            points.Add(point);
                        }
                        else
                        {
                            
                        }
                    }

                    v += dV;
                }

                u += dU;
                if (i % 2 == 1)
                {
                    points.Reverse();
                }
                finalPoints.AddRange(points);
                nextIter: ;
            }

            SavePath(frez, radius, finalPoints);
        }
#endif

        #endregion
    }
}