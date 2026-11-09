using System.Diagnostics;
using System.Text.RegularExpressions;

[assembly: AssemblyFixture(typeof(Aioros.Cosmo.Gallery.Tests.GalleryHost))]

namespace Aioros.Cosmo.Gallery.Tests;

/// <summary>
/// The Gallery as a user reaches it: its built application started on a port Kestrel picks (the address read from its
/// "Now listening on" line), every page answered once before the tests run (<see cref="WarmAsync"/>), its clock standing
/// stopped after the tests. A Gallery that exits or stays silent past <see cref="Startup"/> fails the fixture with its output.
/// </summary>
public sealed partial class GalleryHost : IAsyncLifetime
{
    /// <summary>The pages a visitor reaches from the Gallery's navigation.</summary>
    public static readonly string[] Routes =
    [
        "/", "/installation", "/themes", "/binding", "/fidelity", "/components/badge", "/components/button", "/components/card",
        "/components/alert-dialog", "/components/checkbox", "/components/context-menu", "/components/dialog", "/components/drawer", "/components/dropdown-menu", "/components/field", "/components/hover-card", "/components/popover", "/components/tooltip", "/components/input", "/components/input-group", "/components/input-otp", "/components/progress", "/components/slider", "/components/label", "/components/native-select", "/components/radio-group", "/components/select", "/components/sheet", "/components/sonner", "/components/switch", "/components/textarea", "/components/toggle", "/components/toggle-group",
        "/components/alert", "/components/aspect-ratio", "/components/avatar", "/components/empty", "/components/item", "/components/kbd", "/components/separator", "/components/skeleton", "/components/spinner", "/components/typography",
        "/components/accordion", "/components/breadcrumb", "/components/button-group", "/components/collapsible", "/components/pagination", "/components/tabs",
        "/components/resizable", "/components/carousel", "/components/command", "/components/combobox", "/components/data-table", "/components/sidebar", "/components/attachment", "/components/bubble", "/components/marker", "/components/message", "/components/message-scroller", "/components/code-block", "/components/trace", "/components/questionnaire", "/components/chart", "/examples/workspace",
        "/examples/forms",
        "/blocks/login-01", "/blocks/login-02", "/blocks/login-03", "/blocks/login-04", "/blocks/login-05", "/blocks/signup-01", "/blocks/signup-02", "/blocks/signup-03", "/blocks/signup-04", "/blocks/signup-05",
        "/blocks/dashboard-01", "/blocks/sidebar-01", "/blocks/sidebar-02", "/blocks/sidebar-03", "/blocks/sidebar-04", "/blocks/sidebar-05", "/blocks/sidebar-06", "/blocks/sidebar-07", "/blocks/sidebar-08", "/blocks/sidebar-09", "/blocks/sidebar-10", "/blocks/sidebar-11", "/blocks/sidebar-12", "/blocks/sidebar-13", "/blocks/sidebar-14", "/blocks/sidebar-15", "/blocks/sidebar-16",
    ];

    /// <summary>The foundation's fixtures (buttons, badges, cards), compared by FIDELITY-01.</summary>
    public static readonly string[] FoundationFixtures =
    [
        "button-variants", "button-sizes", "button-extra-sizes", "button-disabled", "button-icons", "button-invalid", "badge-variants", "badge-icons", "card-basic", "card-plain",
    ];

    /// <summary>The text fields' and selects' fixtures, compared by FIDELITY-02.</summary>
    public static readonly string[] InputFixtures =
    [
        "input-states", "textarea-states", "field-form", "field-content", "field-responsive", "native-select", "select-trigger", "input-file",
    ];

    /// <summary>The choice controls' fixtures, compared by FIDELITY-03.</summary>
    public static readonly string[] ChoiceFixtures =
    [
        "checkbox-states", "radio-group", "switch-states", "toggle-states", "toggle-group",
    ];

    /// <summary>The range and composite controls' fixtures, compared by FIDELITY-04.</summary>
    public static readonly string[] RangeFixtures =
    [
        "slider-states", "slider-range", "progress", "input-group", "input-otp",
    ];

    /// <summary>The display and navigation fixtures and their floors, compared by FIDELITY-07.</summary>
    internal static readonly (string Fixture, double Floor)[] DisplayFixtures =
    [
        ("separator-demo", 0.94), ("skeleton-demo", 0.97), ("spinner-demo", 0.92), ("kbd-demo", 0.92), ("avatar-demo", 0.93), ("avatar-badge", 0.96), ("avatar-group-count", 0.96), ("badge-links", 0.96), ("card-separated", 0.96), ("alert-demo", 0.90),
        ("aspect-ratio-demo", 0.97), ("typography-headings", 0.83), ("typography-text", 0.90), ("empty-demo", 0.91), ("empty-outline", 0.89),
        ("item-demo", 0.92), ("item-variant", 0.93), ("item-header-footer", 0.99),
        ("tabs-demo", 0.85), ("tabs-list", 0.62), ("tabs-line", 0.96), ("tabs-vertical", 0.96), ("tabs-vertical-line", 0.96), ("accordion-demo", 0.45), ("collapsible-demo", 0.90), ("collapsible-open", 0.84), ("breadcrumb-demo", 0.91),
        ("breadcrumb-separator", 0.93), ("pagination-demo", 0.92), ("button-group-demo", 0.93), ("button-group-size", 0.93), ("button-group-orientation", 0.93),
        ("scroll-area-demo", 0.96), ("scroll-area-horizontal", 0.59), ("table-demo", 0.77), ("navigation-menu-demo", 0.96), ("menubar-demo", 0.96),
    ];

    /// <summary>The composite fixtures shown at rest and their floors, compared by FIDELITY-09.</summary>
    internal static readonly (string Fixture, double Floor)[] CompositeFixtures =
    [
        ("resizable-demo", 0.97), ("resizable-handle", 0.97), ("resizable-vertical", 0.97), ("carousel-demo", 0.43), ("carousel-api", 0.45), ("carousel-orientation", 0.96),
        ("command-demo", 0.75), ("combobox-demo", 0.96), ("data-table-demo", 0.80),
        ("sidebar-demo", 0.94), ("sidebar-icon", 0.96), ("sidebar-floating", 0.94), ("sidebar-inset", 0.94),
    ];

    /// <summary>shadcn/ui's chat components' examples (marker, bubble, message, attachment, message scroller) and their floors, compared by FIDELITY-18.</summary>
    internal static readonly (string Fixture, double Floor)[] ChatFixtures =
    [
        ("marker-demo", 0.990), ("marker-variants", 0.995), ("marker-icon", 0.990), ("marker-border", 0.990),
        ("bubble-demo", 0.995), ("bubble-variants", 0.990), ("bubble-alignment", 0.990), ("bubble-group-demo", 0.990),
        ("message-demo", 0.995), ("message-group", 0.995), ("message-header-footer", 0.990), ("message-avatar", 0.995),
        ("attachment-demo", 0.990), ("attachment-states", 0.990), ("attachment-sizes", 0.990), ("attachment-group", 0.985),
        ("message-scroller-end", 0.980), ("message-scroller-start", 0.990),
        ("marker-shimmer", 0.995), ("marker-status", 0.990), ("marker-link-button", 0.990), ("bubble-link-button", 0.995), ("bubble-collapsible", 0.990),
        ("attachment-trigger", 0.990), ("attachment-image", 0.990), ("bubble-markdown", 0.995),
    ];

    /// <summary>The radix base's code block, trace and questionnaire examples in the radix-rhea style and their floors, compared by FIDELITY-20.</summary>
    internal static readonly (string Fixture, double Floor)[] RheaFixtures =
    [
        ("code-block-demo", 0.990), ("code-block-basic", 0.995), ("code-block-header", 0.990), ("code-block-line-numbers", 0.995),
        ("code-block-highlighted-lines", 0.995), ("code-block-diff", 0.990), ("code-block-destructive", 0.990), ("code-block-size", 0.990),
        ("code-block-highlighting", 0.995),
        ("code-block-actions", 0.990), ("code-block-ansi", 0.990), ("code-block-chat", 0.990), ("code-block-floating-actions", 0.995), ("code-block-height", 0.990), ("code-block-languages", 0.995), ("code-block-rtl", 0.990), ("code-block-show-more-scroll", 0.990), ("code-block-show-more", 0.990), ("code-block-start-line", 0.990), ("code-block-streaming", 0.990), ("code-block-tabs", 0.990), ("code-block-trace", 0.990), ("code-block-typeset", 0.990), ("code-block-wrap", 0.995), ("code-block-markdown", 0.990),
        ("trace-commit", 0.985), ("trace-demo", 0.990), ("trace-dialog", 0.985), ("trace-link", 0.985), ("trace-open", 0.990), ("trace-sheet", 0.990), ("trace-sources", 0.985), ("trace-thinking", 0.990), ("trace-tool-calls", 0.990), ("trace-turn", 0.985),
        ("questionnaire-animated", 0.990), ("questionnaire-card", 0.990), ("questionnaire-conditional", 0.990), ("questionnaire-controlled", 0.990), ("questionnaire-dialog", 0.985), ("questionnaire-navigation-state", 0.990), ("questionnaire-progress", 0.990), ("questionnaire-resume", 0.985), ("questionnaire-shortcuts", 0.985), ("questionnaire-skip", 0.990), ("questionnaire-validation", 0.990),
        ("trace-static", 0.990), ("trace-status", 0.990), ("trace-description", 0.985), ("trace-group", 0.990), ("trace-collapsible", 0.990), ("trace-steps", 0.990),
        ("questionnaire-demo", 0.990), ("questionnaire-multiple", 0.990), ("questionnaire-freeform", 0.990),
    ];

    /// <summary>shadcn/ui's login and signup blocks and their floors, compared by FIDELITY-16.</summary>
    internal static readonly (string Fixture, double Floor)[] BlockFixtures =
    [
        ("login-01", 0.990), ("login-02", 0.990), ("login-03", 0.985), ("login-04", 0.985), ("login-05", 0.985),
        ("signup-01", 0.990), ("signup-02", 0.990), ("signup-03", 0.990), ("signup-04", 0.985), ("signup-05", 0.985),
    ];

    /// <summary>shadcn/ui's app shell blocks (dashboard-01, sidebar-01 to sidebar-16) and their floors, compared by FIDELITY-22 in a 1560 by 800 viewport.</summary>
    internal static readonly (string Fixture, double Floor)[] ShellFixtures =
    [
        ("dashboard-01", 0.985), ("sidebar-01", 0.985), ("sidebar-02", 0.985), ("sidebar-03", 0.995), ("sidebar-04", 0.995), ("sidebar-05", 0.980), ("sidebar-06", 0.985), ("sidebar-07", 0.985), ("sidebar-08", 0.990), ("sidebar-09", 0.950),
        ("sidebar-10", 0.980), ("sidebar-11", 0.990), ("sidebar-12", 0.980), ("sidebar-13", 0.990), ("sidebar-14", 0.995), ("sidebar-15", 0.980), ("sidebar-16", 0.985),
    ];

    /// <summary>shadcn/ui's chart examples drawn by Aioros.Pictor and their floors, compared by FIDELITY-14.</summary>
    internal static readonly (string Fixture, double Floor)[] ChartFixtures =
    [
        ("chart-area-default", 0.990), ("chart-area-gradient", 0.990), ("chart-area-stacked", 0.990), ("chart-area-interactive", 0.965),
        ("chart-bar-default", 0.990), ("chart-bar-multiple", 0.990), ("chart-bar-horizontal", 0.985), ("chart-bar-label", 0.990),
        ("chart-bar-interactive", 0.980), ("chart-line-default", 0.990), ("chart-line-multiple", 0.980), ("chart-line-dots", 0.990),
        ("chart-line-interactive", 0.980), ("chart-pie-simple", 0.990), ("chart-pie-donut", 0.990), ("chart-pie-donut-text", 0.990),
        ("chart-pie-legend", 0.980), ("chart-radar-default", 0.990), ("chart-radar-dots", 0.990), ("chart-radar-multiple", 0.990),
        ("chart-radial-simple", 0.990), ("chart-radial-text", 0.990), ("chart-radial-stacked", 0.990), ("chart-tooltip-default", 0.985),
        ("chart-tooltip-indicator-line", 0.985), ("chart-tooltip-indicator-none", 0.985),
    ];

    /// <summary>The overlay fixtures Aioros shows open on the web, how a user opens each and its floor, compared by FIDELITY-05.</summary>
    internal static readonly (string Fixture, OverlayOpening Opening, double Floor)[] OverlayFixtures =
    [
        ("popover-open", new("click", "button", "[popover]:popover-open", true), 0.90),
        ("tooltip-open", new("hover", "button", null, false), 0.92),
        ("kbd-tooltip-open", new("hover", "button", null, false), 0.97),
        ("dropdown-menu-open", new("click", "button", "[popover]:popover-open", true), 0.91),
        ("dropdown-menu-checkboxes", new("click", "button", "[popover]:popover-open", true), 0.91),
        ("dialog-open", new("click", "button", "dialog.prompt:modal", false), 0.92),
        ("alert-dialog-open", new("click", "button", "dialog.prompt:modal", false), 0.94),
        ("sheet-open", new("click", "button", "dialog.prompt:modal", false), 0.94),
        ("sheet-top", new("click", "button", "dialog.prompt:modal", false), 0.95),
        ("drawer-open", new("click", "button", "dialog.prompt:modal", false), 0.94),
        ("navigation-menu-open", new("click", "button[popovertarget=m2]", "[popover]:popover-open", false), 0.81),
        ("menubar-open", new("click", "menu button", "[popover]:popover-open", true), 0.92),
        ("combobox-open", new("click", "button", "[popover]:popover-open", true), 0.98),
        ("command-dialog-open", new("press", "Control+j", "dialog[open]", false, [640, 480]), 0.98),
    ];

    /// <summary>The overlay fixtures' pages, the ones Aioros cannot show open on the web included.</summary>
    public static readonly string[] OverlayPages = ["command-filtered", "command-dialog-open", "combobox-open", "data-table-selected", "popover-open", "tooltip-open", "kbd-tooltip-open", "hover-card-open", "dropdown-menu-open", "dropdown-menu-checkboxes", "context-menu-open", "dialog-open", "alert-dialog-open", "sheet-open", "sheet-top", "drawer-open", "sonner-stack", "sonner-types", "sonner-close"];

    /// <summary>The fixtures compared with shadcn/ui's renders, each at <c>/fidelity/{fixture}</c>.</summary>
    public static readonly string[] Fixtures = [.. FoundationFixtures, .. InputFixtures, .. ChoiceFixtures, .. RangeFixtures, .. OverlayPages, .. DisplayFixtures.Select(fixture => fixture.Fixture), .. CompositeFixtures.Select(fixture => fixture.Fixture), .. ChartFixtures.Select(fixture => fixture.Fixture), .. BlockFixtures.Select(fixture => fixture.Fixture), .. ShellFixtures.Select(fixture => fixture.Fixture), .. ChatFixtures.Select(fixture => fixture.Fixture), .. RheaFixtures.Select(fixture => fixture.Fixture)];

    /// <summary>How long the Gallery may take to listen.</summary>
    public static readonly TimeSpan Startup = TimeSpan.FromSeconds(60);

    private readonly System.Collections.Concurrent.ConcurrentQueue<string> _errors = new();

    private Process? _process;

    private Task? _drain;

    public HttpClient Client { get; } = new();

    public Uri Address { get; private set; } = new("http://127.0.0.1/");

    /// <summary>The Gallery project's folder, where it runs as <c>dotnet run</c> runs it.</summary>
    public static string ProjectFolder { get; } = Path.Combine(Root(), "samples", "Gallery", "Gallery");

    /// <summary>The Gallery's built application, in the configuration the tests were built in.</summary>
    public static string Application { get; } = Path.Combine(ProjectFolder, "bin", Configuration(), "net10.0", "Gallery.dll");

    public async ValueTask InitializeAsync()
    {
        _process = Start(_errors, Application, "--urls", "http://127.0.0.1:0");
        Address = await ListeningAsync(_process);
        Client.BaseAddress = Address;
        await WarmedAsync(_process);
        _drain = _process.StandardOutput.ReadToEndAsync();
        await WarmAsync();
    }

    /// <summary>The longest Aioros's own startup warm-up may take before the tests give up on the Gallery.</summary>
    public static readonly TimeSpan Warmup = TimeSpan.FromMinutes(20);

    /// <summary>
    /// Waits for the Gallery's startup warm-up to log that it finished (Aioros's <c>WarmupFinished</c>, SSR-244), so the host's
    /// own warm-up requests find every plan compiled instead of racing Aioros's compilations on a slow runner.
    /// </summary>
    private async Task WarmedAsync(Process process)
    {
        using var warming = new CancellationTokenSource(Warmup, TimeProvider.System);
        try
        {
            while (await process.StandardOutput.ReadLineAsync(warming.Token) is { } line)
            {
                if (line.Contains("Warm-up finished", StringComparison.Ordinal))
                {
                    return;
                }
            }
        }
        catch (OperationCanceledException) when (warming.IsCancellationRequested)
        {
            throw new InvalidOperationException($"The Gallery's startup warm-up did not finish within {Warmup.TotalMinutes} min: {string.Join('\n', _errors)}");
        }

        await process.WaitForExitAsync();
        throw new InvalidOperationException($"The Gallery exited with code {process.ExitCode} while warming up: {string.Join('\n', _errors)}");
    }

    private async Task<Uri> ListeningAsync(Process process)
    {
        using var startup = new CancellationTokenSource(Startup, TimeProvider.System);
        try
        {
            while (await process.StandardOutput.ReadLineAsync(startup.Token) is { } line)
            {
                if (Listening().Match(line) is { Success: true } listening)
                {
                    return new Uri(listening.Groups[1].Value + "/");
                }
            }
        }
        catch (OperationCanceledException) when (startup.IsCancellationRequested)
        {
            throw new InvalidOperationException($"The Gallery did not listen within {Startup.TotalSeconds} s: {string.Join('\n', _errors)}");
        }

        await process.WaitForExitAsync();
        throw new InvalidOperationException($"The Gallery exited with code {process.ExitCode} before listening: {string.Join('\n', _errors)}");
    }

    /// <summary>The cultures the Gallery serves (its <c>Aioros:Cultures</c>), its default first.</summary>
    public static readonly string[] Cultures = ["en-US", "es-ES"];

    /// <summary>
    /// Answers every page once in each culture and theme the Gallery serves, one after another, before any test runs: the Gallery
    /// compiles a page's plan in each culture and theme the first time it answers it, so the tests then find every plan compiled
    /// (after Aioros's own startup warm-up has logged that it finished, SSR-244). Each page's answer is the signal that its plan is
    /// compiled; a page that does not answer within <see cref="Patience.Limit"/> fails the fixture by its name.
    /// </summary>
    private async Task WarmAsync()
    {
        foreach (var culture in Cultures.Select(static (name, index) => index == 0 ? string.Empty : "?culture=" + name))
        {
            foreach (var theme in (string[])["light", "dark"])
            {
                foreach (var page in (string[])[.. Routes, .. Fixtures.Select(fixture => "/fidelity/" + fixture)])
                {
                    await WarmPageAsync(page, culture, theme);
                }
            }
        }
    }

    private async Task WarmPageAsync(string page, string culture, string theme)
    {
        using var bound = new Patience(CancellationToken.None);
        using var request = new HttpRequestMessage(HttpMethod.Get, page.TrimStart('/') + culture);
        request.Headers.Add("Cookie", "theme=" + theme);
        try
        {
            using var answer = await Client.SendAsync(request, bound.Token);
            answer.EnsureSuccessStatusCode();
        }
        catch (OperationCanceledException) when (bound.Token.IsCancellationRequested)
        {
            throw new InvalidOperationException($"The Gallery did not answer {page}{culture} in the {theme} theme within {Patience.Limit.TotalSeconds} s while warming up");
        }
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        if (_process is { HasExited: false })
        {
            _process.Kill(entireProcessTree: true);
            await _process.WaitForExitAsync();
        }

        if (_drain is not null)
        {
            await _drain;
        }

        _process?.Dispose();
    }

    /// <summary>Starts the Gallery's application with <paramref name="arguments"/>, its output redirected, on the tests' clock.</summary>
    public static Process Start(params string[] arguments) => Start(new System.Collections.Concurrent.ConcurrentQueue<string>(), arguments);

    private static Process Start(System.Collections.Concurrent.ConcurrentQueue<string> errors, params string[] arguments)
    {
        var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            WorkingDirectory = ProjectFolder,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        start.Environment["DOTNET_NOLOGO"] = "1";
        var process = Process.Start(start) ?? throw new InvalidOperationException("The Gallery did not start.");
        process.ErrorDataReceived += (_, line) =>
        {
            if (line.Data is { } data)
            {
                errors.Enqueue(data);
            }
        };
        process.BeginErrorReadLine();
        return process;
    }

    private static string Configuration() =>
        AppContext.BaseDirectory.Split(Path.DirectorySeparatorChar).Contains("Release", StringComparer.Ordinal) ? "Release" : "Debug";

    private static string Root()
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "Aioros.Cosmo.slnx")))
        {
            folder = folder.Parent;
        }

        return folder?.FullName ?? throw new InvalidOperationException("The repository root was not found.");
    }

    [GeneratedRegex(@"Now listening on: (http://[^\s]+)")]
    private static partial Regex Listening();
}
