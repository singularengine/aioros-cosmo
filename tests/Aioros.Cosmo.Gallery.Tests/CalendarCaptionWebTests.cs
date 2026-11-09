using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class CalendarCaptionWebTests(GalleryHost gallery, Browser browser)
{
    [Fact]
    [Trait("AC", "CALENDAR-08")]
    public async Task WeekNumbersAreRowHeadersOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var reference = await ReferenceMetrics.OfAsync("calendar-week-numbers", theme);
            var page = await browser.FixtureAsync(gallery.Address, "calendar-week-numbers", theme);
            var weeks = await page.EvaluateAsync<string[]>("""
                () => [...document.querySelectorAll('#fixture tbody tr:not([hidden]):has(th[scope=row])')].map((row) => {
                  const head = row.querySelector('th[scope=row]'), text = head.firstElementChild ?? head, day = row.querySelector('td button'), h = text.getBoundingClientRect(), d = day.getBoundingClientRect(), s = getComputedStyle(text)
                  return [head.textContent.trim(), Math.round(h.top + h.height / 2 - d.top - d.height / 2), Math.round(head.getBoundingClientRect().width), s.fontSize].join(' ')
                })
                """);
            var width = await page.EvaluateAsync<double>("() => document.querySelector('#fixture [data-aioros-calendar]').parentElement.getBoundingClientRect().width");
            await page.Context.CloseAsync();
            wrong.AddRange(weeks.SequenceEqual(["23 0 32 12.8px", "24 0 32 12.8px", "25 0 32 12.8px", "26 0 32 12.8px", "27 0 32 12.8px"]) ? [] : [$"{theme}: weeks {string.Join(" | ", weeks)}"]);
            wrong.AddRange(Math.Abs(width - reference.Single(element => element.Slot == "calendar").Width) <= 1 ? [] : [$"{theme}: width {width}"]);
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "CALENDAR-09")]
    public async Task TheDropdownCaptionPostsTheChosenMonthAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "calendar-dropdown", "light");
        const string Selects = "() => [...document.querySelectorAll('#fixture select')].map((select) => select.options[select.selectedIndex].text + '/' + select.options.length).join(' ')";
        var before = await page.EvaluateAsync<string>(Selects);
        var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await page.Locator("#fixture select").First.SelectOptionAsync(new Microsoft.Playwright.SelectOptionValue { Label = "Sep" });
        await answered;
        await page.WaitForFunctionAsync("() => document.querySelector('#fixture select').options[document.querySelector('#fixture select').selectedIndex].text === 'Sep'", null, new() { PollingInterval = 20 });
        var after = await page.EvaluateAsync<string>(Selects);
        var first = await page.EvaluateAsync<string>("() => [...document.querySelectorAll('#fixture tbody tr:not([hidden]) td button')][0].textContent.trim()");
        await page.Context.CloseAsync();

        Assert.Equal(("Jun/12 2025/101", "Sep/12 2025/101", "31"), (before, after, first));
    }
}
