# Toggle Group

shadcn/ui's `toggle-group` (Radix Toggle Group) as Avalonia's `ListBox` with the `toggle-group` class, which gives it
Cosmo's `CosmoToggleGroup` theme: items in a row with the toggle's look, `SelectionMode="Toggle"` (Radix's
`type="single"`, deselectable) or `Multiple,Toggle` (`type="multiple"`), and the classes `outline`, `sm`, `lg` and
`spacing` (shadcn's `spacing={2}`). Selection, `SelectedItem`, `SelectedItems` and `ItemTemplate` are the `ListBox`'s.

| AC | Criterion |
| --- | --- |
| TOGGLEGROUP-01 | Given the `toggle-group` fixture, when the page is served, then each group is a `role="group"` (multiple) or `role="radiogroup"` (single) element holding a `<label>` per item with a hidden checkbox or radio sharing the group's form field name, the selected items' inputs checked, and each item's content. |
| TOGGLEGROUP-02 | Given the `toggle-group` fixture in each theme on the web, when Chromium computes it, then each item's box within 1 px, border width, font size, weight and text color are shadcn/ui's and the first item's corner radius, the selected items take `--accent` and `--accent-foreground`, and under the pointer an item takes shadcn/ui's hover background; the inner items keep the first item's radius on the web (a per-child style of a list's items, framework need 48) |
| TOGGLEGROUP-03 | Given the `toggle-group` fixture in each theme on the desktop, when it is laid out, then every item's box relative to its group is shadcn/ui's within 0.5 px vertically and 2 px horizontally (the desktop rounds text widths up to whole pixels), each item resolves shadcn/ui's background, foreground, border thickness and corner radius (the first rounded on the left, the last on the right, each with `spacing`), and the selected items take `--accent` and `--accent-foreground`. |
| TOGGLEGROUP-04 | Given a toggle group whose `ItemsSource` is bound to models with an `ItemTemplate`, `SelectionMode="Toggle"` and `SelectedItem` bound two way, and a multiple one, on the desktop, when items are clicked, then the single group selects one item at a time and clicking the selected item again clears the selection, the multiple group keeps several selected, the model follows the single group's selected item, and the items show their templates. |
| TOGGLEGROUP-05 | Given the Toggle Group page on the web, when a visitor selects `Right` in the single group whose `SelectedItem` is bound to the model and posts the form with the `Apply` button, then the answer has `Right` selected and shows the model's status `Aligned Right`. |
