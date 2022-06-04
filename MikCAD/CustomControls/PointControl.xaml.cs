using System.Windows;
using System.Windows.Controls;

namespace MikCAD
{
    public partial class PointControl: UserControl
    {
        public PointControl()
        {
            InitializeComponent();
        }

        private void CollapsePoints(object sender, RoutedEventArgs e)
        {
            Scene.CurrentScene.ObjectsController.CollapsePoints();
        }
    }
}