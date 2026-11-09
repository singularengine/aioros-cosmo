using Aioros.Cosmo.Display;
using Aioros.Cosmo.Navigation;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Display;

public sealed class StructureBindingTests(DesktopSession desktop)
{
    private static T Named<T>(Control view, string name) where T : Control => view.FindControl<T>(name)!;

    private Task<T> ViewAsync<T>(Func<Window, StructureView, StructureModel, T> act) => desktop.RunAsync(() =>
    {
        var model = new StructureModel();
        var view = new StructureView { DataContext = model };
        var window = Windows.Show(view, ThemeVariant.Light);
        var result = act(window, view, model);
        window.Close();
        return result;
    });

    [Fact]
    [Trait("AC", "SCROLLAREA-03")]
    public async Task TheAreasOffsetFollowsTheModelBothWaysAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var area = Named<ScrollViewer>(view, "Tags");
            Windows.Hover(window, area);
            var thumb = Windows.Part<ScrollBar>(area, "PART_VerticalScrollBar").GetVisualDescendants().OfType<Thumb>().Single();
            var grip = Windows.Center(thumb);
            window.MouseDown(grip, MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(grip + new Point(0, 30), RawInputModifiers.LeftMouseButton);
            window.MouseUp(grip + new Point(0, 30), MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            var scrolled = (model.Offset.Y > 0, model.Offset == area.Offset);
            Windows.Click(window, Named<Button>(view, "Top"));
            return (scrolled, area.Offset.Y);
        });

        Assert.Equal(((true, true), 0d), found);
    }

    [Fact]
    [Trait("AC", "TABLE-03")]
    public async Task TheBoundTableFollowsTheModelsSelectionAndRowsAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var table = Named<TableView>(view, "Invoices");
            var rows = table.GetVisualDescendants().OfType<TableViewRow>().ToList();
            Windows.Click(window, rows[2]);
            var clicked = model.Selected?.Number;
            model.Selected = model.Invoices[0];
            Windows.Settle();
            var selected = rows[0].IsSelected;
            var badges = table.GetVisualDescendants().OfType<Badge>().Select(badge => badge.Classes.Contains("outline")).ToList();
            Windows.Click(window, Named<Button>(view, "AddInvoice"));
            return (clicked, selected, badges[0], badges[1], table.GetVisualDescendants().OfType<TableViewRow>().Count(), Named<TextBlock>(view, "Total").Text);
        });

        Assert.Equal(("INV003", true, true, false, 8, "$2,350.00"), found);
    }

    [Fact]
    [Trait("AC", "NAVMENU-04")]
    public async Task ABoundLinkRunsTheModelsCommandAndClosesTheContentAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var trigger = Named<NavigationMenuTrigger>(view, "Docs");
            Windows.Click(window, trigger);
            var links = window.GetVisualDescendants().OfType<Button>().Where(button => button.Classes.Contains("navigation-menu-link")).ToList();
            Windows.Click(window, links[1]);
            return (links.Count, model.Current, Named<TextBlock>(view, "Current").Text, trigger.Flyout!.IsOpen);
        });

        Assert.Equal((3, "/docs/installation", "/docs/installation", false), found);
    }

    [Fact]
    [Trait("AC", "MENUBAR-04")]
    public async Task TheMenubarsItemsRunCommandsAndFollowTheModelsFlagAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var bar = Named<Menu>(view, "Bar");
            var view_ = bar.Items.OfType<MenuItem>().ElementAt(1);
            Windows.Click(window, view_);
            var bookmarks = Named<MenuItem>(view, "Bookmarks");
            Windows.Click(window, bookmarks);
            var set = model.Bookmarks;
            model.Bookmarks = false;
            Windows.Settle();
            var cleared = bookmarks.IsChecked;
            Windows.Click(window, bar.Items.OfType<MenuItem>().First());
            Windows.Click(window, Named<MenuItem>(view, "Print"));
            return (set, cleared, string.Join(",", model.Actions));
        });

        Assert.Equal((true, false, "Print"), found);
    }
}
