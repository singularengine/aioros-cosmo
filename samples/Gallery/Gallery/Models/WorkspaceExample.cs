using System.Collections.ObjectModel;
using Aioros.Cosmo.Tables;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>A page of the workspace's navigation: its title, path, Lucide icon and its sub-pages.</summary>
public sealed record WorkspacePage(string Title, string Path, string Icon, IReadOnlyList<WorkspacePage> Pages)
{
    public WorkspacePage(string title, string path, string icon)
        : this(title, path, icon, [])
    {
    }
}

/// <summary>What the Workspace's loader reads from the request: the sidebar's open state the browser keeps in its cookie.</summary>
public sealed record WorkspaceState(bool SidebarOpen);

/// <summary>
/// The Workspace example's model: a sidebar-07 app shell (its open state, the navigation and the current page with its
/// breadcrumb), a command palette (open state, search) and the payments table, all bound from the page.
/// </summary>
public sealed partial class WorkspaceExample : ObservableObject
{
    public WorkspaceExample(WorkspaceState state)
    {
        Payments = new PaymentLedger().Source;
        SidebarOpen = state.SidebarOpen;
    }

    /// <summary>Keeps what the commands set when the loader runs again after one, taking the sidebar's state the browser kept.</summary>
    public void Receive(WorkspaceState state) => SidebarOpen = state.SidebarOpen;

    /// <summary>Whether a page's path is the current page.</summary>
    public static readonly Avalonia.Data.Converters.IMultiValueConverter IsCurrent = new Avalonia.Data.Converters.FuncMultiValueConverter<string?, bool>(values =>
    {
        var list = values.ToList();
        return list.Count == 2 && list[0] == list[1];
    });

    public IReadOnlyList<WorkspacePage> Platform { get; } =
    [
        new("Dashboard", "/workspace", "layers"),
        new("Payments", "/workspace/payments", "credit-card"),
        new("Customers", "/workspace/customers", "users"),
        new("Settings", "/workspace/settings", "settings-2", [new("General", "/workspace/settings/general", "settings"), new("Team", "/workspace/settings/team", "users"), new("Billing", "/workspace/settings/billing", "credit-card")]),
    ];

    public IReadOnlyList<WorkspacePage> Projects { get; } =
    [
        new("Design Engineering", "/workspace/projects/design", "frame"),
        new("Sales & Marketing", "/workspace/projects/sales", "chart-pie"),
        new("Travel", "/workspace/projects/travel", "map"),
    ];

    public DataTableSource<Payment> Payments { get; }

    [ObservableProperty]
    public partial bool SidebarOpen { get; set; }

    [ObservableProperty]
    public partial bool PaletteOpen { get; set; }

    [ObservableProperty]
    public partial string? PaletteSearch { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Section))]
    public partial string Current { get; set; } = "/workspace/payments";

    [ObservableProperty]
    public partial string LastAction { get; set; } = "Press Ctrl+K for the command palette.";

    public string Section => Platform.Concat(Platform.SelectMany(page => page.Pages)).Concat(Projects).FirstOrDefault(page => page.Path == Current)?.Title ?? "Dashboard";

    public ObservableCollection<string> Recent { get; } = ["Payments", "Customers", "Billing"];

    [RelayCommand]
    private void Navigate(string? path)
    {
        Current = path ?? Current;
        PaletteOpen = false;
    }

    [RelayCommand]
    private void TogglePalette() => PaletteOpen = !PaletteOpen;

    [RelayCommand]
    private void Act(string? action)
    {
        LastAction = action ?? LastAction;
        PaletteOpen = false;
    }
}
