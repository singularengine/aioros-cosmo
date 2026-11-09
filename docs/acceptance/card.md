# Card

shadcn/ui's `card` as `c:Card` (an `ItemsControl` stacking its parts), `c:CardHeader` (title, description and action),
`c:CardContent` and `c:CardFooter` (`src/Aioros.Cosmo/Display/Card.axaml`).

| AC | Criterion |
| --- | --- |
| CARD-01 | Given the `card-basic` fixture (a header with a title, a description and an action, content, and a footer of two buttons) and the `card-plain` fixture (a header without an action) in the light and the dark theme on the web, when they are rendered, then the card's box, background, border, corner radius (14 px) and shadow, and the box of each button inside it, are shadcn/ui's within 1 px. |
| CARD-02 | Given a `c:Card` on the desktop, when it is shown, then its surface is `CosmoCardBrush` with a 1 px `CosmoBorderBrush` border, the corner radius `CosmoRadiusXl`, 24 px above and below, `CosmoShadowSm`, and its parts are stacked 24 px apart; the header shows the title in semibold on a 16 px line and the description at 14 px in `CosmoMutedForegroundBrush` 8 px below it, the action to the right; content and footer are inset 24 px on both sides. |
| CARD-03 | Given a `c:CardHeader` with only a title, when it is shown on the desktop, then it is exactly one title line high (no space kept for a missing description or action), and once its `Description` and `Action` are set at runtime, then both appear. |
| CARD-04 | Given the `card-separated` fixture in each theme on the desktop (a `c:CardHeader` and a `c:CardFooter` with the `border` class, shadcn/ui's `border-b` and `border-t`), when it is laid out, then the card, the header, the content, the footer and its button stand at shadcn/ui's boxes within 1.5 px: the header has a 1 px `--border` bottom border 24 px below its description (`[.border-b]:pb-6`) and the footer a 1 px `--border` top border 24 px above its button (`[.border-t]:pt-6`). |
| CARD-05 | Given the `card-separated` fixture in each theme on the web, when Chromium lays it out, then the header's bottom border and the footer's top border are 1 px in `--border` and every part stands at shadcn/ui's box within 1 px, as CARD-04 states for the desktop. |
| CARD-06 | Given a card with the `flat` class (`sidebar-06`'s opt-in card, shadcn's `shadow-none`) on the desktop, when it is shown, then its frame has no shadow while a plain card keeps `shadow-sm`. |
