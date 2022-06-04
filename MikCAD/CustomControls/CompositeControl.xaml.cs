using System.Windows;
using System.Windows.Controls;

namespace MikCAD
{
    public partial class CompositeControl: UserControl
    {
        public CompositeControl()
        {
            InitializeComponent();
        }

        private void CollapsePoints(object sender, RoutedEventArgs e)
        {
            Scene.CurrentScene.ObjectsController.CollapsePoints();
        }
    }
}