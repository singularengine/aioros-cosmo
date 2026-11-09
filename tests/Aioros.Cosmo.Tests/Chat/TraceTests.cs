using Aioros.Cosmo.Chat;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Chat;

public sealed class TraceTests(DesktopSession desktop)
{
    internal static readonly string[] Traces = ["trace-static", "trace-status", "trace-description", "trace-group", "trace-collapsible", "trace-steps",
        "trace-link", "trace-sources", "trace-dialog", "trace-sheet", "trace-commit", "trace-open", "trace-turn", "trace-demo"];

    private static IEnumerable<string> Within(string what, Rect found, ElementMetrics expected, double width) =>
        Math.Abs(found.X - expected.X) <= 1.5 && Math.Abs(found.Width - expected.Width) <= width && Math.Abs(found.Y - expected.Y) <= 1.5 && Math.Abs(found.Height - expected.Height) <= 1.5
            ? []
            : [$"{what}: {found}, shadcn {Of(expected)}"];

    private static List<T> Shown<T>(Control root)
        where T : Control =>
        [.. All<T>(root).Where(static control => control.IsEffectivelyVisible)];

    private static IEnumerable<string> Boxes(string what, Control root, IReadOnlyList<ElementMetrics> reference, string slot, IReadOnlyList<Control> found, double width = 1.5)
    {
        var expected = reference.Where(element => element.Slot == slot && element.Width > 0).ToList();
        return found.Count != expected.Count
            ? [$"{what}: {found.Count} {slot}, shadcn {expected.Count}"]
            : found.Zip(expected).SelectMany((pair, i) => Within($"{what} {slot} #{i} {pair.Second.Text}", Windows.Box(pair.First, root), pair.Second, width));
    }

    private static IEnumerable<string> Looks(string what, IReadOnlyList<ElementMetrics> reference, List<TraceLine> lines)
    {
        var expected = reference.Where(element => element.Slot == "trace-line" && element.Width > 0).ToList();
        return lines.Count != expected.Count ? [] : lines.Zip(expected).SelectMany(pair =>
            Paints($"{what} {pair.Second.Text} color", pair.First.Foreground, pair.Second.Color)
                .Concat(Math.Abs(pair.First.Opacity - double.Parse(pair.Second.Opacity, System.Globalization.CultureInfo.InvariantCulture)) < 0.01 ? [] : [$"{what} {pair.Second.Text} opacity {pair.First.Opacity}, shadcn {pair.Second.Opacity}"]));
    }

    [Fact]
    [Trait("AC", "TRACE-01")]
    public async Task EveryTraceIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Traces)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (window, root, reference, theme) =>
            {
                var what = $"{fixture} {theme}";
                var lines = Shown<TraceLine>(root);
                var errors = Boxes(what, root, reference, "trace", Shown<Trace>(root)).ToList();
                errors.AddRange(Boxes(what, root, reference, "trace-line", lines, 3));
                errors.AddRange(Boxes(what, root, reference, "trace-title", [.. lines.Select(static line => (Control)Windows.Part<ContentPresenter>(line, "PART_ContentPresenter"))], 3));
                errors.AddRange(Boxes(what, root, reference, "trace-description", [.. lines.Where(static line => line.Description is not null).Select(static line => (Control)Windows.Part<ContentPresenter>(line, "PART_Description"))], 3));
                errors.AddRange(Boxes(what, root, reference, "trace-collapsible-trigger", [.. Shown<ToggleButton>(root).Where(static toggle => toggle.Name == "ExpanderHeader")], 3));
                var inner = Shown<ContentPresenter>(root).Where(static presenter => presenter.Name == "PART_ContentPresenter" && presenter.TemplatedParent is Expander).ToList();
                errors.AddRange(Boxes(what, root, reference, "trace-collapsible-content-inner", [.. inner]));
                errors.AddRange(inner.SelectMany(presenter => presenter.BorderThickness == new Thickness(1, 0, 0, 0) ? Paints($"{what} step border", presenter.BorderBrush, window, "CosmoBorderBrush") : [$"{what} step border {presenter.BorderThickness}"]));
                errors.AddRange(Looks(what, reference, lines));
                errors.AddRange(lines.Where(static line => line.Description is not null).SelectMany(line => Paints($"{what} description", Windows.Part<ContentPresenter>(line, "PART_Description").Foreground,
                    reference.First(element => element.Slot == "trace-description").Color)));
                return errors;
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "TRACE-03")]
    public async Task AStepOpensAndClosesFromItsTriggerAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var window = DisplayFixtures.Show("trace-collapsible", ThemeVariant.Light);
            var root = (Control)window.Content!;
            var trigger = All<ToggleButton>(root).Single(static toggle => toggle.Name == "ExpanderHeader");
            var chevron = Windows.Part<Viewbox>(trigger, "PART_Chevron");
            var content = All<ContentPresenter>(root).Single(static presenter => presenter.Name == "PART_ContentPresenter" && presenter.TemplatedParent is Expander);
            double Angle() => chevron.RenderTransform is RotateTransform rotate ? rotate.Angle : chevron.RenderTransform?.Value.M12 is > 0.99 ? 90 : 0;
            var closed = (content.IsEffectivelyVisible, Angle());
            Windows.Click(window, trigger);
            var open = (content.IsEffectivelyVisible, Angle());
            Windows.Click(window, trigger);
            var again = (content.IsEffectivelyVisible, Angle());
            window.Close();
            return (closed, open, again);
        });

        Assert.Equal((false, 0.0), found.closed);
        Assert.Equal((true, 90.0), found.open);
        Assert.Equal((false, 0.0), found.again);
    }
    private static Window Big(string fixture)
    {
        var window = DisplayFixtures.Show(fixture, ThemeVariant.Light);
        window.Height = 1400;
        Windows.Settle();
        return window;
    }

    private static List<string> Links()
    {
        var window = Big("trace-link");
        var root = (Control)window.Content!;
        var links = All<HyperlinkButton>(root).ToList();
        var errors = links.Select(static link => link.NavigateUri?.ToString()).SequenceEqual(["#link", "https://docs.astro.build/en/install-and-setup/"]) ? new List<string>() : [$"links {string.Join(',', links.Select(static link => link.NavigateUri))}"];
        var line = All<TraceLine>(links[0]).Single();
        errors.AddRange(Paints("line at rest", line.Foreground, window, "CosmoMutedForegroundBrush"));
        Windows.Hover(window, links[0]);
        errors.AddRange(Paints("line under the pointer", line.Foreground, window, "CosmoForegroundBrush"));
        window.Close();
        return errors;
    }

    private static List<string> Opens(string fixture, Func<Control, bool> open)
    {
        var window = Big(fixture);
        var root = (Control)window.Content!;
        var trigger = All<Button>(root).First(static button => button.Classes.Contains("trace") && button is not HyperlinkButton);
        Windows.Click(window, trigger);
        var errors = open(root) ? new List<string>() : [$"{fixture}: nothing opened"];
        window.Close();
        return errors;
    }

    private static List<string> Toggles()
    {
        var window = Big("trace-open");
        var root = (Control)window.Content!;
        var step = All<Expander>(root)[1];
        var button = All<Aioros.Cosmo.Display.CollapsibleTrigger>(root).Single();
        string Label() => string.Join(',', All<TextBlock>(button).Where(static text => text.IsEffectivelyVisible).Select(static text => text.Text));
        var states = new List<string> { $"{step.IsExpanded} {Label()}" };
        Windows.Click(window, button);
        states.Add($"{step.IsExpanded} {Label()}");
        Windows.Click(window, All<ToggleButton>(step).First(static toggle => toggle.Name == "ExpanderHeader"));
        states.Add($"{step.IsExpanded} {Label()}");
        window.Close();
        string[] expected = ["False Show the pnpm output", "True Hide the pnpm output", "False Show the pnpm output"];
        return states.SequenceEqual(expected) ? [] : [$"trace-open {string.Join(" | ", states)}"];
    }

    [Fact]
    [Trait("AC", "TRACE-06")]
    public async Task TheDocsTracesActAsShadcnsAsync()
    {
        var wrong = await desktop.RunAsync(() => Links()
            .Concat(Opens("trace-dialog", static root => All<Aioros.Cosmo.Overlays.Dialog>(root).Single().IsOpen))
            .Concat(Opens("trace-sheet", static root => All<Aioros.Cosmo.Overlays.Sheet>(root).Single().IsOpen))
            .Concat(Opens("trace-commit", static root => All<Aioros.Cosmo.Overlays.Sheet>(root)[0].IsOpen))
            .Concat(Toggles())
            .ToList());

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
}
