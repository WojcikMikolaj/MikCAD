using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MikCAD;

public class ObjectsController
{

    public ObservableCollection<ParameterizedObject> ParameterizedObjects { get; private set; } =
        new ObservableCollection<ParameterizedObject>();

    public bool AddObjectToScene(ParameterizedObject parameterizedObject)
    {
        ParameterizedObjects.Add(parameterizedObject);
        return true;
    }

    public bool DeleteObjectFromScene(ParameterizedObject parameterizedObject)
    {
        return ParameterizedObjects.Remove(parameterizedObject);
    }

    public void DrawObjects()
    {
        foreach (var obj in ParameterizedObjects)
        {
            
        }
    }
}