using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MikCAD.Annotations;


namespace MikCAD
{
    public abstract class ParameterizedObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}