using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Cards = Aioros.Cosmo.Overlays.HoverCard;

namespace Aioros.Cosmo.Tests.Overlays;

public sealed class HoverCardTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    [Fact]
    [Trait("AC", "HOVERCARD-01")]
    public async Task TheOpenCardIsShadcnsHoverCardContentAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var content = (await ReferenceMetrics.SlotsAsync("hover-card-open", name, "hover-card-content"))[0];
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.HoverCardOpen(), variant);
                var presenter = PopoverTests.Presenter(window);
                var errors = PopoverTests.Surface($"{name} card", Windows.Part<Border>(presenter, "LayoutRoot"), presenter.Padding, window, content).ToList();
                window.Close();
                var plain = OverlayFixtures.Outline("Plain");
                Cards.SetOpenDelay(plain, 0);
                Cards.SetCard(plain, "Card");
                var second = Windows.Fixture(plain, 300, variant);
                Windows.Hover(second, plain);
                if (PopoverTests.Presenter(second).Bounds.Width != 256)
                {
                    errors.Add($"{name}: plain card {PopoverTests.Presenter(second).Bounds.Width} px, shadcn 256");
                }

                second.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "HOVERCARD-02")]
    public async Task TheCardOpensStaysAndClosesAsRadixsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var clicks = 0;
            var inside = new Button { Content = "Follow" };
            inside.Click += (_, _) => clicks++;
            var slow = OverlayFixtures.Outline("Slow");
            Cards.SetCard(slow, "Slow card");
            var quick = OverlayFixtures.Outline("Quick");
            quick.Margin = new Thickness(0, 120, 0, 0);
            Cards.SetOpenDelay(quick, 0);
            Cards.SetCloseDelay(quick, 0);
            Cards.SetCard(quick, new StackPanel { Children = { new TextBlock { Text = "Card" }, inside } });
            var lingering = OverlayFixtures.Outline("Lingering");
            lingering.Margin = new Thickness(0, 360, 0, 0);
            Cards.SetOpenDelay(lingering, 0);
            Cards.SetCard(lingering, "Lingering card");
            var window = Windows.Fixture(new Panel { Children = { slow, quick, lingering } }, 560, ThemeVariant.Light);
            var steps = new List<string>();
            Windows.Hover(window, slow);
            steps.Add($"slow {Cards.IsOpen(slow)}");
            Windows.Hover(window, quick);
            steps.Add($"quick {Cards.IsOpen(quick)}");
            Windows.Hover(window, inside);
            steps.Add($"over card {Cards.IsOpen(quick)}");
            Windows.Click(window, inside);
            steps.Add($"clicked {clicks}");
            Windows.Away(window, new Point(600, 540));
            steps.Add($"left {Cards.IsOpen(quick)}");
            Windows.Hover(window, lingering);
            Windows.Away(window, new Point(600, 540));
            steps.Add($"lingers {Cards.IsOpen(lingering)}");
            Windows.Hover(window, lingering);
            quick.Focus(NavigationMethod.Tab);
            Windows.Settle();
            steps.Add($"tab {Cards.IsOpen(quick)}");
            lingering.Focus(NavigationMethod.Pointer);
            Windows.Settle();
            steps.Add($"blur {Cards.IsOpen(quick)}");
            window.Close();
            return steps;
        });

        Assert.Equal(["slow False", "quick True", "over card True", "clicked 1", "left False", "lingers True", "tab True", "blur False"], found);
    }

    [Fact]
    [Trait("AC", "HOVERCARD-03")]
    public async Task TheCardBindsThePagesModelAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new OverlaysModel();
            var view = new OverlaysView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var link = view.FindControl<HyperlinkButton>("UserLink")!;
            Windows.Hover(window, link);
            var presenter = PopoverTests.Presenter(window);
            var texts = presenter.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text).ToList();
            model.User = new UserModel("Vercel", "@vercel", "April 2015");
            Windows.Settle();
            texts.AddRange(presenter.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text));
            window.Close();
            return (link.Content as string, texts);
        });

        Assert.Equal("@vercel", found.Item1);
        Assert.Equal(["Next.js", "Joined December 2021", "Vercel", "Joined April 2015"], found.texts);
    }
}
