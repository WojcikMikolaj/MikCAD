using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Speech.Synthesis.TtsEngine;
using MikCAD.BezierSurfaces;
using MikCAD.CustomControls;
using MikCAD.Extensions;
using MikCAD.Utilities;
using OpenTK.Mathematics;

namespace MikCAD.Sciezki;

public class PathsGenerator
{
    public PathsGeneratorControl PathsGeneratorControl;
    public static PathsGenerator Generator { get; private set; }

    public float XBlockSize { get; set; } = 15;
    public float YBlockSize { get; set; } = 15;
    public float ZBlockSize { get; set; } = 5;
    public float SupportSize { get; set; } = 0f;

    private int _height = 1000;
    private int _width = 1000;
    private const int SamplesPerObjectCount = 500;
    private const float CmToMm = 10;
    private const float UnitsToCm = 1;

    private float[,] HeightMap;
    private float dXPerArrayElement;
    private float dZPerArrayElement;

    public PathsGenerator()
    {
        Generator = this;
        HeightMap = new float[_width, _height];
    }

    public void GenerateRough(CutterType frez, uint radius)
    {
        for (int i = 0; i < _width; ++i)
        {
            for (int j = 0; j < _height; ++j)
            {
                HeightMap[i, j] = SupportSize * CmToMm;
            }
        }

        dXPerArrayElement = XBlockSize / _width;
        //bo y jest zamieniony z z
        dZPerArrayElement = YBlockSize / _height;

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
                            var posYArray = (int) (pos.Z / dZPerArrayElement);

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

        List<CuttingLinePoint> list = new List<CuttingLinePoint>();
        list.Add(new CuttingLinePoint()
        {
            XPosInMm = XBlockSize/2 * CmToMm,
            YPosInMm = YBlockSize/2 * CmToMm,
            ZPosInMm = 2 * ZBlockSize * CmToMm, 
        });

        var distanceBetweenPaths = radius * 0.8f;
        var nominalHeightForFirstPass = (ZBlockSize - SupportSize) / 2 + SupportSize;
        var numberOfPathsOnSinglePlain = (int)XBlockSize * CmToMm / distanceBetweenPaths;

        var cutterArray = CalculateCutterArray(frez, radius);
        
        for(int i=0; i<numberOfPathsOnSinglePlain; ++i)
        {
            var startXinMm = i * distanceBetweenPaths - XBlockSize/2;
            var startYinMm = i % 2 == 0 ? 0 : YBlockSize - YBlockSize/2;
            var startZinMm = MathF.Max(nominalHeightForFirstPass, GetZFromArray(startXinMm, startYinMm, cutterArray));

            for (int j = 0; j < _width; ++j)
            {
                
            }
        }
        
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
                yZArray[it++] = (0.5f*(1.0f/radiusInMm * ConvertFromTexXToXInMm(i)*ConvertFromTexXToXInMm(i)));
            }

            
            it = 0;
            for (int i = -rX + 1; i < rX; i++)
            {
                xZArray[it++] = 0.5f*(1.0f/radiusInMm * ConvertFromTexYToYInMm(i)*ConvertFromTexYToYInMm(i));
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

    private float GetZFromArray(float Xpos, float Ypos, float[,] cutterArray)
    {
        var posXArray = (int) (Xpos / dXPerArrayElement);
        var posYArray = (int) (Ypos / dZPerArrayElement);

        var height = SupportSize * CmToMm;
        
        if (posXArray >= 0
            && posXArray < _width
            && posYArray >= 0
            && posYArray < _height)
        {
            for (int i =  )
            {
                
            }
            
        }

        return ;
    }

    public void GenerateSupportFlatFinish(CutterType frez, uint radius)
    {
    }

    public void GenerateDetailed(CutterType frez, uint radius)
    {
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

    private void SaveAsBitmap()
    {
        DirectBitmap db = new DirectBitmap(_width, _height);
        for (int i = 0; i < _width; ++i)
        {
            for (int j = 0; j < _height; ++j)
            {
                var c = (int) ((HeightMap[i, j] / (ZBlockSize*UnitsToCm*CmToMm)) * 255);
                db.SetPixel(i, j, Color.FromArgb(c, c, c));
            }
        }

        db.BitmapToImageSource().Save("heightmap.bmp");
    }
}