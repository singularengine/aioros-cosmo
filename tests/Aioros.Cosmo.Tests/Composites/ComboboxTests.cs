using Aioros.Cosmo.Search;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Aioros.Cosmo.Tests.Overlays;
using Avalonia;
using Command = Aioros.Cosmo.Search.Command;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Composites;

public sealed class ComboboxTests(DesktopSession desktop)
{
    private static readonly string[] Frameworks = ["Next.js", "SvelteKit", "Nuxt.js", "Remix", "Astro"];

    [Fact]
    [Trait("AC", "COMBOBOX-02")]
    public async Task TheTriggerAndThePopoverAreShadcnsAsync()
    {
        var wrong = await Looks.FixtureAsync(desktop, "combobox-demo", (window, root, reference, theme) => Trigger(window, root, reference, theme));
        wrong.AddRange(await Looks.FixtureAsync(desktop, "combobox-open", (window, root, reference, theme) =>
        {
            DisplayFixtures.Open(window, "combobox-open");
            var presenter = PopoverTests.Presenter(window);
            var errors = PopoverTests.Surface($"{theme} popover", Windows.Part<Border>(presenter, "LayoutRoot"), presenter.Padding, root, reference.Single(element => element.Slot == "popover-content"), 1.5, 1.5).ToList();
            var command = presenter.GetVisualDescendants().OfType<Command>().Single();
            errors.AddRange(Near($"{theme} input", Windows.Box(Windows.Part<Border>(command, "PART_InputWrapper"), root), reference.Single(element => element.Slot == "command-input-wrapper"), 1.5));
            var items = command.GetVisualDescendants().OfType<CommandItem>().ToList();
            var expected = reference.Where(element => element.Slot == "command-item").ToList();
            errors.AddRange(items.Count == expected.Count ? items.Zip(expected).SelectMany((pair, i) => Near($"{theme} item {i}", Windows.Box(pair.First, root), pair.Second, 1.5)) : [$"{theme}: {items.Count} items"]);
            errors.AddRange(items[0].IsSelected ? Paints($"{theme} selected", Windows.Part<Border>(items[0], "PART_Border").Background, expected[0].Background) : [$"{theme}: first item not selected"]);
            return errors;
        }));

        Assert.Empty(wrong);
    }

    private static List<string> Trigger(Window window, Control root, IReadOnlyList<ElementMetrics> reference, string theme)
    {
        var trigger = Windows.Part<Button>(All<Combobox>(root).Single(), "PART_Trigger");
        var expected = reference.Single(element => element.Slot == "popover-trigger");
        var errors = Near($"{theme} trigger", Windows.Box(trigger, root), expected, 1).ToList();
        errors.AddRange(Paints($"{theme} trigger border", trigger.BorderBrush, expected.BorderColor).Concat(Paints($"{theme} trigger fill", trigger.Background, expected.Background)));
        var chevron = trigger.GetVisualDescendants().OfType<Viewbox>().Single();
        var text = Windows.Part<TextBlock>(trigger, "PART_Placeholder");
        errors.AddRange(trigger.Padding.Left == 12 && trigger.FontWeight == FontWeight.Medium && text.Text == "Select framework..." && chevron.Bounds.Width == 16 && chevron.Opacity == 0.5 ? [] : [$"{theme}: trigger {trigger.Padding} {trigger.FontWeight} {text.Text} {chevron.Bounds} {chevron.Opacity}"]);
        errors.AddRange(Paints($"{theme} trigger text", text.Foreground, window, "CosmoForegroundBrush"));
        return errors;
    }

    [Fact]
    [Trait("AC", "COMBOBOX-03")]
    public async Task ChoosingSelectsTogglesAndClosesAsShadcnsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var single = new Combobox { PlaceholderText = "Select framework..." };
            var multiple = new Combobox { PlaceholderText = "Select frameworks...", SelectionMode = SelectionMode.Multiple };
            foreach (var name in Frameworks)
            {
                single.Items.Add(name);
                multiple.Items.Add(name);
            }

            var window = Windows.Show(new StackPanel { Spacing = 320, Margin = new Thickness(24), Children = { single, multiple } }, ThemeVariant.Light);
            Control Trigger(Combobox box) => Windows.Part<Button>(box, "PART_Trigger");
            string Shown(Combobox box) => string.Join('|', Trigger(box).GetVisualDescendants().OfType<TextBlock>().Where(block => block.IsEffectivelyVisible).Select(block => block.Text));
            Windows.Click(window, Trigger(single));
            Windows.Type(window, "re");
            Windows.Key(window, Key.Enter);
            var chosen = (single.SelectedItem as string, single.IsDropDownOpen, Shown(single));
            Windows.Click(window, Trigger(single));
            var checks = string.Join(',', single.List.Visible().Select(item => item.IsChecked == true ? "+" : "-"));
            var search = single.List.Text;
            Windows.Click(window, single.List.Visible().Single(item => item.EffectiveValue == "Remix"));
            var cleared = (single.SelectedItem, Shown(single));
            Windows.Click(window, Trigger(single));
            Windows.Key(window, Key.Escape);
            var escaped = (single.IsDropDownOpen, single.SelectedItem);
            Windows.Click(window, Trigger(multiple));
            Windows.Click(window, multiple.List.Visible().Single(item => item.EffectiveValue == "Next.js"));
            Windows.Click(window, multiple.List.Visible().Single(item => item.EffectiveValue == "Astro"));
            var both = (multiple.IsDropDownOpen, string.Join(',', multiple.SelectedItems!.Cast<string>()), Shown(multiple));
            Windows.Click(window, multiple.List.Visible().Single(item => item.EffectiveValue == "Astro"));
            var left = string.Join(',', multiple.SelectedItems!.Cast<string>());
            window.Close();
            return (chosen, checks, search, cleared, escaped, both, left);
        });

        Assert.Equal(((("Remix", false, "Remix"), "-,-,-,+,-", "", (null, "Select framework..."), (false, null), (true, "Next.js,Astro", "Next.js|Astro"), "Next.js")), found);
    }

    [Fact]
    [Trait("AC", "COMBOBOX-04")]
    public async Task TheBoundSelectionAndOpenStateFollowTheModelAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new CommandModel();
            var view = new CommandView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var single = view.FindControl<Combobox>("Single")!;
            Windows.Click(window, Windows.Part<Button>(single, "PART_Trigger"));
            Windows.Click(window, single.List.Visible().Single(item => item.EffectiveValue == "SvelteKit"));
            var chosen = model.Framework?.Label;
            model.Framework = model.Frameworks[4];
            Windows.Settle();
            var shown = string.Join('|', Windows.Part<Button>(single, "PART_Trigger").GetVisualDescendants().OfType<TextBlock>().Where(block => block.IsEffectivelyVisible).Select(block => block.Text));
            model.DropDown = true;
            Windows.Settle();
            var opened = single.List.IsEffectivelyVisible;
            model.DropDown = false;
            Windows.Settle();
            var multiple = view.FindControl<Combobox>("Multiple")!;
            Windows.Click(window, Windows.Part<Button>(multiple, "PART_Trigger"));
            Windows.Click(window, multiple.List.Visible().Single(item => item.EffectiveValue == "Next.js"));
            Windows.Click(window, multiple.List.Visible().Single(item => item.EffectiveValue == "Remix"));
            var collection = string.Join(',', model.Chosen.Cast<Framework>().Select(framework => framework.Label));
            window.Close();
            return (chosen, shown, opened, collection);
        });

        Assert.Equal(("SvelteKit", "Astro", true, "Next.js,Remix"), found);
    }
}
