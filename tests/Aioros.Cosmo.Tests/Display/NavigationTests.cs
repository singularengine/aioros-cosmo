using Aioros.Cosmo.Navigation;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;
using Path = Avalonia.Controls.Shapes.Path;

namespace Aioros.Cosmo.Tests.Display;

public sealed class NavigationTests(DesktopSession desktop)
{
    private static readonly int[] Steps = [0, 100, 100];

    private Task<List<string>> FixtureAsync(string fixture, Func<Window, Control, IReadOnlyList<ElementMetrics>, string, IEnumerable<string>> check) =>
        Looks.FixtureAsync(desktop, fixture, check);

    [Fact]
    [Trait("AC", "TABS-02")]
    public async Task TheTabStripAndItsTabsAreShadcnsAsync()
    {
        var wrong = await FixtureAsync("tabs-list", (window, root, reference, theme) =>
        {
            var tabs = All<TabControl>(root).Single();
            var strip = Windows.Part<Border>(tabs, "PART_TabsList");
            var list = reference.Single(element => element.Slot == "tabs-list");
            var triggers = reference.Where(element => element.Slot == "tabs-trigger").ToList();
            var items = All<TabItem>(root);
            var errors = Near($"{theme} strip", Windows.Box(strip, root), list, 2.5).Concat(Paints($"{theme} strip", strip.Background, list.Background)).ToList();
            errors.AddRange(strip.CornerRadius.TopLeft == 10 && strip.Padding == new Thickness(3) ? [] : [$"{theme}: strip {strip.CornerRadius} {strip.Padding}"]);
            errors.AddRange(items.Zip(triggers).SelectMany((pair, i) =>
            {
                var border = Windows.Part<Border>(pair.First, "PART_LayoutRoot");
                return Near($"{theme} tab {i}", Windows.Box(pair.First, root), pair.Second, 2.5).Concat(Paints($"{theme} tab {i}", border.Background, pair.Second.Background)).Concat(Paints($"{theme} tab {i} text", pair.First.Foreground, pair.Second.Color))
                    .Concat(border.Padding == new Thickness(8, 4) && border.CornerRadius.TopLeft == 8 ? [] : [$"{theme} tab {i}: {border.Padding} {border.CornerRadius}"]);
            }));
            var selected = Windows.Part<Border>(items[0], "PART_LayoutRoot");
            errors.AddRange(CssShadow.Same(Windows.Shadows(selected.BoxShadow), CssShadow.List(triggers[0].Shadow)) && Windows.Paint(selected.BorderBrush).Near(CssColor.Parse(triggers[0].BorderColor)) ? [] : [$"{theme}: selected {string.Join(',', Windows.Shadows(selected.BoxShadow))} {Windows.Paint(selected.BorderBrush)}"]);
            errors.AddRange(items[3].Opacity == 0.5 ? [] : [$"{theme}: disabled tab {items[3].Opacity}"]);
            Windows.Hover(window, items[1]);
            errors.AddRange(Paints($"{theme} hovered tab", items[1].Foreground, reference.Single(element => element.Slot == "tabs").Color));
            return errors;
        });
        wrong.AddRange(await FixtureAsync("tabs-demo", (window, root, reference, theme) =>
        {
            var tabs = All<TabControl>(root).Single();
            var content = Windows.Part<ContentPresenter>(tabs, "PART_SelectedContentHost");
            var expected = reference.First(element => element.Slot == "tabs-content");
            var box = Windows.Box(content, root);
            return Near($"{theme} content", new Rect(box.X, box.Y, box.Width, expected.Height), expected, 2);
        }));

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TABS-06")]
    public async Task TheLineAndVerticalTabsAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["tabs-line", "tabs-vertical", "tabs-vertical-line"])
        {
            wrong.AddRange(await FixtureAsync(fixture, (_, root, reference, theme) =>
            {
                var tabs = All<TabControl>(root).Single();
                return Strip($"{fixture} {theme}", tabs, root, reference).Concat(Indicators($"{fixture} {theme}", tabs, reference)).Concat(Content($"{fixture} {theme}", tabs, root, reference));
            }));
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Strip(string what, TabControl tabs, Control root, IReadOnlyList<ElementMetrics> reference)
    {
        var strip = Windows.Part<Border>(tabs, "PART_TabsList");
        var list = reference.Single(element => element.Slot == "tabs-list");
        var triggers = reference.Where(element => element.Slot == "tabs-trigger").ToList();
        var items = All<TabItem>(root);
        var vertical = tabs.TabStripPlacement == Dock.Left;
        var gap = tabs.Classes.Contains("line") ? 4 : 0;
        return Near($"{what} strip", Windows.Box(strip, root), list, 2.5).Concat(Paints($"{what} strip", strip.Background, list.Background))
            .Concat(items.Zip(triggers).SelectMany((pair, i) =>
            {
                var border = Windows.Part<Border>(pair.First, "PART_LayoutRoot");
                var box = Windows.Box(pair.First, root);
                var after = i == 0 ? box : Windows.Box(items[i - 1], root);
                var placed = i == 0 || (vertical ? Math.Abs(box.Y - after.Bottom - gap) <= 0.5 && Math.Abs(box.X - pair.Second.X) <= 1.5 : Math.Abs(box.X - after.Right - gap) <= 0.5 && Math.Abs(box.Y - pair.Second.Y) <= 1.5);
                var sized = Math.Abs(box.Width - pair.Second.Width) <= 1.5 && Math.Abs(box.Height - pair.Second.Height) <= 1.5 && (i > 0 || !Near(string.Empty, box, pair.Second, 1.5).Any());
                return (placed && sized ? (string[])[] : [$"{what} tab {i}: {box}, {gap} px after the last, shadcn {pair.Second.X}, {pair.Second.Y}, {pair.Second.Width}, {pair.Second.Height}"])
                    .Concat(Paints($"{what} tab {i}", border.Background, pair.Second.Background)).Concat(Paints($"{what} tab {i} text", pair.First.Foreground, pair.Second.Color))
                    .Concat(CssShadow.Same(Windows.Shadows(border.BoxShadow), CssShadow.List(pair.Second.Shadow)) ? [] : [$"{what} tab {i}: shadow {border.BoxShadow}, shadcn {pair.Second.Shadow}"]);
            }));
    }

    private static IEnumerable<string> Indicators(string what, TabControl tabs, IReadOnlyList<ElementMetrics> reference)
    {
        var line = tabs.Classes.Contains("line");
        var vertical = tabs.TabStripPlacement == Dock.Left;
        var color = CssColor.Parse(reference.Single(element => element.Slot == "tabs").Color);
        return tabs.GetVisualDescendants().OfType<TabItem>().SelectMany((item, i) =>
        {
            var indicator = Windows.Part<Border>(item, "PART_SelectedPipe");
            var box = Windows.Box(indicator, item);
            var expected = vertical ? new Rect(item.Bounds.Width + 2, 0, 2, item.Bounds.Height) : new Rect(0, item.Bounds.Height + 3, item.Bounds.Width, 2);
            var shown = line && item.IsSelected;
            var clipping = indicator.GetVisualAncestors().TakeWhile(ancestor => ancestor != tabs.Parent).Where(ancestor => ancestor.ClipToBounds).Select(ancestor => ancestor.GetType().Name).ToList();
            var right = indicator.Opacity.Equals(shown ? 1 : 0) && (!shown || (box == expected && Windows.Paint(indicator.Background).Near(color) && clipping.Count == 0));
            return right ? (string[])[] : [$"{what} tab {i} line: {indicator.Opacity} {box}, expected {expected} shown {shown}, clipped by {string.Join(", ", clipping)}"];
        });
    }

    private static IEnumerable<string> Content(string what, TabControl tabs, Control root, IReadOnlyList<ElementMetrics> reference)
    {
        if (reference.FirstOrDefault(element => element.Slot == "tabs-content" && element.Width > 0) is not { } content)
        {
            return [];
        }

        var box = Windows.Box(Windows.Part<ContentPresenter>(tabs, "PART_SelectedContentHost"), root);
        return Near($"{what} content", new Rect(box.X, box.Y, box.Width, content.Height), content, 1.5);
    }

    [Fact]
    [Trait("AC", "TABS-07")]
    public async Task ATabIsHoveredAndFocusedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["tabs-list", "tabs-line"])
        {
            foreach (var theme in Themes)
            {
                var hover = await ReferenceMetrics.OfAsync(fixture, theme.Name, "hover");
                var focus = await ReferenceMetrics.OfAsync(fixture, theme.Name, "focus");
                wrong.AddRange(await desktop.RunAsync(() =>
                {
                    var window = DisplayFixtures.Show(fixture, theme.Variant);
                    var items = All<TabItem>((Control)window.Content!);
                    Windows.Hover(window, items[1]);
                    var errors = Paints($"{fixture} {theme.Name} hovered", items[1].Foreground, hover[1].Color).ToList();
                    Windows.Away(window, new Point(2, window.Height - 2));
                    items[0].Focus(NavigationMethod.Tab);
                    Windows.Settle();
                    var border = Windows.Part<Border>(items[0], "PART_LayoutRoot");
                    errors.AddRange(Paints($"{fixture} {theme.Name} focused border", border.BorderBrush, focus[0].BorderColor));
                    errors.AddRange(CssShadow.Same(Windows.Shadows(border.BoxShadow), Ring(focus[0])) ? [] : [$"{fixture} {theme.Name} focused ring {border.BoxShadow}, shadcn {focus[0].Outline} outline and {focus[0].Shadow}"]);
                    window.Close();
                    return errors;
                }));
            }
        }

        Assert.Empty(wrong);
    }

    /// <summary>shadcn/ui's focus ring with its 1 px <c>outline-ring</c> as the shadow it is drawn as, in the ring's color.</summary>
    internal static IReadOnlyList<CssShadow> Ring(ElementMetrics focused)
    {
        var shadows = CssShadow.List(focused.Shadow);
        var color = shadows[0].Color with { A = 1 };
        return focused.Outline is { } outline && outline.StartsWith("solid 1px", StringComparison.Ordinal) ? [new CssShadow(0, 0, 0, 1, color, false), .. shadows] : shadows;
    }

    [Fact]
    [Trait("AC", "TABS-10")]
    public async Task UpAndDownMoveAVerticalStripsTabsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var tabs = new TabControl { TabStripPlacement = Dock.Left, Items = { new TabItem { Header = "One" }, new TabItem { Header = "Two" }, new TabItem { Header = "Three" } }, SelectedIndex = 0 };
            var window = Windows.Show(tabs, ThemeVariant.Light);
            ((TabItem)tabs.Items[0]!).Focus(NavigationMethod.Tab);
            var steps = new List<(int, string?)>();
            foreach (var key in new[] { Key.Down, Key.Down, Key.Down, Key.Up, Key.Right, Key.End })
            {
                Windows.Key(window, key);
                steps.Add((tabs.SelectedIndex, (Windows.Focused(window) as TabItem)?.Header as string));
            }

            window.Close();
            return steps;
        });

        Assert.Equal([(1, "Two"), (2, "Three"), (0, "One"), (2, "Three"), (2, "Three"), (2, "Three")], found);
    }

    [Fact]
    [Trait("AC", "TABS-03")]
    public async Task TheArrowsHomeAndEndMoveAndSelectTabsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var tabs = new TabControl { Items = { new TabItem { Header = "One" }, new TabItem { Header = "Two" }, new TabItem { Header = "Three" }, new TabItem { Header = "Four", IsEnabled = false } }, SelectedIndex = 0 };
            var window = Windows.Show(tabs, ThemeVariant.Light);
            ((TabItem)tabs.Items[0]!).Focus(NavigationMethod.Tab);
            var steps = new List<(int, string?)>();
            foreach (var key in new[] { Key.Right, Key.Left, Key.End, Key.Home, Key.Left })
            {
                Windows.Key(window, key);
                steps.Add((tabs.SelectedIndex, (Windows.Focused(window) as TabItem)?.Header as string));
            }

            window.Close();
            return steps;
        });

        Assert.Equal([(1, "Two"), (0, "One"), (2, "Three"), (0, "One"), (2, "Three")], found);
    }

    [Fact]
    [Trait("AC", "ACCORDION-02")]
    public async Task AccordionItemsAreShadcnsAsync() =>
        Assert.Empty(await FixtureAsync("accordion-demo", (window, root, reference, theme) =>
        {
            var expanders = All<Expander>(root);
            var triggers = reference.Where(element => element.Slot == "accordion-trigger").ToList();
            var headers = expanders.Select(expander => Windows.Part<ToggleButton>(expander, "ExpanderHeader")).ToList();
            var errors = headers.Zip(triggers).SelectMany((pair, i) => Near($"{theme} header {i}", Windows.Box(pair.First, root), pair.Second, 1.5)
                .Concat(Windows.Part<ContentPresenter>(pair.First, "PART_ContentPresenter").FontWeight == FontWeight.Medium ? [] : [$"{theme} header {i}: weight"])).ToList();
            var chevrons = headers.Select(header => Windows.Part<Viewbox>(header, "PART_Chevron")).ToList();
            errors.AddRange(chevrons.Select((chevron, i) => (chevron, i)).Where(pair => pair.chevron.Bounds.Width != 16 || Laid(pair.chevron, root).Right != 616 || Math.Abs(((pair.chevron.RenderTransform as TransformOperations)?.Value.M11 ?? 1) - (pair.i == 0 ? -1 : 1)) > 0.01)
                .Select(pair => $"{theme} chevron {pair.i}: {Windows.Box(pair.chevron, root)} {pair.chevron.RenderTransform}"));
            var paragraphs = expanders[0].Presenter!.GetVisualDescendants().OfType<TextBlock>().ToList();
            errors.AddRange(paragraphs.Zip(reference.Where(element => element.Slot == "p")).SelectMany((pair, i) => Near($"{theme} paragraph {i}", Windows.Box(pair.First, root), pair.Second, 1.5)));
            errors.AddRange(expanders.Select((expander, i) => (expander, i)).Where(pair => pair.expander.BorderThickness.Bottom != (pair.i == 2 ? 0 : 1) || !Windows.Paint(pair.expander.BorderBrush).Near(CssColor.Parse(reference[0].BorderColor))).Select(pair => $"{theme} item {pair.i}: border {pair.expander.BorderThickness}"));
            Windows.Hover(window, headers[1]);
            var decorations = Windows.Part<ContentPresenter>(headers[1], "PART_ContentPresenter").GetVisualDescendants().OfType<TextBlock>().Single().TextDecorations;
            errors.AddRange(decorations is { Count: 1 } && decorations[0].Location == TextDecorationLocation.Underline ? [] : [$"{theme}: no underline under the pointer"]);
            return errors;
        }));

    [Fact]
    [Trait("AC", "ACCORDION-03")]
    public async Task SingleAndMultipleAccordionsKeepTheirItemsOpenAsRadixDoesAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            Accordion Make(bool multiple, bool collapsible) => new() { IsMultiple = multiple, IsCollapsible = collapsible, Items = { new Expander { Header = "A", Content = "a" }, new Expander { Header = "B", Content = "b" }, new Expander { Header = "C", Content = "c" } } };
            var single = Make(false, false);
            var collapsible = Make(false, true);
            var multiple = Make(true, false);
            var panel = new StackPanel { Width = 400, Children = { single, collapsible, multiple } };
            var window = Windows.Show(panel, ThemeVariant.Light);
            List<bool> Open(Accordion accordion) => [.. accordion.Items.OfType<Expander>().Select(item => item.IsExpanded)];
            ToggleButton Header(Accordion accordion, int i) => Windows.Part<ToggleButton>((Expander)accordion.Items[i]!, "ExpanderHeader");
            Windows.Click(window, Header(single, 0));
            Windows.Click(window, Header(single, 1));
            var switched = Open(single);
            Windows.Click(window, Header(single, 1));
            var kept = Open(single);
            Header(collapsible, 2).Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Space);
            Windows.Key(window, Key.Space);
            var closed = Open(collapsible);
            Windows.Click(window, Header(multiple, 0));
            Windows.Click(window, Header(multiple, 2));
            var both = Open(multiple);
            window.Close();
            return (switched, kept, closed, both);
        });

        Assert.Equal([false, true, false], found.switched);
        Assert.Equal([false, true, false], found.kept);
        Assert.Equal([false, false, false], found.closed);
        Assert.Equal([true, false, true], found.both);
    }

    [Fact]
    [Trait("AC", "ACCORDION-04")]
    public async Task TheContentsHeightFollowsTheTransitionsClockAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var time = new ManualTime();
            var transition = new HeightTransition { Time = time };
            var item = new Expander { Header = "Shipping Details", Content = new Border { Height = 80 }, ContentTransition = transition };
            Windows.Holding = false;
            var window = Windows.Show(new StackPanel { Width = 400, Children = { item } }, ThemeVariant.Light);
            var content = item.Presenter!;
            var heights = new List<(double, bool)>();
            item.IsExpanded = true;
            foreach (var step in Steps)
            {
                time.Advance(TimeSpan.FromMilliseconds(step));
                Windows.Settle();
                heights.Add((Math.Round(content.Bounds.Height), content.IsVisible));
            }

            Windows.Settle();
            heights.Add((content.Bounds.Height, content.IsSet(Layoutable.HeightProperty)));
            item.IsExpanded = false;
            foreach (var step in Steps)
            {
                time.Advance(TimeSpan.FromMilliseconds(step));
                Windows.Settle();
                heights.Add((Math.Round(content.Bounds.Height), content.IsVisible));
            }

            Windows.Settle();
            heights.Add((content.Bounds.Height, content.IsVisible));
            Windows.Holding = true;
            window.Close();
            return heights;
        });

        var half = 96 * new Avalonia.Animation.Easings.SplineEasing(0, 0, 0.58, 1).Ease(0.5);
        Assert.Equal([(0.0, true), (Math.Ceiling(half), true), (96.0, true), (96.0, false), (96.0, true), (Math.Ceiling(96 - half), true)], found.Take(6));
        Assert.Equal([false, false], found.Skip(6).Select(step => step.Item2));
    }

    [Fact]
    [Trait("AC", "COLLAPSIBLE-02")]
    public async Task TheCollapsibleIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["collapsible-demo", "collapsible-open"])
        {
            wrong.AddRange(await FixtureAsync(fixture, (window, root, reference, theme) =>
            {
                var expander = All<Expander>(root).Single();
                var toggle = Windows.Part<Border>(expander, "PART_Toggle");
                var errors = Near($"{theme} toggle", Windows.Box(toggle, root), reference.Single(element => element.Slot == "collapsible-trigger"), 1).ToList();
                var rows = All<Border>(root).Where(border => border.Child is TextBlock && border.BorderThickness.Top == 1 && border.IsEffectivelyVisible).ToList();
                var expected = reference.Where(element => element.Tag == "div" && element.Height == 38).ToList();
                errors.AddRange(rows.Count == expected.Count ? rows.Zip(expected).SelectMany((pair, i) => Near($"{theme} row {i}", Windows.Box(pair.First, root), pair.Second, 1)) : [$"{theme}: {rows.Count} rows, shadcn {expected.Count}"]);
                errors.AddRange(Windows.Part<Path>(toggle).Data is not null ? [] : [$"{theme}: no chevrons"]);
                Windows.Hover(window, toggle);
                errors.AddRange(Paints($"{theme} toggle under the pointer", toggle.Background, window, "CosmoButtonGhostHoverBrush"));
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "BREADCRUMB-02")]
    public async Task BreadcrumbsAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["breadcrumb-demo", "breadcrumb-separator"])
        {
            wrong.AddRange(await FixtureAsync(fixture, (window, root, reference, theme) =>
            {
                var breadcrumb = All<Breadcrumb>(root).Single();
                var items = All<BreadcrumbItem>(root);
                var separators = items.Select(item => Windows.Part<ContentPresenter>(item, "PART_Separator")).Where(separator => separator.IsVisible).ToList();
                var errors = separators.Zip(reference.Where(element => element.Slot == "breadcrumb-separator")).SelectMany((pair, i) => Near($"{theme} separator {i}", Windows.Box(pair.First, root), pair.Second, 2)).ToList();
                var parts = items.Select(item => Windows.Part<ContentPresenter>(item, "PART_ContentPresenter").Child!).ToList();
                var expected = reference.Where(element => element.Slot is "breadcrumb-link" or "breadcrumb-page" or "breadcrumb-ellipsis").ToList();
                errors.AddRange(parts.Zip(expected).SelectMany((pair, i) => Crumb($"{theme} item {i}", pair.First, root, pair.Second)));
                errors.AddRange(Windows.Part<ContentPresenter>(items[0], "PART_Separator").IsVisible ? [$"{theme}: the first item has a separator"] : []);
                var link = parts.OfType<HyperlinkButton>().First();
                Windows.Hover(window, link);
                errors.AddRange(Paints($"{theme} link under the pointer", link.Foreground, reference.Single(element => element.Slot == "breadcrumb-page").Color));
                errors.AddRange(AutomationProperties.GetLandmarkType(breadcrumb) == AutomationLandmarkType.Navigation && AutomationProperties.GetName(breadcrumb) == "breadcrumb" ? [] : [$"{theme}: not a navigation landmark"]);
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Crumb(string what, Control part, Control root, ElementMetrics expected)
    {
        var box = Near(what, Windows.Box(part is Button { Content: Control inner } ? inner : part, root), expected, 2);
        return part switch
        {
            HyperlinkButton link => box.Concat(Paints(what, link.Foreground, expected.Color)),
            TextBlock page => box.Concat(Paints(what, page.Foreground, expected.Color)),
            _ => box,
        };
    }

    [Fact]
    [Trait("AC", "PAGINATION-02")]
    public async Task ThePaginationIsShadcnsAsync() =>
        Assert.Empty(await FixtureAsync("pagination-demo", (window, root, reference, theme) =>
        {
            var pagination = All<Pagination>(root).Single();
            var buttons = All<Button>(root);
            var expected = reference.Where(element => element.Slot == "pagination-link").ToList();
            var errors = buttons.Zip(expected).SelectMany((pair, i) => Near($"{theme} button {i}", Windows.Box(pair.First, root), pair.Second, 1)).ToList();
            var ellipsis = All<PaginationEllipsis>(root).Single();
            errors.AddRange(Near($"{theme} ellipsis", Windows.Box(ellipsis, root), reference.Single(element => element.Slot == "pagination-ellipsis"), 1));
            var previous = All<PaginationPrevious>(root).Single();
            var next = All<PaginationNext>(root).Single();
            var chevron = Windows.Part<Viewbox>(previous);
            var text = Windows.Part<ContentPresenter>(previous, "PART_ContentPresenter");
            errors.AddRange(chevron.Bounds.Width == 16 && Windows.Box(text, previous).X - Windows.Box(chevron, previous).Right == 4 && Windows.Paint(Windows.Part<Path>(previous).Stroke) == Windows.Paint(previous.Foreground) ? [] : [$"{theme}: previous chevron {Windows.Box(chevron, previous)} text {Windows.Box(text, previous)}"]);
            var current = buttons.Single(button => button.Classes.Contains("outline"));
            var other = buttons.First(button => button.Classes.Contains("ghost"));
            var currentRoot = Windows.Part<ContentPresenter>(current, "PART_ContentPresenter");
            errors.AddRange(Paints($"{theme} current", currentRoot.Background, expected[2].Background).Concat(Paints($"{theme} other", Windows.Part<ContentPresenter>(other, "PART_ContentPresenter").Background, expected[1].Background)));
            Windows.Hover(window, previous);
            errors.AddRange(Paints($"{theme} previous under the pointer", Windows.Part<Border>(previous, "PART_Root").Background, window, "CosmoButtonGhostHoverBrush"));
            errors.AddRange(AutomationProperties.GetLandmarkType(pagination) == AutomationLandmarkType.Navigation && AutomationProperties.GetName(pagination) == "pagination" && AutomationProperties.GetName(previous) == "Go to previous page" && AutomationProperties.GetName(next) == "Go to next page" && ellipsis.Bounds.Width == 36
                ? [] : [$"{theme}: names"]);
            return errors;
        }));

    [Fact]
    [Trait("AC", "BUTTONGROUP-02")]
    public async Task ButtonGroupsJoinTheirChildrenAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["button-group-demo", "button-group-size", "button-group-orientation"])
        {
            wrong.AddRange(await FixtureAsync(fixture, (window, root, reference, theme) =>
            {
                var children = All<TemplatedControl>(root).Where(control => control.Parent is Aioros.Cosmo.Display.ButtonGroup).ToList();
                var expected = reference.Where(element => element.Tag == "button").ToList();
                var buttons = children.OfType<Button>().ToList();
                var errors = buttons.Zip(expected).SelectMany((pair, i) => Near($"{theme} {fixture} button {i}", Windows.Box(pair.First, root), pair.Second, 2.5)
                    .Concat(Math.Abs(pair.First.CornerRadius.TopLeft - Px(pair.Second.Radius)) < 0.01 && (!pair.First.Classes.Contains("outline") || pair.First.BorderThickness.Top == Px(pair.Second.BorderWidth)) ? [] : [$"{theme} {fixture} button {i}: {pair.First.CornerRadius} {pair.First.BorderThickness}, shadcn {pair.Second.Radius}"])).ToList();
                errors.AddRange(fixture == "button-group-orientation" ? Orientation(root, buttons, reference, theme) : []);

                buttons[^1].Focus(NavigationMethod.Tab);
                Windows.Settle();
                errors.AddRange(buttons[^1].ZIndex == 10 ? [] : [$"{theme}: focused child not above"]);
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static List<string> Orientation(Control root, List<Button> buttons, IReadOnlyList<ElementMetrics> reference, string theme)
    {
        var separator = All<Separator>(root).Single();
        var expected = reference.Single(element => element.Slot == "button-group-separator");
        var errors = Near($"{theme} separator", Windows.Box(separator, root), expected, 1).Concat(Paints($"{theme} separator", separator.Background, expected.Background)).ToList();
        errors.AddRange(buttons[0].CornerRadius == new CornerRadius(8, 8, 0, 0) && buttons[1].CornerRadius == new CornerRadius(0, 0, 8, 8) && buttons[1].BorderThickness == new Thickness(1, 0, 1, 1)
            ? [] : [$"{theme}: vertical {buttons[0].CornerRadius} {buttons[1].CornerRadius} {buttons[1].BorderThickness}"]);
        return errors;
    }
}
