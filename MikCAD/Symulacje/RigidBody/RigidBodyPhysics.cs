using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using MikCAD.Annotations;
using MikCAD.CustomControls;
using MikCAD.Objects;
using MikCAD.Utilities;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using MH = OpenTK.Mathematics.MathHelper;
// ReSharper disable InconsistentNaming

namespace MikCAD.Symulacje.RigidBody;
public partial class RigidBody
{
    private void SetUpPhysics()
    {
        Q = Quaternion.FromAxisAngle((0,0,1),0);
        UpdateRigidBodyRotationMatrix();
        StartingGravityVectorInChangedBase = ChangeBaseMatrixT0 * GravityVector;
    }
    
    public Vector3 DiagonalizedInertiaTensor { get; private set; }
    public Vector3 InversedDiagonalizedInertiaTensor { get; private set; }
    
    private void CalculateDiagonalizedInertiaTensor()
    {
        var faceLength5 = Math.Pow(_cubeEdgeLength, 5);

        var I1 = (float) (11.0f / 12 * faceLength5 * CubeDensity); //wektor własny {-1,0,1}
        var I2 = (float) (11.0f / 12 * faceLength5 * CubeDensity); //wektor własny {-1,1,0} 
        var I3 = (float) (faceLength5 * CubeDensity / 6.0f); //wektor własny {1,1,1} - nasza przekątna

        DiagonalizedInertiaTensor = (I1, I2, I3);
        InversedDiagonalizedInertiaTensor = (1.0f / I1, 1.0f / I2, 1.0f / I3);
    }
    
    private Quaternion Q, Q1;
    private Vector3 W, W1; //prędkość kątowa
    private float t = 0;

    private readonly Matrix3 ChangeBaseMatrixT0 = new Matrix3(
        (-1.0f / 3.0f, -1.0f / 3.0f, 2.0f / 3.0f),
        (-1.0f / 3.0f, 2.0f / 3.0f, -1.0f / 3.0f),
        (1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f));

    private readonly Vector3 GravityVector = new Vector3(0, 0, -9.807f);

    private Vector3 StartingGravityVectorInChangedBase;
    private Vector3 GravityTorque;

    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private Vector3 CalculateTorque(Quaternion Q)
    {
        return Q.Inverted() * StartingGravityVectorInChangedBase;
    }

    
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private Vector3 f(Quaternion Q, Vector3 W)
    {
        return Vector3.Multiply(InversedDiagonalizedInertiaTensor,
            (CalculateTorque(Q) + Vector3.Cross(Vector3.Multiply(DiagonalizedInertiaTensor, W), W)));
    }

    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private Quaternion g(Quaternion Q, Vector3 W)
    {
        Matrix4x3 qMatrix4x3 = new Matrix4x3((-Q.X, -Q.Y, -Q.Z), (Q.W, -Q.Z, Q.Y), (Q.Z, Q.W, -Q.X), (-Q.Y, Q.X, Q.W));

        return new Quaternion(
            w: qMatrix4x3[0, 0] * W[0] + qMatrix4x3[0, 1] * W[1] + qMatrix4x3[0, 2] * W[2],
            x: qMatrix4x3[1, 0] * W[0] + qMatrix4x3[1, 1] * W[1] + qMatrix4x3[1, 2] * W[2],
            y: qMatrix4x3[2, 0] * W[0] + qMatrix4x3[2, 1] * W[1] + qMatrix4x3[2, 2] * W[2],
            z: qMatrix4x3[3, 0] * W[0] + qMatrix4x3[3, 1] * W[1] + qMatrix4x3[3, 2] * W[2]
        );
    }


    public void SimulateNextStep()
    {
        /*
         Jak to policzyć?
         funkcja1  f(t)=I^(-1)(N+(IW)*W)
         funkcja2  g(t)=QxW/2
        //Z poprzedniego kroku
        W(t), Wt(t)
        Q(t), Qt(t)
        //Inne
        I - stałe
        N - liczymy na bieżąco*/


        var f_k1 = f(Q, W);
        var g_k1 = g(Q, W);

        var f_k2 = f(Q + IntegrationStep / 2.0f * g_k1, W + IntegrationStep * f_k1 / 2.0f);
        var g_k2 = g(Q + IntegrationStep / 2.0f * g_k1, W + IntegrationStep * f_k1 / 2.0f);

        var f_k3 = f(Q + IntegrationStep / 2.0f * g_k2, W + IntegrationStep * f_k2 / 2.0f);
        var g_k3 = g(Q + IntegrationStep / 2.0f * g_k2, W + IntegrationStep * f_k2 / 2.0f);

        var f_k4 = f(Q + IntegrationStep / 2.0f * g_k2, W + IntegrationStep * f_k3);
        var g_k4 = g(Q + IntegrationStep / 2.0f * g_k2, W + IntegrationStep * f_k3);

        W1 = W + 1.0f / 6.0f * (f_k1 + 2 * f_k2 + 2 * f_k3 + f_k4) * IntegrationStep;
        Q1 = Q + 1.0f / 6.0f * (g_k1 + 2 * g_k2 + 2 * g_k3 + g_k4) * IntegrationStep;
        //W_t1 = ;


        Q1.Normalize();

        W = W1;
        Q = Q1;
        t += IntegrationStep;
        /*
         Jak to narysować?
         Q(t+delta) - nasza obecna rotacja w układzie bączka
         teraz chcemy to za pomocą "macierz" B obrócić do układu sceny i zastosować na startowym obiekcie
         */

        //Główne pytanie, jak policzyć B?
        //B to nasz kwaternion
    }

    private void ResetPhysics()
    {
        Q = Quaternion.FromAxisAngle((0,0,1),0);
        W = (0, 0, (float) _angularVelocity);
        t = 0;
        UpdateRigidBodyRotationMatrix();
        
    }
}