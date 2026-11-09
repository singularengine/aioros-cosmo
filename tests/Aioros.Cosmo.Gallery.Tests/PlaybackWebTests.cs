using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

/// <summary>The streamed examples played back through the page's live stream, each visitor's own (Aioros's live state per connection).</summary>
public sealed class PlaybackWebTests(GalleryHost gallery, Browser browser)
{
    private const string Text = "document.getElementById('fixture').innerText.trim()";

    private const string Button = "[...document.querySelectorAll('#fixture button')].find(b => b.innerText.trim() === label && b.checkVisibility())";

    private const string Record = """
        () => {
          window.seen = [];
          const frame = () => document.getElementById('fixture') && window.seen.push({
            text: document.getElementById('fixture').innerText.trim(),
            buttons: Object.fromEntries([...document.querySelectorAll('#fixture button')].filter(b => b.checkVisibility()).map(b => [b.innerText.trim(), { disabled: b.disabled }])),
          });
          new MutationObserver(frame).observe(document.documentElement, { subtree: true, childList: true, characterData: true, attributes: true });
        }
        """;

    /// <summary>Opens a fixture once its live stream, opened with the page's plan, named its connection, so the page's posts act on its own turn.</summary>
    private async Task<(IPage Page, Func<int> Posts)> OpenAsync(string fixture)
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280, 900);
        var posts = 0;
        page.Request += (_, request) => posts += request.Method == "POST" ? 1 : 0;
        await page.RunAndWaitForResponseAsync(
            () => page.GotoAsync(new Uri(gallery.Address, "fidelity/" + fixture).ToString()),
            static response => response.Url.Contains("/live?", StringComparison.Ordinal) && response.Url.Contains("plan=", StringComparison.Ordinal));
        await page.WaitForFunctionAsync("() => document.documentElement.hasAttribute('data-aioros-live')", null, new() { PollingInterval = 10 });
        await page.EvaluateAsync(Record);
        return (page, () => posts);
    }

    private static Task<IJSHandle> UntilAsync(IPage page, string condition, object? argument = null) =>
        page.WaitForFunctionAsync($"(arg) => {{ const text = {Text}; const button = (label) => {Button}; return {condition}; }}", argument, new() { PollingInterval = 10 });

    /// <summary>Whether the fixture showed a state the condition holds for at any moment since it opened: every change to the document is recorded as it lands (a live update may put a new fixture element in place of the old), so a state the stream passes through quickly is seen however loaded the machine.</summary>
    private static Task<bool> SawAsync(IPage page, string condition, object? argument = null) =>
        page.EvaluateAsync<bool>($"(arg) => window.seen.some(({{ text, buttons }}) => {{ const button = (label) => buttons[label]; return {condition}; }})", argument);

    private static Task ClickAsync(IPage page, string label) => page.Locator("#fixture button").Filter(new() { HasTextString = label }).First.ClickAsync();

    [Fact]
    [Trait("AC", "CODEBLOCK-12")]
    public async Task TheCodeBlocksStreamOnTheWebAsync()
    {
        var (streaming, streamingPosts) = await OpenAsync("code-block-streaming");
        var whole = await streaming.EvaluateAsync<string>(Text);
        await ClickAsync(streaming, "Replay");
        await UntilAsync(streaming, "text === arg && button('Replay')?.disabled === false && window.seen.some((f) => f.text !== arg)", whole);
        var streamed = await SawAsync(streaming, "text.length > 0 && text.length < arg.length && button('Replay')?.disabled === true", whole);

        var (markdown, markdownPosts) = await OpenAsync("code-block-markdown");
        var served = await markdown.EvaluateAsync<string>(Text);
        await ClickAsync(markdown, "Replay");
        await UntilAsync(markdown, "text === arg && !!button('Replay') && window.seen.some((f) => f.text !== arg)", served);
        var fenced = await SawAsync(markdown, "text.startsWith('How do I debounce a search input in React?') && text.length > 'How do I debounce a search input in React?'.length && text.split(/\\s+/).length < arg.split(/\\s+/).length && !button('Replay')", served);

        Assert.Equal((true, true, 1, 1), (streamed, fenced, streamingPosts(), markdownPosts()));
    }

    [Fact]
    [Trait("AC", "TRACE-09")]
    public async Task TheTracesStreamOnTheWebAsync()
    {
        var (thinking, thinkingPosts) = await OpenAsync("trace-thinking");
        Assert.Equal("Send message", await thinking.EvaluateAsync<string>(Text));
        await ClickAsync(thinking, "Send message");
        await UntilAsync(thinking, "text.includes('Thought') && text.includes('instead of the commit hash.') && !!button('Reset')");
        var thought = await SawAsync(thinking, "text.includes('Thinking') && text.includes('The build') && !text.includes('Image optimization is the bottleneck')");
        await ClickAsync(thinking, "Reset");
        await UntilAsync(thinking, "text === 'Send message'");

        var (tools, toolsPosts) = await OpenAsync("trace-tool-calls");
        await ClickAsync(tools, "Send message");
        await UntilAsync(tools, "text.includes('Failed to run pnpm --version') && text.includes('pnpm is not.') && !!button('Reset')");
        var ran = await SawAsync(tools, "text.includes('Running node --version') && !text.includes('Ran node --version')");
        await ClickAsync(tools, "Reset");
        await UntilAsync(tools, "text === 'Send message'");

        var (demo, demoPosts) = await OpenAsync("trace-demo");
        var served = await demo.EvaluateAsync<string>(Text);
        await ClickAsync(demo, "Replay");
        await UntilAsync(demo, "text === arg && !!button('Replay') && window.seen.some((f) => f.text !== arg)", served);
        var replayed = (await SawAsync(demo, "text.includes('Thinking') && !button('Replay')"), await SawAsync(demo, "/Running (pnpm|git)/.test(text)"));

        Assert.Equal((true, true, (true, true), 2, 2, 1), (thought, ran, replayed, thinkingPosts(), toolsPosts(), demoPosts()));
    }

    [Fact]
    [Trait("AC", "GALLERY-16")]
    public async Task EachVisitorPlaysTheirOwnStreamAsync()
    {
        var (first, _) = await OpenAsync("trace-thinking");
        var (second, secondPosts) = await OpenAsync("trace-thinking");
        await ClickAsync(first, "Send message");
        await UntilAsync(first, "text.includes('Thinking') && text.includes('The build')");
        var during = await second.EvaluateAsync<string>(Text);
        await UntilAsync(first, "!!button('Reset')");
        var after = await second.EvaluateAsync<string>(Text);
        await ClickAsync(second, "Send message");
        await UntilAsync(second, "text.includes('Thinking')");
        var replayed = await first.EvaluateAsync<string>(Text);

        Assert.Equal(("Send message", "Send message", 1), (during, after, secondPosts()));
        Assert.Contains("Reset", replayed, StringComparison.Ordinal);
        Assert.Contains("instead of the commit hash.", replayed, StringComparison.Ordinal);
    }
}
