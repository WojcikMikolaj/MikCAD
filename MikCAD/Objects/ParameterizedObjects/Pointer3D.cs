using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MikCAD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MH = OpenTK.Mathematics.MathHelper;

namespace MikCAD;

public class Pointer3D : ParameterizedObject
{
    public Pointer3D() : base("3D Pointer")
    {
    }

    public override string Name
    {
        get => "3D Pointer";
        set{}
    }

    private Matrix4 _translationMatrix = Matrix4.Identity;

    public override bool RotationEnabled => false;
    public override bool ScaleEnabled => false;
    public override uint[] lines => _lines;
    public override void UpdateTranslationMatrix()
    {
        _translationMatrix = Matrix4.CreateTranslation(_position);
        var vec = new Vector4(_position);
        vec[3] = 1;
        var screen = vec * Scene.CurrentScene.camera.GetViewMatrix() * Scene.CurrentScene.camera.GetProjectionMatrix();
        screen[0] /= screen[3];
        screen[1] /= screen[3];
        screen[2] /= screen[3];
        _screenX =(int)((screen.X + 1)*(MainWindow.current.OpenTkControl.ActualWidth / 2));
        _screenY =(int)((-screen.Y + 1)*(MainWindow.current.OpenTkControl.ActualHeight / 2));
        OnPropertyChanged(nameof(screenX));
        OnPropertyChanged(nameof(screenY));
    }

    public override void UpdateRotationMatrix(Axis axis)
    {
        
    }

    public override void UpdateScaleMatrix()
    {
        
    }
    
    private float[] _vertices = new float[]
    {
        0,0,0,
        1,0,0,
        0,1,0,
        0,0,1,
    };

    private uint[] _lines = new uint[]
    {
        0, 1,
        0, 2,
        0, 3,
    };
    
    public override void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation)
    {

        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * 3 * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        var indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _lines.Length * sizeof(uint), _lines, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.UnsignedInt, false, 0, 0);
        GL.EnableVertexAttribArray(1);
    }

    public override Matrix4 GetModelMatrix()
    {
        return _translationMatrix;
    }

    public override Matrix4 GetOnlyModelMatrix()
    {
        return GetModelMatrix();
    }


    private int _screenX = 0;
    public int screenX
    {
        get
        {
            return _screenX;
        }
        set
        {
            _screenX = (int) MH.Min(MainWindow.current.OpenTkControl.ActualWidth, MH.Max(0, value));
            UpdatePosition();
            OnPropertyChanged(nameof(screenX));
        }
    }
    
    private int _screenY = 0;
    public int screenY
    {
        get
        {
            return _screenY;
        }
        set
        {
            _screenY = (int) MH.Min(MainWindow.current.OpenTkControl.ActualHeight, MH.Max(0, value));
            UpdatePosition();
            OnPropertyChanged(nameof(screenY));
        }
    }

    private void UpdatePosition()
    {
        _position = Raycaster.FindPointOnCameraPlain(screenX, screenY).Xyz;
        _translationMatrix = Matrix4.CreateTranslation(_position);
        OnPropertyChanged(nameof(posX));
        OnPropertyChanged(nameof(posY));
        OnPropertyChanged(nameof(posZ));
    }
    
    public override void PassToDrawProcessor(DrawProcessor drawProcessor, EyeEnum eye, uint vertexAttributeLocation, uint normalAttributeLocation)
    {
        drawProcessor.ProcessObject(this, eye, vertexAttributeLocation, normalAttributeLocation);
    }
}