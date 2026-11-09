namespace Aioros.Cosmo.Gallery.Tests;

public sealed class DatePickerWebTests(GalleryHost gallery, Browser browser)
{
    private const string Field = "#fixture [data-aioros-picker] input:not([type=hidden])";

    private const string Open = "#fixture [popover]:popover-open";

    [Fact]
    [Trait("AC", "DATEPICKER-02")]
    public async Task TheDatePickerOpensItsCalendarAndPostsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            wrong.AddRange(await DemoAsync(theme));
            wrong.AddRange(await OpenAsync(theme));
            wrong.AddRange(await InputAsync(theme));
        }

        Assert.Empty(wrong);
    }

    private async Task<List<string>> DemoAsync(string theme)
    {
        var page = await browser.FixtureAsync(gallery.Address, "date-picker-demo", theme);
        var native = await page.Locator("#fixture input[type=date]").CountAsync();
        var placeholder = await page.Locator(Field).GetAttributeAsync("placeholder");
        await page.Context.CloseAsync();
        return native == 0 && placeholder == "Pick a date" ? [] : [$"{theme} demo: {native} native inputs, placeholder {placeholder}"];
    }

    private async Task<List<string>> OpenAsync(string theme)
    {
        var page = await browser.FixtureAsync(gallery.Address, "date-picker-open", theme);
        var requests = 0;
        page.Request += (_, request) => requests += request.Method == "POST" ? 1 : 0;
        var shown = await page.Locator(Field).InputValueAsync();
        await page.ClickAsync("#fixture button[popovertarget]");
        var caption = (await page.Locator(Open + " [data-aioros-calendar] button").Nth(1).InnerTextAsync()).Trim();
        var opened = (await page.Locator(Open).CountAsync(), requests);
        var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await page.ClickAsync(Open + " td button:text-is('18')");
        await answered;
        await page.WaitForFunctionAsync($"() => document.querySelector('{Field}').value === 'June 18th, 2025'", null, new() { PollingInterval = 20 });
        var closed = await page.Locator(Open).CountAsync();
        await page.Context.CloseAsync();
        return shown == "June 12th, 2025" && caption == "June 2025" && opened == (1, 0) && closed == 0
            ? []
            : [$"{theme} open: shown {shown}, caption {caption}, opened {opened}, popovers open after the day {closed}"];
    }

    private async Task<List<string>> InputAsync(string theme)
    {
        var page = await browser.FixtureAsync(gallery.Address, "date-picker-input", theme);
        var posts = new List<string>();
        page.Request += (_, request) =>
        {
            if (request.Method == "POST")
            {
                posts.Add(request.PostData ?? string.Empty);
            }
        };
        var shown = await page.Locator(Field).InputValueAsync();
        await page.FillAsync(Field, "June 20, 2025");
        var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await page.PressAsync(Field, "Enter");
        await answered;
        var kept = await page.Locator(Field).InputValueAsync();
        await page.Context.CloseAsync();
        return shown == "June 01, 2025" && kept == "June 20, 2025" && posts is [var post] && post.Contains("_chg=", StringComparison.Ordinal)
            ? []
            : [$"{theme} input: shown {shown}, kept {kept}, posts {string.Join(" / ", posts)}"];
    }
}
