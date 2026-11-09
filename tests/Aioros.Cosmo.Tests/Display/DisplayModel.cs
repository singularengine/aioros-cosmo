using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Aioros.Cosmo.Tests.Overlays;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Aioros.Cosmo.Tests.Display;

/// <summary>A team member an avatar shows.</summary>
public sealed record MemberModel(string Name, IImage? Picture);

/// <summary>A project an item shows.</summary>
public sealed record ProjectModel(string Name, int Files);

/// <summary>A plain view model for the display components: states, texts, collections and commands changing them.</summary>
public sealed class DisplayModel : INotifyPropertyChanged
{
    /// <summary>A name's initials, as an avatar's fallback.</summary>
    public static readonly IValueConverter Initials = new FuncValueConverter<string, string>(name => string.Concat((name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => char.ToUpperInvariant(part[0]))));

    private bool _hideSeparator;
    private bool _vertical;
    private bool _loading = true;
    private bool _failed;
    private string _alertTitle = "Heads up!";
    private string _alertMessage = "Nothing saved yet.";
    private int _saves;
    private double _ratio = 16.0 / 9;
    private bool _showLead = true;
    private string _shortcut = "Ctrl";
    private TaskCompletionSource? _saving;

    public DisplayModel()
    {
        Finish = new Desktop.Command(() => Loading = false);
        Fail = new Desktop.Command(() =>
        {
            Failed = true;
            AlertTitle = "Unable to save.";
            AlertMessage = "The name is missing.";
        });
        Succeed = new Desktop.Command(() =>
        {
            Failed = false;
            Saves++;
            AlertTitle = "Saved";
            AlertMessage = "ignored";
        });
        AddMember = new Desktop.Command(() => Members.Add(new MemberModel("Riley Park", null)));
        ClearPicture = new Desktop.Command(() => Members[0] = Members[0] with { Picture = null });
        AddLevel = new Desktop.Command(() => Levels.Add($"Level {Levels.Count + 1}"));
        AddProject = new Desktop.Command(() => Projects.Add(new ProjectModel("Cosmo", 12)));
        RemoveProject = new Desktop.Command(parameter =>
        {
            if (parameter is ProjectModel project)
            {
                Files.Remove(project);
            }
        });
        Save = new AwaitingCommand(SaveAsync);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public System.Windows.Input.ICommand Finish { get; }

    public System.Windows.Input.ICommand Fail { get; }

    public System.Windows.Input.ICommand Succeed { get; }

    public System.Windows.Input.ICommand AddMember { get; }

    public System.Windows.Input.ICommand ClearPicture { get; }

    public System.Windows.Input.ICommand AddLevel { get; }

    public System.Windows.Input.ICommand AddProject { get; }

    public System.Windows.Input.ICommand RemoveProject { get; }

    public AwaitingCommand Save { get; }

    public ObservableCollection<MemberModel> Members { get; } = [];

    public ObservableCollection<string> Levels { get; } = ["Level 1", "Level 2"];

    public ObservableCollection<ProjectModel> Projects { get; } = [];

    public ObservableCollection<ProjectModel> Files { get; } = [new("Cosmo", 12), new("Pictor", 7), new("Aioros", 31)];

    public ObservableCollection<string> Keys { get; } = ["Ctrl", "K"];

    public bool HideSeparator { get => _hideSeparator; set => Set(ref _hideSeparator, value); }

    public bool Vertical { get => _vertical; set => Set(ref _vertical, value); }

    public bool Loading { get => _loading; set => Set(ref _loading, value); }

    public bool Failed { get => _failed; set => Set(ref _failed, value); }

    public string AlertTitle { get => _alertTitle; set => Set(ref _alertTitle, value); }

    public string AlertMessage { get => _alertMessage; set => Set(ref _alertMessage, value); }

    public int Saves { get => _saves; set => Set(ref _saves, value); }

    public double Ratio { get => _ratio; set => Set(ref _ratio, value); }

    public bool ShowLead { get => _showLead; set => Set(ref _showLead, value); }

    public string Shortcut { get => _shortcut; set => Set(ref _shortcut, value); }

    public bool Saving => _saving is not null;

    /// <summary>Ends the save the button started.</summary>
    public void EndSave()
    {
        _saving?.TrySetResult();
    }

    private async Task SaveAsync()
    {
        _saving = new TaskCompletionSource();
        OnPropertyChanged(nameof(Saving));
        await _saving.Task;
        _saving = null;
        OnPropertyChanged(nameof(Saving));
    }

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
