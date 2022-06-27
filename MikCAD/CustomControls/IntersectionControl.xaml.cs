using System.Windows;
using System.Windows.Controls;
using MikCAD.Objects;

namespace MikCAD.CustomControls;

public partial class IntersectionControl : UserControl
{
    public IntersectionControl()
    {
        InitializeComponent();
    }

    private void SetImages(object sender, RoutedEventArgs e)
    {
        MainWindow.current.firstImage.Source = (DataContext as Intersection).firstBmp.BitmapToImageSource();
        MainWindow.current.secondImage.Source = (DataContext as Intersection).secondBmp.BitmapToImageSource();
    }

    private void ConvertToInterpolating(object sender, RoutedEventArgs e)
    {
        (DataContext as Intersection).ConvertToInterpolating();
    }

    private void Delete(object sender, RoutedEventArgs e)
    {
        (DataContext as Intersection)._firstObj.Intersection = null;
        (DataContext as Intersection)._secondObj.Intersection = null;
        MainWindow.current.firstImage.Source = null;
        MainWindow.current.secondImage.Source = null;
    }

    private void ShowC0(object sender, RoutedEventArgs e)
    {
        (DataContext as Intersection).ShowC0();
    }
}