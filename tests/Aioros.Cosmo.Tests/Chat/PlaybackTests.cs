using Aioros.Cosmo.Chat;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Gallery.Models;
using Microsoft.Extensions.Time.Testing;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Chat;

public sealed class PlaybackTests(DesktopSession desktop)
{
    private static string TextOf(TextBlock text) =>
        text.Inlines is { Count: > 0 } inlines ? string.Concat(inlines.OfType<Run>().Select(static run => run.Text)) : text.Text ?? string.Empty;

    private static List<string> Shown(Control root) =>
        [.. All<TextBlock>(root).Where(static text => text.IsEffectivelyVisible).Select(static text => TextOf(text).Trim()).Where(static text => text.Length > 0)];

    private static bool Shows(Control root, string text) => Shown(root).Contains(text);

    private static T Model<T>(Control root) => root.GetSelfAndVisualDescendants().OfType<Control>().Select(static control => control.DataContext).OfType<T>().First();

    private static Button Labelled(Control root, string label) =>
        All<Button>(root).Single(button => button is not CodeBlockCopyButton && (button.Content as string == label || All<TextBlock>(button).Any(text => TextOf(text) == label)));

    private static Button Action(Control root) => All<Button>(root).Single(static button => button is not CodeBlockCopyButton && button.Command is not null);

    /// <summary>Moves the clock on a millisecond at a time, since each step of a stream sets the next one's timer from the time it ran.</summary>
    private static void Wait(FakeTimeProvider clock, int milliseconds)
    {
        for (var elapsed = 0; elapsed < milliseconds; elapsed++)
        {
            clock.Advance(TimeSpan.FromMilliseconds(1));
        }

        Windows.Settle();
    }

    private static (Window Window, Control Root, FakeTimeProvider Clock) Show(string fixture, Func<Control, Playback> playback)
    {
        var window = DisplayFixtures.Show(fixture, ThemeVariant.Light);
        window.Height = 1400;
        Windows.Settle();
        var root = (Control)window.Content!;
        var clock = new FakeTimeProvider();
        playback(root).Clock = clock;
        return (window, root, clock);
    }

    private static List<string> Blocks(Control root) =>
        [.. All<CodeBlock>(root).Where(static block => block.IsEffectivelyVisible)
            .Select(static block => string.Join('\n', All<CodeBlockLine>(block).Where(static line => line.IsEffectivelyVisible).Select(static line => string.Concat(All<TextBlock>(line).Where(static text => text.Name is not ("PART_Number" or "PART_Marker")).Select(TextOf)).TrimEnd())))];

    private static string Words(string text, int count) => string.Concat(ChatReplay.Words(text).Take(count));

    [Fact]
    [Trait("AC", "CODEBLOCK-11")]
    public async Task TheCodeBlocksStreamAsShadcnsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var errors = new List<string>();
            errors.AddRange(Streaming());
            errors.AddRange(StreamedMarkdown());
            return errors;
        });

        Assert.True(found.Count == 0, string.Join('\n', found));
    }

    /// <summary>
    /// What the streaming blocks are expected to show and enable at a step, three characters a step, from shadcn/ui's offsets
    /// (samples/Gallery/tools/codeblocks.py): a block once the stream passes its start, a line once it reaches it, each token
    /// cut where the stream is (<c>code.slice(0, length - offset)</c>), a copy button once its block's code is whole.
    /// </summary>
    private static (List<string> Blocks, List<bool> Copies) Expected(int step)
    {
        var length = step * 3;
        var lines = CodeBlockStreamingScript.LineStarts;
        var tokens = CodeBlockStreamingScript.TokenStarts.Select((start, index) => (start, text: CodeBlockStreamingScript.TokenTexts[index], line: Array.FindLastIndex(lines, line => line <= start))).ToList();
        string Line(int line) => string.Concat(tokens.Where(token => token.line == line).Select(token => token.text[..Math.Clamp(length - token.start, 0, token.text.Length)])).TrimEnd();
        return ([.. CodeBlockStreamingScript.BlockStarts.Select((start, block) => (start, block)).Where(block => length > block.start)
                .Select(block => string.Join('\n', Enumerable.Range(0, lines.Length).Where(line => CodeBlockStreamingScript.LineBlocks[line] == block.block && length >= lines[line]).Select(Line)))],
            [.. CodeBlockStreamingScript.BlockEnds.Select(end => length >= end)]);
    }

    private static List<string> Streaming()
    {
        var errors = new List<string>();
        var (window, root, clock) = Show("code-block-streaming", static root => Model<CodeBlockStreaming>(root).Playback);
        var replay = Labelled(root, "Replay");
        var copies = All<CodeBlock>(root).Select(static block => All<CodeBlockCopyButton>(block).SingleOrDefault()).ToList();
        errors.AddRange(Blocks(root).SequenceEqual(Expected(288).Blocks) && replay.IsEnabled ? [] : [$"at rest: {replay.IsEnabled} [{string.Join(" | ", Blocks(root))}]"]);
        Windows.Click(window, replay);
        errors.AddRange(!replay.IsEnabled && Blocks(root) is [] ? [] : [$"replayed: {replay.IsEnabled} [{string.Join(" | ", Blocks(root))}]"]);
        for (var step = 1; step <= 288 && errors.Count == 0; step++)
        {
            Wait(clock, 16);
            var (blocks, enabled) = Expected(step);
            var copied = copies.Select((copy, index) => copy?.IsEnabled ?? enabled[index]).ToList();
            errors.AddRange(Blocks(root).SequenceEqual(blocks) && copied.SequenceEqual(enabled) && replay.IsEnabled == (step == 288)
                ? [] : [$"step {step}: {replay.IsEnabled} {string.Join(',', copied)} [{string.Join(" | ", Blocks(root))}] expected [{string.Join(" | ", blocks)}]"]);
        }

        errors.AddRange(Expected(1).Blocks is ["exp"] && Expected(96) is { Blocks.Count: 2, Copies: [true, false, ..] } && Expected(95) is { Blocks.Count: 1, Copies: [false, ..] }
            ? [] : ["the stream's steps are not shadcn/ui's"]);
        window.Close();
        return errors;
    }

    private static List<string> StreamedMarkdown()
    {
        const string Fence = "export function useDebounce<T>(value: T, delay = 300) {\n  const";
        var errors = new List<string>();
        var (window, root, clock) = Show("code-block-markdown", static root => Model<CodeBlockMarkdownExample>(root).Chat.Playback);
        var markdown = All<Markdown>(root).Single();
        var replay = Labelled(root, "Replay");
        errors.AddRange(markdown.Text == MarkdownChat.Response ? [] : ["markdown at rest"]);
        Windows.Click(window, replay);
        errors.AddRange(!replay.IsEffectivelyVisible && Shows(root, "How do I debounce a search input in React?") && markdown.Text == string.Empty ? [] : [$"markdown replayed: {replay.IsEffectivelyVisible} '{markdown.Text}'"]);
        var words = ChatReplay.Words(MarkdownChat.Response).Count();
        var fenced = 0;
        for (var word = 1; word <= words && errors.Count == 0; word++)
        {
            Wait(clock, 30);
            var text = Words(MarkdownChat.Response, word);
            var inFence = text.Contains("```tsx\n" + Fence, StringComparison.Ordinal) && !text.Contains("\n```\n", StringComparison.Ordinal);
            fenced += inFence ? 1 : 0;
            errors.AddRange(markdown.Text == text && replay.IsEffectivelyVisible == (word == words) && (!inFence || Blocks(root) is [var code] && code.StartsWith(Fence, StringComparison.Ordinal))
                ? [] : [$"word {word}: '{markdown.Text}' {All<Markdown>(root).Count} '{Model<CodeBlockMarkdownExample>(root).Chat.Answer}' {replay.IsEffectivelyVisible} [{string.Join(" | ", Blocks(root))}]"]);
        }

        errors.AddRange(fenced > 0 ? [] : ["no step inside the fence"]);
        window.Close();
        return errors;
    }

    [Fact]
    [Trait("AC", "TRACE-08")]
    public async Task TheTracesStreamAsShadcnsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var errors = new List<string>();
            errors.AddRange(Thinking());
            errors.AddRange(ToolCalls());
            errors.AddRange(Demo());
            return errors;
        });

        Assert.True(found.Count == 0, string.Join('\n', found));
    }

    private static List<string> Thinking()
    {
        var errors = new List<string>();
        var (window, root, clock) = Show("trace-thinking", static root => Model<TraceThinkingExample>(root).Chat.Playback);
        var button = Action(root);
        errors.AddRange(Shown(root) is ["Send message"] ? [] : [$"thinking at rest: {string.Join(" | ", Shown(root))}"]);
        Windows.Click(window, button);
        var expander = All<Expander>(root).Single();
        var reasoning = (TextBlock)expander.Content!;
        errors.AddRange(!button.IsEffectivelyVisible && Shows(root, "Why is the checkout deploy so slow?") && Shows(root, "Thinking") && !expander.IsEffectivelyVisible
            ? [] : [$"thinking asked: {string.Join(" | ", Shown(root))}"]);
        var words = ChatReplay.Words(ThinkingChat.Reasoning).Count();
        for (var word = 1; word <= words; word++)
        {
            Wait(clock, 60);
            var running = word < words;
            if (TextOf(reasoning) != Words(ThinkingChat.Reasoning, word) || !expander.IsEffectivelyVisible || expander.IsExpanded != running || Shows(root, "Thinking") != running || Shows(root, "Thought") == running)
            {
                errors.Add($"reasoning word {word}: {expander.IsExpanded} {string.Join(" | ", Shown(root))}");
                break;
            }
        }

        Wait(clock, 400);
        errors.AddRange(Shown(root).Any(static text => text.StartsWith("Image", StringComparison.Ordinal)) ? ["the answer before 420 ms"] : []);
        var answer = ChatReplay.Words(ThinkingChat.Answer).Count();
        for (var word = 1; word <= answer; word++)
        {
            Wait(clock, 20);
            if (!Shows(root, Words(ThinkingChat.Answer, word).TrimEnd()))
            {
                errors.Add($"answer word {word}: {string.Join(" | ", Shown(root))}");
                break;
            }
        }

        errors.AddRange(button.IsEffectivelyVisible && Shows(root, "Reset") ? [] : ["thinking ended without Reset"]);
        Windows.Click(window, button);
        errors.AddRange(Shown(root) is ["Send message"] ? [] : [$"thinking reset: {string.Join(" | ", Shown(root))}"]);
        window.Close();
        return errors;
    }

    private static List<string> ToolCalls()
    {
        var errors = new List<string>();
        var (window, root, clock) = Show("trace-tool-calls", static root => Model<TraceToolCallsExample>(root).Chat.Playback);
        var button = Action(root);
        Windows.Click(window, button);
        (string Running, string Done, string Output)[] steps =
            [("Running node --version", "Ran node --version", "v24.18.0"), ("Running npm --version", "Ran npm --version", "11.6.2"), ("Running pnpm --version", "Failed to run pnpm --version", "command not found: pnpm")];
        foreach (var (step, index) in steps.Select(static (step, index) => (step, index)))
        {
            errors.AddRange(ToolStep(root, clock, step, index == steps.Length - 1 ? TraceStatus.Error : TraceStatus.Done));
        }

        Wait(clock, 20 * ChatReplay.Words(ToolCallsChat.Answer).Count());
        errors.AddRange(Shows(root, ToolCallsChat.Answer) && button.IsEffectivelyVisible && Shows(root, "Reset") ? [] : [$"tool calls ended: {string.Join(" | ", Shown(root))}"]);
        Windows.Click(window, button);
        errors.AddRange(Shown(root) is ["Send message"] ? [] : [$"tool calls reset: {string.Join(" | ", Shown(root))}"]);
        window.Close();
        return errors;
    }

    /// <summary>One command of trace-tool-calls: running for 900 ms, then its collapsible line holding its output.</summary>
    private static List<string> ToolStep(Control root, FakeTimeProvider clock, (string Running, string Done, string Output) step, TraceStatus ended)
    {
        var errors = new List<string>();
        var line = All<TraceLine>(root).Single(line => line.IsEffectivelyVisible && line.Content as string == step.Running);
        errors.AddRange(line.Status == TraceStatus.Running && !Shows(root, step.Done) ? [] : [$"{step.Running}: {line.Status} {string.Join(" | ", Shown(root))}"]);
        Wait(clock, 899);
        errors.AddRange(Shows(root, step.Running) ? [] : [$"{step.Running} ended before 900 ms"]);
        Wait(clock, 1);
        var done = All<TraceLine>(root).Single(line => line.IsEffectivelyVisible && line.Content as string == step.Done);
        var expander = done.GetVisualAncestors().OfType<Expander>().First();
        var output = string.Join('\n', expander.GetLogicalDescendants().OfType<CodeBlockLine>().Select(static code => string.Concat(code.GetLogicalDescendants().OfType<TextBlock>().Select(TextOf)).Trim()));
        errors.AddRange(!Shows(root, step.Running) && done.Status == ended && output == step.Output ? [] : [$"{step.Done}: {done.Status} '{output}'"]);
        return errors;
    }

    private static List<string> Demo()
    {
        var errors = new List<string>();
        var (window, root, clock) = Show("trace-demo", static root => Model<TraceDemoExample>(root).Chat.Playback);
        var served = Shown(root);
        var button = Action(root);
        Windows.Click(window, button);
        errors.AddRange(!button.IsEffectivelyVisible && Shows(root, "Thinking") && !Shows(root, DemoChat.Answer) ? [] : [$"demo replayed: {string.Join(" | ", Shown(root))}"]);
        var words = ChatReplay.Words(DemoChat.Reasoning).Count();
        Wait(clock, 40 * (words - 1));
        errors.AddRange(Shows(root, "Thinking") && Shows(root, Words(DemoChat.Reasoning, words - 1).TrimEnd()) ? [] : [$"demo reasoning: {string.Join(" | ", Shown(root))}"]);
        Wait(clock, 40);
        errors.AddRange(Shows(root, "Thought for 5s") && Shows(root, "Running pnpm --version") ? [] : [$"demo thought: {string.Join(" | ", Shown(root))}"]);
        foreach (var (command, wait) in new[] { ("pnpm --version", 700), ("git diff --stat HEAD~1 -- package.json pnpm-lock.yaml", 900), ("pnpm install --frozen-lockfile", 1200) })
        {
            errors.AddRange(Shows(root, "Running " + command) ? [] : [$"demo not running {command}"]);
            Wait(clock, wait);
            errors.AddRange(Shows(root, "Running " + command) ? [$"demo still running {command} after {wait} ms"] : []);
        }

        var sources = All<TraceLine>(root).SingleOrDefault(static line => line.IsEffectivelyVisible && line.Content as string == "Using 2 sources");
        errors.AddRange(sources?.Status == TraceStatus.Running ? [] : [$"demo sources not running: {string.Join(" | ", All<TraceLine>(root).Select(static line => $"{line.Content}:{line.IsEffectivelyVisible}:{line.Status}"))}"]);
        Wait(clock, 1019);
        errors.AddRange(Shows(root, "Using 2 sources") ? [] : ["demo sources ended before the answer"]);
        Wait(clock, 1);
        errors.AddRange(Shows(root, "Used 2 sources") && Shows(root, Words(DemoChat.Answer, 1).TrimEnd()) ? [] : [$"demo answer: {string.Join(" | ", Shown(root))}"]);
        Wait(clock, 20 * ChatReplay.Words(DemoChat.Answer).Count());
        errors.AddRange(Shown(root).SequenceEqual(served) && button.IsEffectivelyVisible ? [] : [$"demo ended: {string.Join(" | ", Shown(root))}"]);
        window.Close();
        return errors;
    }
}
