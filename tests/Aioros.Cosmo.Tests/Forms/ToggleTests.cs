using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Forms;

public sealed class ToggleTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static readonly string[] Kinds = ["", "on", "outline", "sm", "lg", "off", "outline"];

    private static IEnumerable<string> Look(string what, CssColor background, CssColor border, IReadOnlyList<CssShadow> shadow, ElementMetrics expected, bool borderShown = true)
    {
        if (!background.Near(CssColor.Parse(expected.Background)))
        {
            yield return $"{what}: background {background}, shadcn {expected.Background}";
        }

        if (borderShown && !border.Near(CssColor.Parse(expected.BorderColor)))
        {
            yield return $"{what}: border {border}, shadcn {expected.BorderColor}";
        }

        if (!CssShadow.Same(shadow, CssShadow.List(expected.Shadow)))
        {
            yield return $"{what}: shadow {string.Join(", ", shadow)}, shadcn {expected.Shadow}";
        }
    }

    private static IEnumerable<string> Radio(string what, RadioButton radio, ElementMetrics expected)
    {
        var circle = Windows.Part<Border>(radio, "PART_Border");
        return Look(what, Windows.Paint(circle.Background), Windows.Paint(circle.BorderBrush), Windows.Shadows(circle.BoxShadow), expected)
            .Concat(circle.Bounds.Width == 16 && circle.Bounds.Height == 16 && circle.CornerRadius.TopLeft == 8 ? [] : [$"{what}: circle {circle.Bounds.Size}"]);
    }

    [Fact]
    [Trait("AC", "RADIO-03")]
    public async Task EveryStateOfARadioButtonIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var normal = await ReferenceMetrics.SlotsAsync("radio-group", name, "radio-group-item");
            var focus = await ReferenceMetrics.SlotsAsync("radio-group", name, "radio-group-item", "focus");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var radios = new[] { new RadioButton { Content = "Default" }, new RadioButton { Content = "Comfortable", IsChecked = true }, new RadioButton { Content = "Compact", IsEnabled = false } };
                var panel = new StackPanel { Spacing = 12 };
                panel.Children.AddRange(radios);
                var window = Windows.Show(panel, variant);
                var errors = radios.SelectMany((radio, i) => Radio($"{name} #{i}", radio, normal[i])).ToList();
                var dot = Windows.Part<Ellipse>(radios[1], "CheckGlyph");
                var circle = Windows.Part<Border>(radios[1], "PART_Border");
                var center = Windows.Box(dot, circle).Center;
                var primary = CssColor.Parse(name == "light" ? "#171717" : "#E5E5E5");
                if (!dot.IsVisible || dot.Bounds.Width != 8 || Math.Abs(center.X - 8) > 0.5 || Math.Abs(center.Y - 8) > 0.5 || !Windows.Paint(dot.Fill).Near(primary) || Windows.Part<Ellipse>(radios[0], "CheckGlyph").IsVisible)
                {
                    errors.Add($"{name}: dot {dot.Bounds} at {center} {Windows.Paint(dot.Fill)} visible {dot.IsVisible}");
                }

                var content = (Windows.Box(Windows.Part<ContentPresenter>(radios[0], "PART_ContentPresenter"), radios[0]).X, radios[0].FontSize, radios[0].FontWeight, radios[2].Opacity);
                if (content != (28.0, 14.0, Avalonia.Media.FontWeight.Medium, 0.5))
                {
                    errors.Add($"{name}: content {content}");
                }

                for (var i = 0; i < 2; i++)
                {
                    radios[i].Focus(NavigationMethod.Tab);
                    Windows.Settle();
                    errors.AddRange(Radio($"{name} #{i} focus", radios[i], focus[i]));
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "RADIO-04")]
    public async Task OneRadioIsCheckedAndTheModelFollowsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new FormsView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var radios = view.FindControl<StackPanel>("Radios")!.Children.OfType<RadioButton>().ToList();
            var steps = new List<string>();
            void Read(string step) => steps.Add($"{step} {string.Concat(radios.Select(radio => radio.IsChecked == true ? "x" : "-"))} {model.Second}");
            Windows.Click(window, radios[0]);
            Read("click1");
            Windows.Key(window, Key.Down);
            Read("down");
            Windows.Key(window, Key.Up);
            Read("up");
            Windows.Click(window, radios[1]);
            Read("click2");
            window.Close();
            return steps;
        });

        Assert.Equal(["click1 x-- False", "down -x- True", "up x-- False", "click2 -x- True"], found);
    }

    private static IEnumerable<string> Switch(string what, ToggleSwitch toggle, ElementMetrics track, ElementMetrics thumb, double thumbX)
    {
        var outer = Windows.Part<Border>(toggle, "OuterBorder");
        var knob = Windows.Part<Ellipse>(toggle, "SwitchKnob");
        var position = Windows.Box(knob, outer);
        return Look(what, Windows.Paint(outer.Background), Windows.Paint(outer.BorderBrush), Windows.Shadows(outer.BoxShadow), track)
            .Concat(Windows.Paint(knob.Fill).Near(CssColor.Parse(thumb.Background)) ? [] : [$"{what}: thumb {Windows.Paint(knob.Fill)}, shadcn {thumb.Background}"])
            .Concat(Math.Abs(outer.Bounds.Width - track.Width) < 0.01 && Math.Abs(outer.Bounds.Height - Math.Ceiling(track.Height)) < 0.01 && knob.Bounds.Width == thumb.Width
                ? [] : [$"{what}: track {outer.Bounds.Size} thumb {knob.Bounds.Width}, shadcn {track.Width}x{track.Height} {thumb.Width}"])
            .Concat(Math.Abs(position.X - thumbX) < 0.01 ? [] : [$"{what}: thumb at {position.X}, shadcn {thumbX}"]);
    }

    [Fact]
    [Trait("AC", "SWITCH-03")]
    public async Task EveryStateOfASwitchIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var tracks = await ReferenceMetrics.SlotsAsync("switch-states", name, "switch");
            var focus = await ReferenceMetrics.SlotsAsync("switch-states", name, "switch", "focus");
            var thumbs = await ReferenceMetrics.SlotsAsync("switch-states", name, "switch-thumb");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var switches = new[] { new ToggleSwitch(), new ToggleSwitch { IsChecked = true }, new ToggleSwitch { IsEnabled = false }, new ToggleSwitch { IsChecked = true, Classes = { "sm" } } };
                var panel = new StackPanel { Spacing = 24 };
                panel.Children.AddRange(switches);
                var window = Windows.Show(panel, variant);
                var errors = switches.SelectMany((toggle, i) => Switch($"{name} #{i}", toggle, tracks[i], thumbs[i], thumbs[i].X - tracks[i].X)).ToList();
                errors.AddRange(switches[2].Opacity == 0.5 ? [] : [$"{name}: disabled {switches[2].Opacity}"]);
                for (var i = 0; i < 2; i++)
                {
                    switches[i].Focus(NavigationMethod.Tab);
                    Windows.Settle();
                    var outer = Windows.Part<Border>(switches[i], "OuterBorder");
                    errors.AddRange(Look($"{name} #{i} focus", Windows.Paint(outer.Background), Windows.Paint(outer.BorderBrush), Windows.Shadows(outer.BoxShadow), focus[i]));
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "SWITCH-04")]
    public async Task MouseAndKeyboardToggleTheBoundSwitchAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new FormsView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var toggle = view.FindControl<ToggleSwitch>("Airplane")!;
            var steps = new List<bool>();
            Windows.Click(window, Windows.Part<Border>(toggle, "OuterBorder"));
            steps.Add(model.Airplane);
            toggle.Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Space, " ");
            steps.Add(model.Airplane);
            window.Close();
            return (steps, model.Toggles, model.LastParameter);
        });

        Assert.Equal([true, false], found.steps);
        Assert.Equal(2, found.Toggles);
        Assert.Equal("airplane", found.LastParameter);
    }

    private static IEnumerable<string> Toggle(string what, Control toggle, ElementMetrics expected)
    {
        var presenter = Windows.Part<ContentPresenter>(toggle, "PART_ContentPresenter");
        var width = double.Parse(expected.BorderWidth.TrimEnd('p', 'x'), System.Globalization.CultureInfo.InvariantCulture);
        return Look(what, Windows.Paint(presenter.Background), Windows.Paint(presenter.BorderBrush), Windows.Shadows(presenter.BoxShadow), expected, width > 0)
            .Concat(Windows.Paint(presenter.Foreground).Near(CssColor.Parse(expected.Color)) ? [] : [$"{what}: color {Windows.Paint(presenter.Foreground)}, shadcn {expected.Color}"])
            .Concat(presenter.BorderThickness.Top == width ? [] : [$"{what}: border width {presenter.BorderThickness.Top}, shadcn {width}"]);
    }

    [Fact]
    [Trait("AC", "TOGGLE-03")]
    public async Task EveryStateOfAToggleButtonIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var normal = await ReferenceMetrics.SlotsAsync("toggle-states", name, "toggle");
            var hover = await ReferenceMetrics.SlotsAsync("toggle-states", name, "toggle", "hover");
            var focus = await ReferenceMetrics.SlotsAsync("toggle-states", name, "toggle", "focus");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var toggles = Kinds.Select(kind => new ToggleButton
                {
                    Content = new Border { Width = 16, Height = 16 },
                    IsChecked = kind == "on",
                    IsEnabled = kind != "off",
                    Classes = { kind is "outline" or "sm" or "lg" ? kind : "plain" },
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                }).ToList();
                var row = new WrapPanel { ItemSpacing = 8, LineSpacing = 8 };
                row.Children.AddRange(toggles);
                var window = Windows.Show(row, variant);
                var errors = new List<string>();
                for (var i = 0; i < toggles.Count - 1; i++)
                {
                    errors.AddRange(Toggle($"{name} #{i}", toggles[i], normal[i]));
                    errors.AddRange(Math.Abs(toggles[i].Bounds.Height - normal[i].Height) < 0.01 && toggles[i].Padding.Left.ToString(System.Globalization.CultureInfo.InvariantCulture) + "px" == normal[i].PaddingLeft
                        ? [] : [$"{name} #{i}: height {toggles[i].Bounds.Height} padding {toggles[i].Padding.Left}, shadcn {normal[i].Height} {normal[i].PaddingLeft}"]);
                    if (i != 5)
                    {
                        Windows.Hover(window, toggles[i]);
                        errors.AddRange(Toggle($"{name} #{i} hover", toggles[i], hover[i]));
                        window.MouseMove(new Point(790, 1190), RawInputModifiers.None);
                        toggles[i].Focus(NavigationMethod.Tab);
                        Windows.Settle();
                        errors.AddRange(Toggle($"{name} #{i} focus", toggles[i], focus[i]));
                    }
                }

                errors.AddRange(toggles[5].Opacity == 0.5 ? [] : [$"{name}: disabled {toggles[5].Opacity}"]);
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TOGGLE-04")]
    public async Task MouseAndKeyboardToggleTheBoundToggleButtonAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new FormsView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var toggle = view.FindControl<ToggleButton>("Bold")!;
            var steps = new List<bool>();
            Windows.Click(window, toggle);
            steps.Add(model.Bold);
            toggle.Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Space, " ");
            steps.Add(model.Bold);
            window.Close();
            return (steps, model.Toggles, model.LastParameter);
        });

        Assert.Equal([true, false], found.steps);
        Assert.Equal(2, found.Toggles);
        Assert.Equal("bold", found.LastParameter);
    }

    [Fact]
    [Trait("AC", "TOGGLEGROUP-03")]
    public async Task TheToggleGroupItemsAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var groups = await ReferenceMetrics.SlotsAsync("toggle-group", name, "toggle-group");
            var items = await ReferenceMetrics.SlotsAsync("toggle-group", name, "toggle-group-item");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var lists = new[] { ("toggle-group outline", SelectionMode.Multiple | SelectionMode.Toggle, 1, new object[] { new Border { Width = 16, Height = 16 }, new Border { Width = 16, Height = 16 }, new Border { Width = 16, Height = 16 } }), ("toggle-group", SelectionMode.Single | SelectionMode.Toggle, 1, new object[] { "Left", "Center", "Right" }), ("toggle-group outline sm spacing", SelectionMode.Single | SelectionMode.Toggle, 0, new object[] { "Day", "Week", "Month" }) }
                    .Select(spec =>
                    {
                        var list = new ListBox { SelectionMode = spec.Item2 };
                        list.Classes.AddRange(spec.Item1.Split(' '));
                        foreach (var content in spec.Item4)
                        {
                            list.Items.Add(new ListBoxItem { Content = content });
                        }

                        list.SelectedIndex = spec.Item3;
                        return list;
                    }).ToList();
                var panel = new StackPanel { Spacing = 24 };
                panel.Children.AddRange(lists);
                var window = Windows.Show(panel, variant);
                var errors = new List<string>();
                var index = 0;
                for (var g = 0; g < lists.Count; g++)
                {
                    foreach (var item in lists[g].GetRealizedContainers().OfType<ListBoxItem>())
                    {
                        errors.AddRange(Item($"{name} group {g} item {index}", item, lists[g], items[index], groups[g]));
                        index++;
                    }
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static List<string> Item(string what, ListBoxItem item, ListBox list, ElementMetrics expected, ElementMetrics group)
    {
        var box = Windows.Box(item, list);
        var target = new Rect(expected.X - group.X, expected.Y - group.Y, expected.Width, expected.Height);
        var errors = Toggle(what, item, expected).ToList();
        if (Math.Abs(box.X - target.X) > 2 || Math.Abs(box.Y - target.Y) > 0.5 || Math.Abs(box.Width - target.Width) > 2 || Math.Abs(box.Height - target.Height) > 0.5)
        {
            errors.Add($"{what}: {box}, shadcn {target}");
        }

        var radius = Windows.Part<ContentPresenter>(item, "PART_ContentPresenter").CornerRadius.TopLeft;
        if (radius.ToString(System.Globalization.CultureInfo.InvariantCulture) + "px" != expected.Radius)
        {
            errors.Add($"{what}: radius {radius}, shadcn {expected.Radius}");
        }

        return errors;
    }

    [Fact]
    [Trait("AC", "TOGGLEGROUP-04")]
    public async Task TheToggleGroupsSelectOneOrSeveralItemsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new FormsView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var single = view.FindControl<ListBox>("Alignment")!;
            var multiple = view.FindControl<ListBox>("Formats")!;
            var steps = new List<string>();
            var singles = single.GetRealizedContainers().OfType<ListBoxItem>().ToList();
            Windows.Click(window, singles[0]);
            steps.Add($"{model.Alignment} {single.SelectedItems?.Count}");
            Windows.Click(window, singles[2]);
            steps.Add($"{model.Alignment} {single.SelectedItems?.Count}");
            var corner = singles[2].TranslatePoint(new Point(4, 4), window) ?? default;
            window.MouseDown(corner, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(corner, MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            steps.Add($"{model.Alignment ?? "none"} {single.SelectedItems?.Count}");
            var multiples = multiple.GetRealizedContainers().OfType<ListBoxItem>().ToList();
            Windows.Click(window, multiples[0]);
            Windows.Click(window, multiples[1]);
            steps.Add($"multiple {multiple.SelectedItems?.Count}");
            var shown = singles[1].GetVisualDescendants().OfType<TextBlock>().First().Text;
            window.Close();
            return (steps, shown);
        });

        Assert.Equal(["Left 1", "Right 1", "none 0", "multiple 2"], found.steps);
        Assert.Equal("Align Center", found.shown);
    }
}
