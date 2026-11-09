# Input Group

shadcn/ui's `input-group` as Cosmo's `c:InputGroup` (a sealed `ItemsControl` laying a `TextBox` and its addons in a grid)
and `c:InputGroupAddon` (`inline-end` and `block-end` classes), `src/Aioros.Cosmo/Forms/InputGroup.axaml`.

| AC | Criterion |
| --- | --- |
| INPUTGROUP-01 | Given the `input-group` fixture in each theme on the desktop, when it is laid out, then each group's box, its text box's box and each addon's box relative to the fixture are shadcn/ui's within 1.5 px (text widths are rounded up to whole pixels on the desktop) (the group 36 px high, or 108 px with a textarea and a block-end addon), the group resolves shadcn/ui's border, radius, background and `shadow-xs`, the text box inside has no border or shadow of its own, and the addons' text is 14 px medium `--muted-foreground`. |
| INPUTGROUP-02 | Given a `c:InputGroup` on the desktop, when its text box takes the keyboard focus, then the group shows shadcn/ui's ring (the `--ring` border and the 3 px `--ring` at 50% with `shadow-xs`) while the text box itself shows none, and the ring goes when the focus leaves. |
| INPUTGROUP-03 | Given the `input-group` fixture in each theme on the web, when Chromium computes it, then each group is one element at shadcn/ui's box within 1 px holding an `<input>` (or `<textarea>`) and its addons, with shadcn/ui's border color, width and radius, and its addons' text in shadcn/ui's color, size and weight, centered where shadcn/ui's is within 1.5 px |
| INPUTGROUP-04 | Given the Input Group page on the web, when a visitor types `alex` into its group's bound text box and posts the form, then the answer keeps the text and shows the bound addon's text `4/24`. |
