using Aioros.Cosmo.Chat;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using SkiaSharp;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Chat;

public sealed class ChatTests(DesktopSession desktop)
{
    internal static readonly string[] Markers = ["marker-demo", "marker-variants", "marker-icon", "marker-border"];

    internal static readonly string[] Bubbles = ["bubble-demo", "bubble-variants", "bubble-alignment", "bubble-group-demo"];

    internal static readonly string[] Messages = ["message-demo", "message-group", "message-header-footer", "message-avatar"];

    internal static readonly string[] Attachments = ["attachment-demo", "attachment-states", "attachment-sizes", "attachment-group"];

    private static readonly (string Fixture, double Floor)[] Floors =
    [
        ("marker-demo", 0.985), ("marker-variants", 0.985), ("marker-icon", 0.985), ("marker-border", 0.985),
        ("bubble-demo", 0.960), ("bubble-variants", 0.955), ("bubble-alignment", 0.970), ("bubble-group-demo", 0.965),
        ("message-demo", 0.970), ("message-group", 0.975), ("message-header-footer", 0.975), ("message-avatar", 0.960),
        ("attachment-demo", 0.975), ("attachment-states", 0.970), ("attachment-sizes", 0.980), ("attachment-group", 0.980),
        ("message-scroller-end", 0.965), ("message-scroller-start", 0.965),
        ("marker-shimmer", 0.990), ("marker-status", 0.985), ("marker-link-button", 0.985), ("bubble-link-button", 0.970), ("bubble-collapsible", 0.970),
        ("attachment-trigger", 0.985), ("attachment-image", 0.980), ("bubble-markdown", 0.950),
    ];

    /// <summary>
    /// Boxes that differ from shadcn/ui's slots, in order: by more than <paramref name="tolerance"/> across and in height,
    /// <paramref name="width"/> in width and 3 px down the page (the desktop rounds each 22.75 px line of text).
    /// </summary>
    internal static IEnumerable<string> Slots(string what, Control root, IReadOnlyList<ElementMetrics> reference, string slot, IReadOnlyList<Control> found, double tolerance = 1.5, double width = 1.5)
    {
        var expected = reference.Where(element => element.Slot == slot).ToList();
        if (found.Count != expected.Count)
        {
            return [$"{what}: {found.Count} {slot}, shadcn {expected.Count}"];
        }

        return found.Zip(expected).SelectMany((pair, i) =>
        {
            var box = Windows.Box(pair.First, root);
            var near = Math.Abs(box.X - pair.Second.X) <= tolerance && Math.Abs(box.Y - pair.Second.Y) <= Math.Max(tolerance, 3) && Math.Abs(box.Height - pair.Second.Height) <= tolerance && Math.Abs(box.Width - pair.Second.Width) <= width;
            return near ? [] : (IEnumerable<string>)[$"{what} {slot} #{i} {pair.Second.Text}: {box}, shadcn {Of(pair.Second)}"];
        });
    }

    private static List<Control> Visible<T>(Control root, Func<T, Control> part)
        where T : Control =>
        [.. All<T>(root).Where(static control => control.IsEffectivelyVisible).Select(part).Where(static control => control.IsEffectivelyVisible)];

    private static IEnumerable<string> Paint(string what, IBrush? found, string expected) =>
        Windows.Paint(found).Near(CssColor.Parse(expected), 2) ? [] : [$"{what}: {Windows.Paint(found)}, shadcn {expected}"];

    private static List<string> MarkerLook(string what, Window window, Control root, Marker marker)
    {
        var errors = Paints($"{what} text", marker.Foreground, window, "CosmoMutedForegroundBrush").ToList();
        errors.AddRange(marker.FontSize == 14 ? [] : [$"{what} font {marker.FontSize}"]);
        var before = Windows.Part<Border>(marker, "PART_Before");
        var content = Windows.Part<StackPanel>(marker, "PART_Body");
        var separated = before.IsEffectivelyVisible && before.Bounds.Height == 1 && Math.Abs(Windows.Box(content, root).X - Windows.Box(before, root).Right - 12) <= 1.5;
        errors.AddRange(!marker.Classes.Contains("separator") || separated ? [] : [$"{what} separator line {Windows.Box(before, root)}"]);
        errors.AddRange(!marker.Classes.Contains("border") || (marker.BorderThickness == new Thickness(0, 0, 0, 1) && marker.Padding.Bottom == 8) ? [] : [$"{what} border {marker.BorderThickness} {marker.Padding}"]);
        return errors;
    }

    [Fact]
    [Trait("AC", "MARKER-01")]
    public async Task EveryMarkerIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Markers)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (window, root, reference, theme) =>
            {
                var what = $"{fixture} {theme}";
                var errors = Slots(what, root, reference, "marker", Visible<Marker>(root, static marker => marker)).ToList();
                errors.AddRange(Slots(what, root, reference, "marker-icon", Visible<Marker>(root, static marker => Windows.Part<ContentPresenter>(marker, "PART_Icon"))));
                errors.AddRange(Slots(what, root, reference, "marker-content", Visible<Marker>(root, static marker => Windows.Part<ContentPresenter>(marker, "PART_ContentPresenter")), width: 3));
                errors.AddRange(All<Marker>(root).SelectMany(marker => MarkerLook(what, window, root, marker)));
                return errors;
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "BUBBLE-01")]
    public async Task EveryBubbleIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Bubbles)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (_, root, reference, theme) =>
            {
                var what = $"{fixture} {theme}";
                var contents = Visible<Bubble>(root, static bubble => Windows.Part<Border>(bubble, "PART_Content"));
                var errors = Slots(what, root, reference, "bubble-content", contents, width: 3).ToList();
                var expected = reference.Where(element => element.Slot == "bubble-content").ToList();
                errors.AddRange(contents.Count == expected.Count
                    ? contents.Zip(expected).SelectMany(pair => Paint($"{what} {pair.Second.Text} background", ((Border)pair.First).Background, pair.Second.Background)
                        .Concat(Paint($"{what} {pair.Second.Text} text", Avalonia.Controls.Documents.TextElement.GetForeground(((Border)pair.First).Child!), pair.Second.Color)))
                    : []);
                errors.AddRange(Slots(what, root, reference, "bubble-reactions", Visible<Bubble>(root, static bubble => Windows.Part<Border>(bubble, "PART_Reactions")), 3, 3));
                return errors;
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "BUBBLE-03")]
    public async Task TheTintedBubbleIsThePrimaryLightenedAsync()
    {
        var themes = await ReferenceMetrics.ThemesAsync();
        var wrong = await desktop.RunAsync(() => Enum.GetValues<global::Aioros.Cosmo.Theming.CosmoColor>().SelectMany(color => new[] { (Avalonia.Styling.ThemeVariant.Light, "light", 0.93), (Avalonia.Styling.ThemeVariant.Dark, "dark", 0.3) }.SelectMany(variant =>
        {
            var dictionary = global::Aioros.Cosmo.Theming.CosmoResources.Colors(color);
            var primary = themes[color.ToString().ToLowerInvariant()][variant.Item2]["primary"];
            var expected = CssColor.Parse(Tinted(primary, variant.Item3));
            return dictionary.TryGetResource("CosmoBubbleTintedBrush", variant.Item1, out var brush) && brush is ISolidColorBrush solid && Windows.Paint(solid).Near(expected, 1)
                ? []
                : (IEnumerable<string>)[$"{color} {variant.Item2}: {brush}, expected {expected}"];
        })).ToList());

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    /// <summary><c>oklch(from {primary} {lightness} calc(c*0.4) h)</c>.</summary>
    private static string Tinted(string primary, double lightness)
    {
        var parts = primary[(primary.IndexOf('(', StringComparison.Ordinal) + 1)..^1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var chroma = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture) * 0.4;
        var hue = parts.Length > 2 ? parts[2] : "0";
        return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"oklch({lightness} {chroma} {hue})");
    }

    [Fact]
    [Trait("AC", "MESSAGE-01")]
    public async Task EveryMessageIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Messages)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (_, root, reference, theme) =>
            {
                var what = $"{fixture} {theme}";
                var errors = Slots(what, root, reference, "message", Visible<Message>(root, static message => message)).ToList();
                var avatars = All<Message>(root).Select(static message => Windows.Part<ContentPresenter>(message, "PART_Avatar")).Where(static avatar => avatar.IsEffectivelyVisible && avatar.Bounds.Height > 0).Cast<Control>().ToList();
                errors.AddRange(Slots(what, root, [.. reference.Where(static element => element.Slot != "message-avatar" || element.Height > 0)], "message-avatar", avatars));
                errors.AddRange(Slots(what, root, reference, "message-header", Visible<MessageHeader>(root, static header => header), width: 3));
                errors.AddRange(Slots(what, root, reference, "message-footer", Visible<MessageFooter>(root, static footer => footer), width: 3));
                return errors;
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "ATTACHMENT-01")]
    public async Task EveryAttachmentIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Attachments)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (_, root, reference, theme) =>
            {
                var what = $"{fixture} {theme}";
                var errors = Slots(what, root, reference, "attachment", Visible<Attachment>(root, static attachment => attachment)).ToList();
                errors.AddRange(Slots(what, root, reference, "attachment-media", Visible<Attachment>(root, static attachment => Windows.Part<Border>(attachment, "PART_Media"))));
                errors.AddRange(Slots(what, root, reference, "attachment-title", Visible<Attachment>(root, static attachment => Windows.Part<ContentPresenter>(attachment, "PART_Title")), width: 3));
                errors.AddRange(Slots(what, root, reference, "attachment-description", Visible<Attachment>(root, static attachment => Windows.Part<ContentPresenter>(attachment, "PART_Description")), width: 3));
                errors.AddRange(Slots(what, root, reference, "attachment-actions", Visible<Attachment>(root, static attachment => Windows.Part<ContentPresenter>(attachment, "PART_Actions"))));
                return errors;
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "ATTACHMENT-02")]
    public async Task TheAttachmentStatesTakeShadcnsLooksAsync()
    {
        var wrong = await FixtureAsync(desktop, "attachment-states", (window, root, _, theme) =>
        {
            var errors = new List<string>();
            foreach (var attachment in All<Attachment>(root))
            {
                var what = $"{theme} {attachment.State}";
                var dash = Windows.Part<Avalonia.Controls.Shapes.Rectangle>(attachment, "PART_Dash");
                var box = Windows.Part<Border>(attachment, "PART_Box");
                errors.AddRange(dash.IsEffectivelyVisible == (attachment.State == AttachmentState.Idle) && (attachment.State != AttachmentState.Idle || dash.StrokeDashArray is { Count: > 0 }) ? [] : [$"{what}: dashed {dash.IsEffectivelyVisible}"]);
                errors.AddRange(attachment.State switch
                {
                    AttachmentState.Error => Paints($"{what} border", box.BorderBrush, window, "CosmoAttachmentErrorBorderBrush")
                        .Concat(Paints($"{what} media", Windows.Part<Border>(attachment, "PART_Media").Background, window, "CosmoAttachmentErrorMediaBrush"))
                        .Concat(Paints($"{what} icon", Windows.Part<ContentPresenter>(attachment, "PART_MediaPresenter").Foreground, window, "CosmoDestructiveBrush"))
                        .Concat(Paints($"{what} description", Windows.Part<ContentPresenter>(attachment, "PART_Description").Foreground, window, "CosmoAttachmentErrorDescriptionBrush")),
                    AttachmentState.Idle => [],
                    _ => Paints($"{what} border", box.BorderBrush, window, "CosmoBorderBrush"),
                });
            }

            return errors;
        });

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "FIDELITY-17")]
    public async Task EveryChatFixtureMatchesShadcnsRenderOnTheDesktopAsync()
    {
        var verdicts = new List<string>();
        var wrong = new List<string>();
        foreach (var (variant, theme) in Themes)
        {
            foreach (var (fixture, floor) in Floors)
            {
                var (matched, size, expected) = await desktop.RunAsync<(double, (int, int), (int, int))>(() =>
                {
                    var window = DisplayFixtures.Show(fixture, variant);
                    using var ours = Windows.Capture(window);
                    using var reference = Pixels.Reference(fixture, theme);
                    if (Environment.GetEnvironmentVariable("COSMO_CAPTURES") is { Length: > 0 } folder)
                    {
                        using var file = File.Create(Path.Combine(folder, $"{fixture}.{theme}.png"));
                        ours.Encode(file, SKEncodedImageFormat.Png, 100);
                    }

                    window.Close();
                    return (Pixels.Matched(ours, reference), (ours.Width, ours.Height), (reference.Width, reference.Height));
                });
                verdicts.Add($"{fixture}.{theme}: {matched:P2}");
                if (size != expected || matched < floor)
                {
                    wrong.Add($"{fixture}.{theme}: {size} (shadcn {expected}), {matched:P2} of pixels within {Pixels.Tolerance}/255, floor {floor:P0}");
                }
            }
        }

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join(Environment.NewLine, verdicts));
        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
}
