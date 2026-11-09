# Marker

shadcn/ui's `marker` (a row of muted 14 px text with an optional 16 px icon, the `separator` variant drawing a line on
each side of its content and the `border` variant a bottom border) as `c:Marker`, a sealed `ContentControl` with an
`Icon`, and the classes `separator`, `border` and `vertical` (shadcn's `className="flex-col"`). The examples are
shadcn/ui's `marker-demo`, `marker-variants`, `marker-icon` and `marker-border` (apps/v4/examples/radix at the pinned
commit, pointed at the new-york-v4 components).

| AC | Criterion |
| --- | --- |
| MARKER-01 | Given the `marker-demo`, `marker-variants`, `marker-icon` and `marker-border` fixtures in each theme on the desktop, when they are laid out, then every marker, its icon and its content stand at shadcn/ui's boxes within 1.5 px (a content's width within 3 px, its place 3 px down the page), the text is 14 px in `--muted-foreground`, a separator marker's lines are 1 px of `--border` 12 px from its content and a border marker has a 1 px `--border` bottom 8 px below its row. |
| MARKER-02 | Given the same fixtures in each theme on the web, when Chromium lays them out, then every marker's content stands at shadcn/ui's box within 1.5 px (a content's width within 3 px, its place 3 px down the page) and the text's color and size are shadcn/ui's. |
| MARKER-03 | Given the `marker-shimmer`, `marker-status` and `marker-link-button` fixtures in each theme on the desktop, when they are laid out, then every marker (a `c:Marker`, a `c:MarkerLink` or a `c:MarkerButton`), its icon and its content stand at shadcn/ui's boxes within 1.5 px (a content's width within 3 px), the content is in `--muted-foreground`, a marker with the `shimmer` class has its texts swept by the shimmer (THEME-09) and no other marker does, the link's text is underlined with a `StrokeOffset` of 1.95 px (the line's top 3 px below the baseline in Geist, shadcn/ui's `underline-offset-3`), and the link's and the button's content turn `--foreground` under the pointer. Precedent: shadcn/ui's `MarkerLinkButton` example (`Marker asChild` around an `<a>` and a `<button>`, `hover:text-foreground`), Radix's `Slot`; Avalonia's `HyperlinkButton` and `Button` themed as the marker. |
| MARKER-04 | Given the same three fixtures in each theme on the web, when Chromium lays them out, then every marker's content stands at shadcn/ui's box within 1.5 px (a content's width within 3 px) in shadcn/ui's color and size, the shimmering contents carry the shimmer utility (Aioros's TEXT-54), the link is an `<a href>` holding its icon and its text underlined 3 px below the baseline, and the button is a `<button>`. |
