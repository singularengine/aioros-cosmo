using Aioros.Cosmo.Search;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Command = Aioros.Cosmo.Search.Command;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Composites;

public sealed class CommandTests(DesktopSession desktop)
{
    private static readonly (string Value, string Search, string[] Keywords, double Score)[] CmdkScores =
    [
        ("Search Emoji", "se", [], 0.989901), ("Settings⌘S", "se", [], 0.989901), ("Calendar", "cal", [], 0.989901), ("Calculator", "cal", [], 0.989901),
        ("Calendar", "clr", [], 0.028752913132273364), ("Profile⌘P", "pf", [], 0.16794677194317), ("Billing⌘B", "bl", [], 0.16811488683),
        ("next.js", "nj", [], 0.792), ("Next.js", "NEXT", [], 0.98970302969901), ("sveltekit", "kit", [], 0.17), ("Getting Started", "gs", [], 0.8908218089100001),
        ("Search Emoji", "emoji", [], 0.89991), ("Calendar", "xyz", [], 0), ("Settings", "sttings", [], 0.169813017), ("Remix", "rmeix", [], 0.09999000000000001),
        ("a/b_c-d", "bcd", [], 0.5760000000000001), ("Billing", "card", [], 0), ("Billing", "card", ["credit card", "payment"], 0.891),
    ];

    [Fact]
    [Trait("AC", "COMMAND-02")]
    public async Task TheCommandAndItsItemsAreShadcnsAsync()
    {
        var wrong = await Looks.FixtureAsync(desktop, "command-demo", (window, root, reference, theme) => Check(window, root, reference, theme, false));
        wrong.AddRange(await FixtureAsync("command-filtered", (window, root, reference, theme) => Check(window, root, reference, theme, true)));
        Assert.Empty(wrong);
    }

    private Task<List<string>> FixtureAsync(string fixture, Func<Window, Control, IReadOnlyList<ElementMetrics>, string, IEnumerable<string>> check) =>
        Looks.FixtureAsync(desktop, fixture, (window, root, reference, theme) =>
        {
            DisplayFixtures.Open(window, fixture);
            return check(window, root, reference, theme);
        });

    private static List<string> Check(Window window, Control root, IReadOnlyList<ElementMetrics> reference, string theme, bool filtered)
    {
        var what = $"{theme} {(filtered ? "filtered" : "demo")}";
        var command = All<Command>(root).Single();
        var errors = Near($"{what} command", Windows.Box(command, root), reference.Single(element => element.Slot == "command"), 1.5).ToList();
        errors.AddRange(Near($"{what} input", Windows.Box(Windows.Part<Border>(command, "PART_InputWrapper"), root), reference.Single(element => element.Slot == "command-input-wrapper"), 1.5));
        var groups = All<CommandGroup>(root).Where(group => group.IsVisible).ToList();
        var items = All<CommandItem>(root).Where(item => item.IsEffectivelyVisible).ToList();
        var shortcuts = All<TextBlock>(root).Where(block => block.Name == "PART_Shortcut" && block.IsEffectivelyVisible).ToList();
        errors.AddRange(Pairs($"{what} group", groups, reference.Where(element => element.Slot == "command-group").ToList(), root));
        errors.AddRange(Pairs($"{what} item", items, reference.Where(element => element.Slot == "command-item").ToList(), root));
        errors.AddRange(Pairs($"{what} shortcut", shortcuts, reference.Where(element => element.Slot == "command-shortcut").ToList(), root));
        var first = items[0];
        var border = Windows.Part<Border>(first, "PART_Border");
        errors.AddRange(first.IsSelected && border.Padding == new Thickness(8, 6) && border.CornerRadius.TopLeft == 6 ? [] : [$"{what}: first item {first.IsSelected} {border.Padding} {border.CornerRadius}"]);
        errors.AddRange(Paints($"{what} selected", border.Background, reference.First(element => element.Slot == "command-item").Background)
            .Concat(Paints($"{what} selected text", first.Foreground, reference.First(element => element.Slot == "command-item").Color)));
        errors.AddRange(shortcuts.Where(block => block.FontSize != 12).Select(block => $"{what}: shortcut {block.FontSize}"));
        errors.AddRange(shortcuts.SelectMany(block => Paints($"{what} shortcut", block.Foreground, window, "CosmoMutedForegroundBrush")));
        var expected = reference.Single(element => element.Slot == "command");
        errors.AddRange(command.BorderThickness == new Thickness(1) && command.CornerRadius.TopLeft == 10 && CssShadow.Same(Windows.Shadows(Windows.Part<Border>(command, "PART_Root").BoxShadow), CssShadow.List(expected.Shadow)) ? [] : [$"{what}: outline {command.BorderThickness} {command.CornerRadius}"]);
        if (filtered)
        {
            errors.AddRange(items.Select(item => item.EffectiveValue).SequenceEqual(["Search Emoji", "Settings⌘S"]) && !All<CommandSeparator>(root).Single().IsVisible ? [] : [$"{what}: {string.Join(',', items.Select(item => item.EffectiveValue))}"]);
        }
        else
        {
            errors.AddRange(items.Where(item => !item.IsEnabled).Select(item => item.Opacity).SequenceEqual([0.5]) ? [] : [$"{what}: disabled opacity"]);
        }

        return errors;
    }

    private static IEnumerable<string> Pairs(string what, IReadOnlyList<Control> found, List<ElementMetrics> expected, Control root) =>
        found.Count != expected.Count
            ? [$"{what}: {found.Count}, shadcn {expected.Count}"]
            : found.Zip(expected).SelectMany((pair, i) => Near($"{what} {i}", Windows.Box(pair.First, root), pair.Second, 1.5));

    [Fact]
    [Trait("AC", "COMMAND-03")]
    public void TheScoreIsCmdks() =>
        Assert.Empty(CmdkScores.Where(pair => Math.Abs(CommandScore.Score(pair.Value, pair.Search, pair.Keywords) - pair.Score) > 1e-12)
            .Select(pair => $"{pair.Value}/{pair.Search}: {CommandScore.Score(pair.Value, pair.Search, pair.Keywords)}, cmdk {pair.Score}"));

    [Fact]
    [Trait("AC", "COMMAND-04")]
    public async Task TypingFiltersAndTheKeysMoveTheSelectionAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var ran = new List<string>();
            CommandItem Item(string text, bool enabled = true) => new() { Content = text, IsEnabled = enabled, Command = new Desktop.Command(parameter => ran.Add((string)parameter!)), CommandParameter = text };
            var command = new Command
            {
                Width = 400,
                Items =
                {
                    new CommandGroup { Header = "Suggestions", Items = { Item("Calendar"), Item("Search Emoji"), Item("Calculator", false) } },
                    new CommandSeparator(),
                    new CommandGroup { Header = "Settings", Items = { Item("Profile"), Item("Billing"), Item("Settings") } },
                },
            };
            var window = Windows.Show(command, ThemeVariant.Light);
            var input = Windows.Part<TextBox>(command, "PART_Input");
            input.Focus();
            string Selected() => command.SelectedItem?.EffectiveValue ?? "none";
            var steps = new List<string>();
            Windows.Type(window, "cal");
            steps.Add(string.Join('+', command.Visible().Select(item => item.EffectiveValue)) + ":" + Selected());
            Windows.Key(window, Key.Down);
            steps.Add(Selected());
            command.IsLoop = true;
            Windows.Key(window, Key.Down);
            steps.Add(Selected());
            command.IsLoop = false;
            command.Text = string.Empty;
            Windows.Settle();
            foreach (var (key, modifiers) in new[] { (Key.End, RawInputModifiers.None), (Key.Home, RawInputModifiers.None), (Key.N, RawInputModifiers.Control), (Key.P, RawInputModifiers.Control), (Key.Down, RawInputModifiers.Alt), (Key.Down, RawInputModifiers.Meta) })
            {
                window.KeyPress(key, modifiers, PhysicalKey.None, null);
                window.KeyRelease(key, modifiers, PhysicalKey.None, null);
                Windows.Settle();
                steps.Add(Selected());
            }

            Windows.Key(window, Key.Enter);
            Windows.Hover(window, command.Visible().Single(item => item.EffectiveValue == "Billing"));
            steps.Add(Selected());
            Windows.Type(window, "zzz");
            var empty = (command.IsEmpty, command.Visible().Count, All<CommandGroup>(command).Count(group => group.IsVisible), Windows.Part<Avalonia.Controls.Presenters.ContentPresenter>(command, "PART_Empty").IsEffectivelyVisible);
            window.Close();
            return (string.Join(' ', steps), string.Join(',', ran), empty);
        });

        Assert.Equal(("Calendar+Calculator:Calendar Calendar Calendar Settings Calendar Search Emoji Calendar Profile Settings Billing", "Settings", (true, 0, 0, true)), found);
    }

    [Fact]
    [Trait("AC", "COMMAND-05")]
    public async Task TheBoundTextValueAndItemsFollowTheModelAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new CommandModel();
            var view = new CommandView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var settings = view.FindControl<Command>("Settings")!;
            model.Search = "bil";
            Windows.Settle();
            var filtered = (string.Join(',', settings.Visible().Select(item => item.EffectiveValue)), model.Selected);
            var pages = view.FindControl<Command>("Pages")!;
            Windows.Part<TextBox>(pages, "PART_Input").Focus();
            Windows.Key(window, Key.Down);
            Windows.Key(window, Key.Enter);
            var locked = view.FindControl<CommandItem>("Locked")!;
            Windows.Part<TextBox>(settings, "PART_Input").Focus();
            model.Search = string.Empty;
            Windows.Settle();
            Windows.Key(window, Key.End);
            var skipped = (locked.IsEffectivelyEnabled, settings.SelectedItem?.EffectiveValue);
            window.Close();
            return (filtered, model.Opened, skipped);
        });

        Assert.Equal((("Billing", (string?)"Billing"), "/docs", (false, (string?)"Billing")), found);
    }

    [Fact]
    [Trait("AC", "COMMAND-06")]
    public async Task TheDialogOpensWithItsShortcutAsShadcnsAsync()
    {
        var wrong = await Looks.FixtureAsync(desktop, "command-dialog-open", (window, root, reference, theme) =>
        {
            DisplayFixtures.Open(window, "command-dialog-open");
            var dialog = All<CommandDialog>(root).Single();
            var surface = dialog.Surface;
            if (surface is null)
            {
                return [$"{theme}: the shortcut did not open the dialog"];
            }

            var top = (Control)window.Content!;
            var errors = Near($"{theme} surface", Windows.Box(surface, top), reference.Single(element => element.Slot == "dialog-content"), 1.5).ToList();
            var command = surface.GetVisualDescendants().OfType<Command>().Single();
            errors.AddRange(Near($"{theme} input", Windows.Box(Windows.Part<Border>(command, "PART_InputWrapper"), top), reference.Single(element => element.Slot == "command-input-wrapper"), 1.5));
            var items = command.GetVisualDescendants().OfType<CommandItem>().Where(item => item.IsEffectivelyVisible).ToList();
            var expectedItems = reference.Where(element => element.Slot == "command-item").ToList();
            errors.AddRange(items.Zip(expectedItems).Where(pair => pair.First.TranslatePoint(default, top) is { } at && pair.First.Bounds.Height is var height && (Math.Abs(at.Y - pair.Second.Y) > 1.5 || Math.Abs(height - pair.Second.Height) > 1.5)).Select(pair => $"{theme} item {pair.First.EffectiveValue}: {Windows.Box(pair.First, top)}"));
            var groups = command.GetVisualDescendants().OfType<CommandGroup>().ToList();
            errors.AddRange(groups.Select(group => group.Padding).SequenceEqual([new Thickness(8, 4), new Thickness(8, 0, 8, 4)]) ? [] : [$"{theme}: groups {string.Join(',', groups.Select(group => group.Padding))}"]);
            errors.AddRange(Windows.Part<Button>(surface, "PART_Close").IsEffectivelyVisible ? [] : [$"{theme}: no close button"]);
            var icon = Windows.Box(Windows.Part<Viewbox>(command, "PART_SearchIcon"), top);
            var field = reference.Single(element => element.Slot == "command-input");
            errors.AddRange(Math.Abs(icon.Right + 8 - field.X) <= 1.5 && icon.Width == 16 ? [] : [$"{theme}: the search icon at {icon}, shadcn's input at {field.X} (a 16 px icon 8 px before it)"]);
            return errors;
        });
        var behavior = await desktop.RunAsync(() =>
        {
            var model = new CommandModel();
            var view = new CommandView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var before = view.FindControl<Button>("Before")!;
            before.Focus(NavigationMethod.Tab);
            window.KeyPress(Key.K, RawInputModifiers.Meta, PhysicalKey.None, null);
            window.KeyRelease(Key.K, RawInputModifiers.Meta, PhysicalKey.None, null);
            Windows.Settle();
            var opened = model.Palette;
            Windows.Type(window, "pro");
            var dialog = view.FindControl<CommandDialog>("Palette")!;
            var filtered = string.Join(',', dialog.Surface!.GetVisualDescendants().OfType<Command>().Single().Visible().Select(item => item.EffectiveValue));
            Windows.Key(window, Key.Escape);
            var closed = (model.Palette, Windows.Focused(window) == before);
            window.KeyPress(Key.K, RawInputModifiers.Control, PhysicalKey.None, null);
            window.KeyRelease(Key.K, RawInputModifiers.Control, PhysicalKey.None, null);
            Windows.Settle();
            var reopened = model.Palette;
            window.KeyPress(Key.K, RawInputModifiers.Control, PhysicalKey.None, null);
            window.KeyRelease(Key.K, RawInputModifiers.Control, PhysicalKey.None, null);
            Windows.Settle();
            var toggled = model.Palette;
            window.Close();
            return (opened, filtered, closed, reopened, toggled);
        });

        Assert.Empty(wrong);
        Assert.Equal((true, "Profile", (false, true), true, false), behavior);
    }

    [Fact]
    [Trait("AC", "COMMAND-07")]
    public async Task ATriggerTakesTheDialogsGestureAsItsHotKeyAsync()
    {
        var result = await desktop.RunAsync(() =>
        {
            var runs = 0;
            var dialog = new CommandDialog { Gesture = new KeyGesture(Key.K, KeyModifiers.Control) };
            var first = new Button { Content = "Open the palette" };
            first.Command = new Desktop.Command(_ =>
            {
                runs++;
                dialog.IsOpen = !dialog.IsOpen;
            });
            dialog.Trigger = first;
            var other = new Button { Content = "Other", HotKey = new KeyGesture(Key.P, KeyModifiers.Control) };
            var second = new CommandDialog { Gesture = new KeyGesture(Key.J, KeyModifiers.Control), Trigger = other };
            var window = Windows.Show(new StackPanel { Children = { first, other, dialog, second } }, ThemeVariant.Light);
            var keys = (first.HotKey?.ToString(), other.HotKey?.ToString());
            window.KeyPress(Key.K, RawInputModifiers.Control, PhysicalKey.None, null);
            window.KeyRelease(Key.K, RawInputModifiers.Control, PhysicalKey.None, null);
            Windows.Settle();
            var pressed = (runs, dialog.IsOpen);
            dialog.IsOpen = false;
            Windows.Settle();
            window.KeyPress(Key.K, RawInputModifiers.Meta, PhysicalKey.None, null);
            window.KeyRelease(Key.K, RawInputModifiers.Meta, PhysicalKey.None, null);
            Windows.Settle();
            var meta = (runs, dialog.IsOpen);
            dialog.IsOpen = false;
            dialog.Trigger = null;
            var cleared = first.HotKey;
            window.Close();
            return (keys, pressed, meta, cleared);
        });

        Assert.Equal((("Ctrl+K", "Ctrl+P"), (1, true), (1, true), (KeyGesture?)null), result);
    }

    [Fact]
    [Trait("AC", "COMMAND-08")]
    public async Task ADialogWithoutATriggerKeepsItsKeyForTheWebAsync()
    {
        var keys = await desktop.RunAsync(() =>
        {
            var dialog = new CommandDialog { Gesture = new KeyGesture(Key.I, KeyModifiers.Control) };
            var alone = Aioros.Markup.Web.GetModalKey(dialog)?.ToString();
            dialog.Trigger = new Button { Content = "Open" };
            var triggered = Aioros.Markup.Web.GetModalKey(dialog)?.ToString();
            dialog.Trigger = null;
            return (alone, triggered, Aioros.Markup.Web.GetModalKey(dialog)?.ToString());
        });

        Assert.Equal(("Ctrl+I", (string?)null, "Ctrl+I"), keys);
    }

    [Fact]
    [Trait("AC", "COMMAND-09")]
    public async Task AClickRaisedOnAnItemSelectsItBeforeItsCommandAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            Command? command = null;
            var seen = new List<string?>();
            CommandItem Item(string text) => new() { Content = text, Command = new Desktop.Command(_ => seen.Add(command!.SelectedValue)) };
            var accordion = Item("Accordion");
            command = new Command
            {
                Width = 400,
                Items =
                {
                    new CommandGroup { Header = "Pages", Items = { Item("Dashboard"), Item("Forms") } },
                    new CommandGroup { Header = "Components", Items = { Item("Calendar"), accordion, Item("Sidebar") } },
                },
            };
            var window = Windows.Show(command, ThemeVariant.Light);
            command.Text = "ar";
            Windows.Settle();
            var first = command.SelectedValue;
            accordion.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            accordion.Command!.Execute(null);
            Windows.Settle();
            var selected = (command.SelectedItem == accordion, command.SelectedValue);
            window.Close();
            return (first, selected, string.Join(',', seen));
        });

        Assert.Equal(("Calendar", (true, (string?)"Accordion"), "Accordion"), found);
    }

    [Fact]
    [Trait("AC", "COMMAND-12")]
    public async Task AGroupGivesItsItemsTheirCommandAndParameterAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var ran = new List<string>();
            var shared = new Desktop.Command(parameter => ran.Add($"shared {parameter}"));
            var group = new CommandGroup
            {
                Header = "Pages",
                ItemsSource = new[] { new PageItem("Docs", "/docs"), new PageItem("Blocks", "/blocks"), new PageItem("Themes", "/themes") },
                ItemTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<PageItem>((page, _) => new TextBlock { Text = page?.Title }),
                ItemCommand = new Avalonia.Data.Binding { Source = shared },
                ItemCommandParameter = new Avalonia.Data.Binding(nameof(PageItem.Path)),
            };
            var literal = new CommandGroup
            {
                Header = "Literal",
                ItemCommand = new Avalonia.Data.Binding { Source = shared },
                Items = { new CommandItem { Content = "Own", Command = new Desktop.Command(parameter => ran.Add($"own {parameter}")), CommandParameter = "/own" } },
            };
            var command = new Command { Width = 400, Items = { group, literal } };
            var window = Windows.Show(command, ThemeVariant.Light);
            Windows.Part<TextBox>(command, "PART_Input").Focus();
            Windows.Key(window, Key.Down);
            Windows.Key(window, Key.Enter);
            Windows.Key(window, Key.End);
            Windows.Key(window, Key.Enter);
            window.Close();
            return string.Join(',', ran);
        });

        Assert.Equal("shared /blocks,own /own", found);
    }
}
