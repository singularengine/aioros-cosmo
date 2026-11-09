using Aioros.Cosmo.Forms;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Forms;

public sealed class RangeTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static bool Near(Rect a, Rect b, double tolerance) =>
        Math.Abs(a.X - b.X) <= tolerance && Math.Abs(a.Y - b.Y) <= tolerance && Math.Abs(a.Width - b.Width) <= tolerance && Math.Abs(a.Height - b.Height) <= tolerance;

    private static Rect Of(ElementMetrics element, ElementMetrics origin) => new(element.X - origin.X, element.Y - origin.Y, element.Width, element.Height);

    [Fact]
    [Trait("AC", "SLIDER-02")]
    public async Task EveryStateOfASliderIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var roots = await ReferenceMetrics.SlotsAsync("slider-states", name, "slider");
            var thumbs = await ReferenceMetrics.SlotsAsync("slider-states", name, "slider-thumb");
            var tracks = await ReferenceMetrics.SlotsAsync("slider-states", name, "slider-track");
            var ranges = await ReferenceMetrics.SlotsAsync("slider-states", name, "slider-range");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var sliders = new[] { new Slider { Value = 50, Maximum = 100, Width = 360 }, new Slider { Value = 33, Maximum = 100, Width = 360, IsEnabled = false }, new Slider { Value = 40, Maximum = 100, Orientation = Avalonia.Layout.Orientation.Vertical } };
                var panel = new StackPanel { Spacing = 32, Margin = new Thickness(16) };
                panel.Children.AddRange(sliders);
                var window = Windows.Show(panel, variant);
                var errors = sliders.SelectMany((slider, index) => SliderLook(name, slider, index == 0 ? 0 : index == 1 ? 2 : 3, roots, thumbs, tracks, ranges)).ToList();
                errors.AddRange(Sizes(name, sliders));
                var ring = CssShadow.List($"{(name == "light" ? "rgba(161,161,161,0.5)" : "rgba(115,115,115,0.5)")} 0px 0px 0px 4px, rgba(0, 0, 0, 0.1) 0px 1px 3px 0px, rgba(0, 0, 0, 0.1) 0px 1px 2px -1px");
                Windows.Hover(window, Windows.Part<Thumb>(sliders[0], "thumb"));
                errors.AddRange(Ringed(sliders[0], ring) ? [] : [$"{name}: no hover ring"]);
                window.MouseMove(new Point(790, 1190), RawInputModifiers.None);
                sliders[0].Focus(NavigationMethod.Tab);
                Windows.Settle();
                errors.AddRange(Ringed(sliders[0], ring) ? [] : [$"{name}: no focus ring"]);
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Sizes(string name, Slider[] sliders) =>
        sliders[1].Opacity == 0.5 && sliders[2].Bounds.Height >= 176 && sliders[0].Bounds.Height == 6 ? [] : [$"{name}: disabled {sliders[1].Opacity}, vertical {sliders[2].Bounds.Height}, height {sliders[0].Bounds.Height}"];

    private static bool Ringed(Slider slider, IReadOnlyList<CssShadow> ring) =>
        CssShadow.Same(Windows.Shadows(Windows.Part<Border>(Windows.Part<Thumb>(slider, "thumb"), "ThumbBorder").BoxShadow), ring);

    private static IEnumerable<string> SliderLook(string name, Slider slider, int i, IReadOnlyList<ElementMetrics> roots, IReadOnlyList<ElementMetrics> thumbs, IReadOnlyList<ElementMetrics> tracks, IReadOnlyList<ElementMetrics> ranges)
    {
        var thumb = Windows.Part<Thumb>(slider, "thumb");
        var border = Windows.Part<Border>(thumb, "ThumbBorder");
        var reference = thumbs[i == 0 ? 0 : i == 2 ? 3 : 4];
        var box = Windows.Box(thumb, slider);
        var expected = Of(reference, roots[i]);
        if (!Near(box, expected, 0.5))
        {
            yield return $"{name} #{i}: thumb {box}, shadcn {expected}";
        }

        var range = Windows.Paint(Windows.Part<RepeatButton>(slider, "PART_DecreaseButton").Background);
        var track = Windows.Paint(Windows.Part<RepeatButton>(slider, "PART_IncreaseButton").Background);
        if (!range.Near(CssColor.Parse(ranges[i == 0 ? 0 : i == 2 ? 2 : 3].Background)) || !track.Near(CssColor.Parse(tracks[i].Background)))
        {
            yield return $"{name} #{i}: range {range} track {track}";
        }

        if (!Windows.Paint(border.Background).Near(CssColor.Parse(reference.Background)) || !Windows.Paint(border.BorderBrush).Near(CssColor.Parse(reference.BorderColor)) || border.BorderThickness.Top != 1 || !CssShadow.Same(Windows.Shadows(border.BoxShadow), CssShadow.List(reference.Shadow)))
        {
            yield return $"{name} #{i}: thumb {Windows.Paint(border.Background)} {Windows.Paint(border.BorderBrush)} {string.Join(", ", Windows.Shadows(border.BoxShadow))}";
        }
    }

    [Fact]
    [Trait("AC", "SLIDER-03")]
    public async Task TheKeyboardAndThePointerMoveTheBoundSliderAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new InputGroupView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var slider = view.FindControl<Slider>("Volume")!;
            slider.Focus(NavigationMethod.Tab);
            var steps = new List<double>();
            foreach (var key in new[] { Key.Right, Key.Up, Key.Left, Key.Home, Key.End })
            {
                Windows.Key(window, key);
                steps.Add(model.Volume);
            }

            var point = slider.TranslatePoint(new Point(slider.Bounds.Width * 0.75, 3), window) ?? default;
            window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            steps.Add(model.Volume);
            window.Close();
            return steps;
        });

        Assert.Equal([51.0, 52.0, 51.0, 0.0, 100.0], found.Take(5));
        Assert.InRange(found[5], 70, 80);
    }

    [Fact]
    [Trait("AC", "PROGRESS-02")]
    public async Task EveryProgressBarIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var tracks = await ReferenceMetrics.SlotsAsync("progress", name, "progress");
            var indicators = await ReferenceMetrics.SlotsAsync("progress", name, "progress-indicator");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var bars = new[] { 33.0, 66, 100 }.Select(value => new ProgressBar { Value = value, Width = 360 }).ToList();
                var panel = new StackPanel { Spacing = 16 };
                panel.Children.AddRange(bars);
                var window = Windows.Show(panel, variant);
                var errors = new List<string>();
                for (var i = 0; i < bars.Count; i++)
                {
                    var root = Windows.Part<Border>(bars[i], "ProgressBarRoot");
                    var indicator = Windows.Part<Border>(bars[i], "PART_Indicator");
                    var visible = indicators[i].X + indicators[i].Width - tracks[i].X;
                    if (bars[i].Bounds.Height != 8 || root.CornerRadius.TopLeft < 4 || !Windows.Paint(root.Background).Near(CssColor.Parse(tracks[i].Background)) || !Windows.Paint(indicator.Background).Near(CssColor.Parse(indicators[i].Background)) || Math.Abs(indicator.Bounds.Width - visible) > 0.5)
                    {
                        errors.Add($"{name} #{i}: {bars[i].Bounds.Height} {root.CornerRadius} {Windows.Paint(root.Background)} {Windows.Paint(indicator.Background)} {indicator.Bounds.Width}, shadcn {tracks[i].Background} {indicators[i].Background} {visible}");
                    }
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "INPUTGROUP-01")]
    public async Task TheInputGroupsHaveShadcnsBoxesAndLookAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("input-group", name);
            var groups = reference.Where(element => element.Slot == "input-group").ToList();
            var controls = reference.Where(element => element.Slot == "input-group-control").ToList();
            var addons = reference.Where(element => element.Slot == "input-group-addon").ToList();
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var view = new InputGroupView();
                var window = Windows.Show(view, variant);
                var fixture = view.FindControl<Border>("Fixture")!;
                var origin = new ElementMetrics("div", "", 0, 0, 0, 0, "", "", "", "0px", "0px", "none", "", "", "1", "");
                var ours = view.GetVisualDescendants().OfType<InputGroup>().ToList();
                var errors = ours.SelectMany((group, g) => Group($"{name} group {g}", group, fixture, Of(groups[g], origin), groups[g], Of(controls[g], origin))).ToList();
                var ourAddons = view.GetVisualDescendants().OfType<InputGroupAddon>().ToList();
                errors.AddRange(ourAddons.Select((addon, a) => (addon, a)).Where(pair => !Near(Windows.Box(pair.addon, fixture), Of(addons[pair.a], origin), 1.5) || pair.addon.FontSize != 14 || pair.addon.FontWeight != Avalonia.Media.FontWeight.Medium || !Windows.Paint(pair.addon.Foreground).Near(CssColor.Parse(addons[pair.a].Color)))
                    .Select(pair => $"{name} addon {pair.a}: {Windows.Box(pair.addon, fixture)} {Windows.Paint(pair.addon.Foreground)}, shadcn {Of(addons[pair.a], origin)} {addons[pair.a].Color}"));
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Group(string what, InputGroup group, Control fixture, Rect expected, ElementMetrics reference, Rect control)
    {
        var border = Windows.Part<Border>(group, "PART_Border");
        if (!Near(Windows.Box(group, fixture), expected, 1))
        {
            yield return $"{what}: {Windows.Box(group, fixture)}, shadcn {expected}";
        }

        if (!Windows.Paint(border.BorderBrush).Near(CssColor.Parse(reference.BorderColor)) || !Windows.Paint(border.Background).Near(CssColor.Parse(reference.Background)) || border.CornerRadius.TopLeft != 8 || !CssShadow.Same(Windows.Shadows(border.BoxShadow), CssShadow.List(reference.Shadow)))
        {
            yield return $"{what}: look {Windows.Paint(border.BorderBrush)} {Windows.Paint(border.Background)} {border.CornerRadius}";
        }

        var box = group.Items.OfType<TextBox>().Single();
        var part = Windows.Part<Border>(box, "PART_BorderElement");
        if (!Near(Windows.Box(box, fixture), control, 1.5) || part.BorderThickness != default || Windows.Shadows(part.BoxShadow).Count != 0)
        {
            yield return $"{what} control: {Windows.Box(box, fixture)} {part.BorderThickness}, shadcn {control}";
        }
    }

    [Fact]
    [Trait("AC", "INPUTGROUP-02")]
    public async Task TheGroupShowsTheRingWhileItsTextBoxHasTheFocusAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var view = new InputGroupView();
            var window = Windows.Show(view, ThemeVariant.Light);
            var group = view.FindControl<InputGroup>("Search")!;
            var border = Windows.Part<Border>(group, "PART_Border");
            var box = view.FindControl<TextBox>("SearchBox")!;
            box.Focus(NavigationMethod.Tab);
            Windows.Settle();
            var focused = (Windows.Paint(border.BorderBrush), Windows.Shadows(border.BoxShadow), Windows.Shadows(Windows.Part<Border>(box, "PART_BorderElement").BoxShadow).Count);
            view.FindControl<Slider>("Volume")!.Focus(NavigationMethod.Tab);
            Windows.Settle();
            var left = Windows.Paint(border.BorderBrush);
            window.Close();
            return (focused, left);
        });

        Assert.Equal(CssColor.Parse("#A1A1A1"), found.focused.Item1);
        Assert.True(CssShadow.Same(found.focused.Item2, CssShadow.List("rgba(161,161,161,0.5) 0px 0px 0px 3px, rgba(0,0,0,0.05) 0px 1px 2px 0px")), string.Join(", ", found.focused.Item2));
        Assert.Equal(0, found.focused.Item3);
        Assert.Equal(CssColor.Parse("#E5E5E5"), found.left);
    }

    [Fact]
    [Trait("AC", "INPUTOTP-02")]
    public async Task TheCodeBoxesShowTheTypedAndBoundCodeAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new InputGroupView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var otp = view.FindControl<InputOtp>("Code")!;
            List<string> Chars() => [.. otp.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("otp-box")).Select(border => border.GetVisualDescendants().OfType<TextBlock>().First().Text ?? "")];
            var boxes = otp.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("otp-box")).ToList();
            var separators = otp.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("otp-separator") && border.IsVisible).ToList();
            var shape = (boxes.Count, boxes.All(box => box.Bounds.Width == 36 && box.Bounds.Height == 36), boxes[0].CornerRadius, boxes[2].CornerRadius, boxes[3].CornerRadius, separators.Count, separators[0].Bounds.Width);
            Windows.Part<TextBox>(otp, "PART_TextBox").Focus(NavigationMethod.Tab);
            Windows.Type(window, "1");
            Windows.Type(window, "2");
            Windows.Type(window, "3");
            var typed = (string.Concat(Chars()), model.Code, otp.GetVisualDescendants().OfType<Border>().Single(border => border.Classes.Contains("active")) == otp.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("otp-box")).ElementAt(3));
            Windows.Key(window, Key.Back);
            var erased = model.Code;
            model.Code = "987654";
            Windows.Settle();
            var bound = string.Concat(Chars());
            var locked = view.FindControl<InputOtp>("Locked")!.Opacity;
            window.Close();
            return (shape, typed, erased, bound, locked);
        });

        Assert.Equal((6, true, new CornerRadius(8, 0, 0, 8), new CornerRadius(0, 8, 8, 0), new CornerRadius(8, 0, 0, 8), 1, 24.0), found.shape);
        Assert.Equal(("123", (string?)"123", true), found.typed);
        Assert.Equal("12", found.erased);
        Assert.Equal("987654", found.bound);
        Assert.Equal(0.5, found.locked);
    }
}
