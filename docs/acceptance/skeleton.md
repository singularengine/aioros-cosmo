# Skeleton

shadcn/ui's `skeleton` as `c:Skeleton` (a sealed `TemplatedControl`): a box in `--accent` with `--radius-md` corners,
pulsing (Tailwind's `animate-pulse`: opacity 1, 0.5, 1 over 2 s, `cubic-bezier(0.4, 0, 0.6, 1)`, forever).

| AC | Criterion |
| --- | --- |
| SKELETON-01 | Given the `skeleton-demo` fixture in each theme on the web, when the page is served, then each skeleton is a box Chromium computes at shadcn/ui's position and size within 1 px, with shadcn/ui's `--accent` background and radii (fully rounded for the avatar, 8 px for lines, 14 px for the card), each pulsing with a CSS animation (2 s, `cubic-bezier(0.4, 0, 0.6, 1)`, infinite, opacity 0.5 at 50%), Tailwind's `animate-pulse`. |
| SKELETON-02 | Given the `skeleton-demo` fixture in each theme on the desktop, when it is shown, then each `c:Skeleton` paints `--accent` with shadcn/ui's radius and box within 1 px, and carries the pulse: an infinite 2 s animation of `Opacity` through 1, 0.5 and 1 at 0%, 50% and 100% with the spline `0.4,0,0.6,1`. |
| SKELETON-03 | Given a view whose model binds a loading state (compiled bindings, no code-behind), when the model is loading, then the skeletons show and the loaded content (a templated item list) is hidden, and when a command finishes loading, the skeletons hide (`BoolConverters.Not`) and the items show through their `ItemTemplate`. |
