using Aioros.Cosmo.Tests.Desktop;
using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Display;

public sealed class ScrollFadeTests(DesktopSession desktop)
{
    /// <summary>Where the mask turns opaque and where it starts fading again, as fractions of the viewer, or none.</summary>
    private static string Mask(ScrollViewer scroll) => scroll.OpacityMask is LinearGradientBrush { GradientStops: [_, var start, var end, _] }
        ? string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{start.Offset:0.###}-{end.Offset:0.###}")
        : "none";

    [Fact]
    [Trait("AC", "THEME-10")]
    public async Task AScrollFadeMasksTheScrolledEdgesAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            ScrollViewer Viewer(string fade, double content) => new() { Height = 100, Width = 200, Content = new Border { Height = content }, [Web.ScrollFadeProperty] = fade };
            var tall = Viewer("y", 400);
            var bottom = Viewer("b", 400);
            var fits = Viewer("y", 80);
            var window = Windows.Show(new StackPanel { Children = { tall, bottom, fits } }, ThemeVariant.Light);
            Windows.Settle();
            var masks = new List<string> { Mask(tall), Mask(bottom), Mask(fits) };
            tall.Offset = new Vector(0, 48);
            Windows.Settle();
            masks.Add(Mask(tall));
            tall.Offset = new Vector(0, 300);
            bottom.Offset = new Vector(0, 300);
            Windows.Settle();
            masks.Add(Mask(tall));
            masks.Add(Mask(bottom));
            window.Close();
            return masks;
        });

        Assert.Equal(["0-0.88", "0-0.88", "none", "0.06-0.88", "0.12-1", "none"], found);
    }
}
