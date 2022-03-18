using System.Windows.Controls;
using System.Windows.Input;

namespace MikCAD;

public partial class ObjectsList : UserControl
{
    public ObjectsList()
    {
        InitializeComponent();
    }

    private void Select_list_item(object sender, MouseButtonEventArgs e)
    {
        var item = sender as ListViewItem;
        var obj = item.Content as ParameterizedObject;
        Scene.CurrentScene.ObjectsController.SelectedObject = obj;
    }
}