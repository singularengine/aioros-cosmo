using Aioros.Cosmo.Display;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Components;

public sealed class BadgeCardTests(DesktopSession desktop)
{
    private static readonly string[] Variants = ["", "secondary", "destructive", "outline", "ghost", "link"];

    [Fact]
    [Trait("AC", "BADGE-02")]
    public async Task EveryBadgeVariantTakesShadcnsColorsAndBoxAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in new[] { (ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark") })
        {
            var expected = (await ReferenceMetrics.OfAsync("badge-variants", name)).Where(element => element.Tag == "span").ToList();
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var badges = Variants.Select(classes => new Badge { Content = "Badge", Classes = { classes } }).ToList();
                badges.ForEach(badge => badge.Classes.Remove(""));
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                badges.ForEach(row.Children.Add);
                var window = Windows.Show(row, variant);
                var errors = new List<string>();
                for (var i = 0; i < badges.Count; i++)
                {
                    var badge = badges[i];
                    if (!Windows.Paint(badge.Background).Near(CssColor.Parse(expected[i].Background)) || !Windows.Paint(badge.Foreground).Near(CssColor.Parse(expected[i].Color))
                        || !Windows.Paint(badge.BorderBrush).Near(CssColor.Parse(expected[i].BorderColor)))
                    {
                        errors.Add($"{name} {Variants[i]}: {Windows.Paint(badge.Background)} {Windows.Paint(badge.Foreground)} {Windows.Paint(badge.BorderBrush)}, shadcn {expected[i].Background} {expected[i].Color} {expected[i].BorderColor}");
                    }

                    if (badge.Padding != new Thickness(8, 2) || badge.BorderThickness != new Thickness(1) || badge.CornerRadius.TopLeft < badge.Bounds.Height / 2)
                    {
                        errors.Add($"{name} {Variants[i]}: padding {badge.Padding}, border {badge.BorderThickness}, radius {badge.CornerRadius}");
                    }
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "BADGE-03")]
    public async Task IconAndTextInABadgeAreFourPixelsApartAsync()
    {
        var (orientation, spacing, gap) = await desktop.RunAsync(() =>
        {
            var icon = new Border { Width = 12, Height = 12 };
            var text = new TextBlock { Text = "Verified" };
            var badge = new Badge { Classes = { "secondary" }, Content = new StackPanel { Children = { icon, text } } };
            var window = Windows.Show(badge, ThemeVariant.Light);
            var panel = (StackPanel)badge.Content!;
            var result = (panel.Orientation, panel.Spacing, text.Bounds.Left - icon.Bounds.Right);
            window.Close();
            return result;
        });

        Assert.Equal(Orientation.Horizontal, orientation);
        Assert.Equal(4, spacing);
        Assert.Equal(4, gap, 3);
    }

    [Fact]
    [Trait("AC", "CARD-02")]
    public async Task TheCardIsShadcnsSurfaceWithItsPartsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var header = new CardHeader { Title = "Login to your account", Description = "Enter your email below", Action = new Button { Classes = { "link" }, Content = "Sign Up" } };
            var content = new CardContent { Content = new TextBlock { Text = "Body" } };
            var footer = new CardFooter { Content = new Button { Content = "Login" } };
            var card = new Card { Width = 384, Items = { header, content, footer } };
            var window = Windows.Show(card, ThemeVariant.Light);
            var surface = Windows.Part<Border>(card);
            var presenters = Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(header).OfType<ContentPresenter>().ToList();
            var title = presenters[0];
            var description = presenters[1];
            var action = presenters[2];
            var result = new
            {
                Background = Windows.Paint(surface.Background),
                Border = Windows.Paint(surface.BorderBrush),
                surface.BorderThickness,
                surface.CornerRadius,
                surface.Padding,
                Shadow = surface.BoxShadow.ToString(),
                Gaps = (content.Bounds.Top - header.Bounds.Bottom, footer.Bounds.Top - content.Bounds.Bottom),
                TitleWeight = title.FontWeight,
                TitleLine = TextBlock.GetLineHeight(title),
                DescriptionSize = description.FontSize,
                DescriptionColor = Windows.Paint(description.Foreground),
                DescriptionGap = description.Bounds.Top - title.Bounds.Bottom,
                ActionRight = action.Bounds.Left > title.Bounds.Left + 100,
                Insets = (content.Padding.Left, content.Padding.Right, footer.Padding.Left, footer.Padding.Right, header.Padding.Left),
            };
            window.Close();
            return result;
        });

        Assert.Equal(CssColor.Parse("#FFFFFF"), found.Background);
        Assert.Equal(CssColor.Parse("#E5E5E5"), found.Border);
        Assert.Equal(new Thickness(1), found.BorderThickness);
        Assert.Equal(new CornerRadius(14), found.CornerRadius);
        Assert.Equal(new Thickness(0, 24), found.Padding);
        Assert.Equal(BoxShadows.Parse("0 1 3 0 #1A000000, 0 1 2 -1 #1A000000").ToString(), found.Shadow);
        Assert.Equal((24.0, 24.0), found.Gaps);
        Assert.Equal(FontWeight.SemiBold, found.TitleWeight);
        Assert.Equal(16, found.TitleLine);
        Assert.Equal(14, found.DescriptionSize);
        Assert.Equal(CssColor.Parse("#737373"), found.DescriptionColor);
        Assert.Equal(8, found.DescriptionGap, 3);
        Assert.True(found.ActionRight);
        Assert.Equal((24.0, 24.0, 24.0, 24.0, 24.0), found.Insets);
    }

    [Fact]
    [Trait("AC", "CARD-03")]
    public async Task AHeaderKeepsNoSpaceForAMissingDescriptionOrActionAsync()
    {
        var (alone, shown) = await desktop.RunAsync(() =>
        {
            var header = new CardHeader { Title = "Notifications" };
            var window = Windows.Show(new Card { Width = 384, Items = { header } }, ThemeVariant.Light);
            var height = header.Bounds.Height;
            header.Description = "You have 3 unread messages.";
            header.Action = new Badge { Content = "New" };
            Windows.Settle();
            var visible = Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(header).OfType<ContentPresenter>().Count(presenter => presenter.TemplatedParent == header && presenter.IsEffectivelyVisible);
            window.Close();
            return (height, visible);
        });

        Assert.Equal(16, alone, 3);
        Assert.Equal(3, shown);
    }

    [Fact]
    [Trait("AC", "BADGE-04")]
    public async Task ALinkBadgeTakesShadcnsHoverAndFocusAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Display.Looks.Themes)
        {
            var rest = await ReferenceMetrics.SlotsAsync("badge-links", name, "badge");
            var hover = await ReferenceMetrics.OfAsync("badge-links", name, "hover");
            var focus = await ReferenceMetrics.OfAsync("badge-links", name, "focus");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = Display.DisplayFixtures.Show("badge-links", variant);
                var root = (Control)window.Content!;
                var links = Display.Looks.All<HyperlinkButton>(root);
                var errors = links.Count == rest.Count ? new List<string>() : [$"{name}: {links.Count} links, shadcn {rest.Count}"];
                for (var i = 0; i < Math.Min(links.Count, rest.Count); i++)
                {
                    var presenter = Windows.Part<ContentPresenter>(links[i], "PART_ContentPresenter");
                    var box = Windows.Box(links[i], root);
                    var start = i == 0 ? rest[0].X : Windows.Box(links[i - 1], root).Right + 8;
                    errors.AddRange(Math.Abs(box.X - start) <= 0.5 && Math.Abs(box.Y - rest[i].Y) <= 1.5 && Math.Abs(box.Width - rest[i].Width) <= 1.5 && Math.Abs(box.Height - rest[i].Height) <= 1.5
                        ? [] : [$"{name} #{i}: {box}, 8 px after the last at {start}, shadcn {rest[i].Width}x{rest[i].Height} at y {rest[i].Y}"]);
                    errors.AddRange(Look($"{name} #{i} rest", presenter, rest[i]));
                    Windows.Hover(window, links[i]);
                    Windows.Settle();
                    errors.AddRange(Look($"{name} #{i} hover", presenter, hover[i]));
                    Windows.Away(window, new Point(2, window.Height - 2));
                    links[i].Focus(Avalonia.Input.NavigationMethod.Tab);
                    Windows.Settle();
                    errors.AddRange(Display.Looks.Paints($"{name} #{i} focus border", presenter.BorderBrush, focus[i].BorderColor));
                    errors.AddRange(CssShadow.Same(Windows.Shadows(presenter.BoxShadow), CssShadow.List(focus[i].Shadow)) ? [] : [$"{name} #{i} ring {presenter.BoxShadow}, shadcn {focus[i].Shadow}"]);
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Look(string what, ContentPresenter presenter, ElementMetrics expected) =>
        Display.Looks.Paints($"{what} background", presenter.Background, expected.Background)
            .Concat(Display.Looks.Paints($"{what} text", presenter.Foreground, expected.Color))
            .Concat(Display.Looks.Px(expected.BorderWidth) > 0 && CssColor.Parse(expected.BorderColor).A > 0 ? Display.Looks.Paints($"{what} border", presenter.BorderBrush, expected.BorderColor) : [])
            .Concat((presenter.GetValue(TextBlock.TextDecorationsProperty) is { Count: > 0 }) == (expected.Decoration == "underline") ? [] : [$"{what}: decoration {presenter.GetValue(TextBlock.TextDecorationsProperty)?.Count}, shadcn {expected.Decoration}"]);

    [Fact]
    [Trait("AC", "CARD-04")]
    public async Task ASeparatedHeaderAndFooterAreRuledOffAsync() =>
        Assert.Empty(await Display.Looks.FixtureAsync(desktop, "card-separated", (window, root, reference, theme) =>
        {
            var parts = new (string Slot, Control Control)[]
            {
                ("card", Display.Looks.All<Card>(root).Single()), ("card-header", Display.Looks.All<CardHeader>(root).Single()),
                ("card-content", Display.Looks.All<CardContent>(root).Single()), ("card-footer", Display.Looks.All<CardFooter>(root).Single()),
                ("button", Display.Looks.All<Button>(root).Single()),
            };
            var errors = parts.SelectMany(part => Display.Looks.Near($"{theme} {part.Slot}", Windows.Box(part.Control, root), reference.First(element => element.Slot == part.Slot), 1.5)).ToList();
            var header = Display.Looks.All<CardHeader>(root).Single();
            var footer = Display.Looks.All<CardFooter>(root).Single();
            var border = reference.First(element => element.Slot == "card").BorderColor;
            errors.AddRange(header.BorderThickness == new Thickness(0, 0, 0, 1) && footer.BorderThickness == new Thickness(0, 1, 0, 0) ? [] : [$"{theme}: borders {header.BorderThickness} {footer.BorderThickness}"]);
            errors.AddRange(Display.Looks.Paints($"{theme} header border", header.BorderBrush, border).Concat(Display.Looks.Paints($"{theme} footer border", footer.BorderBrush, border)));
            return errors;
        }));
}
