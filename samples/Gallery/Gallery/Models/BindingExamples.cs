using System.Globalization;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>A plan a button shows through its content template.</summary>
public sealed record Plan(string Name, string Price);

/// <summary>The Binding page's view model: plain MVVM with two-way properties, commands with parameters and a converter, no code-behind.</summary>
public sealed partial class BindingExamples : ObservableObject
{
    /// <summary>How many seats a team takes, two-way bound to a field and changed by commands.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total))]
    public partial decimal Seats { get; set; } = 3;

    /// <summary>The team's name, two-way bound to a text field.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InviteCommand))]
    public partial string Team { get; set; } = "Acme";

    /// <summary>Whether the terms are accepted, two-way bound to a checkbox.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InviteCommand))]
    public partial bool Accepted { get; set; }

    /// <summary>What the last command did.</summary>
    [ObservableProperty]
    public partial string Status { get; set; } = "Nothing sent yet";

    /// <summary>The plan the team is on, shown by a data template.</summary>
    public Plan Current { get; } = new("Pro", "$12 per seat");

    /// <summary>The monthly total for the seats.</summary>
    public string Total => string.Create(CultureInfo.InvariantCulture, $"${Seats * 12:0} per month");

    [RelayCommand]
    private void Add(string? step) => Seats = Math.Clamp(Seats + decimal.Parse(step ?? "1", CultureInfo.InvariantCulture), 0, 99);

    [RelayCommand(CanExecute = nameof(CanInvite))]
    private void Invite() => Status = string.Create(CultureInfo.InvariantCulture, $"Invited {Seats:0} people to {Team}");

    private bool CanInvite() => Accepted && !string.IsNullOrWhiteSpace(Team);
}

/// <summary>Converts a seat count to whether it is over the plan's limit of ten, the parameter if one is given.</summary>
public sealed class OverLimit : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is decimal seats && seats > (parameter is string limit ? decimal.Parse(limit, CultureInfo.InvariantCulture) : 10);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
