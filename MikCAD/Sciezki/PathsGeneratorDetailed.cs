//#define DRAW
//#define SHOW_POINTS

#define RACZKA
#define CZUBEK
#define SLOIK


using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MikCAD.BezierSurfaces;
using MikCAD.Extensions;
using MikCAD.Objects;
using OpenTK.Audio.OpenAL.Extensions.Creative.EFX;
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
        // u1 = 5.3068123f; v1 = 5.462653f;
        // u2 = 2.431608f; v2 = 0.07239506f;
        var intersectLeft = new Intersection(detailed, detailedD)
        {
            MaxPointsNumber = 200,
            StartingPointsNumber = 10000,
            NewtonMaxIterations = 10000
        };

        u1 = 5.3068123f;
        v1 = 5.462653f;

        u2 = 2.431608f;
        v2 = 0.07239506f;


        if (intersectLeft.Intersect((detailed.GetValueAt(u1, v1), u1, v1),
                (detailedD.GetValueAt(u2, v2), u2, v2), true))
        {
            intersectLeft.ShowC0();
            // intersectLeft.ConvertToInterpolating();
        }

        var finalPoints = new List<Vector3>();

        var raczkaZewnatrz = new List<Vector3>();
        var raczkaWewnatrz = new List<Vector3>();

        var sloikSrodek = new List<Vector3>();
        var sloikPrawoGora = new List<Vector3>();
        var sloikPrawoPomiedzy = new List<Vector3>();
        var sloikLewo = new List<Vector3>();

        var dziubekLewoGora = new List<Vector3>();
        var dziubekLewoDol = new List<Vector3>();

        var dziubekPrawoGora = new List<Vector3>();
        var dziubekPrawoDol = new List<Vector3>();

        var przeciecieRaczka = new List<Vector3>();
        var przeciecieDziubek = new List<Vector3>();

        var dziura = new List<Vector3>();

        var C0 = new List<Vector3>();

        #region raczka

#if RACZKA
        {
            var finalRPoints = new List<Vector3>();
            var lines = new List<List<Vector3>>();

            var samplesPerParam = 100;

            var dU = detailedR.USize / samplesPerParam;
            var dV = detailedR.VSize / samplesPerParam;
            var u = 0f;

            for (int i = 0; i < samplesPerParam / 2; i++)
            {
                var v = intersectDown.GetVOnULineSecondObject(u) + 0.05f;
                var endv = intersectUp.GetVOnULineSecondObject(u);
                var points = new List<Vector3>();
                for (int j = 0; j < samplesPerParam; j++)
                {
                    //if(j>0)
                    if (i < samplesPerParam / 12 || ((j < 45 || j > 65) && (j < 13 || j > 30)))
                    {
                        var point = detailedR.GetValueAt(u, v);
                        if (point.isFinite())
                        {
                            (point.Y, point.Z) = (-point.Z, point.Y /*-radius/CmToMm*/);
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

                            if (point.Z >= SupportSize * CmToMm - 0.1)
                            {
                                points.Add(point);
                            }
                            else
                            {
                            }
                        }
                    }

                    v += dV;
                    if (v > endv - 0.1f)
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

            finalRPoints.RemoveAt(finalRPoints.Count - 1);
            finalRPoints.RemoveAt(finalRPoints.Count - 1);
            finalRPoints.RemoveAt(finalRPoints.Count - 1);

            raczkaWewnatrz.AddRange(finalRPoints);

            AddMoveFromAndToCenter(finalRPoints);
            finalPoints.AddRange(finalRPoints);


            finalRPoints = new List<Vector3>();
            dU = detailedR.USize / samplesPerParam;
            dV = detailedR.VSize / samplesPerParam;
            u = detailedR.USize / 2 + 6 * dU;

            for (int i = samplesPerParam / 2 + 6; i < samplesPerParam; i++)
            {
                var v = intersectDown.GetVOnULineSecondObject(u) + 0.05f;
                var endv = intersectUp.GetVOnULineSecondObject(u);
                var points = new List<Vector3>();
                for (int j = 0; j < samplesPerParam; j++)
                {
                    var point = detailedR.GetValueAt(u, v);
                    if (point.isFinite())
                    {
                        (point.Y, point.Z) = (-point.Z, point.Y /*-radius/CmToMm*/);
                        point *= CmToMm;

                        if (point.Z >= SupportSize * CmToMm - 0.05)
                        {
                            points.Add(point);
                        }
                        else
                        {
                        }
                    }

                    v += dV;
                    if (v > endv - 0.03f)
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
            // finalRPoints.RemoveAt(0);
            // finalRPoints.RemoveAt(0);
            // finalRPoints.RemoveAt(0);
            // finalRPoints.RemoveAt(0);
            // finalRPoints.RemoveAt(0);
            // finalRPoints.RemoveAt(0);
            // finalRPoints.RemoveAt(0);
            // finalRPoints.RemoveAt(0);
            // finalRPoints.RemoveAt(0);
            // finalRPoints.RemoveAt(0);
            // finalRPoints.RemoveAt(0);

            raczkaZewnatrz.AddRange(finalRPoints);

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
                        (point.Y, point.Z) = (-point.Z, point.Y /*-radius/CmToMm*/);
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

            sloikSrodek.AddRange(mainPartFinalPoints);

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
                            (point.Y, point.Z) = (-point.Z, point.Y /*-radius/CmToMm*/);
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
                if (u > 8.55f)
                {
                    break;
                }
            }

            sloikPrawoPomiedzy.AddRange(middleRightPart);

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
                            (point.Y, point.Z) = (-point.Z, point.Y /*-radius/CmToMm*/);
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

            sloikPrawoGora.AddRange(upRightPart);

            AddMoveFromAndToCenter(upRightPart);
            mainPartFinalPoints.AddRange(upRightPart);

            var leftPart = new List<Vector3>();
            u = 0f;
            mod = 0;

            for (int i = 0; i < samplesPerParam; i++)
            {
                if (i >= 32)
                {
                    v = 5f;
                    var points = new List<Vector3>();
                    for (int j = 0; j < samplesPerParam; j++)
                    {
                        var point = detailed.GetValueAt(u, v);
                        if ((intersectLeft.IsInside(u, v) && i < 35)
                            || (intersectLeft.IsInside(u, v - 0.1f) && i == 35)
                            || (intersectLeft.IsInside(u, v - 0.1f) && i == 36)
                            || (intersectLeft.IsInside(u, v) && i >= 37)
                            )
                        {
                            v += dV;
                            continue;
                        }

                        if (point.isFinite())
                        {
                            (point.Y, point.Z) = (-point.Z, point.Y /*-radius/CmToMm*/);
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

            sloikLewo.AddRange(leftPart);

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

            przeciecieRaczka.AddRange(interUpPointsHelp);

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
                        interLeftPoints.Add(point with {Z = point.Z + .15f});
                    }
                    else
                    {
                        exitOnBrake = true;
                        interLeftPointsHelp.Add(point with
                        {
                            Z = point.Z + .15f
                        });
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

            przeciecieDziubek.AddRange(interLeftPointsHelp);

            AddMoveFromAndToCenter(interLeftPointsHelp);
            mainPartFinalPoints.AddRange(interLeftPointsHelp);

            finalPoints.AddRange(mainPartFinalPoints);
        }
#endif

        #endregion

        #region dziubek

#if CZUBEK
        {
            var finalDPoints = new List<Vector3>();
            var lines = new List<List<Vector3>>();

            var samplesPerParam = 100;
            var samplesPerParamV = 200;

            var u = 0.0f;

            var dU = detailedD.USize / samplesPerParam;
            var dV = detailedD.VSize / samplesPerParamV;


            //gora
            for (int i = 0; i < samplesPerParam; i++)
            {
                if (i > samplesPerParam / 2)
                {
                    var v = 0.0f;
                    var startv = intersectLeft.GetVOnULineSecondObject(u);
                    var points = new List<Vector3>();
                    for (int j = 0; j < 104; j++)
                    {
                        if ((v >= startv + 0.14f && i > samplesPerParam / 2 + 48)
                            || (v >= startv + 0.17f && i > samplesPerParam / 2 + 45 && i <= samplesPerParam / 2 + 48)
                            || (v >= startv + 0.20f && i > samplesPerParam / 2 + 39 && i <= samplesPerParam / 2 + 45)
                            || (v >= startv + 0.18f && i == samplesPerParam / 2 + 39)
                            || (v >= startv + 0.18f && i == samplesPerParam / 2 + 38)
                            || (v >= startv + 0.08f && i <= samplesPerParam / 2 + 37))
                        {
                            var point = new Vector3();

                            point = detailedD.GetValueAt(u, v);

                            if (point.isFinite())
                            {
                                (point.Y, point.Z) = (-point.Z, point.Y /*-radius/CmToMm*/);
                                point *= CmToMm;
                                if (point.Z >= SupportSize * CmToMm - 0.1)
                                {
                                    points.Add(point);
                                }
                                else
                                {
                                }
                            }
                        }

                        v += dV;
                    }


                    if (i % 2 == 1)
                    {
                        points.Reverse();
                    }

                    finalDPoints.AddRange(points);
                    nextIter: ;
                }

                u += dU;
            }

            dziubekPrawoGora.AddRange(finalDPoints);

            AddMoveFromAndToCenter(finalDPoints);
            finalPoints.AddRange(finalDPoints);

            finalDPoints.Clear();
            u = 0;
            var it = 0;
            for (int i = 0; i < samplesPerParam; i++)
            {
                if (i <= samplesPerParam / 2)
                {
                    var v = 0.0f;
                    var startv = intersectLeft.GetVOnULineSecondObject(u);
                    var points = new List<Vector3>();
                    for (int j = 0; j < 104; j++)
                    {
                        if ((v >= startv + 0.1f && i <= 1)
                            ||(v >= startv + 0.05f && i is > 1 and <= 6)
                            ||(v >= startv && i>6))
                        {
                            if (it < 2)
                            {
                                if (v < startv + 0.03f)
                                {
                                    goto skip;
                                }
                            }

                            var point = new Vector3();

                            point = detailedD.GetValueAt(u, v);

                            if (point.isFinite())
                            {
                                (point.Y, point.Z) = (-point.Z, point.Y /*-radius/CmToMm*/);
                                point *= CmToMm;
                                if (point.Z >= SupportSize * CmToMm - 0.1)
                                {
                                    points.Add(point);
                                }
                                else
                                {
                                }
                            }
                        }

                        skip: ;
                        v += dV;
                    }

                    it++;


                    if (i % 2 == 1)
                    {
                        points.Reverse();
                    }

                    finalDPoints.AddRange(points);
                    nextIter: ;
                }

                u += dU;
            }

            dziubekPrawoDol.AddRange(finalDPoints);

            AddMoveFromAndToCenter(finalDPoints);
            finalPoints.AddRange(finalDPoints);

            finalDPoints.Clear();
            u = 0;
            for (int i = 0; i < samplesPerParam; i++)
            {
                if (i > samplesPerParam / 2)
                {
                    var v = 0.0f;
                    var points = new List<Vector3>();
                    for (int j = 0; j < samplesPerParamV; j++)
                    {
                        if (j > 140)
                        {
                            var point = new Vector3();

                            point = detailedD.GetValueAt(u, v);

                            if (point.isFinite())
                            {
                                (point.Y, point.Z) = (-point.Z, point.Y /*-radius/CmToMm*/);
                                point *= CmToMm;
                                if (point.Z >= SupportSize * CmToMm - 0.1)
                                {
                                    points.Add(point);
                                }
                                else
                                {
                                }
                            }
                        }

                        v += dV;
                    }


                    if (i % 2 == 1)
                    {
                        points.Reverse();
                    }

                    finalDPoints.AddRange(points);
                    nextIter: ;
                }

                u += dU;
            }

            dziubekLewoGora.AddRange(finalDPoints);

            AddMoveFromAndToCenter(finalDPoints);
            finalPoints.AddRange(finalDPoints);

            finalDPoints.Clear();
            u = 0;
            for (int i = 0; i < samplesPerParam; i++)
            {
                if (i <= samplesPerParam / 2)
                {
                    var v = 0.0f;
                    var points = new List<Vector3>();
                    for (int j = 0; j < samplesPerParamV; j++)
                    {
                        if (j > 140 && ((i < 5 && j < 190) || j < 180))
                        {
                            var point = new Vector3();

                            point = detailedD.GetValueAt(u, v);

                            if (point.isFinite())
                            {
                                (point.Y, point.Z) = (-point.Z, point.Y /*-radius/CmToMm*/);
                                point *= CmToMm;
                                if (point.Z >= SupportSize * CmToMm - 0.1)
                                {
                                    points.Add(point);
                                }
                                else
                                {
                                }
                            }
                        }

                        v += dV;
                    }


                    if (i % 2 == 1)
                    {
                        points.Reverse();
                    }

                    finalDPoints.AddRange(points);
                    nextIter: ;
                }

                u += dU;
            }

            dziubekLewoDol.AddRange(finalDPoints);

            AddMoveFromAndToCenter(finalDPoints);
            finalPoints.AddRange(finalDPoints);
        }
#endif

        #endregion

        #region dziura

        //finalPoints.Clear();
        var kolko = new List<Vector3>()
        {
            new Vector3(-1.22f * CmToMm, -5.1f * CmToMm, SupportSize * CmToMm + 0.02f),
            new Vector3(-0.55f * CmToMm, -5.1f * CmToMm, SupportSize * CmToMm + 0.02f),

            new Vector3(-0.53f * CmToMm, -5.05f * CmToMm, SupportSize * CmToMm + 0.02f),
            new Vector3(-1.265f * CmToMm, -5.05f * CmToMm, SupportSize * CmToMm + 0.02f),

            new Vector3(-1.285f * CmToMm, -5f * CmToMm, SupportSize * CmToMm + 0.02f),
            new Vector3(-0.50f * CmToMm, -5f * CmToMm, SupportSize * CmToMm + 0.02f),

            new Vector3(-0.49f * CmToMm, -4.95f * CmToMm, SupportSize * CmToMm + 0.02f),
            new Vector3(-1.32f * CmToMm, -4.95f * CmToMm, SupportSize * CmToMm + 0.02f),

            new Vector3(-1.34f * CmToMm, -4.9f * CmToMm, SupportSize * CmToMm + 0.02f),
            new Vector3(-0.46f * CmToMm, -4.9f * CmToMm, SupportSize * CmToMm + 0.02f),

            new Vector3(-0.47f * CmToMm, -4.85f * CmToMm, SupportSize * CmToMm + 0.02f),
            new Vector3(-1.36f * CmToMm, -4.85f * CmToMm, SupportSize * CmToMm + 0.02f),

            new Vector3(-1.38f * CmToMm, -4.8f * CmToMm, SupportSize * CmToMm + 0.02f),
            new Vector3(-0.47f * CmToMm, -4.8f * CmToMm, SupportSize * CmToMm + 0.02f),
        };
        dziura.AddRange(kolko);

        AddMoveFromAndToCenter(kolko);
        finalPoints.AddRange(kolko);

        #endregion

        #region C0

        {
            var u = detailedD.USize;
            var v = detailedD.VSize;
            for (int i = 50; i < 100; i++)
            {
                var pos = detailedD.GetValueAt(u * i / 100f, v / 2);
                (pos.Y, pos.Z) = (-pos.Z, pos.Y /*-radius/CmToMm*/);
                pos *= CmToMm;
                if (pos.Z >= SupportSize * CmToMm - 0.1)
                {
                    C0.Add(pos);
                }
                else
                {
                }
            }

            for (int i = 0; i < 50; i++)
            {
                var pos = detailedD.GetValueAt(u * i / 100f, v / 2);
                (pos.Y, pos.Z) = (-pos.Z, pos.Y /*-radius/CmToMm*/);
                pos *= CmToMm;
                if (pos.Z >= SupportSize * CmToMm - 0.1)
                {
                    C0.Add(pos);
                }
                else
                {
                }
            }
        }

        #endregion


        for (int i = 0; i < finalPoints.Count; i++)
        {
            if (finalPoints[i].Z < SupportSize * CmToMm + 0.02f)
            {
                finalPoints[i] = finalPoints[i] with
                {
                    Z = SupportSize * CmToMm + 0.02f
                };
            }
        }

        var raczka = ConnectPaths(raczkaWewnatrz, raczkaZewnatrz, 0.7f * ZBlockSize * CmToMm);
        var sloikPrawo = ConnectPaths(sloikPrawoPomiedzy, sloikPrawoGora);
        var sloikSrodekPrawo = ConnectPaths(sloikSrodek, sloikPrawo);
        var sloik = ConnectPaths(sloikLewo, sloikSrodekPrawo);


        AddMoveFromAndToCenter(sloikSrodek);
        AddMoveFromAndToCenter(sloikPrawoGora);
        AddMoveFromAndToCenter(sloikPrawoPomiedzy);
        AddMoveFromAndToCenter(sloikLewo);


        var dziubekPrawo = ConnectPaths(dziubekPrawoGora, dziubekPrawoDol, 0.8f * ZBlockSize * CmToMm);
        var dziubekLewo = ConnectPaths(dziubekLewoGora, dziubekLewoDol, 0.75f * ZBlockSize * CmToMm);
        dziubekLewo.Reverse();
        var dziubek = ConnectPaths(dziubekLewo, dziubekPrawo, 0.8f * ZBlockSize * CmToMm);

        raczka = ConnectPaths(raczka, przeciecieRaczka, 0.75f * ZBlockSize * CmToMm);
        raczka = ConnectPaths(raczka, dziura, 0.75f * ZBlockSize * CmToMm);

        dziubek = ConnectPaths(C0, dziubek, 0.8f * ZBlockSize * CmToMm);
        for (int i = przeciecieDziubek.Count / 2; i < przeciecieDziubek.Count; i++)
        {
            przeciecieDziubek[i] = przeciecieDziubek[i] with
            {
                X = przeciecieDziubek[i].X + 0.1f,
                Y = przeciecieDziubek[i].Y + 0.1f,
                Z = przeciecieDziubek[i].Z - 0.08f
            };
        }

        for (int i = 0; i < przeciecieDziubek.Count / 2; i++)
        {
            przeciecieDziubek[i] = przeciecieDziubek[i] with
            {
                X = przeciecieDziubek[i].X + 0.1f,
                Y = przeciecieDziubek[i].Y + 0.1f,
                Z = przeciecieDziubek[i].Z - 0.08f
            };
        }

        dziubek = ConnectPaths(dziubek, przeciecieDziubek, 0.5f * ZBlockSize * CmToMm);

        //sloik = sloikLewo;

        var koncowe = ConnectPaths(raczka, ConnectPaths(sloik, dziubek));
        //var koncowe = ConnectPaths(raczka, ConnectPaths(sloik, przeciecieDziubek));
        //var koncowe = ConnectPaths(dziubek, przeciecieDziubek);
        AddMoveFromAndToCenter(koncowe);

        for (int i = 0; i < koncowe.Count; i++)
        {
            if (koncowe[i].Z < SupportSize * CmToMm + 0.02f)
            {
                koncowe[i] = koncowe[i] with
                {
                    Z = SupportSize * CmToMm + 0.02f
                };
            }
        }

        SavePath(frez, radius, koncowe, false);
    }

    private void AddMoveFromAndToCenter(List<Vector3> list)
    {
        list.Insert(0, new Vector3()
        {
            X = 0,
            Y = 0,
            Z = 1.1f * ZBlockSize * CmToMm,
        });
        list.Insert(1, new Vector3()
        {
            X = list[1].X,
            Y = list[1].Y,
            Z = 1.1f * ZBlockSize * CmToMm,
        });
        list.Add(new Vector3()
        {
            X = list[^1].X,
            Y = list[^1].Y,
            Z = 1.1f * ZBlockSize * CmToMm,
        });
        list.Add(list[0]);
    }

    private List<Vector3> ConnectPaths(List<Vector3> first, List<Vector3> second, float z = -1)
    {
        if (z < 0)
        {
            z = 1.1f * ZBlockSize * CmToMm;
        }

        List<Vector3> finalList = new List<Vector3>(first);
        finalList.Add(first[^1] with {Z = z});
        finalList.Add(second[0] with {Z = z});
        finalList.AddRange(second);
        return finalList;
    }
}