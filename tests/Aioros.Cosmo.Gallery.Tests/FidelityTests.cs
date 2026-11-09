using Aioros.Cosmo.Testing;
using SkiaSharp;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class FidelityTests(GalleryHost gallery, Browser browser)
{
    [Fact]
    [Trait("AC", "FIDELITY-01")]
    public async Task EveryFixtureMatchesShadcnsRenderAsync()
    {
        var verdicts = new List<string>();
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var fixture in GalleryHost.FoundationFixtures)
            {
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
                var shot = await (await page.QuerySelectorAsync("#fixture"))!.ScreenshotAsync(new() { Animations = await Browser.StillAsync(page) });
                await page.Context.CloseAsync();
                using var ours = SKBitmap.Decode(shot);
                using var reference = SKBitmap.Decode(Repository.Path("tools", "shadcn", "out", $"{fixture}.{theme}.png"));
                var matched = Pixels.Matched(ours, reference);
                var floor = fixture.StartsWith("button", StringComparison.Ordinal) ? 0.94 : 0.95;
                verdicts.Add($"{fixture}.{theme}: {matched:P2}");
                if (ours.Width != reference.Width || ours.Height != reference.Height || matched < floor)
                {
                    wrong.Add($"{fixture}.{theme}: {ours.Width}x{ours.Height} (shadcn {reference.Width}x{reference.Height}), {matched:P2} of pixels within {Pixels.Tolerance}/255, floor {floor:P0}");
                }
            }
        }

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join(Environment.NewLine, verdicts));
        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "FIDELITY-02")]
    public async Task EveryInputFixtureMatchesShadcnsRenderAsync() =>
        Assert.Empty(await CompareAsync(new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["input-states"] = 0.96, ["textarea-states"] = 0.95, ["field-form"] = 0.96, ["field-content"] = 0.96, ["field-responsive"] = 0.96, ["native-select"] = 0.96, ["select-trigger"] = 0.96, ["input-file"] = 0.98,
        }));

    [Fact]
    [Trait("AC", "FIDELITY-03")]
    public async Task EveryChoiceFixtureMatchesShadcnsRenderAsync() =>
        Assert.Empty(await CompareAsync(
            new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["checkbox-states"] = 0.96, ["radio-group"] = 0.96, ["switch-states"] = 0.94, ["toggle-states"] = 0.96, ["toggle-group"] = 0.96,
            }));

    [Fact]
    [Trait("AC", "FIDELITY-04")]
    public async Task EveryRangeAndCompositeFixtureMatchesShadcnsRenderAsync() =>
        Assert.Empty(await CompareAsync(
            new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["slider-states"] = 0.95, ["slider-range"] = 0.99, ["progress"] = 0.96, ["input-group"] = 0.95, ["input-otp"] = 0.95,
            }));

    [Fact]
    [Trait("AC", "FIDELITY-07")]
    public async Task EveryDisplayFixtureMatchesShadcnsRenderAsync() =>
        Assert.Empty(await CompareAsync(GalleryHost.DisplayFixtures.ToDictionary(fixture => fixture.Fixture, fixture => fixture.Floor, StringComparer.Ordinal), heights: false));

    [Fact]
    [Trait("AC", "FIDELITY-12")]
    public async Task EveryCalendarFixtureMatchesShadcnsRenderOnTheWebAsync() =>
        Assert.Empty(await CompareAsync(new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["calendar-single"] = 0.99, ["calendar-range"] = 0.99, ["calendar-multiple"] = 0.99, ["calendar-disabled"] = 0.99, ["calendar-outside-hidden"] = 0.99, ["calendar-dropdown"] = 0.98, ["calendar-week-numbers"] = 0.99,
        }, heights: true));

    [Fact]
    [Trait("AC", "FIDELITY-09")]
    public async Task EveryCompositeFixtureMatchesShadcnsRenderAsync() =>
        Assert.Empty(await CompareAsync(GalleryHost.CompositeFixtures.ToDictionary(fixture => fixture.Fixture, fixture => fixture.Floor, StringComparer.Ordinal), heights: false));

    [Fact]
    [Trait("AC", "FIDELITY-14")]
    public async Task EveryChartFixtureMatchesShadcnsRenderOnTheWebAsync() =>
        Assert.Empty(await CompareAsync(GalleryHost.ChartFixtures.ToDictionary(fixture => fixture.Fixture, fixture => fixture.Floor, StringComparer.Ordinal), heights: false));

    [Fact]
    [Trait("AC", "FIDELITY-16")]
    public async Task EveryBlockMatchesShadcnsRenderOnTheWebAsync() =>
        Assert.Empty(await CompareAsync(GalleryHost.BlockFixtures.ToDictionary(fixture => fixture.Fixture, fixture => fixture.Floor, StringComparer.Ordinal), heights: false, viewport: 1100));

    [Fact]
    [Trait("AC", "FIDELITY-22")]
    public async Task EveryShellMatchesShadcnsRenderOnTheWebAsync() =>
        Assert.Empty(await CompareAsync(GalleryHost.ShellFixtures.ToDictionary(fixture => fixture.Fixture, fixture => fixture.Floor, StringComparer.Ordinal), viewport: 800, width: 1560));

    [Fact]
    [Trait("AC", "FIDELITY-18")]
    public async Task EveryChatFixtureMatchesShadcnsRenderOnTheWebAsync() =>
        Assert.Empty(await CompareAsync(GalleryHost.ChatFixtures.ToDictionary(fixture => fixture.Fixture, fixture => fixture.Floor, StringComparer.Ordinal), heights: false));

    [Fact]
    [Trait("AC", "FIDELITY-20")]
    public async Task EveryRheaFixtureMatchesShadcnsRenderOnTheWebAsync() =>
        Assert.Empty(await CompareAsync(GalleryHost.RheaFixtures.ToDictionary(fixture => fixture.Fixture, fixture => fixture.Floor, StringComparer.Ordinal), heights: false, viewport: 1300));

    [Fact]
    [Trait("AC", "FIDELITY-05")]
    public async Task EveryOverlayFixtureOpenMatchesShadcnsRenderAsync()
    {
        var verdicts = new List<string>();
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var (fixture, opening, floor) in GalleryHost.OverlayFixtures)
            {
                var page = await browser.OverlayAsync(gallery.Address, fixture, theme, opening);
                var shot = await (await page.QuerySelectorAsync("#fixture"))!.ScreenshotAsync(new() { Animations = await Browser.StillAsync(page) });
                await page.Context.CloseAsync();
                using var ours = SKBitmap.Decode(shot);
                using var reference = Pixels.Reference(fixture, theme);
                var matched = Pixels.Matched(ours, reference);
                verdicts.Add($"{fixture}.{theme}: {matched:P2}");
                if (ours.Width != reference.Width || ours.Height != reference.Height || matched < floor)
                {
                    wrong.Add($"{fixture}.{theme}: {ours.Width}x{ours.Height} (shadcn {reference.Width}x{reference.Height}), {matched:P2} of pixels within {Pixels.Tolerance}/255, floor {floor:P0}");
                }
            }
        }

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join(Environment.NewLine, verdicts));
        Assert.Empty(wrong);
    }

    /// <summary>
    /// Captures each fixture in both themes and gives the ones that are not as wide as shadcn/ui's capture, differ in height
    /// by more than 8 px (the labels' line heights, framework need 12) or match fewer pixels than their floor.
    /// </summary>
    private async Task<List<string>> CompareAsync(IReadOnlyDictionary<string, double> floors, bool heights = true, int viewport = 900, int width = 1000)
    {
        var verdicts = new List<string>();
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var (fixture, floor) in floors)
            {
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme, viewport, width);
                var shot = await (await page.QuerySelectorAsync("#fixture"))!.ScreenshotAsync(new() { Animations = await Browser.StillAsync(page) });
                await page.Context.CloseAsync();
                if (Environment.GetEnvironmentVariable("COSMO_CAPTURES") is { Length: > 0 } folder)
                {
                    await File.WriteAllBytesAsync(Path.Combine(folder, $"{fixture}.{theme}.web.png"), shot, TestContext.Current.CancellationToken);
                }

                using var ours = SKBitmap.Decode(shot);
                using var reference = SKBitmap.Decode(Repository.Path("tools", "shadcn", "out", $"{fixture}.{theme}.png"));
                var matched = Pixels.Matched(ours, reference);
                verdicts.Add($"{fixture}.{theme}: {matched:P2}");
                if (ours.Width != reference.Width || (heights && Math.Abs(ours.Height - reference.Height) > 8) || matched < floor)
                {
                    wrong.Add($"{fixture}.{theme}: {ours.Width}x{ours.Height} (shadcn {reference.Width}x{reference.Height}), {matched:P2} of pixels within {Pixels.Tolerance}/255, floor {floor:P0}");
                }
            }
        }

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join(Environment.NewLine, verdicts));
        return wrong;
    }
}
