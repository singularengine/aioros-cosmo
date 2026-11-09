namespace Aioros.Cosmo.Gallery.Tests;

public sealed class DateWebTests(GalleryHost gallery, Browser browser)
{
    private const string Selected = "[...document.querySelectorAll('#fixture td[aria-selected=true]')].map((cell) => cell.textContent.trim()).join(',')";

    [Fact]
    [Trait("AC", "CALENDAR-04")]
    public async Task TheCalendarIsWrittenThroughItsThemeAndPostsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var page = await browser.FixtureAsync(gallery.Address, "calendar-single", theme);
            var days = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture table[role=grid] tbody tr:not([hidden]) button')].map((button) => button.textContent.trim())");
            var stops = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture table[role=grid] button[tabindex=\"0\"]')].map((button) => button.textContent.trim())");
            var june = await page.EvaluateAsync<string>(Selected);
            await page.ClickAsync("#fixture button[aria-label='Go to the Next Month']");
            await page.WaitForFunctionAsync("() => document.querySelector('#fixture')?.textContent.includes('July')", null, new() { PollingInterval = 20 });
            var july = await page.EvaluateAsync<string>(Selected);
            await page.ClickAsync("#fixture table[role=grid] tbody tr:not([hidden]) button:text-is('18')");
            await page.WaitForFunctionAsync($"() => {Selected} === '18'", null, new() { PollingInterval = 20 });
            await page.Context.CloseAsync();
            var first = days.Length > 0 ? days[0] : string.Empty;
            wrong.AddRange(days.Length == 35 && first == "1" && stops.SequenceEqual(["12"]) && june == "12" && july.Length == 0
                ? []
                : [$"{theme}: {days.Length} days from {first}, stops {string.Join(",", stops)}, June {june}, July {july}"]);
        }

        Assert.Empty(wrong);
    }
}
