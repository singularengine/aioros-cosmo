using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Overlays;

public sealed class ContextMenuTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static ContextMenu Menu(Window window) => window.GetVisualDescendants().OfType<ContextMenu>().First(menu => menu.IsEffectivelyVisible);

    [Fact]
    [Trait("AC", "CONTEXTMENU-01")]
    public async Task TheMenuOpensAtThePointerAsShadcnsContentAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("context-menu-open", name);
            var content = reference.Single(element => element.Slot == "context-menu-content");
            var rows = reference.Where(element => element.Slot is "context-menu-label" or "context-menu-item" or "context-menu-sub-trigger" or "context-menu-separator" or "context-menu-checkbox-item" or "context-menu-radio-item").ToList();
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.ContextMenuOpen(), variant);
                var menu = Menu(window);
                var surface = Windows.Part<Border>(menu, "LayoutRoot");
                var errors = PopoverTests.Surface($"{name} context menu", surface, menu.Padding, window, content).ToList();
                errors.AddRange(MenuTests.Rows(name, MenuTests.RowsOf(menu), surface, rows, content));
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "CONTEXTMENU-02")]
    public async Task TheKeyboardOpensRunsAndClosesTheMenuAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var ran = new List<string>();
            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Header = "Back", Command = new Command(() => ran.Add("back")) });
            menu.Items.Add(new MenuItem { Header = "Reload", Command = new Command(() => ran.Add("reload")) });
            var area = new Border { Width = 300, Height = 150, Focusable = true, Background = Avalonia.Media.Brushes.Transparent, ContextMenu = menu, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
            var window = Windows.Fixture(area, 448, ThemeVariant.Light);
            var steps = new List<string>();
            area.Focus(NavigationMethod.Tab);
            window.KeyPress(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
            window.KeyRelease(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
            Windows.Settle();
            steps.Add($"shift+f10 {menu.IsOpen}");
            Windows.Key(window, Key.Down);
            Windows.Key(window, Key.Down);
            Windows.Key(window, Key.Enter);
            steps.Add($"return {menu.IsOpen} {string.Join(",", ran)}");
            Windows.RightClick(window, area);
            Windows.Key(window, Key.Escape);
            steps.Add($"escape {menu.IsOpen} {ReferenceEquals(Windows.Focused(window), area)}");
            Windows.RightClick(window, area);
            window.MouseDown(new Point(600, 420), MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(new Point(600, 420), MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            steps.Add($"outside {menu.IsOpen}");
            window.Close();
            return steps;
        });

        Assert.Equal(["shift+f10 True", "return False reload", "escape False True", "outside False"], found);
    }

    [Fact]
    [Trait("AC", "CONTEXTMENU-03")]
    public async Task ContextMenuItemsBindTheModelAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new OverlaysModel();
            var view = new OverlaysView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var area = view.FindControl<Border>("ContextArea")!;
            Windows.RightClick(window, area);
            Windows.Click(window, view.FindControl<MenuItem>("CopyItem")!);
            Windows.RightClick(window, area);
            Windows.Click(window, view.FindControl<MenuItem>("UrlsItem")!);
            window.Close();
            return (string.Join(",", model.Ran), model.FullUrls);
        });

        Assert.Equal(("copy", true), found);
    }
}
