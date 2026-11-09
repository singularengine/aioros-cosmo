using System.Windows.Input;

namespace Aioros.Cosmo.Tests.Desktop;

/// <summary>A command running an action with its parameter, always executable.</summary>
internal sealed class Command(Action<object?> run) : ICommand
{
    public Command(Action run)
        : this(_ => run())
    {
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => run(parameter);
}
