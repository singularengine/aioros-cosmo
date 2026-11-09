using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Aioros.Cosmo.Tables;

/// <summary>A row of a <see cref="DataTableSource{T}"/>: its selection, which the table's checkboxes bind two way (TanStack's row selection).</summary>
public interface IDataTableRow : INotifyPropertyChanged
{
    /// <summary>Whether the row is selected.</summary>
    bool IsSelected { get; set; }
}

/// <summary>A column of a <see cref="DataTableSource{T}"/>: its key, header, visibility (two way, for a Columns menu) and whether it sorts and hides.</summary>
public sealed class DataTableColumn(string key, string header, bool canSort, bool canHide) : INotifyPropertyChanged
{
    private bool _visible = true;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The column's key (TanStack's column id), the sort command's parameter.</summary>
    public string Key { get; } = key;

    /// <summary>The column's header text.</summary>
    public string Header { get; } = header;

    /// <summary>Whether the rows sort by this column.</summary>
    public bool CanSort { get; } = canSort;

    /// <summary>Whether the column can be hidden (TanStack's <c>enableHiding</c>).</summary>
    public bool CanHide { get; } = canHide;

    /// <summary>Whether the column is shown, two way.</summary>
    public bool IsVisible
    {
        get => _visible;
        set
        {
            if (_visible != value)
            {
                _visible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            }
        }
    }
}

/// <summary>
/// The state of shadcn/ui's data table (TanStack Table 9's row models) for a view model: the <see cref="Items"/> filtered by
/// <see cref="FilterText"/> on <see cref="FilterColumn"/> (case-insensitive containment, TanStack's <c>includesString</c>),
/// sorted by <see cref="SortColumn"/> (alphanumeric, case-insensitive; the sort command toggles ascending and descending
/// as shadcn's header button does) and paged by <see cref="PageSize"/> into <see cref="Rows"/>, which a
/// <c>TableView</c> shows; the selection lives on the rows (<see cref="IDataTableRow"/>), counted across pages.
/// </summary>
/// <typeparam name="T">The row type.</typeparam>
[Aioros.Markup.AnyThread]
public sealed class DataTableSource<T> : INotifyPropertyChanged where T : class, IDataTableRow
{
    private readonly Dictionary<string, Func<T, object?>> _values = new(StringComparer.Ordinal);
    private readonly List<DataTableColumn> _columns = [];
    private readonly List<T> _origin;
    private List<T> _filtered = [];
    private string? _filter;
    private string? _filterColumn;
    private string? _sortColumn;
    private bool _descending;
    private int _pageIndex;
    private int _pageSize = 10;
    private bool _paging;

    /// <summary>Starts a table over <paramref name="items"/>; add its columns with <see cref="Column"/>.</summary>
    public DataTableSource(IEnumerable<T> items)
    {
        Items = [.. items];
        _origin = [.. Items];
        Items.CollectionChanged += OnItemsChanged;
        foreach (var item in Items)
        {
            item.PropertyChanged += OnRowChanged;
        }

        SortCommand = new Act(parameter => Sort(parameter as string));
        PreviousPageCommand = new Act(_ => PageIndex--, () => CanPreviousPage);
        NextPageCommand = new Act(_ => PageIndex++, () => CanNextPage);
        FirstPageCommand = new Act(_ => PageIndex = 0, () => CanPreviousPage);
        LastPageCommand = new Act(_ => PageIndex = PageCount - 1, () => CanNextPage);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Every row, in the source order; adding and removing rows refreshes the table.</summary>
    public ObservableCollection<T> Items { get; }

    /// <summary>The rows of the current page, filtered and sorted.</summary>
    public ObservableCollection<T> Rows { get; } = [];

    /// <summary>The columns, in the order they were added.</summary>
    public IReadOnlyList<DataTableColumn> Columns => _columns;

    /// <summary>The columns a Columns menu lists (those that can hide).</summary>
    public IReadOnlyList<DataTableColumn> HideableColumns => [.. _columns.Where(column => column.CanHide)];

    /// <summary>Sorts by the column named by its parameter, ascending first, then descending, back and forth, from the first page (TanStack's <c>autoResetPageIndex</c>).</summary>
    public ICommand SortCommand { get; }

    /// <summary>Goes to the previous page; it cannot run on the first.</summary>
    public ICommand PreviousPageCommand { get; }

    /// <summary>Goes to the next page; it cannot run on the last.</summary>
    public ICommand NextPageCommand { get; }

    /// <summary>Goes to the first page; it cannot run on the first.</summary>
    public ICommand FirstPageCommand { get; }

    /// <summary>Goes to the last page; it cannot run on the last.</summary>
    public ICommand LastPageCommand { get; }

    /// <summary>The filter typed for <see cref="FilterColumn"/>.</summary>
    public string? FilterText
    {
        get => _filter;
        set
        {
            if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(_filter))
            {
                return;
            }

            if (Set(ref _filter, value))
            {
                _pageIndex = 0;
                Refresh();
            }
        }
    }

    /// <summary>The key of the column <see cref="FilterText"/> filters.</summary>
    public string? FilterColumn
    {
        get => _filterColumn;
        set
        {
            if (Set(ref _filterColumn, value))
            {
                Refresh();
            }
        }
    }

    /// <summary>The key of the sorting column, if any.</summary>
    public string? SortColumn
    {
        get => _sortColumn;
        set
        {
            if (Set(ref _sortColumn, value))
            {
                Refresh();
            }
        }
    }

    /// <summary>Whether the sort is descending.</summary>
    public bool SortDescending
    {
        get => _descending;
        set
        {
            if (Set(ref _descending, value))
            {
                Refresh();
            }
        }
    }

    /// <summary>The current page, from 0.</summary>
    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            var page = Math.Clamp(value, 0, Math.Max(0, PageCount - 1));
            if (Set(ref _pageIndex, page))
            {
                Refresh();
            }
        }
    }

    /// <summary>How many rows a page shows (TanStack's default, 10).</summary>
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (Set(ref _pageSize, Math.Max(1, value)))
            {
                Refresh();
            }
        }
    }

    /// <summary>
    /// The table's state as invariant text: the page, its size, the sort, the filter, the hidden columns, the selected rows
    /// and the rows' order, each row by its place among the rows the source started with. Aioros carries it across posts
    /// on the web through <see cref="DataTable.StateProperty"/>, so a post finds the rows its visitor saw; setting it puts
    /// the table back in that state, applying what parses and leaving the rest as it was.
    /// </summary>
    public string State
    {
        get => new DataTableState(_pageIndex, _pageSize, _sortColumn, _descending, _filter, [.. _columns.Where(column => !column.IsVisible).Select(column => column.Key)],
            [.. _origin.Select((row, index) => (row, index)).Where(pair => pair.row.IsSelected).Select(pair => pair.index)], Order()).Write();
        set
        {
            if (value != State)
            {
                Apply(DataTableState.Parse(value));
            }
        }
    }

    /// <summary>The current page counted from 1, as a pagination's "Page 1 of 7" shows it.</summary>
    public int PageNumber => PageIndex + 1;

    /// <summary>How many pages the filtered rows make (at least one).</summary>
    public int PageCount => Math.Max(1, (int)Math.Ceiling(_filtered.Count / (double)PageSize));

    /// <summary>Whether there is a page before the current one.</summary>
    public bool CanPreviousPage => PageIndex > 0;

    /// <summary>Whether there is a page after the current one.</summary>
    public bool CanNextPage => PageIndex < PageCount - 1;

    /// <summary>How many rows pass the filter.</summary>
    public int FilteredCount => _filtered.Count;

    /// <summary>How many of the filtered rows are selected (TanStack's filtered selected row model).</summary>
    public int SelectedCount => _filtered.Count(row => row.IsSelected);

    /// <summary>Whether no row passes the filter.</summary>
    public bool IsEmpty => _filtered.Count == 0;

    /// <summary>
    /// Whether every row of the page is selected (true), some are (null) or none (false), for the header's checkbox; setting it
    /// selects or clears the page's rows, and setting it from the mixed state selects them all, as Radix's checkbox does.
    /// </summary>
    public bool? AllPageRowsSelected
    {
        get => Rows.Count > 0 && Rows.All(row => row.IsSelected) ? true : Rows.Any(row => row.IsSelected) ? null : false;
        set
        {
            var select = AllPageRowsSelected is null || value == true;
            _paging = true;
            foreach (var row in Rows)
            {
                row.IsSelected = select;
            }

            _paging = false;
            Selection();
        }
    }

    /// <summary>Adds a column: its key, header, the value it sorts and filters by, and whether it sorts and hides.</summary>
    public DataTableSource<T> Column(string key, string header, Func<T, object?>? value = null, bool canSort = true, bool canHide = true)
    {
        if (value is not null)
        {
            _values[key] = value;
        }

        var column = new DataTableColumn(key, header, canSort && value is not null, canHide);
        column.PropertyChanged += (_, _) => OnPropertyChanged(nameof(State));
        _columns.Add(column);
        Refresh();
        return this;
    }

    /// <summary>TanStack's alphanumeric sort: case-insensitive, runs of digits compared as numbers.</summary>
    public static int CompareAlphanumeric(object? left, object? right)
    {
        if (left is IComparable && left is not string && left?.GetType() == right?.GetType())
        {
            return Comparer<object>.Default.Compare(left, right);
        }

        var a = Convert.ToString(left, CultureInfo.InvariantCulture) ?? string.Empty;
        var b = Convert.ToString(right, CultureInfo.InvariantCulture) ?? string.Empty;
        return CompareText(a.ToLowerInvariant(), b.ToLowerInvariant());
    }

    private static int CompareText(string a, string b)
    {
        var (i, j) = (0, 0);
        while (i < a.Length && j < b.Length)
        {
            if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
            {
                var (left, nextI) = Number(a, i);
                var (right, nextJ) = Number(b, j);
                if (left != right)
                {
                    return left.CompareTo(right);
                }

                (i, j) = (nextI, nextJ);
                continue;
            }

            if (a[i] != b[j])
            {
                return a[i].CompareTo(b[j]);
            }

            i++;
            j++;
        }

        return (a.Length - i).CompareTo(b.Length - j);
    }

    private static (decimal Value, int End) Number(string text, int start)
    {
        var end = start;
        while (end < text.Length && char.IsDigit(text[end]))
        {
            end++;
        }

        return (decimal.Parse(text[start..end], CultureInfo.InvariantCulture), end);
    }

    private void Sort(string? key)
    {
        if (key is null || !_values.ContainsKey(key))
        {
            return;
        }

        _descending = _sortColumn == key && !_descending;
        _sortColumn = key;
        _pageIndex = 0;
        OnPropertyChanged(nameof(SortColumn));
        OnPropertyChanged(nameof(SortDescending));
        Refresh();
    }

    private void Refresh()
    {
        IEnumerable<T> rows = Items;
        if (!string.IsNullOrEmpty(_filter) && _filterColumn is { } filtered && _values.TryGetValue(filtered, out var filterValue))
        {
            rows = rows.Where(row => Convert.ToString(filterValue(row), CultureInfo.InvariantCulture)?.Contains(_filter, StringComparison.OrdinalIgnoreCase) == true);
        }

        if (_sortColumn is { } sorted && _values.TryGetValue(sorted, out var sortValue))
        {
            var comparer = Comparer<object?>.Create(CompareAlphanumeric);
            rows = _descending ? rows.OrderByDescending(sortValue, comparer) : rows.OrderBy(sortValue, comparer);
        }

        _filtered = [.. rows];
        _pageIndex = Math.Clamp(_pageIndex, 0, PageCount - 1);
        Rows.Clear();
        foreach (var row in _filtered.Skip(_pageIndex * PageSize).Take(PageSize))
        {
            Rows.Add(row);
        }

        foreach (var name in (string[])[nameof(PageIndex), nameof(PageNumber), nameof(PageCount), nameof(CanPreviousPage), nameof(CanNextPage), nameof(FilteredCount), nameof(IsEmpty)])
        {
            OnPropertyChanged(name);
        }

        ((Act)PreviousPageCommand).Changed();
        ((Act)NextPageCommand).Changed();
        ((Act)FirstPageCommand).Changed();
        ((Act)LastPageCommand).Changed();
        Selection();
    }

    private void Selection()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(AllPageRowsSelected));
        OnPropertyChanged(nameof(State));
    }

    /// <summary>The rows' order by their places among the rows the source started with, or none while it is that order or holds other rows.</summary>
    private List<int> Order()
    {
        if (Items.Count != _origin.Count || Items.SequenceEqual(_origin))
        {
            return [];
        }

        var order = Items.Select(row => _origin.IndexOf(row)).ToList();
        return order.Contains(-1) ? [] : order;
    }

    private void Apply(DataTableState state)
    {
        _paging = true;
        _filter = state.Filter;
        if (state.Sort is null || _values.ContainsKey(state.Sort))
        {
            (_sortColumn, _descending) = (state.Sort, state.Sort is not null && state.Descending);
        }

        _pageSize = state.Size is > 0 and var size ? size : _pageSize;
        foreach (var column in _columns.Where(column => column.CanHide))
        {
            column.IsVisible = !state.Hidden.Contains(column.Key, StringComparer.Ordinal);
        }

        for (var index = 0; index < _origin.Count; index++)
        {
            _origin[index].IsSelected = state.Selected.Contains(index);
        }

        if (state.Order.Count == _origin.Count && Items.Count == _origin.Count && state.Order.Distinct().Count() == _origin.Count && state.Order.All(index => index < _origin.Count))
        {
            Items.CollectionChanged -= OnItemsChanged;
            for (var target = 0; target < state.Order.Count; target++)
            {
                Items.Move(Items.IndexOf(_origin[state.Order[target]]), target);
            }

            Items.CollectionChanged += OnItemsChanged;
        }

        _pageIndex = state.Page ?? _pageIndex;
        _paging = false;
        Refresh();
        foreach (var name in (string[])[nameof(FilterText), nameof(SortColumn), nameof(SortDescending), nameof(PageSize)])
        {
            OnPropertyChanged(name);
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var item in e.OldItems?.OfType<T>() ?? [])
        {
            item.PropertyChanged -= OnRowChanged;
        }

        foreach (var item in e.NewItems?.OfType<T>() ?? [])
        {
            item.PropertyChanged += OnRowChanged;
        }

        Refresh();
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDataTableRow.IsSelected) && !_paging)
        {
            Selection();
        }
    }

    private bool Set<TValue>(ref TValue field, TValue value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<TValue>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed class Act(Action<object?> run, Func<bool>? can = null) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => can?.Invoke() ?? true;

        public void Execute(object? parameter) => run(parameter);

        public void Changed() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
