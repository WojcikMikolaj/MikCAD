using System.Windows;
using System.Windows.Controls;

namespace MikCAD.CustomControls
{
    public partial class TorusControl : UserControl
    {
        public TorusControl()
        {
            InitializeComponent();
        }
        
        private void SpawnPoints(object sender, RoutedEventArgs e)
        {
            var obj = Scene.CurrentScene.ObjectsController.SelectedObject;
            //var startingPoints = (obj as IIntersectable)?.GetStartingPoints();
            var startingPoints = (obj as IIntersectable)?.GetRandomStartingPoints();

            foreach (var p in startingPoints)
            {
                Scene.CurrentScene.ObjectsController.AddObjectToScene(new ParameterizedPoint($"{p.u}; {p.v}")
                {
                    posX = p.pos.X,
                    posY = p.pos.Y,
                    posZ = p.pos.Z,
                });    
            }
        }
    }
}