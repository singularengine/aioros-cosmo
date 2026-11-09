using System.Text.RegularExpressions;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class HomeWebTests(GalleryHost gallery, Browser browser)
{
    [Fact]
    [Trait("AC", "GALLERY-07")]
    public async Task TheHomePageLeadsWithTheBrandsAndShowsTheLibraryAsync()
    {
        var html = await gallery.Client.GetStringAsync(string.Empty, TestContext.Current.CancellationToken);
        var start = html.IndexOf("<main", StringComparison.Ordinal);
        var main = html[start..html.IndexOf("</main>", start, StringComparison.Ordinal)];
        var texts = Tag().Replace(main, " ");
        var nav = html[..start];

        Assert.Contains("<title>Aioros.Cosmo</title>", html, StringComparison.Ordinal);
        Assert.Matches(">Aioros.Cosmo<", main);
        Assert.Contains("The XAML framework for web and native apps", texts, StringComparison.Ordinal);
        Assert.Single(Regex.Matches(texts, "Avalonia", RegexOptions.None, TimeSpan.FromSeconds(1)));
        Assert.DoesNotContain("shadcn", texts, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(">Aioros.Cosmo<", nav, StringComparison.Ordinal);
        Assert.DoesNotContain("shadcn/ui for XAML", nav, StringComparison.Ordinal);
        Assert.All((string[])["/installation", "/components", "/blocks/login-01", "/fidelity"], link => Assert.Contains($"href=\"{link}\"", main, StringComparison.Ordinal));
        Assert.Contains("recharts-surface", main, StringComparison.Ordinal);
        Assert.Contains("<input type=\"email\"", main, StringComparison.Ordinal);
        Assert.Contains("<input type=\"password\"", main, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "GALLERY-08")]
    public async Task TheHomePageFitsWideAndNarrowViewsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var (width, columns) in new[] { (1280, 3), (390, 1) })
            {
                var page = await browser.PageAsync(gallery.Address, theme, width, 900);
                await page.GotoAsync(gallery.Address.ToString());
                var (scroll, lefts) = await page.EvaluateAsync<double[]>("""
                    () => {
                      const cards = [...document.querySelectorAll("#showcase > *")].map((el) => Math.round(el.getBoundingClientRect().x))
                      return [document.documentElement.scrollWidth - document.documentElement.clientWidth, new Set(cards).size]
                    }
                    """) is [var overflow, var distinct] ? (overflow, distinct) : (-1, -1);
                await page.Context.CloseAsync();
                wrong.AddRange(scroll == 0 ? [] : [$"{theme} {width}: {scroll} px of horizontal scroll"]);
                wrong.AddRange(lefts == columns ? [] : [$"{theme} {width}: the showcase in {lefts} columns, expected {columns}"]);
            }
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "GALLERY-12")]
    public async Task TheVisitorsCardFillsItsRowAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1440, 900);
        await page.GotoAsync(gallery.Address.ToString());
        var found = await page.EvaluateAsync<string[]>("""
            () => {
              const [visitors, login] = document.querySelectorAll("#showcase > *")
              const card = visitors.getBoundingClientRect(), padding = parseFloat(getComputedStyle(visitors).paddingBottom)
              const last = Math.max(...[...visitors.querySelectorAll("*")].filter((element) => !element.closest("svg") && element.getBoundingClientRect().height > 0).map((element) => element.getBoundingClientRect().bottom))
              return [String(card.bottom - padding - last), String(card.height - login.getBoundingClientRect().height), visitors.innerText, String([...visitors.querySelectorAll(".recharts-wrapper")].map((wrapper) => wrapper.getBoundingClientRect().height).find((height) => height > 0))]
            }
            """);
        await page.Context.CloseAsync();

        Assert.True(double.Parse(found[0], System.Globalization.CultureInfo.InvariantCulture) <= 32, $"{found[0]} px left under the Visitors card's content");
        Assert.Equal("0", found[1]);
        Assert.Contains("Trending up by 5.2% this month", found[2], StringComparison.Ordinal);
        Assert.Contains("January - June 2024", found[2], StringComparison.Ordinal);
        Assert.Contains("Mobile", found[2], StringComparison.Ordinal);
        Assert.Equal("255", found[3]);
    }

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex Tag();
}
