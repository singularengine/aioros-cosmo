using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Aioros.Cosmo.Tables;
using Avalonia.Data.Converters;

namespace CosmoStarter.Models;

/// <summary>A section of dashboard-01's document (its <c>data.json</c>): its header, type, status, target, limit and reviewer, the last three editable.</summary>
public sealed class DashboardSection(int id, string header, string type, string status, string target, string limit, string reviewer) : IDataTableRow
{
    private static readonly IReadOnlyList<ChartRow> Visitors = VisitorSamples.MonthsByDevice;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The section's identifier, its drag key.</summary>
    public int Id { get; } = id;

    /// <summary>The section's header, which opens its viewer.</summary>
    public string Header { get; } = header;

    /// <summary>Opens the viewer on the section (its header's command), once its table sets it.</summary>
    public ICommand? View { get; internal set; }

    /// <summary>The visitors of the last six months by device, which the viewer's chart shows.</summary>
    public IReadOnlyList<ChartRow> Months { get; } = Visitors;

    /// <summary>The section's type.</summary>
    public string Type { get; } = type;

    /// <summary>The section's status.</summary>
    public string Status { get; } = status;

    /// <summary>Whether the section is done (its status badge shows a filled check, otherwise a loader).</summary>
    public bool IsDone => Status == "Done";

    /// <summary>The section's target, edited in its cell.</summary>
    public string Target
    {
        get;
        set => Set(ref field, value, nameof(Target));
    } = target;

    /// <summary>The section's limit, edited in its cell.</summary>
    public string Limit
    {
        get;
        set => Set(ref field, value, nameof(Limit));
    } = limit;

    /// <summary>The section's reviewer, or <c>Assign reviewer</c> for a section without one, whose cell offers a select.</summary>
    public string Reviewer { get; } = reviewer;

    /// <summary>The reviewer chosen in the cell's select, which the select keeps showing as shadcn/ui's uncontrolled one does (the section's reviewer stays as it was).</summary>
    public string? Assignee
    {
        get;
        set => Set(ref field, value, nameof(Assignee));
    }

    /// <summary>Whether the section has a reviewer (its cell shows the name, otherwise a select).</summary>
    public bool IsAssigned => Reviewer != "Assign reviewer";

    /// <inheritdoc />
    public bool IsSelected
    {
        get;
        set => Set(ref field, value, nameof(IsSelected));
    }

    private void Set<T>(ref T field, T value, string name)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

/// <summary>
/// dashboard-01's model: the visitors chart and its time range, the document's sections as a table (selected, paged, its
/// columns shown or hidden, its rows reordered by their handles), the view the tabs show, and the section the viewer shows.
/// </summary>
public sealed class Dashboard : INotifyPropertyChanged
{
    /// <summary>A column key as its menu item shows it (<c>capitalize</c>).</summary>
    public static readonly IValueConverter Capitalize = new FuncValueConverter<string?, string>(text => string.IsNullOrEmpty(text) ? string.Empty : char.ToUpperInvariant(text[0]) + text[1..]);

    /// <summary>Creates the dashboard over the sections of shadcn/ui's <c>data.json</c>, ten rows a page.</summary>
    public Dashboard()
    {
        Sections = new DataTableSource<DashboardSection>(Document) { PageSize = 10 }
            .Column("drag", string.Empty, canSort: false, canHide: false)
            .Column("select", string.Empty, canSort: false, canHide: false)
            .Column("header", "Header", section => section.Header, canHide: false)
            .Column("type", "Section Type", section => section.Type)
            .Column("status", "Status", section => section.Status)
            .Column("target", "Target", section => section.Target)
            .Column("limit", "Limit", section => section.Limit)
            .Column("reviewer", "Reviewer", section => section.Reviewer)
            .Column("actions", string.Empty, canSort: false, canHide: false);
        View = new Run(parameter =>
        {
            Viewing = parameter as DashboardSection ?? Viewing;
            ViewerOpen = true;
        });
        foreach (var section in Sections.Items)
        {
            var shown = section;
            section.View = new Run(_ => View.Execute(shown));
        }
        Move = new Run(parameter =>
        {
            if (parameter is ValueTuple<string, string, int> move && int.TryParse(move.Item1, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                MoveSection(id, (Sections.PageIndex * Sections.PageSize) + move.Item3);
            }
        });
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The visitors of the last three months, shown over the range the toggle group chooses.</summary>
    public ChartInteractive Visitors { get; } = new();

    /// <summary>The document's sections as a table.</summary>
    public DataTableSource<DashboardSection> Sections { get; }

    /// <summary>The page sizes the table offers.</summary>
    public IReadOnlyList<int> PageSizes { get; } = [10, 20, 30, 40, 50];

    /// <summary>The view the tabs show: Outline, Past Performance, Key Personnel or Focus Documents.</summary>
    public int Tab
    {
        get;
        set
        {
            field = value;
            Changed(nameof(Tab));
        }
    }

    /// <summary>The section the viewer shows.</summary>
    public DashboardSection? Viewing
    {
        get;
        set
        {
            field = value;
            Changed(nameof(Viewing));
        }
    }

    /// <summary>Whether the viewer is open (two way with its drawer).</summary>
    public bool ViewerOpen
    {
        get;
        set
        {
            field = value;
            Changed(nameof(ViewerOpen));
        }
    }

    /// <summary>Opens the viewer on the section its parameter is.</summary>
    public ICommand View { get; }

    /// <summary>Moves a section dragged by its handle: the parameter is the drag's <c>(item, target, index)</c>, the item the section's id and the index where it lands on the page.</summary>
    public ICommand Move { get; }

    /// <summary>Moves the section with <paramref name="id"/> to <paramref name="index"/> among all the rows, as dnd-kit's <c>arrayMove</c>.</summary>
    public void MoveSection(int id, int index)
    {
        var from = Sections.Items.ToList().FindIndex(section => section.Id == id);
        if (from >= 0)
        {
            Sections.Items.Move(from, Math.Clamp(index, 0, Sections.Items.Count - 1));
        }
    }

    /// <summary>shadcn/ui's 68 sections.</summary>
    public static IEnumerable<DashboardSection> Document =>
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
        new(21, "System Architecture Overview", "Technical content", "In Process", "24", "18", "Maya Johnson"),
        new(22, "Risk Management Plan", "Narrative", "Done", "15", "22", "Carlos Rodriguez"),
        new(23, "Compliance Documentation", "Legal", "In Process", "31", "27", "Sarah Chen"),
        new(24, "API Documentation", "Technical content", "Done", "8", "12", "Raj Patel"),
        new(25, "User Interface Mockups", "Visual", "In Process", "19", "25", "Leila Ahmadi"),
        new(26, "Database Schema", "Technical content", "Done", "22", "20", "Thomas Wilson"),
        new(27, "Testing Methodology", "Technical content", "In Process", "17", "14", "Assign reviewer"),
        new(28, "Deployment Strategy", "Narrative", "Done", "26", "30", "Eddie Lake"),
        new(29, "Budget Breakdown", "Financial", "In Process", "13", "16", "Jamik Tashpulatov"),
        new(30, "Market Analysis", "Research", "Done", "29", "32", "Sophia Martinez"),
        new(31, "Competitor Comparison", "Research", "In Process", "21", "19", "Assign reviewer"),
        new(32, "Maintenance Plan", "Technical content", "Done", "16", "23", "Alex Thompson"),
        new(33, "User Personas", "Research", "In Process", "27", "24", "Nina Patel"),
        new(34, "Accessibility Compliance", "Legal", "Done", "18", "21", "Assign reviewer"),
        new(35, "Performance Metrics", "Technical content", "In Process", "23", "26", "David Kim"),
        new(36, "Disaster Recovery Plan", "Technical content", "Done", "14", "17", "Jamik Tashpulatov"),
        new(37, "Third-party Integrations", "Technical content", "In Process", "25", "28", "Eddie Lake"),
        new(38, "User Feedback Summary", "Research", "Done", "20", "15", "Assign reviewer"),
        new(39, "Localization Strategy", "Narrative", "In Process", "12", "19", "Maria Garcia"),
        new(40, "Mobile Compatibility", "Technical content", "Done", "28", "31", "James Wilson"),
        new(41, "Data Migration Plan", "Technical content", "In Process", "19", "22", "Assign reviewer"),
        new(42, "Quality Assurance Protocols", "Technical content", "Done", "30", "33", "Priya Singh"),
        new(43, "Stakeholder Analysis", "Research", "In Process", "11", "14", "Eddie Lake"),
        new(44, "Environmental Impact Assessment", "Research", "Done", "24", "27", "Assign reviewer"),
        new(45, "Intellectual Property Rights", "Legal", "In Process", "17", "20", "Sarah Johnson"),
        new(46, "Customer Support Framework", "Narrative", "Done", "22", "25", "Jamik Tashpulatov"),
        new(47, "Version Control Strategy", "Technical content", "In Process", "15", "18", "Assign reviewer"),
        new(48, "Continuous Integration Pipeline", "Technical content", "Done", "26", "29", "Michael Chen"),
        new(49, "Regulatory Compliance", "Legal", "In Process", "13", "16", "Assign reviewer"),
        new(50, "User Authentication System", "Technical content", "Done", "28", "31", "Eddie Lake"),
        new(51, "Data Analytics Framework", "Technical content", "In Process", "21", "24", "Jamik Tashpulatov"),
        new(52, "Cloud Infrastructure", "Technical content", "Done", "16", "19", "Assign reviewer"),
        new(53, "Network Security Measures", "Technical content", "In Process", "29", "32", "Lisa Wong"),
        new(54, "Project Timeline", "Planning", "Done", "14", "17", "Eddie Lake"),
        new(55, "Resource Allocation", "Planning", "In Process", "27", "30", "Assign reviewer"),
        new(56, "Team Structure and Roles", "Planning", "Done", "20", "23", "Jamik Tashpulatov"),
        new(57, "Communication Protocols", "Planning", "In Process", "15", "18", "Assign reviewer"),
        new(58, "Success Metrics", "Planning", "Done", "30", "33", "Eddie Lake"),
        new(59, "Internationalization Support", "Technical content", "In Process", "23", "26", "Jamik Tashpulatov"),
        new(60, "Backup and Recovery Procedures", "Technical content", "Done", "18", "21", "Assign reviewer"),
        new(61, "Monitoring and Alerting System", "Technical content", "In Process", "25", "28", "Daniel Park"),
        new(62, "Code Review Guidelines", "Technical content", "Done", "12", "15", "Eddie Lake"),
        new(63, "Documentation Standards", "Technical content", "In Process", "27", "30", "Jamik Tashpulatov"),
        new(64, "Release Management Process", "Planning", "Done", "22", "25", "Assign reviewer"),
        new(65, "Feature Prioritization Matrix", "Planning", "In Process", "19", "22", "Emma Davis"),
        new(66, "Technical Debt Assessment", "Technical content", "Done", "24", "27", "Eddie Lake"),
        new(67, "Capacity Planning", "Planning", "In Process", "21", "24", "Jamik Tashpulatov"),
        new(68, "Service Level Agreements", "Legal", "Done", "26", "29", "Assign reviewer"),
    ];

    private void Changed(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed class Run(Action<object?> run) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => run(parameter);
    }
}
