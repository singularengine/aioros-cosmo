using Aioros.Cosmo.Chat;
using Aioros.Cosmo.Display;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Presenters;
using Avalonia.Styling;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Chat;

public sealed class MessageScrollerTests(DesktopSession desktop)
{
    internal static readonly string[] Scrollers = ["message-scroller-end", "message-scroller-start"];

    private static IEnumerable<string> Within(string what, Rect found, ElementMetrics expected, double down = 3) =>
        Math.Abs(found.X - expected.X) <= 1.5 && Math.Abs(found.Width - expected.Width) <= 1.5 && Math.Abs(found.Y - expected.Y) <= down && Math.Abs(found.Height - expected.Height) <= down
            ? []
            : [$"{what}: {found}, shadcn {Of(expected)}"];

    private static List<string> Frame(string what, Control root, IReadOnlyList<ElementMetrics> reference, MessageScroller scroller)
    {
        var slot = (string name) => reference.First(element => element.Slot == name);
        var viewport = Windows.Part<ScrollViewer>(scroller, "PART_Viewport");
        var content = All<ContentPresenter>(scroller).First(presenter => presenter.Name == "PART_ContentPresenter" && presenter.TemplatedParent == scroller);
        var expected = slot("message-scroller-content");
        var errors = Within($"{what} card", Windows.Box(All<Card>(root).Single(), root), slot("card")).ToList();
        errors.AddRange(Within($"{what} viewport", Windows.Box(viewport, root), slot("message-scroller-viewport")));
        errors.AddRange(Math.Abs(Windows.Box(content, root).Width - expected.Width) <= 1.5 ? [] : [$"{what} content width {Windows.Box(content, root).Width}, shadcn {expected.Width}"]);
        var bottom = Windows.Box(content, root).Bottom;
        var visibleBottom = Windows.Box(viewport, root).Bottom;
        errors.AddRange(scroller.DefaultScrollPosition == MessageScrollPosition.Start || Math.Abs(bottom - visibleBottom) <= 1.5 ? [] : [$"{what} content ends at {bottom}, the viewport at {visibleBottom}"]);
        return errors;
    }

    [Fact]
    [Trait("AC", "MESSAGESCROLLER-01")]
    public async Task TheScrollerOpensAtItsPositionAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Scrollers)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (_, root, reference, theme) =>
            {
                var what = $"{fixture} {theme}";
                var scroller = All<MessageScroller>(root).Single();
                var errors = Frame(what, root, reference, scroller);
                var bubbles = All<Bubble>(root).Select(static bubble => (Control)Windows.Part<Border>(bubble, "PART_Content")).ToList();
                var expected = reference.Where(element => element.Slot == "bubble-content").ToList();
                var end = scroller.DefaultScrollPosition == MessageScrollPosition.End;
                errors.AddRange(bubbles.Count == expected.Count
                    ? bubbles.Zip(expected).Where(pair => pair.Second.Y + pair.Second.Height > 106 && pair.Second.Y < 423)
                        .SelectMany(pair => Within($"{what} bubble {pair.Second.Text}", Windows.Box(pair.First, root), pair.Second))
                    : [$"{what}: {bubbles.Count} bubbles, shadcn {expected.Count}"]);
                var button = Windows.Part<Button>(scroller, "PART_Button");
                errors.AddRange(button.IsEffectivelyVisible == !end ? [] : [$"{what} button visible {button.IsEffectivelyVisible}"]);
                var shadcn = reference.First(element => element.Slot == "message-scroller-button");
                errors.AddRange(end ? [] : Within($"{what} button", Windows.Box(button, root), shadcn, 1.5).Concat(Paints($"{what} button background", button.Background, shadcn.Background)));
                errors.AddRange(scroller.Classes.Contains(":end") == end ? [] : [$"{what} :end"]);
                return errors;
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "MESSAGESCROLLER-02")]
    public async Task TheButtonScrollsToTheEndWhichFollowsNewMessagesAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var window = DisplayFixtures.Show("message-scroller-start", ThemeVariant.Light);
            var root = (Control)window.Content!;
            var scroller = All<MessageScroller>(root).Single();
            var viewport = Windows.Part<ScrollViewer>(scroller, "PART_Viewport");
            var button = Windows.Part<Button>(scroller, "PART_Button");
            Windows.Click(window, button);
            var clicked = (End: AtEnd(viewport), Classes: scroller.Classes.Contains(":end"), Button: button.IsEffectivelyVisible);
            var transcript = (StackPanel)scroller.Content!;
            var added = new Message { Content = new MessageContent { Children = { new Bubble { Content = "Deployed. Have a good weekend!" } } } };
            transcript.Children.Add(added);
            Windows.Settle();
            var last = Windows.Box(Windows.Part<Border>(All<Bubble>(added).Single(), "PART_Content"), root);
            var shown = Windows.Box(viewport, root);
            var result = (clicked, Followed: AtEnd(viewport), InView: last.Y >= shown.Y && last.Bottom <= shown.Bottom, Button: button.IsEffectivelyVisible);
            window.Close();
            return result;
        });

        Assert.Equal((true, true, false), found.clicked);
        Assert.True(found.Followed);
        Assert.True(found.InView);
        Assert.False(found.Button);
    }

    private static bool AtEnd(ScrollViewer viewport) => viewport.Offset.Y >= viewport.Extent.Height - viewport.Viewport.Height - 1 && viewport.Offset.Y > 0;
    [Fact]
    [Trait("AC", "MESSAGESCROLLER-05")]
    public async Task TheTranscriptFadesAtItsBottomAwayFromItsEndAsync()
    {
        static string Mask(ScrollViewer viewport) => viewport.OpacityMask is LinearGradientBrush { GradientStops: [_, var start, var end, _] }
            ? string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{start.Offset:0.##}-{end.Offset:0.##}")
            : "none";
        var found = await desktop.RunAsync(() =>
        {
            var masks = new List<string>();
            foreach (var fixture in (string[])["message-scroller-start", "message-scroller-end"])
            {
                var window = DisplayFixtures.Show(fixture, ThemeVariant.Light);
                var viewport = Windows.Part<ScrollViewer>(All<MessageScroller>((Control)window.Content!).Single(), "PART_Viewport");
                masks.Add($"{Mask(viewport)} {Math.Min(viewport.Bounds.Height * 0.12, 40) / viewport.Bounds.Height:0.##}");
                window.Close();
            }

            return masks;
        });

        Assert.Equal(2, found.Count);
        var start = found[0].Split(' ');
        Assert.Equal($"0-{1 - double.Parse(start[1], System.Globalization.CultureInfo.InvariantCulture):0.##}", start[0]);
        Assert.StartsWith("none ", found[1], StringComparison.Ordinal);
    }
}
