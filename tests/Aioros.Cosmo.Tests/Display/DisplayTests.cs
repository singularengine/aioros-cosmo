using Aioros.Cosmo.Display;
using Path = Avalonia.Controls.Shapes.Path;
using Spinner = Aioros.Cosmo.Display.Spinner;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Overlays;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Display;

public sealed class DisplayTests(DesktopSession desktop)
{
    private Task<List<string>> FixtureAsync(string fixture, Func<Window, Control, IReadOnlyList<ElementMetrics>, string, IEnumerable<string>> check) =>
        Looks.FixtureAsync(desktop, fixture, check);

    private static Style Animated(Window window, string type) =>
        window.Styles.OfType<Aioros.Cosmo.Theming.CosmoTheme>().Single().OfType<Style>().Single(style => style.Animations.Count > 0 && (style.Selector?.ToString() ?? string.Empty).EndsWith(type, StringComparison.Ordinal));

    [Fact]
    [Trait("AC", "SEPARATOR-02")]
    public async Task SeparatorsAreShadcnsLinesAsync() =>
        Assert.Empty(await FixtureAsync("separator-demo", (window, root, reference, theme) =>
        {
            var separators = All<Separator>(root);
            var expected = reference.Where(element => element.Slot == "separator").ToList();
            var errors = separators.Zip(expected).SelectMany((pair, i) => Near($"{theme} #{i}", Windows.Box(pair.First, root), pair.Second, 1).Concat(Paints($"{theme} #{i}", pair.First.Background, pair.Second.Background))).ToList();
            var menu = new Separator();
            var context = new ContextMenu { Items = { new MenuItem { Header = "Copy" }, menu } };
            root.ContextMenu = context;
            context.Open(root);
            Windows.Settle();
            errors.AddRange(ReferenceEquals(menu.Theme, window.FindResource("CosmoMenuSeparator")) ? [] : [$"{theme}: a menu's separator has the theme {menu.Theme}"]);
            context.Close();
            return separators.Count == 3 ? errors : [.. errors, $"{theme}: {separators.Count} separators"];
        }));

    [Fact]
    [Trait("AC", "SKELETON-02")]
    public async Task SkeletonsAreAccentBoxesThatPulseAsync() =>
        Assert.Empty(await FixtureAsync("skeleton-demo", (window, root, reference, theme) =>
        {
            var skeletons = All<Skeleton>(root);
            var expected = reference.Where(element => element.Slot == "skeleton").ToList();
            var errors = skeletons.Zip(expected).SelectMany((pair, i) =>
            {
                var border = Windows.Part<Border>(pair.First);
                return Near($"{theme} #{i}", Windows.Box(pair.First, root), pair.Second, 1).Concat(Paints($"{theme} #{i}", border.Background, pair.Second.Background))
                    .Concat(Math.Min(border.CornerRadius.TopLeft, 24) == Math.Min(Looks.Px(pair.Second.Radius), 24) ? [] : [$"{theme} #{i}: radius {border.CornerRadius}"]);
            }).ToList();
            var pulse = Animated(window, "Skeleton").Animations.OfType<Animation>().Single();
            var frames = pulse.Children.Select(frame => (frame.Cue.CueValue, (double)frame.Setters.OfType<Setter>().Single(setter => setter.Property == Visual.OpacityProperty).Value!)).ToList();
            errors.AddRange(pulse.Duration == TimeSpan.FromSeconds(2) && pulse.IterationCount.IsInfinite && pulse.Easing is SplineEasing { X1: 0.4, Y1: 0, X2: 0.6, Y2: 1 } && frames.SequenceEqual([(0.0, 1.0), (0.5, 0.5), (1.0, 1.0)])
                ? [] : [$"{theme}: pulse {pulse.Duration} {pulse.Easing} {string.Join(',', frames)}"]);
            return skeletons.Count == 6 ? errors : [.. errors, $"{theme}: {skeletons.Count} skeletons"];
        }));

    [Fact]
    [Trait("AC", "SPINNER-02")]
    public async Task SpinnersTurnInTheirParentsColorAsync() =>
        Assert.Empty(await FixtureAsync("spinner-demo", (window, root, reference, theme) =>
        {
            var spinners = All<Spinner>(root);
            var expected = reference.Where(element => element.Tag == "svg").ToList();
            var errors = spinners.Take(4).Zip(expected).SelectMany((pair, i) => Near($"{theme} #{i}", Laid(pair.First, root), pair.Second, 1)).ToList();
            var primary = reference.First(element => element.Tag == "button");
            var outline = reference.Last(element => element.Slot == "badge");
            errors.AddRange(Paints($"{theme} button spinner", Windows.Part<Path>(spinners[4]).Stroke, primary.Color));
            errors.AddRange(Paints($"{theme} badge spinner", Windows.Part<Path>(spinners[8]).Stroke, outline.Color));
            errors.AddRange(spinners.Skip(4).Select(spinner => spinner.Bounds.Width).SequenceEqual([16.0, 16, 12, 12, 12]) ? [] : [$"{theme}: inner spinners {string.Join(',', spinners.Skip(4).Select(spinner => spinner.Bounds.Width))}"]);
            errors.AddRange(spinners.All(spinner => AutomationProperties.GetName(spinner) == "Loading" && AutomationProperties.GetLiveSetting(spinner) == AutomationLiveSetting.Polite) ? [] : [$"{theme}: spinners not named Loading, polite"]);
            var turn = Animated(window, "Spinner").Animations.OfType<Animation>().Single();
            var angles = turn.Children.Select(frame => (frame.Cue.CueValue, Convert.ToDouble(frame.Setters.OfType<Setter>().Single().Value, System.Globalization.CultureInfo.InvariantCulture))).ToList();
            errors.AddRange(turn.Duration == TimeSpan.FromSeconds(1) && turn.IterationCount.IsInfinite && turn.Easing is LinearEasing && angles.SequenceEqual([(0.0, 0.0), (1.0, 360.0)]) && spinners[0].RenderTransformOrigin == RelativePoint.Center
                ? [] : [$"{theme}: turn {turn.Duration} {turn.Easing} {string.Join(',', angles)} {spinners[0].RenderTransformOrigin}"]);
            return errors;
        }));

    [Fact]
    [Trait("AC", "TYPOGRAPHY-05")]
    public async Task BalancedTextsKeepTheirLinesAndEvenTheirWidthsAsync()
    {
        var wrapped = 0;
        var wrong = new List<string>();
        foreach (var fixture in (string[])["empty-demo", "field-content", "item-header-footer"])
        {
            wrong.AddRange(await FixtureAsync(fixture, (window, root, reference, theme) =>
                All<TextBlock>(root).Where(text => Aioros.Markup.Web.GetTextWrap(text) == "balance" && !string.IsNullOrEmpty(text.Text)).SelectMany(text =>
                {
                    var (lines, errors) = Balanced($"{theme} {fixture} '{text.Text}'", text);
                    wrapped += lines > 1 ? 1 : 0;
                    return errors;
                }).ToList()));
        }

        var tip = await desktop.RunAsync(() =>
        {
            var window = OverlayFixtures.Show(OverlayFixtures.TooltipOpen(), ThemeVariant.Light);
            var wrap = window.GetVisualDescendants().OfType<ToolTip>().First(found => found.IsEffectivelyVisible).GetVisualDescendants().OfType<TextBlock>().Select(Aioros.Markup.Web.GetTextWrap).FirstOrDefault();
            window.Close();
            return wrap;
        });

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
        Assert.True(wrapped >= 4, $"{wrapped} wrapped texts");
        Assert.Equal("balance", tip);
    }

    private static (int Lines, List<string> Errors) Balanced(string what, TextBlock text)
    {
        static Avalonia.Media.TextFormatting.TextLayout Layout(TextBlock text, string value, double width) =>
            new(value, new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, null, text.TextAlignment, TextWrapping.Wrap, maxWidth: width, lineHeight: text.LineHeight, letterSpacing: text.LetterSpacing);

        var errors = new List<string>();
        var width = text.Bounds.Width;
        var lines = text.TextLayout.TextLines.Select(line => line.Width).ToList();
        var plain = Layout(text, text.Text!, width).TextLines.Count;
        var word = text.Text!.Split(' ').Max(part => Layout(text, part, double.PositiveInfinity).Width);
        if (lines.Count != plain || lines.Max() - lines.Min() > word)
        {
            errors.Add($"{what}: {lines.Count} lines ({string.Join(", ", lines.Select(line => Math.Round(line)))}) of {plain}, a word {word:F0}");
        }

        Aioros.Markup.Web.SetTextWrap(text, null);
        Windows.Settle();
        if (text.Bounds.Width != width || text.Padding != default)
        {
            errors.Add($"{what}: width {width} then {text.Bounds.Width}, padding {text.Padding} without the balance");
        }

        return (lines.Count, errors);
    }

    [Fact]
    [Trait("AC", "KBD-02")]
    public async Task KeysAreShadcnsAsync() =>
        Assert.Empty(await FixtureAsync("kbd-demo", (window, root, reference, theme) =>
        {
            var keys = All<Kbd>(root);
            var expected = reference.Where(element => element.Slot == "kbd").ToList();
            var errors = keys.Zip(expected).SelectMany((pair, i) => Key($"{theme} #{i}", pair.First, root, pair.Second)).ToList();
            var group = All<KbdGroup>(root)[0];
            errors.AddRange(group.Spacing == 4 && group.Orientation == Avalonia.Layout.Orientation.Horizontal ? [] : [$"{theme}: group {group.Spacing}"]);
            return keys.Count == 10 ? errors : [.. errors, $"{theme}: {keys.Count} keys"];
        }));

    /// <summary>A key's look and box, its symbols in Geist or, for those Geist lacks (⌘ ⌥ ⌃), in Cosmo Symbols on every machine.</summary>
    private static IEnumerable<string> Key(string what, Kbd key, Control root, ElementMetrics expected)
    {
        var border = Windows.Part<Border>(key);
        var look = key.FontSize == 12 && key.FontWeight == FontWeight.Medium && border.CornerRadius.TopLeft == 6 && !key.IsHitTestVisible;
        return Near(what, Windows.Box(key, root), expected, 1.5)
            .Concat(Paints(what, border.Background, expected.Background)).Concat(Paints($"{what} text", key.Foreground, expected.Color))
            .Concat(look ? [] : [$"{what}: {key.FontSize} {key.FontWeight} {border.CornerRadius}"]);
    }

    [Fact]
    [Trait("AC", "AVATAR-02")]
    public async Task AvatarsShowTheirImageOrFallbackAsync() =>
        Assert.Empty(await FixtureAsync("avatar-demo", (window, root, reference, theme) =>
        {
            var avatars = All<Avatar>(root);
            var expected = reference.Where(element => element.Slot == "avatar").ToList();
            var errors = avatars.Zip(expected).SelectMany((pair, i) =>
            {
                var clip = Windows.Part<Border>(pair.First, "PART_Root");
                var fallback = Windows.Part<Border>(pair.First, "PART_Fallback");
                return Near($"{theme} #{i}", Windows.Box(pair.First, root), pair.Second, 1)
                    .Concat(clip.ClipToBounds && Math.Min(clip.CornerRadius.TopLeft, 99) == Math.Min(Looks.Px(pair.Second.Radius), 99) ? [] : [$"{theme} #{i}: clip {clip.CornerRadius}"])
                    .Concat(fallback.IsVisible == (pair.First.Source is null) ? [] : [$"{theme} #{i}: fallback shown {fallback.IsVisible}"]);
            }).ToList();
            var fallbacks = reference.Where(element => element.Slot == "avatar-fallback").ToList();
            errors.AddRange(avatars.Skip(5).Zip(fallbacks).SelectMany((pair, i) => Paints($"{theme} fallback {i}", Windows.Part<Border>(pair.First, "PART_Fallback").Background, pair.Second.Background).Concat(Paints($"{theme} fallback {i} text", pair.First.Foreground, pair.Second.Color))));
            var ring = (BoxShadows)window.FindResource(window.ActualThemeVariant, "CosmoRingBackgroundShadow")!;
            errors.AddRange(avatars.Skip(2).Take(3).Select((avatar, i) => (avatar, i)).Where(pair => pair.avatar.Margin.Left != -8 || Windows.Part<Border>(pair.avatar, "PART_Ring").BoxShadow != ring).Select(pair => $"{theme} grouped {pair.i}: {pair.avatar.Margin} {Windows.Part<Border>(pair.avatar, "PART_Ring").BoxShadow}"));
            return avatars.Count == 8 ? errors : [.. errors, $"{theme}: {avatars.Count} avatars"];
        }));

    [Fact]
    [Trait("AC", "AVATAR-05")]
    public async Task ABadgeAndAGroupCountFollowTheirAvatarsAsync()
    {
        var wrong = await FixtureAsync("avatar-badge", (window, root, reference, theme) => Avatars(window, root, reference, theme));
        wrong.AddRange(await FixtureAsync("avatar-group-count", (window, root, reference, theme) => Avatars(window, root, reference, theme)));
        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Avatars(Window window, Control root, IReadOnlyList<ElementMetrics> reference, string theme)
    {
        var ring = (BoxShadows)window.FindResource(window.ActualThemeVariant, "CosmoRingBackgroundShadow")!;
        var parts = new (string Slot, List<Control> Controls)[]
        {
            ("avatar", [.. All<Avatar>(root)]), ("avatar-badge", [.. All<AvatarBadge>(root)]), ("avatar-group-count", [.. All<AvatarGroupCount>(root)]),
        };
        foreach (var (slot, controls) in parts)
        {
            var expected = reference.Where(element => element.Slot == slot).ToList();
            if (controls.Count != expected.Count)
            {
                yield return $"{theme} {slot}: {controls.Count}, shadcn {expected.Count}";
                continue;
            }

            foreach (var (control, element) in controls.Zip(expected))
            {
                foreach (var error in Near($"{theme} {slot}", Windows.Box(control, root), element, 1.5))
                {
                    yield return error;
                }

                if (control is AvatarBadge or AvatarGroupCount)
                {
                    var border = control.GetVisualChildren().OfType<Border>().Single();
                    foreach (var error in Paints($"{theme} {slot}", border.Background, element.Background).Concat(Paints($"{theme} {slot} text", ((ContentControl)control).Foreground, element.Color)))
                    {
                        yield return error;
                    }

                    if (border.BoxShadow != ring || !border.CornerRadius.TopLeft.Equals(9999))
                    {
                        yield return $"{theme} {slot}: ring {border.BoxShadow} radius {border.CornerRadius}";
                    }
                }
            }
        }

        foreach (var badge in All<AvatarBadge>(root))
        {
            var avatar = badge.FindAncestorOfType<Avatar>()!;
            var clipped = Windows.Part<Border>(avatar, "PART_Root").IsVisualAncestorOf(badge);
            var icon = Windows.Part<ContentPresenter>(badge, "PART_ContentPresenter").IsVisible;
            if (!clipped || icon == avatar.Classes.Contains("sm"))
            {
                yield return $"{theme} badge in {string.Join(' ', avatar.Classes)}: inside the circle {clipped}, icon shown {icon}";
            }
        }
    }

    [Fact]
    [Trait("AC", "ALERT-02")]
    public async Task AlertsAreShadcnsAsync() =>
        Assert.Empty(await FixtureAsync("alert-demo", (window, root, reference, theme) =>
        {
            var alerts = All<Alert>(root);
            var errors = alerts.Zip(reference.Where(element => element.Slot == "alert")).SelectMany((pair, i) =>
            {
                var border = Windows.Part<Border>(pair.First);
                return Near($"{theme} #{i}", Windows.Box(pair.First, root), pair.Second, 1).Concat(Paints($"{theme} #{i}", border.Background, pair.Second.Background)).Concat(Paints($"{theme} #{i} border", border.BorderBrush, pair.Second.BorderColor))
                    .Concat(border.CornerRadius.TopLeft == 10 && border.Padding == new Thickness(16, 12) && AutomationProperties.GetLiveSetting(pair.First) == AutomationLiveSetting.Assertive ? [] : [$"{theme} #{i}: {border.CornerRadius} {border.Padding}"]);
            }).ToList();
            var titles = alerts.Select(alert => Windows.Part<ContentPresenter>(alert, "PART_Title")).ToList();
            errors.AddRange(titles.Zip(reference.Where(element => element.Slot == "alert-title")).SelectMany((pair, i) => Near($"{theme} title {i}", Windows.Box(pair.First, root), pair.Second, 1).Concat(Paints($"{theme} title {i}", pair.First.Foreground, pair.Second.Color))
                .Concat(pair.First.LetterSpacing == -0.35 && pair.First.FontWeight == FontWeight.Medium ? [] : [$"{theme} title {i}: {pair.First.LetterSpacing} {pair.First.FontWeight}"])));
            var descriptions = new[] { alerts[0], alerts[2] }.Select(alert => Windows.Part<ContentPresenter>(alert, "PART_Description")).ToList();
            errors.AddRange(descriptions.Zip(reference.Where(element => element.Slot == "alert-description")).SelectMany((pair, i) => Near($"{theme} description {i}", Windows.Box(pair.First, root), pair.Second, 1).Concat(Paints($"{theme} description {i}", pair.First.Foreground, pair.Second.Color))));
            var bullets = All<ListItem>(root).Select(item => (Windows.Part<Ellipse>(item), Windows.Part<ContentPresenter>(item, "PART_ContentPresenter"))).ToList();
            errors.AddRange(bullets.Where(pair => pair.Item1.Bounds.Width != 4 || Windows.Box(pair.Item2, root).X - Windows.Box(pair.Item1, root).X < 18.5).Select(pair => $"{theme}: bullet {pair.Item1.Bounds} text {pair.Item2.Bounds}"));
            return alerts[1].Bounds.Height == 46 && bullets.Count == 3 ? errors : [.. errors, $"{theme}: {alerts[1].Bounds.Height} high without description, {bullets.Count} bullets"];
        }));

    [Fact]
    [Trait("AC", "ASPECTRATIO-02")]
    public async Task TheAspectRatioSizesItsChildAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var ratios = new[] { 16.0 / 9, 1, 4.0 / 3 }.Select(ratio => new AspectRatio { Ratio = ratio, Width = 450, Child = new Border() }).ToList();
            var free = new AspectRatio { Ratio = 2, Child = new Border { Width = 120 } };
            var panel = new StackPanel();
            panel.Children.AddRange(ratios);
            panel.Children.Add(new Canvas { Children = { free } });
            var window = Windows.Show(panel, ThemeVariant.Light);
            var heights = ratios.Select(ratio => (ratio.Bounds.Height, ratio.Child!.Bounds.Height)).ToList();
            var unconstrained = free.Bounds.Size;
            window.Close();
            var refused = new[] { 0, double.PositiveInfinity }.Count(ratio => Throws(() => _ = new AspectRatio { Ratio = ratio }));
            return (heights, unconstrained, refused);
        });

        Assert.Equal([253.125, 450.0, 337.5], found.heights.Select(height => height.Item1));
        Assert.All(found.heights, height => Assert.InRange(height.Item2 - height.Item1, 0, 1));
        Assert.Equal(new Size(120, 60), found.unconstrained);
        Assert.Equal(2, found.refused);
    }

    private static bool Throws(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    [Fact]
    [Trait("AC", "TYPOGRAPHY-02")]
    public async Task TypographyIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["typography-headings", "typography-text"])
        {
            wrong.AddRange(await FixtureAsync(fixture, (window, root, reference, theme) => Typography(root, reference, theme)));
        }

        Assert.Empty(wrong);
    }

    private static List<string> Typography(Control root, IReadOnlyList<ElementMetrics> reference, string theme)
    {
        var texts = All<TextBlock>(root);
        var errors = reference.Where(element => element.Text.Length > 0 && element.Tag != "ul").SelectMany(expected => TextLook(root, texts.FirstOrDefault(block => block.Text == expected.Text), expected, theme)).ToList();
        var items = All<ListItem>(root);
        errors.AddRange(items.Select(item => (Windows.Box(Windows.Part<Ellipse>(item), root), Windows.Box(Windows.Part<ContentPresenter>(item, "PART_ContentPresenter"), root))).Where(pair => pair.Item1.Width != 5 || pair.Item2.X - pair.Item1.X != 17).Select(pair => $"{theme}: bullet {pair.Item1} text {pair.Item2}"));
        errors.AddRange(items.Zip(items.Skip(1)).Where(pair => pair.Second.Bounds.Y - pair.First.Bounds.Bottom != 8).Select(_ => $"{theme}: items not 8 px apart"));
        return errors;
    }

    private static IEnumerable<string> TextLook(Control root, TextBlock? text, ElementMetrics expected, string theme)
    {
        if (text is null)
        {
            return [$"{theme}: no text {expected.Text}"];
        }

        var tracking = expected.Tag is "h1" or "h2" or "h3" or "h4" ? -0.025 * text.FontSize : 0;
        var weight = expected.Tag == "h1" ? 700 : int.Parse(expected.FontWeight, System.Globalization.CultureInfo.InvariantCulture);
        var italic = expected.Tag == "blockquote" ? FontStyle.Italic : FontStyle.Normal;
        var level = expected.Tag is ['h', var digit] ? digit - '0' : 0;
        var look = text.FontSize == Looks.Px(expected.FontSize) && text.LineHeight == Looks.Px(expected.LineHeight ?? "0px") && (int)text.FontWeight == weight && Math.Abs(text.LetterSpacing - tracking) < 0.01 && text.FontStyle == italic && AutomationProperties.GetHeadingLevel(text) == level;
        var box = expected.Tag is "h1" or "h2" or "code" or "blockquote" or "lead" or "p" ? [] : Near($"{theme} {expected.Tag}", Windows.Box(text, root), expected, 1.5);
        return (look ? [] : new[] { $"{theme} {expected.Tag}: {text.FontSize}/{text.LineHeight} {text.FontWeight} {text.LetterSpacing} {text.FontStyle} level {AutomationProperties.GetHeadingLevel(text)}" })
            .Concat(Paints($"{theme} {expected.Tag}", text.Foreground, expected.Color)).Concat(box);
    }

    [Fact]
    [Trait("AC", "EMPTY-02")]
    public async Task EmptyStatesAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["empty-demo", "empty-outline"])
        {
            wrong.AddRange(await FixtureAsync(fixture, (window, root, reference, theme) =>
            {
                var empty = All<Empty>(root).Single();
                var tile = All<EmptyMedia>(root).Single();
                var errors = Near($"{theme} {fixture}", Windows.Box(empty, root), reference.Single(element => element.Slot == "empty"), 1).ToList();
                var shadcnTile = reference.Single(element => element.Slot == "empty-icon");
                errors.AddRange(Near($"{theme} tile", Windows.Box(tile, root), shadcnTile, 1.5).Concat(Paints($"{theme} tile", Windows.Part<Border>(tile).Background, shadcnTile.Background)));
                errors.AddRange(Windows.Part<Border>(tile).CornerRadius.TopLeft == 10 ? [] : [$"{theme}: tile radius"]);
                var title = Windows.Part<ContentPresenter>(empty, "PART_Title");
                var description = Windows.Part<ContentPresenter>(empty, "PART_Description");
                var shadcnTitle = reference.Single(element => element.Slot == "empty-title");
                var shadcnDescription = reference.Single(element => element.Slot == "empty-description");
                errors.AddRange(Near($"{theme} title", Windows.Box(title, root), shadcnTitle, 1.5).Concat(Paints($"{theme} title", title.Foreground, shadcnTitle.Color)));
                errors.AddRange(Math.Abs(Windows.Box(description, root).Y - shadcnDescription.Y) <= 1.5 && description.FontSize == 14 ? [] : [$"{theme}: description {Windows.Box(description, root)}"]);
                errors.AddRange(Paints($"{theme} description", description.Foreground, shadcnDescription.Color));
                var buttons = All<Button>(root);
                errors.AddRange(buttons.Zip(reference.Where(element => element.Tag == "button")).SelectMany((pair, i) => Near($"{theme} button {i}", Windows.Box(pair.First, root), pair.Second, 1.5)));
                errors.AddRange(Dashes(empty, fixture == "empty-outline", reference.Single(element => element.Slot == "empty").BorderColor, theme));
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Dashes(Empty empty, bool outline, string border, string theme)
    {
        var dashes = Windows.Part<Rectangle>(empty, "PART_Dashes");
        if (dashes.IsVisible != outline)
        {
            return [$"{theme}: dashes shown {dashes.IsVisible}"];
        }

        return !outline || (dashes.StrokeDashArray is { Count: > 0 } && dashes.RadiusX == 10 && Windows.Paint(dashes.Stroke).Near(CssColor.Parse(border))) ? [] : [$"{theme}: dashes {dashes.RadiusX} {Windows.Paint(dashes.Stroke)}"];
    }

    [Fact]
    [Trait("AC", "ITEM-02")]
    public async Task ItemsAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["item-demo", "item-variant"])
        {
            wrong.AddRange(await FixtureAsync(fixture, (window, root, reference, theme) =>
            {
                var items = All<Item>(root);
                var errors = items.Zip(reference.Where(element => element.Slot == "item")).SelectMany((pair, i) =>
                {
                    var border = Windows.Part<Border>(pair.First, "PART_Border");
                    var title = Windows.Part<ContentPresenter>(pair.First, "PART_Title");
                    return Near($"{theme} item {i}", Windows.Box(pair.First, root), pair.Second, 2.5).Concat(Paints($"{theme} item {i}", border.Background, pair.Second.Background)).Concat(Paints($"{theme} item {i} border", border.BorderBrush, pair.Second.BorderColor))
                        .Concat(title.FontWeight == FontWeight.Medium && title.FontSize == 14 ? [] : [$"{theme} item {i}: title font"]);
                }).ToList();
                errors.AddRange(items.Select(item => Windows.Part<ContentPresenter>(item, "PART_Title")).Zip(reference.Where(element => element.Slot == "item-title")).SelectMany((pair, i) => Placed($"{theme} title {i}", Windows.Box(pair.First, root), pair.Second, 2.5)));
                errors.AddRange(items.Where(item => item.Description is not null).Select(item => Windows.Part<ContentPresenter>(item, "PART_Description")).Zip(reference.Where(element => element.Slot == "item-description")).SelectMany((pair, i) => Placed($"{theme} description {i}", Windows.Box(pair.First, root), pair.Second, 2.5).Concat(Paints($"{theme} description {i}", pair.First.Foreground, pair.Second.Color))));
                errors.AddRange(All<Button>(root).Zip(reference.Where(element => element.Tag == "button")).SelectMany((pair, i) => Near($"{theme} action {i}", Windows.Box(pair.First, root), pair.Second, 2.5)));
                errors.AddRange(Hovers(window, items, theme));
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "ITEM-05")]
    public async Task AnItemsHeaderAndFooterAreFullRowsAsync() =>
        Assert.Empty(await FixtureAsync("item-header-footer", (window, root, reference, theme) =>
        {
            ElementMetrics Slot(string slot) => reference.First(element => element.Slot == slot);
            var item = All<Item>(root).Single();
            var header = All<ItemHeader>(root).Single();
            var footer = All<ItemFooter>(root).Single();
            var errors = Near($"{theme} item", Windows.Box(item, root), Slot("item"), 1.5)
                .Concat(Near($"{theme} header", Windows.Box(header, root), Slot("item-header"), 1.5))
                .Concat(Near($"{theme} badge", Windows.Box(All<Badge>(root).Single(), root), Slot("badge"), 1.5))
                .Concat(Placed($"{theme} title", Windows.Box(Windows.Part<ContentPresenter>(item, "PART_Title"), root), Slot("item-title"), 1.5))
                .Concat(Placed($"{theme} description", Windows.Box(Windows.Part<ContentPresenter>(item, "PART_Description"), root), Slot("item-description"), 1.5))
                .Concat(Near($"{theme} action", Windows.Box(All<Button>(root).Single(), root), Slot("button"), 1.5))
                .Concat(Near($"{theme} footer", Windows.Box(footer, root), Slot("item-footer"), 1.5)).ToList();
            var ends = footer.Children.Select(child => Windows.Box(child, footer)).ToList();
            errors.AddRange(ends[0].X == 0 && Math.Abs(ends[^1].Right - footer.Bounds.Width) < 0.5 ? [] : [$"{theme}: footer texts at {string.Join(", ", ends)}"]);
            return errors;
        }));

    [Fact]
    [Trait("AC", "ITEM-07")]
    public async Task AnItemSeparatorIsALineBetweenGroupedItemsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var first = new Item { Title = "Inbox", Description = "12 unread messages." };
            var separator = new ItemSeparator();
            var second = new Item { Title = "Archive", Description = "Everything you kept." };
            var group = new ItemGroup { Width = 448, Children = { first, separator, second } };
            var window = Windows.Show(group, ThemeVariant.Light);
            var line = Windows.Box(separator, group);
            var errors = Paints("separator", Windows.Part<Border>(separator).Background, window, "CosmoBorderBrush").ToList();
            if (line.Height != 1 || line.Width != 448 || separator.Margin != default || line.Y != first.Bounds.Bottom || second.Bounds.Y != line.Bottom)
            {
                errors.Add($"separator {line} margin {separator.Margin} between {first.Bounds} and {second.Bounds}");
            }

            window.Close();
            return errors;
        });

        Assert.Empty(found);
    }

    private static List<string> Hovers(Window window, IEnumerable<Item> items, string theme)
    {
        var errors = new List<string>();
        foreach (var item in items)
        {
            var rest = Windows.Paint(Windows.Part<Border>(item, "PART_Border").Background);
            Windows.Hover(window, item);
            var hovered = Windows.Paint(Windows.Part<Border>(item, "PART_Border").Background);
            var expected = item.Classes.Contains("link") ? Windows.Paint((IBrush)window.FindResource(window.ActualThemeVariant, "CosmoAccentHalfBrush")!) : rest;
            errors.AddRange(hovered == expected ? [] : [$"{theme}: {item.Title} under the pointer {hovered}, expected {expected}"]);
            Windows.Away(window, new Point(2, window.Height - 2));
        }

        return errors;
    }
}
