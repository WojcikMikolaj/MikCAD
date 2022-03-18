using System;
using System.Windows;
using System.Windows.Input;
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
        public Scene scene { get; set; }
        public static MainWindow current;
        
        public MainWindow()
        {
            current = this;
            InitializeComponent();
            this.DataContext = this;
            var mainSettings = new GLWpfControlSettings {MajorVersion = 2, MinorVersion = 1};
            OpenTkControl.Start(mainSettings);
            scene = new Scene();
            scene.Initialise(400, 400);
        }

        private void OpenTkControl_OnRender(TimeSpan delta) {
            scene.OnRenderFrame();
        }

        private void OpenTkControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            scene.camera._height = (float)this.OpenTkControl.ActualHeight;
            scene.camera._width = (float)this.OpenTkControl.ActualWidth;
            scene.camera.UpdateProjectionMatrix();
        }
    }
}