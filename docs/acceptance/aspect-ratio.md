# Aspect Ratio

shadcn/ui's `aspect-ratio` (Radix AspectRatio) as `c:AspectRatio` (a sealed `Decorator` with `Ratio`): its child as wide
as the room it is given and `Ratio` times less high.

| AC | Criterion |
| --- | --- |
| ASPECTRATIO-01 | Given the `aspect-ratio-demo` fixture in each theme on the web, when the page is served, then the image is an `<img>` Chromium computes 450 by 253.13 px within 1 px with 10 px corners, as shadcn/ui's, written at the size the engine laid it out; a page whose width changes keeps that size, as CSS `aspect-ratio` is not written (framework need 43). |
| ASPECTRATIO-02 | Given `c:AspectRatio`s with ratios 16:9, 1 and 4:3 given 450 px on the desktop, when they are laid out, then they are 253.13, 450 and 337.5 px high and their child fills them; given no width constraint it takes its child's width; and a ratio of 0 or infinity is refused. |
| ASPECTRATIO-03 | Given a view whose model binds `Ratio` (a choice of 16:9 or 1:1 two way, compiled bindings, no code-behind), when the model's choice changes, then the decorator's height follows (253.13 then 450 px for 450 px of width). |
