using System.Collections.Generic;
using MikCAD.Utilities;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace MikCAD;

public class GregoryPatch : ParameterizedObject
{
    public GregoryPatch(List<ParameterizedPoint> innerRing, List<ParameterizedPoint> outerRing) : base("gregoryPatch")
    {
        var inner1 = innerRing.GetRange(0, 4);
        var inner2 = innerRing.GetRange(3, 4);
        var inner3 = innerRing.GetRange(6, 3);
        inner3.Add(innerRing[0]);
        innerBezierControlPoint = new[]
        {
            inner1,
            inner2,
            inner3
        };

        var outer1 = outerRing.GetRange(0, 4);
        var outer2 = outerRing.GetRange(4, 4);
        var outer3 = outerRing.GetRange(8, 4);
        outerBezierControlPoint = new[]
        {
            outer1,
            outer2,
            outer3
        };

        ReconstructPatch();
    }

    private List<ParameterizedPoint>[] innerBezierControlPoint;
    private List<ParameterizedPoint>[] outerBezierControlPoint;

    private Vector3[] B = new Vector3[3];
    private Vector3[,] R = new Vector3[3, 2];
    private Vector3[,] S = new Vector3[3, 2];
    private Vector3[] T = new Vector3[3];

    private Vector3[,] RpD = new Vector3[3, 2];
    private Vector3[,] SpD = new Vector3[3, 2];

    private Vector3[] P2 = new Vector3[3];
    private Vector3[] ai = new Vector3[3];
    private Vector3 p0;
    private Vector3[] P1 = new Vector3[3];
    private Vector3[,] RedPoints = new Vector3[3, 2];

    private Vector3[] OB = new Vector3[3];
    private Vector3[,] OR = new Vector3[3, 2];
    private Vector3[,] OS = new Vector3[3, 2];
    private Vector3[] OT = new Vector3[3];

    public void ReconstructPatch()
    {
        Vector3 pom;
        B[0] = innerBezierControlPoint[0][0]._position;
        B[1] = innerBezierControlPoint[1][0]._position;
        B[2] = innerBezierControlPoint[2][0]._position;

        for (int i = 0; i < 3; i++)
        {
            var IP = new[]
            {
                innerBezierControlPoint[i][0]._position,
                innerBezierControlPoint[i][1]._position,
                innerBezierControlPoint[i][2]._position,
                innerBezierControlPoint[i][3]._position
            };

            var OP = new[]
            {
                outerBezierControlPoint[i][0]._position,
                outerBezierControlPoint[i][1]._position,
                outerBezierControlPoint[i][2]._position,
                outerBezierControlPoint[i][3]._position
            };

            R[i, 0] = MiddlePoint(IP[0], IP[1]);
            R[i, 1] = MiddlePoint(IP[2], IP[3]);
            pom = MiddlePoint(IP[1], IP[2]);
            S[i, 0] = MiddlePoint(R[i, 0], pom);
            S[i, 1] = MiddlePoint(pom, R[i, 1]);
            T[i] = MiddlePoint(S[i, 0], S[i, 1]);

            OR[i, 0] = MiddlePoint(OP[0], OP[1]);
            OR[i, 1] = MiddlePoint(OP[2], OP[3]);
            pom = MiddlePoint(OP[1], OP[2]);
            OS[i, 0] = MiddlePoint(OR[i, 0], pom);
            OS[i, 1] = MiddlePoint(pom, OR[i, 1]);
            OT[i] = MiddlePoint(OS[i, 0], OS[i, 1]);

            RpD[i, 0] = R[i, 0] + (R[i, 0] - OR[i, 0]);
            RpD[i, 1] = R[i, 1] + (R[i, 1] - OR[i, 1]);

            SpD[i, 0] = S[i, 0] + (S[i, 0] - OS[i, 0]);
            SpD[i, 1] = S[i, 1] + (S[i, 1] - OS[i, 1]);

            P2[i] = T[i] + (T[i] - OT[i]);

            ai[i] = Vector3.Lerp(T[i], P2[i], 1.5f);
        }

        p0 = (ai[0] + ai[1] + ai[2]) / 3;
        for (int i = 0; i < 3; i++)
        {
            P1[i] = Vector3.Lerp(p0, ai[i], 2.0f / 3);
        }

        RedPoints[0, 0] = P1[0] - (SpD[0, 0] - P2[0]) / 3;
        RedPoints[2, 1] = P1[0] + (SpD[0, 0] - P2[0]) / 3;

        RedPoints[1, 0] = P1[1] - (SpD[1, 0] - P2[1]) / 3;
        RedPoints[0, 1] = P1[1] + (SpD[1, 0] - P2[1]) / 3;

        RedPoints[2, 0] = P1[2] - (SpD[2, 0] - P2[2]) / 3;
        RedPoints[1, 1] = P1[2] + (SpD[2, 0] - P2[2]) / 3;

        // for (int i = 0; i < 3; i++)
        // {
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"B{i}")
        //     {
        //         posX = B[i].X,
        //         posY = B[i].Y,
        //         posZ = B[i].Z
        //     });
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"R{i},0")
        //     {
        //         posX = R[i, 0].X,
        //         posY = R[i, 0].Y,
        //         posZ = R[i, 0].Z
        //     });
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"R{i},1")
        //     {
        //         posX = R[i, 1].X,
        //         posY = R[i, 1].Y,
        //         posZ = R[i, 1].Z
        //     });
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"S{i},0")
        //     {
        //         posX = S[i, 0].X,
        //         posY = S[i, 0].Y,
        //         posZ = S[i, 0].Z
        //     });
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"R{i},1")
        //     {
        //         posX = S[i, 1].X,
        //         posY = S[i, 1].Y,
        //         posZ = S[i, 1].Z
        //     });
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"T{i}")
        //     {
        //         posX = T[i].X,
        //         posY = T[i].Y,
        //         posZ = T[i].Z,
        //     });
        //
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"RpD{i},0")
        //     {
        //         posX = RpD[i, 0].X,
        //         posY = RpD[i, 0].Y,
        //         posZ = RpD[i, 0].Z
        //     });
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"RpD{i},1")
        //     {
        //         posX = RpD[i, 1].X,
        //         posY = RpD[i, 1].Y,
        //         posZ = RpD[i, 1].Z
        //     });
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"SpD{i},0")
        //     {
        //         posX = SpD[i, 0].X,
        //         posY = SpD[i, 0].Y,
        //         posZ = SpD[i, 0].Z
        //     });
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"SpD{i},1")
        //     {
        //         posX = SpD[i, 1].X,
        //         posY = SpD[i, 1].Y,
        //         posZ = SpD[i, 1].Z
        //     });
        //
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"P2{i}")
        //     {
        //         posX = P2[i].X,
        //         posY = P2[i].Y,
        //         posZ = P2[i].Z,
        //     });
        //
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"P1{i}")
        //     {
        //         posX = P1[i].X,
        //         posY = P1[i].Y,
        //         posZ = P1[i].Z,
        //     });
        // }
        //
        // Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"P0")
        // {
        //     posX = p0.X,
        //     posY = p0.Y,
        //     posZ = p0.Z,
        // });
        // for (int i = 0; i < 3; i++)
        // for (int j = 0; j < 2; j++)
        //     Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"RP{i},{j}")
        //     {
        //         posX = RedPoints[i, j].X,
        //         posY = RedPoints[i, j].Y,
        //         posZ = RedPoints[i, j].Z,
        //     });
        _patches = GeneratePatches();
        _lines = GenerateLines();
    }

    Vector3 MiddlePoint(Vector3 A, Vector3 B) => (A + B) / 2;

    Vector3 EvaluateCurveAtT(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        t = 1 - t;
        int i = 0;
        int j = 0;
        Vector3[] arr = new Vector3[] {p0, p1, p2, p3};
        for (i = 1; i < 4; i++)
        {
            for (j = 0; j < 4 - i; j++)
            {
                arr[j] = arr[j] * (1.0f - t) + arr[j + 1] * t;
            }
        }

        return arr[0];
    }

    #region InheritedNotUsed

    public override void UpdateTranslationMatrix()
    {
    }

    public override void UpdateRotationMatrix(Axis axis)
    {
    }

    public override void UpdateScaleMatrix()
    {
    }

    public override Matrix4 GetModelMatrix()
    {
        return Matrix4.Identity;
    }

    public override Matrix4 GetOnlyModelMatrix()
    {
        return Matrix4.Identity;
    }

    #endregion

    private uint[] _lines;
    public override uint[] lines => _lines;

    private uint[] GenerateLines()
    {
        int size = 11*2*3;
        uint[] lines = new uint[size];

        var it = 0;

        for (int i = 0; i < 3; i++)
        {
            //t-p2
            lines[it++] = (uint)(i*20+3);
            lines[it++] = (uint)(i*20+7);
            //p2-p1
            lines[it++] = (uint)(i*20+7);
            lines[it++] = (uint)(i*20+11);
            //p1-p0
            lines[it++] = (uint)(i*20+11);
            lines[it++] = (uint)(i*20+15);
            //s0-spd
            lines[it++] = (uint)(i*20+2);
            lines[it++] = (uint)(i*20+6);
            //p2-spd
            lines[it++] = (uint)(i*20+7);
            lines[it++] = (uint)(i*20+6);
            //p1-rp
            lines[it++] = (uint)(i*20+11);
            lines[it++] = (uint)(i*20+18);
            //r0-rpd
            lines[it++] = (uint)(i*20+1);
            lines[it++] = (uint)(i*20+16);
            //r-1-rpd
            lines[it++] = (uint)(i*20+4);
            lines[it++] = (uint)(i*20+17);
            //s-1-spd-1
            lines[it++] = (uint)(i*20+8);
            lines[it++] = (uint)(i*20+9);
            //p2-1-spd-1
            lines[it++] = (uint)(i*20+13);
            lines[it++] = (uint)(i*20+9);
            //p1-1-rp-1
            lines[it++] = (uint)(i*20+14);
            lines[it++] = (uint)(i*20+19);
        }

        return lines;
    }

    private float[] _vertices;
    public float[] vertices => _vertices;

    public override void GenerateVertices()
    {
        var verticesPom = new float[3, 20 * Point.Size];

        _vertices = new float[3 * 20 * Point.Size];

        var it = 0;
        for (int i = 0; i < 3; i++)
        {
            it = 0;
            //B[i]
            verticesPom[i, it++] = B[i].X;
            verticesPom[i, it++] = B[i].Y;
            verticesPom[i, it++] = B[i].Z;
            //R[i,0]
            verticesPom[i, it++] = R[i, 0].X;
            verticesPom[i, it++] = R[i, 0].Y;
            verticesPom[i, it++] = R[i, 0].Z;
            //S[i,0]
            verticesPom[i, it++] = S[i, 0].X;
            verticesPom[i, it++] = S[i, 0].Y;
            verticesPom[i, it++] = S[i, 0].Z;
            //T[i]
            verticesPom[i, it++] = T[i].X;
            verticesPom[i, it++] = T[i].Y;
            verticesPom[i, it++] = T[i].Z;
            //R[i-1,1]
            verticesPom[i, it++] = R[(i + 2) % 3, 1].X;
            verticesPom[i, it++] = R[(i + 2) % 3, 1].Y;
            verticesPom[i, it++] = R[(i + 2) % 3, 1].Z;
            //miejsce łączenie RpD
            verticesPom[i, it++] = 0;
            verticesPom[i, it++] = 0;
            verticesPom[i, it++] = 0;
            //SpD[i,0]
            verticesPom[i, it++] = SpD[i, 0].X;
            verticesPom[i, it++] = SpD[i, 0].Y;
            verticesPom[i, it++] = SpD[i, 0].Z;
            //P2[i]
            verticesPom[i, it++] = P2[i].X;
            verticesPom[i, it++] = P2[i].Y;
            verticesPom[i, it++] = P2[i].Z;
            //S[i-1,1]
            verticesPom[i, it++] = S[(i + 2) % 3, 1].X;
            verticesPom[i, it++] = S[(i + 2) % 3, 1].Y;
            verticesPom[i, it++] = S[(i + 2) % 3, 1].Z;
            //SpD[i-1,1]
            verticesPom[i, it++] = SpD[(i + 2) % 3, 1].X;
            verticesPom[i, it++] = SpD[(i + 2) % 3, 1].Y;
            verticesPom[i, it++] = SpD[(i + 2) % 3, 1].Z;
            //miejsce łączenia czerwonych
            verticesPom[i, it++] = 0;
            verticesPom[i, it++] = 0;
            verticesPom[i, it++] = 0;
            //P1[i]
            verticesPom[i, it++] = P1[i].X;
            verticesPom[i, it++] = P1[i].Y;
            verticesPom[i, it++] = P1[i].Z;
            //T[i-1]
            verticesPom[i, it++] = T[(i + 2) % 3].X;
            verticesPom[i, it++] = T[(i + 2) % 3].Y;
            verticesPom[i, it++] = T[(i + 2) % 3].Z;
            //P2[i-1]
            verticesPom[i, it++] = P2[(i + 2) % 3].X;
            verticesPom[i, it++] = P2[(i + 2) % 3].Y;
            verticesPom[i, it++] = P2[(i + 2) % 3].Z;
            //P1[i-1]
            verticesPom[i, it++] = P1[(i + 2) % 3].X;
            verticesPom[i, it++] = P1[(i + 2) % 3].Y;
            verticesPom[i, it++] = P1[(i + 2) % 3].Z;
            //P0
            verticesPom[i, it++] = p0.X;
            verticesPom[i, it++] = p0.Y;
            verticesPom[i, it++] = p0.Z;
            //RpD[i,0]
            verticesPom[i, it++] = RpD[i, 0].X;
            verticesPom[i, it++] = RpD[i, 0].Y;
            verticesPom[i, it++] = RpD[i, 0].Z;
            //RpD[i-1,1]
            verticesPom[i, it++] = RpD[(i + 2) % 3, 1].X;
            verticesPom[i, it++] = RpD[(i + 2) % 3, 1].Y;
            verticesPom[i, it++] = RpD[(i + 2) % 3, 1].Z;
            //RedPoints[i-1,1]
            verticesPom[i, it++] = RedPoints[(i + 2) % 3, 1].X;
            verticesPom[i, it++] = RedPoints[(i + 2) % 3, 1].Y;
            verticesPom[i, it++] = RedPoints[(i + 2) % 3, 1].Z;
            //RedPoints[i-1,0]
            verticesPom[i, it++] = RedPoints[(i + 2) % 3, 0].X;
            verticesPom[i, it++] = RedPoints[(i + 2) % 3, 0].Y;
            verticesPom[i, it++] = RedPoints[(i + 2) % 3, 0].Z;
        }

        it = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 20 * Point.Size; j++)
            {
                _vertices[it++] = verticesPom[i, j];
            }
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length *  sizeof(float), vertices,
            BufferUsageHint.StaticDraw);

        GL.BindVertexArray(_vao);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
        _lines = GenerateLines();

        GL.BufferData(BufferTarget.ElementArrayBuffer, _lines.Length * sizeof(uint), _lines,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
    }

    public uint[] patches => _patches;
    private int tessLevel = 4;
    public int UDivisions
    {
        get => tessLevel;
        set
        {
            if (value > 4)
            {
                tessLevel = value;
                OnPropertyChanged(nameof(UDivisions));
                OnPropertyChanged(nameof(VDivisions));
            }
        }
    }

    public int VDivisions
    {
        get => tessLevel;
        set
        {
            if (value > 4)
            {
                tessLevel = value;
                OnPropertyChanged(nameof(UDivisions));
                OnPropertyChanged(nameof(VDivisions));
            }
        }
    }
    public bool DrawPolygon { get; set; } = false;

    private uint[] _patches;

    private uint[] GeneratePatches()
    {
        int patchesCount = 3;
        uint[] patches = new uint[patchesCount * 20];
        //return patches;
        int it = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                patches[it++] = (uint) (i * 20 + j);
            }
        }

        return patches;
    }

    public override void PassToDrawProcessor(DrawProcessor drawProcessor, EyeEnum eye)
    {
        drawProcessor.ProcessObject(this, eye);
    }
}