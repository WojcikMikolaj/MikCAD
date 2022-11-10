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
        W = ((float) _angularVelocity, (float) _angularVelocity, (float) _angularVelocity);
        t = 0;
    }

    public Matrix3 InertiaTensor { get; private set; }
    public Matrix3 InversedInertiaTensor { get; private set; }

    private void CalculateDiagonalizedInertiaTensor()
    {
        var faceLength5density = (float) (Math.Pow(_cubeEdgeLength, 5) * CubeDensity);
        InertiaTensor = new Matrix3(
            (2.0f / 3.0f * faceLength5density, -1.0f / 4.0f * faceLength5density, -1.0f / 4.0f * faceLength5density),
            (-1.0f / 4.0f * faceLength5density, 2.0f / 3.0f * faceLength5density, -1.0f / 4.0f * faceLength5density),
            (-1.0f / 4.0f * faceLength5density, -1.0f / 4.0f * faceLength5density, 2.0f / 3 * faceLength5density));
        InversedInertiaTensor = InertiaTensor.Inverted();
    }

    private Quaternion Q, Q1;
    private Vector3 W, W1; //prędkość kątowa
    private float t = 0;

    private readonly Vector3 GravityVector = new Vector3(0, -9.807f, 0);
    private Vector3 GravityTorque;

    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private Vector3 CalculateTorque(Quaternion Q)
    {
        if (!IsGravityEnabled)
        {
            return Vector3.Zero;
        }

        //mam g;
        var G = Q.Inverted() * (IsGravityFlipped ? -GravityVector : GravityVector);
        float faceLength4ro = MathF.Pow((float) CubeEdgeLength, 4) * (float) CubeDensity;
        var integrationResult = (
            -1.0f / 2 * faceLength4ro * G.Y + 1.0f / 2 * faceLength4ro * G.Z,
            1.0f / 2 * faceLength4ro * G.X - 1.0f / 2 * faceLength4ro * G.Z,
            -1.0f / 2 * faceLength4ro * G.X + 1.0f / 2 * faceLength4ro * G.Y
        );
        return integrationResult;
    }


    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private Vector3 f(Quaternion Q, Vector3 W)
    {
        return (CalculateTorque(Q) + Vector3.Cross(W * InertiaTensor, W)) * InversedInertiaTensor;
    }

    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private Quaternion g(Quaternion Q, Vector3 W)
    {
        Quaternion qW = new Quaternion(x: W.X / 2, y: W.Y / 2, z: W.Z / 2, w: 0);
        return Q * qW;
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
        //Powinno się normalizować (Q + przyrost) 
        var f_k2 = f(Q + IntegrationStep / 2.0f * g_k1, W + IntegrationStep * f_k1 / 2.0f);
        var g_k2 = g(Q + IntegrationStep / 2.0f * g_k1, W + IntegrationStep * f_k1 / 2.0f);

        var f_k3 = f(Q + IntegrationStep / 2.0f * g_k2, W + IntegrationStep * f_k2 / 2.0f);
        var g_k3 = g(Q + IntegrationStep / 2.0f * g_k2, W + IntegrationStep * f_k2 / 2.0f);

        var f_k4 = f(Q + IntegrationStep * g_k3, W + IntegrationStep * f_k3);
        var g_k4 = g(Q + IntegrationStep * g_k3, W + IntegrationStep * f_k3);

        W1 = W + 1.0f / 6.0f * (f_k1 + 2 * f_k2 + 2 * f_k3 + f_k4) * IntegrationStep;
        Q1 = Q + 1.0f / 6.0f * (g_k1 + 2 * g_k2 + 2 * g_k3 + g_k4) * IntegrationStep;
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
        _rotation = InitialRotation;
        _rotation.Z -= (float) _cubeDeviation;
        UpdateRotationMatrix();
        Q = _rigidBodyRotation.ExtractRotation();
        W = ((float) _angularVelocity, (float) _angularVelocity, (float) _angularVelocity);
        t = 0;
        UpdateRigidBodyRotationMatrix();
    }
}