using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using SkiaSharp;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class GalleryTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] CardTexts = ["Total Revenue", "New Customers", "Active Accounts", "Growth Rate", "+12.5%", "-20%"];

    [Fact]
    [Trait("AC", "GALLERY-01")]
    public async Task EveryPageOfTheGalleryPrecompilesInBothThemesAsync()
    {
        var plan = Path.Combine(Path.GetTempPath(), $"cosmo-precompile-{Guid.NewGuid():N}.json");
        using var process = GalleryHost.Start(GalleryHost.Application, "build", "--precompile=" + plan);
        var output = await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        File.Delete(plan);

        var pages = Precompiled().Match(output);
        var identical = Identical().Match(output);
        Assert.Equal(0, process.ExitCode);
        Assert.True(pages.Success, output);
        Assert.Equal(pages.Groups[1].Value, pages.Groups[2].Value);
        Assert.All(GalleryHost.Routes, route => Assert.Matches("precompiled  " + Regex.Escape(route) + @"(  \(.*\))?\n", output));
        Assert.True(identical.Success, output);
        Assert.Equal(identical.Groups[1].Value, identical.Groups[2].Value);
        Assert.Equal(2 * GalleryHost.Cultures.Length * int.Parse(pages.Groups[1].Value, CultureInfo.InvariantCulture), int.Parse(identical.Groups[1].Value, CultureInfo.InvariantCulture));
        Assert.Matches(@"Production models: (\d+) of \1 pages load their models as production does", output);
        Assert.DoesNotContain("AIOROS032", output, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "GALLERY-15")]
    public void NoGalleryPageNeedsAvaloniaForItsModels()
    {
        var registry = typeof(global::Gallery.Models.ShellExample).Assembly.GetType("Aioros.Generated.AiorosPages", throwOnError: true);
        var pages = (System.Collections.Immutable.ImmutableArray<Aioros.Pages.PageDefinition>)registry!.GetProperty("All")!.GetValue(null)!;

        Assert.NotEmpty(pages);
        Assert.Empty(pages.Where(page => page.ModelsUseAvalonia).Select(page => page.Route));
    }

    [GeneratedRegex(@"Precompiled (\d+) of (\d+) pages")]
    private static partial Regex Precompiled();

    [GeneratedRegex(@"Identical output: (\d+) of (\d+)")]
    private static partial Regex Identical();

    [Fact]
    [Trait("AC", "GALLERY-02")]
    public async Task EveryPageAnswersAndIsMarkedCurrentAsync()
    {
        var cancel = TestContext.Current.CancellationToken;
        foreach (var route in GalleryHost.Routes)
        {
            using var answer = await gallery.Client.GetAsync(route.TrimStart('/'), cancel);
            var html = await answer.Content.ReadAsStringAsync(cancel);
            Assert.Equal(System.Net.HttpStatusCode.OK, answer.StatusCode);
            Assert.Matches("<title>[^<]*Cosmo</title>", html);
            var current = Current().Matches(html).Select(match => match.Groups[1].Value).Distinct().ToList();
            Assert.Equal([route], current);
        }
    }

    [GeneratedRegex("<a href=\"([^\"]+)\"[^>]*aria-current=\"page\"")]
    private static partial Regex Current();

    [Fact]
    [Trait("AC", "GALLERY-03")]
    public async Task OnAPhoneTheNavigationCollapsesBehindTheMenuAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 390, 844);
        await page.GotoAsync(new Uri(gallery.Address, "components/button").ToString());
        var closed = (await page.IsVisibleAsync("#nav a[href='/components/card']"), await page.IsVisibleAsync("button[aria-label='Menu']"));
        var overflow = await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth");
        await page.ClickAsync("button[aria-label='Menu']");
        await page.WaitForSelectorAsync("#nav a[href='/components/card']", new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });
        await page.Context.CloseAsync();

        Assert.Equal((false, true), closed);
        Assert.False(overflow);
    }

    [Fact]
    [Trait("AC", "GALLERY-04")]
    public async Task TheDesktopWindowIsCapturedHeadlessAsync()
    {
        var capture = Path.Combine(Path.GetTempPath(), $"cosmo-desktop-{Guid.NewGuid():N}.png");
        var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            WorkingDirectory = GalleryHost.ProjectFolder,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in (string[])[GalleryHost.Application, "--desktop", "--path=/components/button"])
        {
            start.ArgumentList.Add(argument);
        }

        start.Environment["AIOROS_DESKTOP_HEADLESS"] = "1";
        start.Environment["AIOROS_DESKTOP_CAPTURE"] = capture;
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var errors = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        Assert.True(File.Exists(capture), await output + await errors);
        using var bitmap = SKBitmap.Decode(capture);
        File.Delete(capture);

        Assert.True(bitmap.Width >= 1024 && bitmap.Height >= 600, $"{bitmap.Width}x{bitmap.Height}");
        Assert.Equal(new SKColor(0xFA, 0xFA, 0xFA), bitmap.GetPixel(200, bitmap.Height - 20));
        Assert.Equal(new SKColor(0xFF, 0xFF, 0xFF), bitmap.GetPixel(bitmap.Width - 40, bitmap.Height - 20));
    }

    [Fact]
    [Trait("AC", "GALLERY-05")]
    public async Task TheDashboardShowsItsCardsTabsAndPagedTableAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1440);
        var url = new Uri(gallery.Address, "examples/dashboard").ToString();
        await page.GotoAsync(url);
        var main = page.Locator("main");
        var crumbs = await main.Locator(":text-is('Dashboard')").CountAsync();
        var cards = await Task.WhenAll(CardTexts.Select(text => main.Locator($":text-is('{text}')").CountAsync()));
        var tabs = await main.Locator("[role=tab]:has-text('Past Performance'), [role=tab]:has-text('Key Personnel'), [role=tab]:has-text('Focus Documents'), [role=tab]:has-text('Outline')").CountAsync();
        var rows = await main.Locator("table tbody tr").AllInnerTextsAsync();
        var first = (rows.Count, rows[0].Contains("Cover page", StringComparison.Ordinal) && rows[0].Contains("In Process", StringComparison.Ordinal), await main.Locator(":text-is('Page 1 of 4')").CountAsync());
        await main.Locator("[role=tab]:has-text('Past Performance')").ClickAsync();
        await page.WaitForSelectorAsync("main :text-is('Showing: Past Performance')");
        var switched = await main.Locator("text=Past performance documents appear here.").IsVisibleAsync();
        await page.GotoAsync(url);
        await page.ClickAsync("main button[aria-label='Go to next page']");
        await page.WaitForSelectorAsync("main :text-is('Page 2 of 4')");
        var second = await main.Locator("table tbody tr").First.InnerTextAsync();
        await page.GotoAsync(url);
        await page.ClickAsync("main button:has-text('Add Section')");
        await page.WaitForSelectorAsync("main :text-is('21 sections')");
        var added = (await main.Locator(":text-is('Page 5 of 5')").CountAsync(), await main.Locator("table tbody tr").Last.InnerTextAsync());
        await page.Context.CloseAsync();

        Assert.True(crumbs > 0 && cards.All(count => count > 0) && tabs == 4, $"{crumbs} {string.Join(",", cards)} {tabs}");
        Assert.Equal((5, true, 1), first);
        Assert.True(switched);
        Assert.Contains("Capabilities", second, StringComparison.Ordinal);
        Assert.Equal(1, added.Item1);
        Assert.Contains("New section", added.Item2, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "GALLERY-06")]
    public async Task TheWorkspaceIsACompleteAppLayoutAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1440);
        await page.GotoAsync(new Uri(gallery.Address, "examples/workspace").ToString());
        var main = page.Locator("main");
        var crumbs = (await main.Locator(":text-is('Workspace')").CountAsync(), await main.Locator(":text-is('Payments')").CountAsync());
        var cards = await Task.WhenAll(((string[])["Revenue", "Selected", "$4,568.00"]).Select(text => main.Locator($":text-is('{text}')").CountAsync()));
        var rows = await main.Locator("table tbody tr td:nth-child(3)").AllInnerTextsAsync();
        var summary = await main.Locator(":text-is('0 of 12 row(s) selected.')").CountAsync();
        await main.Locator("button:has-text('Next')").Last.ClickAsync();
        await page.WaitForSelectorAsync("main td:has-text('olivia@example.com')");
        await page.Context.CloseAsync();
        var capture = Path.Combine(Path.GetTempPath(), $"cosmo-workspace-{Guid.NewGuid():N}.png");
        var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet") { WorkingDirectory = GalleryHost.ProjectFolder, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in (string[])[GalleryHost.Application, "--desktop", "--path=/examples/workspace"])
        {
            start.ArgumentList.Add(argument);
        }

        start.Environment["AIOROS_DESKTOP_HEADLESS"] = "1";
        start.Environment["AIOROS_DESKTOP_CAPTURE"] = capture;
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var errors = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        Assert.True(File.Exists(capture), await output + await errors);
        using var bitmap = SKBitmap.Decode(capture);
        File.Delete(capture);
        var column = Enumerable.Range(260, 400).Where(x => bitmap.GetPixel(x, bitmap.Height - 40) == new SKColor(0xFA, 0xFA, 0xFA)).ToList();

        Assert.True(crumbs.Item1 > 0 && crumbs.Item2 > 0 && cards.All(count => count > 0) && summary == 1, $"{crumbs} {string.Join(',', cards)} {summary}");
        Assert.Equal("ken99@example.com", rows[0]);
        Assert.True(column.Count is >= 250 and <= 258, $"{column.Count} px of --sidebar");
        Assert.Equal(new SKColor(0xFF, 0xFF, 0xFF), bitmap.GetPixel(column.Max() + 4, bitmap.Height - 40));
    }
}
