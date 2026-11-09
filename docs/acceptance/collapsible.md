# Collapsible

shadcn/ui's `collapsible` (Radix Collapsible) as Avalonia's `Expander` with the `collapsible` class (Cosmo's
`CosmoCollapsible` theme): no chrome, the header (the whole header toggles, as `CollapsibleTrigger`) with shadcn's demo's
32 px ghost toggle and Lucide's `chevrons-up-down` at its top right, inset by `Padding`, the content 8 px below.

| AC | Criterion |
| --- | --- |
| COLLAPSIBLE-01 | Given the `collapsible-demo` and `collapsible-open` fixtures in each theme on the web, when the pages are served, then the collapsible is a `<details>` (`open` for the open fixture) whose `<summary>` holds the header's text and first row, the rows' borders, radii, padding and Geist Mono text are shadcn/ui's, the header's 32 px ghost toggle with its icon is written beside the text (since Aioros `0.1.0-preview.467.b967340`, framework need 47), and a click on the summary opens it without a request. |
| COLLAPSIBLE-02 | Given the `collapsible-demo` and `collapsible-open` fixtures in each theme on the desktop, when they are shown, then the toggle is 32 px square at shadcn/ui's box within 1 px with its chevrons, the rows are at shadcn/ui's boxes within 1 px, the closed one hides its content and the open one shows its two rows 8 px apart, and the toggle turns `--accent` under the pointer. |
| COLLAPSIBLE-03 | Given a view whose model binds a collapsible's `IsExpanded` two way (compiled bindings, no code-behind), when the model opens it and the user then presses Space on its focused header, then its content shows and then hides, and the model's state follows to false. |
