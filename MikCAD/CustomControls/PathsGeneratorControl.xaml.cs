using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MikCAD.Sciezki;
using MikCAD.Symulacje.RigidBody;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.Forms.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace MikCAD.CustomControls;

public partial class PathsGeneratorControl : UserControl
{
    public PathsGeneratorControl()
    {
        InitializeComponent();
        
    }

    private void GenerateRough(object sender, RoutedEventArgs e)
    {
        PathsGenerator.Generator.PathsGeneratorControl ??= this;
        
        CutterType frez;
        uint radius = 1; 
        
        if (radioRF10.IsChecked.Value || radioRF12.IsChecked.Value)
        {
            frez = CutterType.Flat;
        }
        else
        {
            frez = CutterType.Spherical;
        }

        if (radioRF10.IsChecked.Value)
        {
            radius = 10;
        }
        if (radioRF12.IsChecked.Value)
        {
            radius = 12;
        }
        if (radioRK01.IsChecked.Value)
        {
            radius = 1;
        }
        if (radioRK08.IsChecked.Value)
        {
            radius = 8;
        }
        if (radioRK16.IsChecked.Value)
        {
            radius = 16;
        }

        PathsGenerator.Generator.GenerateRough(frez, radius);
    }

    private void GenerateSupportFlatFinish(object sender, RoutedEventArgs e)
    {
        PathsGenerator.Generator.PathsGeneratorControl ??= this;
        
        CutterType frez;
        uint radius = 1; 
        
        if (radioSFF10.IsChecked.Value || radioSFF12.IsChecked.Value)
        {
            frez = CutterType.Flat;
        }
        else
        {
            frez = CutterType.Spherical;
        }

        if (radioSFF10.IsChecked.Value)
        {
            radius = 10;
        }
        if (radioSFF12.IsChecked.Value)
        {
            radius = 12;
        }
        if (radioSFK01.IsChecked.Value)
        {
            radius = 1;
        }
        if (radioSFK08.IsChecked.Value)
        {
            radius = 8;
        }
        if (radioSFK16.IsChecked.Value)
        {
            radius = 16;
        }
        
        PathsGenerator.Generator.GenerateSupportFlatFinish(frez, radius);
    }

    private void GenerateDetailed(object sender, RoutedEventArgs e)
    {
        PathsGenerator.Generator.PathsGeneratorControl ??= this;
        
        CutterType frez;
        uint radius = 1; 
        
        if (radioDF10.IsChecked.Value || radioDF12.IsChecked.Value)
        {
            frez = CutterType.Flat;
        }
        else
        {
            frez = CutterType.Spherical;
        }

        if (radioDF10.IsChecked.Value)
        {
            radius = 10;
        }
        if (radioDF12.IsChecked.Value)
        {
            radius = 12;
        }
        if (radioDK01.IsChecked.Value)
        {
            radius = 1;
        }
        if (radioDK08.IsChecked.Value)
        {
            radius = 8;
        }
        if (radioDK16.IsChecked.Value)
        {
            radius = 16;
        }
        
        PathsGenerator.Generator.GenerateDetailed(frez, radius);
    }
}