using System.Collections.ObjectModel;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>A team member an avatar shows: their name and, when they set one, their picture.</summary>
public sealed record Member(string Name, string? Picture)
{
    /// <summary>The member's initials, as the avatar's fallback.</summary>
    public string Initials => string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => char.ToUpperInvariant(part[0])));
}

/// <summary>A project an item or an empty state shows.</summary>
public sealed record Project(string Name, string Updated);

/// <summary>The display pages' model: shortcuts, members, projects and the commands changing them.</summary>
public sealed partial class DisplayExamples : ObservableObject
{
    /// <summary>Turns a shortcut's modifier into the key a Mac shows when <c>Mac</c> is the parameter's state.</summary>
    public static readonly IValueConverter MacKey = new FuncValueConverter<bool, string>(mac => mac ? "⌘" : "Ctrl");

    /// <summary>The loading button's label.</summary>
    public static readonly IValueConverter LoadingText = new FuncValueConverter<bool, string>(loading => loading ? "Loading..." : "Load");

    [ObservableProperty]
    public partial bool ShowSeparator { get; set; } = true;

    [ObservableProperty]
    public partial bool UseMacKeys { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; } = true;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool Failed { get; set; }

    [ObservableProperty]
    public partial string AlertTitle { get; set; } = "Heads up!";

    [ObservableProperty]
    public partial string AlertMessage { get; set; } = "You can add components to your app using the cli.";

    [ObservableProperty]
    public partial int Saves { get; set; }

    public ObservableCollection<Member> Members { get; } = [new("Shad Cn", "/Assets/avatar-shadcn.png"), new("Max Leiter", "/Assets/avatar-maxleiter.png"), new("Evil Rabbit", "/Assets/avatar-evilrabbit.png")];

    public ObservableCollection<Project> Projects { get; } = [new("Cosmo", "Updated today"), new("Pictor", "Updated yesterday"), new("Aioros", "Updated last week")];

    public ObservableCollection<string> Levels { get; } = ["1st level of puns: 5 gold coins", "2nd level of jokes: 10 gold coins"];

    public string MemberCount => $"{Members.Count} members";

    public string ProjectCount => $"{Projects.Count} items";

    [RelayCommand]
    private void AddMember()
    {
        Members.Add(new Member("Riley Park", null));
        OnPropertyChanged(nameof(MemberCount));
    }

    [RelayCommand]
    private void Remove(Project? project)
    {
        if (project is not null && Projects.Remove(project))
        {
            OnPropertyChanged(nameof(ProjectCount));
        }
    }

    [RelayCommand]
    private void Restore()
    {
        Projects.Clear();
        Projects.Add(new("Cosmo", "Updated today"));
        OnPropertyChanged(nameof(ProjectCount));
    }

    [ObservableProperty]
    public partial string KeysStatus { get; set; } = "Showing Windows and Linux keys";

    [RelayCommand]
    private void ApplyKeys() => KeysStatus = UseMacKeys ? "Showing Mac keys" : "Showing Windows and Linux keys";

    [RelayCommand]
    private void Load() => Loading = !Loading;

    [RelayCommand]
    private void AddLevel() => Levels.Add($"{Levels.Count + 1}th level of one-liners: {(Levels.Count + 1) * 10} gold coins");

    [RelayCommand]
    private void Save()
    {
        Failed = string.IsNullOrWhiteSpace(Name);
        AlertTitle = Failed ? "Unable to save your profile." : "Success! Your changes have been saved";
        AlertMessage = Failed ? "Please enter your name and try again." : $"Saved as {Name.Trim()}.";
        Saves++;
    }
}
