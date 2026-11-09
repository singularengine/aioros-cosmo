using System.Text.RegularExpressions;
using Avalonia.Input;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class GalleryPagesWebTests(GalleryHost gallery, Browser browser)
{
    private const string Cut = """
        () => {
          const view = document.documentElement.clientWidth, cut = []
          const clip = (element) => {
            for (let parent = element.parentElement; parent && parent !== document.body; parent = parent.parentElement) {
              const overflow = getComputedStyle(parent).overflowX
              if (overflow !== "visible") return ["auto", "scroll"].includes(overflow) ? null : parent.getBoundingClientRect()
            }
            return { left: 0, right: view }
          }
          const outside = (box, area) => box.left < area.left - 1 || box.right > area.right + 1
          for (const element of document.querySelectorAll("main *")) {
            const box = element.getBoundingClientRect()
            if (element.closest("svg") || box.width < 1 || box.height < 1 || getComputedStyle(element).visibility === "hidden") continue
            const area = clip(element)
            if (!area || !outside(box, area) || outside(element.parentElement.getBoundingClientRect(), area)) continue
            cut.push(`${(element.innerText || element.getAttribute("aria-label") || element.tagName).trim().replace(/\s+/g, " ").slice(0, 40)} ${Math.round(box.left)}..${Math.round(box.right)} in ${Math.round(area.left)}..${Math.round(area.right)}`)
          }
          return cut
        }
        """;

    private const string Switcher = """
        () => {
          const channels = (color) => color.match(/[\d.]+/g).map(Number)
          const luminance = ([r, g, b]) => [r, g, b].map((c) => c / 255).map((c) => c <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4).reduce((sum, c, i) => sum + c * [0.2126, 0.7152, 0.0722][i], 0)
          const over = (fore, back) => { const a = fore.length > 3 ? fore[3] : 1; return [0, 1, 2].map((i) => fore[i] * a + back[i] * (1 - a)) }
          return ["Dark", "System"].flatMap((name) => {
            const label = [...document.querySelectorAll("a")].find((link) => link.textContent.trim() === name)
            let back = label
            while (back && channels(getComputedStyle(back).backgroundColor).length > 3 && channels(getComputedStyle(back).backgroundColor)[3] === 0) back = back.parentElement
            const ground = channels(getComputedStyle(back).backgroundColor).slice(0, 3)
            return [label.querySelector("div:not(:has(*))"), label.querySelector("svg")].map((part) => {
              const color = over(channels(getComputedStyle(part).color), ground), light = [luminance(color), luminance(ground)].sort((x, y) => y - x)
              return `${name} ${part.tagName} ${getComputedStyle(part).color} ${((light[0] + 0.05) / (light[1] + 0.05)).toFixed(2)}`
            })
          })
        }
        """;

    private async Task<List<string>> ComponentsAsync()
    {
        var html = await gallery.Client.GetStringAsync(string.Empty, TestContext.Current.CancellationToken);
        return [.. Navigated().Matches(html[..html.IndexOf("<main", StringComparison.Ordinal)]).Select(static match => match.Groups[1].Value).Distinct()];
    }

    [Fact]
    [Trait("AC", "GALLERY-09")]
    public async Task EveryDemoFitsAPhoneAsync()
    {
        var wrong = new List<string>();
        var page = await browser.PageAsync(gallery.Address, "light", 390, 844);
        foreach (var route in await ComponentsAsync())
        {
            await page.GotoAsync(new Uri(gallery.Address, route.TrimStart('/')).ToString());
            wrong.AddRange((await page.EvaluateAsync<string[]>(Cut)).Select(part => $"{route}: {part}"));
        }

        await page.Context.CloseAsync();
        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "GALLERY-10")]
    public async Task TheThemeSwitcherReadsInBothThemesAsync()
    {
        var found = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var theme in (string[])["light", "dark"])
        {
            var page = await browser.PageAsync(gallery.Address, theme, 1440, 900);
            await page.GotoAsync(new Uri(gallery.Address, "components/button").ToString());
            found[theme] = await page.EvaluateAsync<string[]>(Switcher);
            await page.Context.CloseAsync();
        }

        var light = found["light"].Select(static part => (Part: part, Color: Color().Match(part).Value, Ratio: double.Parse(part[(part.LastIndexOf(' ') + 1)..], System.Globalization.CultureInfo.InvariantCulture))).ToList();
        Assert.True(light.All(static part => part.Color == "rgba(10, 10, 10, 0.6)" && part.Ratio >= 4.5), string.Join(Environment.NewLine, found["light"]));
        Assert.True(found["dark"].All(static part => part.Contains("rgb(161, 161, 161)", StringComparison.Ordinal)), string.Join(Environment.NewLine, found["dark"]));
    }

    [Fact]
    [Trait("AC", "GALLERY-11")]
    public async Task TheComponentsIndexLinksEveryComponentAsync()
    {
        var cancel = TestContext.Current.CancellationToken;
        using var answer = await gallery.Client.GetAsync("components", cancel);
        var html = await answer.Content.ReadAsStringAsync(cancel);
        var main = html[html.IndexOf("<main", StringComparison.Ordinal)..];
        var listed = Link().Matches(main).Select(static match => (Route: match.Groups[1].Value, Title: match.Groups[2].Value)).ToList();
        var components = (await ComponentsAsync()).Order(StringComparer.Ordinal).ToList();
        var home = await gallery.Client.GetStringAsync(string.Empty, cancel);

        Assert.Equal(System.Net.HttpStatusCode.OK, answer.StatusCode);
        Assert.Matches("<title>Components[^<]*</title>", html);
        Assert.Equal(components, listed.Select(static link => link.Route).Order(StringComparer.Ordinal));
        Assert.Equal(listed.Select(static link => link.Title).Order(StringComparer.OrdinalIgnoreCase), listed.Select(static link => link.Title));
        Assert.Matches("<a href=\"/components\"[^>]*>(<[^>]+>)*Browse components<", home);
    }

    [Fact]
    [Trait("AC", "GALLERY-13")]
    public async Task TheWorkspaceTeamsShowTheirNumberedShortcutsAsync()
    {
        var html = await gallery.Client.GetStringAsync("examples/workspace", TestContext.Current.CancellationToken);
        var shortcuts = ((string[])["Acme Inc", "Acme Corp.", "Evil Corp."]).Select(team => Regex.Match(html, $"<span class=\"mh\">(?:<[^>]+>)*{Regex.Escape(team)}(?:<[^>]+>)*</span><kbd class=\"kg\"[^>]*>([^<]*)</kbd>").Groups[1].Value);

        Assert.Equal(["Ctrl+1", "Ctrl+2", "Ctrl+3"], shortcuts);
    }

    [Fact]
    [Trait("AC", "GALLERY-14")]
    public async Task NoKeyGestureNamesItsKeyByABareDigitAsync()
    {
        var root = Path.GetFullPath(Path.Combine(GalleryHost.ProjectFolder, "..", "..", ".."));
        var files = Directory.EnumerateFiles(Path.Combine(root, "samples", "Gallery"), "*.axaml", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "tests", "Aioros.Cosmo.Tests"), "*.axaml", SearchOption.AllDirectories))
            .Where(static file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) && !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToList();
        var texts = await Task.WhenAll(files.Select(file => File.ReadAllTextAsync(file, TestContext.Current.CancellationToken)));
        var wrong = files.Zip(texts).SelectMany(file => Gesture().Matches(file.Second)
                .SelectMany(static match => match.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Where(static gesture => gesture.Split('+')[^1].All(char.IsAsciiDigit))
                .Select(gesture => $"{Path.GetRelativePath(root, file.First)}: {gesture} reads as {KeyGesture.Parse(gesture)}"))
            .ToList();

        Assert.NotEmpty(files);
        Assert.Empty(wrong);
    }

    [GeneratedRegex("(?:InputGesture|HotKey|Gesture|ModalKey|Keys)=\"([^\"{][^\"]*)\"")]
    private static partial Regex Gesture();

    [GeneratedRegex("<a href=\"(/components/[a-z-]+)\"[^>]*>(?:<[^>]+>)*([^<]+)<")]
    private static partial Regex Link();

    [GeneratedRegex("<a href=\"(/components/[a-z-]+)\"")]
    private static partial Regex Navigated();

    [GeneratedRegex(@"rgba?\([^)]*\)")]
    private static partial Regex Color();
}
