using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Theming;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Aioros.Cosmo.Tests.Desktop;

/// <summary>Windows showing Cosmo controls in the headless session, and what the tests read from them.</summary>
internal static class Windows
{
    /// <summary>Shows <paramref name="content"/> in an 800 by 1200 window with <paramref name="theme"/> (a default <see cref="CosmoTheme"/> when null) in <paramref name="variant"/>.</summary>
    public static Window Show(Control content, ThemeVariant variant, CosmoTheme? theme = null)
    {
        var window = new Window { Width = 800, Height = 1200, RequestedThemeVariant = variant };
        window.Styles.Add(theme ?? new CosmoTheme());
        window.Styles.Add(new Aioros.Cosmo.Charts.CosmoChartsTheme());
        window.Content = content;
        _shown = window;
        Clock = Clocked ? new StillClock(window) : null;
        window.Show();
        Settle();
        return window;
    }

    private static Window? _shown;

    /// <summary>
    /// Shows <paramref name="content"/> as a reference fixture: a 640 px wide window of <paramref name="height"/> on
    /// <c>--background</c>, the content inset 24 px, as the reference app's fixture box, its text antialiased in gray as
    /// Chromium draws the reference.
    /// </summary>
    public static Window Fixture(Control content, double height, ThemeVariant variant, bool host = false, double width = 640, double padding = 24)
    {
        var window = new Window { Width = width, Height = height, RequestedThemeVariant = variant };
        TextOptions.SetTextRenderingMode(window, TextRenderingMode.Antialias);
        window.Styles.Add(new CosmoTheme());
        window.Styles.Add(new Aioros.Cosmo.Charts.CosmoChartsTheme());
        window[!TemplatedControl.BackgroundProperty] = window.GetResourceObservable("CosmoBackgroundBrush").ToBinding();
        var inset = new Border { Padding = new Thickness(padding), Child = content };
        if (host)
        {
            var root = new Panel { Children = { inset } };
            Aioros.Cosmo.Overlays.OverlayHost.SetIsHost(root, true);
            window.Content = root;
        }
        else
        {
            window.Content = inset;
        }
        _shown = window;
        Clock = Clocked ? new StillClock(window) : null;
        window.Show();
        Settle();
        return window;
    }

    /// <summary>The window's rendered frame, its popups included, as Skia draws it.</summary>
    public static SkiaSharp.SKBitmap Capture(Window window)
    {
        Settle();
        using var frame = window.CaptureRenderedFrame() ?? throw new InvalidOperationException("The window rendered no frame.");
        using var stream = new MemoryStream();
        frame.Save(stream, new Avalonia.Media.Imaging.PngBitmapEncoderOptions());
        stream.Position = 0;
        return SkiaSharp.SKBitmap.Decode(stream);
    }

    /// <summary>The control that has the keyboard focus in a window.</summary>
    public static IInputElement? Focused(Window window) => window.FocusManager?.GetFocusedElement();

    /// <summary>Moves the mouse to a point of the window, away from every control under test.</summary>
    public static void Away(Window window, Point point)
    {
        window.MouseMove(point, RawInputModifiers.None);
        Settle();
    }

    /// <summary>
    /// Runs Avalonia's queued work and a layout pass, with the transitions of the shown window's controls held, so each
    /// state's look lands at once and the tests read the settled look (the web resolves its looks the same way).
    /// </summary>
    public static void Settle()
    {
        Dispatcher.UIThread.RunJobs();
        Hold();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        Hold();
    }

    /// <summary>Whether the windows shown next run on a <see cref="StillClock"/> of their own (<see cref="Clock"/>), so a test of an animation moves it by hand.</summary>
    public static bool Clocked { get; set; }

    /// <summary>The still clock of the window shown last, when <see cref="Clocked"/>.</summary>
    public static StillClock? Clock { get; private set; }

    /// <summary>Whether <see cref="Settle"/> holds transitions (and expanders' content transitions); a test of an animation turns it off.</summary>
    public static bool Holding { get; set; } = true;

    private static void Hold()
    {
        if (_shown is null || !Holding)
        {
            return;
        }

        foreach (var expander in Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(_shown).OfType<Expander>().Where(expander => expander.ContentTransition is not null))
        {
            expander.SetValue(Expander.ContentTransitionProperty, null, Avalonia.Data.BindingPriority.Animation);
        }

        foreach (var visual in Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(_shown).OfType<Avalonia.Animation.Animatable>())
        {
            if (visual.Transitions is { Count: > 0 })
            {
                visual.SetValue(Avalonia.Animation.Animatable.TransitionsProperty, null, Avalonia.Data.BindingPriority.Animation);
            }
        }
    }

    /// <summary>The color a brush paints, its opacity folded into the alpha; transparent for no brush.</summary>
    public static CssColor Paint(IBrush? brush) => brush is ISolidColorBrush solid
        ? new CssColor(solid.Color.R, solid.Color.G, solid.Color.B, solid.Color.A / 255.0 * solid.Opacity)
        : CssColor.Transparent;

    /// <summary>The shadows of an Avalonia <see cref="BoxShadows"/> as CSS shadows, the transparent ones dropped.</summary>
    public static IReadOnlyList<CssShadow> Shadows(BoxShadows shadows) =>
        [.. Enumerable.Range(0, shadows.Count).Select(index => shadows[index])
            .Select(shadow => new CssShadow(shadow.OffsetX, shadow.OffsetY, shadow.Blur, shadow.Spread, new CssColor(shadow.Color.R, shadow.Color.G, shadow.Color.B, shadow.Color.A / 255.0), shadow.IsInset))
            .Where(shadow => shadow.Color.A > 0.002)];

    /// <summary>A control's box relative to another control (an ancestor) on the desktop.</summary>
    public static Rect Box(Control control, Control root) =>
        control.TransformToVisual(root) is { } transform ? new Rect(control.Bounds.Size).TransformToAABB(transform) : new Rect(control.Bounds.Size);

    /// <summary>Presses and releases a key with the focused control receiving it.</summary>
    public static void Key(Window window, Key key, string? text = null)
    {
        window.KeyPress(key, RawInputModifiers.None, PhysicalKey.None, text);
        window.KeyRelease(key, RawInputModifiers.None, PhysicalKey.None, text);
        Settle();
    }

    /// <summary>Presses and releases a key with modifiers held, the focused control receiving it.</summary>
    public static void Key(Window window, Key key, RawInputModifiers modifiers)
    {
        window.KeyPress(key, modifiers, PhysicalKey.None, null);
        window.KeyRelease(key, modifiers, PhysicalKey.None, null);
        Settle();
    }

    /// <summary>Types text into the focused control.</summary>
    public static void Type(Window window, string text)
    {
        window.KeyTextInput(text);
        Settle();
    }

    /// <summary>The center of a control in its window's coordinates.</summary>
    public static Point Center(Control control) =>
        control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), TopLevel.GetTopLevel(control)!) ?? default;

    /// <summary>Moves the mouse over a control's center.</summary>
    public static void Hover(Window window, Control control)
    {
        (TopLevel.GetTopLevel(control) ?? window).MouseMove(Center(control), RawInputModifiers.None);
        Settle();
    }

    /// <summary>Clicks a control's center with the left mouse button.</summary>
    public static void Click(Window window, Control control)
    {
        var top = TopLevel.GetTopLevel(control) ?? window;
        top.MouseDown(Center(control), MouseButton.Left, RawInputModifiers.None);
        top.MouseUp(Center(control), MouseButton.Left, RawInputModifiers.None);
        Settle();
    }

    /// <summary>Clicks a control's center with the right mouse button, as a context menu opens.</summary>
    public static void RightClick(Window window, Control control)
    {
        var top = TopLevel.GetTopLevel(control) ?? window;
        top.MouseDown(Center(control), MouseButton.Right, RawInputModifiers.None);
        top.MouseUp(Center(control), MouseButton.Right, RawInputModifiers.None);
        Settle();
    }

    /// <summary>The first visual descendant of a control of a type, with a name when given.</summary>
    public static T Part<T>(Control control, string? name = null) where T : Control =>
        Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(control).OfType<T>().First(part => name is null || part.Name == name);
}
