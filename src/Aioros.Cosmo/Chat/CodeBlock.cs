using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Aioros.Cosmo.Chat;

/// <summary>
/// shadcn/ui's CodeBlock (the radix base in the radix-rhea style): a rounded block of code on <c>--muted</c> with its
/// <see cref="Title"/> and <see cref="Actions"/> in a header and its <see cref="Footer"/>; the content is a
/// <see cref="CodeBlockContent"/>. The <c>destructive</c> class tints it with <c>--destructive</c>, <c>sm</c> makes it
/// smaller; it is <c>:header</c> while it has a title. Actions without a title float over the code's top end
/// (<c>:floating</c>), shown while the pointer is over the block or the focus is in it, as shadcn/ui's
/// <c>CodeBlockActions</c> placed in the block itself.
/// </summary>
[PseudoClasses(":header", ":footer", ":floating")]
public sealed class CodeBlock : ContentControl
{
    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<CodeBlock, object?>(nameof(Title));

    /// <summary>Defines the <see cref="Actions"/> property.</summary>
    public static readonly StyledProperty<object?> ActionsProperty = AvaloniaProperty.Register<CodeBlock, object?>(nameof(Actions));

    /// <summary>Defines the <see cref="Footer"/> property.</summary>
    public static readonly StyledProperty<object?> FooterProperty = AvaloniaProperty.Register<CodeBlock, object?>(nameof(Footer));

    /// <summary>The header's title: a file name or a label, with an optional 16 px icon before it.</summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The header's buttons, at its end (24 px ghost buttons of the <c>code-block</c> class), or, with no title, the buttons floating over the code.</summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <summary>The row below the code.</summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>Marks the block <c>:header</c> or <c>:footer</c> as its parts are set.</summary>
    /// <param name="change">The changed property.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TitleProperty || change.Property == ActionsProperty)
        {
            PseudoClasses.Set(":header", Title is not null);
            PseudoClasses.Set(":floating", Title is null && Actions is not null);
        }
        else if (change.Property == FooterProperty)
        {
            PseudoClasses.Set(":footer", Footer is not null);
        }
    }
}

/// <summary>
/// shadcn/ui's CodeBlockContent: the code in Geist Mono on relaxed lines, left to right in a right-to-left page as its
/// <c>dir="ltr"</c>, scrolling sideways or wrapped, its edges fading as it scrolls (shadcn/ui's scroll fades)
/// (<see cref="Wrap"/>, <c>:wrap</c>); its content is a panel of <see cref="CodeBlockLine"/>s, numbered from
/// <see cref="StartLine"/> with <see cref="LineNumbers"/> (<c>:numbered</c>).
/// </summary>
[PseudoClasses(":numbered", ":wrap")]
public sealed class CodeBlockContent : ContentControl
{
    /// <summary>Defines the <see cref="LineNumbers"/> property.</summary>
    public static readonly StyledProperty<bool> LineNumbersProperty = AvaloniaProperty.Register<CodeBlockContent, bool>(nameof(LineNumbers));

    /// <summary>Defines the <see cref="StartLine"/> property.</summary>
    public static readonly StyledProperty<int> StartLineProperty = AvaloniaProperty.Register<CodeBlockContent, int>(nameof(StartLine), 1);

    /// <summary>Defines the <see cref="Wrap"/> property.</summary>
    public static readonly StyledProperty<bool> WrapProperty = AvaloniaProperty.Register<CodeBlockContent, bool>(nameof(Wrap));

    /// <summary>Whether the lines are numbered.</summary>
    public bool LineNumbers
    {
        get => GetValue(LineNumbersProperty);
        set => SetValue(LineNumbersProperty, value);
    }

    /// <summary>The first line's number.</summary>
    public int StartLine
    {
        get => GetValue(StartLineProperty);
        set => SetValue(StartLineProperty, value);
    }

    /// <summary>Whether long lines wrap instead of scrolling sideways.</summary>
    public bool Wrap
    {
        get => GetValue(WrapProperty);
        set => SetValue(WrapProperty, value);
    }

    /// <summary>Numbers the lines once the template is applied.</summary>
    /// <param name="e">The applied template.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Number();
    }

    /// <summary>Numbers the lines again as the numbering or the content changes.</summary>
    /// <param name="change">The changed property.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WrapProperty)
        {
            PseudoClasses.Set(":wrap", Wrap);
        }
        else if (change.Property == LineNumbersProperty || change.Property == StartLineProperty || change.Property == ContentProperty || change.Property == FontSizeProperty || change.Property == FontFamilyProperty)
        {
            PseudoClasses.Set(":numbered", LineNumbers);
            Number();
        }
    }

    private void Number()
    {
        if (Content is not Panel panel)
        {
            return;
        }

        var number = StartLine;
        var lines = panel.Children.OfType<CodeBlockLine>().ToList();
        var digits = (StartLine + Math.Max(lines.Count, 1) - 1).ToString(System.Globalization.CultureInfo.InvariantCulture).Length;
        var ch = new Avalonia.Media.TextFormatting.TextLayout("0", new Avalonia.Media.Typeface(FontFamily), FontSize, null).WidthIncludingTrailingWhitespace;
        foreach (var line in lines)
        {
            line.Number = LineNumbers ? number++ : 0;
            line.NumberWidth = Math.Max(24, digits * ch);
        }
    }
}

/// <summary>
/// shadcn/ui's CodeBlockLine: one line of code across the whole block, its <see cref="Number"/> in a 24 px column when
/// numbered; the <c>highlighted</c>, <c>added</c> and <c>removed</c> classes tint it, the last two with a <c>+</c> or
/// <c>−</c> before it.
/// </summary>
[PseudoClasses(":numbered")]
public sealed class CodeBlockLine : ContentControl
{
    /// <summary>Defines the <see cref="Number"/> property.</summary>
    public static readonly StyledProperty<int> NumberProperty = AvaloniaProperty.Register<CodeBlockLine, int>(nameof(Number));

    /// <summary>Defines the <see cref="NumberWidth"/> property.</summary>
    public static readonly StyledProperty<double> NumberWidthProperty = AvaloniaProperty.Register<CodeBlockLine, double>(nameof(NumberWidth), 24);

    /// <summary>The width of the numbers' column, at least 24 px and as wide as the content's widest number, as shadcn/ui's subgrid column.</summary>
    public double NumberWidth
    {
        get => GetValue(NumberWidthProperty);
        set => SetValue(NumberWidthProperty, value);
    }

    /// <summary>The line's number, set by its <see cref="CodeBlockContent"/>; 0 shows none.</summary>
    public int Number
    {
        get => GetValue(NumberProperty);
        set => SetValue(NumberProperty, value);
    }

    /// <summary>Marks the line <c>:numbered</c> while it has a number.</summary>
    /// <param name="change">The changed property.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == NumberProperty)
        {
            PseudoClasses.Set(":numbered", Number > 0);
        }
    }
}

/// <summary>
/// shadcn/ui's CodeBlockCopyButton: a 24 px ghost button that copies its <see cref="Value"/> to the clipboard and shows a
/// check (<c>:copied</c>) for two seconds; on the web the browser copies it to the visitor's clipboard with no request
/// (<c>a:Web.Copies</c>).
/// </summary>
[PseudoClasses(":copied")]
public sealed class CodeBlockCopyButton : Button
{
    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<CodeBlockCopyButton, string?>(nameof(Value));

    private ITimer? reset;

    /// <summary>Binds the text the browser copies to <see cref="Value"/>, so a bound value follows the model in the page's plan.</summary>
    public CodeBlockCopyButton() => Bind(Web.CopiesProperty, CompiledBinding.Create<CodeBlockCopyButton, string?>(button => button.Value, source: this, targetNullValue: string.Empty));

    /// <summary>The text copied.</summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Puts a text on the clipboard: the top level's clipboard, or the tests' record (the headless clipboard keeps nothing).</summary>
    internal Func<string, Task>? Write { get; set; }

    /// <summary>The clock of the two seconds the check shows.</summary>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <summary>The last copy, done once the clipboard holds the value.</summary>
    public Task Copying { get; private set; } = Task.CompletedTask;

    /// <summary>Copies the value and shows the check.</summary>
    protected override void OnClick()
    {
        base.OnClick();
        var write = Write;
        if (write is null && TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
        {
            write = clipboard.SetTextAsync;
        }

        if (write is not null)
        {
            Copying = CopyAsync(write);
        }
    }

    private async Task CopyAsync(Func<string, Task> write)
    {
        await write(Value ?? string.Empty);
        PseudoClasses.Set(":copied", true);
        reset?.Dispose();
        reset = TimeProvider.CreateTimer(_ => Dispatcher.UIThread.Post(() => PseudoClasses.Set(":copied", false)), null, TimeSpan.FromSeconds(2), Timeout.InfiniteTimeSpan);
    }

    /// <summary>Stops the check's clock when the button leaves the tree.</summary>
    /// <param name="e">The event.</param>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        reset?.Dispose();
        reset = null;
    }
}
