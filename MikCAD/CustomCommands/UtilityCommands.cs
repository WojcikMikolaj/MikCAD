using System.Windows.Input;

namespace MikCAD.CustomCommands;

public static class UtilityCommands
{
    public static RoutedCommand ClearScene = new RoutedCommand("ClearScene", typeof(MainWindow));
}