using Aioros.Cosmo.Navigation;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Composites;

public sealed class SidebarTests(DesktopSession desktop)
{
    [Fact]
    [Trait("AC", "SIDEBAR-02")]
    public async Task TheSidebarItsMenusAndTheInsetAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["sidebar-demo", "sidebar-floating", "sidebar-inset"])
        {
            wrong.AddRange(await Display.Looks.FixtureAsync(desktop, fixture, (window, root, reference, theme) => Check(window, root, reference, $"{theme} {fixture}", fixture)));
        }

        Assert.Empty(wrong);
    }

    private static List<string> Check(Window window, Control root, IReadOnlyList<ElementMetrics> reference, string what, string fixture)
    {
        ElementMetrics Slot(string slot, int index = 0) => reference.Where(element => element.Slot == slot).ElementAt(index);
        ElementMetrics Inside(ElementMetrics element) => element with { X = element.X + 8, Y = element.Y + 8, Width = element.Width - 16, Height = element.Height - 16 };
        var sidebar = All<Sidebar>(root).Single();
        var inner = Windows.Part<Border>(sidebar, "PART_Inner");
        var errors = Near($"{what} inner", Windows.Box(inner, root), Slot("sidebar-inner"), 1.5).ToList();
        errors.AddRange(Near($"{what} header", Windows.Box(All<SidebarHeader>(root).Single(), root), Inside(Slot("sidebar-header")), 1.5));
        errors.AddRange(Near($"{what} footer", Windows.Box(All<SidebarFooter>(root).Single(), root), Inside(Slot("sidebar-footer")), 1.5));
        errors.AddRange(Near($"{what} content", Windows.Box(All<SidebarContent>(root).Single(), root), Slot("sidebar-content"), 1.5));
        errors.AddRange(All<SidebarGroup>(root).Select((group, i) => (group, i)).SelectMany(pair => Near($"{what} group {pair.i}", Windows.Box(pair.group, root), Inside(Slot("sidebar-group", pair.i)), 1.5)));
        errors.AddRange(Pairs($"{what} label", All<SidebarGroupLabel>(root), reference, "sidebar-group-label", root));
        errors.AddRange(Pairs($"{what} button", All<SidebarMenuButton>(root), reference, "sidebar-menu-button", root));
        errors.AddRange(Pairs($"{what} sub", All<SidebarMenuSub>(root), reference, "sidebar-menu-sub", root));
        errors.AddRange(Pairs($"{what} sub button", All<SidebarMenuSubButton>(root), reference, "sidebar-menu-sub-button", root));
        errors.AddRange(Near($"{what} rail", Windows.Box(All<SidebarRail>(root).Single(), root), Slot("sidebar-rail"), 1.5));
        errors.AddRange(Near($"{what} inset", Windows.Box(All<SidebarInset>(root).Single(), root), Slot("sidebar-inset"), 1.5));
        errors.AddRange(Near($"{what} trigger", Windows.Box(All<SidebarTrigger>(root).Single(), root), Slot("sidebar-trigger"), 1.5));
        errors.AddRange(Near($"{what} separator", Windows.Box(All<Separator>(root).Single(), root), Slot("separator"), 1.5));
        errors.AddRange(Paints($"{what} column", inner.Background, Slot("sidebar-inner").Background));
        errors.AddRange(Appearance(window, root, what, fixture, inner));
        return errors;
    }

    private static List<string> Appearance(Window window, Control root, string what, string fixture, Border inner)
    {
        var errors = new List<string>();
        var expected = fixture switch
        {
            "sidebar-floating" => (new Thickness(1), 10d, true),
            "sidebar-inset" => (new Thickness(0), 0d, false),
            _ => (new Thickness(0, 0, 1, 0), 0d, false),
        };
        errors.AddRange(inner.BorderThickness == expected.Item1 && inner.CornerRadius.TopLeft == expected.Item2 && (Windows.Shadows(inner.BoxShadow).Count > 0) == expected.Item3 ? [] : [$"{what}: column {inner.BorderThickness} {inner.CornerRadius} {inner.BoxShadow}"]);
        if (fixture == "sidebar-inset")
        {
            var inset = All<SidebarInset>(root).Single();
            var card = Windows.Part<Border>(inset, "PART_Root");
            errors.AddRange(card.CornerRadius.TopLeft == 14 && Windows.Shadows(card.BoxShadow).Count > 0 ? [] : [$"{what}: inset card {card.CornerRadius}"]);
            errors.AddRange(Paints($"{what} provider", All<SidebarProvider>(root).Single().Background, window, "CosmoSidebarBrush"));
        }

        var label = All<SidebarGroupLabel>(root)[0];
        errors.AddRange(label.FontSize == 12 && label.FontWeight == FontWeight.Medium && label.Opacity == 0.7 ? [] : [$"{what}: label {label.FontSize} {label.FontWeight} {label.Opacity}"]);
        var buttons = All<SidebarMenuButton>(root);
        var active = buttons.Single(button => button.IsActive);
        errors.AddRange(Paints($"{what} active", Windows.Part<Border>(active, "PART_Border").Background, window, "CosmoSidebarAccentBrush")
            .Concat(Paints($"{what} active text", active.Foreground, window, "CosmoSidebarAccentForegroundBrush")));
        var chevrons = buttons.Select(button => Windows.Part<Viewbox>(button, "PART_Chevron")).Where(chevron => chevron.IsVisible).Select(chevron => chevron.RenderTransform is TransformOperations operations ? Math.Round(Math.Atan2(operations.Value.M12, operations.Value.M11) * 180 / Math.PI) : 0).ToList();
        errors.AddRange(active.FontWeight == FontWeight.Medium && chevrons.SequenceEqual([90d, 0, 0, 0]) ? [] : [$"{what}: active {active.FontWeight}, chevrons {string.Join(',', chevrons)}"]);
        Windows.Hover(window, buttons[2]);
        errors.AddRange(Paints($"{what} hovered", Windows.Part<Border>(buttons[2], "PART_Border").Background, window, "CosmoSidebarAccentBrush"));
        return errors;
    }

    private static IEnumerable<string> Pairs(string what, IReadOnlyList<Control> found, IReadOnlyList<ElementMetrics> reference, string slot, Control root)
    {
        var shown = found.Where(control => control.IsEffectivelyVisible).ToList();
        var expected = reference.Where(element => element.Slot == slot && element.Width > 0).ToList();
        return shown.Count != expected.Count
            ? [$"{what}: {shown.Count}, shadcn {expected.Count}"]
            : shown.Zip(expected).SelectMany((pair, i) => Near($"{what} {i}", Windows.Box(pair.First, root), pair.Second, 1.5));
    }

    [Fact]
    [Trait("AC", "SIDEBAR-03")]
    public async Task TheIconSidebarShowsItsIconsAndTipsAsync()
    {
        var wrong = await Display.Looks.FixtureAsync(desktop, "sidebar-icon", (window, root, reference, theme) =>
        {
            var sidebar = All<Sidebar>(root).Single();
            var errors = sidebar.Bounds.Width == 48 && All<SidebarHeader>(root).Single().Bounds.Height == 32 && All<SidebarFooter>(root).Single().Bounds.Height == 32 ? new List<string>() : [$"{theme}: sidebar {sidebar.Bounds}"];
            errors.AddRange(Pairs($"{theme} button", All<SidebarMenuButton>(root), reference, "sidebar-menu-button", root));
            errors.AddRange(All<SidebarMenuButton>(root).Where(button => button.IsEffectivelyVisible).Where(button => button.Bounds.Size != new Size(32, 32)).Select(button => $"{theme}: button {button.Bounds}"));
            errors.AddRange(All<SidebarGroupLabel>(root).Where(label => label.IsEffectivelyVisible).Where(label => label.Opacity != 0 || label.Margin.Top != -32).Select(label => $"{theme}: label {label.Opacity} {label.Margin}"));
            errors.AddRange(All<SidebarMenuSub>(root).Any(sub => sub.IsEffectivelyVisible) || All<SidebarGroup>(root)[1].IsEffectivelyVisible ? [$"{theme}: sub-menu or Projects shown"] : []);
            var models = All<SidebarMenuButton>(root).Single(button => button.Tooltip == "Models");
            errors.AddRange((string?)ToolTip.GetTip(models) == "Models" && ToolTip.GetPlacement(models) == PlacementMode.Right ? [] : [$"{theme}: tip {ToolTip.GetTip(models)} {ToolTip.GetPlacement(models)}"]);
            All<SidebarProvider>(root).Single().IsOpen = true;
            Windows.Settle();
            errors.AddRange(ToolTip.GetTip(models) is null ? [] : [$"{theme}: tip while expanded"]);
            return errors;
        });

        Assert.Empty(wrong);
    }

    private static (SidebarProvider Provider, Sidebar Sidebar, SidebarInset Inset, TextBox Box) Build(SidebarCollapsible collapsible = SidebarCollapsible.Icon, SidebarSide side = SidebarSide.Left)
    {
        var models = new SidebarMenuItem { IsCollapsible = true, Children = { new SidebarMenuButton { Content = "Models", Tooltip = "Models" }, new SidebarMenuSub { Items = { new SidebarMenuSubButton { Content = "Genesis" } } } } };
        var sidebar = new Sidebar
        {
            Collapsible = collapsible,
            Side = side,
            Items =
            {
                new SidebarContent { Items = { new SidebarGroup { Children = { new SidebarMenu { Items = { models } } } } } },
                new SidebarRail(),
            },
        };
        var box = new TextBox();
        var inset = new SidebarInset { Content = new StackPanel { Children = { new SidebarTrigger(), box } } };
        return (new SidebarProvider { Width = 800, Height = 400, Children = { sidebar, inset } }, sidebar, inset, box);
    }

    [Fact]
    [Trait("AC", "SIDEBAR-04")]
    public async Task TheTriggerRailKeysAndItemsToggleAsShadcnsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var (provider, sidebar, inset, box) = Build();
            var window = Windows.Show(provider, ThemeVariant.Light);
            var transition = (sidebar.GetBaseValue(Animatable.TransitionsProperty).GetValueOrDefault() as Transitions)?.OfType<DoubleTransition>().SingleOrDefault(each => each.Property == Avalonia.Layout.Layoutable.WidthProperty)?.Duration;
            Windows.Click(window, provider.GetVisualDescendants().OfType<SidebarTrigger>().Single());
            var collapsed = (provider.IsOpen, sidebar.Bounds.Width, inset.Bounds.Width);
            Windows.Click(window, provider.GetVisualDescendants().OfType<SidebarRail>().Single());
            var railed = provider.IsOpen;
            box.Focus();
            window.KeyPress(Key.B, RawInputModifiers.Control, PhysicalKey.None, null);
            Windows.Settle();
            var control = provider.IsOpen;
            window.KeyPress(Key.B, RawInputModifiers.Meta, PhysicalKey.None, null);
            Windows.Settle();
            var meta = provider.IsOpen;
            var item = provider.GetVisualDescendants().OfType<SidebarMenuItem>().Single();
            var button = item.Children.OfType<SidebarMenuButton>().Single();
            var sub = item.Children.OfType<SidebarMenuSub>().Single();
            Windows.Click(window, button);
            var opened = (item.IsExpanded, sub.IsVisible, (Windows.Part<Viewbox>(button, "PART_Chevron").RenderTransform as TransformOperations)?.Value.M12 > 0.99);
            Windows.Click(window, button);
            var closed = (item.IsExpanded, sub.IsVisible);
            item.IsExpanded = true;
            Windows.Settle();
            var coded = sub.IsVisible;
            window.Close();
            return (transition, collapsed, railed, control, meta, opened, closed, coded);
        });
        var variants = await desktop.RunAsync(() =>
        {
            var offcanvas = Build(SidebarCollapsible.Offcanvas);
            var window = Windows.Show(offcanvas.Provider, ThemeVariant.Light);
            offcanvas.Provider.IsOpen = false;
            Windows.Settle();
            var slid = (offcanvas.Sidebar.Bounds.Width, Windows.Box(Windows.Part<Border>(offcanvas.Sidebar, "PART_Container"), offcanvas.Provider).Right <= 0);
            window.Close();
            var fixedOne = Build(SidebarCollapsible.None);
            window = Windows.Show(fixedOne.Provider, ThemeVariant.Light);
            fixedOne.Provider.IsOpen = false;
            Windows.Settle();
            var kept = fixedOne.Sidebar.Bounds.Width;
            window.Close();
            var right = Build(side: SidebarSide.Right);
            window = Windows.Show(right.Provider, ThemeVariant.Light);
            var end = (Windows.Box(right.Sidebar, right.Provider).Right, Windows.Part<Border>(right.Sidebar, "PART_Inner").BorderThickness);
            window.Close();
            return (slid, kept, end);
        });

        Assert.Equal(((TimeSpan?)TimeSpan.FromMilliseconds(200), (false, 48d, 752d), true, false, true, (true, true, true), (false, false), true), found);
        Assert.Equal(((0d, true), 256d, (800d, new Thickness(1, 0, 0, 0))), variants);
    }

    [Fact]
    [Trait("AC", "SIDEBAR-05")]
    public async Task UnderTheBreakpointTheSidebarIsASheetAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var (provider, sidebar, inset, _) = Build();
            provider.Width = double.NaN;
            provider.Height = double.NaN;
            var window = Windows.Show(provider, ThemeVariant.Light);
            window.Width = 390;
            window.Height = 600;
            Windows.Settle();
            Windows.Settle();
            var mobile = (provider.IsMobile, inset.Bounds.Width);
            Windows.Click(window, provider.GetVisualDescendants().OfType<SidebarTrigger>().Single());
            var container = Windows.Part<Border>(sidebar, "PART_Container");
            var mask = Windows.Part<Border>(sidebar, "PART_Mask");
            var sheet = (provider.IsMobileOpen, provider.IsOpen, container.Bounds.Width, Windows.Box(container, provider).X, mask.IsVisible, Math.Round(Windows.Paint(mask.Background).A, 2), Windows.Shadows(Windows.Part<Border>(sidebar, "PART_Inner").BoxShadow).Count > 0);
            Windows.Key(window, Key.Escape);
            var escaped = provider.IsMobileOpen;
            Windows.Click(window, provider.GetVisualDescendants().OfType<SidebarTrigger>().Single());
            window.MouseDown(new Point(380, 300), MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(new Point(380, 300), MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            var masked = provider.IsMobileOpen;
            window.Width = 1000;
            Windows.Settle();
            Windows.Settle();
            var wide = (provider.IsMobile, provider.IsMobileOpen, sidebar.Bounds.Width, Windows.Box(inset, provider).X);
            window.Close();
            return (mobile, sheet, escaped, masked, wide);
        });

        Assert.Equal(((true, 390d), (true, true, 288d, 0d, true, 0.5, true), false, false, (false, false, 256d, 256d)), found);
    }

    [Fact]
    [Trait("AC", "SIDEBAR-06")]
    public async Task TheBoundStateNavigationAndStoreFollowTheModelAsync()
    {
        var file = Path.Combine(Path.GetTempPath(), $"cosmo-sidebar-{Guid.NewGuid():N}.txt");
        var model = new SidebarModel { Store = new SidebarFileStore(file) };
        var first = await desktop.RunAsync(() =>
        {
            var view = new SidebarView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var provider = view.GetVisualDescendants().OfType<SidebarProvider>().Single();
            Windows.Click(window, view.GetVisualDescendants().OfType<SidebarTrigger>().Single());
            var state = model.Open;
            model.Open = true;
            Windows.Settle();
            var expanded = provider.Sidebar!.Bounds.Width;
            var buttons = view.GetVisualDescendants().OfType<SidebarMenuButton>().ToList();
            Windows.Click(window, buttons[1]);
            var active = string.Join(',', buttons.Select(button => button.IsActive));
            window.Close();
            return (state, expanded, model.Current, active, provider.Persisting);
        });
        await first.Persisting;
        var written = await File.ReadAllTextAsync(file, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(file, "sidebar_state=False", TestContext.Current.CancellationToken);
        var restored = new SidebarModel { Store = new SidebarFileStore(file) };
        var second = await desktop.RunAsync(() =>
        {
            var view = new SidebarView { DataContext = restored };
            var window = Windows.Show(view, ThemeVariant.Light);
            return (window, view.GetVisualDescendants().OfType<SidebarProvider>().Single());
        });
        await second.Item2.Persisting;
        var collapsed = await desktop.RunAsync(() =>
        {
            Windows.Settle();
            var state = (restored.Open, second.Item2.IsOpen);
            second.window.Close();
            return state;
        });
        File.Delete(file);

        Assert.Equal((false, 256d, "/models", "False,True,False", "sidebar_state=True", (false, false)), (first.state, first.expanded, first.Current, first.active, written.Trim(), collapsed));
    }
}
