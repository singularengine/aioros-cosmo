using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using SkiaSharp;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Chat;

public sealed class RheaFidelityTests(DesktopSession desktop)
{
    private static readonly (string Fixture, double Floor)[] Floors =
    [
        ("code-block-demo", 0.945), ("code-block-basic", 0.955), ("code-block-header", 0.950), ("code-block-line-numbers", 0.970),
        ("code-block-highlighted-lines", 0.965), ("code-block-diff", 0.955), ("code-block-destructive", 0.950), ("code-block-size", 0.940),
        ("code-block-highlighting", 0.965),
        ("code-block-actions", 0.940), ("code-block-ansi", 0.960), ("code-block-chat", 0.950), ("code-block-floating-actions", 0.955), ("code-block-height", 0.945), ("code-block-languages", 0.965), ("code-block-rtl", 0.970), ("code-block-show-more-scroll", 0.950), ("code-block-show-more", 0.950), ("code-block-start-line", 0.960), ("code-block-streaming", 0.940), ("code-block-tabs", 0.950), ("code-block-trace", 0.950), ("code-block-typeset", 0.930), ("code-block-wrap", 0.930), ("code-block-markdown", 0.950),
        ("trace-commit", 0.970), ("trace-demo", 0.955), ("trace-dialog", 0.980), ("trace-link", 0.975), ("trace-open", 0.970), ("trace-sheet", 0.985), ("trace-sources", 0.970), ("trace-thinking", 0.990), ("trace-tool-calls", 0.990), ("trace-turn", 0.960),
        ("questionnaire-animated", 0.950), ("questionnaire-card", 0.965), ("questionnaire-conditional", 0.960), ("questionnaire-controlled", 0.940), ("questionnaire-dialog", 0.980), ("questionnaire-navigation-state", 0.950), ("questionnaire-progress", 0.955), ("questionnaire-resume", 0.940), ("questionnaire-shortcuts", 0.960), ("questionnaire-skip", 0.955), ("questionnaire-validation", 0.960),
        ("trace-static", 0.980), ("trace-status", 0.975), ("trace-description", 0.970), ("trace-group", 0.970), ("trace-collapsible", 0.985), ("trace-steps", 0.970),
        ("questionnaire-demo", 0.945), ("questionnaire-multiple", 0.965), ("questionnaire-freeform", 0.955),
    ];

    [Fact]
    [Trait("AC", "FIDELITY-19")]
    public async Task EveryRheaFixtureMatchesShadcnsRenderOnTheDesktopAsync()
    {
        var verdicts = new List<string>();
        var wrong = new List<string>();
        foreach (var (variant, theme) in Themes)
        {
            foreach (var (fixture, floor) in Floors)
            {
                var (matched, size, expected) = await desktop.RunAsync<(double, (int, int), (int, int))>(() =>
                {
                    var window = DisplayFixtures.Show(fixture, variant);
                    using var ours = Windows.Capture(window);
                    using var reference = Pixels.Reference(fixture, theme);
                    if (Environment.GetEnvironmentVariable("COSMO_CAPTURES") is { Length: > 0 } folder)
                    {
                        using var file = File.Create(Path.Combine(folder, $"{fixture}.{theme}.png"));
                        ours.Encode(file, SKEncodedImageFormat.Png, 100);
                    }

                    window.Close();
                    return (Pixels.Matched(ours, reference), (ours.Width, ours.Height), (reference.Width, reference.Height));
                });
                verdicts.Add($"{fixture}.{theme}: {matched:P2}");
                if (size != expected || matched < floor)
                {
                    wrong.Add($"{fixture}.{theme}: {size} (shadcn {expected}), {matched:P2} of pixels within {Pixels.Tolerance}/255, floor {floor:P0}");
                }
            }
        }

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join(Environment.NewLine, verdicts));
        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
}
