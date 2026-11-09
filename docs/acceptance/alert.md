# Alert

shadcn/ui's `alert` as `c:Alert` (a sealed `ContentControl` with `Icon` and `Title`, its content the description):
`rounded-lg border px-4 py-3 text-sm bg-card`, the icon 16 px in a 16 px column 12 px before the title, the title medium
with `tracking-tight`, the description `--muted-foreground` 2 px below; `destructive` turns the text `--destructive` and
the description `--destructive` at 90%.

| AC | Criterion |
| --- | --- |
| ALERT-01 | Given the `alert-demo` fixture in each theme on the web, when the page is served, then Chromium computes each alert's box within 1 px and its background, border color, 10 px radius, 16 by 12 px padding, and each title's and description's box, 14 px font, weight and color as shadcn/ui's (the destructive one's `--destructive` and its description's `--destructive` at 90%), the icon drawn in the text's color, each title's `tracking-tight` as `letter-spacing:-0.35px` and each alert `role="alert"` (framework needs 40 and 41, resolved in Aioros `0.1.0-preview.408.cc55357`). |
| ALERT-02 | Given the `alert-demo` fixture in each theme on the desktop, when it is shown, then each `c:Alert`'s border, background, radius and padding, its title's and description's boxes within 1 px, fonts, colors and the title's letter spacing (-0.35 px), are shadcn/ui's, the alert without description is 46 px high, the destructive alert's list bullets are 4 px dots 19 px before their text, and every alert has an assertive live setting (`role="alert"`). |
| ALERT-03 | Given a view whose model binds an alert's `Title` and content and its `destructive` class (compiled bindings, no code-behind), when a command fails a save, then the alert turns destructive with the model's error title and message, and when a second command succeeds, it shows the success title and the description formatted with `StringFormat`. |
| ALERT-04 | Given the Alert page on the web, when a visitor posts `Save` with an empty name, then the answer's alert is destructive (the description's text `--destructive` at 90%) and reads the model's error message, and posting it again with a name shows the success description. |
