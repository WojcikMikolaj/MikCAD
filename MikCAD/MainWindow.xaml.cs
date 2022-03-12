using System;
using System.Windows.Interop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;


namespace MikCAD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Scene _scene = new Scene();
        public MainWindow()
        {
            InitializeComponent();
            
            var mainSettings = new GLWpfControlSettings {MajorVersion = 2, MinorVersion = 1};
            OpenTkControl.Start(mainSettings);
            _scene.Initialise(400, 400);
        }

        private void OpenTkControl_OnRender(TimeSpan delta) {
            _scene.OnRenderFrame();
        }
    }
}