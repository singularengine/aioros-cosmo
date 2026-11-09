using Aioros.Markup;
using Avalonia;

namespace Aioros.Cosmo.Forms;

/// <summary>The texts of Cosmo's file input, which the browser's own file input takes from its language: set them to translate the picker.</summary>
public static class FileInput
{
    /// <summary>The button's text.</summary>
    public static readonly AttachedProperty<string> ChooseTextProperty =
        AvaloniaProperty.RegisterAttached<FilePicker, string>("ChooseText", typeof(FileInput), "Choose File");

    /// <summary>The text shown while no file is chosen.</summary>
    public static readonly AttachedProperty<string> EmptyTextProperty =
        AvaloniaProperty.RegisterAttached<FilePicker, string>("EmptyText", typeof(FileInput), "No file chosen");

    /// <summary>Gets <see cref="ChooseTextProperty"/>.</summary>
    /// <param name="picker">The picker.</param>
    /// <returns>The button's text.</returns>
    public static string GetChooseText(FilePicker picker) => picker.GetValue(ChooseTextProperty);

    /// <summary>Sets <see cref="ChooseTextProperty"/>.</summary>
    /// <param name="picker">The picker.</param>
    /// <param name="value">The button's text.</param>
    public static void SetChooseText(FilePicker picker, string value) => picker.SetValue(ChooseTextProperty, value);

    /// <summary>Gets <see cref="EmptyTextProperty"/>.</summary>
    /// <param name="picker">The picker.</param>
    /// <returns>The text shown while no file is chosen.</returns>
    public static string GetEmptyText(FilePicker picker) => picker.GetValue(EmptyTextProperty);

    /// <summary>Sets <see cref="EmptyTextProperty"/>.</summary>
    /// <param name="picker">The picker.</param>
    /// <param name="value">The text shown while no file is chosen.</param>
    public static void SetEmptyText(FilePicker picker, string value) => picker.SetValue(EmptyTextProperty, value);
}
