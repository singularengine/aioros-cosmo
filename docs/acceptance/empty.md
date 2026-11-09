# Empty

shadcn/ui's `empty` as `c:Empty` (a sealed `ContentControl` with `Media`, `Title` and `Description`, its content the
actions) and `c:EmptyMedia` (`icon`: a 40 px `--muted` tile): centered in its box with 48 px padding (shadcn's `md:p-12`),
the header 384 px wide at most, 24 px from the content; `outline` draws the dashed 1 px border.

| AC | Criterion |
| --- | --- |
| EMPTY-01 | Given the `empty-demo` and `empty-outline` fixtures in each theme on the web, when the pages are served, then Chromium computes the media tile, title, description and buttons at shadcn/ui's positions within 1.5 px (the description's lines excepted, `text-balance`) with shadcn/ui's colors, the 18 px medium title and the 14 px description on 22.75 px lines, the title's tracking `-0.45px`, and only the outline's border dashed (framework needs 40 and 44, resolved in Aioros `0.1.0-preview.408.cc55357`). |
| EMPTY-02 | Given the `empty-demo` and `empty-outline` fixtures in each theme on the desktop, when they are shown, then the empty state has shadcn/ui's height, its media tile is 40 px on `--muted` with 10 px corners 16 px above the title, the title, description and actions have shadcn/ui's boxes within 1.5 px, fonts and colors, and the `outline` one draws its 1 px border dashed in `--border` with 10 px corners. |
| EMPTY-03 | Given a view whose model binds an `c:Empty`'s visibility to an empty collection (`ObjectConverters`, compiled bindings, no code-behind) and its action button to a command, when the command adds a project, then the empty state hides and the project shows through its item template, and removing it shows the empty state again. |
