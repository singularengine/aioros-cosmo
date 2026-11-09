using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace Aioros.Cosmo.Forms;

/// <summary>
/// shadcn/ui's Field: a label, its control, a description and an error stacked 12 px apart, or in a row with
/// <c>Orientation="Horizontal"</c> where a <see cref="FieldContent"/> (or else a <see cref="FieldLabel"/>) takes the room
/// the others leave, as <c>flex-1</c> and <c>flex-auto</c> do. The <c>responsive</c> class turns it into a row when its
/// <see cref="FieldGroup"/> is at least 448 px wide (shadcn/ui's <c>orientation="responsive"</c>, the group's <c>@md</c>).
/// The <c>invalid</c> class (bindable: <c>Classes.invalid="{Binding …}"</c>) colors its texts with the destructive token,
/// as shadcn's <c>data-invalid</c>. Its parts are laid out in a grid of its shown children, so both targets lay it out alike:
/// stacked, the row that spaces a part from the one before it is bound to the part's visibility, so a hidden part takes its
/// gap with it in Avalonia and in the web's plan alike (as shadcn's flex gap); in a row, the field sizes its columns itself.
/// </summary>
public sealed class Field : Grid
{
    /// <summary>Whether the parts stack (the default) or stand in a row.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Field, Orientation>(nameof(Orientation), Orientation.Vertical);

    /// <summary>The gap between the parts.</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<Field, double>(nameof(Spacing));

    /// <summary>Creates a field that places its parts as they are added.</summary>
    public Field()
    {
        Children.CollectionChanged += Changed;
    }

    /// <summary>Whether the parts stack (the default) or stand in a row.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>The gap between the parts.</summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
        {
            Build();
        }
        else if (change.Property == SpacingProperty)
        {
            Size();
        }
    }

    private void Changed(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var child in e.OldItems?.OfType<Control>() ?? [])
        {
            child.PropertyChanged -= Shown;
        }

        foreach (var child in e.NewItems?.OfType<Control>() ?? [])
        {
            child.PropertyChanged += Shown;
        }

        Build();
    }

    private void Shown(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsVisibleProperty && Row)
        {
            Size();
        }
    }

    private bool Row => Orientation == Orientation.Horizontal;

    private void Build()
    {
        if (!Row)
        {
            Stack();
            return;
        }

        var tracks = Math.Max(Children.Count * 2, 1);
        RowSpacing = 0;
        ColumnSpacing = 0;
        RowDefinitions = [new RowDefinition(GridLength.Auto)];
        ColumnDefinitions = [.. Enumerable.Range(0, tracks).Select(static _ => new ColumnDefinition(GridLength.Auto))];
        for (var index = 0; index < Children.Count; index++)
        {
            SetRow(Children[index], 0);
            SetColumn(Children[index], index * 2);
        }

        Size();
    }

    /// <summary>
    /// The stacked parts, each in its own row after a spacing row bound to its visibility (none before the first), and a
    /// last row taking the room left.
    /// </summary>
    private void Stack()
    {
        RowSpacing = 0;
        ColumnSpacing = 0;
        PseudoClasses.Set(":content", false);
        ColumnDefinitions = [new ColumnDefinition(GridLength.Star)];
        var rows = new RowDefinitions();
        for (var index = 0; index < Children.Count; index++)
        {
            if (index > 0)
            {
                var spacing = new RowDefinition(GridLength.Auto);
                spacing.Bind(RowDefinition.HeightProperty, CompiledBinding.Create<Control, bool>(part => part.IsVisible, source: Children[index], converter: FieldSpacing.Instance, converterParameter: Spacing.ToString(CultureInfo.InvariantCulture)));
                rows.Add(spacing);
            }

            rows.Add(new RowDefinition(GridLength.Auto));
            SetRow(Children[index], rows.Count - 1);
            SetColumn(Children[index], 0);
        }

        rows.Add(new RowDefinition(GridLength.Star));
        RowDefinitions = rows;
    }

    private void Size()
    {
        if (!Row)
        {
            Stack();
            return;
        }

        var shown = Children.Where(static child => child.IsVisible).ToList();
        var flexible = shown.Find(static child => child is FieldContent) ?? shown.Find(static child => child is FieldLabel);
        PseudoClasses.Set(":content", shown.Exists(static child => child is FieldContent));
        var sizes = Children.SelectMany(child => (GridLength[])[ReferenceEquals(child, flexible) ? GridLength.Star : GridLength.Auto, child.IsVisible && shown.IndexOf(child) < shown.Count - 1 ? new GridLength(Spacing) : new GridLength(0)]).ToList();
        if (sizes.Count == 0)
        {
            sizes.Add(GridLength.Star);
        }

        sizes[^1] = flexible is null ? GridLength.Star : new GridLength(0);
        for (var index = 0; index < sizes.Count && index < ColumnDefinitions.Count; index++)
        {
            ColumnDefinitions[index].Width = sizes[index];
        }
    }
}

/// <summary>shadcn/ui's FieldContent: a field's label or title and its description, 6 px apart, beside its control in a row.</summary>
public sealed class FieldContent : StackPanel
{
}

/// <summary>shadcn/ui's FieldTitle: a field's title where its label would be, 14 px and medium on a snug line.</summary>
public sealed class FieldTitle : TextBlock
{
}

/// <summary>shadcn/ui's FieldGroup: fields stacked 28 px apart (16 px for a group inside a group), a container its responsive fields follow.</summary>
public sealed class FieldGroup : StackPanel
{
}

/// <summary>shadcn/ui's FieldSet: a <see cref="FieldLegend"/>, its description and its fields stacked 24 px apart.</summary>
public sealed class FieldSet : StackPanel
{
}

/// <summary>shadcn/ui's FieldLabel: a <see cref="Label"/> with the field's snug line height.</summary>
public sealed class FieldLabel : Label
{
}

/// <summary>shadcn/ui's FieldLegend: a field set's title (16 px, medium); the <c>label</c> class makes it 14 px.</summary>
public sealed class FieldLegend : TextBlock
{
}

/// <summary>shadcn/ui's FieldDescription: helper text in the muted foreground.</summary>
public sealed class FieldDescription : TextBlock
{
}

/// <summary>
/// shadcn/ui's FieldSeparator: a 20 px row across a field group with a line at its middle and its content, if any, centered
/// on the background over the line; the <c>card</c> class puts the content on the card's color.
/// </summary>
public sealed class FieldSeparator : ContentControl
{
}

/// <summary>shadcn/ui's FieldError: an error message in the destructive color.</summary>
public sealed class FieldError : TextBlock
{
}

/// <summary>The height of the row that spaces a field's part from the one before it: the spacing (the parameter) while the part is shown, none while it is hidden.</summary>
public sealed class FieldSpacing : IValueConverter
{
    /// <summary>The converter a field's spacing rows bind through.</summary>
    public static readonly FieldSpacing Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? new GridLength(double.Parse(parameter as string ?? "0", CultureInfo.InvariantCulture)) : new GridLength(0);

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

