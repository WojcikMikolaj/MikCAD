using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD.Objects;

public class Grid
{
    private Camera _camera;
    private int width = 100;
    private int height = 100;

    private Vector3[] _vertices;
    private float[] _verticesFloatArray;
    private uint[] _lines;
    public uint[] lines => _lines;
    public float[] vertices => _verticesFloatArray;

    public Grid(Camera camera)
    {
        _camera = camera;
        _vertices = new Vector3[width * height];
        _lines = new uint[2 * 2 * width * height];
        _verticesFloatArray = new float[3 * width * height];
        UpdateGrid();
    }

    public void UpdateGrid()
    {
        var pos = new Vector3((float) MH.Floor(_camera.posX), 0, (float) MH.Floor(_camera.posZ));

        var startPos = pos - new Vector3(width / 2.0f, 0, height / 2.0f);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                _vertices[i * height + j] = startPos + new Vector3(i, 0, j);
            }
        }

        for (int i = 0; i < width * height; i++)
        {
            _verticesFloatArray[3 * i] = _vertices[i].X;
            _verticesFloatArray[3 * i + 1] = _vertices[i].Y;
            _verticesFloatArray[3 * i + 2] = _vertices[i].Z;
        }

        var it = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height - 1; j++)
            {
                _lines[it++] = (uint) (i * height + j);
                _lines[it++] = (uint) (i * height + j + 1);
            }
        }

        for (int i = 0; i < height * (width - 1); i++)
        {
            _lines[it++] = (uint) i;
            _lines[it++] = (uint) (i + width);
        }
    }
}