using System.Globalization;

namespace Aioros.Cosmo.Tables;

/// <summary>
/// A data table's state as <see cref="DataTableSource{T}.State"/> writes it: the page, its size, the sort, the filter, the
/// hidden columns' keys, and the selected rows and the rows' order, each row by its place among the rows the source
/// started with (TanStack Table's <c>pagination</c>, <c>sorting</c>, <c>columnFilters</c>, <c>columnVisibility</c> and
/// <c>rowSelection</c>); parts that do not parse are left out.
/// </summary>
internal sealed record DataTableState(int? Page, int? Size, string? Sort, bool Descending, string? Filter, IReadOnlyList<string> Hidden, IReadOnlyList<int> Selected, IReadOnlyList<int> Order)
{
    public string Write()
    {
        var parts = new List<string>
        {
            $"page={Number(Page ?? 0)}",
            $"size={Number(Size ?? 0)}",
        };
        parts.AddRange(Sort is null ? [] : [$"sort={Uri.EscapeDataString(Sort)}", $"desc={(Descending ? 1 : 0)}"]);
        parts.AddRange(string.IsNullOrEmpty(Filter) ? [] : [$"filter={Uri.EscapeDataString(Filter)}"]);
        parts.AddRange(Hidden.Count == 0 ? [] : [$"hidden={string.Join(',', Hidden.Select(Uri.EscapeDataString))}"]);
        parts.AddRange(Selected.Count == 0 ? [] : [$"selected={string.Join(',', Selected.Select(Number))}"]);
        parts.AddRange(Order.Count == 0 ? [] : [$"order={string.Join(',', Order.Select(Number))}"]);
        return string.Join(';', parts);
    }

    public static DataTableState Parse(string? text)
    {
        var parts = (text ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .Where(pair => pair.Length == 2)
            .GroupBy(pair => pair[0], StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last()[1], StringComparer.Ordinal);
        string? Text(string key) => parts.TryGetValue(key, out var value) ? Uri.UnescapeDataString(value) : null;
        IReadOnlyList<string> Keys(string key) => parts.TryGetValue(key, out var value) ? [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Uri.UnescapeDataString)] : [];
        IReadOnlyList<int> Numbers(string key) => parts.TryGetValue(key, out var value) ? [.. value.Split(',').Select(Parsed).OfType<int>()] : [];
        return new(Parsed(Text("page")), Parsed(Text("size")), Text("sort"), Text("desc") == "1", Text("filter"), Keys("hidden"), Numbers("selected"), Numbers("order"));
    }

    private static int? Parsed(string? text) => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : null;

    private static string Number(int number) => number.ToString(CultureInfo.InvariantCulture);
}
