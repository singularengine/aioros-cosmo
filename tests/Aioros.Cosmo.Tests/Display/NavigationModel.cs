using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aioros.Cosmo.Tests.Display;

/// <summary>A tab of a bound tab control.</summary>
public sealed record TabModel(string Header, string Body);

/// <summary>A question of a bound accordion, its open state two way.</summary>
public sealed class QuestionModel(string ask, string answer) : INotifyPropertyChanged
{
    private bool _open;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Ask { get; } = ask;

    public string Answer { get; } = answer;

    public bool Open
    {
        get => _open;
        set
        {
            if (_open != value)
            {
                _open = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Open)));
            }
        }
    }
}

/// <summary>A page button of a bound pagination.</summary>
public sealed record PageModel(int Number, bool IsCurrent)
{
    public bool IsOther => !IsCurrent;
}

/// <summary>A plain view model for the navigation components.</summary>
public sealed class NavigationModel : INotifyPropertyChanged
{
    private int _selectedTab;
    private bool _open;
    private int _page = 2;
    private string _chosen = "Archive";
    private bool _snoozeShown = true;

    public NavigationModel()
    {
        Deeper = new Desktop.Command(() => Crumbs.Add($"Level {Crumbs.Count}"));
        GoTo = new Desktop.Command(parameter => Page = parameter is int page ? page : Page);
        Previous = new Desktop.Command(() => Page = Math.Max(1, Page - 1));
        Next = new Desktop.Command(() => Page = Math.Min(10, Page + 1));
        Choose = new Desktop.Command(parameter =>
        {
            Chosen = parameter as string ?? Chosen;
            Ran.Add(Chosen);
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<TabModel> Tabs { get; } = [new("Account", "Your account."), new("Password", "Your password.")];

    public IReadOnlyList<QuestionModel> Questions { get; } = [new("Is it accessible?", "Yes."), new("Is it styled?", "Yes, as shadcn's."), new("Is it animated?", "Yes, by default.")];

    public ObservableCollection<string> Crumbs { get; } = ["Home", "Components"];

    public List<string> Ran { get; } = [];

    public System.Windows.Input.ICommand Deeper { get; }

    public System.Windows.Input.ICommand GoTo { get; }

    public System.Windows.Input.ICommand Previous { get; }

    public System.Windows.Input.ICommand Next { get; }

    public System.Windows.Input.ICommand Choose { get; }

    public int SelectedTab { get => _selectedTab; set => Set(ref _selectedTab, value); }

    public bool Open { get => _open; set => Set(ref _open, value); }

    public int Page
    {
        get => _page;
        set
        {
            Set(ref _page, value);
            OnPropertyChanged(nameof(Pages));
        }
    }

    public IReadOnlyList<PageModel> Pages => [.. Enumerable.Range(Math.Clamp(Page - 1, 1, 8), 3).Select(number => new PageModel(number, number == Page))];

    public string Chosen
    {
        get => _chosen;
        set
        {
            Set(ref _chosen, value);
            OnPropertyChanged(nameof(ArchiveChosen));
            OnPropertyChanged(nameof(ReportChosen));
        }
    }

    public bool ArchiveChosen => Chosen == "Archive";

    public bool ReportChosen => Chosen == "Report";

    public bool SnoozeShown { get => _snoozeShown; set => Set(ref _snoozeShown, value); }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(name);
        }
    }

    private void OnPropertyChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
