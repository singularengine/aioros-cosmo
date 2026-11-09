using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Metadata;

namespace Aioros.Cosmo.Dates;

/// <summary>
/// The presets of shadcn/ui's <c>date-picker-with-presets</c> on a <see cref="CalendarDatePicker"/>: a select above the
/// calendar in its popover whose choice sets the picked date that many days from today, as the example's
/// <c>onValueChange</c> sets <c>addDays(today, value)</c>. Setting <see cref="ItemsProperty"/> gives the picker the
/// <c>presets</c> class, which Cosmo's theme lays its popover out with (8 px padding, the select, the calendar framed).
/// </summary>
public static class DatePresets
{
    /// <summary>Defines the presets a picker offers.</summary>
    public static readonly AttachedProperty<DatePresetList?> ItemsProperty =
        AvaloniaProperty.RegisterAttached<CalendarDatePicker, DatePresetList?>("Items", typeof(DatePresets));

    /// <summary>Defines the chosen preset's index, none with -1.</summary>
    public static readonly AttachedProperty<int> IndexProperty =
        AvaloniaProperty.RegisterAttached<CalendarDatePicker, int>("Index", typeof(DatePresets), -1, defaultBindingMode: BindingMode.TwoWay);

    static DatePresets()
    {
        ItemsProperty.Changed.AddClassHandler<CalendarDatePicker>((picker, change) => picker.Classes.Set("presets", change.NewValue is DatePresetList { Count: > 0 }));
        IndexProperty.Changed.AddClassHandler<CalendarDatePicker>((picker, _) =>
        {
            if (GetItems(picker) is { } items && GetIndex(picker) is var index && index >= 0 && index < items.Count)
            {
                picker.SetCurrentValue(CalendarDatePicker.SelectedDateProperty, DateTime.Today.AddDays(items[index].Days));
            }
        });
    }

    /// <summary>Gets <see cref="ItemsProperty"/>.</summary>
    public static DatePresetList? GetItems(CalendarDatePicker picker) => picker.GetValue(ItemsProperty);

    /// <summary>Sets <see cref="ItemsProperty"/>.</summary>
    public static void SetItems(CalendarDatePicker picker, DatePresetList? value) => picker.SetValue(ItemsProperty, value);

    /// <summary>Gets <see cref="IndexProperty"/>.</summary>
    public static int GetIndex(CalendarDatePicker picker) => picker.GetValue(IndexProperty);

    /// <summary>Sets <see cref="IndexProperty"/>.</summary>
    public static void SetIndex(CalendarDatePicker picker, int value) => picker.SetValue(IndexProperty, value);
}

/// <summary>A preset of <see cref="DatePresets"/>: its text and how many days from today it picks.</summary>
public sealed class DatePreset
{
    /// <summary>The text the select shows.</summary>
    [Content]
    public string Content { get; set; } = string.Empty;

    /// <summary>How many days from today the preset picks.</summary>
    public int Days { get; set; }

    /// <inheritdoc />
    public override string ToString() => Content;
}

/// <summary>Presets written in XAML for <see cref="DatePresets.ItemsProperty"/>.</summary>
public sealed class DatePresetList : List<DatePreset>;
