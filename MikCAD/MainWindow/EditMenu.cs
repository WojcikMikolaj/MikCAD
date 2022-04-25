using System;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MikCAD.Annotations;
using MikCAD.BezierCurves;
using MikCAD.Utilities;

namespace MikCAD;

public partial class MainWindow
{
    private void OnSaveCommand(object sender, RoutedEventArgs e)
    {
        FileDialog diag = new SaveFileDialog();
        if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            
        }
    }
    
    private void OnLoadCommand(object sender, RoutedEventArgs e)
    {
        FileDialog diag = new OpenFileDialog();
        if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            
        }
    }
}