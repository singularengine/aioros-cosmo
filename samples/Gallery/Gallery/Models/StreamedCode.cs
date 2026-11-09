using CommunityToolkit.Mvvm.ComponentModel;
using static Gallery.Models.CodeBlockStreamingScript;

namespace Gallery.Models;

/// <summary>
/// The stream of shadcn/ui's <c>code-block-streaming</c> example: Replay streams its five code blocks again, three characters every
/// 16 ms on <see cref="Playback.Clock"/>, each block shown once the stream reaches it, a line once it starts, every token
/// cut where the stream is, a block's copy button enabled once its code is whole and Replay enabled once it all is.
/// </summary>
public sealed partial class StreamedCode : ObservableObject
{
    private const int Step = 3;
    private static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(16);

    public StreamedCode() => Show(BlockEnds[^1]);

    /// <summary>The clock the stream plays on.</summary>
    public Playback Playback { get; } = new();

    [ObservableProperty]
    public partial bool[] Blocks { get; private set; } = [];

    [ObservableProperty]
    public partial bool[] Lines { get; private set; } = [];

    [ObservableProperty]
    public partial string[] Tokens { get; private set; } = [];

    [ObservableProperty]
    public partial bool[] Copies { get; private set; } = [];

    [ObservableProperty]
    public partial bool Idle { get; private set; }

    /// <summary>Streams the blocks again from the start.</summary>
    public void Replay()
    {
        var total = BlockEnds[^1];
        Show(0);
        Playback.Play(Enumerable.Range(1, (total + Step - 1) / Step).Select(tick => (Tick, (Action)(() => Show(Math.Min(total, tick * Step))))));
    }

    private void Show(int length)
    {
        Blocks = [.. BlockStarts.Select(start => length > start)];
        Lines = [.. LineStarts.Select((start, index) => length > BlockStarts[LineBlocks[index]] && length >= start)];
        Tokens = [.. TokenTexts.Select((text, index) => text[..Math.Clamp(length - TokenStarts[index], 0, text.Length)])];
        Copies = [.. BlockEnds.Select(end => length >= end)];
        Idle = length >= BlockEnds[^1];
    }
}
