using System;
using System.Windows.Input;

namespace SystemdManager.Framework;

public abstract class Command : ICommand
{

    public event EventHandler CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Execute(object parameter);

    public abstract bool CanExecute(object parameter);

}