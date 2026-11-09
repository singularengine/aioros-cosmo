using System.Globalization;
using Aioros.Cosmo.Dates;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class DatePostsWebTests(GalleryHost gallery, Browser browser)
{
    private const string Open = "#fixture [popover]:popover-open";

    private static Task<IResponse> PostAsync(IPage page) => page.WaitForResponseAsync(response => response.Request.Method == "POST");

    [Fact]
    [Trait("AC", "CALENDAR-10")]
    public async Task SeveralDaysToggleAcrossPostsOnTheWebAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "calendar-multiple", "light");
        Task<string> SelectedAsync() => page.EvaluateAsync<string>("() => [...document.querySelectorAll('#fixture td[aria-selected=true] button')].map((b) => b.textContent).join(',')");
        var steps = new List<string> { await SelectedAsync() };
        foreach (var day in (string[])["24", "25"])
        {
            var answered = PostAsync(page);
            await page.Locator($"#fixture td button:text-is('{day}')").First.ClickAsync();
            await answered;
            await page.WaitForFunctionAsync($"() => [...document.querySelectorAll('#fixture td button')].some((b) => b.textContent === '{day}' && b.closest('td').getAttribute('aria-selected') === {(day == "24" ? "null" : "'true'")})", null, new() { PollingInterval = 20 });
            steps.Add(await SelectedAsync());
        }

        await page.Context.CloseAsync();
        Assert.Equal(["3,12,13,24", "3,12,13", "3,12,13,25"], steps);
    }

    [Fact]
    [Trait("AC", "DATEPICKER-05")]
    public async Task TheRangeStaysOpenAndKeepsItsEndsOnTheWebAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "date-picker-range-open", "light");
        await page.ClickAsync("#fixture [popovertarget]");
        var steps = new List<string>();
        foreach (var (day, nth, text) in new[] { ("25", 0, "Jan 20, 2022 - Jan 25, 2022"), ("14", 1, "Jan 20, 2022 - Feb 14, 2022") })
        {
            var answered = PostAsync(page);
            await page.Locator($"{Open} td button:text-is('{day}')").Nth(nth).ClickAsync();
            await answered;
            await page.WaitForFunctionAsync($"() => document.querySelector('#fixture [popovertarget]').textContent.includes('{text}')", null, new() { PollingInterval = 20 });
            steps.Add($"{(await page.Locator("#fixture [popovertarget]").InnerTextAsync()).Trim()} {await page.Locator(Open).CountAsync()}");
        }

        await page.Context.CloseAsync();
        Assert.Equal(["Jan 20, 2022 - Jan 25, 2022 1", "Jan 20, 2022 - Feb 14, 2022 1"], steps);
    }

    [Fact]
    [Trait("AC", "DATEPICKER-06")]
    public async Task APresetPicksTomorrowOnTheWebAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "date-picker-presets-open", "light");
        await page.ClickAsync("#fixture button[popovertarget]");
        var answered = PostAsync(page);
        await page.Locator($"{Open} select").SelectOptionAsync(new SelectOptionValue { Label = "Tomorrow" });
        await answered;
        var tomorrow = DateText.Format(DateTime.Today.AddDays(1), CultureInfo.InvariantCulture);
        await page.WaitForFunctionAsync($"() => [...document.querySelectorAll('#fixture input')].some((i) => i.value === '{tomorrow}')", null, new() { PollingInterval = 20 });
        var open = await page.Locator(Open).CountAsync();
        await page.Context.CloseAsync();
        Assert.Equal(1, open);
    }

    [Fact]
    [Trait("AC", "CALENDAR-11")]
    public async Task CalendarsSpeakTheVisitorsCultureAsync()
    {
        var calendar = await gallery.Client.GetStringAsync("fidelity/calendar-dropdown?culture=es-ES", TestContext.Current.CancellationToken);
        var picker = await gallery.Client.GetStringAsync("fidelity/date-picker-open?culture=es-ES", TestContext.Current.CancellationToken);

        Assert.Contains(">jun</option>", calendar, StringComparison.Ordinal);
        Assert.Contains(">LU<", calendar.ToUpperInvariant(), StringComparison.Ordinal);
        Assert.Contains("junio de 2025", picker, StringComparison.Ordinal);
        Assert.Contains("value=\"12 de junio de 2025\"", picker, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "CALENDAR-12")]
    public async Task BoundCalendarsAreServedFromTheirPlanAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light");
        var served = await page.GotoAsync(new Uri(gallery.Address, "components/calendar").ToString());
        Task<string[]> SelectedAsync() => page.EvaluateAsync<string[]>("() => [...document.querySelectorAll('[data-aioros-calendar]')].map((c) => [...c.querySelectorAll('td[aria-selected=true] button')].map((b) => b.textContent).join(','))");
        var planned = await page.Locator("meta[name=aioros-plan]").CountAsync();
        var before = await SelectedAsync();
        var answered = PostAsync(page);
        await page.Locator("[data-aioros-calendar] >> nth=0").Locator("td button:text-is('20')").First.ClickAsync();
        await answered;
        await page.WaitForFunctionAsync("() => document.querySelector('[data-aioros-calendar] td[aria-selected=true] button')?.textContent === '20'", null, new() { PollingInterval = 20 });
        var after = await SelectedAsync();
        await page.Context.CloseAsync();

        Assert.NotNull(served);
        Assert.Equal(1, planned);
        Assert.Equal(["12", "9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26", "3,12,13,24"], before[..3]);
        Assert.Equal(["20", .. before[1..]], after);
    }
}
