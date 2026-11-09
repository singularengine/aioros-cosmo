using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>A tab a bound tab control shows: its header and its page's text.</summary>
public sealed record TabPage(string Header, string Title, string Body);

/// <summary>A question a bound accordion shows.</summary>
public sealed record Question(string Ask, string Answer);

/// <summary>A page button of a bound pagination.</summary>
public sealed record PageLink(int Number, bool IsCurrent)
{
    public bool IsOther => !IsCurrent;
}

/// <summary>A crumb of a bound breadcrumb.</summary>
public sealed record Crumb(string Name, string Href);

/// <summary>The navigation pages' model: tabs, questions, crumbs, pages and the commands changing them.</summary>
public sealed partial class NavigationExamples : ObservableObject
{
    public IReadOnlyList<TabPage> Tabs { get; } =
    [
        new("Account", "Account", "Make changes to your account here. Click save when you're done."),
        new("Password", "Password", "Change your password here. After saving, you'll be logged out."),
    ];

    [ObservableProperty]
    public partial int SelectedTab { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = "Nothing saved yet";

    public IReadOnlyList<Question> Questions { get; } =
    [
        new("Is it accessible?", "Yes. It adheres to the WAI-ARIA design pattern."),
        new("Is it styled?", "Yes. It comes with default styles that match the other components' aesthetic."),
        new("Is it animated?", "Yes. It's animated by default, but you can disable it if you prefer."),
    ];

    public string FirstAnswer => Questions[0].Answer;

    public string SecondAnswer => Questions[1].Answer;

    [ObservableProperty]
    public partial bool Starred { get; set; }

    [ObservableProperty]
    public partial bool AccessibleOpen { get; set; }

    public ObservableCollection<Crumb> Crumbs { get; } = [new("Home", "/"), new("Components", "/components/button")];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Pages))]
    public partial int Page { get; set; } = 2;

    public static int PageCount => 10;

    /// <summary>The current page and its neighbors, as shadcn's pagination example shows them.</summary>
    public IReadOnlyList<PageLink> Pages
    {
        get
        {
            var first = Math.Clamp(Page - 1, 1, PageCount - 2);
            return [.. Enumerable.Range(first, 3).Select(number => new PageLink(number, number == Page))];
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArchiveChosen), nameof(ReportChosen), nameof(SnoozeChosen))]
    public partial string Chosen { get; set; } = "Archive";

    public bool ArchiveChosen => Chosen == "Archive";

    public bool ReportChosen => Chosen == "Report";

    public bool SnoozeChosen => Chosen == "Snooze";

    [ObservableProperty]
    public partial string Action { get; set; } = "Nothing done yet";

    [RelayCommand]
    private void Save(string? what) => Status = $"{what} saved";

    [RelayCommand]
    private void ToggleAnswer() => AccessibleOpen = !AccessibleOpen;

    [RelayCommand]
    private void GoTo(int page) => Page = Math.Clamp(page, 1, PageCount);

    [RelayCommand]
    private void Previous() => Page = Math.Max(1, Page - 1);

    [RelayCommand]
    private void Next() => Page = Math.Min(PageCount, Page + 1);

    [ObservableProperty]
    public partial string CurrentCrumb { get; set; } = "Breadcrumb";

    [RelayCommand]
    private void Deeper()
    {
        Crumbs.Add(new($"Level {Crumbs.Count}", "/components/breadcrumb"));
        CurrentCrumb = $"Level {Crumbs.Count - 1}";
    }

    [RelayCommand]
    private void Choose(string? what)
    {
        Chosen = what ?? Chosen;
        Action = Chosen switch
        {
            "Report" => "Reported",
            "Snooze" => "Snoozed",
            _ => "Archived",
        };
    }
}
