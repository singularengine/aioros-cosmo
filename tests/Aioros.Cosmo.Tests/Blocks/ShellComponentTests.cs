using Aioros.Cosmo.Display;
using Aioros.Cosmo.Navigation;
using Aioros.Cosmo.Overlays;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Blocks;

public sealed class ShellComponentTests(DesktopSession desktop)
{
    private static SidebarMenuButton Labelled(string text) => new() { Content = new TextBlock { Text = text } };

    [Fact]
    [Trait("AC", "SIDEBAR-11")]
    public async Task AnActionThatTogglesIsItsItemsTriggerAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var action = new SidebarMenuAction { Toggles = true, Content = new Border { Width = 16, Height = 16 } };
            var button = Labelled("Models");
            var item = new SidebarMenuItem { IsCollapsible = true, Children = { button, action, new SidebarMenuSub { Items = { new SidebarMenuSubButton { Content = "Genesis" } } } } };
            var plain = Labelled("Travel");
            var provider = new SidebarProvider
            {
                MobileBreakpoint = 0,
                Children = { new Sidebar { Collapsible = SidebarCollapsible.Icon, Items = { new SidebarContent { Items = { new SidebarGroup { Children = { new SidebarMenu { Items = { item, new SidebarMenuItem { Children = { plain } } } } } } } } } }, new SidebarInset() },
            };
            var window = Windows.Fixture(provider, 600, ThemeVariant.Light, width: 800, padding: 0);
            var steps = new List<string>();
            Windows.Click(window, action);
            steps.Add($"action {item.IsExpanded} {Math.Round(action.RenderTransform?.Value.M12 ?? 0, 2)}");
            Windows.Click(window, button);
            steps.Add($"button {item.IsExpanded} chevron {Windows.Part<Viewbox>(button, "PART_Chevron").IsVisible}");
            Windows.Click(window, action);
            steps.Add($"again {item.IsExpanded} padding {button.Padding.Right} plain {plain.Padding.Right}");
            provider.IsOpen = false;
            Windows.Settle();
            steps.Add($"icons {button.Padding.Right}");
            window.Close();
            return steps;
        });

        Assert.Equal(["action True 1", "button True chevron False", "again False padding 32 plain 8", "icons 8"], found);
    }

    [Fact]
    [Trait("AC", "SIDEBAR-13")]
    public async Task TheProviderDocksAHeaderAndASecondSidebarAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var header = new Border { Height = 57 };
            DockPanel.SetDock(header, Dock.Top);
            var left = new Sidebar();
            var right = new Sidebar { Side = SidebarSide.Right, Collapsible = SidebarCollapsible.None, Classes = { "bordered" } };
            var plainRight = new Sidebar { Collapsible = SidebarCollapsible.None };
            var inset = new SidebarInset();
            var provider = new SidebarProvider { MobileBreakpoint = 0, Children = { header, left, right, inset } };
            var window = Windows.Fixture(provider, 800, ThemeVariant.Light, width: 1280, padding: 0);
            var root = (Control)window.Content!;
            string Boxes() => string.Join(" | ", ((Control[])[header, left, right, inset]).Select(control => Windows.Box(control, root).ToString()));
            var steps = new List<string> { Boxes(), $"border {Windows.Part<Border>(right, "PART_Inner").BorderThickness}" };
            provider.Toggle();
            Windows.Settle();
            steps.Add(Boxes());
            provider.IsOpen = true;
            provider.MobileBreakpoint = 2000;
            Windows.Settle();
            steps.Add($"mobile {Windows.Box(right, root)}");
            var lone = new SidebarProvider { MobileBreakpoint = 0, Children = { plainRight, new SidebarInset() } };
            window.Close();
            var other = Windows.Fixture(lone, 400, ThemeVariant.Light, width: 800, padding: 0);
            steps.Add($"plain border {Windows.Part<Border>(plainRight, "PART_Inner").BorderThickness}");
            other.Close();
            return steps;
        });

        Assert.Equal(
        [
            "0, 0, 1280, 57 | 0, 57, 256, 743 | 1024, 57, 256, 743 | 256, 57, 768, 743",
            "border 1,0,0,0",
            "0, 0, 1280, 57 | 0, 57, 0, 743 | 1024, 57, 256, 743 | 0, 57, 1024, 743",
            "mobile 1024, 57, 256, 743",
            "plain border 0,0,0,0",
        ], found);
    }

    [Fact]
    [Trait("AC", "SIDEBAR-15")]
    public async Task TheShellsSidebarPartsLookAsShadcnsAsync()
    {
        var found = new List<string>();
        found.AddRange(await FixtureAsync(desktop, "sidebar-01", (window, root, _, theme) =>
        {
            var input = All<SidebarInput>(root).Single();
            var border = Windows.Part<Border>(input, "PART_BorderElement");
            return theme == "light" ? [$"input {input.Bounds.Height} {Windows.Paint(input.Background)} {Windows.Shadows(border.BoxShadow).Count}"] : [];
        }));
        foreach (var (fixture, width) in new[] { ("sidebar-04", 304d), ("sidebar-09", 350d), ("dashboard-01", 288d) })
        {
            found.AddRange(await FixtureAsync(desktop, fixture, (_, root, _, theme) => theme == "light" ? [$"{fixture} {All<Sidebar>(root)[0].ExpandedWidth == width && Math.Abs(All<Sidebar>(root)[0].Bounds.Width - width) < 0.5}"] : []));
        }

        found.AddRange(await FixtureAsync(desktop, "sidebar-02", (window, root, _, theme) =>
        {
            var trigger = Windows.Part<ToggleButton>(All<Expander>(root)[0], "ExpanderHeader");
            var chevron = trigger.GetVisualDescendants().OfType<Viewbox>().Last();
            return theme == "light" ? [$"group {trigger.Bounds.Height} {trigger.FontSize} {trigger.FontWeight} {Math.Round(chevron.RenderTransform?.Value.M12 ?? 0, 2)}"] : [];
        }));
        found.AddRange(await FixtureAsync(desktop, "sidebar-04", (_, root, _, theme) =>
        {
            var sub = All<SidebarMenuSub>(root)[0];
            return theme == "light" ? [$"flat {sub.BorderThickness} {sub.Padding.Left}"] : [];
        }));

        Assert.Equal(["input 32 rgba(255,255,255,1) 0", "sidebar-04 True", "sidebar-09 True", "dashboard-01 True", "group 32 14 Medium 1", "flat 0,0,0,0 6"], found);
    }

    [Fact]
    [Trait("AC", "POPOVER-08")]
    public async Task APopoverBoundOpenBeforeItsFlyoutOpensOnceLoadedAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var button = new Button { Content = "More" };
            Popover.SetIsOpen(button, true);
            button.Flyout = new Flyout { Content = new TextBlock { Text = "Actions" } };
            var window = Windows.Fixture(new Panel { Children = { button } }, 300, ThemeVariant.Light);
            var open = button.Flyout.IsOpen;
            window.Close();
            return open;
        });

        Assert.True(found);
    }

    [Fact]
    [Trait("AC", "CALENDAR-13")]
    public async Task ASidebarCalendarHasWiderDaysAndTodayOnTheSidebarPrimaryAsync()
    {
        var found = await FixtureAsync(desktop, "sidebar-12", (window, root, _, theme) =>
        {
            var days = All<CalendarDayButton>(root).Where(day => day.IsEffectivelyVisible).ToList();
            var chosen = days.Single(day => day.DataContext is DateTime date && date == new DateTime(2026, 10, 16, 0, 0, 0, DateTimeKind.Unspecified));
            var errors = days.All(day => day.Bounds.Width == 33 && day.Bounds.Height == 33) ? new List<string>() : [$"{theme}: days {days[0].Bounds}"];
            errors.AddRange(Math.Abs(Windows.Box(days[1], root).X - Windows.Box(days[0], root).X - 33) < 0.5 ? [] : [$"{theme}: columns"]);
            errors.AddRange(Paints($"{theme} chosen", chosen.Background, window, "CosmoSidebarPrimaryBrush"));
            errors.AddRange(Paints($"{theme} chosen text", chosen.Foreground, window, "CosmoSidebarPrimaryForegroundBrush"));
            return errors;
        });

        Assert.Empty(found);
    }

    [Fact]
    [Trait("AC", "CARD-06")]
    public async Task AFlatCardHasNoShadowAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var flat = new Card { Classes = { "flat" } };
            var plain = new Card();
            var window = Windows.Fixture(new StackPanel { Children = { flat, plain } }, 300, ThemeVariant.Light);
            var shadows = $"{Windows.Shadows(Windows.Part<Border>(flat, "PART_Border").BoxShadow).Count} {Windows.Shadows(Windows.Part<Border>(plain, "PART_Border").BoxShadow).Count}";
            window.Close();
            return shadows;
        });

        Assert.Equal("0 2", found);
    }
}
