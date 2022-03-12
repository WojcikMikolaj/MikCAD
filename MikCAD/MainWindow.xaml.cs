using System;
using System.Windows.Interop;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.WPF;


namespace MikCAD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenGLControl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
        {
            //  Get the OpenGL instance.
            var gl = args.OpenGL;

            scene.Draw(gl);
        }

        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLRoutedEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            scene.Initialise(gl, 400, 400);
        }

        private void OpenGLControl_Resized(object sender, OpenGLRoutedEventArgs args)
        {
        }

        /// <summary>
        /// The axies, which may be drawn.
        /// </summary>
        private readonly Axies axies = new Axies();

        /// <summary>
        /// The scene we're drawing.
        /// </summary>
        private readonly Scene scene = new Scene();
    }
}