# Breadcrumb

shadcn/ui's `breadcrumb` as `c:Breadcrumb` (a sealed `ItemsControl` whose containers are `c:BreadcrumbItem`s: the
separator `SeparatorTemplate` draws, Lucide's `chevron-right` 14 px by default, before every item but the first),
`HyperlinkButton`s themed as `BreadcrumbLink` (`--muted-foreground`, `--foreground` under the pointer), `c:BreadcrumbPage`
and `c:BreadcrumbEllipsis`; items 10 px apart in 14 px text.

| AC | Criterion |
| --- | --- |
| BREADCRUMB-01 | Given the `breadcrumb-demo` and `breadcrumb-separator` fixtures in each theme on the web, when the pages are served, then each link is an `<a href>` and Chromium computes every link's, page's and separator's box as shadcn/ui's within 1 px (the menu button without the browser's padding, framework need 50), the links `--muted-foreground` turning `--foreground` under the pointer and the page `--foreground`, in 14 px text, and the breadcrumb is a `<nav aria-label="breadcrumb">` holding an `<ol>` of `<li>` items whose separators are `aria-hidden` (framework needs 41 and 50, resolved in Aioros `0.1.0-preview.408.cc55357`); the page's `aria-current="page"` is not written, since the page is a text and Aioros marks only links. |
| BREADCRUMB-02 | Given the `breadcrumb-demo` and `breadcrumb-separator` fixtures in each theme on the desktop, when they are shown, then the links, separators (14 px chevrons, or the custom slashes), ellipsis and page are at shadcn/ui's boxes within 2 px (each text's width rounded up to a whole pixel) with shadcn/ui's colors, the first item has no separator, a link turns `--foreground` under the pointer, and the breadcrumb is a navigation landmark named `breadcrumb`. |
| BREADCRUMB-03 | Given a view whose model binds a `c:Breadcrumb`'s `ItemsSource` (each crumb through an `ItemTemplate` of a link and the last as a page) and its `SeparatorTemplate` from the view's resources (compiled bindings, no code-behind), when the model navigates deeper (a command adding a crumb), then the breadcrumb shows one more item after one more separator, the first still without one. |
| BREADCRUMB-04 | Given the `breadcrumb-demo` fixture on the web, when the page is served, then its page item ("Breadcrumb", a `c:BreadcrumbPage`) carries `aria-current="page"` and no link or other item does, as shadcn/ui's `BreadcrumbPage` (`aria-current="page"`). |
