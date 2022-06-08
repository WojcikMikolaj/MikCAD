using System.Collections.Generic;
using MikCAD.Utilities;
using OpenTK.Mathematics;

namespace MikCAD;

public class GregoryPatch : ParameterizedObject, I2DObject
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
    private Vector3[,] R = new Vector3[3,2];
    private Vector3[,] S = new Vector3[3,2];
    private Vector3[] T = new Vector3[3];
    
    private Vector3[,] RpD = new Vector3[3,2];
    private Vector3[,] SpD = new Vector3[3,2];
    
    private Vector3[] P2 = new Vector3[3];
    private Vector3[] ai = new Vector3[3];
    private Vector3 p0;
    private Vector3[] P1 = new Vector3[3];
    
    private Vector3[] OB = new Vector3[3];
    private Vector3[,] OR = new Vector3[3,2];
    private Vector3[,] OS = new Vector3[3,2];
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
                EvaluateCurveAtT(0.25f, innerBezierControlPoint[i][0]._position,
                    innerBezierControlPoint[i][1]._position,
                    innerBezierControlPoint[i][2]._position, innerBezierControlPoint[i][3]._position),
                EvaluateCurveAtT(0.75f, innerBezierControlPoint[i][0]._position,
                    innerBezierControlPoint[i][1]._position,
                    innerBezierControlPoint[i][2]._position, innerBezierControlPoint[i][3]._position),
                innerBezierControlPoint[i][3]._position
            };

            var OP = new[]
            {
                outerBezierControlPoint[i][0]._position,
                EvaluateCurveAtT(0.25f, outerBezierControlPoint[i][0]._position,
                    outerBezierControlPoint[i][1]._position,
                    outerBezierControlPoint[i][2]._position, outerBezierControlPoint[i][3]._position),
                EvaluateCurveAtT(0.75f, outerBezierControlPoint[i][0]._position,
                    outerBezierControlPoint[i][1]._position,
                    outerBezierControlPoint[i][2]._position, outerBezierControlPoint[i][3]._position),
                outerBezierControlPoint[i][3]._position
            };

            R[i,0] = MiddlePoint(IP[0], IP[1]);
            R[i,1] = MiddlePoint(IP[2], IP[3]);
            pom = MiddlePoint(IP[1], IP[2]);
            S[i,0] = MiddlePoint(R[i,0], pom);
            S[i,1] = MiddlePoint(pom, R[i,1]);
            T[i] = MiddlePoint(S[i,0], S[i,1]);
            
            OR[i,0] = MiddlePoint(OP[0], OP[1]);
            OR[i,1] = MiddlePoint(OP[2], OP[3]);
            pom = MiddlePoint(OP[1], OP[2]);
            OS[i,0] = MiddlePoint(OR[i,0], pom);
            OS[i,1] = MiddlePoint(pom, OR[i,1]);
            OT[i] = MiddlePoint(OS[i,0], OS[i,1]);

            RpD[i, 0] = R[i, 0] + (R[i, 0] - OR[i, 0]);
            RpD[i, 1] = R[i, 1] + (R[i, 1] - OR[i, 1]);
            
            SpD[i, 0] = S[i, 0] + (S[i, 0] - OS[i, 0]);
            SpD[i, 1] = S[i, 1] + (S[i, 1] - OS[i, 1]);

            P2[i] = T[i] + (T[i] - OT[i]);
            
            ai[i] = Vector3.Lerp(T[i],P2[i],1.5f);
        }

        p0 = (ai[0] + ai[1] + ai[2]) / 3;
        for (int i = 0; i < 3; i++)
        {
            P1[i] = Vector3.Lerp(p0,ai[i],2.0f/3);
        }

        for (int i = 0; i < 3; i++)
        {
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = B[i].X,
                posY = B[i].Y,
                posZ = B[i].Z
            });
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = R[i,0].X,
                posY = R[i,0].Y,
                posZ = R[i,0].Z
            });
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = R[i,1].X,
                posY = R[i,1].Y,
                posZ = R[i,1].Z
            });
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = S[i,0].X,
                posY = S[i,0].Y,
                posZ = S[i,0].Z
            });
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = S[i,1].X,
                posY = S[i,1].Y,
                posZ = S[i,1].Z
            });
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = T[i].X,
                posY = T[i].Y,
                posZ = T[i].Z,
            });
            
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = RpD[i,0].X,
                posY = RpD[i,0].Y,
                posZ = RpD[i,0].Z
            });
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = RpD[i,1].X,
                posY = RpD[i,1].Y,
                posZ = RpD[i,1].Z
            });
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = SpD[i,0].X,
                posY = SpD[i,0].Y,
                posZ = SpD[i,0].Z
            });
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = SpD[i,1].X,
                posY = SpD[i,1].Y,
                posZ = SpD[i,1].Z
            });
            
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = P2[i].X,
                posY = P2[i].Y,
                posZ = P2[i].Z,
            });
            
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
            {
                posX = P1[i].X,
                posY = P1[i].Y,
                posZ = P1[i].Z,
            });
        }
        Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint()
        {
            posX = p0.X,
            posY = p0.Y,
            posZ = p0.Z,
        });
    }

    Vector3 MiddlePoint(Vector3 A, Vector3 B) => (A + B) / 2;
    Vector3 EvaluateCurveAtT(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
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

    public override uint[] lines { get; }

    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        throw new System.NotImplementedException();
    }

    public override void PassToDrawProcessor(DrawProcessor drawProcessor, EyeEnum eye, uint vertexAttributeLocation,
        uint normalAttributeLocation)
    {
        throw new System.NotImplementedException();
    }
}