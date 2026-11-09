using Aioros.Cosmo.Charts;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Pictor;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;

namespace Aioros.Cosmo.Tests.Charts;

public sealed class ChartContentTests(DesktopSession desktop)
{
    private static List<Border> Indicators(Control root) =>
        [.. root.GetLogicalDescendants().Prepend(root).OfType<Border>().Where(static border => border.Width is 10 or 4 or 0)];

    [Fact]
    [Trait("AC", "CHART-02")]
    public async Task TheTooltipContentIsShadcnsBoxWithEachIndicatorAsync()
    {
        var (box, items, dots, lines, dashes, none, labels) = await desktop.RunAsync(() =>
        {
            var dotContent = new ChartTooltipContent();
            var lineContent = new ChartTooltipContent { Indicator = ChartIndicator.Line };
            var dashedContent = new ChartTooltipContent { Indicator = ChartIndicator.Dashed };
            var hiddenContent = new ChartTooltipContent { HideIndicator = true, HideLabel = true, Indicator = ChartIndicator.Line };
            var dot = (Border)dotContent.Build(null)!;
            var unlabelled = new ChartTooltipContent { HideLabel = true, Indicator = ChartIndicator.Line }.Build(null)!;
            static List<TooltipItems> Items(Control root) => [.. root.GetLogicalDescendants().OfType<TooltipItems>()];
            return (
                (dot.MinWidth, dot.Padding, dot.CornerRadius.TopLeft, dot.BorderThickness.Left, dot.BoxShadow.Count),
                new[] { dot, lineContent.Build(null)!, dashedContent.Build(null)!, hiddenContent.Build(null)! }.Select(static root => Items(root).Select(static items => items.Spacing).ToList()).ToList(),
                Indicators(dotContent.ItemTemplate.Build(null)!).Select(static border => (border.Width, border.Height, border.CornerRadius.TopLeft, border.VerticalAlignment)).ToList(),
                Indicators(lineContent.Build(null)!).Concat(Indicators(lineContent.ItemTemplate.Build(null)!)).Select(static border => (border.Width, border.CornerRadius.TopLeft)).ToList(),
                Indicators(dashedContent.Build(null)!).Concat(Indicators(dashedContent.ItemTemplate.Build(null)!)).Select(static border => (border.Width, border.BorderThickness.Left, SvgElement.GetBorderStyle(border), border.Background is null, border.CornerRadius.TopLeft)).ToList(),
                Indicators(hiddenContent.ItemTemplate.Build(null)!).Count,
                unlabelled.GetLogicalDescendants().OfType<TextBlock>().Where(static text => text.Name is "PART_Label" or "PART_NestedLabel").Select(static text => text.IsVisible).ToList());
        });

        Assert.Equal((128.0, new Avalonia.Thickness(10, 6), 8.0, 1.0, 2), box);
        Assert.All(items, found => Assert.Equal([6.0], found));
        Assert.Equal([(10.0, 10.0, 2.0, VerticalAlignment.Center)], dots);
        Assert.Equal([(4.0, 2.0), (4.0, 2.0)], lines);
        Assert.Equal([(0.0, 1.5, "dashed", true, 2.0), (0.0, 1.5, "dashed", true, 2.0)], dashes);
        Assert.Equal(0, none);
        Assert.Equal([false, false], labels);
    }

    [Fact]
    [Trait("AC", "CHART-12")]
    public async Task APlainLegendTakesShadcnsLegendUnderTheThemeAsync()
    {
        var (plain, own, none) = await desktop.RunAsync(() =>
        {
            var template = new ChartLegendContent { Spacing = 8 };
            var (first, second, third) = (new Legend(), new Legend { Template = template }, new Legend { Template = null, ContentTemplate = null });
            var window = Windows.Show(new StackPanel { Children = { Chart(first), Chart(second), Chart(third) } }, Avalonia.Styling.ThemeVariant.Light);
            var found = ((first.Template is ChartLegendContent, first.ContentTemplate is ChartLegendItem), ReferenceEquals(second.Template, template), (third.Template, third.ContentTemplate));
            window.Close();
            return found;

            static PieChart Chart(Legend legend) => new() { Width = 200, Height = 200, Elements = { legend } };
        });

        Assert.Equal((true, true), plain);
        Assert.True(own);
        Assert.Equal((null, null), none);
    }

    [Fact]
    [Trait("AC", "CHART-03")]
    public async Task TheLegendContentIsARowOfSquaresAsync()
    {
        var (frame, items, entry, square) = await desktop.RunAsync(() =>
        {
            var root = (Border)new ChartLegendContent { Spacing = 8, ItemBasis = 0.25 }.Build(null)!;
            var plain = (LegendItems)((Border)new ChartLegendContent().Build(null)!).Child!;
            var row = (StackPanel)new ChartLegendItem().Build(null)!;
            var mark = row.Children.OfType<Border>().Single();
            var placed = (LegendItems)root.Child!;
            return (
                (root.Padding.Top, placed.Orientation, placed.Spacing, placed.ItemBasis),
                (plain.Spacing, double.IsNaN(plain.ItemBasis)),
                (row.Orientation, row.Spacing),
                (mark.Width, mark.Height, mark.CornerRadius.TopLeft));
        });

        Assert.Equal((12.0, Orientation.Horizontal, 8.0, 0.25), frame);
        Assert.Equal((16.0, true), items);
        Assert.Equal((Orientation.Horizontal, 6.0), entry);
        Assert.Equal((8.0, 8.0, 2.0), square);
    }

    [Fact]
    [Trait("AC", "CHART-10")]
    public async Task AChartConfigNamesAndColorsTheSeriesByDataKeyAsync()
    {
        var (light, dark, bars, line, area, own, other, pie) = await desktop.RunAsync(() =>
        {
            var config = new ChartConfig
            {
                new ChartConfigItem { Key = "Desktop", Label = "Desktop", Color = "PictorTickText" },
                new ChartConfigItem { Key = "Mobile", Label = "Mobile", Color = "#ff0000" },
            };
            var bar = new Bar { DataKey = "Desktop" };
            var mobile = new Bar { DataKey = "Mobile" };
            var mine = new Bar { DataKey = "Desktop", Name = "Mine", Fill = "#00ff00" };
            var stranger = new Bar { DataKey = "Visitors" };
            var barChart = new BarChart { Width = 300, Height = 200 };
            barChart.Elements.AddRange([bar, mobile, mine, stranger]);
            ChartConfig.SetConfig(barChart, config);
            var lineSeries = new Line { DataKey = "Desktop" };
            var lineChart = new LineChart { Width = 300, Height = 200 };
            ChartConfig.SetConfig(lineChart, config);
            lineChart.Elements.Add(lineSeries);
            var areaSeries = new Area { DataKey = "Desktop" };
            var areaChart = new AreaChart { Width = 300, Height = 200 };
            ChartConfig.SetConfig(areaChart, config);
            areaChart.Elements.Add(areaSeries);
            var slices = new Pie { DataKey = "Desktop" };
            var pieChart = new PieChart { Width = 300, Height = 200 };
            ChartConfig.SetConfig(pieChart, config);
            pieChart.Elements.Add(slices);
            var window = Windows.Fixture(new StackPanel { Children = { barChart, lineChart, areaChart, pieChart } }, 900, Avalonia.Styling.ThemeVariant.Light);
            var lightPalette = window.FindResource(Avalonia.Styling.ThemeVariant.Light, "PictorTickText");
            var found = (bar.Name, bar.Fill);
            window.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
            var darkPalette = window.FindResource(Avalonia.Styling.ThemeVariant.Dark, "PictorTickText");
            var result = (
                (lightPalette, found),
                (darkPalette, bar.Fill),
                (mobile.Name, mobile.Fill),
                (lineSeries.Name, lineSeries.Stroke, lineSeries.IsSet(Series.FillProperty)),
                (areaSeries.Fill, areaSeries.Stroke),
                (mine.Name, mine.Fill),
                (stranger.Name, stranger.IsSet(Series.FillProperty)),
                slices.IsSet(Pie.NameProperty));
            window.Close();
            return result;
        });

        Assert.Equal(light.Item1, light.found.Fill);
        Assert.Equal("Desktop", light.found.Name);
        Assert.NotEqual(light.Item1, dark.Item1);
        Assert.Equal(dark.Item1, dark.Item2);
        Assert.Equal(("Mobile", (object?)"#ff0000"), bars);
        Assert.Equal(("Desktop", dark.Item1, false), line);
        Assert.Equal((dark.Item1, dark.Item1), area);
        Assert.Equal(("Mine", (object?)"#00ff00"), own);
        Assert.Equal(((string?)null, false), other);
        Assert.False(pie);
    }
}

