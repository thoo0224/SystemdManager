using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SystemdManager.Framework;

public class ViewModel : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        RaisePropertyChanged(propertyName);
    }

    protected virtual bool SetProperty<T>(ref T storage, T value,
        [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    public void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
