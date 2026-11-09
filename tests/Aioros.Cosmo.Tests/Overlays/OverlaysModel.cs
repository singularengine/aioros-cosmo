using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aioros.Cosmo.Tests.Overlays;

/// <summary>A user a hover card shows.</summary>
public sealed record UserModel(string Name, string Handle, string Joined);

/// <summary>A note a tooltip shows through its content template.</summary>
public sealed record NoteModel(string Text);

/// <summary>A recent file a menu lists through its item template.</summary>
public sealed record RecentFile(string Name);

/// <summary>A command that never executes, as a menu item whose action is unavailable.</summary>
public sealed class BlockedCommand : System.Windows.Input.ICommand
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

/// <summary>A command that starts an asynchronous action and keeps its task, as a command awaiting a dialog's answer.</summary>
public sealed class AwaitingCommand(Func<Task> run) : System.Windows.Input.ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public Task? Running { get; private set; }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => Running = run();
}

/// <summary>A plain view model for the overlays: open states, texts, objects drawn by templates and a command recording its parameters.</summary>
public sealed class OverlaysModel : INotifyPropertyChanged
{
    private bool _popoverOpen;
    private string _layer = "Background";
    private int _unread = 3;
    private NoteModel _note = new("Saved");
    private UserModel _user = new("Next.js", "@nextjs", "December 2021");

    private TaskCompletionSource<string>? _answer;
    private bool _dialogOpen;
    private bool _confirmOpen;
    private bool _sheetOpen;
    private string _name = "Pedro Duarte";

    public OverlaysModel()
    {
        Run = new Desktop.Command(parameter => Ran.Add(parameter));
        OpenDialog = new AwaitingCommand(() => AskAsync(() => DialogOpen = true));
        Confirm = new AwaitingCommand(() => AskAsync(() => ConfirmOpen = true));
        Answer = new Desktop.Command(parameter =>
        {
            _answer?.TrySetResult(parameter as string ?? string.Empty);
            DialogOpen = false;
            ConfirmOpen = false;
        });
    }

    public AwaitingCommand OpenDialog { get; }

    public AwaitingCommand Confirm { get; }

    public System.Windows.Input.ICommand Answer { get; }

    public string? Outcome { get; private set; }

    public string Name { get => _name; set => Set(ref _name, value); }

    public bool DialogOpen
    {
        get => _dialogOpen;
        set
        {
            Set(ref _dialogOpen, value);
            if (!value)
            {
                _answer?.TrySetResult("closed");
            }
        }
    }

    public bool ConfirmOpen
    {
        get => _confirmOpen;
        set
        {
            Set(ref _confirmOpen, value);
            if (!value)
            {
                _answer?.TrySetResult("closed");
            }
        }
    }

    public bool SheetOpen { get => _sheetOpen; set => Set(ref _sheetOpen, value); }

    private bool _drawerOpen;

    public bool DrawerOpen { get => _drawerOpen; set => Set(ref _drawerOpen, value); }

    public System.Collections.ObjectModel.ObservableCollection<Aioros.Cosmo.Overlays.Toast> Toasts { get; } = [];

    private async Task AskAsync(Action open)
    {
        _answer = new TaskCompletionSource<string>();
        open();
        Outcome = await _answer.Task;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public System.Windows.Input.ICommand Run { get; }

    public List<object?> Ran { get; } = [];

    public bool PopoverOpen { get => _popoverOpen; set => Set(ref _popoverOpen, value); }

    private bool _menuOpen;
    private bool _statusBar = true;
    private bool _fullUrls;

    public bool MenuOpen { get => _menuOpen; set => Set(ref _menuOpen, value); }

    public bool StatusBar { get => _statusBar; set => Set(ref _statusBar, value); }

    public bool FullUrls { get => _fullUrls; set => Set(ref _fullUrls, value); }

    public System.Windows.Input.ICommand Blocked { get; } = new BlockedCommand();

    public IReadOnlyList<RecentFile> Recent { get; } = [new("notes.md"), new("plan.xlsx")];

    public string Layer { get => _layer; set => Set(ref _layer, value); }

    public int Unread { get => _unread; set => Set(ref _unread, value); }

    public NoteModel Note { get => _note; set => Set(ref _note, value); }

    public UserModel User { get => _user; set => Set(ref _user, value); }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
