using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ComboBox = System.Windows.Controls.ComboBox;
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
        Simulator3C.Simulator._simulator3CControl = this;
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
                    Simulator3C.Simulator.FileName = "Brak pliku";
                    MessageBox.Show($"Błąd podczas wczytywania pliku\nKod błędu: {result.Item2}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                ProgressBar.Value = 0;
            }
            else
            {
                Simulator3C.Simulator.FileName = "Brak pliku";
                MessageBox.Show("Brak pliku lub plik jest pusty", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void StartMilling(object sender, RoutedEventArgs e)
    {
        Simulator3C.Simulator.StartMilling();
    }

    private void ResetBlock(object sender, RoutedEventArgs e)
    {
        Simulator3C.Simulator.ResetBlock();
    }

    private void ChangeCutter(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox c)
        {
            if (c.SelectedIndex == 0)
            {
                Simulator3C.Simulator.SphericalSelected = true;
                Simulator3C.Simulator.FlatSelected = false;
            }
            else
            {
                Simulator3C.Simulator.SphericalSelected = false;
                Simulator3C.Simulator.FlatSelected = true;
            }
        }
    }

    public void UpdateProgressBar(int value)
    {
        ProgressBar.Value = value;
    }

    private void StopSimulation(object sender, RoutedEventArgs e)
    {
        Simulator3C.Simulator.StopSimulation();
    }

    private void SkipMilling(object sender, RoutedEventArgs e)
    {
        Simulator3C.Simulator.SkipMilling();
    }
}