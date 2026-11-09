using Aioros.Cosmo.Forms;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Forms;

public sealed class FieldLayoutTests(DesktopSession desktop)
{
    private static string? Slot(Control control) => control switch
    {
        FieldGroup => "field-group",
        Field => "field",
        FieldContent => "field-content",
        FieldLabel or FieldTitle => "field-label",
        FieldDescription => "field-description",
        Border { Name: "OuterBorder" } track when track.TemplatedParent is ToggleSwitch => "switch",
        Avalonia.Controls.Shapes.Ellipse { Name: "SwitchKnob" } knob when knob.TemplatedParent is ToggleSwitch => "switch-thumb",
        Border { Name: "PART_Border" } box when box.TemplatedParent is CheckBox => "checkbox",
        Border { Name: "PART_BorderElement" } => "input",
        _ => null,
    };

    private static IEnumerable<string> Parts(string what, Control root, IReadOnlyList<ElementMetrics> reference)
    {
        var parts = root.GetVisualDescendants().OfType<Control>().Where(control => control.IsEffectivelyVisible).Select(control => (Slot: Slot(control), Control: control)).Where(part => part.Slot is not null).ToList();
        foreach (var slot in reference.Select(element => element.Slot).Distinct())
        {
            var ours = parts.Where(part => part.Slot == slot).Select(part => Windows.Box(part.Control, root)).ToList();
            var theirs = reference.Where(element => element.Slot == slot).ToList();
            if (ours.Count != theirs.Count)
            {
                yield return $"{what} {slot}: {ours.Count} parts, shadcn {theirs.Count}";
                continue;
            }

            foreach (var (box, expected) in ours.Zip(theirs))
            {
                if (Math.Abs(box.X - expected.X) > 2.5 || Math.Abs(box.Y - expected.Y) > 2.5 || Math.Abs(box.Width - expected.Width) > 2.5 || Math.Abs(box.Height - expected.Height) > 2.5)
                {
                    yield return $"{what} {slot} {expected.Text}: {box}, shadcn {expected.X}, {expected.Y}, {expected.Width}, {expected.Height}";
                }
            }
        }
    }

    [Fact]
    [Trait("AC", "FIELD-08")]
    public async Task AFieldContentTakesTheRowItsControlLeavesAsync()
    {
        var wrong = await FixtureAsync(desktop, "field-content", (_, root, reference, theme) => Parts(theme, root, reference));
        wrong.AddRange(await desktop.RunAsync(() =>
        {
            var label = new FieldLabel { Content = "Password" };
            var box = new TextBox();
            var field = new Field { Spacing = 12, Height = 200, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Children = { label, box } };
            var window = Windows.Show(field, ThemeVariant.Light);
            var found = (Label: Windows.Box(label, field), Box: Windows.Box(box, field));
            window.Close();
            return found == (new Rect(0, 0, found.Label.Width, 20), new Rect(0, 32, found.Box.Width, 36)) ? (string[])[] : [$"a tall field: label {found.Label}, text box {found.Box}"];
        }));

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "FIELD-09")]
    public async Task AResponsiveFieldTurnsAtItsGroupsWidthAsync() =>
        Assert.Empty(await FixtureAsync(desktop, "field-responsive", (_, root, reference, theme) => Parts(theme, root, reference)));

    [Fact]
    [Trait("AC", "FIELD-13")]
    public async Task AHiddenPartLeavesNoGapAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var box = new TextBox();
            var description = new FieldDescription { Text = "We send one email a month." };
            var next = new FieldLabel { Content = "Name" };
            var group = new FieldGroup { Width = 448, Children = { new Field { Children = { new FieldLabel { Content = "Email" }, box, description } }, new Field { Children = { next, new TextBox() } } } };
            var window = Windows.Show(group, ThemeVariant.Light);
            var shown = Windows.Box(next, group).Y - Windows.Box(box, group).Bottom;
            description.IsVisible = false;
            Windows.Settle();
            var hidden = Windows.Box(next, group).Y - Windows.Box(box, group).Bottom;
            window.Close();
            return (Shown: shown, Hidden: hidden);
        });

        Assert.Equal((28.0 + 33, 28.0), found);
    }

    [Fact]
    [Trait("AC", "FIELD-15")]
    public async Task ADescriptionBeforeTheLastPartHugsItsTitleAsync()
    {
        var wrong = await FixtureAsync(desktop, "slider-range", (_, root, reference, theme) =>
        {
            var title = Windows.Box(All<FieldTitle>(root).Single(), root);
            var description = Windows.Box(All<FieldDescription>(root).Single(), root);
            var expected = reference.Single(element => element.Slot == "field-description").Y - reference.First(element => element.Slot == "field-label").Y;
            return Math.Abs(description.Y - title.Y - expected) <= 1 ? [] : [$"{theme}: description {description.Y - title.Y} below the title, shadcn {expected}"];
        });
        wrong.AddRange(await desktop.RunAsync(() =>
        {
            var box = new TextBox();
            var last = new FieldDescription { Text = "Choose a unique username for your account." };
            var field = new Field { Width = 448, Children = { new FieldLabel { Content = "Username" }, box, last } };
            var window = Windows.Show(field, ThemeVariant.Light);
            var gap = Windows.Box(last, field).Y - Windows.Box(box, field).Bottom;
            window.Close();
            return gap == 12 ? (string[])[] : [$"a last description {gap} below its control"];
        }));

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "BUTTON-12")]
    public async Task AnInvalidButtonTakesTheDestructiveBorderAndRingAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var rest = await ReferenceMetrics.SlotsAsync("button-invalid", name, "button");
            var focus = await ReferenceMetrics.OfAsync("button-invalid", name, "focus");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = Display.DisplayFixtures.Show("button-invalid", variant);
                var buttons = ((Control)window.Content!).GetVisualDescendants().OfType<Button>().ToList();
                var errors = new List<string>();
                for (var i = 0; i < buttons.Count; i++)
                {
                    var presenter = Windows.Part<ContentPresenter>(buttons[i], "PART_ContentPresenter");
                    errors.AddRange(Border($"{name} #{i} rest", presenter, rest[i]));
                    var box = Windows.Box(buttons[i], (Control)window.Content!);
                    errors.AddRange(Math.Abs(box.X - rest[i].X) <= 1.5 && Math.Abs(box.Y - rest[i].Y) <= 1.5 && Math.Abs(box.Width - rest[i].Width) <= 1.5 && Math.Abs(box.Height - rest[i].Height) <= 1.5 ? [] : [$"{name} #{i} box {box}, shadcn {rest[i].X}, {rest[i].Y}, {rest[i].Width}, {rest[i].Height}"]);
                    buttons[i].Focus(Avalonia.Input.NavigationMethod.Tab);
                    Windows.Settle();
                    errors.AddRange(Border($"{name} #{i} focus", presenter, focus[i]));
                    errors.AddRange(CssShadow.Same(Windows.Shadows(presenter.BoxShadow), CssShadow.List(focus[i].Shadow)) ? [] : [$"{name} #{i} ring {presenter.BoxShadow}, shadcn {focus[i].Shadow}"]);
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Border(string what, ContentPresenter presenter, ElementMetrics expected)
    {
        var width = double.Parse(expected.BorderWidth.TrimEnd('p', 'x'), System.Globalization.CultureInfo.InvariantCulture);
        if (presenter.BorderThickness.Top != width)
        {
            yield return $"{what} border width {presenter.BorderThickness.Top}, shadcn {width}";
        }
        else if (width > 0 && !Windows.Paint(presenter.BorderBrush).Near(CssColor.Parse(expected.BorderColor)))
        {
            yield return $"{what} border {Windows.Paint(presenter.BorderBrush)}, shadcn {expected.BorderColor}";
        }
    }
}
