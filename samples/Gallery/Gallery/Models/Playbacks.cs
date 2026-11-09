using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Gallery.Models;

/// <summary>The Code Block page's model: its streaming and markdown examples' models, whose changes it raises again for the page's live stream.</summary>
public sealed class CodeBlockPlayback : ObservableObject
{
    public CodeBlockPlayback()
        : this(new CodeBlockStreaming(), new CodeBlockMarkdownExample())
    {
    }

    public CodeBlockPlayback(CodeBlockStreaming streaming, CodeBlockMarkdownExample markdown)
    {
        Streaming = streaming;
        Markdown = markdown;
        streaming.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Streaming));
        markdown.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Markdown));
    }

    /// <summary>The streaming example.</summary>
    public CodeBlockStreaming Streaming { get; }

    /// <summary>The markdown example.</summary>
    public CodeBlockMarkdownExample Markdown { get; }
}

/// <summary>The Trace page's model: its thinking, tool-call and demo examples' models, whose changes it raises again for the page's live stream.</summary>
public sealed class TracePlayback : ObservableObject
{
    public TracePlayback()
        : this(new TraceThinkingExample(), new TraceToolCallsExample(), new TraceDemoExample())
    {
    }

    public TracePlayback(TraceThinkingExample thinking, TraceToolCallsExample toolCalls, TraceDemoExample demo)
    {
        Thinking = thinking;
        ToolCalls = toolCalls;
        Demo = demo;
        thinking.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Thinking));
        toolCalls.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ToolCalls));
        demo.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Demo));
    }

    /// <summary>The thinking example.</summary>
    public TraceThinkingExample Thinking { get; }

    /// <summary>The tool-call example.</summary>
    public TraceToolCallsExample ToolCalls { get; }

    /// <summary>The demo.</summary>
    public TraceDemoExample Demo { get; }
}
