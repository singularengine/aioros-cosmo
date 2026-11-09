using Aioros.Cosmo.Display;
using Aioros.Cosmo.Navigation;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Display;

public sealed class StructureTests(DesktopSession desktop)
{
    private Task<List<string>> FixtureAsync(string fixture, Func<Window, Control, IReadOnlyList<ElementMetrics>, string, IEnumerable<string>> check) =>
        Looks.FixtureAsync(desktop, fixture, check);

    private static IEnumerable<string> Is(string what, bool condition, object? found) => condition ? [] : [$"{what}: {found}"];

    [Fact]
    [Trait("AC", "SCROLLAREA-02")]
    public async Task TheScrollBarsShowOverTheAreaAsAThinRoundedThumbAsync()
    {
        var wrong = await FixtureAsync("scroll-area-demo", (window, root, reference, theme) =>
        {
            var area = All<ScrollViewer>(root).Single();
            var bar = Windows.Part<ScrollBar>(area, "PART_VerticalScrollBar");
            var errors = Near($"{theme} area", Windows.Box(area, root), reference.Single(element => element.Slot == "scroll-area"), 0.5).ToList();
            errors.AddRange(Is($"{theme} bar at rest", bar.Opacity == 0, bar.Opacity));
            Windows.Hover(window, area);
            var thumb = bar.GetVisualDescendants().OfType<Thumb>().Single();
            var thumbBox = Windows.Box(thumb, area);
            var expectedLength = area.Viewport.Height / area.Extent.Height * bar.Bounds.Height;
            errors.AddRange(Is($"{theme} bar under the pointer", bar.Opacity == 1, bar.Opacity));
            errors.AddRange(Is($"{theme} bar box", Math.Abs(bar.Bounds.Width - 10) < 0.5 && Math.Abs(Windows.Box(bar, area).Right - (area.Bounds.Width - 1)) < 0.5, Windows.Box(bar, area)));
            errors.AddRange(Is($"{theme} thumb", Math.Abs(thumbBox.Width - 7) < 0.5 && Math.Abs(thumbBox.Height - expectedLength) < 2, thumbBox));
            errors.AddRange(Paints($"{theme} thumb", thumb.Background, window, "CosmoBorderBrush"));
            errors.AddRange(Is($"{theme} thumb corners", thumb.GetVisualDescendants().OfType<Border>().Single().CornerRadius.TopLeft >= 3.5, thumb));
            var grip = Windows.Center(thumb);
            window.MouseDown(grip, MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(grip + new Point(0, 40), RawInputModifiers.LeftMouseButton);
            window.MouseUp(grip + new Point(0, 40), MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            errors.AddRange(Is($"{theme} drag", area.Offset.Y > 200, area.Offset));
            return errors;
        });
        wrong.AddRange(await FixtureAsync("scroll-area-horizontal", (window, root, reference, theme) =>
        {
            var area = All<ScrollViewer>(root).Single();
            var bar = Windows.Part<ScrollBar>(area, "PART_HorizontalScrollBar");
            var errors = Is($"{theme} horizontal at rest", bar.Opacity == 0, bar.Opacity).ToList();
            Windows.Hover(window, area);
            var box = Windows.Box(bar, area);
            errors.AddRange(Is($"{theme} horizontal bar", bar.Opacity == 1 && Math.Abs(box.Height - 10) < 0.5 && Math.Abs(box.Bottom - (area.Bounds.Height - 1)) < 0.5 && area.Extent.Width > area.Viewport.Width, box));
            return errors;
        }));

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "SCROLLAREA-04")]
    public async Task TheBarsHideSixHundredMillisecondsAfterThePointerLeavesAsync() =>
        Assert.Empty(await FixtureAsync("scroll-area-demo", (window, root, reference, theme) =>
        {
            var time = new ManualTime();
            ScrollAreaBars.SetClock(window, time);
            var area = All<ScrollViewer>(root).Single();
            var bar = Windows.Part<ScrollBar>(area, "PART_VerticalScrollBar");
            var shown = new List<double>();
            void Look(TimeSpan advance)
            {
                time.Advance(advance);
                Windows.Settle();
                shown.Add(bar.Opacity);
            }

            Windows.Hover(window, area);
            Look(TimeSpan.Zero);
            window.MouseMove(new Point(620, 10), RawInputModifiers.None);
            Look(TimeSpan.FromMilliseconds(599));
            Look(TimeSpan.FromMilliseconds(1));
            Windows.Hover(window, area);
            Look(TimeSpan.Zero);
            window.MouseMove(new Point(620, 10), RawInputModifiers.None);
            Look(TimeSpan.FromMilliseconds(300));
            Windows.Hover(window, area);
            Look(TimeSpan.FromMilliseconds(600));
            return shown.SequenceEqual([1, 1, 0, 1, 1, 1]) ? [] : [$"{theme}: opacities {string.Join(',', shown)}"];
        }));

    [Fact]
    [Trait("AC", "TABLE-02")]
    public async Task TheTableRowsHeaderFooterAndCaptionAreShadcnsAsync()
    {
        var wrong = await FixtureAsync("table-demo", (window, root, reference, theme) =>
        {
            var headers = All<TableViewColumnHeader>(root);
            var heads = reference.Where(element => element.Tag == "th").ToList();
            var rows = All<TableViewRow>(root);
            var trs = reference.Where(element => element.Tag == "tr").ToList();
            var errors = headers.Zip(heads).SelectMany((pair, i) => Near($"{theme} header {i}", Windows.Box(pair.First, root), pair.Second, 1)
                .Concat(Is($"{theme} header {i} weight", pair.First.FontWeight == FontWeight.Medium, pair.First.FontWeight))
                .Concat(Paints($"{theme} header {i} text", pair.First.Foreground, pair.Second.Color))).ToList();
            var line = Windows.Part<Border>(All<ScrollViewer>(root)[0], "PART_Header");
            errors.AddRange(Is($"{theme} header line", line.BorderThickness == new Thickness(0, 0, 0, 1), line.BorderThickness).Concat(Paints($"{theme} header line", line.BorderBrush, window, "CosmoBorderBrush")));
            errors.AddRange(rows.Zip(trs.Skip(1)).SelectMany((pair, i) => Near($"{theme} row {i}", Windows.Box(pair.First, root), pair.Second, 1)
                .Concat(Is($"{theme} row {i} border", pair.First.BorderThickness == (i == rows.Count - 1 ? new Thickness(0) : new Thickness(0, 0, 0, 1)), pair.First.BorderThickness))));
            var amount = rows[0].GetVisualDescendants().OfType<TableViewCell>().Last().GetVisualDescendants().OfType<TextBlock>().Single();
            errors.AddRange(Is($"{theme} amount end", Math.Abs(Windows.Box(amount, root).Right - (24 + 592 - 8)) < 1, Windows.Box(amount, root)));
            var footer = All<TableFooter>(root).Single();
            var tfoot = trs[^1];
            errors.AddRange(Near($"{theme} footer", Windows.Box(footer, root), tfoot, 1).Concat(Paints($"{theme} footer", footer.Background, reference.Single(element => element.Tag == "tfoot").Background)));
            errors.AddRange(Is($"{theme} footer weight", footer.FontWeight == FontWeight.Medium && footer.BorderThickness == new Thickness(0, 1, 0, 0), footer.FontWeight));
            var caption = All<TableCaption>(root).Single();
            var expected = reference.Single(element => element.Tag == "caption");
            var box = Windows.Box(caption, root);
            errors.AddRange(Is($"{theme} caption", Math.Abs(box.Center.X - (expected.X + expected.Width / 2)) < 1 && Math.Abs(box.Y - expected.Y) < 1, box).Concat(Paints($"{theme} caption", caption.Foreground, expected.Color)));
            Windows.Hover(window, rows[2]);
            errors.AddRange(Paints($"{theme} hovered row", Windows.Part<Border>(rows[2], "Border").Background, window, "CosmoMutedHalfBrush"));
            Windows.Click(window, rows[3]);
            Windows.Away(window, new Point(2, window.Height - 2));
            errors.AddRange(Paints($"{theme} selected row", Windows.Part<Border>(rows[3], "Border").Background, window, "CosmoMutedBrush"));
            return errors;
        });

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "NAVMENU-02")]
    public async Task TheNavigationMenuAndItsOpenContentAreShadcnsAsync()
    {
        var wrong = await FixtureAsync("navigation-menu-open", (window, root, reference, theme) =>
        {
            var items = root.GetVisualDescendants().OfType<Button>().Where(button => button is NavigationMenuTrigger || button.Classes.Contains("navigation-menu-trigger")).ToList();
            var expected = reference.Where(element => element.Slot is "navigation-menu-trigger" || (element.Slot == "navigation-menu-link" && element.Text == "Docs")).ToList();
            var errors = items.Zip(expected).SelectMany((pair, i) => Near($"{theme} item {i}", Windows.Box(pair.First, root), pair.Second, 2.5)
                .Concat(Is($"{theme} item {i} weight", pair.First.FontWeight == FontWeight.Medium, pair.First.FontWeight))).ToList();
            errors.AddRange(Paints($"{theme} trigger", items[0].Background, expected[0].Background));
            var chevron = items[0].GetVisualDescendants().OfType<Viewbox>().Single();
            errors.AddRange(Is($"{theme} chevron", chevron.Bounds.Width == 12, chevron.Bounds));
            var components = (NavigationMenuTrigger)items[1];
            Windows.Click(window, components);
            var presenter = window.GetVisualDescendants().OfType<FlyoutPresenter>().FirstOrDefault(presenter => presenter.IsEffectivelyVisible);
            var content = reference.Single(element => element.Slot == "navigation-menu-content");
            errors.AddRange(presenter is null ? [$"{theme}: no content"] : Near($"{theme} content", Windows.Box(presenter, root), content, 2.5)
                .Concat(Paints($"{theme} content", presenter.Background, content.Background))
                .Concat(Is($"{theme} content look", presenter.Padding == new Thickness(8, 8, 10, 8) && presenter.CornerRadius.TopLeft == 8 && presenter.BorderThickness == new Thickness(1), presenter.Padding)));
            var links = presenter?.GetVisualDescendants().OfType<Button>().ToList() ?? [];
            var expectedLinks = reference.Where(element => element.Slot == "navigation-menu-link" && element.Text != "Docs").ToList();
            errors.AddRange(links.Zip(expectedLinks).SelectMany((pair, i) => Placed($"{theme} link {i}", Windows.Box(pair.First, root), pair.Second, 1.5)));
            Windows.Hover(window, components);
            errors.AddRange(Paints($"{theme} open trigger", components.Background, expected[1].Background));
            errors.AddRange(Is($"{theme} turned chevron", components.GetVisualDescendants().OfType<Viewbox>().Single().RenderTransform?.Value.M11 is < -0.99, components.GetVisualDescendants().OfType<Viewbox>().Single().RenderTransform));
            Windows.Away(window, new Point(2, window.Height - 2));
            errors.AddRange(Paints($"{theme} open trigger away", components.Background, window, "CosmoAccentHalfBrush"));
            if (links.Count > 0)
            {
                Windows.Hover(window, links[0]);
                errors.AddRange(Paints($"{theme} hovered link", links[0].Background, window, "CosmoAccentBrush"));
            }

            return errors;
        });

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "NAVMENU-03")]
    public async Task HoverOpensAfterRadixsDelaySwitchesAtOnceAndClosesOnLeaveAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var time = new ManualTime();
            var window = DisplayFixtures.Show("navigation-menu-demo", ThemeVariant.Light);
            var menu = window.GetVisualDescendants().OfType<NavigationMenu>().Single();
            menu.Time = time;
            var triggers = menu.Children.OfType<NavigationMenuTrigger>().ToList();
            var steps = new List<string?>();
            string? Open() => (menu.OpenTrigger?.Content as string) ?? "none";
            void Pass(TimeSpan span)
            {
                time.Advance(span);
                Windows.Settle();
            }

            Windows.Hover(window, triggers[0]);
            Pass(TimeSpan.FromMilliseconds(199));
            steps.Add(Open());
            Pass(TimeSpan.FromMilliseconds(1));
            steps.Add(Open());
            Windows.Hover(window, triggers[1]);
            Windows.Settle();
            steps.Add(Open());
            steps.Add(triggers[0].Flyout!.IsOpen ? "home open" : "home closed");
            Windows.Away(window, new Point(600, window.Height - 2));
            Pass(TimeSpan.FromMilliseconds(149));
            steps.Add(Open());
            Pass(TimeSpan.FromMilliseconds(1));
            steps.Add(Open());
            triggers[0].Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Right);
            steps.Add((Windows.Focused(window) as ContentControl)?.Content as string);
            Windows.Key(window, Key.Left);
            steps.Add((Windows.Focused(window) as ContentControl)?.Content as string);
            window.Close();
            return steps;
        });

        Assert.Equal(["none", "Home", "Components", "home closed", "Components", "none", "Components", "Home"], found);
    }

    [Fact]
    [Trait("AC", "MENUBAR-02")]
    public async Task TheMenubarAndItsOpenMenuAreShadcnsAsync()
    {
        var wrong = await FixtureAsync("menubar-open", (window, root, reference, theme) =>
        {
            var bar = All<Menu>(root).Single();
            var expectedBar = reference.Single(element => element.Slot == "menubar");
            var frame = bar.GetVisualDescendants().OfType<Border>().First();
            var errors = Near($"{theme} bar", Windows.Box(bar, root), expectedBar, 1).Concat(Paints($"{theme} bar", frame.Background, expectedBar.Background)).ToList();
            errors.AddRange(Is($"{theme} bar look", frame.CornerRadius.TopLeft == 8 && frame.BorderThickness == new Thickness(1) && CssShadow.Same(Windows.Shadows(frame.BoxShadow), CssShadow.List(expectedBar.Shadow)), frame.CornerRadius));
            var items = bar.Items.OfType<MenuItem>().ToList();
            var triggers = reference.Where(element => element.Slot == "menubar-trigger").ToList();
            errors.AddRange(items.Zip(triggers).SelectMany((pair, i) => Near($"{theme} trigger {i}", Windows.Box(pair.First, root), pair.Second, 2.5)
                .Concat(Is($"{theme} trigger {i} weight", pair.First.FontWeight == FontWeight.Medium, pair.First.FontWeight))));
            Windows.Click(window, items[0]);
            Windows.Away(window, new Point(2, window.Height - 2));
            errors.AddRange(Paints($"{theme} open trigger", items[0].Background, triggers[0].Background));
            var popup = Windows.Part<Popup>(items[0], "PART_Popup");
            var surface = (Border)popup.Child!;
            var content = reference.Single(element => element.Slot == "menubar-content");
            errors.AddRange(Near($"{theme} menu", Windows.Box(surface, root), content, 1).Concat(Paints($"{theme} menu", surface.Background, content.Background)));
            errors.AddRange(Is($"{theme} menu look", surface.CornerRadius.TopLeft == 8 && surface.BorderThickness == new Thickness(1) && CssShadow.Same(Windows.Shadows(surface.BoxShadow), CssShadow.List(content.Shadow)), surface.BoxShadow));
            var first = items[0].Items.OfType<MenuItem>().First();
            errors.AddRange(Near($"{theme} first item", Windows.Box(first, root), reference.First(element => element.Slot == "menubar-item"), 1));
            return errors;
        });

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "MENUBAR-03")]
    public async Task TheArrowsMoveBetweenMenusAndEscapeClosesAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var window = DisplayFixtures.Show("menubar-demo", ThemeVariant.Light);
            var items = window.GetVisualDescendants().OfType<Menu>().Single().Items.OfType<MenuItem>().ToList();
            string Open() => string.Join(",", items.Where(item => item.IsSubMenuOpen).Select(item => item.Header));
            var steps = new List<string>();
            Windows.Click(window, items[0]);
            steps.Add(Open());
            Windows.Key(window, Key.Right);
            steps.Add(Open());
            Windows.Key(window, Key.Left);
            steps.Add(Open());
            steps.Add((Windows.Focused(window) as MenuItem)?.Header as string ?? "none");
            Windows.Key(window, Key.Down);
            steps.Add((Windows.Focused(window) as MenuItem)?.Header as string ?? "none");
            Windows.Key(window, Key.Escape);
            steps.Add(Open());
            window.Close();
            return steps;
        });

        Assert.Equal(["File", "Edit", "File", "New Tab", "New Window", ""], found);
    }
}
