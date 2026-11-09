using Aioros.Cosmo.Display;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Composites;

public sealed class CarouselTests(DesktopSession desktop)
{
    [Fact]
    [Trait("AC", "CAROUSEL-02")]
    public async Task SlidesAndButtonsAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["carousel-demo", "carousel-api", "carousel-orientation"])
        {
            wrong.AddRange(await Looks.FixtureAsync(desktop, fixture, (_, root, reference, theme) => Check(root, reference, $"{theme} {fixture}", fixture == "carousel-orientation" ? 90 : 0)));
        }

        Assert.Empty(wrong);
    }

    private static List<string> Check(Control root, IReadOnlyList<ElementMetrics> reference, string what, double turned)
    {
        var carousel = All<Carousel>(root).Single();
        var errors = Near($"{what} viewport", Windows.Box(Windows.Part<Border>(carousel, "PART_Viewport"), root), reference.Single(element => element.Slot == "carousel-content"), 1).ToList();
        errors.AddRange(Near($"{what} card", Windows.Box(All<Card>(root)[0], root), reference.First(element => element.Slot == "card"), 1));
        errors.AddRange(Near($"{what} content", Windows.Box(All<CardContent>(root)[0], root), reference.First(element => element.Slot == "card-content"), 1));
        errors.AddRange(Control(root, carousel, reference.Single(element => element.Slot == "carousel-previous"), "PART_Previous", what, turned));
        errors.AddRange(Control(root, carousel, reference.Single(element => element.Slot == "carousel-next"), "PART_Next", what, turned));
        var previous = Windows.Part<Button>(carousel, "PART_Previous");
        errors.AddRange(!previous.IsEffectivelyEnabled ? [] : [$"{what}: previous enabled on the first slide"]);
        errors.AddRange(Windows.Part<Avalonia.Controls.Shapes.Path>(previous).Data is not null ? [] : [$"{what}: no arrow"]);
        return errors;
    }

    private static List<string> Control(Control root, Carousel carousel, ElementMetrics expected, string name, string what, double turned)
    {
        var button = Windows.Part<Button>(carousel, name);
        var rotation = button.RenderTransform is Avalonia.Media.Transformation.TransformOperations operations ? Math.Round(Math.Atan2(operations.Value.M12, operations.Value.M11) * 180 / Math.PI) : 0;
        var errors = Near($"{what} {name}", Laid(button, root), expected, 1).ToList();
        errors.AddRange(button.CornerRadius.TopLeft >= 16 && rotation == turned ? [] : [$"{what} {name}: radius {button.CornerRadius} turned {rotation}"]);
        errors.AddRange(Paints($"{what} {name} border", button.BorderBrush, expected.BorderColor));
        if (name == "PART_Next")
        {
            errors.AddRange(Paints($"{what} {name} fill", button.Background, expected.Background));
        }

        return errors;
    }

    [Fact]
    [Trait("AC", "CAROUSEL-03")]
    public async Task ButtonsKeysLoopAndIndicatorsMoveTheSlidesAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var carousel = new Carousel { Width = 300, Height = 120, ItemsSource = Enumerable.Range(1, 5).Select(n => $"Slide {n}").ToList(), Classes = { "indicators" } };
            var window = Windows.Show(new Border { Padding = new Thickness(60), Child = carousel, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top }, ThemeVariant.Light);
            var next = Windows.Part<Button>(carousel, "PART_Next");
            var previous = Windows.Part<Button>(carousel, "PART_Previous");
            Windows.Click(window, next);
            Windows.Click(window, next);
            var steps = new List<int> { carousel.SelectedIndex };
            previous.Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Left);
            steps.Add(carousel.SelectedIndex);
            Windows.Key(window, Key.Right);
            steps.Add(carousel.SelectedIndex);
            carousel.SelectedIndex = 4;
            Windows.Settle();
            var disabledAtEnd = !next.IsEffectivelyEnabled;
            Windows.Key(window, Key.Right);
            steps.Add(carousel.SelectedIndex);
            carousel.WrapSelection = true;
            Windows.Settle();
            var enabledWithLoop = next.IsEffectivelyEnabled;
            Windows.Key(window, Key.Right);
            steps.Add(carousel.SelectedIndex);
            var dots = Windows.Part<CarouselIndicators>(carousel).Children.OfType<Button>().ToList();
            Windows.Click(window, dots[3]);
            steps.Add(carousel.SelectedIndex);
            var marked = string.Join(',', dots.Select((dot, i) => (dot, i)).Where(pair => pair.dot.Classes.Contains("selected")).Select(pair => pair.i));
            var primary = Windows.Paint(Windows.Part<Border>(dots[3], "PART_Dot").Background) == Windows.Paint((IBrush)window.FindResource(ThemeVariant.Light, "CosmoPrimaryBrush")!);
            var slide = carousel.PageTransition as PageSlide;
            var horizontal = (slide?.Duration, slide?.Orientation);
            carousel.Classes.Add("vertical");
            Windows.Settle();
            var vertical = (carousel.PageTransition as PageSlide)?.Orientation;
            window.Close();
            return (string.Join(',', steps), disabledAtEnd, enabledWithLoop, marked, primary, horizontal, vertical);
        });

        Assert.Equal(("2,1,2,4,0,3", true, true, "3", true, ((TimeSpan?)TimeSpan.FromMilliseconds(300), (PageSlide.SlideAxis?)PageSlide.SlideAxis.Horizontal), (PageSlide.SlideAxis?)PageSlide.SlideAxis.Vertical), found);
    }

    [Fact]
    [Trait("AC", "CAROUSEL-04")]
    public async Task TheBoundIndexAndSlidesFollowTheModelAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new CompositesModel();
            var view = new CompositesView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var carousel = view.FindControl<Carousel>("Slides")!;
            var label = view.FindControl<TextBlock>("SlideLabel")!;
            var next = Windows.Part<Button>(carousel, "PART_Next");
            Windows.Click(window, next);
            var first = (model.Index, label.Text);
            model.Index = 4;
            Windows.Settle();
            var shown = (carousel.SelectedIndex, next.IsEffectivelyEnabled);
            Windows.Click(window, view.FindControl<Button>("AddSlide")!);
            var added = (label.Text, next.IsEffectivelyEnabled);
            window.Close();
            return (first, shown, added);
        });

        Assert.Equal(((1, "Slide 2 of 5"), (4, false), ("Slide 5 of 6", true)), found);
    }
}
