using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>A section of shadcn/ui's dashboard document: its header, type, status, target, limit and reviewer.</summary>
public sealed record Section(int Id, string Header, string Type, string Status, string Target, string Limit, string Reviewer)
{
    public bool IsDone => Status == "Done";

    public bool InProcess => !IsDone;
}

/// <summary>The dashboard example's model: the document's sections paged five at a time, the chosen tab and the selection.</summary>
public sealed partial class DashboardExample : ObservableObject
{
    private const int PageSize = 5;

    public ChartInteractive Visitors { get; } = new();

    public ObservableCollection<Section> Sections { get; } =
    [
        new(1, "Cover page", "Cover page", "In Process", "18", "5", "Eddie Lake"),
        new(2, "Table of contents", "Table of contents", "Done", "29", "24", "Eddie Lake"),
        new(3, "Executive summary", "Narrative", "Done", "10", "13", "Eddie Lake"),
        new(4, "Technical approach", "Narrative", "Done", "27", "23", "Jamik Tashpulatov"),
        new(5, "Design", "Narrative", "In Process", "2", "16", "Jamik Tashpulatov"),
        new(6, "Capabilities", "Narrative", "In Process", "20", "8", "Jamik Tashpulatov"),
        new(7, "Integration with existing systems", "Narrative", "In Process", "19", "21", "Jamik Tashpulatov"),
        new(8, "Innovation and Advantages", "Narrative", "Done", "25", "26", "Assign reviewer"),
        new(9, "Overview of EMR's Innovative Solutions", "Technical content", "Done", "7", "23", "Assign reviewer"),
        new(10, "Advanced Algorithms and Machine Learning", "Narrative", "Done", "30", "28", "Assign reviewer"),
        new(11, "Adaptive Communication Protocols", "Narrative", "Done", "9", "31", "Assign reviewer"),
        new(12, "Advantages Over Current Technologies", "Narrative", "Done", "12", "0", "Assign reviewer"),
        new(13, "Past Performance", "Narrative", "Done", "22", "33", "Assign reviewer"),
        new(14, "Customer Feedback and Satisfaction Levels", "Narrative", "Done", "15", "34", "Assign reviewer"),
        new(15, "Implementation Challenges and Solutions", "Narrative", "Done", "3", "35", "Assign reviewer"),
        new(16, "Security Measures and Data Protection Policies", "Narrative", "In Process", "6", "36", "Assign reviewer"),
        new(17, "Scalability and Future Proofing", "Narrative", "Done", "4", "37", "Assign reviewer"),
        new(18, "Cost-Benefit Analysis", "Plain language", "Done", "14", "38", "Assign reviewer"),
        new(19, "User Training and Onboarding Experience", "Narrative", "Done", "17", "39", "Assign reviewer"),
        new(20, "Future Development Roadmap", "Narrative", "Done", "11", "40", "Assign reviewer"),
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Visible), nameof(PageLabel), nameof(CanGoBack), nameof(CanGoForward))]
    public partial int PageIndex { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TabLabel))]
    public partial int SelectedTab { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionLabel))]
    public partial Section? SelectedSection { get; set; }

    public int PageCount => (Sections.Count + PageSize - 1) / PageSize;

    public IReadOnlyList<Section> Visible => [.. Sections.Skip(PageIndex * PageSize).Take(PageSize)];

    public string PageLabel => $"Page {PageIndex + 1} of {PageCount}";

    public bool CanGoBack => PageIndex > 0;

    public bool CanGoForward => PageIndex < PageCount - 1;

    public string TabLabel => SelectedTab switch
    {
        1 => "Past Performance",
        2 => "Key Personnel",
        3 => "Focus Documents",
        _ => "Outline",
    };

    public string SelectionLabel => SelectedSection is { } section ? $"Selected: {section.Header}" : $"{Sections.Count} sections";

    [RelayCommand]
    private void First() => PageIndex = 0;

    [RelayCommand]
    private void Previous() => PageIndex = Math.Max(0, PageIndex - 1);

    [RelayCommand]
    private void Next() => PageIndex = Math.Min(PageCount - 1, PageIndex + 1);

    [RelayCommand]
    private void Last() => PageIndex = PageCount - 1;

    [RelayCommand]
    private void AddSection()
    {
        Sections.Add(new(Sections.Count + 1, "New section", "Narrative", "In Process", "0", "0", "Assign reviewer"));
        OnPropertyChanged(nameof(PageCount));
        PageIndex = PageCount - 1;
        OnPropertyChanged(nameof(Visible));
        OnPropertyChanged(nameof(PageLabel));
        OnPropertyChanged(nameof(SelectionLabel));
    }
}
