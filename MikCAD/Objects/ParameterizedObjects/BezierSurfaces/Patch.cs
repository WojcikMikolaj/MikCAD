namespace MikCAD.BezierSurfaces;

public class Patch
{
    uint[] idx = new uint[16];

    internal uint GetIdAtI(int i) => idx[i];
    internal uint GetIdAtI(int i, int j) => idx[i * 4 + j];
    internal void SetIdAtI(int i, uint id) => idx[i] = id;
    internal void SetIdAtI(int i, int j, uint id) => idx[i * 4 + j] = id;
}