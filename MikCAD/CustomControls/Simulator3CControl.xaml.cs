using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace MikCAD.CustomControls;

public partial class Simulator3CControl : UserControl
{
    public Simulator3CControl()
    {
        InitializeComponent();
    }

    public void LoadFile(object sender, RoutedEventArgs e)
    {
        FileDialog diag = new OpenFileDialog()
        {
            Filter = "Path files (*.kXX;*.fXX)|*.k**;*.f**"
        };
        if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var lines = File.ReadAllLines(diag.FileName);
            if (lines != null && lines.Length > 0)
            {
                var result = Simulator3C.Simulator.ParsePathFile(diag.FileName, lines);
                if (!result.Item1)
                {
                    MessageBox.Show($"Błąd podczas wczytywania pliku\nKod błędu: {result.Item2}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Brak pliku lub plik jest pusty", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}