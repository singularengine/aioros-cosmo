using Aioros.Markup;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;

namespace Aioros.Cosmo.Tables;

/// <summary>
/// What a <c>TableView</c> needs to be shadcn/ui's data table: with <c>c:DataTable.SelectsRows="True"</c>, each row whose
/// item is an <see cref="IDataTableRow"/> takes the <c>selected</c> class while its item is selected (shadcn's
/// <c>data-state="selected"</c>, <c>--muted</c>), without the table's own click selection; with <c>c:DataTable.State</c>
/// bound to its source's state, the table's page, sort, filter, columns, selection and order travel across posts on the web.
/// </summary>
public static class DataTable
{
    /// <summary>Defines the <c>SelectsRows</c> attached property.</summary>
    public static readonly AttachedProperty<bool> SelectsRowsProperty = AvaloniaProperty.RegisterAttached<TableView, bool>("SelectsRows", typeof(DataTable));

    /// <summary>
    /// Defines the <c>State</c> attached property: bound two way to a <see cref="DataTableSource{T}.State"/>, the table's
    /// state travels across posts on the web (<c>a:Web.State</c>), so a post finds the rows its visitor saw; the desktop
    /// keeps the source itself.
    /// </summary>
    public static readonly AttachedProperty<string?> StateProperty =
        AvaloniaProperty.RegisterAttached<TableView, string?>("State", typeof(DataTable), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    static DataTable()
    {
        StateProperty.Changed.AddClassHandler<TableView>((table, _) => Web.SetState(table, "DataTable.State"));
        SelectsRowsProperty.Changed.AddClassHandler<TableView>(OnSelectsRows);
    }

    /// <summary>Gets the table's travelling state.</summary>
    public static string? GetState(TableView table) => table.GetValue(StateProperty);

    /// <summary>Sets the table's travelling state.</summary>
    public static void SetState(TableView table, string? value) => table.SetValue(StateProperty, value);

    private static void OnSelectsRows(TableView table, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.GetNewValue<bool>())
        {
            table.ContainerPrepared += OnPrepared;
            table.ContainerClearing += OnClearing;
        }
        else
        {
            table.ContainerPrepared -= OnPrepared;
            table.ContainerClearing -= OnClearing;
        }
    }

    /// <summary>Whether the table's rows show their items' selection.</summary>
    public static bool GetSelectsRows(TableView table) => table.GetValue(SelectsRowsProperty);

    /// <summary>Makes the table's rows show their items' selection.</summary>
    public static void SetSelectsRows(TableView table, bool value) => table.SetValue(SelectsRowsProperty, value);

    private static void OnPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container.DataContext is IDataTableRow row)
        {
            var mirror = new Mirror(e.Container, row);
            e.Container.SetValue(MirrorProperty, mirror);
            row.PropertyChanged += mirror.Changed;
            mirror.Apply();
        }
    }

    private static void OnClearing(object? sender, ContainerClearingEventArgs e)
    {
        if (e.Container.GetValue(MirrorProperty) is { } mirror)
        {
            mirror.Row.PropertyChanged -= mirror.Changed;
            e.Container.ClearValue(MirrorProperty);
            e.Container.Classes.Remove("selected");
        }
    }

    private static readonly AttachedProperty<Mirror?> MirrorProperty = AvaloniaProperty.RegisterAttached<Control, Mirror?>("Mirror", typeof(DataTable));

    private sealed class Mirror(Control container, IDataTableRow row)
    {
        public IDataTableRow Row => row;

        public void Apply() => container.Classes.Set("selected", row.IsSelected);

        public void Changed(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IDataTableRow.IsSelected))
            {
                Apply();
            }
        }
    }
}
