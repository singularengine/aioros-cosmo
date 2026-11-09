# Button Group

shadcn/ui's `button-group` as `c:ButtonGroup` (a sealed `StackPanel`, `Orientation`) with styles of `CosmoTheme`: its
children's inner corners square (`CosmoRadiusMdStart`, `End`, `Top`, `Bottom`), the shared borders single (the outline
buttons', text boxes', selects' and texts' leading border dropped), nested groups 8 px apart, a `Separator` 1 px of
`--input`; `c:ButtonGroupText` is shadcn's `ButtonGroupText`.

| AC | Criterion |
| --- | --- |
| BUTTONGROUP-01 | Given the `button-group-demo`, `button-group-size` and `button-group-orientation` fixtures in each theme on the web, when the pages are served, then Chromium computes every button, text, input and separator at shadcn/ui's boxes within 1 px with shadcn/ui's corner radii (8 px outside, 0 inside), border widths (the inner leading borders dropped) and colors, each group `role="group"` (framework need 41, resolved in Aioros `0.1.0-preview.408.cc55357`). |
| BUTTONGROUP-02 | Given the same fixtures in each theme on the desktop, when they are shown, then every child is at shadcn/ui's box within 2.5 px (text widths rounded up to whole pixels) with shadcn/ui's corners and borders, the vertical group's top and bottom corners rounded, nested groups 8 px apart, and the separator 1 px of `--input` as high as its group; a focused child is drawn above its neighbors. |
| BUTTONGROUP-03 | Given a view whose model binds a group's buttons' commands with parameters and a bound class (`Classes.secondary`) marking the chosen one, and a button whose visibility is bound (compiled bindings, no code-behind), when a button runs its command, then the model records its parameter and the bound class moves to it, and when the bound button hides, the group's remaining children keep their joined corners (the last visible one is not re-rounded: Avalonia's `:nth-last-child` counts hidden children, as shadcn's `:last-child` does). |
| BUTTONGROUP-04 | Given the Button Group page on the web, when a visitor posts the `Report` button of the bound group, then the answer reads `Reported` and the `Report` button carries the chosen look (secondary) while the others stay outline. |
