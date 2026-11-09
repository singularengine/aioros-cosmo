using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Block = Markdig.Syntax.Block;
using Indented = Markdig.Syntax.CodeBlock;
using Inline = Markdig.Syntax.Inlines.Inline;
using Parser = Markdig.Markdown;

namespace Aioros.Cosmo.Chat;

/// <summary>
/// shadcn/ui's Markdown (streamdown, as the docs' chat examples render an answer): its <see cref="Text"/> parsed as
/// CommonMark (Markdig) into Cosmo's controls, 16 px apart: paragraphs, headings, lists, quotes and rules as texts,
/// inline code as a small muted box, links as links, and each fenced block as a <see cref="CodeBlock"/> titled with its
/// language, with a copy button and its lines numbered (<see cref="LineNumbers"/>), highlighted by the <see cref="ICodeHighlighter"/> found under the
/// <c>CosmoCodeHighlighter</c> resource key (plain without one). The controls are built as the text is set, so the web
/// writes what the desktop shows, and laid out unrounded (the <c>markdown-body</c> class), so 22.75 px lines do not drift.
/// </summary>
public sealed class Markdown : Decorator
{
    /// <summary>Defines the <see cref="Text"/> property.</summary>
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<Markdown, string?>(nameof(Text));

    /// <summary>Defines the <see cref="LineNumbers"/> property.</summary>
    public static readonly StyledProperty<bool> LineNumbersProperty = AvaloniaProperty.Register<Markdown, bool>(nameof(LineNumbers), true);

    private static readonly double[] HeadingSizes = [30, 24, 20, 18, 16, 14];

    /// <summary>The markdown.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Whether the fenced blocks number their lines, as shadcn/ui's <c>components/markdown.tsx</c> does (the default).</summary>
    public bool LineNumbers
    {
        get => GetValue(LineNumbersProperty);
        set => SetValue(LineNumbersProperty, value);
    }

    /// <summary>Builds the controls again as the text changes.</summary>
    /// <param name="change">The changed property.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty || change.Property == LineNumbersProperty)
        {
            Child = Build(Text ?? string.Empty);
        }
    }

    /// <summary>Builds the controls again once the markdown can find the app's highlighter.</summary>
    /// <param name="e">The event.</param>
    protected override void OnAttachedToLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Child = Build(Text ?? string.Empty);
    }

    private StackPanel Build(string text)
    {
        var panel = new StackPanel { Spacing = 16, Classes = { "markdown-body" } };
        foreach (var block in Parser.Parse(text))
        {
            panel.Children.Add(Block(block));
        }

        return panel;
    }

    private Control Block(Block block) => block switch
    {
        HeadingBlock heading => Heading(heading),
        ParagraphBlock paragraph => Paragraph(paragraph.Inline),
        FencedCodeBlock fenced => Code(Lines(fenced), fenced.Info ?? string.Empty),
        Indented code => Code(Lines(code), string.Empty),
        ListBlock list => List(list),
        QuoteBlock quote => Quote(quote),
        ThematicBreakBlock => new Separator { Classes = { "markdown" } },
        ContainerBlock container => Stack(container),
        _ => new TextBlock(),
    };

    private StackPanel Stack(ContainerBlock container)
    {
        var panel = new StackPanel { Spacing = 16 };
        foreach (var child in container)
        {
            panel.Children.Add(Block(child));
        }

        return panel;
    }

    private static TextBlock Heading(HeadingBlock heading)
    {
        var text = Paragraph(heading.Inline);
        text.FontSize = HeadingSizes[Math.Clamp(heading.Level, 1, 6) - 1];
        text.FontWeight = FontWeight.SemiBold;
        text.Classes.Add("markdown-heading");
        return text;
    }

    private static TextBlock Paragraph(ContainerInline? inlines)
    {
        var text = new TextBlock { TextWrapping = TextWrapping.Wrap, Inlines = [] };
        foreach (var inline in inlines ?? Enumerable.Empty<Inline>())
        {
            Add(text.Inlines, inline, FontWeight.Normal, FontStyle.Normal, null);
        }

        return text;
    }

    private static void Add(InlineCollection target, Inline inline, FontWeight weight, FontStyle style, string? link)
    {
        switch (inline)
        {
            case LiteralInline literal:
                target.Add(Literal(literal.Content.ToString(), weight, style, link));
                break;
            case CodeInline code:
                target.Add(new InlineUIContainer(new Border { Classes = { "markdown-code" }, Child = new TextBlock { Text = code.Content } }));
                break;
            case EmphasisInline emphasis:
                AddAll(target, emphasis, emphasis.DelimiterCount >= 2 ? FontWeight.SemiBold : weight, emphasis.DelimiterCount == 1 ? FontStyle.Italic : style, link);
                break;
            case LinkInline { IsImage: false } anchor:
                AddAll(target, anchor, weight, style, anchor.Url);
                break;
            case LineBreakInline breaking:
                target.Add(breaking.IsHard ? new LineBreak() : new Run(" "));
                break;
            case ContainerInline container:
                AddAll(target, container, weight, style, link);
                break;
        }
    }

    private static void AddAll(InlineCollection target, ContainerInline container, FontWeight weight, FontStyle style, string? link)
    {
        foreach (var child in container)
        {
            Add(target, child, weight, style, link);
        }
    }

    private static Run Literal(string text, FontWeight weight, FontStyle style, string? link)
    {
        var run = new Run(text) { FontWeight = weight, FontStyle = style };
        if (link is not null)
        {
            run.TextDecorations = TextDecorations.Underline;
            run.SetValue(Web.HrefProperty, link);
        }

        return run;
    }

    private StackPanel List(ListBlock list)
    {
        var panel = new StackPanel { Classes = { "markdown-list" } };
        var number = int.TryParse(list.OrderedStart, out var start) ? start : 1;
        foreach (var item in list.OfType<ListItemBlock>())
        {
            var marker = list.IsOrdered ? $"{number++}." : "•";
            var row = new DockPanel();
            row.Children.Add(new TextBlock { Text = marker, Margin = new Thickness(0, 0, 8, 0), [DockPanel.DockProperty] = Dock.Left });
            row.Children.Add(Stack(item));
            panel.Children.Add(row);
        }

        return panel;
    }

    private Border Quote(QuoteBlock quote) => new() { Classes = { "markdown-quote" }, Child = Stack(quote) };

    /// <summary>A code block's lines, none for a fence just opened, whose lines Markdig has not allocated.</summary>
    private static string Lines(LeafBlock block) =>
        block.Lines.Count == 0 ? string.Empty : string.Join('\n', block.Lines.Lines.Take(block.Lines.Count).Select(static line => line.ToString()));

    private CodeBlock Code(string code, string language)
    {
        var lines = new StackPanel();
        var highlighted = this.TryFindResource("CosmoCodeHighlighter", out var found) && found is ICodeHighlighter highlighter ? highlighter.Highlight(code, language) : null;
        var tokens = highlighted ?? [.. code.Split('\n').Select(static line => (IReadOnlyList<CodeToken>)[new CodeToken(line, null)])];
        foreach (var line in tokens)
        {
            lines.Children.Add(new CodeBlockLine { Content = Line(line) });
        }

        return new CodeBlock
        {
            Title = string.IsNullOrEmpty(language) ? "text" : language,
            Actions = new CodeBlockCopyButton { Value = code },
            Content = new CodeBlockContent { LineNumbers = LineNumbers, Content = lines },
        };
    }

    /// <summary>A line of code: its tokens as runs on their code brushes, a space for an empty line so it keeps its height.</summary>
    private static TextBlock Line(IReadOnlyList<CodeToken> tokens)
    {
        var text = new TextBlock { Inlines = [] };
        foreach (var token in tokens.Where(static token => token.Text.Length > 0))
        {
            var run = new Run(token.Text);
            if (token.Brush is { } brush)
            {
                run.Bind(TextElement.ForegroundProperty, text.GetResourceObservable(brush));
            }

            text.Inlines.Add(run);
        }

        if (text.Inlines.Count == 0)
        {
            text.Inlines.Add(new Run(" "));
        }

        return text;
    }
}
