# Textarea

shadcn/ui's `textarea` as Avalonia's `TextBox` with `AcceptsReturn="True"` (Cosmo's control theme), written as a
`<textarea>` on the web.

| AC | Criterion |
| --- | --- |
| TEXTAREA-01 | Given the `textarea-states` fixture in the light and the dark theme on the web, when the page is served, then each multi-line `TextBox` is a `<textarea>` that sizes to its content (`field-sizing: content`) and Chromium computes its x, width and height (64 px, shadcn's `min-h-16`), border color and width, corner radius, padding (8 px 12 px), font size, text color and opacity as shadcn/ui's, and its background where shadcn's is not transparent (as for INPUT-02). |
| TEXTAREA-02 | Given a multi-line `TextBox` on the desktop, when it is shown empty, then it is 64 px high with 12 by 8 px padding and its text at the top; when Return is typed between lines, the box keeps the new line, wraps long text and grows with its content. |
