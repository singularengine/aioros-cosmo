using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>A user a hover card shows.</summary>
public sealed record GalleryUser(string Name, string Handle, string About, string Joined);

/// <summary>The overlay pages' model: what popovers, tooltips and hover cards show, and commands reading it.</summary>
public sealed partial class OverlayExamples : ObservableObject
{
    [ObservableProperty]
    public partial string Width { get; set; } = "100%";

    [ObservableProperty]
    public partial string Height { get; set; } = "25px";

    [ObservableProperty]
    public partial string Status { get; set; } = "Nothing applied yet";

    [ObservableProperty]
    public partial int Unread { get; set; } = 3;

    public GalleryUser User { get; } = new("Next.js", "@nextjs", "The React Framework – created and maintained by @vercel.", "December 2021");

    [RelayCommand]
    private void Apply(string? source) => Status = $"Applied {Width} by {Height} from the {source}";

    [ObservableProperty]
    public partial string Choice { get; set; } = "Nothing chosen yet";

    [ObservableProperty]
    public partial bool StatusBar { get; set; } = true;

    public IReadOnlyList<string> Teams { get; } = ["Acme", "Monsters", "Rocket"];

    public bool Locked { get; } = true;

    private TaskCompletionSource<string>? _answer;

    [ObservableProperty]
    public partial string Name { get; set; } = "Pedro Duarte";

    [ObservableProperty]
    public partial string Username { get; set; } = "@peduarte";

    [ObservableProperty]
    public partial string Outcome { get; set; } = "No answer yet";

    [ObservableProperty]
    public partial bool EditOpen { get; set; }

    [ObservableProperty]
    public partial bool ConfirmOpen { get; set; }

    [ObservableProperty]
    public partial bool ShareOpen { get; set; }

    [ObservableProperty]
    public partial bool AllowOpen { get; set; }

    [ObservableProperty]
    public partial bool SheetOpen { get; set; }

    [ObservableProperty]
    public partial Aioros.Cosmo.Overlays.SheetSide Side { get; set; }

    [ObservableProperty]
    public partial bool DrawerOpen { get; set; }

    [ObservableProperty]
    public partial int Goal { get; set; } = 350;

    partial void OnEditOpenChanged(bool value) => Closed(value);

    partial void OnConfirmOpenChanged(bool value) => Closed(value);

    partial void OnSheetOpenChanged(bool value) => Closed(value);

    partial void OnShareOpenChanged(bool value) => Closed(value);

    partial void OnAllowOpenChanged(bool value) => Closed(value);

    partial void OnDrawerOpenChanged(bool value) => Closed(value);

    private void Closed(bool open)
    {
        if (!open)
        {
            _answer?.TrySetResult("closed");
        }
    }

    private async Task AskAsync(Action open, string question)
    {
        _answer = new TaskCompletionSource<string>();
        open();
        var answer = await _answer.Task;
        Outcome = $"{question}: {answer}";
    }

    [RelayCommand]
    private Task EditAsync() => AskAsync(() => EditOpen = true, "Edit profile");

    [RelayCommand]
    private Task ConfirmAsync() => AskAsync(() => ConfirmOpen = true, "Delete account");

    [RelayCommand]
    private Task SheetAsync(string? side)
    {
        Side = Enum.TryParse<Aioros.Cosmo.Overlays.SheetSide>(side, out var parsed) ? parsed : Aioros.Cosmo.Overlays.SheetSide.Right;
        return AskAsync(() => SheetOpen = true, $"Sheet ({Side})");
    }

    [RelayCommand]
    private Task ShareAsync() => AskAsync(() => ShareOpen = true, "Share link");

    [RelayCommand]
    private Task AllowAsync() => AskAsync(() => AllowOpen = true, "Accessory");

    [RelayCommand]
    private Task DrawerAsync() => AskAsync(() => DrawerOpen = true, "Move goal");

    [RelayCommand]
    private void Answer(string? answer)
    {
        _answer?.TrySetResult(answer ?? string.Empty);
        (EditOpen, ConfirmOpen, SheetOpen, DrawerOpen, ShareOpen, AllowOpen) = (false, false, false, false, false, false);
    }

    [RelayCommand]
    private void Adjust(string? step) => Goal = Math.Clamp(Goal + int.Parse(step ?? "0", System.Globalization.CultureInfo.InvariantCulture), 200, 400);

    public System.Collections.ObjectModel.ObservableCollection<Aioros.Cosmo.Overlays.Toast> Toasts { get; } = [];

    [ObservableProperty]
    public partial string Undone { get; set; } = "Nothing undone yet";

    [RelayCommand]
    private void Toast() => Toasts.Add(new Aioros.Cosmo.Overlays.Toast
    {
        Title = "Event has been created",
        Description = "Sunday, December 03, 2023 at 9:00 AM",
        ActionText = "Undo",
        ActionCommand = UndoCommand,
        ActionParameter = "event",
    });

    [RelayCommand]
    private void Stack()
    {
        Toasts.Add(new Aioros.Cosmo.Overlays.Toast { Title = "Event has been created", Description = "Sunday, December 03, 2023 at 9:00 AM" });
        Toasts.Add(new Aioros.Cosmo.Overlays.Toast { Title = "Event has been created", Kind = Aioros.Cosmo.Overlays.ToastKind.Success });
        Toast();
    }

    [RelayCommand]
    private void Closable() => Toasts.Add(new Aioros.Cosmo.Overlays.Toast
    {
        Title = "Event has been created",
        Description = "Sunday, December 03, 2023 at 9:00 AM",
        CloseButton = true,
    });

    [RelayCommand]
    private void Undo(string? what) => Undone = $"Undid the {what}";

    [RelayCommand]
    private void Kind(string? kind)
    {
        var parsed = Enum.TryParse<Aioros.Cosmo.Overlays.ToastKind>(kind, out var value) ? value : Aioros.Cosmo.Overlays.ToastKind.Default;
        var title = parsed switch
        {
            Aioros.Cosmo.Overlays.ToastKind.Info => "Be at the area 10 minutes before the event time",
            Aioros.Cosmo.Overlays.ToastKind.Warning => "Event start time cannot be earlier than 8am",
            Aioros.Cosmo.Overlays.ToastKind.Error => "Event has not been created",
            _ => "Event has been created",
        };
        Toasts.Add(new Aioros.Cosmo.Overlays.Toast { Title = title, Kind = parsed });
    }

    [RelayCommand]
    private async Task PromiseAsync()
    {
        var toast = new Aioros.Cosmo.Overlays.Toast { Title = "Loading...", Kind = Aioros.Cosmo.Overlays.ToastKind.Loading };
        Toasts.Add(toast);
        await Task.Delay(TimeSpan.FromSeconds(2), TimeProvider.System);
        toast.Resolve(Aioros.Cosmo.Overlays.ToastKind.Success, "Event has been created");
    }

    [RelayCommand]
    private void Read() => Unread = Math.Max(0, Unread - 1);

    [RelayCommand]
    private void Choose(string? item) => Choice = $"You chose {item}";

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete() => Choice = "Deleted";

    private bool CanDelete() => !Locked;
}
