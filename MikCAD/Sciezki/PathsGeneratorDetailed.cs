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

            var detailed = new IIntersectableDecoratorStage2(surfaces[1])
            {
                DistanceFromSurface = radius / CmToMm
            };
            var detailedR = new IIntersectableDecoratorStage2(surfaces[0])
            {
                DistanceFromSurface = radius / CmToMm
            };

            var intersect = new Intersection(surfaces[1], surfaces[0]);
            //var intersect = new Intersection(detailed, detailedR);
            if (intersect.Intersect())
            {
                //intersect.ShowC0();
                //intersect.ShowC0DecoratedDecorated(detailedR, detailed);
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