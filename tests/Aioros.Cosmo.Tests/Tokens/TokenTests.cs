using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Theming;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Tokens;

public sealed class TokenTests(DesktopSession desktop)
{
    private static readonly string[] Tokens = ["background", "foreground", "card", "card-foreground", "popover", "popover-foreground", "primary", "primary-foreground", "secondary", "secondary-foreground", "muted", "muted-foreground", "accent", "accent-foreground", "destructive", "border", "input", "ring", "chart-1", "chart-2", "chart-3", "chart-4", "chart-5", "sidebar", "sidebar-foreground", "sidebar-primary", "sidebar-primary-foreground", "sidebar-accent", "sidebar-accent-foreground", "sidebar-border", "sidebar-ring"];

    private static readonly (ThemeVariant Variant, string Name)[] Variants = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static string Pascal(string token) => string.Concat(token.Split('-').Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

    private static object? Resource(ResourceDictionary dictionary, string key, ThemeVariant variant) =>
        dictionary.TryGetResource(key, variant, out var value) ? value : null;

    [Fact]
    [Trait("AC", "TOKENS-01")]
    public async Task EveryTokenIsThePinnedOklchConvertedToSrgbAsync()
    {
        var themes = await ReferenceMetrics.ThemesAsync();

        var wrong = await desktop.RunAsync(() => Enum.GetValues<CosmoColor>()
            .SelectMany(color => Variants.SelectMany(variant => Mismatches(color, variant.Variant, themes[color.ToString().ToLowerInvariant()][variant.Name])))
            .ToList());

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Mismatches(CosmoColor color, ThemeVariant variant, IReadOnlyDictionary<string, string> palette)
    {
        var dictionary = CosmoResources.Colors(color);
        foreach (var token in Tokens)
        {
            var expected = CssColor.Parse(palette[token]);
            var asColor = Resource(dictionary, $"Cosmo{Pascal(token)}Color", variant) is Color c ? new CssColor(c.R, c.G, c.B, c.A / 255.0) : (CssColor?)null;
            var asBrush = Resource(dictionary, $"Cosmo{Pascal(token)}Brush", variant) is ISolidColorBrush b ? Windows.Paint(b) : (CssColor?)null;
            if (asColor is not { } found || asBrush is not { } painted || !found.Near(expected, 0) || !painted.Near(expected, 0))
            {
                yield return $"{color} {variant} {token}: expected {expected}, color {asColor}, brush {asBrush}";
            }
        }
    }

    [Fact]
    [Trait("AC", "TOKENS-02")]
    public async Task TheConversionMatchesChromiumWithinOneLevelAsync()
    {
        var checkedValues = 0;
        var wrong = new List<string>();
        foreach (var color in Enum.GetValues<CosmoColor>())
        {
            foreach (var (_, name) in Variants)
            {
                foreach (var (token, (css, rgba)) in await ReferenceMetrics.TokensAsync(color.ToString().ToLowerInvariant(), name))
                {
                    var ours = CssColor.Parse(css);
                    var alpha = (int)Math.Round(ours.A * 255);
                    var premultiplied = new[] { ours.R, ours.G, ours.B }.Select(channel => channel * ours.A);
                    var chromium = rgba.Take(3).Select(channel => channel * rgba[3] / 255.0);
                    checkedValues++;
                    if (Math.Abs(alpha - rgba[3]) > 1 || premultiplied.Zip(chromium).Any(pair => Math.Abs(pair.First - pair.Second) > 1))
                    {
                        wrong.Add($"{color} {name} {token} {css}: ours {ours}, Chromium [{string.Join(',', rgba)}]");
                    }
                }
            }
        }

        Assert.Equal(744, checkedValues);
        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TOKENS-03")]
    public async Task TheRadiusScaleFollowsTheRegistryAsync()
    {
        var rems = new Dictionary<CosmoRadius, double> { [CosmoRadius.None] = 0, [CosmoRadius.Small] = 0.3, [CosmoRadius.Medium] = 0.5, [CosmoRadius.Default] = 0.625, [CosmoRadius.Large] = 0.75, [CosmoRadius.ExtraLarge] = 1 };
        var steps = new (string Step, double Factor)[] { ("Sm", 0.6), ("Md", 0.8), ("Lg", 1), ("Xl", 1.4), ("2Xl", 1.8), ("3Xl", 2.2), ("4Xl", 2.6) };

        var wrong = await desktop.RunAsync(() => rems.SelectMany(rem =>
        {
            var dictionary = CosmoResources.Radius(rem.Key);
            var px = rem.Value * 16;
            var errors = new List<string>();
            if (Resource(dictionary, "CosmoRadius", ThemeVariant.Default) is not double radius || Math.Abs(radius - px) > 1e-9)
            {
                errors.Add($"{rem.Key}: CosmoRadius");
            }

            errors.AddRange(steps.Where(step => Resource(dictionary, "CosmoRadius" + step.Step, ThemeVariant.Default) is not CornerRadius corner
                    || Math.Abs(corner.TopLeft - Math.Round(px * step.Factor, 2)) > 1e-9 || corner.TopLeft != corner.BottomRight)
                .Select(step => $"{rem.Key}: CosmoRadius{step.Step}"));
            return errors;
        }).ToList());
        var standard = await desktop.RunAsync(() => steps.Take(4).Select(step => ((CornerRadius)Resource(CosmoResources.Radius(CosmoRadius.Default), "CosmoRadius" + step.Step, ThemeVariant.Default)!).TopLeft).ToList());

        Assert.Empty(wrong);
        Assert.Equal([6.0, 8.0, 10.0, 14.0], standard);
    }

    [Fact]
    [Trait("AC", "TOKENS-04")]
    public async Task TheComponentBrushesAndShadowsCarryShadcnsOpacitiesAsync()
    {
        var themes = await ReferenceMetrics.ThemesAsync();
        var spec = new (string Key, string Light, double LightAlpha, string Dark, double DarkAlpha)[]
        {
            ("ButtonPrimaryHover", "primary", 0.9, "primary", 0.9), ("ButtonDestructive", "destructive", 1, "destructive", 0.6),
            ("ButtonDestructiveHover", "destructive", 0.9, "destructive", 0.6), ("ButtonOutline", "background", 1, "input", 0.3),
            ("ButtonOutlineBorder", "border", 1, "input", 1), ("ButtonOutlineHover", "accent", 1, "input", 0.5),
            ("ButtonSecondaryHover", "secondary", 0.8, "secondary", 0.8), ("ButtonGhostHover", "accent", 1, "accent", 0.5),
            ("FocusRing", "ring", 0.5, "ring", 0.5), ("BadgeDestructive", "destructive", 1, "destructive", 0.6),
        };

        var wrong = await desktop.RunAsync(() =>
        {
            var errors = new List<string>();
            foreach (var color in Enum.GetValues<CosmoColor>())
            {
                var dictionary = CosmoResources.Colors(color);
                var palette = themes[color.ToString().ToLowerInvariant()];
                foreach (var (key, light, lightAlpha, dark, darkAlpha) in spec)
                {
                    foreach (var (variant, name, token, alpha) in new[] { (ThemeVariant.Light, "light", light, lightAlpha), (ThemeVariant.Dark, "dark", dark, darkAlpha) })
                    {
                        var basis = CssColor.Parse(palette[name][token]);
                        var expected = basis with { A = Math.Round(basis.A * alpha * 255) / 255 };
                        var found = Windows.Paint(Resource(dictionary, $"Cosmo{key}Brush", variant) as IBrush);
                        if (!found.Near(expected, 0))
                        {
                            errors.Add($"{color} {name} {key}: {found}, expected {expected}");
                        }
                    }
                }

                foreach (var (variant, name) in Variants)
                {
                    var ring = CssColor.Parse(palette[name]["ring"]);
                    var focus = (BoxShadows)Resource(dictionary, "CosmoFocusRingShadow", variant)!;
                    if (focus.Count != 1 || focus[0].Spread != 3 || focus[0].Blur != 0 || focus[0].Color != Color.FromArgb(128, (byte)ring.R, (byte)ring.G, (byte)ring.B))
                    {
                        errors.Add($"{color} {name} CosmoFocusRingShadow: {focus}");
                    }

                    if (Resource(dictionary, "CosmoShadowXs", variant)?.ToString() != BoxShadows.Parse("0 1 2 0 #0D000000").ToString()
                        || Resource(dictionary, "CosmoShadowSm", variant)?.ToString() != BoxShadows.Parse("0 1 3 0 #1A000000, 0 1 2 -1 #1A000000").ToString())
                    {
                        errors.Add($"{color} {name}: shadow-xs or shadow-sm");
                    }
                }
            }

            return errors;
        });

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TOKENS-05")]
    public async Task EveryColorDictionaryHoldsTheSameKeysAsync()
    {
        var sets = await desktop.RunAsync(() => Enum.GetValues<CosmoColor>()
            .SelectMany(color => Variants.Select(variant => (Name: $"{color} {variant.Name}", Keys: ((ResourceDictionary)CosmoResources.Colors(color).ThemeDictionaries[variant.Variant]).Keys.Select(key => key.ToString()!).Order(StringComparer.Ordinal).ToList())))
            .ToList());

        var reference = sets[0].Keys;
        Assert.Equal(24, sets.Count);
        Assert.True(reference.Count >= Tokens.Length * 2, string.Join(", ", reference));
        Assert.Empty(sets.Where(set => !set.Keys.SequenceEqual(reference)).Select(set => $"{set.Name}: {string.Join(", ", set.Keys.Except(reference).Concat(reference.Except(set.Keys)))}"));
    }
}
