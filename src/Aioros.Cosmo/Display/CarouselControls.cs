using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Aioros.Cosmo.Display;

/// <summary>
/// What shadcn/ui's Carousel (Embla) adds to Avalonia's <see cref="Carousel"/>, registered once by <c>CosmoTheme</c>: the
/// template's previous and next buttons move the selection, Left and Right do the same while the focus is inside the
/// carousel, and the carousel takes <c>:first</c>, <c>:last</c> and <c>:loop</c> (its <c>WrapSelection</c>) so its
/// buttons are disabled where Embla's <c>canScrollPrev</c> and <c>canScrollNext</c> are false.
/// </summary>
internal static class CarouselControls
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        InputElement.KeyDownEvent.AddClassHandler<Carousel>(OnKeyDown, RoutingStrategies.Tunnel);
        Button.ClickEvent.AddClassHandler<Button>(OnClick, RoutingStrategies.Bubble);
        SelectingItemsControl.SelectedIndexProperty.Changed.AddClassHandler<Carousel>((carousel, _) => Mark(carousel));
        SelectingItemsControl.WrapSelectionProperty.Changed.AddClassHandler<Carousel>((carousel, _) => Mark(carousel));
        ItemsControl.ItemCountProperty.Changed.AddClassHandler<Carousel>((carousel, _) => Mark(carousel));
        Visual.BoundsProperty.Changed.AddClassHandler<Carousel>((carousel, _) => Align(carousel));
        Carousel.ViewportFractionProperty.Changed.AddClassHandler<Carousel>((carousel, _) => Align(carousel));
    }

    private static void Align(Carousel carousel)
    {
        if (!carousel.Classes.Contains("align-start") || Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(carousel).OfType<ScrollViewer>().FirstOrDefault(part => part.Name == "PART_ScrollViewer") is not { } viewer)
        {
            return;
        }

        var vertical = carousel.Classes.Contains("vertical");
        var extent = vertical ? carousel.Bounds.Height : carousel.Bounds.Width;
        var shift = -(1 - carousel.ViewportFraction) / 2 * extent;
        viewer.RenderTransform = new Avalonia.Media.TranslateTransform(vertical ? 0 : shift, vertical ? shift : 0);
    }

    private static void Mark(Carousel carousel)
    {
        var index = carousel.SelectedIndex;
        carousel.Classes.Set("first", index <= 0);
        carousel.Classes.Set("last", index >= carousel.ItemCount - 1);
        carousel.Classes.Set("loop", carousel.WrapSelection);
    }

    private static void OnKeyDown(Carousel carousel, KeyEventArgs e)
    {
        if (e.Key == Key.Left)
        {
            Move(carousel, -1);
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            Move(carousel, 1);
            e.Handled = true;
        }
    }

    private static void OnClick(Button button, RoutedEventArgs e)
    {
        if (button.TemplatedParent is not Carousel carousel)
        {
            return;
        }

        if (button.Name == "PART_Previous")
        {
            Move(carousel, -1);
        }
        else if (button.Name == "PART_Next")
        {
            Move(carousel, 1);
        }
    }

    private static void Move(Carousel carousel, int step)
    {
        var count = carousel.ItemCount;
        if (count == 0)
        {
            return;
        }

        var next = carousel.SelectedIndex + step;
        if (carousel.WrapSelection)
        {
            next = (next + count) % count;
        }

        if (next >= 0 && next < count)
        {
            carousel.SetCurrentValue(SelectingItemsControl.SelectedIndexProperty, next);
        }
    }
}

/// <summary>
/// The slide indicators of a carousel (Cosmo's addition to shadcn/ui's Carousel, whose API example shows "Slide 1 of 5"):
/// one 8 px dot per slide, the selected one in <c>--primary</c>, each a button that selects its slide.
/// <see cref="SelectedIndex"/> binds two way to the carousel's.
/// </summary>
public sealed class CarouselIndicators : StackPanel
{
    /// <summary>Defines <see cref="Count"/>.</summary>
    public static readonly StyledProperty<int> CountProperty = AvaloniaProperty.Register<CarouselIndicators, int>(nameof(Count));

    /// <summary>Defines <see cref="SelectedIndex"/>.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<CarouselIndicators, int>(nameof(SelectedIndex), defaultBindingMode: BindingMode.TwoWay);

    static CarouselIndicators()
    {
        CountProperty.Changed.AddClassHandler<CarouselIndicators>((indicators, _) => indicators.Rebuild());
        SelectedIndexProperty.Changed.AddClassHandler<CarouselIndicators>((indicators, _) => indicators.Mark());
    }

    /// <summary>Lays the dots in a row, 8 px apart.</summary>
    public CarouselIndicators()
    {
        Orientation = Avalonia.Layout.Orientation.Horizontal;
        Spacing = 8;
    }

    /// <summary>How many slides there are.</summary>
    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>The selected slide, two way.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    private void Rebuild()
    {
        Children.Clear();
        for (var index = 0; index < Count; index++)
        {
            var slide = index;
            var dot = new Button { Classes = { "carousel-indicator" } };
            AutomationProperties.SetName(dot, $"Slide {slide + 1}");
            dot.Click += (_, _) => SetCurrentValue(SelectedIndexProperty, slide);
            Children.Add(dot);
        }

        Mark();
    }

    private void Mark()
    {
        for (var index = 0; index < Children.Count; index++)
        {
            Children[index].Classes.Set("selected", index == SelectedIndex);
        }
    }
}
