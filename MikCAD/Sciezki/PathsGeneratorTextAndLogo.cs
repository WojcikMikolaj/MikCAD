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
    public float TextStartXInMm { get; set; } = -70;
    public float TextStartYInMm { get; set; } = 70;
    public float TextHeightInMm { get; set; } = 10;

    public float zText => SupportSize * CmToMm - 0.05f;

    public void GenerateTextAndLogo(CutterType frez, uint radiusInMm)
    {
        List<Vector3> tekst = new List<Vector3>();
        List<Vector3> M = new List<Vector3>();
        List<Vector3> I1 = new List<Vector3>();
        List<Vector3> K1 = new List<Vector3>();
        List<Vector3> O = new List<Vector3>();
        List<Vector3> L_ = new List<Vector3>();
        List<Vector3> A = new List<Vector3>();
        List<Vector3> J1 = new List<Vector3>();
        List<Vector3> W = new List<Vector3>();
        List<Vector3> O_ = new List<Vector3>();
        List<Vector3> J2 = new List<Vector3>();
        List<Vector3> C = new List<Vector3>();
        List<Vector3> I2 = new List<Vector3>();
        List<Vector3> K2 = new List<Vector3>();


        var currYPos = TextStartYInMm;
        var moveBetweenLetters = TextHeightInMm / 3;
        var moveBetweenPartsOfLetter = TextHeightInMm / 3;
        //M
        M = GenerateM(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenLetters;

        //I
        I1 = GenerateI(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenLetters;

        //K
        K1 = GenerateK(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenLetters;

        //0
        O = GenerateO(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenPartsOfLetter;
        currYPos -= moveBetweenLetters;

        //Ł
        L_ = GenerateL_(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenLetters;
        
        //A
        A = GenerateA(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenLetters;
        
        //J
        J1 = GenerateJ(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenLetters;

        //Spacja
        currYPos -= 3 * moveBetweenLetters;
        
        //W
        W = GenerateW(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenLetters;
        
        //Ó
        O_ = GenerateO_(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenPartsOfLetter;
        currYPos -= moveBetweenLetters;
        
        //J
        J2 = GenerateJ(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenLetters;
        
        //C
        C = GenerateC(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenPartsOfLetter;
        currYPos -= moveBetweenLetters;
        
        //I
        I2 = GenerateI(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenLetters;
        
        //K
        K2 = GenerateK(TextStartXInMm, ref currYPos, moveBetweenPartsOfLetter);
        currYPos -= moveBetweenLetters;

        tekst.AddRange(M);
        tekst = ConnectPaths(tekst, I1);
        tekst = ConnectPaths(tekst, K1);
        tekst = ConnectPaths(tekst, O);
        tekst = ConnectPaths(tekst, L_);
        tekst = ConnectPaths(tekst, A);
        tekst = ConnectPaths(tekst, J1);
        tekst = ConnectPaths(tekst, W);
        tekst = ConnectPaths(tekst, O_);
        tekst = ConnectPaths(tekst, J2);
        tekst = ConnectPaths(tekst, C);
        tekst = ConnectPaths(tekst, I2);
        tekst = ConnectPaths(tekst, K2);
        AddMoveFromAndToCenter(tekst);
        SavePath(frez, 1, tekst, false, true);
    }

    List<Vector3> GenerateM(float startX, ref float currYPos, float moveBetweenPartsOfLetter)
    {
        List<Vector3> M = new List<Vector3>();
        M.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos,
                Z = zText
            });
        M.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos,
                Z = zText
            });
        M.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        M.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        M.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos,
                Z = zText
            });
        return M;
    }

    List<Vector3> GenerateI(float startX, ref float currYPos, float moveBetweenPartsOfLetter)
    {
        List<Vector3> I = new List<Vector3>();
        I.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos,
                Z = zText
            });
        I.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos,
                Z = zText
            });
        return I;
    }

    List<Vector3> GenerateK(float startX, ref float currYPos, float moveBetweenPartsOfLetter)
    {
        List<Vector3> K = new List<Vector3>();
        K.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos,
                Z = zText
            });
        K.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos,
                Z = zText
            });
        K.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm / 2,
                Y = currYPos,
                Z = zText
            });
        K.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        K.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm / 2,
                Y = currYPos = currYPos + moveBetweenPartsOfLetter,
                Z = zText
            });
        K.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        return K;
    }

    List<Vector3> GenerateO(float startX, ref float currYPos, float moveBetweenPartsOfLetter)
    {
        List<Vector3> O = new List<Vector3>();
        O.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/5,
                Y = currYPos,
                Z = zText
            });
        O.Add(
            new Vector3()
            {
                X = startX + 4*TextHeightInMm/5,
                Y = currYPos,
                Z = zText
            });
        O.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos - moveBetweenPartsOfLetter/4,
                Z = zText
            });
        O.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos - 3*moveBetweenPartsOfLetter/4,
                Z = zText
            });
        O.Add(
            new Vector3()
            {
                X = startX + 4*TextHeightInMm/5,
                Y = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        O.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/5,
                Y = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        O.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos - 3*moveBetweenPartsOfLetter/4,
                Z = zText
            });
        O.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos - moveBetweenPartsOfLetter/4,
                Z = zText
            });
        O.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/5,
                Y = currYPos,
                Z = zText
            });
        return O;
    }

    List<Vector3> GenerateL_(float startX, ref float currYPos, float moveBetweenPartsOfLetter)
    {
        List<Vector3> L_ = new List<Vector3>();
        L_.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos,
                Z = zText
            });
        L_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos,
                Z = zText
            });
        L_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm / 2,
                Y = currYPos,
                Z = zText
            });
        L_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm / 2 - TextHeightInMm / 3,
                Y = currYPos + moveBetweenPartsOfLetter / 2,
                Z = zText
            });
        L_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm / 2 + TextHeightInMm / 3,
                Y = currYPos - moveBetweenPartsOfLetter / 2,
                Z = zText
            });
        L_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm / 2,
                Y = currYPos,
                Z = zText
            });
        L_.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos,
                Z = zText
            });
        L_.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        return L_;
    }

    List<Vector3> GenerateA(float startX, ref float currYPos, float moveBetweenPartsOfLetter)
    {
        List<Vector3> A = new List<Vector3>();
        A.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos,
                Z = zText
            });
        A.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/2,
                Y = currYPos - moveBetweenPartsOfLetter/2,
                Z = zText
            });
        A.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/2,
                Y = currYPos - 3*moveBetweenPartsOfLetter/2,
                Z = zText
            });
        A.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/2,
                Y = currYPos - moveBetweenPartsOfLetter/2,
                Z = zText
            });
        A.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        A.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        return A;
    }
    
    List<Vector3> GenerateJ(float startX, ref float currYPos, float moveBetweenPartsOfLetter)
    {
        List<Vector3> J = new List<Vector3>();
        J.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos,
                Z = zText
            });
        J.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        J.Add(
            new Vector3()
            {
                X = startX - TextHeightInMm /7,
                Y = currYPos,
                Z = zText
            });
        J.Add(
            new Vector3()
            {
                X = startX - 2*TextHeightInMm /7,
                Y = currYPos = currYPos + moveBetweenPartsOfLetter/4,
                Z = zText
            });
        J.Add(
            new Vector3()
            {
                X = startX - 2*TextHeightInMm /7,
                Y = currYPos = currYPos + 2 * moveBetweenPartsOfLetter/4,
                Z = zText
            });
        J.Add(
            new Vector3()
            {
                X = startX - TextHeightInMm /7,
                Y = currYPos = currYPos + moveBetweenPartsOfLetter/4,
                Z = zText
            });
        J.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos,
                Z = zText
            });
        J.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm /7,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter/4,
                Z = zText
            });
        J.Add(
            new Vector3()
            {
                X = startX + 2 * TextHeightInMm /7,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        currYPos = currYPos + moveBetweenPartsOfLetter / 4;
        return J;
    }
    
    List<Vector3> GenerateW(float startX, ref float currYPos, float moveBetweenPartsOfLetter)
    {
        List<Vector3> W = new List<Vector3>();
        W.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos,
                Z = zText
            });
        W.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos - moveBetweenPartsOfLetter/2,
                Z = zText
            });
        W.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        W.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos - moveBetweenPartsOfLetter/2,
                Z = zText
            });
        W.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });

        return W;
    }
    
    List<Vector3> GenerateO_(float startX, ref float currYPos, float moveBetweenPartsOfLetter)
    {
        List<Vector3> O_ = new List<Vector3>();
        O_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/5,
                Y = currYPos,
                Z = zText
            });
        O_.Add(
            new Vector3()
            {
                X = startX + 4*TextHeightInMm/5,
                Y = currYPos,
                Z = zText
            });
        O_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos - moveBetweenPartsOfLetter/4,
                Z = zText
            });
        
        
        O_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos - moveBetweenPartsOfLetter/2,
                Z = zText
            });
        O_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm + TextHeightInMm/5,
                Y = currYPos - moveBetweenPartsOfLetter/2 -moveBetweenPartsOfLetter/4,
                Z = zText
            });
        O_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm - TextHeightInMm/5,
                Y = currYPos - moveBetweenPartsOfLetter/2 + moveBetweenPartsOfLetter/4,
                Z = zText
            });
        O_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos - moveBetweenPartsOfLetter/2,
                Z = zText
            });
        
        
        O_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos - 3*moveBetweenPartsOfLetter/4,
                Z = zText
            });
        O_.Add(
            new Vector3()
            {
                X = startX + 4*TextHeightInMm/5,
                Y = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        O_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/5,
                Y = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        O_.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos - 3*moveBetweenPartsOfLetter/4,
                Z = zText
            });
        O_.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos - moveBetweenPartsOfLetter/4,
                Z = zText
            });
        O_.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/5,
                Y = currYPos,
                Z = zText
            });
        
        return O_;
    }
    
    List<Vector3> GenerateC(float startX, ref float currYPos, float moveBetweenPartsOfLetter)
    {
        List<Vector3> C = new List<Vector3>();
        C.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/5,
                Y = currYPos,
                Z = zText
            });
        C.Add(
            new Vector3()
            {
                X = startX + 4*TextHeightInMm/5,
                Y = currYPos,
                Z = zText
            });
        C.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos - moveBetweenPartsOfLetter/4,
                Z = zText
            });
        C.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm,
                Y = currYPos - 3*moveBetweenPartsOfLetter/4,
                Z = zText
            });
        C.Add(
            new Vector3()
            {
                X = startX + 5*TextHeightInMm/6,
                Y = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        
        
        C.Add(
            new Vector3()
            {
                X = startX + 5*TextHeightInMm/6,
                Y = currYPos - moveBetweenPartsOfLetter,
                Z = zText + 0.5f
            });
        C.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/6,
                Y = currYPos - moveBetweenPartsOfLetter,
                Z = zText+0.5f
            });
        
        
        C.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/6,
                Y = currYPos - moveBetweenPartsOfLetter,
                Z = zText
            });
        C.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos - 3*moveBetweenPartsOfLetter/4,
                Z = zText
            });
        C.Add(
            new Vector3()
            {
                X = startX,
                Y = currYPos - moveBetweenPartsOfLetter/4,
                Z = zText
            });
        C.Add(
            new Vector3()
            {
                X = startX + TextHeightInMm/5,
                Y = currYPos,
                Z = zText
            });
        
        return C;
    }
}