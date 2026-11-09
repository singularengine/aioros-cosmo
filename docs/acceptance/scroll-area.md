# Scroll Area

shadcn/ui's `scroll-area` as Avalonia's `ScrollViewer` with the `scroll-area` class (Cosmo's `CosmoScrollArea` theme):
the content clipped to the area's rounded border, and Radix's scroll bars over it (10 px, a 1 px transparent leading
border and 1 px padding, a fully rounded thumb of `--border`), shown while the pointer is over the area (Radix's
`type="hover"`).

| AC | Criterion |
| --- | --- |
| SCROLLAREA-01 | Given the `scroll-area-demo` and `scroll-area-horizontal` fixtures in each theme on the web, when the pages are served, then the vertical area is a 192 by 288 px box at shadcn/ui's place that is itself the scroll container (`overflow-y:auto`) with the 1 px `--border` border and an 8 px radius, and its tags at shadcn/ui's positions within 1 px; the horizontal area is 384 px wide and scrolls along x, its pictures' rounded frames keeping their 300 px (framework need 52, resolved by Aioros's themed scroll viewer written as one framed box that scrolls) |
| SCROLLAREA-02 | Given the same fixtures in each theme on the desktop, when they are shown, then no scroll bar is visible at rest; with the pointer over the vertical area its bar is 10 px wide against the right edge with a 7 px thumb of `--border`, fully rounded, whose length is the visible share of the content, and dragging the thumb 40 px down scrolls the tags by the content's share of it; over the horizontal area its bar is 10 px high against the bottom edge. |
| SCROLLAREA-03 | Given a view whose model gives the tags (an `ItemsControl` in a `scroll-area` with an `ItemTemplate`) and binds the area's `Offset` two way (compiled bindings, no code-behind), when the user scrolls the area, then the model's offset follows, and when the model's command moves its offset back to the top, the area scrolls to its first tag. |
| SCROLLAREA-04 | Given the `scroll-area-demo` fixture on the desktop with a test clock, when the pointer enters the area, then its bar shows at once; when the pointer leaves, the bar is still shown 599 ms later and hidden at 600 ms (Radix's `type="hover"` with `scrollHideDelay` 600 ms); and the pointer coming back before 600 ms keeps it shown. |
| SCROLLAREA-05 | Given the `scroll-area-demo` fixture on the web, when Chromium computes the area at rest and under the pointer, then its bar is transparent at rest and the thumb's `--border` color under the pointer, its `scrollbar-color` transition delayed 600 ms when the pointer leaves. |
