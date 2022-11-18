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
            var mainPartFinalPoints = new List<Vector3>();
            var lines = new List<List<Vector3>>();

            var detailed = new IIntersectableDecoratorStage3(surfaces[1])
            {
                DistanceFromSurface = radius / CmToMm
            };
            var detailedR = new IIntersectableDecoratorStage3(surfaces[0])
            {
                DistanceFromSurface = radius / CmToMm
            };
            
            var detailedD = new IIntersectableDecoratorStage3(surfaces[2])
            {
                DistanceFromSurface = radius / CmToMm
            };

            //Przecięcie z górną częścią rączki
            // u1 = 8.440287f; v1 = 5.3860373f;
            // u2 = 0.28591698f; v2 = 7.5931625f;
            var intersectUp = new Intersection(detailed, detailedR)
            {
                MaxPointsNumber = 100,
                StartingPointsNumber = 10000,
                NewtonMaxIterations = 10000
            };
            
            var u1 = 8.440287f;
            var v1 = 5.3860373f;
            
            var u2 = 0.28591698f;
            var v2 = 7.5931625f;
            if (intersectUp.Intersect((detailed.GetValueAt(u1, v1), u1, v1),
                    (detailedR.GetValueAt(u2,v2),u2,v2),true))
            {
                intersectUp.ShowC0();
                //intersectUp.ConvertToInterpolating();
            }
            
            //Przecięcie z dolną częścią rączki
            // u1 = 8.309978f; v1 = 3.2709327f;
            // u2 = 0.24050847f; v2 = 0.511595f;
            
            var intersectDown = new Intersection(detailed, detailedR)
            {
                UseCursor = true,
                MaxPointsNumber = 200,
                StartingPointsNumber = 10000,
                NewtonMaxIterations = 10000
            };
            
            u1 = 8.309978f;
            v1 = 3.2709327f;
            
            u2 = 0.24050847f;
            v2 = 0.511595f;
            
            if (intersectDown.Intersect((detailed.GetValueAt(u1, v1), u1, v1),
                    (detailedR.GetValueAt(u2,v2),u2,v2),true))
            {
                intersectDown.ShowC0();
                //intersectDown.ConvertToInterpolating();
            }
            
            //Przecięcie z dziubkiem
            // u1 = 2.566685f; v1 = 4.3139234f;
            // u2 = 3.96224f; v2 = 0.8268527f;
            var intersectLeft = new Intersection(detailed, detailedD)
            {
                MaxPointsNumber = 10000,
                StartingPointsNumber = 10000,
                NewtonMaxIterations = 10000
            };

            u1 = 2.566685f;
            v1 = 4.3139234f;
            
            u2 = 3.96224f;
            v2 = 0.8268527f;
            
            if (intersectLeft.Intersect((detailed.GetValueAt(u1, v1), u1, v1),
                    (detailedD.GetValueAt(u2,v2),u2,v2),true))
            {
                intersectLeft.ShowC0();
               // intersectLeft.ConvertToInterpolating();
            }

            var samplesPerParam = 110;

            var u = 0.0f;
            var v = 0.0f;

            var dU = detailed.USize / samplesPerParam;
            var dV = detailed.VSize / samplesPerParam;

            var mod = 0;
            
            for (int i = 0; i < samplesPerParam; i++)
            {
                v = 0.0f;
                var points = new List<Vector3>();
                for (int j = 0; j < samplesPerParam; j++)
                {
                    if (intersectUp.IsInside(u, v)
                        || intersectDown.IsInside(u,v)
                        || intersectLeft.IsInside(u,v))
                    {
                        break;
                    }
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
                if ((i + mod) % 2 == 1)
                {
                    points.Reverse();
                }
                mainPartFinalPoints.AddRange(points);
                nextIter: ;
                if (i == 42)
                {
                    points.Reverse();
                    mainPartFinalPoints.AddRange(points);
                    mod++;
                }

                if (i == 103)
                {
                    points.Reverse();
                    mainPartFinalPoints.AddRange(points);
                    mod++;
                }
                
                if (i == 104)
                {
                    points.Reverse();
                    mainPartFinalPoints.AddRange(points);
                    mod++;
                }
                
                if (i == 105)
                {
                    points.Reverse();
                    mainPartFinalPoints.AddRange(points);
                    mod++;
                }
            }


            var middleRightPart = new List<Vector3>();
            u = 6f;
            mod = 0;
            
            for (int i = 0; i < samplesPerParam; i++)
            {
                v =3f;
                var points = new List<Vector3>();
                for (int j = 0; j < samplesPerParam; j++)
                {
                    var point = detailed.GetValueAt(u, v);
                    if (intersectDown.IsInside(u, v))
                    {
                        v += dV;
                        continue;
                    }
                    if (intersectUp.IsInside(u, v))
                    {
                        break;
                    }
                    if (point.isFinite())
                    {
                        (point.Y, point.Z) = (-point.Z, point.Y);
                        point *= CmToMm;

                        if (point.Z >= SupportSize * CmToMm - 0.1)
                        {
                            points.Add(point);
                        }
                        else
                        {

                        }
                    }
                    v += dV;
                    if (v > 6)
                    {
                        break;
                    }
                    
                }
                if ((i + mod) % 2 == 1)
                {
                    points.Reverse();
                }
                middleRightPart.AddRange(points);
                
                u += dU;
                if (u > 8.5f)
                {
                    break;
                }
            }
            
            
            SavePath(frez, radius, middleRightPart, false);
        }
#endif

        #endregion
        
        #region sloik

#if CZUBEK
        {
            var finalPoints = new List<Vector3>();
            var lines = new List<List<Vector3>>();

            var detailed = new IIntersectableDecoratorStage3(surfaces[2])
            {
                DistanceFromSurface = radius / CmToMm
            };
            var samplesPerParam = 100;

            var u = 0.0f;
            var v = 0.0f;

            var dU = detailed.USize / samplesPerParam;
            var dV = detailed.VSize / samplesPerParam;

            detailed.Sample(20,20);

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
    }
}