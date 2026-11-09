# Separator

shadcn/ui's `separator` (Radix Separator) as Avalonia's `Separator` with Cosmo's control theme
(`src/Aioros.Cosmo/Display/Display.axaml`): a 1 px line in `--border`, horizontal by default, `vertical` as a class.

| AC | Criterion |
| --- | --- |
| SEPARATOR-01 | Given the `separator-demo` fixture in each theme on the web, when the page is served, then each `Separator` is an `<hr>` with no border and Chromium computes the horizontal one 592 by 1 px and the two vertical ones 1 by 20 px at shadcn/ui's positions within 1 px, all with shadcn/ui's `--border` background. |
| SEPARATOR-02 | Given the `separator-demo` fixture in each theme on the desktop, when it is shown, then the horizontal `Separator` is 592 by 1 px 16 px below the text, each `vertical` one 1 px wide and as high as its 20 px row, all painting shadcn/ui's `--border` color, at shadcn/ui's boxes within 1 px; and a `Separator` in a menu keeps the menu separator's look (`CosmoMenuSeparator`). |
| SEPARATOR-03 | Given a view whose model binds a `Separator`'s `IsVisible` (through `BoolConverters.Not`) and its `vertical` class (compiled bindings, no code-behind), when the model's values change, then the separator hides and shows and turns from 592 by 1 px into 1 px wide as high as its row; and when an app's resources override `CosmoBorderBrush`, the separator paints the app's color. |
