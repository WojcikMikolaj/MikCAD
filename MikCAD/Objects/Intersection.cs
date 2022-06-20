using System.ComponentModel;
using System.Runtime.CompilerServices;
using MikCAD.Annotations;

namespace MikCAD.Objects;

public class Intersection : INotifyPropertyChanged
{
    public Intersection(IIntersectable first, IIntersectable second)
    {
        this._firstObj = first;
        this._secondObj = second;

        if (first == second)
            _selfIntersection = true;
    }

    private readonly bool _selfIntersection;

    private IIntersectable _firstObj;
    private IIntersectable _secondObj;

    public int NumberOfPoints { get; set; }
    public float Steps { get; set; }
    public bool UseCursor { get; set; }

    public void Intersect()
    {
        ;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}