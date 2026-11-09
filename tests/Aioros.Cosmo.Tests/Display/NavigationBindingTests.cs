using Aioros.Cosmo.Navigation;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Display;

public sealed class NavigationBindingTests(DesktopSession desktop)
{
    private static T Named<T>(Control view, string name) where T : Control => view.FindControl<T>(name)!;

    private static List<string> Texts(Control control) => [.. control.GetVisualDescendants().OfType<TextBlock>().Where(text => text.IsEffectivelyVisible).Select(text => text.Text ?? string.Empty)];

    private Task<T> ViewAsync<T>(Func<Window, NavigationView, NavigationModel, T> act) => desktop.RunAsync(() =>
    {
        var model = new NavigationModel();
        var view = new NavigationView { DataContext = model };
        var window = Windows.Show(view, ThemeVariant.Light);
        var result = act(window, view, model);
        window.Close();
        return result;
    });

    [Fact]
    [Trait("AC", "TABS-04")]
    public async Task BoundTabsFollowTheModelBothWaysAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var tabs = Named<TabControl>(view, "Tabs");
            model.SelectedTab = 1;
            Windows.Settle();
            var page = Texts(Windows.Part<Avalonia.Controls.Presenters.ContentPresenter>(tabs, "PART_SelectedContentHost")).Single();
            Windows.Click(window, tabs.GetVisualDescendants().OfType<TabItem>().First());
            return (page, model.SelectedTab, Texts(tabs.GetVisualDescendants().OfType<TabItem>().First()).Single());
        });

        Assert.Equal(("Your password.", 0, "Account"), found);
    }

    [Fact]
    [Trait("AC", "ACCORDION-05")]
    public async Task BoundQuestionsOpenWithTheModelAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var accordion = Named<Accordion>(view, "Questions");
            model.Questions[2].Open = true;
            Windows.Settle();
            var items = accordion.GetVisualDescendants().OfType<Expander>().ToList();
            var answer = Texts(items[2].Presenter!).Single();
            Windows.Click(window, Windows.Part<ToggleButton>(items[0], "ExpanderHeader"));
            return (answer, items.Select(item => item.IsExpanded).ToList(), model.Questions.Select(question => question.Open).ToList(), Texts(Windows.Part<ToggleButton>(items[0], "ExpanderHeader")).Single());
        });

        Assert.Equal("Yes, by default.", found.answer);
        Assert.Equal([true, false, false], found.Item2);
        Assert.Equal([true, false, false], found.Item3);
        Assert.Equal("Is it accessible?", found.Item4);
    }

    [Fact]
    [Trait("AC", "COLLAPSIBLE-03")]
    public async Task TheCollapsibleFollowsTheModelBothWaysAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var collapsible = Named<Expander>(view, "Starred");
            model.Open = true;
            Windows.Settle();
            var shown = collapsible.Presenter!.IsVisible;
            Windows.Part<ToggleButton>(collapsible, "ExpanderHeader").Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Space);
            return (shown, collapsible.Presenter.IsVisible, model.Open);
        });

        Assert.Equal((true, false, false), found);
    }

    [Fact]
    [Trait("AC", "BREADCRUMB-03")]
    public async Task BoundCrumbsGrowWithTheirSeparatorsAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var breadcrumb = Named<Breadcrumb>(view, "Crumbs");
            List<(bool, string)> Crumbs() => [.. breadcrumb.GetVisualDescendants().OfType<BreadcrumbItem>().Select(item => (Windows.Part<Avalonia.Controls.Presenters.ContentPresenter>(item, "PART_Separator").IsVisible, Texts(item)[^1]))];
            var before = Crumbs();
            model.Deeper.Execute(null);
            Windows.Settle();
            return (before, Crumbs(), Texts(breadcrumb.GetVisualDescendants().OfType<BreadcrumbItem>().Last())[0]);
        });

        Assert.Equal([(false, "Home"), (true, "Components")], found.before);
        Assert.Equal([(false, "Home"), (true, "Components"), (true, "Level 2")], found.Item2);
        Assert.Equal("/", found.Item3);
    }

    [Fact]
    [Trait("AC", "PAGINATION-03")]
    public async Task BoundPagesFollowTheModelAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            Windows.Click(window, Named<PaginationNext>(view, "Next"));
            var afterNext = model.Page;
            var page4 = Named<Pagination>(view, "Pages").GetVisualDescendants().OfType<Button>().Single(button => button.Content is 4);
            Windows.Click(window, page4);
            Windows.Settle();
            var outlined = Named<Pagination>(view, "Pages").GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("outline")).Content;
            return (afterNext, model.Page, outlined);
        });

        Assert.Equal((3, 4, (object)4), found);
    }

    [Fact]
    [Trait("AC", "BUTTONGROUP-03")]
    public async Task TheGroupsButtonsRunTheirCommandsAndKeepTheirCornersAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            Windows.Click(window, Named<Button>(view, "Report"));
            var classes = (Named<Button>(view, "Archive").Classes.Contains("secondary"), Named<Button>(view, "Report").Classes.Contains("secondary"));
            model.SnoozeShown = false;
            Windows.Settle();
            return (model.Ran.ToList(), classes, Named<Button>(view, "Report").CornerRadius);
        });

        Assert.Equal(["Report"], found.Item1);
        Assert.Equal((false, true), found.classes);
        Assert.Equal(new Avalonia.CornerRadius(0), found.Item3);
    }
}
