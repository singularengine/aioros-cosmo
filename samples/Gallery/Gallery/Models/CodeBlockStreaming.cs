using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>The <c>code-block-streaming</c> page's model: its own <see cref="StreamedCode"/>, each of its changes raised again: on the web the live connection's, which the page's Replay acts on, so each visitor plays their own.</summary>
public sealed partial class CodeBlockStreaming : ObservableObject, IDisposable
{
    private readonly StreamedCode stream;

    public CodeBlockStreaming()
        : this(new StreamedCode())
    {
    }

    public CodeBlockStreaming(StreamedCode stream)
    {
        this.stream = stream;
        stream.PropertyChanged += Forward;
    }

    /// <summary>The clock the stream plays on.</summary>
    public Playback Playback => stream.Playback;

    public bool[] Blocks => stream.Blocks;

    public bool[] Lines => stream.Lines;

    public string[] Tokens => stream.Tokens;

    public bool[] Copies => stream.Copies;

    public bool Idle => stream.Idle;

    public void Dispose()
    {
        stream.PropertyChanged -= Forward;
        stream.Playback.Stop();
    }

    [RelayCommand]
    private void Replay() => stream.Replay();

    private void Forward(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(e.PropertyName);
}
