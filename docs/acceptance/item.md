# Item

shadcn/ui's `item` as `c:Item` (a sealed `ContentControl` with `Media`, `Title`, `Description` and `Actions`),
`c:ItemMedia` (`icon`, `image`) and `c:ItemGroup`: `rounded-md border border-transparent p-4 gap-4 text-sm`, `outline`,
`muted` (`--muted` at 50%), `sm` (`px-4 py-3 gap-2.5`) and `link` (`--accent` at 50% under the pointer).

| AC | Criterion |
| --- | --- |
| ITEM-01 | Given the `item-demo` and `item-variant` fixtures in each theme on the web, when the pages are served, then Chromium computes every item's box within 1 px, its border color, background (`--muted` at 50% for `muted`), 8 px radius and padding, the title's 14 px medium text on a 19.25 px line, the description's `--muted-foreground` on a 21 px line, and the actions' boxes as shadcn/ui's. |
| ITEM-02 | Given the `item-demo` and `item-variant` fixtures in each theme on the desktop, when they are shown, then every `c:Item`, its media, title, description and actions have shadcn/ui's boxes within 2.5 px (Avalonia rounds each 78.25 px item to 79 px, see the layout rounding decision), colors and fonts, and the `link` item turns `--accent` at 50% under the pointer while the others keep their background. |
| ITEM-03 | Given a view whose model binds an `ItemsControl` of items (panel `c:ItemGroup`, an `ItemTemplate` of `c:Item` binding `Title`, `Description` and an action button's command with the item as its parameter, compiled bindings, no code-behind), when the action of the second item runs, then the model removes it and the group shows the remaining items; an item's bound `Description` formatted with `StringFormat` follows the model. |
| ITEM-04 | Given the Item page on the web, when a visitor posts the `Remove` action of an item in the bound list, then the answer's list no longer holds it and reads `2 items`. |
| ITEM-05 | Given the `item-header-footer` fixture in each theme on the desktop, when it is shown, then the `c:Item`, its `c:ItemHeader` (a row of a text and a badge at its ends), its title, description and actions, and its `c:ItemFooter` (a row of two texts at its ends) have shadcn/ui's boxes within 1.5 px: the header a full row 16 px above the content, the footer a full row 16 px below it. |
| ITEM-06 | Given the same fixture on the web, when Chromium lays it out, then the same parts have shadcn/ui's boxes within 1.5 px. |
| ITEM-07 | Given a `c:ItemGroup` of two items with a `c:ItemSeparator` between them on the desktop, when it is laid out, then the separator is a 1 px `--border` line as wide as the group with no margin above or below it (shadcn/ui's `ItemSeparator`, `my-0`). |
| ITEM-08 | Given the same group on the web (the Item page's grouped items), when Chromium lays it out, then the separator is a 1 px line of `--border` as wide as the group, touching the items above and below it. |
