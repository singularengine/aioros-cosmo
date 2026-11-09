using Aioros.Cosmo.Chat;
using Aioros.Cosmo.Display;
using Aioros.Cosmo.Overlays;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Chat;

public sealed class ChatActionTests(DesktopSession desktop)
{
    internal static readonly string[] Markers = ["marker-shimmer", "marker-status", "marker-link-button"];

    internal static readonly string[] Bubbles = ["bubble-link-button", "bubble-collapsible", "bubble-markdown"];

    internal static readonly string[] Attachments = ["attachment-trigger", "attachment-image"];

    private static List<Control> Shown(IEnumerable<Control> controls) => [.. controls.Where(static control => control.IsEffectivelyVisible)];

    /// <summary>Whether a shimmering text is painted by the sweep: its texts' foreground a gradient.</summary>
    private static bool Sweeps(Control part) =>
        part.GetVisualDescendants().OfType<TextBlock>().Any() && part.GetVisualDescendants().OfType<TextBlock>().All(static text => text.Foreground is LinearGradientBrush);

    private static List<string> MarkerParts(string what, Window window, Control root, IReadOnlyList<Testing.ElementMetrics> reference)
    {
        var markers = Shown(All<Control>(root).Where(static control => control is Marker or MarkerLink or MarkerButton));
        var errors = ChatTests.Slots(what, root, reference, "marker", markers).ToList();
        errors.AddRange(ChatTests.Slots(what, root, reference, "marker-icon", Shown(markers.Select(static marker => (Control)Windows.Part<ContentPresenter>(marker, "PART_Icon")))));
        var contents = markers.Select(static marker => (Control)Windows.Part<ContentPresenter>(marker, "PART_ContentPresenter")).ToList();
        errors.AddRange(ChatTests.Slots(what, root, reference, "marker-content", contents, width: 3));
        foreach (var (marker, content) in markers.Zip(contents))
        {
            errors.AddRange(Paints($"{what} {marker.GetType().Name} text", ((ContentPresenter)content).Foreground, window, "CosmoMutedForegroundBrush"));
            errors.AddRange(marker.Classes.Contains("shimmer") == Sweeps(content) ? [] : [$"{what}: a shimmer marker sweeps {Sweeps(content)}"]);
        }

        foreach (var link in markers.OfType<MarkerLink>())
        {
            var underline = Windows.Part<ContentPresenter>(link, "PART_ContentPresenter").GetValue(TextBlock.TextDecorationsProperty)?.SingleOrDefault();
            errors.AddRange(underline is { Location: TextDecorationLocation.Underline, StrokeOffset: 1.95, StrokeOffsetUnit: TextDecorationUnit.Pixel } ? [] : [$"{what}: link underline {underline?.StrokeOffset}"]);
        }

        return errors;
    }

    [Fact]
    [Trait("AC", "MARKER-03")]
    public async Task ShimmeringLinkedAndPressedMarkersAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Markers)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (window, root, reference, theme) => MarkerParts($"{fixture} {theme}", window, root, reference)));
        }

        wrong.AddRange(await desktop.RunAsync(() =>
        {
            var window = DisplayFixtures.Show("marker-link-button", ThemeVariant.Light);
            var root = (Control)window.Content!;
            var errors = new List<string>();
            foreach (var marker in All<Control>(root).Where(static control => control is MarkerLink or MarkerButton))
            {
                Windows.Hover(window, marker);
                errors.AddRange(Paints($"{marker.GetType().Name} hovered", Windows.Part<ContentPresenter>(marker, "PART_ContentPresenter").Foreground, window, "CosmoForegroundBrush"));
                Windows.Away(window, new Avalonia.Point(1, 1));
            }

            window.Close();
            return errors;
        }));

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    /// <summary>A bubble's box: its content border, or the button that is its content.</summary>
    private static Control Box(Bubble bubble) => bubble.Content is Button button ? Windows.Part<Border>(button, "PART_Box") : Windows.Part<Border>(bubble, "PART_Content");

    [Fact]
    [Trait("AC", "BUBBLE-04")]
    public async Task ABubbleThatIsAButtonOrCollapsibleIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Bubbles)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (window, root, reference, theme) =>
            {
                var what = $"{fixture} {theme}";
                var bubbles = All<Bubble>(root).Where(static bubble => bubble.IsEffectivelyVisible).ToList();
                var errors = ChatTests.Slots(what, root, reference, "bubble-content", [.. bubbles.Select(Box)], width: 3).ToList();
                foreach (var bubble in bubbles.Where(static bubble => bubble.Content is Button))
                {
                    var button = (Button)bubble.Content!;
                    errors.AddRange(Paints($"{what} action", ((Border)Box(bubble)).Background, window, "CosmoBubbleTintedBrush"));
                    Windows.Hover(window, button);
                    errors.AddRange(Paints($"{what} action hovered", ((Border)Box(bubble)).Background, window, "CosmoBubbleTintedHoverBrush"));
                    Windows.Away(window, new Avalonia.Point(1, 1));
                }

                return errors;
            }));
        }

        var found = await desktop.RunAsync(() =>
        {
            var window = DisplayFixtures.Show("bubble-collapsible", ThemeVariant.Light);
            window.Height = 1200;
            Windows.Settle();
            var root = (Control)window.Content!;
            var collapsible = All<Collapsible>(root).Single();
            var trigger = All<CollapsibleTrigger>(root).Single();
            var chevron = All<Control>(trigger).Single(static box => box.Classes.Contains("chevron"));
            var texts = All<TextBlock>(collapsible).Where(static text => Collapsible.GetShows(text) != CollapsibleShows.Always).ToList();
            string State() => $"{collapsible.IsOpen} {string.Join(',', texts.Where(static text => text.IsEffectivelyVisible).Select(static text => text.Text![..9]))} {chevron.RenderTransform?.Value.M11:0}";
            var closed = State();
            Windows.Click(window, trigger);
            var open = State();
            Windows.Click(window, trigger);
            var again = State();
            window.Close();
            return (closed, open, again);
        });

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
        Assert.Equal("False The acces,Show more 1", found.closed);
        Assert.Equal("True The acces,Show less -1", found.open);
        Assert.Equal(found.closed, found.again);
    }

    [Fact]
    [Trait("AC", "ATTACHMENT-04")]
    public async Task AnAttachmentWithATriggerOrAnImageIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Attachments)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (_, root, reference, theme) =>
            {
                var what = $"{fixture} {theme}";
                var attachments = Shown(All<Attachment>(root));
                var errors = ChatTests.Slots(what, root, reference, "attachment", attachments).ToList();
                errors.AddRange(ChatTests.Slots(what, root, reference, "attachment-media", [.. attachments.Select(static attachment => (Control)Windows.Part<Border>(attachment, "PART_Media"))]));
                errors.AddRange(ChatTests.Slots(what, root, reference, "attachment-title", [.. attachments.Select(static attachment => (Control)Windows.Part<ContentPresenter>(attachment, "PART_Title"))], width: 3));
                errors.AddRange(ChatTests.Slots(what, root, reference, "attachment-actions", [.. attachments.Select(static attachment => (Control)Windows.Part<ContentPresenter>(attachment, "PART_Actions"))]));
                return errors;
            }));
        }

        wrong.AddRange(await desktop.RunAsync(() =>
        {
            var window = DisplayFixtures.Show("attachment-trigger", ThemeVariant.Light);
            var root = (Control)window.Content!;
            var attachment = All<Attachment>(root).Single();
            var box = Windows.Part<Border>(attachment, "PART_Box");
            var trigger = (Button)attachment.Trigger!;
            var dialog = All<Dialog>(root).Single();
            var errors = new List<string>();
            errors.AddRange(Windows.Box(trigger, root) == Windows.Box(box, root) ? [] : [$"trigger {Windows.Box(trigger, root)}, card {Windows.Box(box, root)}"]);
            Windows.Hover(window, trigger);
            errors.AddRange(Paints("hovered card", box.Background, window, "CosmoMutedHalfBrush"));
            Windows.Click(window, All<Button>(Windows.Part<ContentPresenter>(attachment, "PART_Actions"))[0]);
            errors.AddRange(dialog.IsOpen ? ["an action opened the dialog"] : []);
            Windows.Click(window, trigger);
            errors.AddRange(dialog.IsOpen ? [] : ["the trigger did not open the dialog"]);
            window.Close();
            return errors;
        }));

        wrong.AddRange(await FixtureAsync(desktop, "attachment-states", (_, root, _, theme) => All<Attachment>(root)
            .Where(static attachment => Sweeps(Windows.Part<ContentPresenter>(attachment, "PART_Title")) != attachment.State is AttachmentState.Uploading or AttachmentState.Processing)
            .Select(attachment => $"attachment-states {theme} {attachment.State}: title sweeps {!(attachment.State is AttachmentState.Uploading or AttachmentState.Processing)}")));

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "TRACE-04")]
    public async Task ARunningTitleShimmersAsync()
    {
        var wrong = await FixtureAsync(desktop, "trace-status", (_, root, _, theme) => All<TraceLine>(root)
            .Where(static line => Sweeps(Windows.Part<ContentPresenter>(line, "PART_ContentPresenter")) != (line.Status == TraceStatus.Running))
            .Select(line => $"trace-status {theme} {line.Status}: title sweeps {line.Status != TraceStatus.Running}"));

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
}
