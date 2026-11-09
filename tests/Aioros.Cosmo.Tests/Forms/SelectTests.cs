using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Path = Avalonia.Controls.Shapes.Path;
using Presenter = Avalonia.Controls.Presenters.ContentPresenter;

namespace Aioros.Cosmo.Tests.Forms;

public sealed class SelectTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static ComboBox Combo(string? placeholder, params string[] items)
    {
        var combo = new ComboBox { Width = 180, PlaceholderText = placeholder };
        foreach (var item in items)
        {
            combo.Items.Add(new ComboBoxItem { Content = item });
        }

        combo.SelectedIndex = placeholder is null ? 0 : -1;
        return combo;
    }

    private static IEnumerable<string> Trigger(string what, ComboBox combo, ElementMetrics expected)
    {
        var part = Windows.Part<Border>(combo, "Background");
        if (!Windows.Paint(part.Background).Near(CssColor.Parse(expected.Background)))
        {
            yield return $"{what}: background {Windows.Paint(part.Background)}, shadcn {expected.Background}";
        }

        if (!Windows.Paint(part.BorderBrush).Near(CssColor.Parse(expected.BorderColor)))
        {
            yield return $"{what}: border {Windows.Paint(part.BorderBrush)}, shadcn {expected.BorderColor}";
        }

        if (!CssShadow.Same(Windows.Shadows(part.BoxShadow), CssShadow.List(expected.Shadow)))
        {
            yield return $"{what}: shadow {string.Join(", ", Windows.Shadows(part.BoxShadow))}, shadcn {expected.Shadow}";
        }
    }

    [Fact]
    [Trait("AC", "SELECT-01")]
    public async Task EveryStateOfTheTriggerIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var normal = await ReferenceMetrics.SlotsAsync("select-trigger", name, "select-trigger");
            var hover = await ReferenceMetrics.SlotsAsync("select-trigger", name, "select-trigger", "hover");
            var focus = await ReferenceMetrics.SlotsAsync("select-trigger", name, "select-trigger", "focus");
            var native = (await ReferenceMetrics.SlotsAsync("native-select", name, "native-select-icon"))[0];
            var nativeSelect = (await ReferenceMetrics.SlotsAsync("native-select", name, "native-select"))[0];
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var combos = new[] { Combo("Select a fruit", "Apple"), Combo(null, "Banana"), Combo(null, "Banana"), Combo(null, "Banana") };
                combos[2].Classes.Add("sm");
                combos[3].IsEnabled = false;
                var nativeCombo = Combo(null, "Select status", "Todo", "Done");
                nativeCombo.Width = double.NaN;
                nativeCombo.Classes.Add("native");
                var panel = new StackPanel { Spacing = 16, Margin = new Thickness(24), Children = { combos[0], combos[1], combos[2], combos[3], nativeCombo } };
                var window = Windows.Show(panel, variant);
                var errors = new List<string>();
                for (var i = 0; i < combos.Length; i++)
                {
                    var box = Windows.Box(combos[i], panel);
                    var expected = new Rect(normal[i].X - 24, normal[i].Y - 24, normal[i].Width, normal[i].Height);
                    if (Math.Abs(box.Y - expected.Y) > 0.5 || Math.Abs(box.Height - expected.Height) > 0.5 || Math.Abs(box.Width - expected.Width) > 0.5)
                    {
                        errors.Add($"{name} #{i}: {box}, shadcn {expected}");
                    }

                    errors.AddRange(Trigger($"{name} #{i} rest", combos[i], normal[i]));
                    if (i < 3)
                    {
                        Windows.Hover(window, combos[i]);
                        errors.AddRange(Trigger($"{name} #{i} hover", combos[i], hover[i]));
                        window.MouseMove(new Point(790, 590), Avalonia.Input.RawInputModifiers.None);
                        combos[i].Focus(NavigationMethod.Tab);
                        Windows.Settle();
                        errors.AddRange(Trigger($"{name} #{i} focus", combos[i], focus[i]));
                    }
                }

                var placeholder = Windows.Part<TextBlock>(combos[0], "PlaceholderTextBlock");
                if (!placeholder.IsVisible || !Windows.Paint(placeholder.Foreground).Near(CssColor.Parse(normal[0].Color)))
                {
                    errors.Add($"{name}: placeholder {Windows.Paint(placeholder.Foreground)} visible {placeholder.IsVisible}, shadcn {normal[0].Color}");
                }

                var glyph = Windows.Part<Path>(combos[1], "DropDownGlyph");
                var nativeGlyph = Windows.Part<Path>(nativeCombo, "DropDownGlyph");
                var right = combos[1].Bounds.Width - Windows.Box(glyph, combos[1]).Right;
                var nativeRight = nativeCombo.Bounds.Width - Windows.Box(nativeGlyph, nativeCombo).Right;
                var expectedNative = nativeSelect.X + nativeSelect.Width - native.X - native.Width;
                if (right != 12 || nativeRight != expectedNative || glyph.Bounds.Width != 16 || glyph.Opacity != 0.5 || !Windows.Paint(glyph.Stroke).Near(CssColor.Parse(native.Color)) || combos[3].Opacity != 0.5)
                {
                    errors.Add($"{name}: chevron {right} / native {nativeRight} (shadcn {expectedNative}), {glyph.Bounds.Width} px, {glyph.Opacity}, {Windows.Paint(glyph.Stroke)}; disabled {combos[3].Opacity}");
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "SELECT-02")]
    public async Task TheDropDownIsShadcnsSelectContentAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("select-open", name);
            var content = reference.Single(element => element.Slot == "select-content");
            var expected = reference.Where(element => element.Slot is "select-label" or "select-item" or "select-separator").ToList();
            var indicator = reference.Where(element => element.Slot == "select-item-indicator").ToList()[1];
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var combo = new ComboBox { Width = 180 };
                combo.Items.Add(new ComboBoxItem { Content = "Fruits", Classes = { "label" } });
                combo.Items.Add(new ComboBoxItem { Content = "Apple" });
                combo.Items.Add(new ComboBoxItem { Content = "Banana" });
                combo.Items.Add(new ComboBoxItem { Content = "Grapes", IsEnabled = false });
                combo.Items.Add(new ComboBoxItem { Classes = { "separator" } });
                combo.Items.Add(new ComboBoxItem { Content = "Vegetables", Classes = { "label" } });
                combo.Items.Add(new ComboBoxItem { Content = "Carrot" });
                combo.SelectedIndex = 2;
                var window = Windows.Show(new StackPanel { Margin = new Thickness(24), Children = { combo } }, variant);
                combo.IsDropDownOpen = true;
                Windows.Settle();
                var surface = (Border)Windows.Part<Popup>(combo, "PART_Popup").Child!;
                var items = combo.GetRealizedContainers().OfType<ComboBoxItem>().ToList();
                var errors = Surface(name, surface, content).Concat(items.SelectMany((item, i) => Item($"{name} item #{i}", item, surface, expected[i], content, i))).ToList();
                errors.AddRange(Indicator(name, items, surface, indicator, content));
                Windows.Hover(window, items[1]);
                var accent = reference.Where(element => element.Slot == "select-item").ToList()[1];
                var hovered = Windows.Part<Presenter>(items[1], "PART_ContentPresenter");
                if (!Windows.Paint(hovered.Background).Near(CssColor.Parse(accent.Background)) || !Windows.Paint(items[1].Foreground).Near(CssColor.Parse(accent.Color)))
                {
                    errors.Add($"{name}: hovered {Windows.Paint(hovered.Background)} {Windows.Paint(items[1].Foreground)}, shadcn {accent.Background} {accent.Color}");
                }

                combo.IsDropDownOpen = false;
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Surface(string name, Border surface, ElementMetrics content)
    {
        var look = (Windows.Paint(surface.Background), Windows.Paint(surface.BorderBrush), surface.BorderThickness.Top, surface.CornerRadius.TopLeft, Windows.Shadows(surface.BoxShadow));
        if (!look.Item1.Near(CssColor.Parse(content.Background)) || !look.Item2.Near(CssColor.Parse(content.BorderColor)) || look.Item3 != 1 || look.Item4 != 8 || !CssShadow.Same(look.Item5, CssShadow.List(content.Shadow)))
        {
            yield return $"{name}: surface {look.Item1} {look.Item2} {look.Item3} {look.Item4} {string.Join(", ", look.Item5)}, shadcn {content.Background} {content.BorderColor} {content.Shadow}";
        }

        if (Math.Abs(surface.Bounds.Width - content.Width) > 0.5 || Math.Abs(surface.Bounds.Height - content.Height) > 0.5)
        {
            yield return $"{name}: surface {surface.Bounds.Size}, shadcn {content.Width}x{content.Height}";
        }
    }

    private static IEnumerable<string> Item(string what, ComboBoxItem item, Border surface, ElementMetrics expected, ElementMetrics content, int index)
    {
        var box = Windows.Box(item, surface);
        var target = new Rect(expected.X - content.X, expected.Y - content.Y, expected.Width, expected.Height);
        if (Math.Abs(box.X - target.X) > 0.5 || Math.Abs(box.Y - target.Y) > 0.5 || Math.Abs(box.Width - target.Width) > 0.5 || Math.Abs(box.Height - target.Height) > 0.5)
        {
            yield return $"{what}: {box}, shadcn {target}";
        }

        var separator = item.Classes.Contains("separator");
        var fill = Windows.Paint(separator ? item.Background : Windows.Part<Presenter>(item, "PART_ContentPresenter").Background);
        var selected = index == 2;
        if (!selected && !fill.Near(CssColor.Parse(expected.Background)))
        {
            yield return $"{what}: background {fill}, shadcn {expected.Background}";
        }

        var opacity = item.Opacity.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (!separator && ((!selected && !Windows.Paint(item.Foreground).Near(CssColor.Parse(expected.Color))) || opacity != expected.Opacity))
        {
            yield return $"{what}: color {Windows.Paint(item.Foreground)} opacity {opacity}, shadcn {expected.Color} {expected.Opacity}";
        }
    }

    private static IEnumerable<string> Indicator(string name, List<ComboBoxItem> items, Border surface, ElementMetrics indicator, ElementMetrics content)
    {
        var check = Windows.Part<Path>(items[2], "Indicator");
        var box = Windows.Box(check, surface);
        var expected = new Point(indicator.X - content.X - 1, indicator.Y - content.Y - 1);
        if (!check.IsVisible || Math.Abs(box.X - expected.X) > 0.5 || Math.Abs(box.Y - expected.Y) > 0.5 || Windows.Part<Path>(items[1], "Indicator").IsVisible)
        {
            yield return $"{name}: check {box} visible {check.IsVisible}, shadcn at {expected}";
        }
    }

    [Fact]
    [Trait("AC", "SELECT-03")]
    public async Task TheKeyboardMovesTypesAheadOpensAndClosesAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var combo = new ComboBox { Width = 180 };
            foreach (var (text, enabled) in new[] { ("Apple", true), ("Banana", true), ("Grapes", false), ("Carrot", true) })
            {
                combo.Items.Add(new ComboBoxItem { Content = text, IsEnabled = enabled });
            }

            combo.SelectedIndex = 0;
            var window = Windows.Show(new StackPanel { Margin = new Thickness(24), Children = { combo } }, ThemeVariant.Light);
            combo.Focus(NavigationMethod.Tab);
            Windows.Settle();
            var steps = new List<string>();
            Windows.Key(window, Key.Down);
            steps.Add($"down {combo.SelectedIndex}");
            Windows.Key(window, Key.Down);
            steps.Add($"down {combo.SelectedIndex}");
            Windows.Key(window, Key.Up);
            steps.Add($"up {combo.SelectedIndex}");
            Windows.Type(window, "a");
            steps.Add($"a {combo.SelectedIndex}");
            Windows.Key(window, Key.Space, " ");
            steps.Add($"space {combo.IsDropDownOpen}");
            Windows.Key(window, Key.Escape);
            steps.Add($"escape {combo.IsDropDownOpen}");
            Windows.Key(window, Key.Enter);
            steps.Add($"enter {combo.IsDropDownOpen}");
            Windows.Key(window, Key.Enter);
            steps.Add($"enter {combo.IsDropDownOpen}");
            window.Close();
            return steps;
        });

        Assert.Equal(["down 1", "down 3", "up 1", "a 0", "space True", "escape False", "enter True", "enter False"], found);
    }

    [Fact]
    [Trait("AC", "SELECT-04")]
    public async Task TheSelectionAndItsValueBindTwoWayThroughTheTemplateAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new FormsView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var combo = view.FindControl<ComboBox>("Fruits")!;
            combo.IsDropDownOpen = true;
            Windows.Settle();
            var banana = combo.GetRealizedContainers().OfType<ComboBoxItem>().ElementAt(1);
            Windows.Click(window, banana);
            var chosen = (model.Fruit?.Name, model.Code, Shown(combo));
            model.Code = "carrot";
            Windows.Settle();
            var set = (combo.SelectedIndex, model.Fruit?.Name, Shown(combo));
            window.Close();
            return (chosen, set);
        });

        Assert.Equal(("Banana", (string?)"banana", "Fruit: Banana"), found.chosen);
        Assert.Equal((2, (string?)"Carrot", "Fruit: Carrot"), found.set);
    }

    private static string? Shown(ComboBox combo) =>
        Windows.Part<ContentControl>(combo, "ContentPresenter").GetVisualDescendants().OfType<TextBlock>().FirstOrDefault()?.Text;
}
