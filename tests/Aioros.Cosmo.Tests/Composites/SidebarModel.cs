using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Aioros.Cosmo.Navigation;
using Avalonia.Data.Converters;

namespace Aioros.Cosmo.Tests.Composites;

/// <summary>A page of a bound sidebar menu.</summary>
public sealed record SidebarPage(string Title, string Path);

/// <summary>A plain view model for a bound sidebar: its open state, its pages, the current page and its store.</summary>
public sealed class SidebarModel : INotifyPropertyChanged
{
    private bool _open = true;
    private string _current = "/";

    public SidebarModel() => Navigate = new Desktop.Command(parameter => Current = parameter as string ?? Current);

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Whether a page's path is the current page.</summary>
    public static readonly IMultiValueConverter IsCurrent = new FuncMultiValueConverter<string?, bool>(values =>
    {
        var list = values.ToList();
        return list.Count == 2 && list[0] == list[1];
    });

    public System.Windows.Input.ICommand Navigate { get; }

    public ObservableCollection<SidebarPage> Pages { get; } = [new("Playground", "/"), new("Models", "/models"), new("Documentation", "/docs")];

    public ISidebarStateStore? Store { get; init; }

    public bool Open
    {
        get => _open;
        set => Set(ref _open, value);
    }

    public string Current
    {
        get => _current;
        set => Set(ref _current, value);
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
