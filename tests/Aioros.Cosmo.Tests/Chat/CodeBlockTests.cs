using Aioros.Cosmo.Chat;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Styling;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Chat;

public sealed class CodeBlockTests(DesktopSession desktop)
{
    internal static readonly string[] Blocks =
    [
        "code-block-demo", "code-block-basic", "code-block-header", "code-block-line-numbers", "code-block-highlighted-lines", "code-block-diff",
        "code-block-destructive", "code-block-size", "code-block-highlighting",
        "code-block-actions", "code-block-ansi", "code-block-chat", "code-block-floating-actions", "code-block-height", "code-block-languages", "code-block-rtl",
        "code-block-show-more", "code-block-show-more-scroll", "code-block-start-line", "code-block-streaming", "code-block-tabs", "code-block-trace", "code-block-typeset", "code-block-wrap", "code-block-markdown",
    ];

    /// <summary>shiki's github-light and github-dark colors of the highlighted tokens, its terminal colors of the ANSI output among them.</summary>
    internal static readonly Dictionary<string, string[]> Shiki = new(StringComparer.Ordinal)
    {
        ["light"] = ["#24292E", "#D73A49", "#032F62", "#6F42C1", "#005CC5", "#E36209", "#6A737D", "#22863A", "#28A745", "#8024292E"],
        ["dark"] = ["#E1E4E8", "#F97583", "#9ECBFF", "#B392F0", "#79B8FF", "#FFAB70", "#6A737D", "#85E89D", "#34D058", "#EA4A5A", "#80E1E4E8"],
    };

    private static IEnumerable<string> Within(string what, Rect found, ElementMetrics expected, double width = 1.5) =>
        Math.Abs(found.X - expected.X) <= 1.5 && Math.Abs(found.Width - expected.Width) <= width && Math.Abs(found.Y - expected.Y) <= 3 && Math.Abs(found.Height - expected.Height) <= 3
            ? []
            : [$"{what}: {found}, shadcn {Of(expected)}"];

    private static IEnumerable<string> Boxes(string what, Control root, IReadOnlyList<ElementMetrics> reference, string slot, IReadOnlyList<Control> found, double width = 1.5, Func<Control, IBrush?>? paint = null)
    {
        var expected = reference.Where(element => element.Slot == slot).ToList();
        if (found.Count != expected.Count)
        {
            return [$"{what}: {found.Count} {slot}, shadcn {expected.Count}"];
        }

        return found.Zip(expected).SelectMany((pair, i) => Within($"{what} {slot} #{i} {pair.Second.Text}", Windows.Box(pair.First, root), pair.Second, width)
            .Concat(paint is null ? [] : Paints($"{what} {slot} #{i} background", paint(pair.First), pair.Second.Background)));
    }

    private static IEnumerable<string> Tokens(string what, Control root, string theme)
    {
        var colors = Shiki[theme].Select(CssColor.Parse).ToList();
        return All<CodeBlockContent>(root).SelectMany(All<TextBlock>).SelectMany(text => text.Inlines ?? []).OfType<Run>()
            .Where(run => !string.IsNullOrWhiteSpace(run.Text) && !colors.Any(color => Windows.Paint(run.Foreground).Near(color, 1)))
            .Select(run => $"{what} token {run.Text}: {Windows.Paint(run.Foreground)}");
    }

    [Fact]
    [Trait("AC", "CODEBLOCK-01")]
    public async Task EveryCodeBlockIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Blocks)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (_, root, reference, theme) =>
            {
                var what = $"{fixture} {theme}";
                var blocks = All<CodeBlock>(root);
                var errors = Boxes(what, root, reference, "code-block", [.. blocks], paint: static block => ((CodeBlock)block).Background).ToList();
                errors.AddRange(Boxes(what, root, reference, "code-block-title", [.. blocks.Where(static block => block.Title is not null).Select(static block => (Control)Windows.Part<ContentPresenter>(block, "PART_Title"))], 3));
                errors.AddRange(Boxes(what, root, reference, "code-block-actions", [.. blocks.Where(static block => block.Actions is not null).Select(static block => (Control)Windows.Part<ContentPresenter>(block, block.Title is null ? "PART_Floating" : "PART_Actions"))]));
                errors.AddRange(Boxes(what, root, reference, "code-block-content", [.. All<CodeBlockContent>(root)], 3));
                if (reference.Any(element => element.Slot == "code-block-line"))
                {
                    errors.AddRange(Boxes(what, root, reference, "code-block-line", [.. All<CodeBlockLine>(root)], 3, static line => ((CodeBlockLine)line).Background));
                }

                errors.AddRange(Tokens(what, root, theme));
                return errors;
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "CODEBLOCK-03")]
    public async Task LinesAreNumberedFromTheStartLineAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            CodeBlockLine Line(string text) => new() { Content = new TextBlock { Text = text } };
            var numbered = new CodeBlockContent { LineNumbers = true, StartLine = 7, Content = new StackPanel { Children = { Line("a"), Line("b"), Line("c") } } };
            var plain = new CodeBlockContent { Content = new StackPanel { Children = { Line("a") } } };
            var window = Windows.Show(new StackPanel { Width = 300, Children = { new CodeBlock { Content = numbered }, new CodeBlock { Content = plain } } }, ThemeVariant.Light);
            var numbers = All<CodeBlockLine>(numbered).Select(line => Windows.Part<TextBlock>(line, "PART_Number")).ToList();
            var first = All<CodeBlockLine>(numbered)[0];
            var gap = Windows.Box(Windows.Part<ContentPresenter>(first, "PART_ContentPresenter"), window).X - Windows.Box(numbers[0], window).Right;
            var result = (
                Texts: numbers.Where(static number => number.IsEffectivelyVisible).Select(static number => number.Text).ToList(),
                Widths: numbers.Select(number => number.Bounds.Width).Distinct().ToList(),
                Right: numbers.All(static number => number.TextAlignment == TextAlignment.Right),
                Muted: numbers.SelectMany(number => Paints("number", number.Foreground, window, "CosmoMutedForegroundBrush")).ToList(),
                Gap: gap,
                Plain: All<CodeBlockLine>(plain).Select(line => Windows.Part<TextBlock>(line, "PART_Number").IsEffectivelyVisible).ToList());
            window.Close();
            return result;
        });

        Assert.Equal(["7", "8", "9"], found.Texts);
        Assert.Equal([24.0], found.Widths);
        Assert.True(found.Right);
        Assert.Empty(found.Muted);
        Assert.Equal(12, found.Gap, 1);
        Assert.Equal([false], found.Plain);
    }

    [Fact]
    [Trait("AC", "CODEBLOCK-04")]
    public async Task TheCopyButtonCopiesAndShowsItsCheckForTwoSecondsAsync()
    {
        var time = new ManualTime();
        var written = new List<string>();
        var button = await desktop.RunAsync(() =>
        {
            var copy = new CodeBlockCopyButton { Value = "pnpm add ai", TimeProvider = time, Write = text =>
            {
                written.Add(text);
                return Task.CompletedTask;
            },
            };
            var window = Windows.Show(new StackPanel { Width = 100, Children = { copy } }, ThemeVariant.Light);
            Windows.Click(window, copy);
            return (Window: window, Copy: copy);
        });
        var clipboard = string.Join("|", written);
        var copied = await desktop.RunAsync(() =>
        {
            Windows.Settle();
            return (Check: Windows.Part<Avalonia.Controls.Shapes.Path>(button.Copy, "PART_Check").Opacity == 1, Icon: Windows.Part<Avalonia.Controls.Shapes.Path>(button.Copy, "PART_Copy").Opacity == 1);
        });
        var later = await desktop.RunAsync(() =>
        {
            time.Advance(TimeSpan.FromSeconds(2));
            Windows.Settle();
            var result = (Check: Windows.Part<Avalonia.Controls.Shapes.Path>(button.Copy, "PART_Check").Opacity == 1, Icon: Windows.Part<Avalonia.Controls.Shapes.Path>(button.Copy, "PART_Copy").Opacity == 1);
            button.Window.Close();
            return result;
        });

        Assert.Equal("pnpm add ai", clipboard);
        Assert.Equal((true, false), copied);
        Assert.Equal((false, true), later);
    }
    private static Window Shown(string fixture)
    {
        var window = DisplayFixtures.Show(fixture, ThemeVariant.Light);
        window.Height = 1400;
        Windows.Settle();
        return window;
    }

    private static List<string> Floating()
    {
        var window = Shown("code-block-floating-actions");
        var block = All<CodeBlock>((Control)window.Content!).Single();
        var floating = Windows.Part<ContentPresenter>(block, "PART_Floating");
        var box = Windows.Box(floating, block);
        var errors = new List<string>();
        errors.AddRange(Math.Abs(box.Right - (block.Bounds.Width - 12)) <= 0.5 && Math.Abs(box.Y - 12) <= 0.5 ? [] : [$"floating at {box} in {block.Bounds.Size}"]);
        errors.AddRange(floating.Opacity == 0 ? [] : [$"floating at rest {floating.Opacity}"]);
        Windows.Hover(window, All<CodeBlockContent>(block).Single());
        errors.AddRange(floating.Opacity == 1 ? [] : [$"floating under the pointer {floating.Opacity}"]);
        Windows.Away(window, new Point(1, 1));
        All<CodeBlockCopyButton>(block).Single().Focus(Avalonia.Input.NavigationMethod.Tab);
        Windows.Settle();
        errors.AddRange(floating.Opacity == 1 ? [] : [$"floating with the focus in {floating.Opacity}"]);
        window.Close();
        return errors;
    }

    private static List<string> ShowMore(string fixture, double open)
    {
        var window = Shown(fixture);
        var root = (Control)window.Content!;
        var content = All<CodeBlockContent>(root).Single();
        var trigger = All<Aioros.Cosmo.Display.CollapsibleTrigger>(root).Single();
        string Label() => string.Join(',', All<TextBlock>(trigger).Where(static text => text.IsEffectivelyVisible).Select(static text => text.Text));
        var errors = new List<string>();
        var viewport = Windows.Part<ScrollViewer>(content, "PART_Viewport");
        errors.AddRange(content.Bounds.Height == 256 && Label() == "Show more" && viewport.OpacityMask is LinearGradientBrush ? [] : [$"{fixture} closed: {content.Bounds.Height} {Label()} {viewport.OpacityMask}"]);
        Windows.Click(window, trigger);
        errors.AddRange(Math.Abs(content.Bounds.Height - open) <= 1 && Label() == "Show less" ? [] : [$"{fixture} open: {content.Bounds.Height} {Label()}"]);
        window.Close();
        return errors;
    }

    private static List<string> WrapToggle()
    {
        var window = Shown("code-block-actions");
        var root = (Control)window.Content!;
        var content = All<CodeBlockContent>(root).Single();
        var toggle = All<Aioros.Cosmo.Display.CollapsibleTrigger>(root).Single();
        var states = new List<string> { $"{content.Wrap} {Windows.Paint(toggle.Foreground)}" };
        foreach (var _ in Enumerable.Range(0, 2))
        {
            Windows.Click(window, toggle);
            Windows.Away(window, new Point(1, 1));
            states.Add($"{content.Wrap} {Windows.Paint(toggle.Foreground)}");
        }
        var foreground = Windows.Paint((IBrush)window.FindResource(ThemeVariant.Light, "CosmoForegroundBrush")!);
        var muted = Windows.Paint((IBrush)window.FindResource(ThemeVariant.Light, "CosmoMutedForegroundBrush")!);
        window.Close();
        string[] expected = [$"True {foreground}", $"False {muted}", $"True {foreground}"];
        return states.SequenceEqual(expected) ? [] : [$"wrap toggle {string.Join(" | ", states)}, expected {string.Join(" | ", expected)}"];
    }

    private static List<string> Numbers()
    {
        var window = Shown("code-block-start-line");
        var numbers = All<CodeBlockLine>((Control)window.Content!).Select(line => Windows.Part<TextBlock>(line, "PART_Number")).ToList();
        var widest = new Avalonia.Media.TextFormatting.TextLayout("1005", new Typeface(numbers[0].FontFamily), numbers[0].FontSize, null).WidthIncludingTrailingWhitespace;
        var errors = numbers.Select(number => number.Bounds.Width).Distinct().ToList() is [var width] && Math.Abs(width - widest) <= 0.5 && numbers[0].Text == "995" && numbers[^1].Text == "1005"
            ? new List<string>()
            : [$"numbers {string.Join(',', numbers.Select(static number => $"{number.Text}:{number.Bounds.Width}"))}, widest {widest}"];
        window.Close();
        return errors;
    }

    private static List<string> RightToLeft()
    {
        var window = Shown("code-block-rtl");
        var root = (Control)window.Content!;
        var block = All<CodeBlock>(root).Single();
        var frame = Windows.Box(block, root);
        var title = Windows.Box(Windows.Part<ContentPresenter>(block, "PART_Title"), root);
        var content = All<CodeBlockContent>(block).Single();
        var line = Windows.Box(Windows.Part<ContentPresenter>(All<CodeBlockLine>(content)[0], "PART_ContentPresenter"), root);
        var icon = All<Avalonia.Controls.Shapes.Path>(Windows.Part<ContentPresenter>(block, "PART_Title")).Single();
        var errors = new List<string>();
        errors.AddRange(Math.Abs(title.Right - (frame.Right - 16)) <= 1 ? [] : [$"rtl title {title} in {frame}"]);
        errors.AddRange(content.FlowDirection == FlowDirection.LeftToRight && line.X < frame.Center.X ? [] : [$"rtl code {content.FlowDirection} {line} in {frame}"]);
        errors.AddRange(icon.FlowDirection == FlowDirection.LeftToRight ? [] : [$"rtl icon {icon.FlowDirection}"]);
        window.Close();
        return errors;
    }

    [Fact]
    [Trait("AC", "CODEBLOCK-06")]
    public async Task TheDocsExamplesBehaveAsShadcnsAsync()
    {
        var wrong = await desktop.RunAsync(() => Floating()
            .Concat(ShowMore("code-block-show-more", 22.75 * 34 + 16))
            .Concat(ShowMore("code-block-show-more-scroll", 384))
            .Concat(WrapToggle())
            .Concat(Numbers())
            .Concat(RightToLeft())
            .ToList());

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
    [Fact]
    [Trait("AC", "CODEBLOCK-10")]
    public async Task AStreamingFenceTakesTheTokensOfTheCodeItBeginsAsync()
    {
        const string First = "export function useDebounce<T>(value: T, delay = 300) {";
        var found = await desktop.RunAsync(() =>
        {
            var tokens = new ShikiTokens { Source = new Uri("avares://Aioros.Cosmo.Tests/Assets/highlighted-markdown.json") };
            var whole = tokens.Highlight(First, "tsx") is { } line ? string.Concat(line[0].Select(static token => token.Text)) : null;
            var cut = tokens.Highlight(First + "\n  const [deb", "tsx")?.Select(static line => string.Join('|', line.Select(static token => token.Text))).ToList();
            return (whole, cut, tokens.Highlight(First, "ts") is null, tokens.Highlight("import x", "tsx") is null);
        });

        Assert.Equal(First, found.whole);
        Assert.Equal(2, found.cut?.Count);
        Assert.Equal(First, found.cut![0].Replace("|", string.Empty, StringComparison.Ordinal));
        Assert.Equal("  |const| [|deb", found.cut[1]);
        Assert.True(found.Item3);
        Assert.True(found.Item4);
    }

    [Fact]
    [Trait("AC", "CODEBLOCK-08")]
    public async Task MarkdownIsWrittenAsCosmosControlsAsync()
    {
        const string Text = "# Title\n\nSome **bold** and *slanted* with `code` and [a link](https://aioros.dev).\n\n- one\n- two\n\n> quoted\n\n```tsx\nconst a = 1\n```";
        var found = await desktop.RunAsync(() =>
        {
            var markdown = new Markdown { Text = Text };
            var window = Windows.Show(new StackPanel { Width = 448, Children = { markdown } }, ThemeVariant.Light);
            var blocks = ((StackPanel)markdown.Child!).Children.ToList();
            var heading = (TextBlock)blocks[0];
            var paragraph = (TextBlock)blocks[1];
            var inlines = paragraph.Inlines!.ToList();
            var code = inlines.OfType<InlineUIContainer>().Select(static container => (Border)container.Child).Single();
            var link = inlines.OfType<Run>().Single(static run => run.Text == "a link");
            var list = (StackPanel)blocks[2];
            var quote = (Border)blocks[3];
            var fenced = (CodeBlock)blocks[4];
            var content = (CodeBlockContent)fenced.Content!;
            var result = new List<string>
            {
                $"{((StackPanel)markdown.Child).Spacing}",
                $"{heading.FontSize} {heading.FontWeight}",
                string.Join('|', inlines.OfType<Run>().Select(static run => $"{run.Text}:{run.FontWeight}:{run.FontStyle}")),
                $"{((TextBlock)code.Child!).Text} {((TextBlock)code.Child).FontSize} {code.CornerRadius.TopLeft} {code.Padding}",
                $"{link.TextDecorations?.Count} {Aioros.Markup.Web.GetHref(link)}",
                string.Join('|', list.Children.OfType<DockPanel>().Select(static row => ((TextBlock)row.Children[0]).Text)),
                $"{quote.BorderThickness}",
                $"{fenced.Title} {((CodeBlockCopyButton)fenced.Actions!).Value} {content.LineNumbers} {All<CodeBlockLine>(content).Count}",
            };
            result.AddRange(Paints("code", code.Background, window, "CosmoMutedBrush"));
            window.Close();
            return result;
        });

        Assert.Equal([
            "16", "30 DemiBold",
            "Some :Normal:Normal|bold:DemiBold:Normal| and :Normal:Normal|slanted:Normal:Italic| with :Normal:Normal| and :Normal:Normal|a link:Normal:Normal|.:Normal:Normal",
            "code 12.6 6 4,2,4,2", "1 https://aioros.dev", "•|•", "4,0,0,0", "tsx const a = 1 True 1",
        ], found);

        var fixture = await FixtureAsync(desktop, "code-block-markdown", (_, root, _, theme) =>
        {
            var content = All<CodeBlockContent>(root).Single();
            var colored = All<TextBlock>(content).SelectMany(text => text.Inlines ?? []).OfType<Run>().Count(static run => run.IsSet(TextElement.ForegroundProperty));
            return colored > 10 && !content.LineNumbers ? Tokens($"code-block-markdown {theme}", root, theme) : [$"code-block-markdown {theme}: {colored} highlighted tokens, numbered {content.LineNumbers}"];
        });
        Assert.True(fixture.Count == 0, string.Join(Environment.NewLine, fixture));
    }

    [Fact]
    [Trait("AC", "CODEBLOCK-13")]
    public async Task AFenceJustOpenedIsACodeBlockWithAnEmptyLineAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var markdown = new Markdown { Text = "Wrap the value in a hook.\n\n```tsx\n" };
            var blocks = ((StackPanel)markdown.Child!).Children.ToList();
            return (blocks.Count, blocks[0] is TextBlock, blocks[^1] is CodeBlock { Content: CodeBlockContent { Content: StackPanel { Children: [CodeBlockLine { Content: TextBlock line }] } } } && string.Concat(line.Inlines!.OfType<Run>().Select(static run => run.Text)).Trim().Length == 0);
        });

        Assert.Equal((2, true, true), found);
    }
}
