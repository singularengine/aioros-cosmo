using System.ComponentModel;
using Aioros.Cosmo.Tables;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Gallery.Models;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Composites;

public sealed class DataTableTests(DesktopSession desktop)
{
    private sealed class Row(string email) : IDataTableRow
    {
        private bool _selected;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Email { get; } = email;

        public bool IsSelected
        {
            get => _selected;
            set
            {
                _selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    [Fact]
    [Trait("AC", "DATATABLE-03")]
    public async Task TheTableAndItsRowsAreShadcnsAsync()
    {
        var wrong = await Looks.FixtureAsync(desktop, "data-table-demo", (window, root, reference, theme) => Check(root, reference, $"{theme} demo"));
        wrong.AddRange(await Looks.FixtureAsync(desktop, "data-table-selected", (window, root, reference, theme) =>
        {
            DisplayFixtures.Open(window, "data-table-selected");
            var errors = Check(root, reference, $"{theme} selected");
            var rows = All<TableViewRow>(root);
            var emails = rows.Select(row => ((Payment)row.DataContext!).Email.ToLowerInvariant().Split('@')[0]);
            errors.AddRange(emails.SequenceEqual(["abe45", "carmella", "ken99", "monserrat44", "silas22"]) ? [] : [$"{theme}: {string.Join(',', emails)}"]);
            var muted = rows.Select(row => Windows.Paint(Windows.Part<Border>(row, "Border").Background) == Windows.Paint((IBrush)window.FindResource(window.ActualThemeVariant, "CosmoMutedBrush")!)).ToList();
            errors.AddRange(muted.SequenceEqual([true, false, true, false, false]) ? [] : [$"{theme}: muted rows {string.Join(',', muted)}"]);
            var header = All<CheckBox>(root)[0];
            var summary = All<TextBlock>(root).Last(block => block.Text?.EndsWith("row(s) selected.", StringComparison.Ordinal) == true).Text;
            errors.AddRange(header.IsChecked is null && summary == "2 of 5 row(s) selected." ? [] : [$"{theme}: header {header.IsChecked}, {summary}"]);
            return errors;
        }));

        Assert.Empty(wrong);
    }

    private static List<string> Check(Control root, IReadOnlyList<ElementMetrics> reference, string what)
    {
        var errors = Near($"{what} filter", Windows.Box(All<TextBox>(root).Single(), root), reference.Single(element => element.Slot == "input"), 1.5).ToList();
        var buttons = All<Button>(root);
        errors.AddRange(Near($"{what} columns", Windows.Box(buttons.First(button => button.Classes.Contains("outline") && !button.Classes.Contains("sm")), root), reference.Single(element => element.Slot == "dropdown-menu-trigger" && element.Text == "Columns"), 1.5));
        errors.AddRange(Near($"{what} table", Windows.Box(All<TableView>(root).Single(), root), reference.Single(element => element.Slot == "table-container"), 1.5));
        var rows = All<TableViewRow>(root);
        var expectedRows = reference.Where(element => element.Slot == "table-row").Skip(1).ToList();
        errors.AddRange(rows.Count == 5 ? rows.Zip(expectedRows).SelectMany((pair, i) => Near($"{what} row {i}", Windows.Box(pair.First, root), pair.Second, 1.5)) : [$"{what}: {rows.Count} rows"]);
        var header = All<TableViewColumnHeader>(root);
        errors.AddRange(header.Count > 0 && header[0].Bounds.Height == 40 ? [] : [$"{what}: header {header.FirstOrDefault()?.Bounds}"]);
        var boxes = All<CheckBox>(root);
        var expectedBoxes = reference.Where(element => element.Slot == "checkbox").ToList();
        errors.AddRange(boxes.Count == expectedBoxes.Count ? boxes.Zip(expectedBoxes).SelectMany((pair, i) => Near($"{what} checkbox {i}", Windows.Box(Windows.Part<Border>(pair.First, "PART_Border"), root), pair.Second, 1.5)) : [$"{what}: {boxes.Count} checkboxes"]);
        errors.AddRange(Near($"{what} sort", Windows.Box(buttons.Single(button => button.Classes.Contains("ghost") && button.Classes.Contains("has-icon")), root), reference.Single(element => element.Slot == "button" && element.Text == "Email"), 1.5));
        var actions = buttons.Where(button => button.Classes.Contains("icon-sm")).ToList();
        errors.AddRange(actions.Zip(reference.Where(element => element.Slot == "dropdown-menu-trigger" && element.Text == "Open menu")).SelectMany((pair, i) => Near($"{what} actions {i}", Windows.Box(pair.First, root), pair.Second, 1.5)));
        var pager = buttons.Where(button => button.Classes.Contains("sm") && button.Classes.Contains("outline")).ToList();
        errors.AddRange(pager.Zip(reference.Where(element => element.Slot == "button" && element.Text is "Previous" or "Next")).SelectMany((pair, i) => Near($"{what} pager {i}", Windows.Box(pair.First, root), pair.Second, 1.5)
            .Concat(pair.First.IsEffectivelyEnabled ? [$"{what} pager {i} enabled"] : [])));
        return errors;
    }

    [Fact]
    [Trait("AC", "DATATABLE-04")]
    public void TheSourceFiltersSortsPagesAndSelectsAsTanStack()
    {
        var source = new DataTableSource<Row>(Enumerable.Range(1, 12).Select(n => new Row(n % 3 == 0 ? $"USER{n}@example.com" : $"user{n}@example.com"))) { PageSize = 5, FilterColumn = "email" };
        source.Column("email", "Email", row => row.Email);
        source.FilterText = "1";
        var filtered = (source.FilteredCount, source.PageIndex, source.NextPageCommand.CanExecute(null));
        source.SortCommand.Execute("email");
        var ascending = string.Join(',', source.Rows.Select(row => row.Email.Split('@')[0].ToLowerInvariant()));
        source.SortCommand.Execute("email");
        var descending = string.Join(',', source.Rows.Select(row => row.Email.Split('@')[0].ToLowerInvariant()));
        source.FilterText = string.Empty;
        source.SortDescending = false;
        var pages = (source.FilteredCount, source.PageCount, source.PreviousPageCommand.CanExecute(null));
        source.Rows[0].IsSelected = true;
        source.Rows[1].IsSelected = true;
        var mixed = source.AllPageRowsSelected;
        source.NextPageCommand.Execute(null);
        source.Rows[0].IsSelected = true;
        var counted = source.SelectedCount;
        source.AllPageRowsSelected = false;
        var fromMixed = (source.AllPageRowsSelected, source.Rows.All(row => row.IsSelected));
        source.AllPageRowsSelected = false;
        var cleared = source.AllPageRowsSelected;
        source.NextPageCommand.Execute(null);
        var last = (source.PageIndex, source.NextPageCommand.CanExecute(null), source.Rows.Count);

        Assert.Equal(((4, 0, false), "user1,user10,user11,user12", "user12,user11,user10,user1", (12, 3, false), (bool?)null, 3, ((bool?)true, true), (bool?)false, (2, false, 2)), (filtered, ascending, descending, pages, mixed, counted, fromMixed, cleared, last));
    }

    [Fact]
    [Trait("AC", "DATATABLE-05")]
    public async Task TheBoundTableFollowsTheModelAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var view = DisplayFixtures.Build("data-table-demo");
            var window = Windows.Show(new Border { Margin = new Thickness(24), Width = 592, Child = view, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top }, ThemeVariant.Light);
            var model = (PaymentTable)view.DataContext!;
            var boxes = view.GetVisualDescendants().OfType<CheckBox>().ToList();
            Windows.Click(window, boxes[2]);
            var row = view.GetVisualDescendants().OfType<TableViewRow>().ElementAt(1);
            var selected = (model.Source.Items[1].IsSelected, row.Classes.Contains("selected"), Windows.Paint(Windows.Part<Border>(row, "Border").Background) == Windows.Paint((IBrush)window.FindResource(ThemeVariant.Light, "CosmoMutedBrush")!));
            Windows.Click(window, boxes[0]);
            var all = model.Source.Items.All(payment => payment.IsSelected);
            Windows.Part<TextBox>(view).Focus();
            Windows.Type(window, "abe");
            var filtered = model.Source.Rows.Count;
            Windows.Click(window, view.GetVisualDescendants().OfType<Button>().First(button => button.Classes.Contains("outline")));
            var status = window.GetVisualDescendants().OfType<MenuItem>().First(item => (string?)item.Header == "Status");
            Windows.Click(window, status);
            var hidden = !view.GetVisualDescendants().OfType<TableView>().Single().Columns[1].IsVisible;
            Windows.Away(window, new Point(790, 1190));
            model.Source.FilterText = string.Empty;
            Windows.Settle();
            Windows.Click(window, view.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("ghost") && button.Classes.Contains("has-icon")));
            var sorted = model.Source.Rows[0].Email;
            window.Close();
            return (selected, all, filtered, hidden, sorted);
        });

        Assert.Equal(((true, true, true), true, 1, true, "Abe45@example.com"), found);
    }

    [Fact]
    [Trait("AC", "DATATABLE-06")]
    public void TheSourceGoesToTheFirstAndLastPagesAndCountsPagesFromOne()
    {
        var source = new DataTableSource<Row>(Enumerable.Range(1, 68).Select(n => new Row($"user{n}@example.com"))) { PageSize = 10 };
        source.Column("email", "Email", row => row.Email);
        source.LastPageCommand.Execute(null);
        var last = (source.PageIndex, source.PageNumber, source.LastPageCommand.CanExecute(null), source.NextPageCommand.CanExecute(null));
        source.FirstPageCommand.Execute(null);
        var first = (source.PageNumber, source.FirstPageCommand.CanExecute(null), source.PreviousPageCommand.CanExecute(null));
        source.Items.Move(0, 2);
        var moved = string.Join(",", source.Rows.Take(3).Select(row => row.Email[..5]));
        source.LastPageCommand.Execute(null);
        source.FilterText = string.Empty;
        var kept = source.PageIndex;
        source.SortCommand.Execute("email");
        var sorted = source.PageIndex;

        Assert.Equal((6, 0), (kept, sorted));
        Assert.Equal((6, 7, false, false), last);
        Assert.Equal((1, false, false), first);
        Assert.Equal("user2,user3,user1", moved);
    }

    [Fact]
    [Trait("AC", "DATATABLE-07")]
    public void TheSourcesStateIsWrittenAndPutBackAsText()
    {
        static DataTableSource<Row> Payments() =>
            new DataTableSource<Row>(Enumerable.Range(1, 12).Select(n => new Row($"user{n}@example.com"))) { PageSize = 5, FilterColumn = "email" }
                .Column("email", "Email", row => row.Email).Column("status", "Status", row => row.Email.Length);
        static string Shown(DataTableSource<Row> source) =>
            $"{source.PageIndex} {source.PageSize} {source.SortColumn} {source.SortDescending} {source.FilterText} {string.Join(",", source.Columns.Where(column => !column.IsVisible).Select(column => column.Key))} "
            + $"{string.Join(",", source.Rows.Select(row => row.Email[..6]))} {source.SelectedCount} {string.Join(",", source.Items.Take(3).Select(row => row.Email[..6]))}";
        var source = Payments();
        source.FilterText = "a";
        source.SortCommand.Execute("email");
        source.SortCommand.Execute("email");
        source.Columns[1].IsVisible = false;
        source.Items[0].IsSelected = true;
        source.Items[6].IsSelected = true;
        source.Items.Move(2, 0);
        source.NextPageCommand.Execute(null);
        var copy = Payments();
        copy.State = source.State;
        var broken = Payments();
        broken.State = "page=x;size=3;bogus=1;selected=1,99;order=0,0,1;hidden=status;sort=nothing";

        Assert.Equal("page=1;size=5;sort=email;desc=1;filter=a;hidden=status;selected=0,6;order=2,0,1,3,4,5,6,7,8,9,10,11", source.State);
        Assert.Equal("1 5 email True a status user7@,user6@,user5@,user4@,user3@ 2 user3@,user1@,user2@", Shown(source));
        Assert.Equal(Shown(source), Shown(copy));
        Assert.Equal("0 3  False  status user1@,user2@,user3@ 1 user1@,user2@,user3@", Shown(broken));
    }
}
