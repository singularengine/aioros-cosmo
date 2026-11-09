using System.ComponentModel;
using System.Windows.Input;

namespace Gallery.Models;

/// <summary>A team of the app shells' team switcher: its name, plan and the name of its Lucide logo, which the page draws.</summary>
public sealed record ShellTeam(string Name, string Plan, string Logo, string Shortcut);

/// <summary>A mail of sidebar-09's inbox: who sent it, its subject, when and its first lines.</summary>
public sealed record ShellMail(string Name, string Email, string Subject, string Date, string Teaser);

/// <summary>
/// The app shell blocks' model (shadcn/ui's sidebar-07 to sidebar-16): the team the switcher shows and selects, and
/// sidebar-09's folder, its mails, the unread filter and the sidebar's open state, which choosing a folder opens.
/// </summary>
public sealed class ShellExample : INotifyPropertyChanged
{
    private const string GalleryVerticalEnd = "gallery-vertical-end";
    private const string AudioWaveform = "audio-waveform";
    private const string Command = "command";

    /// <summary>Creates the model with sidebar-07's teams, Acme Inc shown.</summary>
    public ShellExample()
    {
        Teams = Workspace;
        ActiveTeam = Teams[0];
        SelectTeam = new Run(parameter => ActiveTeam = Teams.FirstOrDefault(team => team.Name == parameter as string) ?? ActiveTeam);
        SelectFolder = new Run(parameter =>
        {
            Folder = parameter as string ?? Folder;
            SidebarOpen = true;
        });
        OpenSettings = new Run(_ => SettingsOpen = true);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>sidebar-07's teams.</summary>
    public static IReadOnlyList<ShellTeam> Workspace { get; } = [new("Acme Inc", "Enterprise", GalleryVerticalEnd, "Ctrl+1"), new("Acme Corp.", "Startup", AudioWaveform, "Ctrl+2"), new("Evil Corp.", "Free", Command, "Ctrl+3")];

    /// <summary>sidebar-10's and sidebar-15's teams.</summary>
    public static IReadOnlyList<ShellTeam> Notes { get; } = [new("Acme Inc", "Enterprise", Command, "Ctrl+1"), new("Acme Corp.", "Startup", AudioWaveform, "Ctrl+2"), new("Evil Corp.", "Free", Command, "Ctrl+3")];

    /// <summary>The block whose teams the switcher lists: <c>sidebar-10</c> and <c>sidebar-15</c> list their own, any other sidebar-07's.</summary>
    public string Block
    {
        get;
        set
        {
            field = value;
            Teams = value is "sidebar-10" or "sidebar-15" ? Notes : Workspace;
            ActiveTeam = Teams[0];
            Changed(nameof(Teams));
        }
    } = "sidebar-07";

    /// <summary>The teams the switcher lists.</summary>
    public IReadOnlyList<ShellTeam> Teams { get; private set; }

    /// <summary>The team the switcher shows.</summary>
    public ShellTeam ActiveTeam
    {
        get;
        set
        {
            field = value;
            foreach (var name in (string[])[nameof(ActiveTeam), nameof(ShowsGallery), nameof(ShowsWaveform), nameof(ShowsCommand)])
            {
                Changed(name);
            }
        }
    }

    /// <summary>Whether the team shown has the gallery logo (sidebar-07's Acme Inc).</summary>
    public bool ShowsGallery => ActiveTeam.Logo == GalleryVerticalEnd;

    /// <summary>Whether the team shown has the audio waveform logo (Acme Corp.).</summary>
    public bool ShowsWaveform => ActiveTeam.Logo == AudioWaveform;

    /// <summary>Whether the team shown has the command logo.</summary>
    public bool ShowsCommand => ActiveTeam.Logo == Command;

    /// <summary>Shows the team its parameter names.</summary>
    public ICommand SelectTeam { get; }

    /// <summary>sidebar-09's folder, the active item of its icon rail and its list's title.</summary>
    public string Folder
    {
        get;
        set
        {
            field = value;
            foreach (var name in (string[])[nameof(Folder), nameof(InInbox), nameof(InDrafts), nameof(InSent), nameof(InJunk), nameof(InTrash)])
            {
                Changed(name);
            }
        }
    } = "Inbox";

    /// <summary>Whether the Inbox is the folder shown.</summary>
    public bool InInbox => Folder == "Inbox";

    /// <summary>Whether Drafts is the folder shown.</summary>
    public bool InDrafts => Folder == "Drafts";

    /// <summary>Whether Sent is the folder shown.</summary>
    public bool InSent => Folder == "Sent";

    /// <summary>Whether Junk is the folder shown.</summary>
    public bool InJunk => Folder == "Junk";

    /// <summary>Whether Trash is the folder shown.</summary>
    public bool InTrash => Folder == "Trash";

    /// <summary>Opens the folder its parameter names, and the sidebar with it, as sidebar-09's rail does.</summary>
    public ICommand SelectFolder { get; }

    /// <summary>Whether the sidebar is open (two way with the provider).</summary>
    public bool SidebarOpen
    {
        get;
        set
        {
            field = value;
            Changed(nameof(SidebarOpen));
        }
    } = true;

    /// <summary>sidebar-09's unread filter.</summary>
    public bool Unreads
    {
        get;
        set
        {
            field = value;
            Changed(nameof(Unreads));
        }
    }

    /// <summary>Whether sidebar-10's page actions are open, as the block opens them once it is shown.</summary>
    public bool ActionsOpen
    {
        get;
        set
        {
            field = value;
            Changed(nameof(ActionsOpen));
        }
    } = true;

    /// <summary>Whether sidebar-13's settings dialog is open, as the block opens it from the start.</summary>
    public bool SettingsOpen
    {
        get;
        set
        {
            field = value;
            Changed(nameof(SettingsOpen));
        }
    } = true;

    /// <summary>Opens sidebar-13's settings dialog.</summary>
    public ICommand OpenSettings { get; }

    /// <summary>sidebar-09's mails.</summary>
    public IReadOnlyList<ShellMail> Mails { get; } =
    [
        new("William Smith", "williamsmith@example.com", "Meeting Tomorrow", "09:34 AM", "Hi team, just a reminder about our meeting tomorrow at 10 AM.\nPlease come prepared with your project updates."),
        new("Alice Smith", "alicesmith@example.com", "Re: Project Update", "Yesterday", "Thanks for the update. The progress looks great so far.\nLet's schedule a call to discuss the next steps."),
        new("Bob Johnson", "bobjohnson@example.com", "Weekend Plans", "2 days ago", "Hey everyone! I'm thinking of organizing a team outing this weekend.\nWould you be interested in a hiking trip or a beach day?"),
        new("Emily Davis", "emilydavis@example.com", "Re: Question about Budget", "2 days ago", "I've reviewed the budget numbers you sent over.\nCan we set up a quick call to discuss some potential adjustments?"),
        new("Michael Wilson", "michaelwilson@example.com", "Important Announcement", "1 week ago", "Please join us for an all-hands meeting this Friday at 3 PM.\nWe have some exciting news to share about the company's future."),
        new("Sarah Brown", "sarahbrown@example.com", "Re: Feedback on Proposal", "1 week ago", "Thank you for sending over the proposal. I've reviewed it and have some thoughts.\nCould we schedule a meeting to discuss my feedback in detail?"),
        new("David Lee", "davidlee@example.com", "New Project Idea", "1 week ago", "I've been brainstorming and came up with an interesting project concept.\nDo you have time this week to discuss its potential impact and feasibility?"),
        new("Olivia Wilson", "oliviawilson@example.com", "Vacation Plans", "1 week ago", "Just a heads up that I'll be taking a two-week vacation next month.\nI'll make sure all my projects are up to date before I leave."),
        new("James Martin", "jamesmartin@example.com", "Re: Conference Registration", "1 week ago", "I've completed the registration for the upcoming tech conference.\nLet me know if you need any additional information from my end."),
        new("Sophia White", "sophiawhite@example.com", "Team Dinner", "1 week ago", "To celebrate our recent project success, I'd like to organize a team dinner.\nAre you available next Friday evening? Please let me know your preferences."),
    ];

    private void Changed(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed class Run(Action<object?> run) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => run(parameter);
    }
}
