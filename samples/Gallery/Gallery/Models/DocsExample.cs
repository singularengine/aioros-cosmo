using System.ComponentModel;
using System.Windows.Input;

namespace Gallery.Models;

/// <summary>The documentation shells' model (shadcn/ui's sidebar-01 and sidebar-02): the version their switcher shows and selects.</summary>
public sealed class DocsExample : INotifyPropertyChanged
{
    /// <summary>Creates the model with shadcn's default version, 1.0.1.</summary>
    public DocsExample() => SelectVersion = new Selection(this);

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The version the switcher shows.</summary>
    public string Version
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                foreach (var name in (string[])[nameof(Version), nameof(VersionText), nameof(ShowsStable), nameof(ShowsAlpha), nameof(ShowsBeta)])
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
            }
        }
    } = "1.0.1";

    /// <summary>The version as the switcher shows it, <c>v1.0.1</c>.</summary>
    public string VersionText => "v" + Version;

    /// <summary>Whether 1.0.1 is shown, so its menu item carries the check.</summary>
    public bool ShowsStable => Version == "1.0.1";

    /// <summary>Whether 1.1.0-alpha is shown.</summary>
    public bool ShowsAlpha => Version == "1.1.0-alpha";

    /// <summary>Whether 2.0.0-beta1 is shown.</summary>
    public bool ShowsBeta => Version == "2.0.0-beta1";

    /// <summary>Selects the version its parameter names.</summary>
    public ICommand SelectVersion { get; }

    private sealed class Selection(DocsExample owner) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => parameter is string;

        public void Execute(object? parameter) => owner.Version = parameter as string ?? owner.Version;
    }
}
