using Aioros.Cosmo.Display;
using Aioros.Cosmo.Forms;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using SkiaSharp;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Blocks;

public sealed class BlockTests(DesktopSession desktop)
{
    internal static readonly string[] Blocks = ["login-01", "login-02", "login-03", "login-04", "login-05", "signup-01", "signup-02", "signup-03", "signup-04", "signup-05"];

    private static readonly (string Fixture, double Floor)[] Floors =
    [
        ("login-01", 0.960), ("login-02", 0.960), ("login-03", 0.965), ("login-04", 0.965), ("login-05", 0.955),
        ("signup-01", 0.950), ("signup-02", 0.950), ("signup-03", 0.955), ("signup-04", 0.965), ("signup-05", 0.955),
    ];

    private static IEnumerable<string> Boxes(string what, Control root, IReadOnlyList<ElementMetrics> reference, string slot, IEnumerable<Control> controls, bool placed = false)
    {
        var expected = reference.Where(element => element.Slot == slot).ToList();
        var found = controls.Where(static control => control.IsEffectivelyVisible).ToList();
        if (found.Count != expected.Count)
        {
            return [$"{what}: {found.Count} {slot}s, shadcn {expected.Count}"];
        }

        return found.Zip(expected).SelectMany((pair, i) => Stands($"{what} {slot} #{i} {pair.Second.Text}", Windows.Box(pair.First, root), pair.Second, placed, slot == "card" ? 3 : 1.5));
    }

    private static IEnumerable<string> Stands(string what, Rect found, ElementMetrics expected, bool placed, double height)
    {
        var near = Math.Abs(found.X - expected.X) <= 1.5 && Math.Abs(found.Y - expected.Y) <= 3 && Math.Abs(found.Height - expected.Height) <= height && (placed || Math.Abs(found.Width - expected.Width) <= 1.5);
        return near ? [] : [$"{what}: {found}, shadcn {Of(expected)}"];
    }

    [Fact]
    [Trait("AC", "BLOCKS-01")]
    public async Task EveryBlockStandsAtShadcnsBoxesAsync()
    {
        var wrong = new List<string>();
        foreach (var block in Blocks)
        {
            wrong.AddRange(await FixtureAsync(desktop, block, (_, root, reference, theme) =>
                Boxes($"{block} {theme}", root, reference, "card", All<Card>(root))
                    .Concat(Boxes($"{block} {theme}", root, reference, "field-label", All<FieldLabel>(root), placed: true))
                    .Concat(Boxes($"{block} {theme}", root, reference, "input", All<TextBox>(root)))
                    .Concat(Boxes($"{block} {theme}", root, reference, "button", All<Button>(root).Where(static button => button is not HyperlinkButton)))));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "FIELD-06")]
    public async Task TheFieldSeparatorIsALineWithItsContentOnTheBackgroundAsync()
    {
        var wrong = new List<string>();
        foreach (var (block, background) in new[] { ("login-02", "CosmoBackgroundBrush"), ("login-03", "CosmoCardBrush") })
        {
            wrong.AddRange(await FixtureAsync(desktop, block, (window, root, reference, theme) =>
            {
                var separator = All<FieldSeparator>(root).Single();
                var line = Windows.Part<Border>(separator, "PART_Line");
                var content = Windows.Part<Border>(separator, "PART_Content");
                var expected = reference.Single(element => element.Slot == "field-separator-content");
                var errors = Near($"{block} {theme} separator", Windows.Box(separator, root), reference.Single(element => element.Slot == "field-separator"), 1).ToList();
                errors.AddRange(Near($"{block} {theme} line", Windows.Box(line, root), reference.Single(element => element.Slot == "separator"), 1));
                errors.AddRange(Placed($"{block} {theme} content", Windows.Box(content, root), expected, 1));
                errors.AddRange(Math.Abs(Windows.Box(content, root).Width - expected.Width) <= 2 ? [] : [$"{block} {theme} content width {Windows.Box(content, root).Width}, shadcn {expected.Width}"]);
                errors.AddRange(separator.Margin == new Thickness(0, -8) ? [] : [$"{block} {theme} margin {separator.Margin}"]);
                errors.AddRange(Paints($"{block} {theme} line", line.Background, window, "CosmoBorderBrush"));
                errors.AddRange(Paints($"{block} {theme} content", content.Background, window, background));
                errors.AddRange(Paints($"{block} {theme} text", separator.Foreground, window, "CosmoMutedForegroundBrush"));
                errors.AddRange(separator.FontSize == 14 ? [] : [$"{block} {theme} font {separator.FontSize}"]);
                return errors;
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "BLOCKS-04")]
    public async Task TheImageBlocksShowTheLocalPlaceholderAsync()
    {
        var wrong = new List<string>();
        foreach (var block in (string[])["login-04", "signup-04"])
        {
            wrong.AddRange(await FixtureAsync(desktop, block, (_, root, _, theme) => Placeholder($"{block} {theme}", root, theme)));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    private static List<string> Placeholder(string what, Control root, string theme)
    {
        var image = All<Image>(root).Single();
        var card = Windows.Box(All<Card>(root).Single(), root);
        var box = Windows.Box(image, root);
        var shade = (Border)((Panel)image.Parent!).Children[1];
        var errors = image.Source is Bitmap { PixelSize.Width: 1200 } && image.Stretch == Avalonia.Media.Stretch.UniformToFill ? new List<string>() : [$"{what}: image {image.Source} {image.Stretch}"];
        errors.AddRange(Math.Abs(box.Right - (card.Right - 1)) <= 1 && Math.Abs(box.X - card.Center.X) <= 1 && Math.Abs(box.Height - (card.Height - 2)) <= 1 ? [] : [$"{what}: image {box}, card {card}"]);
        var expected = theme == "dark" ? Avalonia.Media.Color.FromArgb(0xCC, 0, 0, 0) : Avalonia.Media.Colors.Transparent;
        errors.AddRange(shade.Background is Avalonia.Media.ISolidColorBrush solid && solid.Color == expected ? [] : [$"{what}: shade {shade.Background}"]);
        return errors;
    }

    [Fact]
    [Trait("AC", "FIDELITY-15")]
    public async Task EveryBlockMatchesShadcnsRenderOnTheDesktopAsync()
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
