using Aioros.Cosmo.Forms;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Forms;

public sealed class TextFieldTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    [Fact]
    [Trait("AC", "LABEL-02")]
    public async Task ALabelHasShadcnsTextAndFocusesItsTargetAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var box = new TextBox();
            var label = new Label { Content = "Email", Target = box };
            var invalid = new Label { Content = "Bad" };
            var off = new Label { Content = "Off", IsEnabled = false };
            var panel = new StackPanel { Spacing = 8, Children = { label, box, new Field { Classes = { "invalid" }, Children = { invalid } }, off } };
            var window = Windows.Show(panel, ThemeVariant.Light);
            var text = label.GetVisualDescendants().OfType<TextBlock>().First();
            var looks = (label.Padding, label.FontSize, label.FontWeight, text.LineHeight, Windows.Paint(label.Foreground), Windows.Paint(invalid.Foreground), off.Opacity);
            Windows.Click(window, label);
            var focused = box.IsFocused;
            window.Close();
            return (looks, focused);
        });

        Assert.Equal((default(Thickness), 14.0, FontWeight.Medium, 14.0, CssColor.Parse("#0A0A0A"), CssColor.Parse("#E7000B"), 0.5), found.looks);
        Assert.True(found.focused);
    }

    [Fact]
    [Trait("AC", "LABEL-03")]
    public async Task ABoundLabelShowsItsFormattedValueAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new FormsView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var label = view.FindControl<Label>("CountLabel")!;
            string Text() => string.Concat(label.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text));
            var before = Text();
            model.Count = 7;
            Windows.Settle();
            var after = Text();
            window.Close();
            return (before, after);
        });

        Assert.Equal(("Name (3 unread)", "Name (7 unread)"), found);
    }

    private sealed record FieldLook(CssColor Background, CssColor Border, IReadOnlyList<CssShadow> Shadow);

    private static FieldLook Read(TextBox box)
    {
        var part = Windows.Part<Border>(box, "PART_BorderElement");
        return new(Windows.Paint(part.Background), Windows.Paint(part.BorderBrush), Windows.Shadows(part.BoxShadow));
    }

    private static IEnumerable<string> Differences(string what, FieldLook found, ElementMetrics expected)
    {
        if (!found.Background.Near(CssColor.Parse(expected.Background)))
        {
            yield return $"{what}: background {found.Background}, shadcn {expected.Background}";
        }

        if (!found.Border.Near(CssColor.Parse(expected.BorderColor)))
        {
            yield return $"{what}: border {found.Border}, shadcn {expected.BorderColor}";
        }

        if (!CssShadow.Same(found.Shadow, CssShadow.List(expected.Shadow)))
        {
            yield return $"{what}: shadow {string.Join(", ", found.Shadow)}, shadcn {string.Join(", ", CssShadow.List(expected.Shadow))}";
        }
    }

    [Fact]
    [Trait("AC", "INPUT-04")]
    public async Task EveryStateOfATextBoxIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var rest = (await ReferenceMetrics.SlotsAsync("input-states", name, "input"))[0];
            var focus = (await ReferenceMetrics.SlotsAsync("input-states", name, "input", "focus"))[0];
            var invalid = (await ReferenceMetrics.SlotsAsync("field-form", name, "input"))[1];
            var invalidFocus = (await ReferenceMetrics.SlotsAsync("field-form", name, "input", "focus"))[1];
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var box = new TextBox { PlaceholderText = "Email" };
                var off = new TextBox { IsEnabled = false };
                var other = new Button { Content = "Elsewhere" };
                var window = Windows.Show(new StackPanel { Spacing = 8, Width = 384, Children = { box, off, other } }, variant);
                var errors = new List<string>();
                var part = Windows.Part<Border>(box, "PART_BorderElement");
                var geometry = (part.CornerRadius.TopLeft, box.Padding, box.Bounds.Height, box.FontSize, Windows.Paint(box.PlaceholderForeground), Windows.Paint(box.SelectionBrush), Windows.Paint(box.SelectionForegroundBrush), off.Opacity);
                errors.AddRange(Differences($"{name} rest", Read(box), rest));
                box.Focus(NavigationMethod.Tab);
                Windows.Settle();
                errors.AddRange(Differences($"{name} focus", Read(box), focus));
                DataValidationErrors.SetError(box, new InvalidOperationException("Enter a valid email address."));
                Windows.Settle();
                errors.AddRange(Differences($"{name} invalid focus", Read(box), invalidFocus));
                other.Focus(NavigationMethod.Pointer);
                Windows.Settle();
                errors.AddRange(Differences($"{name} invalid", Read(box), invalid));
                var expected = (8.0, new Thickness(12, 4), 36.0, 14.0, CssColor.Parse(name == "light" ? "#737373" : "#A1A1A1"), CssColor.Parse(name == "light" ? "#171717" : "#E5E5E5"), CssColor.Parse(name == "light" ? "#FAFAFA" : "#171717"), 0.5);
                if (geometry != expected)
                {
                    errors.Add($"{name}: {geometry}, shadcn {expected}");
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "INPUT-07")]
    public async Task AFilePickerIsShadcnsFileInputAsync()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}", "photo.png");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, [1, 2, 3], TestContext.Current.CancellationToken);
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var input = (await ReferenceMetrics.SlotsAsync("input-file", name, "input"))[0];
            wrong.AddRange(await await desktop.RunAsync(async () =>
            {
                var window = Display.DisplayFixtures.Show("input-file", variant);
                var root = (Control)window.Content!;
                var picker = root.GetVisualDescendants().OfType<Aioros.Markup.FilePicker>().Single();
                var part = Windows.Part<Border>(picker, "PART_Root");
                var texts = part.GetVisualDescendants().OfType<TextBlock>().ToList();
                var errors = Display.Looks.Near($"{name} picker", Windows.Box(picker, root), input, 1).ToList();
                errors.AddRange(Differences($"{name} picker", new(Windows.Paint(part.Background), Windows.Paint(part.BorderBrush), Windows.Shadows(part.BoxShadow)), input));
                var first = texts[0].TranslatePoint(default, picker) ?? default;
                var second = texts[1].TranslatePoint(default, picker) ?? default;
                if (texts.Count != 2 || texts[0].Text != "Choose File" || texts[0].FontWeight != FontWeight.Medium || texts[0].FontSize != 14 || texts[1].Text != "No file chosen"
                    || first.X != 13 || Math.Abs(second.X - first.X - texts[0].Bounds.Width - 4) > 0.5 || first.Y != second.Y || part.CornerRadius.TopLeft != 8)
                {
                    errors.Add($"{name}: texts {string.Join('|', texts.Select(text => $"{text.Text} {text.FontWeight} {text.FontSize}"))} at {first} and {second}, radius {part.CornerRadius}");
                }

                picker.File = await window.StorageProvider.TryGetFileFromPathAsync(new Uri(path));
                Windows.Settle();
                errors.AddRange(texts[1].Text == "photo.png" ? [] : [$"{name}: chosen file shown as {texts[1].Text}"]);
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "INPUT-09")]
    public async Task AFilePickersTextsCanBeTranslatedAsync()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}", "photo.png");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, [1, 2, 3], TestContext.Current.CancellationToken);

        var (translated, plain, chosen, cleared) = await await desktop.RunAsync(async () =>
        {
            var picker = new Aioros.Markup.FilePicker();
            FileInput.SetChooseText(picker, "Elegir archivo");
            FileInput.SetEmptyText(picker, "Ningún archivo");
            var other = new Aioros.Markup.FilePicker();
            var window = Windows.Show(new StackPanel { Children = { picker, other } }, ThemeVariant.Light);
            static string Texts(Control control) => string.Join('|', control.GetVisualDescendants().OfType<TextBlock>().Select(static text => text.Text));
            var (first, second) = (Texts(picker), Texts(other));
            picker.File = await window.StorageProvider.TryGetFileFromPathAsync(new Uri(path));
            Windows.Settle();
            var named = Texts(picker);
            picker.File = null;
            Windows.Settle();
            var empty = Texts(picker);
            window.Close();
            return (first, second, named, empty);
        });

        Assert.Equal("Elegir archivo|Ningún archivo", translated);
        Assert.Equal("Choose File|No file chosen", plain);
        Assert.Equal("Elegir archivo|photo.png", chosen);
        Assert.Equal("Elegir archivo|Ningún archivo", cleared);
    }

    [Fact]
    [Trait("AC", "INPUT-05")]
    public async Task TypingUpdatesTheTwoWayBoundTextAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new FormsView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var box = view.FindControl<TextBox>("NameBox")!;
            box.Focus(NavigationMethod.Tab);
            Windows.Type(window, "Ada");
            var typed = model.Name;
            model.Name = "Grace";
            Windows.Settle();
            var shown = box.Text;
            model.Locked = true;
            Windows.Settle();
            var enabled = box.IsEnabled;
            window.Close();
            return (typed, shown, enabled);
        });

        Assert.Equal(("Ada", "Grace", false), found);
    }

    [Fact]
    [Trait("AC", "TEXTAREA-02")]
    public async Task AMultiLineTextBoxStartsAt64PxAndGrowsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var area = new TextBox { AcceptsReturn = true, Width = 384 };
            var window = Windows.Show(new StackPanel { Children = { area } }, ThemeVariant.Light);
            var empty = (area.Bounds.Height, area.Padding, area.VerticalContentAlignment, area.TextWrapping);
            area.Focus(NavigationMethod.Tab);
            Windows.Type(window, "one");
            Windows.Key(window, Key.Enter);
            Windows.Type(window, "two");
            for (var line = 0; line < 3; line++)
            {
                Windows.Key(window, Key.Enter);
                Windows.Type(window, "more");
            }

            var grown = area.Bounds.Height;
            var text = area.Text;
            window.Close();
            return (empty, grown, text);
        });

        Assert.Equal((64.0, new Thickness(12, 8), Avalonia.Layout.VerticalAlignment.Top, TextWrapping.Wrap), found.empty);
        Assert.StartsWith("one" + Environment.NewLine + "two", found.text, StringComparison.Ordinal);
        Assert.True(found.grown > 64, $"{found.grown}");
    }

    [Fact]
    [Trait("AC", "FIELD-01")]
    public async Task TheFieldPartsHaveShadcnsBoxesAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("field-form", name);
            var origin = reference.Single(element => element.Slot == "field-group");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var view = new FieldFormView();
                var window = Windows.Show(view, variant);
                var group = view.FindControl<FieldGroup>("Group")!;
                var parts = new List<(string Slot, Control Control)> { ("field-group", group) };
                foreach (var field in group.GetVisualDescendants().OfType<Control>())
                {
                    parts.AddRange(Slot(field) is { } slot ? [(slot, field)] : []);
                }

                var errors = parts.Select(part => part.Slot).Distinct().SelectMany(slot => Boxes(name, slot, [.. parts.Where(part => part.Slot == slot).Select(part => part.Control)], [.. reference.Where(element => element.Slot == slot)], group, origin)).ToList();
                var destructive = CssColor.Parse(reference.Single(element => element.Slot == "field-error").Color);
                var invalid = group.Children.OfType<Field>().Single(field => field.Classes.Contains("invalid"));
                var colors = new[] { Windows.Paint(invalid.Children.OfType<FieldLabel>().Single().Foreground), Windows.Paint(invalid.Children.OfType<TextBox>().Single().Foreground), Windows.Paint(ErrorText(invalid).Foreground) };
                errors.AddRange(colors.Where(color => !color.Near(destructive)).Select(color => $"{name}: invalid text {color}, shadcn {destructive}"));
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Boxes(string name, string slot, List<Control> ours, List<ElementMetrics> theirs, Control group, ElementMetrics origin)
    {
        if (ours.Count != theirs.Count)
        {
            yield return $"{name} {slot}: {ours.Count} parts, shadcn {theirs.Count}";
            yield break;
        }

        for (var i = 0; i < ours.Count; i++)
        {
            var box = Windows.Box(ours[i], group);
            var expected = new Rect(theirs[i].X - origin.X, theirs[i].Y - origin.Y, theirs[i].Width, theirs[i].Height);
            if (Math.Abs(box.X - expected.X) > 2.5 || Math.Abs(box.Y - expected.Y) > 2.5 || Math.Abs(box.Width - expected.Width) > 2.5 || Math.Abs(box.Height - expected.Height) > 2.5)
            {
                yield return $"{name} {slot} #{i}: {box}, shadcn {expected}";
            }
        }
    }

    private static TextBlock ErrorText(Control field) =>
        field.GetVisualDescendants().OfType<TextBlock>().First(text => text.Text == "Enter a valid email address.");

    private static string? Slot(Control control) => control switch
    {
        Field => "field",
        FieldLabel => "field-label",
        FieldDescription => "field-description",
        FieldLegend => "field-legend",
        FieldSet => "field-set",
        Border { Name: "PART_Border" } box when box.TemplatedParent is CheckBox => "checkbox",
        Border { Name: "PART_BorderElement" } => "input",
        TextBlock { Text: "Enter a valid email address." } text when text.FindAncestorOfType<FieldLabel>() is null => "field-error",
        _ => null,
    };

    [Fact]
    [Trait("AC", "FIELD-03")]
    public async Task ADataValidationErrorShowsAsAFieldErrorAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new FormsView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var box = view.FindControl<TextBox>("EmailBox")!;
            model.Report(nameof(FormsModel.Email), "Enter a valid email address.");
            Windows.Settle();
            var part = Windows.Part<Border>(box, "PART_BorderElement");
            var message = box.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(text => text.Text == "Enter a valid email address.");
            var shown = (box.Classes.Contains(":error"), Windows.Paint(part.BorderBrush), message is null ? double.NaN : Windows.Box(message, box).Y - part.Bounds.Height, message?.FontSize, message is null ? default : Windows.Paint(message.Foreground));
            model.Report(nameof(FormsModel.Email), null);
            Windows.Settle();
            var cleared = (box.Classes.Contains(":error"), box.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == "Enter a valid email address." && text.IsEffectivelyVisible));
            window.Close();
            return (shown, cleared);
        });

        Assert.Equal((true, CssColor.Parse("#E7000B"), 12.0, (double?)14, CssColor.Parse("#E7000B")), found.shown);
        Assert.Equal((false, false), found.cleared);
    }

    [Fact]
    [Trait("AC", "FIELD-05")]
    public async Task ABoundInvalidClassTurnsTheFieldDestructiveAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new FormsModel();
            var view = new FormsView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var field = view.FindControl<Field>("Field")!;
            var label = view.FindControl<FieldLabel>("FieldLabel")!;
            var states = new List<(bool, CssColor)>();
            void Read() => states.Add((field.Classes.Contains("invalid"), Windows.Paint(label.Foreground)));
            Read();
            model.Invalid = true;
            Windows.Settle();
            Read();
            model.Invalid = false;
            Windows.Settle();
            Read();
            window.Close();
            return states;
        });

        Assert.Equal([(false, CssColor.Parse("#0A0A0A")), (true, CssColor.Parse("#E7000B")), (false, CssColor.Parse("#0A0A0A"))], found);
    }
}
