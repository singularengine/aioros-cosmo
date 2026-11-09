# Table

shadcn/ui's `table` as Avalonia's `TableView` with Cosmo's themes (`TableView`, `TableViewColumnHeader`, `TableViewRow`,
`TableViewCell`: a 40 px header row of medium text over a 1 px `--border` line, rows with a 1 px bottom border but the
last, 8 px cell padding, `--muted` at half opacity under the pointer and `--muted` when selected), `c:TableFooter`
(shadcn's `TableFooter` row) and `c:TableCaption`.

| AC | Criterion |
| --- | --- |
| TABLE-01 | Given the `table-demo` fixture in each theme on the web, when the page is served, then it is a `<table>` whose header has the columns Invoice, Status, Method and Amount (the last right-aligned) and whose body has the 7 invoices, every cell padded 8 px in 14 px text, the invoice numbers medium; the footer has `--muted` at half opacity, a 1 px top border and medium text, and the caption is muted and centered; the header's background is Fluent's chrome, the rows' borders and the header's 40 px are not written and star widths are shared equally (framework need 51). |
| TABLE-02 | Given the `table-demo` fixture in each theme on the desktop, when it is shown, then the header row is 40 px high with medium text in `--foreground` over a 1 px `--border` line, the columns start at shadcn/ui's x within 1 px, each row is 37 px with a 1 px `--border` bottom line but the last, the amounts are right-aligned 8 px from the edge, the footer is 37 px with `--muted` at half opacity and a top border, and the caption is muted, centered and 16 px below; a row under the pointer turns `--muted` at half opacity and a selected row `--muted`. |
| TABLE-03 | Given a view whose model gives the invoices (an observable collection with a status cell template whose badge's class comes from a converter) and binds the table's `SelectedItem` two way and an add command (compiled bindings, no code-behind), when the user clicks the third row, then the model's selected invoice is the third; when the model selects the first, the first row is selected; and when the add command runs, the table shows 8 rows and the footer's bound total grows. |
| TABLE-04 | Given the Table page on the web, when a visitor posts the add button of the bound table, then the answer's table has 8 body rows and its footer reads the model's new total. |
