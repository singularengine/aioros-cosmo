using Aioros.Cosmo.Display;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Binding;

public sealed class BindingTests(DesktopSession desktop)
{
    [Fact]
    [Trait("AC", "BINDING-02")]
    public async Task AClickRunsTheBoundCommandWithItsParameterAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new BindingModel();
            var view = new BindingView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            Windows.Click(window, view.FindControl<Button>("AddFive")!);
            var header = view.FindControl<CardHeader>("Header")!;
            var title = header.GetVisualDescendants().OfType<TextBlock>().First().Text;
            var plan = view.FindControl<Badge>("Plan")!.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text).ToList();
            window.Close();
            return (model.Runs, model.LastParameter, title, plan);
        });

        Assert.Equal(1, found.Runs);
        Assert.Equal("5", found.LastParameter);
        Assert.Equal("5 seats", found.title);
        Assert.Equal(["Pro", "$12 per seat"], found.plan);
    }

    [Fact]
    [Trait("AC", "BINDING-06")]
    public async Task ConvertersChooseTheBadgesVariantAndVisibilityAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new BindingModel();
            var view = new BindingView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var badge = view.FindControl<Badge>("Limit")!;
            var states = new List<(bool Destructive, CssColor Background, bool Visible)>();
            void Read() => states.Add((badge.Classes.Contains("destructive"), Windows.Paint(badge.Background), badge.IsVisible));
            Read();
            model.Count = 9;
            Windows.Settle();
            Read();
            model.Count = 2;
            model.Note = null;
            Windows.Settle();
            Read();
            model.Note = "back";
            Windows.Settle();
            Read();
            window.Close();
            return states;
        });

        Assert.Equal(
            [(false, CssColor.Parse("#171717"), true), (true, CssColor.Parse("#E7000B"), true), (false, CssColor.Parse("#171717"), false), (false, CssColor.Parse("#171717"), true)],
            found);
    }
}
