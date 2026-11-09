using System.Text.RegularExpressions;
using Aioros.Cosmo.Chat;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Gallery.Models;

/// <summary>
/// One scripted chat turn of shadcn/ui's chat examples, as their mock transport (<c>@shadcn/helpers</c>' <c>createChat</c>)
/// streams it: the question, then the answer's reasoning, tool calls, sources and text arriving over time, which the
/// examples' XAML binds part by part. A text or a reasoning arrives a word at a time (<c>\S+\s*</c>), each word after its
/// delay; a tool call shows running at once and its output after its own wait.
/// </summary>
public sealed partial class ChatReplay(int tools) : ObservableObject
{
    private bool? openChoice;

    /// <summary>The clock the turn plays on.</summary>
    public Playback Playback { get; } = new();

    [ObservableProperty]
    public partial bool Asked { get; set; }

    [ObservableProperty]
    public partial bool Answering { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnswer))]
    public partial string Answer { get; set; } = string.Empty;

    /// <summary>Whether the answer's text has started.</summary>
    public bool HasAnswer => Answer.Length > 0;

    [ObservableProperty]
    public partial bool Idle { get; set; } = true;

    [ObservableProperty]
    public partial string Action { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReasoningEmpty), nameof(ReasoningShown))]
    public partial bool ReasoningStarted { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReasoningEmpty), nameof(ReasoningShown))]
    public partial string Reasoning { get; set; } = string.Empty;

    /// <summary>The reasoning has started and shows no text yet: a plain line, not a collapsible.</summary>
    public bool ReasoningEmpty => ReasoningStarted && Reasoning.Length == 0;

    /// <summary>The reasoning shows text: a collapsible line.</summary>
    public bool ReasoningShown => ReasoningStarted && Reasoning.Length > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReasoningOpen), nameof(ReasoningStatus), nameof(ReasoningDone), nameof(ThinkingTitle))]
    public partial bool ReasoningRunning { get; set; }

    /// <summary>The reasoning has ended.</summary>
    public bool ReasoningDone => !ReasoningRunning;

    /// <summary>The reasoning line's look: running while it streams.</summary>
    public TraceStatus ReasoningStatus => ReasoningRunning ? TraceStatus.Running : TraceStatus.Done;

    /// <summary>The reasoning line's title once it ended (<c>Thought</c>, <c>Thought for 5s</c>).</summary>
    public string Thought { get; init; } = "Thought";

    /// <summary>The reasoning line's title.</summary>
    public string ThinkingTitle => ReasoningRunning ? "Thinking" : Thought;

    /// <summary>Whether the reasoning is open: as the visitor left it, or open while it runs (<c>open ?? running</c>).</summary>
    public bool ReasoningOpen
    {
        get => openChoice ?? ReasoningRunning;
        set
        {
            if (value != ReasoningOpen)
            {
                openChoice = value;
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    public partial bool[] ToolsRunning { get; set; } = new bool[tools];

    [ObservableProperty]
    public partial bool[] ToolsShown { get; set; } = new bool[tools];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SourcesTitle), nameof(SourcesStatus), nameof(SourcesDone))]
    public partial bool SourcesRunning { get; set; }

    [ObservableProperty]
    public partial bool SourcesShown { get; set; }

    /// <summary>The sources line's title.</summary>
    public string SourcesTitle => SourcesRunning ? "Using 2 sources" : "Used 2 sources";

    /// <summary>The sources line's look.</summary>
    public TraceStatus SourcesStatus => SourcesRunning ? TraceStatus.Running : TraceStatus.Done;

    /// <summary>The sources are no longer being used.</summary>
    public bool SourcesDone => !SourcesRunning;

    /// <summary>Clears the turn, as <c>setMessages([])</c>.</summary>
    public void Clear()
    {
        openChoice = null;
        Asked = false;
        Answering = false;
        Answer = string.Empty;
        ReasoningStarted = false;
        Reasoning = string.Empty;
        ReasoningRunning = false;
        ToolsRunning = new bool[ToolsRunning.Length];
        ToolsShown = new bool[ToolsShown.Length];
        SourcesRunning = false;
        SourcesShown = false;
        OnPropertyChanged(nameof(ReasoningOpen));
    }

    /// <summary>Shows a tool call running, or done with its output.</summary>
    public void Tool(int index, bool running)
    {
        ToolsRunning = [.. ToolsRunning.Select((value, at) => at == index ? running : value)];
        ToolsShown = [.. ToolsShown.Select((value, at) => at == index ? !running : value)];
    }

    /// <summary>The words a text streams in, each with what it adds.</summary>
    public static IEnumerable<string> Words(string text) => Words().Matches(text).Select(static match => match.Value);

    [GeneratedRegex(@"\S+\s*")]
    private static partial Regex Words();
}
