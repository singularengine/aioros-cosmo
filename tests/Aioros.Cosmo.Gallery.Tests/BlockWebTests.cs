using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class BlockWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Brands = ["Apple", "Google", "Meta"];

    private static readonly string[] Muted = ["login-03", "login-04", "login-05", "signup-03", "signup-04", "signup-05"];

    private static readonly string[] Blocks = [.. GalleryHost.BlockFixtures.Select(static fixture => fixture.Fixture)];

    private async Task<string> FixtureAsync(string name)
    {
        var html = await gallery.Client.GetStringAsync("fidelity/" + name, TestContext.Current.CancellationToken);
        return html[html.IndexOf("id=\"fixture\"", StringComparison.Ordinal)..];
    }

    private static IEnumerable<string> Boxes(string what, IReadOnlyList<ElementMetrics> found, IReadOnlyList<ElementMetrics> expected) =>
        found.Count != expected.Count
            ? [$"{what}: {found.Count}, shadcn {expected.Count}"]
            : found.Zip(expected).SelectMany((pair, i) => Compare.Box($"{what} #{i} {pair.Second.Text}", pair.First, pair.Second, 1.5));

    [Fact]
    [Trait("AC", "BLOCKS-02")]
    public async Task EveryBlockStandsAtShadcnsBoxesOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var block in Blocks)
            {
                var page = await browser.FixtureAsync(gallery.Address, block, theme);
                var inputs = await Browser.PickAsync(page, "#fixture input:not([type=hidden])");
                var buttons = await Browser.PickAsync(page, "#fixture button");
                await page.Context.CloseAsync();
                wrong.AddRange(Boxes($"{block} {theme} input", inputs, await ReferenceMetrics.SlotsAsync(block, theme, "input")));
                wrong.AddRange(Boxes($"{block} {theme} button", buttons, await ReferenceMetrics.SlotsAsync(block, theme, "button")));
            }
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "BLOCKS-03")]
    public async Task EveryBlockIsAFormOfLabeledFieldsAndLinksAsync()
    {
        var wrong = new List<string>();
        foreach (var block in Blocks)
        {
            wrong.AddRange(Form(block, await FixtureAsync(block), await ReferenceMetrics.SlotsAsync(block, "light", "input")));
        }

        foreach (var (block, verb) in new[] { ("login-04", "Login"), ("signup-04", "Sign up") })
        {
            var fixture = await FixtureAsync(block);
            wrong.AddRange(Brands.Where(brand => !fixture.Contains($"aria-label=\"{verb} with {brand}\"", StringComparison.Ordinal)).Select(brand => $"{block}: no {verb} with {brand}"));
            wrong.AddRange(Image().IsMatch(fixture) ? [] : [$"{block}: no placeholder image"]);
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    private static List<string> Form(string block, string fixture, IReadOnlyList<ElementMetrics> expected)
    {
        var wrong = new List<string>();
        var inputs = Input().Matches(fixture).Select(static match => (Type: match.Groups[1].Value, Id: match.Groups[2].Value)).ToList();
        wrong.AddRange(inputs.Count == expected.Count ? [] : [$"{block}: {inputs.Count} inputs, shadcn {expected.Count}"]);
        wrong.AddRange(inputs.Where(input => input.Type != (input.Id switch { "email" => "email", "name" => "text", _ => "password" })).Select(input => $"{block}: #{input.Id} is {input.Type}"));
        wrong.AddRange(inputs.Where(input => !fixture.Contains($"<label for=\"{input.Id}\"", StringComparison.Ordinal)).Select(input => $"{block}: no label for #{input.Id}"));
        var login = block.StartsWith("login", StringComparison.Ordinal);
        var account = login ? "Sign up" : "Sign in";
        wrong.AddRange(Regex.IsMatch(fixture, $"<a href=\"#\"[^>]*><span[^>]*>{account}</span></a>", RegexOptions.None, TimeSpan.FromSeconds(1)) ? [] : [$"{block}: no link {account}"]);
        var forgotten = Regex.IsMatch(fixture, "<a href=\"#\"[^>]*><div[^>]*>Forgot your password\\?</div></a>", RegexOptions.None, TimeSpan.FromSeconds(1));
        wrong.AddRange(forgotten == (login && block != "login-05") ? [] : [$"{block}: link to the forgotten password {forgotten}"]);
        wrong.AddRange(!Muted.Contains(block) || Regex.IsMatch(fixture, "<a href=\"#\"[^>]*><span[^>]*>Terms of Service</span></a>", RegexOptions.None, TimeSpan.FromSeconds(1)) ? [] : [$"{block}: no link to the terms"]);
        return wrong;
    }

    private static List<string> Separator(string what, IReadOnlyList<ElementMetrics> divs, IReadOnlyList<ElementMetrics> expected)
    {
        var text = divs.Last(element => element.Text == "Or continue with");
        var content = divs[divs.ToList().IndexOf(text) - 1];
        var row = divs.Last(element => element.Text == "Or continue with" && element.Height == 20 && element.Width > content.Width + 20);
        var line = divs.Single(element => element.Height == 1 && Math.Abs(element.Y - (row.Y + 10)) < 0.5);
        var span = expected.Single(element => element.Slot == "field-separator-content");
        var rule = expected.Single(element => element.Slot == "separator");
        var wrong = Compare.Box($"{what} separator", row, expected.Single(element => element.Slot == "field-separator"), 1).ToList();
        wrong.AddRange(Compare.Box($"{what} line", line, rule, 1));
        wrong.AddRange(Math.Abs(content.X - span.X) <= 1 && Math.Abs(content.Width - span.Width) <= 2 && Math.Abs(content.Y - span.Y) <= 1 ? [] : [$"{what} content {content.X},{content.Y} {content.Width}, shadcn {span.X},{span.Y} {span.Width}"]);
        wrong.AddRange(CssColor.Parse(content.Background).Near(CssColor.Parse(span.Background)) ? [] : [$"{what} content background {content.Background}, shadcn {span.Background}"]);
        wrong.AddRange(CssColor.Parse(text.Color).Near(CssColor.Parse(span.Color)) && text.FontSize == span.FontSize ? [] : [$"{what} text {text.Color} {text.FontSize}, shadcn {span.Color} {span.FontSize}"]);
        wrong.AddRange(CssColor.Parse(line.Background).Near(CssColor.Parse(rule.Background)) ? [] : [$"{what} line {line.Background}"]);
        return wrong;
    }

    [Fact]
    [Trait("AC", "FIELD-07")]
    public async Task TheFieldSeparatorIsShadcnsOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var block in (string[])["login-02", "login-03"])
            {
                var page = await browser.FixtureAsync(gallery.Address, block, theme);
                var texts = await Browser.PickAsync(page, "#fixture div");
                await page.Context.CloseAsync();
                wrong.AddRange(Separator($"{block} {theme}", texts, await ReferenceMetrics.OfAsync(block, theme)));
            }
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "BLOCKS-05")]
    public async Task EveryBlockIsAGalleryPageAsync()
    {
        var wrong = new List<string>();
        foreach (var block in Blocks)
        {
            var html = await gallery.Client.GetStringAsync("blocks/" + block, TestContext.Current.CancellationToken);
            var main = html[html.IndexOf("<main", StringComparison.Ordinal)..];
            wrong.AddRange(main.Contains("<input type=\"email\"", StringComparison.Ordinal) ? [] : [$"{block}: no form"]);
            wrong.AddRange(Blocks.Where(other => !html.Contains($"href=\"/blocks/{other}\"", StringComparison.Ordinal)).Select(other => $"{block}: no link to {other}"));
        }

        var page = await browser.PageAsync(gallery.Address, "dark", 1280, 1000);
        await page.GotoAsync(new Uri(gallery.Address, "blocks/login-03").ToString());
        var (frame, card) = await page.EvaluateAsync<double[]>("""
            () => {
              const main = document.querySelector("main")
              let frame = main.querySelector("label")
              while (frame && frame !== main && frame.getBoundingClientRect().height < 720) frame = frame.parentElement
              if (!frame || frame === main) return [0, 0, 0]
              const box = frame.getBoundingClientRect(), label = main.querySelector("label").getBoundingClientRect()
              return [box.height, label.y - box.y, box.y + box.height - label.y]
            }
            """) is [var height, var above, var below] ? (height, Math.Min(above, below)) : (0, 0);
        await page.Context.CloseAsync();

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
        Assert.True(frame >= 720 && card > 100, $"the frame is {frame} px tall and the first label {card} px from its nearer edge");
    }

    private const string Outside = """
        () => {
          const main = document.querySelector("main")
          let frame = main.querySelector("label")
          while (frame && frame !== main && frame.getBoundingClientRect().height < 720) frame = frame.parentElement
          const box = frame.getBoundingClientRect(), image = frame.querySelector("img")
          const outside = [...frame.querySelectorAll("input:not([type=hidden]), button, a, label")]
            .map((element) => [element, element.getBoundingClientRect()])
            .filter(([, part]) => part.width > 0 && (part.left < box.left - 0.5 || part.right > box.right + 0.5))
            .map(([element, part]) => `${(element.textContent || element.id || element.getAttribute("aria-label")).trim()} ${Math.round(part.left)}..${Math.round(part.right)} in ${Math.round(box.left)}..${Math.round(box.right)}`)
          return [String(document.documentElement.scrollWidth - document.documentElement.clientWidth), String(image ? image.getBoundingClientRect().width : 0), ...outside]
        }
        """;

    private const string WideCard = """
        () => {
          const main = document.querySelector("main"), image = main.querySelector("img")
          let card = image
          while (card && !(card.querySelector("input") && getComputedStyle(card).borderTopWidth === "1px")) card = card.parentElement
          let column = image
          while (column.parentElement.getBoundingClientRect().width < card.getBoundingClientRect().width - 4) column = column.parentElement
          const box = (element) => element.getBoundingClientRect()
          const labels = [...main.querySelectorAll("label")], links = [...main.querySelectorAll("a")]
          const password = labels.find((label) => label.textContent === "Password"), confirm = labels.find((label) => label.textContent === "Confirm Password")
          const forgot = links.find((link) => link.textContent === "Forgot your password?"), account = links.find((link) => ["Sign up", "Sign in"].includes(link.textContent))
          const inputs = [...main.querySelectorAll("input[type=password]")].map((input) => box(input).y)
          return [box(card).width, box(column).width, (box(card).width - 2) / 2, box(password).height, forgot ? box(forgot).height : 0,
            forgot ? box(forgot).left - box(password).right : 1, box(account).height, confirm ? box(confirm).height : 0, Math.max(...inputs) - Math.min(...inputs)]
        }
        """;

    [Fact]
    [Trait("AC", "BLOCKS-06")]
    public async Task EveryBlockFitsAPhoneAsync()
    {
        var wrong = new List<string>();
        var page = await browser.PageAsync(gallery.Address, "light", 390, 844);
        foreach (var block in Blocks)
        {
            await page.GotoAsync(new Uri(gallery.Address, "blocks/" + block).ToString());
            var found = await page.EvaluateAsync<string[]>(Outside);
            wrong.AddRange(found[0] == "0" ? [] : [$"{block}: {found[0]} px of horizontal scroll"]);
            wrong.AddRange(found[1] == "0" ? [] : [$"{block}: an image {found[1]} px wide"]);
            wrong.AddRange(found.Skip(2).Select(part => $"{block}: {part}"));
        }

        await page.Context.CloseAsync();
        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "BLOCKS-07")]
    public async Task TheFormAndImageBlocksStandInTheirWideCardAsync()
    {
        var wrong = new List<string>();
        var page = await browser.PageAsync(gallery.Address, "light", 1440, 900);
        foreach (var block in (string[])["login-04", "signup-04"])
        {
            await page.GotoAsync(new Uri(gallery.Address, "blocks/" + block).ToString());
            var found = await page.EvaluateAsync<double[]>(WideCard);
            wrong.AddRange(Math.Abs(found[0] - 896) <= 0.5 ? [] : [$"{block}: the card is {found[0]} px wide"]);
            wrong.AddRange(Math.Abs(found[1] - found[2]) <= 0.5 ? [] : [$"{block}: the image's column is {found[1]} px of {found[2]}"]);
            wrong.AddRange(found[3] <= 21 && found[4] <= 21 && found[7] <= 21 ? [] : [$"{block}: labels {found[3]} and {found[7]} px tall, the forgotten password's link {found[4]}"]);
            wrong.AddRange(found[5] > 0 ? [] : [$"{block}: Password and its link {found[5]} px apart"]);
            wrong.AddRange(found[6] <= 21 ? [] : [$"{block}: the account's link is {found[6]} px tall"]);
            wrong.AddRange(found[8] <= 0.5 ? [] : [$"{block}: the password inputs {found[8]} px apart vertically"]);
        }

        await page.Context.CloseAsync();
        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    private const string Breakpoint = """
        () => {
          const main = document.querySelector("main"), image = main.querySelector("img"), input = main.querySelector("input:not([type=hidden])")
          let card = input.parentElement
          while (card && getComputedStyle(card).borderTopWidth !== "1px") card = card.parentElement
          let padded = card.parentElement
          while (padded && parseFloat(getComputedStyle(padded).paddingTop) === 0) padded = padded.parentElement
          return [parseFloat(getComputedStyle(padded).paddingTop), card.getBoundingClientRect().width, image.getBoundingClientRect().width,
            input.getBoundingClientRect().left - card.getBoundingClientRect().left - 1]
        }
        """;

    [Fact]
    [Trait("AC", "BLOCKS-08")]
    public async Task TheFormAndImageBlockFollowsItsFrameAsync()
    {
        var found = new List<string>();
        foreach (var width in (int[])[700, 1440])
        {
            var page = await browser.PageAsync(gallery.Address, "light", width, 900);
            await page.GotoAsync(new Uri(gallery.Address, "blocks/login-04").ToString());
            found.Add($"{width}: " + string.Join(" ", await page.EvaluateAsync<double[]>(Breakpoint)));
            await page.Context.CloseAsync();
        }

        Assert.Equal(["700: 24 384 0 24", "1440: 40 896 447 32"], found);
    }

    [GeneratedRegex("<input type=\"([a-z]+)\" id=\"([a-z]+)\"")]
    private static partial Regex Input();

    [GeneratedRegex("<img src=\"data:image/png;base64,[^\"]+\" alt=\"Image\"")]
    private static partial Regex Image();
}
