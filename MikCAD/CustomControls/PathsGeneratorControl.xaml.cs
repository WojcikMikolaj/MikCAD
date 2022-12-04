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
        uint diameter = 1; 
        
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
            diameter = 10;
        }
        if (radioRF12.IsChecked.Value)
        {
            diameter = 12;
        }
        if (radioRK01.IsChecked.Value)
        {
            diameter = 1;
        }
        if (radioRK08.IsChecked.Value)
        {
            diameter = 8;
        }
        if (radioRK16.IsChecked.Value)
        {
            diameter = 16;
        }

        PathsGenerator.Generator.GenerateRough(frez, diameter/2);
    }

    private void GenerateSupportFlatFinish(object sender, RoutedEventArgs e)
    {
        PathsGenerator.Generator.PathsGeneratorControl ??= this;
        
        CutterType frez;
        uint diameter = 1; 
        
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
            diameter = 10;
        }
        if (radioSFF12.IsChecked.Value)
        {
            diameter = 12;
        }
        if (radioSFK01.IsChecked.Value)
        {
            diameter = 1;
        }
        if (radioSFK08.IsChecked.Value)
        {
            diameter = 8;
        }
        if (radioSFK16.IsChecked.Value)
        {
            diameter = 16;
        }
        
        PathsGenerator.Generator.GenerateSupportFlatFinish(frez, diameter/2);
    }

    private void GenerateFlatEnvelope(object sender, RoutedEventArgs e)
    {
        PathsGenerator.Generator.PathsGeneratorControl ??= this;
        
        CutterType frez;
        uint diameter = 1; 
        
        if (radioFEF10.IsChecked.Value || radioFEF12.IsChecked.Value)
        {
            frez = CutterType.Flat;
        }
        else
        {
            frez = CutterType.Spherical;
        }

        if (radioFEF10.IsChecked.Value)
        {
            diameter = 10;
        }
        if (radioFEF12.IsChecked.Value)
        {
            diameter = 12;
        }
        if (radioFEK01.IsChecked.Value)
        {
            diameter = 1;
        }
        if (radioFEK08.IsChecked.Value)
        {
            diameter = 8;
        }
        if (radioFEK16.IsChecked.Value)
        {
            diameter = 16;
        }
        
        PathsGenerator.Generator.GenerateFlatEnvelope(frez, diameter/2);
    }
    
    private void GenerateDetailed(object sender, RoutedEventArgs e)
    {
        PathsGenerator.Generator.PathsGeneratorControl ??= this;
        
        CutterType frez;
        uint diameter = 1; 
        
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
            diameter = 10;
        }
        if (radioDF12.IsChecked.Value)
        {
            diameter = 12;
        }
        if (radioDK01.IsChecked.Value)
        {
            diameter = 1;
        }
        if (radioDK08.IsChecked.Value)
        {
            diameter = 8;
        }
        if (radioDK16.IsChecked.Value)
        {
            diameter = 16;
        }
        
        PathsGenerator.Generator.GenerateDetailed(frez, diameter/2);
    }

    private void GenerateTextAndLogo(object sender, RoutedEventArgs e)
    {
        PathsGenerator.Generator.PathsGeneratorControl ??= this;
        
        CutterType frez = CutterType.Spherical;
        uint diameter = 1;
        PathsGenerator.Generator.GenerateTextAndLogo(frez, diameter/2);
    }
}