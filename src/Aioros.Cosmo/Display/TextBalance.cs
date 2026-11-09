using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Aioros.Cosmo.Display;

/// <summary>
/// CSS's <c>text-wrap: balance</c> on the desktop for a stretched <see cref="TextBlock"/> marked with
/// <see cref="Web.TextWrapProperty"/> <c>balance</c> (shadcn/ui's <c>text-balance</c>): its lines wrap at the narrowest width
/// that keeps their count, the box keeping its width and its alignment through its padding. Cosmo's theme registers it.
/// </summary>
internal static class TextBalance
{
    private static readonly AttachedProperty<Thickness?> BaseProperty = AvaloniaProperty.RegisterAttached<TextBlock, Thickness?>("Base", typeof(TextBalance));

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        Web.TextWrapProperty.Changed.AddClassHandler<TextBlock>((text, change) =>
        {
            if (change.GetNewValue<string?>() == "balance")
            {
                text.SizeChanged += Resized;
                text.PropertyChanged += Changed;
                Balance(text);
            }
            else
            {
                text.SizeChanged -= Resized;
                text.PropertyChanged -= Changed;
                Restore(text);
            }
        });
    }

    private static void Resized(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
        {
            Balance((TextBlock)sender!);
        }
    }

    private static void Changed(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextBlock.TextProperty || e.Property == TextBlock.FontSizeProperty || e.Property == TextBlock.FontWeightProperty
            || e.Property == TextBlock.FontFamilyProperty || e.Property == TextBlock.LineHeightProperty)
        {
            Balance((TextBlock)sender!);
        }
    }

    private static void Restore(TextBlock text)
    {
        if (text.GetValue(BaseProperty) is { } padding)
        {
            text.Padding = padding;
            text.ClearValue(BaseProperty);
        }
    }

    private static void Balance(TextBlock text)
    {
        if (Web.GetIsWritten(text) || text.HorizontalAlignment != HorizontalAlignment.Stretch || string.IsNullOrEmpty(text.Text) || text.Bounds.Width <= 0)
        {
            return;
        }

        var padding = text.GetValue(BaseProperty) ?? text.Padding;
        text.SetValue(BaseProperty, padding);
        var width = text.Bounds.Width - padding.Left - padding.Right;
        var count = Lines(text, width).Count;
        var fit = width;
        if (count > 1)
        {
            var low = width / count;
            while (fit - low > 0.5)
            {
                var middle = (low + fit) / 2;
                if (Lines(text, middle).Count == count)
                {
                    fit = middle;
                }
                else
                {
                    low = middle;
                }
            }
        }

        var extra = width - fit;
        text.Padding = text.TextAlignment switch
        {
            TextAlignment.Center => new Thickness(padding.Left + extra / 2, padding.Top, padding.Right + extra / 2, padding.Bottom),
            TextAlignment.Right or TextAlignment.End => new Thickness(padding.Left + extra, padding.Top, padding.Right, padding.Bottom),
            _ => new Thickness(padding.Left, padding.Top, padding.Right + extra, padding.Bottom),
        };
    }

    private static IReadOnlyList<TextLine> Lines(TextBlock text, double width) =>
        new TextLayout(text.Text, new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, null,
            text.TextAlignment, TextWrapping.Wrap, maxWidth: width, lineHeight: text.LineHeight, letterSpacing: text.LetterSpacing).TextLines;
}
