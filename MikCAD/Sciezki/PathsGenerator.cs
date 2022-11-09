using System.Drawing;
using System.IO;
using System.Net;
using System.Speech.Synthesis.TtsEngine;
using MikCAD.BezierSurfaces;
using MikCAD.CustomControls;
using MikCAD.Extensions;
using MikCAD.Utilities;

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
    private const int SamplesPerObjectCount = 2000;
    private const float CmToMm = 10;
    private const float UnitsToCm = 1;

    private float[,] HeightMap;

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

        float dXPerArrayElement = XBlockSize / _width;
        //bo y jest zamieniony z z
        float dZPerArrayElement = YBlockSize / _height;

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
    }

    public void GenerateSupportFlatFinish(CutterType frez, uint radius)
    {
    }

    public void GenerateDetailed(CutterType frez, uint radius)
    {
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