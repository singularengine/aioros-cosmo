using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>The <c>code-block-markdown</c> page's model: its own turn, each change raised again: on the web the live connection's, which the page's posts act on, so each visitor plays their own.</summary>
public sealed partial class CodeBlockMarkdownExample : ObservableObject, IDisposable
{
    private readonly MarkdownChat turn;

    public CodeBlockMarkdownExample()
        : this(new MarkdownChat())
    {
    }

    public CodeBlockMarkdownExample(MarkdownChat turn)
    {
        this.turn = turn;
        turn.Chat.PropertyChanged += Forward;
    }

    /// <summary>The turn.</summary>
    public ChatReplay Chat => turn.Chat;

    public void Dispose()
    {
        turn.Chat.PropertyChanged -= Forward;
        turn.Chat.Playback.Stop();
    }

    [RelayCommand]
    private void Replay() => turn.Replay();

    private void Forward(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Chat));
}

/// <summary>The <c>trace-thinking</c> page's model: its own turn, each change raised again: on the web the live connection's, which the page's posts act on, so each visitor plays their own.</summary>
public sealed partial class TraceThinkingExample : ObservableObject, IDisposable
{
    private readonly ThinkingChat turn;

    public TraceThinkingExample()
        : this(new ThinkingChat())
    {
    }

    public TraceThinkingExample(ThinkingChat turn)
    {
        this.turn = turn;
        turn.Chat.PropertyChanged += Forward;
    }

    /// <summary>The turn.</summary>
    public ChatReplay Chat => turn.Chat;

    public void Dispose()
    {
        turn.Chat.PropertyChanged -= Forward;
        turn.Chat.Playback.Stop();
    }

    [RelayCommand]
    private void Act() => turn.Act();

    private void Forward(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Chat));
}

/// <summary>The <c>trace-tool-calls</c> page's model: its own turn, each change raised again: on the web the live connection's, which the page's posts act on, so each visitor plays their own.</summary>
public sealed partial class TraceToolCallsExample : ObservableObject, IDisposable
{
    private readonly ToolCallsChat turn;

    public TraceToolCallsExample()
        : this(new ToolCallsChat())
    {
    }

    public TraceToolCallsExample(ToolCallsChat turn)
    {
        this.turn = turn;
        turn.Chat.PropertyChanged += Forward;
    }

    /// <summary>The turn.</summary>
    public ChatReplay Chat => turn.Chat;

    public void Dispose()
    {
        turn.Chat.PropertyChanged -= Forward;
        turn.Chat.Playback.Stop();
    }

    [RelayCommand]
    private void Act() => turn.Act();

    private void Forward(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Chat));
}

/// <summary>The <c>trace-demo</c> page's model: its own turn, each change raised again: on the web the live connection's, which the page's posts act on, so each visitor plays their own.</summary>
public sealed partial class TraceDemoExample : ObservableObject, IDisposable
{
    private readonly DemoChat turn;

    public TraceDemoExample()
        : this(new DemoChat())
    {
    }

    public TraceDemoExample(DemoChat turn)
    {
        this.turn = turn;
        turn.Chat.PropertyChanged += Forward;
    }

    /// <summary>The turn.</summary>
    public ChatReplay Chat => turn.Chat;

    public void Dispose()
    {
        turn.Chat.PropertyChanged -= Forward;
        turn.Chat.Playback.Stop();
    }

    [RelayCommand]
    private void Replay() => turn.Replay();

    private void Forward(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Chat));
}
