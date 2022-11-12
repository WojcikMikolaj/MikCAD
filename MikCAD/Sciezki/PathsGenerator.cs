﻿#define GENERIC 
//#define SPECIALIZED

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

public class PathsGenerator
{
    public PathsGeneratorControl PathsGeneratorControl;
    public static PathsGenerator Generator { get; private set; }
    private uint runs = 0;

    public float XBlockSize { get; set; } = 15;
    public float YBlockSize { get; set; } = 15;
    public float ZBlockSize { get; set; } = 5;
    public float SupportSize { get; set; } = 1.85f;

    public bool IsGenerateHeightmap { get; set; } = true;

    public float FlatEps { get; set; } = 0.1f;

    private int _height = 1000;
    private int _width = 1000;
    private const int SamplesPerObjectCount = 500;
    private const float CmToMm = 10;
    private const float UnitsToCm = 1;
    private const float SafetyDistance = 1;

    private float[,] HeightMap;
    private float dXPerArrayElement;
    private float dZPerArrayElement;

    private float dXInMmPerArrayElement;
    private float dYInMmPerArrayElement;

    public PathsGenerator()
    {
        Generator = this;
        HeightMap = new float[_width, _height];
    }

    public void GenerateRough(CutterType frez, uint radius)
    {
        GenerateHeightmap();

        #region first_pass

        List<CuttingLinePoint> list = new List<CuttingLinePoint>();
        list.Add(new CuttingLinePoint()
        {
            XPosInMm = 0,
            YPosInMm = 0,
            ZPosInMm = 2 * ZBlockSize * CmToMm,
        });

        list.Add(new CuttingLinePoint()
        {
            XPosInMm = -XBlockSize / 2 * CmToMm,
            YPosInMm = -YBlockSize / 2 * CmToMm,
            ZPosInMm = 2 * ZBlockSize * CmToMm,
        });

        var distanceBetweenPaths = radius * 0.8f;
        var nominalHeightForFirstPass = ((ZBlockSize - SupportSize) / 2 + SupportSize) * CmToMm;
        var numberOfPathsOnSinglePlain = (int) (XBlockSize * CmToMm / distanceBetweenPaths);

        var cutterArray = CalculateCutterArray(frez, radius);
        var rX = ConvertXInMmToTexX(radius);
        var rY = ConvertYInMmToTexY(radius);


        for (int i = 0; i < numberOfPathsOnSinglePlain + 2; ++i)
        {
            bool moveRight = i % 2 == 0;
            var startXinMm = (moveRight ? -XBlockSize / 2 : XBlockSize / 2) * CmToMm;
            var startYinMm = i * distanceBetweenPaths - (YBlockSize / 2) * CmToMm;
            var startZinMm = MathF.Max(nominalHeightForFirstPass,
                GetZFromArray(startXinMm, startYinMm, cutterArray, rX, rY)) + SafetyDistance;

            list.Add(new CuttingLinePoint()
            {
                XPosInMm = startXinMm,
                YPosInMm = startYinMm,
                ZPosInMm = startZinMm
            });

            var posXInMm = startXinMm;
            var posYInMm = startYinMm;
            var posZInMm = startZinMm;

            while (true)
            {
                if (moveRight)
                {
                    if (posXInMm + radius >= XBlockSize * CmToMm / 2)
                    {
                        break;
                    }

                    posXInMm += radius * 0.01f;
                }
                else
                {
                    if (posXInMm - radius <= -XBlockSize * CmToMm / 2)
                    {
                        break;
                    }

                    posXInMm -= radius * 0.01f;
                }

                posZInMm = MathF.Max(nominalHeightForFirstPass,
                    GetZFromArray(posXInMm, posYInMm, cutterArray, rX, rY)) + SafetyDistance;

                list.Add(new CuttingLinePoint()
                {
                    XPosInMm = posXInMm,
                    YPosInMm = posYInMm,
                    ZPosInMm = posZInMm
                });
            }

            var endXinMm = (moveRight ? XBlockSize / 2 : -XBlockSize / 2) * CmToMm;
            var endYinMm = i * distanceBetweenPaths - (YBlockSize / 2) * CmToMm;
            var endZinMm = MathF.Max(nominalHeightForFirstPass,
                GetZFromArray(endXinMm, endYinMm, cutterArray, rX, rY)) + SafetyDistance;

            list.Add(new CuttingLinePoint()
            {
                XPosInMm = endXinMm,
                YPosInMm = endYinMm,
                ZPosInMm = endZinMm
            });
        }

        #endregion

        #region second_pass

        var lastXInMm = list[^1].XPosInMm;
        var lastYInMm = list[^1].YPosInMm;
        var lastZInMm = MathF.Max(SupportSize * CmToMm,
            GetZFromArray(lastXInMm, lastYInMm, cutterArray, rX, rY)) + SafetyDistance;
        ;


        for (int i = 0; i < numberOfPathsOnSinglePlain + 2; ++i)
        {
            //== dla +2 != dla +1 
            bool moveRight = i % 2 == 0;
            var startXinMm = (moveRight ? XBlockSize / 2 : -XBlockSize / 2) * CmToMm;
            var startYinMm = (numberOfPathsOnSinglePlain + 1 - i) * distanceBetweenPaths - (YBlockSize / 2) * CmToMm;
            var startZinMm = MathF.Max(SupportSize * CmToMm,
                GetZFromArray(startXinMm, startYinMm, cutterArray, rX, rY)) + SafetyDistance;

            list.Add(new CuttingLinePoint()
            {
                XPosInMm = startXinMm,
                YPosInMm = startYinMm,
                ZPosInMm = startZinMm
            });

            var posXInMm = startXinMm;
            var posYInMm = startYinMm;
            var posZInMm = startZinMm;

            while (true)
            {
                if (!moveRight)
                {
                    if (posXInMm + radius >= XBlockSize * CmToMm / 2)
                    {
                        break;
                    }

                    posXInMm += radius * 0.01f;
                }
                else
                {
                    if (posXInMm - radius <= -XBlockSize * CmToMm / 2)
                    {
                        break;
                    }

                    posXInMm -= radius * 0.01f;
                }

                posZInMm = MathF.Max(SupportSize * CmToMm,
                    GetZFromArray(posXInMm, posYInMm, cutterArray, rX, rY)) + SafetyDistance;
                list.Add(new CuttingLinePoint()
                {
                    XPosInMm = posXInMm,
                    YPosInMm = posYInMm,
                    ZPosInMm = posZInMm
                });
            }

            var endXinMm = (moveRight ? -XBlockSize / 2 : XBlockSize / 2) * CmToMm;
            var endYinMm = (numberOfPathsOnSinglePlain + 1 - i) * distanceBetweenPaths - (YBlockSize / 2) * CmToMm;
            var endZinMm = MathF.Max(SupportSize * CmToMm,
                GetZFromArray(endXinMm, endYinMm, cutterArray, rX, rY)) + SafetyDistance;

            list.Add(new CuttingLinePoint()
            {
                XPosInMm = endXinMm,
                YPosInMm = endYinMm,
                ZPosInMm = endZinMm
            });
        }

        #endregion


        SavePath(frez, radius, list);
    }

    public void GenerateSupportFlatFinish(CutterType frez, uint radius)
    {
        GenerateHeightmap();

        List<CuttingLinePoint> list = new List<CuttingLinePoint>();
        list.Add(new CuttingLinePoint()
        {
            XPosInMm = 0,
            YPosInMm = 0,
            ZPosInMm = 2 * ZBlockSize * CmToMm,
        });

        list.Add(new CuttingLinePoint()
        {
            XPosInMm = -XBlockSize / 2 * CmToMm - radius,
            YPosInMm = -YBlockSize / 2 * CmToMm,
            ZPosInMm = 2 * ZBlockSize * CmToMm,
        });


        var distanceBetweenPaths = (2 - FlatEps) * radius;
        var numberOfPathsOnSinglePlain = (int) (XBlockSize * CmToMm / distanceBetweenPaths);

        var cutterArray = CalculateCutterArray(frez, radius);
        var rX = ConvertXInMmToTexX(radius);
        var rY = ConvertYInMmToTexY(radius);

        var moveBack = false;
        var moveRight = true;
        var startXinMm = 0.0f;
        var startYinMm = 0.0f;
        var startZinMm = 0.0f;

        for (int i = 0; i < 2 * numberOfPathsOnSinglePlain; ++i)
        {
            if (moveBack)
            {
            }
            else
            {
                moveRight = i % 2 == 0;
            }

            startXinMm = (moveRight ? -XBlockSize / 2 * CmToMm - radius : XBlockSize / 2 * CmToMm + radius);
            if (i < numberOfPathsOnSinglePlain + 1)
            {
                startYinMm = i * distanceBetweenPaths - (YBlockSize / 2) * CmToMm;
            }
            else
            {
                startYinMm = (numberOfPathsOnSinglePlain - (i - numberOfPathsOnSinglePlain)) * distanceBetweenPaths -
                             (YBlockSize / 2) * CmToMm;
            }

            startZinMm = SupportSize * CmToMm;
            moveBack = false;

            list.Add(new CuttingLinePoint()
            {
                XPosInMm = startXinMm,
                YPosInMm = startYinMm,
                ZPosInMm = startZinMm
            });

            var posXInMm = startXinMm;
            var posYInMm = startYinMm;
            var posZInMm = startZinMm;

            while (true)
            {
                if (moveRight)
                {
                    if (posXInMm + radius >= XBlockSize * CmToMm / 2)
                    {
                        break;
                    }

                    posXInMm += radius * 0.01f;
                }
                else
                {
                    if (posXInMm - radius <= -XBlockSize * CmToMm / 2)
                    {
                        break;
                    }

                    posXInMm -= radius * 0.01f;
                }

                posZInMm = SupportSize * CmToMm;

                if (ObjectInRadius(posXInMm, posYInMm, rX, rY))
                {
                    list.Add(new CuttingLinePoint()
                    {
                        XPosInMm = startXinMm,
                        YPosInMm = startYinMm,
                        ZPosInMm = startZinMm
                    });
                    moveBack = true;
                    break;
                }

                list.Add(new CuttingLinePoint()
                {
                    XPosInMm = posXInMm,
                    YPosInMm = posYInMm,
                    ZPosInMm = posZInMm
                });
            }

            if (!moveBack)
            {
                var endXinMm = (moveRight ? XBlockSize / 2 * CmToMm + radius : -XBlockSize / 2 * CmToMm - radius);
                var endYinMm = i * distanceBetweenPaths - (YBlockSize / 2) * CmToMm;
                var endZinMm = SupportSize * CmToMm;

                list.Add(new CuttingLinePoint()
                {
                    XPosInMm = endXinMm,
                    YPosInMm = endYinMm,
                    ZPosInMm = endZinMm
                });
            }
        }

        list.Add(new CuttingLinePoint()
        {
            XPosInMm = -XBlockSize / 2 * CmToMm - radius,
            YPosInMm = -YBlockSize / 2 * CmToMm,
            ZPosInMm = SupportSize * CmToMm,
        });

        SavePath(frez, radius, list);
    }

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
#if SPECIALIZED

        #region raczka

        {
            if (surfaces.Count >= 0)
            {
            }
        }

        #endregion

        #region sloik

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

                var decorated = new IIntersectableDecorator(surfaces[1])
                {
                    DistanceFromSurface = radius / CmToMm
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
                    intersection.ShowC0();
                }
            }
        }

        #endregion

        #region czubek

        {
            if (surfaces.Count >= 3)
            {
                // //GORA PRAWO
                // //1) u=2.8404841; v=1.865617;
                // //2) u=3.0553267; v=1.3731587;
                {
                    var u1 = 2.8404841f;
                    var v1 = 1.865617f;

                    var u2 = 3.0553267f;
                    var v2 = 1.3731587f;

                    var decorated = new IIntersectableDecorator(surfaces[2])
                    {
                        DistanceFromSurface = radius / CmToMm
                    };
                    var intersection = new Intersection(supportSurface, decorated)
                    {
                        StartingPointsNumber = 30000,
                        MaxPointsNumber = 10000,
                        NewtonMaxIterations = 1000,
                        UseRandom = true,
                        UseCursor = true
                    };
                    var result = intersection.Intersect((supportSurface.GetValueAt(u1, v1), u1, v1),
                        (decorated.GetValueAt(u2, v2), u2, v2), true);

                    if (result)
                    {
                        intersection.ShowC0();
                    }
                }
                //
                //GORA LEWO
                //1) u=3.4132; v=0.9082303;
                //2) u=3.0791092; v=5.7197366;
                {
                    var u1 = 3.4132f;
                    var v1 = 0.9082303f;

                    var u2 = 3.0791092f;
                    var v2 = 5.7197366f;

                    var decorated = new IIntersectableDecorator(surfaces[2])
                    {
                        DistanceFromSurface = radius / CmToMm
                    };

                    var intersection = new Intersection(supportSurface, decorated)
                    {
                        StartingPointsNumber = 30000,
                        MaxPointsNumber = 10000,
                        NewtonMaxIterations = 1000,
                        UseRandom = true,
                        UseCursor = true
                    };
                    var result = intersection.Intersect((supportSurface.GetValueAt(u1, v1), u1, v1),
                        (decorated.GetValueAt(u2, v2), u2, v2), true);

                    if (result)
                    {
                        intersection.ShowC0();
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

                    var decorated = new IIntersectableDecorator(surfaces[2])
                    {
                        DistanceFromSurface = radius / CmToMm
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
                        intersection.ShowC0();
                    }
                }
            }
        }

        #endregion

#endif
        foreach (var surf in surfaces)
        {
#if GENERIC
            var decorated = new IIntersectableDecorator(surf)
            {
                DistanceFromSurface = radius / CmToMm
            };

            // var uPointsNum = 50;
            // var vPointsNum = 50;
            //
            // var du = decorated.USize / uPointsNum; 
            // var dv = decorated.VSize / vPointsNum;
            //
            // var u = 0.0f;
            // var v = 0.0f;
            //
            // for (int i = 0; i < uPointsNum; i++)
            // {
            //     v = 0;
            //     for (int j = 0; j < vPointsNum; j++)
            //     {
            //         var pos = decorated.GetValueAt(u, v);
            //         Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            //         {
            //             posX = pos.X,
            //             posY = pos.Y,
            //             posZ = pos.Z
            //         });
            //         v += dv;
            //     }
            //
            //     u += du;
            // }

            var intersection = new Intersection(supportSurface, decorated)
            {
                StartingPointsNumber = 30000,
                MaxPointsNumber = 10000,
                NewtonMaxIterations = 1000,
                UseRandom = true,
                UseCursor = true
            };

            var result = intersection.IntersectAndMoveDistance(radius/CmToMm);
            if (result)
            {
                intersection.ShowC0();
            }

            break;

#endif
        }

        List<CuttingLinePoint> list = new List<CuttingLinePoint>();

        SavePath(frez, radius, list);
    }

    public void GenerateDetailed(CutterType frez, uint radius)
    {
    }

    private static void SavePath(CutterType frez, uint radius, List<CuttingLinePoint> list)
    {
        CuttingLines cuttingLines = new CuttingLines()
        {
            points = list.ToArray()
        };
        cuttingLines.SaveFile(frez, radius);
    }

    private float[,] CalculateCutterArray(CutterType frez, uint radiusInMm)
    {
        var rX = ConvertXInMmToTexX(radiusInMm);
        var rY = ConvertYInMmToTexY(radiusInMm);

        var yZArray = new float[(rY - 1) * 2 + 1];
        var xZArray = new float[(rX - 1) * 2 + 1];
        var xyZArray = new float[(rX - 1) * 2 + 1, (rY - 1) * 2 + 1];

        if (frez == CutterType.Flat)
        {
        }
        else
        {
            int it = 0;
            for (int i = -rY + 1; i < rY; i++)
            {
                yZArray[it++] =
                    (0.5f * (1.0f / radiusInMm * ConvertFromTexXToXInMm(i) * ConvertFromTexXToXInMm(i)));
            }


            it = 0;
            for (int i = -rX + 1; i < rX; i++)
            {
                xZArray[it++] = 0.5f * (1.0f / radiusInMm * ConvertFromTexYToYInMm(i) * ConvertFromTexYToYInMm(i));
            }


            it = 0;
            for (int i = -rX + 1; i < rX; i++)
            {
                var itt = 0;
                for (int j = -rY + 1; j < rY; j++)
                {
                    xyZArray[it, itt] = xZArray[it] + yZArray[itt];
                    itt++;
                }

                it++;
            }
        }

        return xyZArray;
    }

    private bool ObjectInRadius(float xPosInMm, float yPosInMm, int rX, int rY)
    {
        var posXArray = (int) ((xPosInMm + XBlockSize * CmToMm / 2) / dXInMmPerArrayElement);
        var posYArray = (int) ((yPosInMm + YBlockSize * CmToMm / 2) / dYInMmPerArrayElement);

        var height = 0.0f;
        int it = 0;
        for (int i = -2 * rX + 1; i < 2 * rX; i++)
        {
            var itt = 0;
            for (int j = -2 * rY + 1; j < 2 * rY; j++)
            {
                if (posXArray + (int) (i * dXInMmPerArrayElement * 4) >= 0
                    && posXArray + (int) (i * dXInMmPerArrayElement * 4) < _width
                    && posYArray + (int) (j * dYInMmPerArrayElement * 4) >= 0
                    && posYArray + (int) (j * dYInMmPerArrayElement * 4) < _height)
                {
                    if (i * i + j * j <= (2 * rX + 12) * (2 * rX + 12))
                    {
                        height = MathF.Max(height,
                            HeightMap[posXArray + (int) (i * dXInMmPerArrayElement * 4),
                                posYArray + (int) (j * dYInMmPerArrayElement * 4)]);
                    }
                }
            }
        }

        return height > Single.Epsilon + SupportSize * CmToMm;
    }

    private float GetZFromArray(float xPosInMm, float yPosInMm, float[,] cutterArray, int rX, int rY)
    {
        var posXArray = (int) ((xPosInMm + XBlockSize * CmToMm / 2) / dXInMmPerArrayElement);
        var posYArray = (int) ((yPosInMm + YBlockSize * CmToMm / 2) / dYInMmPerArrayElement);

        var height = 0.0f;
        float heightInPoint = 0.0f;

        if (posXArray >= 0
            && posXArray < _width
            && posYArray >= 0
            && posYArray < _height)
        {
            height = HeightMap[posXArray, posYArray];
        }


        int it = 0;
        for (int i = -2 * rX + 1; i < 2 * rX; i++)
        {
            var itt = 0;
            for (int j = -2 * rY + 1; j < 2 * rY; j++)
            {
                if (posXArray + (int) (i * dXInMmPerArrayElement * 4) >= 0
                    && posXArray + (int) (i * dXInMmPerArrayElement * 4) < _width
                    && posYArray + (int) (j * dYInMmPerArrayElement * 4) >= 0
                    && posYArray + (int) (j * dYInMmPerArrayElement * 4) < _height)
                {
                    //tu coś nie tak z tym dodawaniem
                    height = MathF.Max(height,
                        HeightMap[posXArray + (int) (i * dXInMmPerArrayElement * 4),
                            posYArray + (int) (j * dYInMmPerArrayElement * 4)]);
                }
            }
        }

        return height;
    }

    private int ConvertXInMmToTexX(float rInMm)
    {
        return (int) (rInMm * (_width / (XBlockSize * CmToMm)));
    }

    private int ConvertYInMmToTexY(float rInMm)
    {
        return (int) (rInMm * (_height / (YBlockSize * CmToMm)));
    }

    private float ConvertFromTexXToXInMm(float value)
    {
        return value * ((XBlockSize * CmToMm) / (float) _width);
    }

    private float ConvertFromTexYToYInMm(float value)
    {
        return value * ((YBlockSize * CmToMm) / (float) _height);
    }

    private void GenerateHeightmap()
    {
        dXPerArrayElement = XBlockSize / _width;
        //bo y jest zamieniony z z
        dZPerArrayElement = YBlockSize / _height;
        dXInMmPerArrayElement = dXPerArrayElement * CmToMm;
        dYInMmPerArrayElement = dZPerArrayElement * CmToMm;

        if (IsGenerateHeightmap || runs < 1)
        {
            runs++;
            for (int i = 0; i < _width; ++i)
            {
                for (int j = 0; j < _height; ++j)
                {
                    HeightMap[i, j] = SupportSize * CmToMm;
                }
            }

            foreach (var parameterizedObject in Scene.CurrentScene.ObjectsController.ParameterizedObjects)
            {
                switch (parameterizedObject)
                {
                    case BezierSurfaceC2 surf:
                        float dU = surf.USize / SamplesPerObjectCount;
                        float dV = surf.VSize / SamplesPerObjectCount;
                        float u = 0;
                        float v = 0;


                        for (int i = 0; i < SamplesPerObjectCount; ++i)
                        {
                            v = 0;
                            for (int j = 0; j < SamplesPerObjectCount; ++j)
                            {
                                var pos = surf.GetValueAt(u, v);

                                pos *= UnitsToCm;
                                pos += (XBlockSize / 2, 0, YBlockSize / 2);

                                var posXArray = (int) (pos.X / dXPerArrayElement);
                                //odwrócone, żeby się zgadzało na frezarce
                                var posYArray = _height - (int) (pos.Z / dZPerArrayElement);

                                pos *= CmToMm;

                                if (posXArray >= 0
                                    && posXArray < _width
                                    && posYArray >= 0
                                    && posYArray < _height)
                                {
                                    if (HeightMap[posXArray, posYArray] < pos.Y)
                                    {
                                        HeightMap[posXArray, posYArray] = pos.Y;
                                    }
                                }

                                v += dV;
                            }

                            u += dU;
                        }

                        break;
                    default:
                        break;
                }
            }

            SaveAsBitmap();
        }
    }

    private void AddSupportSurface()
    {
        MainWindow.current.LoadFile(@"C:\Users\mikow\Documents\CAD_CAM\PUSN\podstawka_18mm\support185.json", false);
    }

    private void RemoveSupportSurface()
    {
    }

    private void SaveAsBitmap()
    {
        DirectBitmap db = new DirectBitmap(_width, _height);
        for (int i = 0; i < _width; ++i)
        {
            for (int j = 0; j < _height; ++j)
            {
                var c = (int) ((HeightMap[i, j] / (ZBlockSize * UnitsToCm * CmToMm)) * 255);
                db.SetPixel(i, j, Color.FromArgb(c, c, c));
            }
        }

        db.BitmapToImageSource().Save("heightmap.bmp");
    }
}