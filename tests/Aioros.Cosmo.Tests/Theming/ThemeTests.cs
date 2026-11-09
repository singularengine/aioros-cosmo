using Aioros.Cosmo.Display;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Theming;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Theming;

public sealed class ThemeTests(DesktopSession desktop)
{
    [Fact]
    [Trait("AC", "THEME-01")]
    public async Task TheVariantSwitchesAtRuntimeAsync()
    {
        var (light, dark) = await desktop.RunAsync(() =>
        {
            var button = new Button { Content = "Button" };
            var window = Windows.Show(button, ThemeVariant.Light);
            var before = (Windows.Paint(window.Background), Windows.Paint(button.Background));
            window.RequestedThemeVariant = ThemeVariant.Dark;
            Windows.Settle();
            var after = (Windows.Paint(window.Background), Windows.Paint(button.Background));
            window.Close();
            return (before, after);
        });

        Assert.Equal((CssColor.Parse("#FFFFFF"), CssColor.Parse("#171717")), light);
        Assert.Equal((CssColor.Parse("#0A0A0A"), CssColor.Parse("#E5E5E5")), dark);
    }

    [Fact]
    [Trait("AC", "THEME-02")]
    public async Task TheThemeTakesAColorAndARadiusAsync()
    {
        var (blue, buttonRadius, cardRadius, neutral) = await desktop.RunAsync(() =>
        {
            var button = new Button { Content = "Button" };
            var card = new Card { Items = { new CardHeader { Title = "Title" } } };
            var theme = new CosmoTheme { Color = CosmoColor.Blue, Radius = CosmoRadius.Large };
            var window = Windows.Show(new StackPanel { Children = { button, card } }, ThemeVariant.Light, theme);
            var result = (Windows.Paint(button.Background), button.CornerRadius.TopLeft, card.CornerRadius.TopLeft, default(CssColor));
            theme.Color = CosmoColor.Neutral;
            Windows.Settle();
            result.Item4 = Windows.Paint(button.Background);
            window.Close();
            return result;
        });

        Assert.Equal(CssColor.Parse("#1447E6"), blue);
        Assert.Equal(9.6, buttonRadius, 6);
        Assert.Equal(16.8, cardRadius, 6);
        Assert.Equal(CssColor.Parse("#171717"), neutral);
    }

    [Fact]
    [Trait("AC", "THEME-03")]
    public async Task AnAppsResourcesOverrideATokenPerVariantAsync()
    {
        var (light, dark) = await desktop.RunAsync(() =>
        {
            var button = new Button { Content = "Button" };
            var window = new Window { Width = 400, Height = 300, RequestedThemeVariant = ThemeVariant.Light };
            window.Styles.Add(new CosmoTheme());
            var overrides = new ResourceDictionary();
            overrides.ThemeDictionaries[ThemeVariant.Light] = new ResourceDictionary { ["CosmoPrimaryBrush"] = new SolidColorBrush(Color.Parse("#4F39F6")) };
            window.Resources.MergedDictionaries.Add(overrides);
            window.Content = button;
            window.Show();
            Windows.Settle();
            var first = Windows.Paint(button.Background);
            window.RequestedThemeVariant = ThemeVariant.Dark;
            Windows.Settle();
            var second = Windows.Paint(button.Background);
            window.Close();
            return (first, second);
        });

        Assert.Equal(CssColor.Parse("#4F39F6"), light);
        Assert.Equal(CssColor.Parse("#E5E5E5"), dark);
    }

    [Fact]
    [Trait("AC", "THEME-04")]
    public async Task AScopeSwapsItsContentsTokensAtRuntimeAsync()
    {
        var (violet, green, greenRadius, outside, outsideRadius) = await desktop.RunAsync(() =>
        {
            var inner = new Button { Content = "Inner" };
            var outer = new Button { Content = "Outer" };
            var scope = new ThemeScope { Color = CosmoColor.Violet, Content = inner };
            var window = Windows.Show(new StackPanel { Orientation = Orientation.Horizontal, Children = { scope, outer } }, ThemeVariant.Light);
            var before = Windows.Paint(inner.Background);
            scope.Color = CosmoColor.Green;
            scope.Radius = CosmoRadius.None;
            Windows.Settle();
            var result = (before, Windows.Paint(inner.Background), inner.CornerRadius.TopLeft, Windows.Paint(outer.Background), outer.CornerRadius.TopLeft);
            window.Close();
            return result;
        });

        Assert.Equal(CssColor.Parse("#7F22FE"), violet);
        Assert.Equal(CssColor.Parse("#5EA500"), green);
        Assert.Equal(0, greenRadius);
        Assert.Equal(CssColor.Parse("#171717"), outside);
        Assert.Equal(8, outsideRadius);
    }
}
