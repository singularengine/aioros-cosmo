using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Aioros.Cosmo.Tests.Composites;

/// <summary>A page a bound command menu opens.</summary>
public sealed record PageItem(string Title, string Path);

/// <summary>A command that never runs, so its item is disabled.</summary>
internal sealed class Never : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => false;

    public void Execute(object? parameter)
    {
    }
}

/// <summary>A plain view model for the command menus, the command dialog and the comboboxes.</summary>
public sealed class CommandModel : INotifyPropertyChanged
{
    private string? _search;
    private string? _selected;
    private string? _opened;
    private bool _palette;
    private Framework? _framework;
    private bool _dropDown;

    public CommandModel() => Open = new Desktop.Command(parameter => Opened = parameter as string);

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand Open { get; }

    public ICommand Locked { get; } = new Never();

    public ObservableCollection<PageItem> Pages { get; } = [new("Home", "/"), new("Docs", "/docs"), new("Blog", "/blog")];

    public IReadOnlyList<Framework> Frameworks { get; } = [new("next.js", "Next.js"), new("sveltekit", "SvelteKit"), new("nuxt.js", "Nuxt.js"), new("remix", "Remix"), new("astro", "Astro")];

    public ObservableCollection<object> Chosen { get; } = [];

    public string? Search
    {
        get => _search;
        set => Set(ref _search, value);
    }

    public string? Selected
    {
        get => _selected;
        set => Set(ref _selected, value);
    }

    public string? Opened
    {
        get => _opened;
        set => Set(ref _opened, value);
    }

    public bool Palette
    {
        get => _palette;
        set => Set(ref _palette, value);
    }

    public Framework? Framework
    {
        get => _framework;
        set => Set(ref _framework, value);
    }

    public bool DropDown
    {
        get => _dropDown;
        set => Set(ref _dropDown, value);
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

/// <summary>A framework a combobox offers.</summary>
public sealed record Framework(string Value, string Label);
