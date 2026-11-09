using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Path = Avalonia.Controls.Shapes.Path;

namespace Aioros.Cosmo.Tests.Forms;

public sealed class ChoiceTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static IEnumerable<string> Box(string what, CheckBox check, ElementMetrics expected)
    {
        var box = Windows.Part<Border>(check, "PART_Border");
        var found = (Windows.Paint(box.Background), Windows.Paint(box.BorderBrush), Windows.Shadows(box.BoxShadow));
        if (!found.Item1.Near(CssColor.Parse(expected.Background)) || !found.Item2.Near(CssColor.Parse(expected.BorderColor)) || !CssShadow.Same(found.Item3, CssShadow.List(expected.Shadow)))
        {
            yield return $"{what}: {found.Item1} {found.Item2} {string.Join(", ", found.Item3)}, shadcn {expected.Background} {expected.BorderColor} {expected.Shadow}";
        }

        if (box.Bounds.Width != 16 || box.Bounds.Height != 16 || box.CornerRadius.TopLeft != 4)
        {
            yield return $"{what}: box {box.Bounds.Size} radius {box.CornerRadius.TopLeft}";
        }
    }

    [Fact]
    [Trait("AC", "CHECKBOX-03")]
    public async Task EveryStateOfACheckBoxIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var normal = await ReferenceMetrics.SlotsAsync("checkbox-states", name, "checkbox");
            var focus = await ReferenceMetrics.SlotsAsync("checkbox-states", name, "checkbox", "focus");
            var indicators = await ReferenceMetrics.SlotsAsync("checkbox-states", name, "checkbox-indicator");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var checks = new[] { new CheckBox { Content = "Accept terms and conditions" }, new CheckBox { Content = "Enable notifications", IsChecked = true }, new CheckBox { Content = "Select all", IsThreeState = true, IsChecked = null }, new CheckBox { Content = "Disabled", IsEnabled = false } };
                var panel = new StackPanel { Spacing = 24 };
                panel.Children.AddRange(checks);
                var window = Windows.Show(panel, variant);
                var errors = checks.SelectMany((check, i) => Box($"{name} #{i}", check, normal[i])).ToList();
                foreach (var (check, indicator) in new[] { (checks[1], indicators[0]), (checks[2], indicators[1]) })
                {
                    var glyph = Windows.Part<Path>(check, "CheckGlyph");
                    if (!glyph.IsVisible || !Windows.Paint(glyph.Stroke).Near(CssColor.Parse(indicator.Color)))
                    {
                        errors.Add($"{name}: glyph {Windows.Paint(glyph.Stroke)} visible {glyph.IsVisible}, shadcn {indicator.Color}");
                    }
                }

                var content = Windows.Part<ContentPresenter>(checks[0], "PART_ContentPresenter");
                var text = (Windows.Box(content, checks[0]).X, checks[0].FontSize, checks[0].FontWeight, checks[3].Opacity, Windows.Part<Path>(checks[0], "CheckGlyph").IsVisible);
                if (text != (28.0, 14.0, FontWeight.Medium, 0.5, false))
                {
                    errors.Add($"{name}: content {text}");
                }

                for (var i = 0; i < 2; i++)
                {
                    checks[i].Focus(NavigationMethod.Tab);
                    Windows.Settle();
                    errors.AddRange(Box($"{name} #{i} focus", checks[i], focus[i]));
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "CHECKBOX-04")]
    public async Task MouseAndKeyboardToggleTheBoundCheckBoxAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new FormsView { DataContext = model };
            var tri = new CheckBox { IsThreeState = true, Content = "Three" };
            var window = Windows.Show(new StackPanel { Children = { view, tri } }, ThemeVariant.Light);
            var check = view.FindControl<CheckBox>("Agree")!;
            var steps = new List<bool>();
            window.MouseDown(Windows.Center(Windows.Part<Border>(check, "PART_Border")), MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(Windows.Center(Windows.Part<Border>(check, "PART_Border")), MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            steps.Add(model.Agree);
            Windows.Click(window, Windows.Part<ContentPresenter>(check, "PART_ContentPresenter"));
            steps.Add(model.Agree);
            check.Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Space, " ");
            steps.Add(model.Agree);
            var cycle = new List<bool?>();
            for (var i = 0; i < 3; i++)
            {
                Windows.Click(window, tri);
                cycle.Add(tri.IsChecked);
            }

            window.Close();
            return (steps, model.Toggles, model.LastParameter, cycle);
        });

        Assert.Equal([true, false, true], found.steps);
        Assert.Equal(3, found.Toggles);
        Assert.Equal("terms", found.LastParameter);
        Assert.Equal([true, null, false], found.cycle);
    }
}
