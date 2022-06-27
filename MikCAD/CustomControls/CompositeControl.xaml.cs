using System.Windows;
using System.Windows.Controls;

namespace MikCAD.CustomControls
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

        private void PatchHole(object sender, RoutedEventArgs e)
        {
            if (!Scene.CurrentScene.ObjectsController.PatchHole())
                MessageBox.Show("Nie udało się zalepić dziury", "Błąd");
        }

        private void IntersectObjects(object sender, RoutedEventArgs e)
        {
            var dialog = new Window()
            {
                Content = new IntersectionModalControl()
                {
                    intersection = Scene.CurrentScene.ObjectsController.GetNewIntersectionObject()
                },
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                Owner = MainWindow.current,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            
            dialog.ShowDialog();
        }
    }
}