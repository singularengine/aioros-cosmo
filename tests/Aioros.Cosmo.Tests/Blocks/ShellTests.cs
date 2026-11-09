using Aioros.Cosmo.Display;
using Aioros.Cosmo.Navigation;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Gallery.Models;
using SkiaSharp;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Blocks;

public sealed class ShellTests(DesktopSession desktop)
{
    internal static readonly string[] Shells = ["dashboard-01", .. Enumerable.Range(1, 16).Select(static n => $"sidebar-{n:00}")];

    private static readonly (string Fixture, double Floor)[] Floors =
    [
        ("dashboard-01", 0.975), ("sidebar-01", 0.985), ("sidebar-02", 0.985), ("sidebar-03", 0.990), ("sidebar-04", 0.990), ("sidebar-05", 0.985), ("sidebar-06", 0.985),
        ("sidebar-07", 0.990), ("sidebar-08", 0.990), ("sidebar-09", 0.950), ("sidebar-10", 0.970), ("sidebar-11", 0.990), ("sidebar-12", 0.985), ("sidebar-13", 0.990),
        ("sidebar-14", 0.990), ("sidebar-15", 0.970), ("sidebar-16", 0.985),
    ];

    /// <summary>
    /// Our controls of a slot of shadcn/ui's render: a menu button that is its collapsible item's trigger or a menu's
    /// trigger carries the trigger's slot there (Radix's <c>asChild</c> gives it <c>collapsible-trigger</c> or
    /// <c>dropdown-menu-trigger</c>), so it is left out; a breadcrumb item is its link or page; sidebar-16's 32 px trigger
    /// is the plain button of its site header.
    /// </summary>
    internal static IEnumerable<Control> Slot(Control root, string slot) => slot switch
    {
        "sidebar-menu-button" => All<SidebarMenuButton>(root).Where(static button => button.Flyout is null && !(button.Parent is SidebarMenuItem { IsCollapsible: true } item && !item.Children.OfType<SidebarMenuAction>().Any(static action => action.Toggles))),
        "sidebar-menu-sub-button" => All<SidebarMenuSubButton>(root),
        "sidebar-group-label" => All<Control>(root).Where(static control => control is SidebarGroupLabel || (control is ToggleButton { Name: "ExpanderHeader" } && control.FindAncestorOfType<Expander>()?.Classes.Contains("sidebar-group") == true)),
        "sidebar-trigger" => All<SidebarTrigger>(root).Where(static trigger => trigger.Width is not 32),
        "button" => All<SidebarTrigger>(root).Where(static trigger => trigger.Width is 32),
        "sidebar-inset" => All<SidebarInset>(root),
        "breadcrumb-item" => All<Breadcrumb>(root).SelectMany(static crumbs => crumbs.GetVisualDescendants().OfType<Control>().Where(static control => control is HyperlinkButton or BreadcrumbPage)),
        "card" => All<Card>(root),
        "tabs-trigger" => All<TabItem>(root),
        _ => [],
    };

    /// <summary>A control's box in the root by its center, so a turned control (sidebar-14's trigger) stands where it is laid out.</summary>
    internal static Rect Laid(Control control, Control root) =>
        control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), root) is { } center
            ? new Rect(center.X - (control.Bounds.Width / 2), center.Y - (control.Bounds.Height / 2), control.Bounds.Width, control.Bounds.Height)
            : default;

    private static IEnumerable<string> Boxes(string what, Control root, IReadOnlyList<ElementMetrics> reference, string slot)
    {
        var expected = reference.Where(element => element.Slot == slot).OrderBy(static element => element.Y).ThenBy(static element => element.X).ToList();
        var found = Slot(root, slot).Where(static control => control.IsEffectivelyVisible).Select(control => Laid(control, root)).OrderBy(static box => box.Y).ThenBy(static box => box.X).ToList();
        if (found.Count != expected.Count)
        {
            return [$"{what}: {found.Count} {slot}s, shadcn {expected.Count}"];
        }

        var frame = root.Bounds.Height;
        return found.Zip(expected).SelectMany((pair, i) =>
            Math.Abs(pair.First.X - pair.Second.X) <= 1.5 && Math.Abs(pair.First.Y - pair.Second.Y) <= 3 && Math.Abs(pair.First.Width - pair.Second.Width) <= 1.5
            && Math.Abs(pair.First.Height - (slot == "sidebar-inset" && pair.Second.Height > frame - pair.Second.Y ? frame - pair.Second.Y - (pair.Second.Y < 16 ? pair.Second.Y : 0) : pair.Second.Height)) <= 3
                ? []
                : (IEnumerable<string>)[$"{what} {slot} #{i} {pair.Second.Text[..Math.Min(40, pair.Second.Text.Length)]}: {pair.First}, shadcn {Of(pair.Second)}"]);
    }

    [Fact]
    [Trait("AC", "BLOCKS-09")]
    public async Task EveryShellStandsAtShadcnsBoxesAsync()
    {
        var wrong = new List<string>();
        foreach (var shell in Shells.Where(static shell => shell != "sidebar-13"))
        {
            wrong.AddRange(await FixtureAsync(desktop, shell, (_, root, reference, theme) =>
                ((string[])["sidebar-menu-button", "sidebar-menu-sub-button", "sidebar-group-label", "sidebar-trigger", "sidebar-inset", "breadcrumb-item", .. shell == "dashboard-01" ? (string[])["card", "tabs-trigger"] : [], .. shell == "sidebar-16" ? (string[])["button"] : []])
                    .SelectMany(slot => Boxes($"{shell} {theme}", root, reference, slot))));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "FIDELITY-21")]
    public async Task EveryShellMatchesShadcnsRenderOnTheDesktopAsync()
    {
        var verdicts = new List<string>();
        var wrong = new List<string>();
        foreach (var (variant, theme) in Themes)
        {
            foreach (var (fixture, floor) in Floors)
            {
                var (matched, size, expected) = await desktop.RunAsync<(double, (int, int), (int, int))>(() =>
                {
                    var window = DisplayFixtures.Show(fixture, variant);
                    using var ours = Windows.Capture(window);
                    using var reference = Pixels.Reference(fixture, theme);
                    if (Environment.GetEnvironmentVariable("COSMO_CAPTURES") is { Length: > 0 } folder)
                    {
                        using var file = File.Create(Path.Combine(folder, $"{fixture}.{theme}.png"));
                        ours.Encode(file, SKEncodedImageFormat.Png, 100);
                    }

                    window.Close();
                    return (Pixels.Matched(ours, reference), (ours.Width, ours.Height), (reference.Width, reference.Height));
                });
                verdicts.Add($"{fixture}.{theme}: {matched:P2}");
                if (size != expected || matched < floor)
                {
                    wrong.Add($"{fixture}.{theme}: {size} (shadcn {expected}), {matched:P2} of pixels within {Pixels.Tolerance}/255, floor {floor:P0}");
                }
            }
        }

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join(Environment.NewLine, verdicts));
        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    private static Task<T> ShownAsync<T>(DesktopSession desktop, string fixture, Func<Window, Control, T> act) => desktop.RunAsync(() =>
    {
        var window = DisplayFixtures.Show(fixture, Avalonia.Styling.ThemeVariant.Light);
        try
        {
            return act(window, (Control)window.Content!);
        }
        finally
        {
            window.Close();
        }
    });

    private static void Press(Window window, Control control)
    {
        control.BringIntoView();
        Windows.Settle();
        Windows.Click(window, control);
    }

    private static T Model<T>(Control root) => root.GetVisualDescendants().Select(static visual => (visual as Control)?.DataContext).OfType<T>().First();

    private static double Width(Control root, int index = 0) => All<Sidebar>(root)[index].Bounds.Width;

    private static T Named<T>(Control root, string name) where T : Control =>
        All<T>(root).First(control => Avalonia.Automation.AutomationProperties.GetName(control) == name || (control as ContentControl)?.Content as string == name || Text(control) == name);

    private static string? Text(Control control) => control.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(block => !string.IsNullOrEmpty(block.Text))?.Text;

    [Fact]
    [Trait("AC", "BLOCKS-12")]
    public async Task EveryShellsSidebarFoldsAndUnfoldsAsync()
    {
        var found = new List<string>
        {
            await ShownAsync(desktop, "sidebar-07", (window, root) =>
            {
                Windows.Click(window, All<SidebarTrigger>(root).Single());
                var folded = Width(root);
                Windows.Click(window, All<SidebarRail>(root).Single());
                return $"07 {folded} {Width(root)}";
            }),
            await ShownAsync(desktop, "sidebar-01", (window, root) =>
            {
                Windows.Click(window, All<SidebarTrigger>(root).Single());
                return $"01 {Width(root)} inset {All<SidebarInset>(root).Single().Bounds.Width}";
            }),
            await ShownAsync(desktop, "sidebar-14", (window, root) =>
            {
                var trigger = All<SidebarTrigger>(root).Single();
                var after = Windows.Box(trigger, root).X > Windows.Box(All<BreadcrumbItem>(root)[^1], root).Right;
                Windows.Click(window, trigger);
                return $"14 {after} {Width(root)}";
            }),
            await ShownAsync(desktop, "sidebar-16", (window, root) =>
            {
                Windows.Click(window, Named<Button>(root, "Toggle Sidebar"));
                var header = All<Border>(root).First(border => border.Height == 57);
                return $"16 {Width(root)} header {Windows.Box(header, root).Width} sidebar at {Windows.Box(All<Sidebar>(root).Single(), root).Y}";
            }),
            await ShownAsync(desktop, "sidebar-15", (window, root) =>
            {
                Windows.Click(window, All<SidebarTrigger>(root).Single());
                return $"15 {Width(root)} {Width(root, 1)}";
            }),
        };

        Assert.Equal(["07 48 256", "01 0 inset 1280", "14 True 0", "16 0 header 1280 sidebar at 57", "15 0 256"], found);
    }

    [Fact]
    [Trait("AC", "BLOCKS-14")]
    public async Task TheSwitchersPickTheVersionAndTheTeamAsync()
    {
        var found = new List<string>
        {
            await ShownAsync(desktop, "sidebar-01", (window, root) =>
            {
                var switcher = All<SidebarMenuButton>(root)[0];
                Windows.Click(window, switcher);
                var flyout = (MenuFlyout)switcher.Flyout!;
                var beta = flyout.Items.OfType<MenuItem>().Single(item => item.CommandParameter as string == "2.0.0-beta1");
                Windows.Click(window, beta);
                var model = (DocsExample)switcher.DataContext!;
                var shown = switcher.GetVisualDescendants().OfType<TextBlock>().Select(block => block.Text).ToList();
                var closed = flyout.IsOpen;
                Windows.Click(window, switcher);
                var checks = flyout.Items.OfType<MenuItem>().Where(item => ((Control)item.Header!).GetVisualDescendants().OfType<Viewbox>().Any(box => box.IsVisible)).Select(item => item.CommandParameter);
                return $"01 {shown[1]} {model.Version} checked {string.Join(",", checks)} open {closed}";
            }),
            await ShownAsync(desktop, "sidebar-07", (window, root) => Team(window, root, "07")),
            await ShownAsync(desktop, "sidebar-10", (window, root) =>
            {
                Model<ShellExample>(root).ActionsOpen = false;
                Windows.Settle();
                return Team(window, root, "10");
            }),
        };

        Assert.Equal(["01 v2.0.0-beta1 2.0.0-beta1 checked 2.0.0-beta1 open False", "07 Acme Corp. Startup audio Ctrl+2", "10 Acme Corp. Startup audio Ctrl+2"], found);
    }

    private static string Team(Window window, Control root, string what)
    {
        var switcher = All<SidebarMenuButton>(root)[0];
        var flyout = (MenuFlyout)switcher.Flyout!;
        Windows.Click(window, switcher);
        var corp = flyout.Items.OfType<MenuItem>().Single(item => item.Header as string == "Acme Corp.");
        Windows.Click(window, corp);
        var model = (ShellExample)switcher.DataContext!;
        var texts = switcher.GetVisualDescendants().OfType<TextBlock>().Where(block => block.IsEffectivelyVisible).Select(block => block.Text).Where(text => !string.IsNullOrEmpty(text)).ToList();
        var logos = switcher.GetVisualDescendants().OfType<Viewbox>().Count(box => box.IsEffectivelyVisible && box.Parent is Panel { Parent: Border });
        return $"{what} {texts[0]} {model.ActiveTeam.Plan} {(model.ShowsWaveform && logos == 1 ? "audio" : "other")} {corp.InputGesture?.ToString("p", null)}";
    }

    private static SidebarMenuItem Item(Control root, string title) => All<SidebarMenuItem>(root).First(item => item.Children.OfType<SidebarMenuButton>().Any(button => button.GetVisualDescendants().OfType<TextBlock>().Any(block => block.Text == title) || button.Content as string == title));

    [Fact]
    [Trait("AC", "BLOCKS-16")]
    public async Task TheShellsCollapsiblePartsFoldAndUnfoldAsync()
    {
        var found = new List<string>
        {
            await ShownAsync(desktop, "sidebar-02", (window, root) =>
            {
                var group = All<Expander>(root).First(expander => expander.Classes.Contains("sidebar-group"));
                Windows.Click(window, Windows.Part<ToggleButton>(group, "ExpanderHeader"));
                return $"02 {group.IsExpanded}";
            }),
            await ShownAsync(desktop, "sidebar-05", (window, root) =>
            {
                var item = Item(root, "Getting Started");
                var button = item.Children.OfType<SidebarMenuButton>().Single();
                string Icon() => string.Join(",", ((string[])["PART_Chevron", "PART_Minus"]).Select(name => Windows.Part<Viewbox>(button, name)).Where(static box => box.IsEffectivelyVisible)
                    .Select(box => Windows.Part<Avalonia.Controls.Shapes.Path>(box).Data?.Bounds.Height > 0 ? "plus" : "minus"));
                var closed = Icon();
                Windows.Click(window, button);
                return $"05 {item.IsExpanded} {closed} {Icon()}";
            }),
            await ShownAsync(desktop, "sidebar-07", (window, root) =>
            {
                var item = Item(root, "Models");
                Windows.Click(window, item.Children.OfType<SidebarMenuButton>().Single());
                return $"07 {item.IsExpanded} {item.Children.OfType<SidebarMenuSub>().Single().IsEffectivelyVisible}";
            }),
            await ShownAsync(desktop, "sidebar-08", (window, root) =>
            {
                var item = Item(root, "Models");
                Windows.Click(window, item.Children.OfType<SidebarMenuButton>().Single());
                var byLink = item.IsExpanded;
                var action = item.Children.OfType<SidebarMenuAction>().Single();
                Windows.Click(window, action);
                return $"08 {byLink} {item.IsExpanded} {action.RenderTransform?.Value.M12 > 0.99}";
            }),
            await ShownAsync(desktop, "sidebar-10", (window, root) =>
            {
                Model<ShellExample>(root).ActionsOpen = false;
                Windows.Settle();
                var item = Item(root, "Personal Life Management");
                Windows.Hover(window, item);
                var chevron = item.Children.OfType<SidebarMenuAction>().First(action => action.Toggles);
                var start = Windows.Box(chevron, root).X - Windows.Box(item, root).X;
                Windows.Click(window, chevron);
                return $"10 {start} {item.IsExpanded}";
            }),
            await ShownAsync(desktop, "sidebar-11", (window, root) =>
            {
                var item = Item(root, "app");
                var button = item.Children.OfType<SidebarMenuButton>().Single();
                Windows.Click(window, button);
                var chevron = button.GetVisualDescendants().OfType<Viewbox>().First(box => box.Classes.Contains("chevron"));
                return $"11 {item.IsExpanded} {chevron.RenderTransform?.Value.M12 > 0.99} {Windows.Box(chevron, root).X < Windows.Box(button.GetVisualDescendants().OfType<Viewbox>().First(box => box != chevron && box.Name is not ("PART_Chevron" or "PART_Minus")), root).X}";
            }),
            await ShownAsync(desktop, "sidebar-12", (window, root) =>
            {
                var favorites = All<Expander>(root).Single(expander => expander.Classes.Contains("calendars") && Text(Windows.Part<ToggleButton>(expander, "ExpanderHeader")) == "Favorites");
                Windows.Click(window, Windows.Part<ToggleButton>(favorites, "ExpanderHeader"));
                return $"12 {favorites.IsExpanded} {string.Join(",", favorites.GetVisualDescendants().OfType<SidebarMenuButton>().Where(button => button.IsEffectivelyVisible).Select(Text))}";
            }),
        };

        Assert.Equal(["02 False", "05 True plus minus", "07 True True", "08 False True True", "10 8 True", "11 True True True", "12 True Holidays,Birthdays"], found);
    }

    [Fact]
    [Trait("AC", "BLOCKS-18")]
    public async Task TheFoldersThePopoverAndTheDialogOpenAsTheBlocksDoAsync()
    {
        var found = new List<string>
        {
            await ShownAsync(desktop, "sidebar-10", (window, root) =>
            {
                var more = All<Button>(root).Single(button => button.Flyout is Flyout);
                var presenter = window.GetVisualDescendants().OfType<FlyoutPresenter>().FirstOrDefault() ?? TopLevel.GetTopLevel(window)?.GetVisualDescendants().OfType<FlyoutPresenter>().FirstOrDefault();
                var open = more.Flyout?.IsOpen;
                return $"10 {open} {presenter?.Bounds.Width}";
            }),
            await ShownAsync(desktop, "sidebar-13", (window, root) =>
            {
                var surface = window.GetVisualDescendants().OfType<Aioros.Cosmo.Overlays.DialogSurface>().Single();
                var size = $"{surface.Bounds.Width}x{surface.Bounds.Height}";
                Windows.Key(window, Key.Escape);
                var model = Model<ShellExample>(root);
                var closed = model.SettingsOpen;
                Windows.Click(window, Named<Button>(root, "Open Dialog"));
                return $"13 {size} {closed} {model.SettingsOpen}";
            }),
            await ShownAsync(desktop, "sidebar-09", (window, root) =>
            {
                var provider = All<SidebarProvider>(root).Single();
                provider.IsOpen = false;
                Windows.Settle();
                var drafts = All<SidebarMenuButton>(root).First(button => button.Tooltip == "Drafts");
                Windows.Click(window, drafts);
                var model = (ShellExample)provider.DataContext!;
                var title = All<TextBlock>(root).First(block => block.FontSize == 16 && block.FontWeight == Avalonia.Media.FontWeight.Medium).Text;
                Windows.Click(window, All<ToggleSwitch>(root).Single());
                return $"09 {title} {drafts.IsActive} {provider.IsOpen} {Width(root)} unreads {model.Unreads}";
            }),
        };

        Assert.Equal(["10 True 224", "13 800x500 False True", "09 Drafts True True 350 unreads True"], found);
    }

    [Fact]
    [Trait("AC", "BLOCKS-20")]
    public async Task TheDashboardsChartAndTabsFollowTheirChoicesAsync()
    {
        var found = await ShownAsync(desktop, "dashboard-01", (window, root) =>
        {
            var model = Model<DashboardBlock>(root);
            var cards = All<Card>(root).Where(card => card.Classes.Contains("section")).Select(card => Windows.Box(card, root)).ToList();
            var grid = $"{cards.Select(box => box.X).Distinct().Count()}x{cards.Select(box => box.Y).Distinct().Count()}";
            var range = All<ListBox>(root).Single(list => list.Classes.Contains("range"));
            var days = new List<int> { model.Visitors.Range.Count };
            foreach (var index in (int[])[1, 2])
            {
                Press(window, range.ContainerFromIndex(index)!);
                days.Add(model.Visitors.Range.Count);
            }

            var tabs = All<TabItem>(root);
            Press(window, tabs[1]);
            var placeholder = All<Avalonia.Controls.Shapes.Rectangle>(root).Count(rectangle => rectangle.IsEffectivelyVisible && rectangle.StrokeDashArray is { Count: > 0 });
            var table = All<TableView>(root).Any(view => view.IsEffectivelyVisible);
            var columns = Named<Button>(root, "Customize Columns").IsEffectivelyVisible;
            return $"{grid} {string.Join(",", days)} tab {model.Tab} placeholder {placeholder} table {table} buttons {columns}";
        });

        Assert.Equal("2x2 91,31,8 tab 1 placeholder 1 table False buttons True", found);
    }

    [Fact]
    [Trait("AC", "BLOCKS-22")]
    public async Task TheDashboardsTablePagesSelectsHidesAssignsReordersAndOpensAsync()
    {
        var found = await ShownAsync(desktop, "dashboard-01", (window, root) =>
        {
            var model = Model<DashboardBlock>(root);
            var steps = new List<string>();
            All<TableView>(root).Single().BringIntoView();
            Windows.Settle();
            int Selects()
            {
                Named<Button>(root, "Go to last page").BringIntoView();
                Windows.Settle();
                return All<ComboBox>(root).Count(box => box.IsEffectivelyVisible && box.PlaceholderText == "Assign reviewer");
            }

            string Page() => All<TextBlock>(root).First(block => block.Text?.StartsWith("Page ", StringComparison.Ordinal) == true).Text!;
            var badges = All<Badge>(root).Select(badge => (Text: badge.GetVisualDescendants().OfType<TextBlock>().Select(block => block.Text).LastOrDefault(), Badge: badge))
                .Where(badge => badge.Text is "In Process" or "Done").Take(2)
                .Select(badge => $"{badge.Text}:{badge.Badge.GetVisualDescendants().OfType<Viewbox>().Count(box => box.IsEffectivelyVisible)}").ToList();
            steps.Add($"{model.Sections.Rows.Count} of {model.Sections.Items.Count} {Page()} {string.Join(" ", badges)} selects {Selects()}");
            Press(window, Named<Button>(root, "Go to last page"));
            steps.Add($"{Page()} {Named<Button>(root, "Go to next page").IsEffectivelyEnabled} {Named<Button>(root, "Go to last page").IsEffectivelyEnabled}");
            Press(window, Named<Button>(root, "Go to first page"));
            model.Sections.PageSize = 20;
            Windows.Settle();
            steps.Add(Page());
            model.Sections.PageSize = 10;
            Windows.Settle();
            var boxes = All<CheckBox>(root).Where(box => Avalonia.Automation.AutomationProperties.GetName(box) == "Select row").Take(2).ToList();
            boxes.ForEach(box => Press(window, box));
            steps.Add(All<TextBlock>(root).First(block => block.Text?.EndsWith("row(s) selected.", StringComparison.Ordinal) == true).Text!);
            model.Sections.Columns[4].IsVisible = false;
            Windows.Settle();
            steps.Add($"status {All<TextBlock>(root).Any(block => block.IsEffectivelyVisible && block.Text == "In Process")}");
            var unassigned = model.Sections.Rows.First(row => !row.IsAssigned);
            var select = All<ComboBox>(root).First(box => box.IsEffectivelyVisible && box.PlaceholderText == "Assign reviewer");
            select.SelectedItem = "Eddie Lake";
            Windows.Settle();
            steps.Add($"{unassigned.Header} {unassigned.Assignee} {unassigned.IsAssigned} {select.IsEffectivelyVisible && Equals(select.SelectedItem, "Eddie Lake")}");
            var first = model.Sections.Rows[0];
            model.Move.Execute(("1", "sections", 3));
            Windows.Settle();
            steps.Add($"moved {model.Sections.Rows.IndexOf(first)} {model.Sections.Rows[0].Header}");
            var header = All<Button>(root).First(button => button.Classes.Contains("link") && button.Content as string == model.Sections.Rows[0].Header);
            Press(window, header);
            var surface = window.GetVisualDescendants().OfType<Aioros.Cosmo.Overlays.DialogSurface>().Single();
            var target = surface.GetVisualDescendants().OfType<TextBox>().First(box => Avalonia.Automation.AutomationProperties.GetName(box) == "Target");
            steps.Add($"viewer {Windows.Box(surface, root)} {surface.GetVisualDescendants().OfType<ContentPresenter>().Any(presenter => presenter.Content as string == model.Sections.Rows[0].Header)} {target.Text == model.Sections.Rows[0].Target}");
            return steps;
        });

        Assert.Equal(["10 of 68 Page 1 of 7 In Process:1 Done:1 selects 3", "Page 7 of 7 False False", "Page 1 of 4", "2 of 68 row(s) selected.", "status False", "Innovation and Advantages Eddie Lake False True", "moved 3 Table of contents", "viewer 896, 0, 384, 800 True True"], found);
    }
}
