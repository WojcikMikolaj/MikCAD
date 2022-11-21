#define SPECIALIZED
//#define GENERIC 
#define DRAW
#define ADD_POINTS
//#define SHOW_POINTS
#define RACZKA
#define CZUBEK
#define SLOIK

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Speech.Synthesis.TtsEngine;
using MikCAD.BezierSurfaces;
using MikCAD.CustomControls;
using MikCAD.Extensions;
using MikCAD.Objects;
using MikCAD.Utilities;
using OpenTK.Mathematics;

namespace MikCAD.Sciezki;

public partial class PathsGenerator
{
    public void GenerateFlatEnvelope(CutterType frez, uint radius)
    {
        var surfaces = new List<BezierSurfaceC2>();
        BezierSurfaceC2 supportSurface = null;

        AddSupportSurface();

        foreach (var o in Scene.CurrentScene.ObjectsController.ParameterizedObjects)
        {
            if (o is BezierSurfaceC2 surf)
            {
                if (o != Scene.CurrentScene.ObjectsController.ParameterizedObjects.Last())
                {
                    surfaces.Add(surf);
                }
                else
                {
                    supportSurface = surf;
                }
            }
        }

        var finalPoints = new List<Vector3>();
        var pointsR = new List<Vector3>();
        var pointsGR = new List<Vector3>();
        var pointsD = new List<Vector3>();
        var pointsDL = new List<Vector3>();
        var pointsGLL = new List<Vector3>();
        var pointsGL = new List<Vector3>();
#if SPECIALIZED

        #region raczka

#if RACZKA
        {
            if (surfaces.Count >= 0)
            {
                //punkty startowe
                //1) u=2.3709621; v=4.4300275;
                //2) u=3.1167536; v=5.4869013;
                var u1 = 2.3709621f;
                var v1 = 4.4300275f;

                var u2 = 3.1167536f;
                var v2 = 5.4869013f;

                var decorated = new IIntersectableDecoratorStage2(surfaces[0])
                {
                    DistanceFromSurface = radius * 1.3f / CmToMm
                };
                var intersection = new Intersection(supportSurface, surfaces[0])
                {
                    StartingPointsNumber = 10000,
                    MaxPointsNumber = 1000,
                    UseRandom = true,
                };
                var result = intersection.Intersect((supportSurface.GetValueAt(u1, v1), u1, v1),
                    (surfaces[0].GetValueAt(u2, v2), u2, v2), true);

                if (result)
                {
#if DRAW
                    intersection.ShowC0Decorated(decorated);
#endif
#if ADD_POINTS
                    var it = 0;
                    foreach (var point in intersection.points)
                    {
                        var p = decorated.GetValueAt(point.s, point.t) * CmToMm;
                        (p.Y, p.Z) = (-p.Z, p.Y);
                        p.Z = SupportSize * CmToMm;
                        if (it is >= 43 and <= 85)
                        {
                            pointsR.Add(p);
                        }

                        if (it is >= 98 and <= 144)
                        {
                            pointsR.Add(p);
                        }

                        it++;
                    }
#endif
#if SHOW_POINTS
                    intersection.ConvertToInterpolatingDecorated(decorated);
#endif
                }
            }
        }
#endif

        #endregion


        #region sloik

#if SLOIK
        {
            if (surfaces.Count >= 2)
            {
                //punkty startowe
                //1) u=1.4388611; v=3.082061;
                //2) u=8.197167; v=1.1560346;
                var u1 = 1.4388611f;
                var v1 = 3.082061f;

                var u2 = 8.197167f;
                var v2 = 1.1560346f;

                var decorated = new IIntersectableDecoratorStage3(surfaces[1])
                {
                    DistanceFromSurface = radius *1.1f/ CmToMm
                };
                var intersection = new Intersection(supportSurface, decorated)
                {
                    StartingPointsNumber = 10000,
                    MaxPointsNumber = 1000,
                    UseRandom = true,
                };
                var result = intersection.Intersect((supportSurface.GetValueAt(u1, v1), u1, v1),
                    (decorated.GetValueAt(u2, v2), u2, v2), true);

                if (result)
                {
#if DRAW
                    intersection.ShowC0();
#endif
#if ADD_POINTS
                    var it = 0;
                    var pom = decorated.GetValueAt(intersection.points[1].s, intersection.points[1].t) * CmToMm;
                    pointsGL.Add(new Vector3()
                    {
                        X = pom.X + 1.2f * radius,
                        Y = -pom.Z,
                        Z = SupportSize * CmToMm
                    });
                    foreach (var point in intersection.points)
                    {
                        var p = decorated.GetValueAt(point.s, point.t) * CmToMm;
                        (p.Y, p.Z) = (-p.Z, p.Y);
                        p.Z = SupportSize * CmToMm;
                        if (it is >= 371 and <= 424)
                        {
                            pointsGR.Add(p);
                        }

                        if (it is >= 110 and <= 289)
                        {
                            pointsD.Add(p);
                        }

                        if (it is >= 1 and <= 44)
                        {
                            pointsGL.Add(p);
                        }

                        it++;
                    }

                    pointsGR.Add(new Vector3()
                    {
                        X = pointsGL[0].X,
                        Y = pointsGR[^1].Y,
                        Z = SupportSize * CmToMm,
                    });
#endif
#if SHOW_POINTS
                    intersection.ConvertToInterpolating();
#endif
                }
            }
        }
#endif

        #endregion

#if CZUBEK

        #region czubek

        {
            if (surfaces.Count >= 3)
            {
                //GORA
                //1) u=3.1762052; v=0.59365934;
                //2) u=3.0921733; v=6.8938847;
                {
                    var u1 = 3.1762052f;
                    var v1 = 0.59365934f;

                    var u2 = 3.0921733f;
                    var v2 = 6.8938847f;
                    var decorated = new IIntersectableDecoratorStage2(surfaces[2])
                    {
                        DistanceFromSurface = radius * 1.25f / CmToMm
                    };

                    var intersection = new Intersection(supportSurface, surfaces[2])
                    {
                    };
                    var result = intersection.Intersect((supportSurface.GetValueAt(u1, v1), u1, v1),
                        (surfaces[2].GetValueAt(u2, v2), u2, v2), true);

                    if (result)
                    {
#if DRAW
                        intersection.ShowC0();
#endif
#if ADD_POINTS
                        var it = 0;
                        foreach (var point in intersection.points)
                        {
                            var p = decorated.GetValueAt(point.s, point.t) * CmToMm;
                            (p.Y, p.Z) = (-p.Z, p.Y);
                            p.Z = SupportSize * CmToMm;
                            if (it is >= 60 and <= 120)
                            {
                                pointsGLL.Add(p);
                            }

                            it++;
                        }

                        pointsGLL.Add(new Vector3()
                        {
                            X = pointsGLL[^1].X,
                            Y = pointsGLL[^1].Y + 1.5f * radius,
                            Z = SupportSize * CmToMm
                        });
#endif
#if SHOW_POINTS
                         intersection.ConvertToInterpolating();
#endif
                    }
                }
                //DOL
                //1) u=2.5; v=0.7;
                //2) u=0.92; v=4.55;
                {
                    var u1 = 2.5f;
                    var v1 = 0.7f;

                    var u2 = 0.92f;
                    var v2 = 4.55f;

                    var decorated = new IIntersectableDecoratorStage2(surfaces[2])
                    {
                        DistanceFromSurface = radius * 1.3f / CmToMm
                    };

                    var intersection = new Intersection(supportSurface, decorated)
                    {
                        StartingPointsNumber = 10000,
                        MaxPointsNumber = 10000,
                        UseRandom = false,
                        UseCursor = true
                    };
                    Scene.CurrentScene.ObjectsController._pointer.posX = 0f;
                    Scene.CurrentScene.ObjectsController._pointer.posY = 2.07f;
                    Scene.CurrentScene.ObjectsController._pointer.posZ = -5.94f;
                    var result = intersection.Intersect((supportSurface.GetValueAt(u1, v1), u1, v1),
                        (decorated.GetValueAt(u2, v2), u2, v2), true);
                    if (result)
                    {
#if DRAW
                        intersection.ShowC0();
#endif
#if ADD_POINTS
                        var it = 0;
                        foreach (var point in intersection.points)
                        {
                            var p = supportSurface.GetValueAt(point.u, point.v) * CmToMm;
                            (p.Y, p.Z) = (-p.Z, p.Y);
                            p.Z = SupportSize * CmToMm;
                            pointsDL.Add(p);

                            it++;
                        }
                        pointsDL.RemoveAt(pointsDL.Count-1);
                        pointsDL.RemoveAt(pointsDL.Count-1);
                        pointsDL.RemoveAt(pointsDL.Count-1);
#endif
#if SHOW_POINTS
                        intersection.ConvertToInterpolating();
#endif
                    }
                }
            }
        }

        #endregion
#endif

#if ADD_POINTS
        finalPoints.Add(new Vector3()
        {
            X = 0,
            Y = 0,
            Z = 2 * ZBlockSize * CmToMm
        });
        var introPoint = pointsGL[0];
        introPoint.X = XBlockSize / 2 * CmToMm + 2 * radius;
         introPoint.Z = 2 * ZBlockSize * CmToMm;
         finalPoints.Add(introPoint);
         introPoint.Z = pointsGL[0].Z;
         finalPoints.Add(introPoint);
         finalPoints.AddRange(pointsGL);
         finalPoints.AddRange(pointsGLL);
         finalPoints.Add(new Vector3()
         {
             X = 2.29f*CmToMm,
             Y = 7.18f*CmToMm,
             Z = pointsDL[0].Z
         });
         finalPoints.AddRange(pointsDL);
         finalPoints.AddRange(pointsD);
        finalPoints.AddRange(pointsR);
        finalPoints.AddRange(pointsGR);
        finalPoints.Add(pointsGL[0]);
        finalPoints.Add(finalPoints[2]);
        finalPoints.Add(finalPoints[1]);
        finalPoints.Add(finalPoints[0]);
        for(int i = 0; i < finalPoints.Count; i++)
        {
            finalPoints[i] = finalPoints[i] with {Z = finalPoints[i].Z + 0.02f};
        }
#endif
#endif
        SavePath(frez, radius, finalPoints, false);
    }
}