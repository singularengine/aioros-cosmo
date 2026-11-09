using System.ComponentModel;
using System.Runtime.CompilerServices;
using Aioros.Cosmo.Forms;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Forms;

public sealed class RangeSliderTests(DesktopSession desktop)
{
    private sealed class Budget : INotifyPropertyChanged
    {
        private double _lower = 200;
        private double _upper = 800;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double Lower
        {
            get => _lower;
            set => Set(ref _lower, value);
        }

        public double Upper
        {
            get => _upper;
            set => Set(ref _upper, value);
        }

        private void Set(ref double field, double value, [CallerMemberName] string? name = null)
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    private static Thumb Thumb(RangeSlider range, string part) => Windows.Part<Thumb>(Windows.Part<Slider>(range, part), "thumb");

    private static IEnumerable<string> Thumbs(string what, RangeSlider range, Control root, ElementMetrics lower, ElementMetrics upper, Point origin)
    {
        foreach (var (part, expected) in new[] { ("PART_LowerSlider", lower), ("PART_UpperSlider", upper) })
        {
            var box = Windows.Box(Thumb(range, part), root);
            var target = new Rect(expected.X - origin.X, expected.Y - origin.Y, expected.Width, expected.Height);
            if (Math.Abs(box.X - target.X) > 0.5 || Math.Abs(box.Y - target.Y) > 0.5 || Math.Abs(box.Width - target.Width) > 0.5 || Math.Abs(box.Height - target.Height) > 0.5)
            {
                yield return $"{what} {part}: {box}, shadcn {target}";
            }
        }
    }

    private static IEnumerable<string> Track(string what, Window window, RangeSlider range, CssColor muted, CssColor primary)
    {
        using var frame = Windows.Capture(window);
        var horizontal = range.Orientation == Avalonia.Layout.Orientation.Horizontal;
        var low = Windows.Box(Thumb(range, "PART_LowerSlider"), window);
        var high = Windows.Box(Thumb(range, "PART_UpperSlider"), window);
        var across = Windows.Box(range, window);
        var points = horizontal
            ? new[] { (low.X - 6, across.Center.Y, muted), ((low.Right + high.X) / 2, across.Center.Y, primary), (high.Right + 6, across.Center.Y, muted) }
            : [(across.Center.X, low.Bottom + 6, muted), (across.Center.X, (high.Bottom + low.Y) / 2, primary), (across.Center.X, high.Y - 6, muted)];
        foreach (var (x, y, expected) in points)
        {
            var pixel = frame.GetPixel((int)Math.Round(x), (int)Math.Round(y));
            var found = new CssColor(pixel.Red, pixel.Green, pixel.Blue, 1);
            if (!found.Near(expected, 3))
            {
                yield return $"{what} track at {x:0},{y:0}: {found}, shadcn {expected}";
            }
        }
    }

    [Fact]
    [Trait("AC", "SLIDER-05")]
    public async Task ARangeHasTwoThumbsAndItsTrackBetweenThemAsync()
    {
        var wrong = await FixtureAsync(desktop, "slider-range", (window, root, reference, theme) =>
        {
            var thumbs = reference.Where(element => element.Slot == "slider-thumb").ToList();
            var muted = CssColor.Parse(reference.First(element => element.Slot == "slider-track").Background);
            var primary = CssColor.Parse(reference.First(element => element.Slot == "slider-range").Background);
            var roots = reference.Where(element => element.Slot == "slider").ToList();
            var ranges = All<RangeSlider>(root);
            return Thumbs($"{theme} horizontal", ranges[0], ranges[0], thumbs[0], thumbs[1], new Point(roots[0].X, roots[0].Y))
                .Concat(Thumbs($"{theme} vertical", ranges[1], ranges[1], thumbs[2], thumbs[3], new Point(roots[1].X, roots[1].Y)))
                .Concat(Track($"{theme} horizontal", window, ranges[0], muted, primary))
                .Concat(Track($"{theme} vertical", window, ranges[1], muted, primary))
                .ToList();
        });
        foreach (var (variant, name) in Themes)
        {
            var roots = await ReferenceMetrics.SlotsAsync("slider-states", name, "slider");
            var thumbs = await ReferenceMetrics.SlotsAsync("slider-states", name, "slider-thumb");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var range = new RangeSlider { Width = 360, Maximum = 100, TickFrequency = 1, IsSnapToTickEnabled = true, LowerValue = 25, UpperValue = 75 };
                var window = Windows.Show(new StackPanel { Margin = new Thickness(16), Children = { range } }, variant);
                var errors = Thumbs($"{name} slider-states", range, range, thumbs[1], thumbs[2], new Point(roots[1].X, roots[1].Y)).ToList();
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "SLIDER-06")]
    public async Task ThePointerAndTheKeysMoveTheValuesAsRadixDoesAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new Budget();
            var range = new RangeSlider { Width = 400, Maximum = 1000, TickFrequency = 10, IsSnapToTickEnabled = true, DataContext = model };
            range.Bind(RangeSlider.LowerValueProperty, new Avalonia.Data.Binding(nameof(Budget.Lower)));
            range.Bind(RangeSlider.UpperValueProperty, new Avalonia.Data.Binding(nameof(Budget.Upper)));
            var window = Windows.Show(new StackPanel { Margin = new Thickness(16), Children = { range } }, ThemeVariant.Light);
            var steps = new List<string>();
            Point At(double value) => range.TranslatePoint(new Point(value / 1000 * range.Bounds.Width, 3), window) ?? default;
            string State() => $"{model.Lower}-{model.Upper}{(Windows.Part<Slider>(range, "PART_UpperSlider").IsKeyboardFocusWithin ? " upper" : Windows.Part<Slider>(range, "PART_LowerSlider").IsKeyboardFocusWithin ? " lower" : string.Empty)}";
            void Press(double value) => window.MouseDown(At(value), MouseButton.Left, RawInputModifiers.None);
            void Move(double value) => window.MouseMove(At(value), RawInputModifiers.LeftMouseButton);
            void Release(double value)
            {
                window.MouseUp(At(value), MouseButton.Left, RawInputModifiers.None);
                Windows.Settle();
            }

            void Key(Key key, RawInputModifiers modifiers = RawInputModifiers.None)
            {
                window.KeyPress(key, modifiers, PhysicalKey.None, null);
                window.KeyRelease(key, modifiers, PhysicalKey.None, null);
                Windows.Settle();
            }

            Press(703);
            Windows.Settle();
            steps.Add(State());
            Move(750);
            Release(750);
            steps.Add(State());
            Press(100);
            Release(100);
            steps.Add(State());
            Press(100);
            Move(900);
            Release(900);
            steps.Add(State());
            Windows.Part<Slider>(range, "PART_LowerSlider").Focus(NavigationMethod.Tab);
            Key(Avalonia.Input.Key.Right);
            steps.Add(State());
            Key(Avalonia.Input.Key.Right, RawInputModifiers.Shift);
            steps.Add(State());
            Key(Avalonia.Input.Key.PageDown);
            steps.Add(State());
            Key(Avalonia.Input.Key.PageUp);
            Key(Avalonia.Input.Key.PageUp);
            steps.Add(State());
            Key(Avalonia.Input.Key.Home);
            steps.Add(State());
            Key(Avalonia.Input.Key.End);
            steps.Add(State());
            range.IsEnabled = false;
            Windows.Settle();
            Press(500);
            Release(500);
            steps.Add(State());
            window.Close();
            return steps;
        });

        Assert.Equal(["200-700 upper", "200-750 upper", "100-750 lower", "750-900 upper", "760-900 lower", "860-900 lower", "760-900 lower", "900-960 upper", "0-960 lower", "0-1000 upper", "0-1000"], found);
    }
}
