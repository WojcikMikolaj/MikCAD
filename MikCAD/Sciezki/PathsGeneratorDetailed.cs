//#define DRAW
//#define SHOW_POINTS

#define RACZKA
//#define CZUBEK
#define SLOIK


using System.Collections.Generic;
using System.Linq;
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
                (detailedR.GetValueAt(u2, v2), u2, v2), true))
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
                (detailedR.GetValueAt(u2, v2), u2, v2), true))
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
                (detailedD.GetValueAt(u2, v2), u2, v2), true))
        {
            intersectLeft.ShowC0();
            // intersectLeft.ConvertToInterpolating();
        }

        var finalPoints = new List<Vector3>();
        
        #region raczka

#if RACZKA
        {
            var finalRPoints = new List<Vector3>();
            var lines = new List<List<Vector3>>();

            var samplesPerParam = 100;
            
            var dU = detailedR.USize / samplesPerParam;
            var dV = detailedR.VSize / samplesPerParam;
            var u = 0f;

            for (int i = 0; i < samplesPerParam/2; i++)
            {
                var v = intersectDown.GetVOnULineSecondObject(u);
                var endv = intersectUp.GetVOnULineSecondObject(u); 
                var points = new List<Vector3>();
                for (int j = 0; j < samplesPerParam; j++)
                {
                    
                    if (i<samplesPerParam/12||((j < 45 || j>65)&&(j < 13 || j > 30)))
                    {
                        var point = detailedR.GetValueAt(u, v);
                        if (point.isFinite())
                        {
                            (point.Y, point.Z) = (-point.Z, point.Y);
                            point *= CmToMm;

                            // var l = -1;
                            // for (int k = 0; k < points.Count; k++)
                            // {
                            //     if (Vector3.DistanceSquared(points[k], point) < 0.06f)
                            //     {
                            //         l = k;
                            //     }
                            // }
                            //
                            // if (l != -1)
                            // {
                            //     points.RemoveRange(l, points.Count - l);
                            // }

                            if (point.Z >= SupportSize * CmToMm)
                            {
                                points.Add(point);
                            }
                            else
                            {
                            }
                        }
                    }

                    v += dV;
                    if (v > endv)
                    {
                        break;
                    }
                }

                u += dU;
                if (i % 2 == 1)
                {
                    points.Reverse();
                }
                finalRPoints.AddRange(points);

            }
            finalRPoints.RemoveAt(finalRPoints.Count-1);
            finalRPoints.RemoveAt(finalRPoints.Count-1);
            finalRPoints.RemoveAt(finalRPoints.Count-1);
            AddMoveFromAndToCenter(finalRPoints);
            finalPoints.AddRange(finalRPoints);
            
            
            finalRPoints = new List<Vector3>();
            dU = detailedR.USize / samplesPerParam;
            dV = detailedR.VSize / samplesPerParam;
            u = detailedR.USize / 2 + 6*dU;

            for (int i = samplesPerParam/2+6; i < samplesPerParam; i++)
            {
                var v = intersectDown.GetVOnULineSecondObject(u);
                var endv = intersectUp.GetVOnULineSecondObject(u); 
                var points = new List<Vector3>();
                for (int j = 0; j < samplesPerParam; j++)
                {
                    var point = detailedR.GetValueAt(u, v);
                    if (point.isFinite())
                    {
                        (point.Y, point.Z) = (-point.Z, point.Y);
                        point *= CmToMm;

                        if (point.Z >= SupportSize * CmToMm)
                        {
                            points.Add(point);
                        }
                        else
                        {
                        }
                    }
                    v += dV;
                    if (v > endv)
                    {
                        break;
                    }
                }

                u += dU;
                if (i % 2 == 1)
                {
                    points.Reverse();
                }
                finalRPoints.AddRange(points);

            }
            finalRPoints.RemoveAt(0);
            finalRPoints.RemoveAt(0);
            finalRPoints.RemoveAt(0);
            finalRPoints.RemoveAt(0);
            AddMoveFromAndToCenter(finalRPoints);
            finalPoints.AddRange(finalRPoints);
        }
#endif

        #endregion

        #region sloik

#if SLOIK
        {
            var mainPartFinalPoints = new List<Vector3>();
            var lines = new List<List<Vector3>>();

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
                        || intersectDown.IsInside(u, v)
                        || intersectLeft.IsInside(u, v))
                    {
                        break;
                    }

                    var point = detailed.GetValueAt(u, v);
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

            AddMoveFromAndToCenter(mainPartFinalPoints);

            var middleRightPart = new List<Vector3>();
            u = 0f;
            mod = 0;

            for (int i = 0; i < samplesPerParam; i++)
            {
                if (i >= 97)
                {
                    v = 3f;
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
                }

                u += dU;
                if (u > 8.5f)
                {
                    break;
                }
            }

            AddMoveFromAndToCenter(middleRightPart);
            mainPartFinalPoints.AddRange(middleRightPart);


            var upRightPart = new List<Vector3>();
            u = 0f;
            mod = 0;

            for (int i = 0; i < samplesPerParam; i++)
            {
                if (i >= 97)
                {
                    v = 6f;
                    var points = new List<Vector3>();
                    for (int j = 0; j < samplesPerParam; j++)
                    {
                        var point = detailed.GetValueAt(u, v);
                        if (intersectUp.IsInside(u, v))
                        {
                            v += dV;
                            continue;
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
                        if (v > detailed.VSize)
                        {
                            break;
                        }
                    }

                    if ((i + mod) % 2 == 1)
                    {
                        points.Reverse();
                    }

                    upRightPart.AddRange(points);
                }

                u += dU;
                if (u > 8.55f)
                {
                    break;
                }
            }

            AddMoveFromAndToCenter(upRightPart);
            mainPartFinalPoints.AddRange(upRightPart);

            var leftPart = new List<Vector3>();
            u = 0f;
            mod = 0;

            for (int i = 0; i < samplesPerParam; i++)
            {
                if (i >= 32)
                {
                    v = 4.4f;
                    var points = new List<Vector3>();
                    for (int j = 0; j < samplesPerParam; j++)
                    {
                        var point = detailed.GetValueAt(u, v);
                        if (intersectLeft.IsInside(u, v))
                        {
                            v += dV;
                            continue;
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
                        if (v > detailed.VSize)
                        {
                            break;
                        }
                    }

                    if ((i + mod) % 2 == 1)
                    {
                        points.Reverse();
                    }

                    leftPart.AddRange(points);
                }

                u += dU;
                if (u > 6f)
                {
                    break;
                }
            }

            AddMoveFromAndToCenter(leftPart);
            mainPartFinalPoints.AddRange(leftPart);
            
            bool useHelp = false;
            bool exitOnBrake = false;
            var interDownPoints = new List<Vector3>();
            foreach (var p in intersectDown.points)
            {
                var point = new Vector3()
                {
                    X = p.pos.X,
                    Y = -p.pos.Z,
                    Z = p.pos.Y
                };
                point *= CmToMm;
                if (point.Z > SupportSize * CmToMm)
                {
                    if (!useHelp)
                    {

                    }
                    else
                    {
                        exitOnBrake = true;
                        interDownPoints.Add(point);
                    }
                }
                else
                {
                    if (exitOnBrake)
                    {
                        break;
                    }
                    useHelp = true;
                    continue;
                }
            }
            //AddMoveFromAndToCenter(interDownPoints);
            //mainPartFinalPoints.AddRange(interDownPoints);
            
            useHelp = false;
            exitOnBrake = false;
            var interUpPoints = new List<Vector3>();
            var interUpPointsHelp = new List<Vector3>();
            foreach (var p in intersectUp.points)
            {
                var point = new Vector3()
                {
                    X = p.pos.X,
                    Y = -p.pos.Z,
                    Z = p.pos.Y
                };
                point *= CmToMm;
                if (point.Z > SupportSize * CmToMm)
                {
                    if (!useHelp)
                    {
                        interUpPoints.Add(point);
                    }
                    else
                    {
                        exitOnBrake = true;
                        interUpPointsHelp.Add(point);
                    }
                }
                else
                {
                    if (exitOnBrake)
                    {
                        break;
                    }
                    useHelp = true;
                    continue;
                }
            }
            interUpPointsHelp.AddRange(interUpPoints);
            //AddMoveFromAndToCenter(interUpPointsHelp);
            //mainPartFinalPoints.AddRange(interUpPointsHelp);
            
            interUpPointsHelp.AddRange(interDownPoints);
            AddMoveFromAndToCenter(interUpPointsHelp);
            mainPartFinalPoints.AddRange(interUpPointsHelp);
            
            useHelp = false;
            exitOnBrake = false;
            var interLeftPoints = new List<Vector3>();
            var interLeftPointsHelp = new List<Vector3>();
            foreach (var p in intersectLeft.points)
            {
                var point = new Vector3()
                {
                    X = p.pos.X,
                    Y = -p.pos.Z,
                    Z = p.pos.Y
                };
                point *= CmToMm;
                if (point.Z > SupportSize * CmToMm)
                {
                    if (!useHelp)
                    {
                        interLeftPoints.Add(point);
                    }
                    else
                    {
                        exitOnBrake = true;
                        interLeftPointsHelp.Add(point);
                    }
                }
                else
                {
                    if (exitOnBrake)
                    {
                        break;
                    }
                    useHelp = true;
                    continue;
                }
            }
            interLeftPointsHelp.AddRange(interLeftPoints);
            AddMoveFromAndToCenter(interLeftPointsHelp);
            mainPartFinalPoints.AddRange(interLeftPointsHelp);
            
            finalPoints.AddRange(mainPartFinalPoints);
        }
#endif

        #endregion

        #region sloik

#if CZUBEK
        {
            var finalPoints = new List<Vector3>();
            var lines = new List<List<Vector3>>();
            
            var samplesPerParam = 100;

            var u = 0.0f;
            var v = 0.0f;

            var dU = detailedD.USize / samplesPerParam;
            var dV = detailedD.VSize / samplesPerParam;

            for (int i = 0; i < samplesPerParam; i++)
            {
                v = 0.0f;
                var points = new List<Vector3>();
                for (int j = 0; j < samplesPerParam; j++)
                {
                    var point = detailedD.GetValueAt(u, v);
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
            
            AddMoveFromAndToCenter(finalPoints);
            SavePath(frez, radius, finalPoints, false);
        }
#endif

        #endregion
        SavePath(frez, radius,finalPoints, false);
    }

    private void AddMoveFromAndToCenter(List<Vector3> list)
    {
        list.Insert(0, new Vector3()
        {
            X = 0,
            Y = 0,
            Z = 2 * ZBlockSize * CmToMm,
        });
        list.Insert(1, new Vector3()
        {
            X = list[1].X,
            Y = list[1].Y,
            Z = 2 * ZBlockSize * CmToMm,
        });
        list.Add(new Vector3()
        {
            X = list[^1].X,
            Y = list[^1].Y,
            Z = 2 * ZBlockSize * CmToMm,
        });
        list.Add(list[0]);
    }
}