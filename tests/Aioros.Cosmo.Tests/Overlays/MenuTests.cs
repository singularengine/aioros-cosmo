using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Path = Avalonia.Controls.Shapes.Path;

namespace Aioros.Cosmo.Tests.Overlays;

public sealed class MenuTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static readonly string[] Letters = ["s", "s", "i"];

    internal static MenuFlyoutPresenter Presenter(Window window) => window.GetVisualDescendants().OfType<MenuFlyoutPresenter>().First(presenter => presenter.IsEffectivelyVisible);

    /// <summary>The differences between a menu's rows (items, labels, separators) and shadcn/ui's, relative to the menu's surface.</summary>
    internal static IEnumerable<string> Rows(string what, IReadOnlyList<Control> rows, Border surface, IReadOnlyList<ElementMetrics> expected, ElementMetrics content)
    {
        if (rows.Count != expected.Count)
        {
            yield return $"{what}: {rows.Count} rows, shadcn {expected.Count}";
            yield break;
        }

        for (var i = 0; i < rows.Count; i++)
        {
            var box = Windows.Box(rows[i], surface);
            var target = new Rect(expected[i].X - content.X, expected[i].Y - content.Y, expected[i].Width, expected[i].Height);
            if (Math.Abs(box.X - target.X) > 0.5 || Math.Abs(box.Width - target.Width) > 0.5 || Math.Abs(box.Y - target.Y) > 1 || Math.Abs(box.Height - target.Height) > 1)
            {
                yield return $"{what} #{i} {expected[i].Text}: {box}, shadcn {target}";
            }

            if (rows[i] is not MenuItem item)
            {
                continue;
            }

            var root = Windows.Part<Border>(item, "PART_LayoutRoot");
            var look = (item.Padding.Left, item.Padding.Top, root.CornerRadius.TopLeft, item.FontSize, (int)item.FontWeight, item.Opacity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            var shadcn = (PopoverTests.Px(expected[i].PaddingLeft!), PopoverTests.Px(expected[i].PaddingTop!), PopoverTests.Px(expected[i].Radius), PopoverTests.Px(expected[i].FontSize), int.Parse(expected[i].FontWeight, System.Globalization.CultureInfo.InvariantCulture), expected[i].Opacity);
            if (look != shadcn)
            {
                yield return $"{what} #{i} {expected[i].Text}: {look}, shadcn {shadcn}";
            }
        }
    }

    internal static List<Control> RowsOf(ItemsControl menu) => [.. menu.GetRealizedContainers().Select(container => container is Avalonia.Controls.Presenters.ContentPresenter { Child: { } child } ? child : container).OfType<Control>()];

    [Fact]
    [Trait("AC", "DROPDOWN-01")]
    public async Task TheOpenMenuIsShadcnsDropdownMenuContentAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("dropdown-menu-open", name);
            var content = reference.Single(element => element.Slot == "dropdown-menu-content");
            var rows = reference.Where(element => element.Slot is "dropdown-menu-label" or "dropdown-menu-item" or "dropdown-menu-sub-trigger" or "dropdown-menu-separator").ToList();
            var shortcut = reference.First(element => element.Slot == "dropdown-menu-shortcut");
            var tokens = await ReferenceMetrics.TokensAsync("neutral", name);
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.DropdownMenuOpen(), variant);
                var presenter = Presenter(window);
                var surface = Windows.Part<Border>(presenter, "LayoutRoot");
                var errors = PopoverTests.Surface($"{name} menu", surface, presenter.Padding, window, content).ToList();
                var items = RowsOf(presenter);
                errors.AddRange(Rows(name, items, surface, rows, content));
                var gesture = Windows.Part<TextBlock>(items[1], "PART_InputGestureText");
                if (gesture.FontSize != 12 || !Windows.Paint(gesture.Foreground).Near(CssColor.Parse(shortcut.Color)))
                {
                    errors.Add($"{name}: shortcut {gesture.FontSize} {Windows.Paint(gesture.Foreground)}, shadcn 12 {shortcut.Color}");
                }

                var chevron = Windows.Part<Path>(items[7], "PART_ChevronPath");
                if (!chevron.IsVisible || chevron.Bounds.Width != 16 || !Windows.Paint(chevron.Stroke).Near(CssColor.Parse(shortcut.Color)) || Windows.Part<Path>(items[6], "PART_ChevronPath").IsVisible)
                {
                    errors.Add($"{name}: chevron {chevron.IsVisible} {chevron.Bounds.Width} {Windows.Paint(chevron.Stroke)}");
                }

                Windows.Hover(window, items[1]);
                var highlighted = (Windows.Paint(Windows.Part<Border>(items[1], "PART_LayoutRoot").Background), Windows.Paint(((MenuItem)items[1]).Foreground));
                if (!highlighted.Item1.Near(CssColor.Parse(tokens["accent"].Css)) || !highlighted.Item2.Near(CssColor.Parse(tokens["accent-foreground"].Css)))
                {
                    errors.Add($"{name}: highlighted {highlighted}, shadcn {tokens["accent"].Css} {tokens["accent-foreground"].Css}");
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "DROPDOWN-02")]
    public async Task CheckRadioInsetAndDestructiveItemsAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("dropdown-menu-checkboxes", name);
            var content = reference.Single(element => element.Slot == "dropdown-menu-content");
            var rows = reference.Where(element => element.Slot is "dropdown-menu-label" or "dropdown-menu-checkbox-item" or "dropdown-menu-separator").ToList();
            var tokens = await ReferenceMetrics.TokensAsync("neutral", name);
            var destructive = CssColor.Parse(tokens["destructive"].Css);
            var faded = destructive with { A = name == "light" ? 0.1 : 0.2 };
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.DropdownMenuCheckboxes(), variant);
                var presenter = Presenter(window);
                var surface = Windows.Part<Border>(presenter, "LayoutRoot");
                var items = RowsOf(presenter);
                var errors = Rows($"{name} checkboxes", items, surface, rows, content).ToList();
                var check = Windows.Part<Path>(items[2], "PART_Check");
                var center = check.TranslatePoint(new Point(8, 8), items[2]) ?? default;
                if (!check.IsVisible || check.Bounds.Width != 16 || center.X != 15 || Windows.Part<Path>(items[4], "PART_Check").IsVisible)
                {
                    errors.Add($"{name}: check {check.IsVisible} {check.Bounds.Width} at {center}");
                }

                window.Close();
                errors.AddRange(ContextItems(name, variant, destructive, faded));
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static List<string> ContextItems(string name, ThemeVariant variant, CssColor destructive, CssColor faded)
    {
        var errors = new List<string>();
        var context = OverlayFixtures.Show(OverlayFixtures.ContextMenuOpen(), variant);
        var menu = context.GetVisualDescendants().OfType<ContextMenu>().First();
        var entries = RowsOf(menu);
        var dot = Windows.Part<Ellipse>(entries[9], "PART_Dot");
        var dotCenter = dot.TranslatePoint(new Point(4, 4), entries[9]) ?? default;
        if (!dot.IsVisible || dot.Bounds.Width != 8 || dotCenter.X != 15 || Windows.Part<Ellipse>(entries[10], "PART_Dot").IsVisible || ((MenuItem)entries[8]).Padding.Left != 32 || ((MenuItem)entries[0]).Padding.Left != 32)
        {
            errors.Add($"{name}: dot {dot.IsVisible} {dot.Bounds.Width} at {dotCenter}; inset label {((MenuItem)entries[8]).Padding.Left}");
        }

        var tools = (MenuItem)entries[3];
        Windows.Hover(context, tools);
        tools.IsSubMenuOpen = true;
        Windows.Settle();
        var sub = tools.GetLogicalChildren().OfType<MenuItem>().Last();
        var popup = Windows.Part<Avalonia.Controls.Primitives.Popup>(tools, "PART_Popup");
        var subSurface = (Border)popup.Child!;
        if (Windows.Box(subSurface, context).X < Windows.Box(tools, context).Right || !CssShadow.Same(Windows.Shadows(subSurface.BoxShadow), CssShadow.List("rgba(0, 0, 0, 0.1) 0px 10px 15px -3px, rgba(0, 0, 0, 0.1) 0px 4px 6px -4px")))
        {
            errors.Add($"{name}: submenu at {Windows.Box(subSurface, context)} beside {Windows.Box(tools, context)}, shadow {string.Join(", ", Windows.Shadows(subSurface.BoxShadow))}");
        }

        if (!Windows.Paint(sub.Foreground).Near(destructive))
        {
            errors.Add($"{name}: destructive {Windows.Paint(sub.Foreground)}, shadcn {destructive}");
        }

        Windows.Hover(context, sub);
        var hovered = (Windows.Paint(Windows.Part<Border>(sub, "PART_LayoutRoot").Background), Windows.Paint(sub.Foreground));
        if (!hovered.Item1.Near(faded) || !hovered.Item2.Near(destructive))
        {
            errors.Add($"{name}: destructive highlighted {hovered}, shadcn {faded} {destructive}");
        }

        context.Close();
        return errors;
    }

    [Fact]
    [Trait("AC", "DROPDOWN-03")]
    public async Task TheKeyboardMovesTypesAheadAndOpensSubmenusAsRadixsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var fixture = OverlayFixtures.DropdownMenuOpen();
            var window = Windows.Fixture(fixture.Content, fixture.Height, ThemeVariant.Light);
            var trigger = (Button)fixture.Content;
            string Focused() => Windows.Focused(window) is MenuItem item ? $"{item.Header}" : Windows.Focused(window)?.GetType().Name ?? "nothing";
            var steps = new List<string>();
            trigger.Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Enter);
            steps.Add($"return {Focused()}");
            foreach (var (key, label) in new[] { (Key.Down, "down"), (Key.Up, "up"), (Key.Up, "up"), (Key.End, "end"), (Key.Home, "home") })
            {
                Windows.Key(window, key);
                steps.Add($"{label} {Focused()}");
            }

            foreach (var letter in Letters)
            {
                Windows.Type(window, letter);
                steps.Add($"{letter} {Focused()}");
            }

            Windows.Key(window, Key.Right);
            steps.Add($"right {Focused()}");
            Windows.Key(window, Key.Left);
            steps.Add($"left {Focused()}");
            Windows.Key(window, Key.Escape);
            steps.Add($"escape {trigger.Flyout!.IsOpen} {Focused()}");
            Windows.Key(window, Key.Down);
            steps.Add($"down {trigger.Flyout.IsOpen} {Focused()}");
            window.Close();
            return steps;
        });

        Assert.Equal(["return Profile", "down Billing", "up Profile", "up Profile", "end Log out", "home Profile", "s Settings", "s Support", "i Invite users", "right Email", "left Invite users", "escape False Button", "down True Profile"], found);
    }

    [Fact]
    [Trait("AC", "DROPDOWN-04")]
    public async Task MenuItemsBindTheModelAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new OverlaysModel();
            var view = new OverlaysView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var steps = new List<string>();
            Windows.Click(window, view.FindControl<Button>("RecentTrigger")!);
            steps.Add(string.Join(",", Presenter(window).GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text).Where(text => text?.StartsWith("Open", StringComparison.Ordinal) == true)));
            Windows.Key(window, Key.Escape);
            var trigger = view.FindControl<Button>("MenuTrigger")!;
            Windows.Click(window, trigger);
            steps.Add($"open {model.MenuOpen} delete enabled {view.FindControl<MenuItem>("DeleteItem")!.IsEffectivelyEnabled}");
            Windows.Click(window, view.FindControl<MenuItem>("StatusItem")!);
            steps.Add($"status {model.StatusBar}");
            Windows.Click(window, trigger);
            Windows.Click(window, view.FindControl<MenuItem>("BottomItem")!);
            steps.Add($"radio {view.FindControl<MenuItem>("TopItem")!.IsChecked} {view.FindControl<MenuItem>("BottomItem")!.IsChecked}");
            Windows.Click(window, trigger);
            Windows.Click(window, view.FindControl<MenuItem>("ProfileItem")!);
            steps.Add($"ran {string.Join(",", model.Ran)} open {trigger.Flyout!.IsOpen} {model.MenuOpen}");
            window.Close();
            return steps;
        });

        Assert.Equal(["Open notes.md,Open plan.xlsx", "open True delete enabled False", "status False", "radio False True", "ran profile open False False"], found);
    }
}
