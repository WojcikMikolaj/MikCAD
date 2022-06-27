using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using OpenTK.Mathematics;
using UserControl = System.Windows.Controls.UserControl;

namespace MikCAD.CustomControls
{
    public partial class CameraControl : UserControl
    {
        public CameraControl()
        {
            InitializeComponent();
        }

        private void LeftEyeColor_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var color = SetColor(sender);
            if (color.HasValue)
                Scene.CurrentScene.camera.leftColor = new Vector4(color.Value.R / 255.0f, color.Value.G / 255.0f,
                    color.Value.B / 255.0f, color.Value.A/255.0f);
        }

        private Color? SetColor(object sender)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Color color = new Color();
                color.A = colorDialog.Color.A;
                color.R = colorDialog.Color.R;
                color.G = colorDialog.Color.G;
                color.B = colorDialog.Color.B;
                (sender as System.Windows.Controls.Label).Background = new SolidColorBrush(color);
                return color;
            }
            return null;
        }
        private void RightEyeColor_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var color = SetColor(sender);
            if (color.HasValue)
                Scene.CurrentScene.camera.rightColor = new Vector4(color.Value.R / 255.0f, color.Value.G / 255.0f,
                    color.Value.B / 255.0f, color.Value.A/255.0f);
        }
    }
}