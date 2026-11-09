using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Components;

public sealed class ButtonTests(DesktopSession desktop)
{
    private static readonly string[] Variants = ["", "destructive", "outline", "secondary", "ghost", "link"];

    private static readonly string[] Focused = ["", "destructive", "outline"];

    private static readonly string[] IconClasses = ["has-icon", "sm has-icon", "xs has-icon", "lg has-icon", ""];

    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private sealed record Look(CssColor Background, CssColor Foreground, CssColor Border, double BorderWidth);

    private static Look Read(Button button) =>
        new(Windows.Paint(button.Background), Windows.Paint(button.Foreground), Windows.Paint(button.BorderBrush), button.BorderThickness.Top);

    private static IEnumerable<string> Differences(string what, Look found, ElementMetrics expected)
    {
        var width = double.Parse(expected.BorderWidth.TrimEnd('p', 'x'), System.Globalization.CultureInfo.InvariantCulture);
        if (!found.Background.Near(CssColor.Parse(expected.Background)))
        {
            yield return $"{what} background {found.Background}, shadcn {CssColor.Parse(expected.Background)}";
        }

        if (!found.Foreground.Near(CssColor.Parse(expected.Color)))
        {
            yield return $"{what} foreground {found.Foreground}, shadcn {CssColor.Parse(expected.Color)}";
        }

        if (Math.Abs(found.BorderWidth - width) > 0.01 || (width > 0 && !found.Border.Near(CssColor.Parse(expected.BorderColor))))
        {
            yield return $"{what} border {found.Border} {found.BorderWidth}, shadcn {CssColor.Parse(expected.BorderColor)} {width}";
        }
    }

    [Fact]
    [Trait("AC", "BUTTON-02")]
    public async Task EveryVariantTakesShadcnsColorsAtRestAndUnderThePointerAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var normal = await ReferenceMetrics.OfAsync("button-variants", name);
            var hover = await ReferenceMetrics.OfAsync("button-variants", name, "hover");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var buttons = Variants.Select(classes => new Button { Content = "Button", Classes = { classes } }).ToList();
                buttons.ForEach(button => button.Classes.Remove(""));
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(24) };
                buttons.ForEach(row.Children.Add);
                var window = Windows.Show(row, variant);
                var errors = new List<string>();
                for (var i = 0; i < buttons.Count; i++)
                {
                    errors.AddRange(Differences($"{name} {Variants[i]} rest", Read(buttons[i]), normal[i]));
                    Windows.Hover(window, buttons[i]);
                    errors.AddRange(Differences($"{name} {Variants[i]} hover", Read(buttons[i]), hover[i]));
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "BUTTON-06")]
    public async Task TheKeyboardFocusShowsShadcnsRingAsync()
    {
        var found = await desktop.RunAsync(() => Themes.SelectMany(theme =>
        {
            var buttons = Focused.Select(classes => new Button { Content = "Button", Classes = { classes } }).ToList();
            buttons.ForEach(button => button.Classes.Remove(""));
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            buttons.ForEach(row.Children.Add);
            var window = Windows.Show(row, theme.Variant);
            var rings = buttons.Select(button =>
            {
                button.Focus(NavigationMethod.Tab);
                Windows.Settle();
                var presenter = Windows.Part<ContentPresenter>(button, "PART_ContentPresenter");
                return (theme.Name, Variant: button.Classes.FirstOrDefault(name => !name.StartsWith(':')) ?? "default", Pseudo: button.Classes.Contains(":focus-visible"), Shadow: presenter.BoxShadow.ToString(), Border: Windows.Paint(presenter.BorderBrush));
            }).ToList();
            window.Close();
            return rings;
        }).ToList());

        Assert.All(found, ring => Assert.True(ring.Pseudo, $"{ring.Name} {ring.Variant} has no :focus-visible"));
        Assert.Equal(
            [
                ("light", "default", "0 0 0 3 #80a1a1a1", "#A1A1A1"), ("light", "destructive", "0 0 0 3 #33e7000b", "#A1A1A1"), ("light", "outline", Shadow("0 0 0 3 #80a1a1a1, 0 1 2 0 #0d000000"), "#A1A1A1"),
                ("dark", "default", "0 0 0 3 #80737373", "#737373"), ("dark", "destructive", "0 0 0 3 #66ff6467", "#737373"), ("dark", "outline", Shadow("0 0 0 3 #80737373, 0 1 2 0 #0d000000"), "#26FFFFFF"),
            ],
            found.Select(ring => (ring.Name, ring.Variant, ring.Shadow.ToLowerInvariant(), Hex(ring.Border))));
    }

    private static string Shadow(string text) => BoxShadows.Parse(text).ToString().ToLowerInvariant();

    private static string Hex(CssColor color) =>
        "#" + (color.A >= 0.999 ? "" : ((int)Math.Round(color.A * 255)).ToString("X2", System.Globalization.CultureInfo.InvariantCulture)) + $"{color.R:X2}{color.G:X2}{color.B:X2}";

    [Fact]
    [Trait("AC", "BUTTON-07")]
    public async Task ADisabledButtonIsHalfTransparentAndIgnoresThePointerAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var runs = 0;
            var command = new Command(() => runs++);
            var buttons = Variants.Select(classes => new Button { Content = "Button", Classes = { classes }, IsEnabled = false, Command = command }).ToList();
            buttons.ForEach(button => button.Classes.Remove(""));
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            buttons.ForEach(row.Children.Add);
            var window = Windows.Show(row, ThemeVariant.Light);
            var states = buttons.Select(button =>
            {
                Windows.Click(window, button);
                return (button.Opacity, Over: button.Classes.Contains(":pointerover"));
            }).ToList();
            window.Close();
            return (states, runs);
        });

        Assert.All(found.states, state => Assert.Equal((0.5, false), state));
        Assert.Equal(0, found.runs);
    }

    [Fact]
    [Trait("AC", "BUTTON-08")]
    public async Task IconAndTextContentIsLaidOutInARowWithShadcnsGapAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            Button Make(string classes)
            {
                var button = new Button { Content = new StackPanel { Children = { new Border { Width = 16, Height = 16 }, new TextBlock { Text = "Mail" } } } };
                button.Classes.AddRange(classes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                return button;
            }

            var buttons = IconClasses.Select(Make).ToList();
            var own = new Button { Content = new StackPanel { Spacing = 20, Children = { new TextBlock { Text = "A" }, new TextBlock { Text = "B" } } } };
            var column = new StackPanel { Spacing = 8 };
            buttons.ForEach(column.Children.Add);
            column.Children.Add(own);
            var window = Windows.Show(column, ThemeVariant.Light);
            var result = buttons.Select(button => button.Content is StackPanel panel ? (panel.Orientation, panel.Spacing, button.Padding.Left) : default).ToList();
            var custom = own.Content is StackPanel ownPanel ? ownPanel.Spacing : 0;
            window.Close();
            return (result, custom);
        });

        Assert.Equal([(Orientation.Horizontal, 8.0, 12.0), (Orientation.Horizontal, 6.0, 10.0), (Orientation.Horizontal, 4.0, 6.0), (Orientation.Horizontal, 8.0, 16.0), (Orientation.Horizontal, 8.0, 16.0)], found.result);
        Assert.Equal(20, found.custom);
    }

    [Fact]
    [Trait("AC", "BUTTON-14")]
    public async Task EveryExtraSizeHasShadcnsBoxAsync() =>
        Assert.Empty(await FixtureAsync(desktop, "button-extra-sizes", (_, root, reference, theme) =>
        {
            var buttons = All<Button>(root);
            var expected = reference.Where(element => element.Tag == "button").ToList();
            var errors = buttons.Count == expected.Count ? [] : new List<string> { $"{theme}: {buttons.Count} buttons, shadcn {expected.Count}" };
            foreach (var (button, element) in buttons.Zip(expected))
            {
                var box = Windows.Box(button, root);
                errors.AddRange(Math.Abs(box.X - element.X) <= 2 && Math.Abs(box.Y - element.Y) <= 1 && Math.Abs(box.Width - element.Width) <= 1 && Math.Abs(box.Height - element.Height) <= 1 ? [] : [$"{theme} {element.Text}: {box}, shadcn {element.X}, {element.Y}, {element.Width}, {element.Height}"]);
            }

            errors.AddRange(buttons.Where(button => Aioros.Markup.IconStyle.GetSize(button) != (button.Classes.Contains("xs") || button.Classes.Contains("icon-xs") ? 12 : 16))
                .Select(button => $"{theme} {string.Join(' ', button.Classes)}: icons {Aioros.Markup.IconStyle.GetSize(button)} px"));
            return errors;
        }));
}
