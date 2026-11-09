using System.Net;
using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class ThemeBindingWebTests(GalleryHost gallery, Browser browser)
{
    [Fact]
    [Trait("AC", "THEME-05")]
    public async Task TheCustomizerPreviewsTheChosenColorAndRadiusAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1440);
        await page.GotoAsync(new Uri(gallery.Address, "themes?color=blue&radius=none").ToString());
        var found = await page.EvaluateAsync<string[]>("""
            (() => {
              const button = [...document.querySelectorAll("button")].find((b) => b.textContent.trim() === "Deploy")
              const heading = document.querySelector("h1")
              const chip = (text) => [...document.querySelectorAll("a")].find((a) => a.textContent.trim() === text)
              const style = (el) => getComputedStyle(el)
              return [style(button).backgroundColor, style(button).borderTopLeftRadius, style(heading).color, style(chip("Blue")).borderTopWidth, style(chip("Neutral")).borderTopWidth, style(chip("0")).borderTopWidth, style(chip("0.625")).borderTopWidth]
            })()
            """);
        await page.Context.CloseAsync();

        Assert.Equal(["rgb(20, 71, 230)", "0px", "rgb(10, 10, 10)", "2px", "1px", "2px", "1px"], found);
    }

    [Fact]
    [Trait("AC", "THEME-06")]
    public async Task TheThemeSwitchRendersTheDarkAndTheLightTokensAsync()
    {
        var cancel = TestContext.Current.CancellationToken;
        using var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
        using var visitor = new HttpClient(handler) { BaseAddress = gallery.Address };

        (await visitor.GetAsync("theme/dark", cancel)).EnsureSuccessStatusCode();
        var dark = await LooksAsync(visitor, await visitor.GetStringAsync("fidelity/button-variants", cancel), cancel);
        (await visitor.GetAsync("theme/light", cancel)).EnsureSuccessStatusCode();
        var light = await LooksAsync(visitor, await visitor.GetStringAsync("fidelity/button-variants", cancel), cancel);

        Assert.Equal(("#0a0a0a", "#e5e5e5", "#171717"), dark);
        Assert.Equal(("#ffffff", "#171717", "#fafafa"), light);
    }

    private static async Task<(string Body, string Background, string Color)> LooksAsync(HttpClient visitor, string html, CancellationToken cancel)
    {
        var body = BodyBackground().Match(html).Groups[1].Value;
        var classes = FirstButton().Match(html).Groups[1].Value.Split(' ');
        var css = string.Concat(StyleBlock().Matches(html).Select(match => match.Groups[1].Value));
        foreach (var sheet in StyleSheet().Matches(html).Select(match => match.Groups[1].Value))
        {
            css += await visitor.GetStringAsync(sheet.TrimStart('/'), cancel);
        }

        var rule = classes.Select(name => Regex.Match(css, @"\." + name + @"\{([^}]*)\}", RegexOptions.None, TimeSpan.FromSeconds(1))).First(match => match.Success && match.Groups[1].Value.Contains("height:36px", StringComparison.Ordinal)).Groups[1].Value;
        return (body, Regex.Match(rule, "background:(#[0-9a-f]+)", RegexOptions.None, TimeSpan.FromSeconds(1)).Groups[1].Value, Regex.Match(rule, ";color:(#[0-9a-f]+)", RegexOptions.None, TimeSpan.FromSeconds(1)).Groups[1].Value);
    }

    [GeneratedRegex("<link rel=\"stylesheet\" href=\"([^\"]+)\"")]
    private static partial Regex StyleSheet();

    [GeneratedRegex("<body style=\"background:(#[0-9a-f]+)")]
    private static partial Regex BodyBackground();

    [GeneratedRegex("id=\"fixture\".*?<button[^>]*class=\"([^\"]+)\"", RegexOptions.Singleline)]
    private static partial Regex FirstButton();

    [GeneratedRegex("<style[^>]*>(.*?)</style>", RegexOptions.Singleline)]
    private static partial Regex StyleBlock();

    private async Task<Microsoft.Playwright.IPage> BindingPageAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "binding").ToString());
        return page;
    }

    private static Task<string> SeatsBadgeAsync(Microsoft.Playwright.IPage page) => page.EvaluateAsync<string>("""
        (() => { const badge = [...document.querySelectorAll("div")].find((d) => parseFloat(getComputedStyle(d).borderTopLeftRadius) >= 9999 && /seats$/.test(d.textContent.trim()));
          return badge.textContent.trim() + " " + getComputedStyle(badge).backgroundColor })()
        """);

    [Fact]
    [Trait("AC", "BINDING-01")]
    public async Task APostRunsTheCommandWithItsParameterAfterTheTwoWayFieldsAsync()
    {
        var page = await BindingPageAsync();
        await page.FillAsync("input[aria-label='Team name']", "Rocket");
        await page.FillAsync("input[aria-label='Seats']", "8");
        await page.ClickAsync("button:text-is('+5')");
        await page.WaitForSelectorAsync("text=Team Rocket");
        var found = (await page.InnerTextAsync("form"), await page.InputValueAsync("input[aria-label='Seats']"), await SeatsBadgeAsync(page));
        await page.Context.CloseAsync();

        Assert.Contains("$156 per month", found.Item1, StringComparison.Ordinal);
        Assert.Equal("13", found.Item2);
        Assert.StartsWith("13 seats", found.Item3, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "BINDING-07")]
    public async Task ABoundCopyValueFollowsTheModelAsync()
    {
        var page = await BindingPageAsync();
        const string Copies = "document.querySelector(\"button[aria-label='Copy the total']\").getAttribute('data-aioros-copies')";
        var served = await page.EvaluateAsync<string?>(Copies);
        var planned = await page.EvaluateAsync<bool>("!!document.querySelector('meta[name=aioros-plan]')");
        await page.ClickAsync("button[aria-label='One seat more']");
        await page.WaitForSelectorAsync("text=4 seats");
        var posted = await page.EvaluateAsync<string?>(Copies);
        await page.Context.CloseAsync();

        Assert.Equal(("$36 per month", "$48 per month", true), (served, posted, planned));
    }

    [Fact]
    [Trait("AC", "BINDING-03")]
    public async Task AConverterChoosesTheBadgesVariantAsync()
    {
        var page = await BindingPageAsync();
        await page.FillAsync("input[aria-label='Seats']", "8");
        await page.ClickAsync("button[aria-label='One seat more']");
        await page.WaitForSelectorAsync("text=9 seats");
        var nine = await SeatsBadgeAsync(page);
        await page.ClickAsync("button:text-is('+5')");
        await page.WaitForSelectorAsync("text=14 seats");
        var fourteen = await SeatsBadgeAsync(page);
        await page.Context.CloseAsync();

        Assert.Equal("9 seats rgb(23, 23, 23)", nine);
        Assert.Equal("14 seats rgb(231, 0, 11)", fourteen);
    }

    [Fact]
    [Trait("AC", "BINDING-04")]
    public async Task CanExecuteEnablesTheCommandOnceTheTermsAreAcceptedAsync()
    {
        var page = await BindingPageAsync();
        var before = await page.IsDisabledAsync("button:text-is('Send invites')");
        await page.FillAsync("input[aria-label='Team name']", "Rocket");
        await page.FillAsync("input[aria-label='Seats']", "13");
        await page.CheckAsync("input[type=checkbox]");
        await page.ClickAsync("button:text-is('+5')");
        await page.WaitForSelectorAsync("text=18 seats");
        var enabled = !await page.IsDisabledAsync("button:text-is('Send invites')");
        await page.ClickAsync("button:text-is('Send invites')");
        await page.WaitForSelectorAsync("text=Invited 18 people to Rocket");
        await page.Context.CloseAsync();

        Assert.True(before);
        Assert.True(enabled);
    }

    [Fact]
    [Trait("AC", "BINDING-05")]
    public async Task ABoundBadgeShowsItsContentTemplateAsync()
    {
        var html = await gallery.Client.GetStringAsync("binding", TestContext.Current.CancellationToken);

        Assert.Contains(">Pro<", html, StringComparison.Ordinal);
        Assert.Contains(">$12 per seat<", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Plan {", html, StringComparison.Ordinal);
    }
}
