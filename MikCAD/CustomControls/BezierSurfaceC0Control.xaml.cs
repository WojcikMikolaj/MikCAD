using System.Windows;
using System.Windows.Controls;
using MikCAD.BezierCurves;
using MikCAD.BezierSurfaces;

namespace MikCAD;

public partial class BezierSurfaceC0Control : UserControl
{
    public BezierSurfaceC0Control()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var obj = Scene.CurrentScene.ObjectsController.SelectedObject;
        if (obj is BezierSurfaceC0 surf0)
        {
            surf0.Applied = true;
        }
        if (obj is BezierSurfaceC2 surf2)
        {
            surf2.Applied = true;
        }
    }
    
    private void SpawnPoints(object sender, RoutedEventArgs e)
    {
        var obj = Scene.CurrentScene.ObjectsController.SelectedObject;
        var startingPoints = (obj as IIntersectable)?.GetStartingPoints();

        foreach (var p in startingPoints)
        {
            Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"{p.u}; {p.v}")
            {
                posX = p.pos.X,
                posY = p.pos.Y,
                posZ = p.pos.Z,
            });    
            var f = (obj as IIntersectable).GetPositionAndGradient(p.u, p.v);
            var fdU = new BezierCurveC0()
            {
                Name = $"fdU :{p.u}:{p.v}"
            };
            var fdV = new BezierCurveC0(){
            
                Name = $"fdV :{p.u}:{p.v}"
            };
            var fPos = new ParameterizedPoint($"f :{p.u}:{p.v}")
            {
                posX = f.pos.X,
                posY = f.pos.Y,
                posZ = f.pos.Z,
            };
            var fPosdU = new ParameterizedPoint($"fdU :{p.u}:{p.v}")
            {
                posX = f.pos.X + f.dU.X,
                posY = f.pos.Y+ f.dU.Y,
                posZ = f.pos.Z+ f.dU.Z,
            };
            var fPosdV = new ParameterizedPoint($"fdV :{p.u}:{p.v}")
            {
                posX = f.pos.X + f.dV.X,
                posY = f.pos.Y+ f.dV.Y,
                posZ = f.pos.Z+ f.dV.Z,
            };
            Scene.CurrentScene.ObjectsController.SelectedObject = null;
            Scene.CurrentScene.ObjectsController.AddObjectToScene(fdU);
            Scene.CurrentScene.ObjectsController.SelectedObject = null;
            Scene.CurrentScene.ObjectsController.AddObjectToScene(fdV);
            Scene.CurrentScene.ObjectsController.SelectedObject = null;
            Scene.CurrentScene.ObjectsController.AddObjectToScene(fPos);
            Scene.CurrentScene.ObjectsController.AddObjectToScene(fPosdU);
            Scene.CurrentScene.ObjectsController.AddObjectToScene(fPosdV);
            fdU.ProcessObject(fPos);
            fdU.ProcessObject(fPosdU);
            fdV.ProcessObject(fPos);
            fdV.ProcessObject(fPosdV);
        }
    }
}