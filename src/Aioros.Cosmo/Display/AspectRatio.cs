using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;

namespace Aioros.Cosmo.Display;

/// <summary>shadcn/ui's AspectRatio (Radix AspectRatio): its child as wide as the room it is given and <see cref="Ratio"/> times less high.</summary>
public sealed class AspectRatio : Decorator
{
    /// <summary>Defines the <see cref="Ratio"/> property.</summary>
    public static readonly StyledProperty<double> RatioProperty = AvaloniaProperty.Register<AspectRatio, double>(nameof(Ratio), 1, validate: ratio => ratio > 0 && double.IsFinite(ratio));

    static AspectRatio()
    {
        AffectsMeasure<AspectRatio>(RatioProperty);
        RatioProperty.Changed.AddClassHandler<AspectRatio>((ratio, change) => Web.SetAspectRatio(ratio, change.GetNewValue<double>()));
    }

    /// <summary>Keeps the ratio on the web as CSS <c>aspect-ratio</c> (<c>a:Web.AspectRatio</c>), so its height follows its width in any viewport.</summary>
    public AspectRatio() => Web.SetAspectRatio(this, Ratio);

    /// <summary>The width divided by the height (16:9 is 1.7778); 1 by default.</summary>
    public double Ratio
    {
        get => GetValue(RatioProperty);
        set => SetValue(RatioProperty, value);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        if (double.IsInfinity(availableSize.Width))
        {
            Child?.Measure(availableSize);
        }

        var width = double.IsInfinity(availableSize.Width) ? (Child?.DesiredSize.Width ?? 0) : availableSize.Width;
        var size = new Size(width, width / Ratio);
        Child?.Measure(size);
        return size;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = new Size(finalSize.Width, finalSize.Width / Ratio);
        Child?.Arrange(new Rect(size));
        return size;
    }
}
