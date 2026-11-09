# Progress

shadcn/ui's `progress` (Radix Progress) as Avalonia's `ProgressBar` with Cosmo's control theme
(`src/Aioros.Cosmo/Forms/ProgressBar.axaml`): 8 px high, fully rounded, `--primary` on `--primary` at 20%.

| AC | Criterion |
| --- | --- |
| PROGRESS-01 | Given the `progress` fixture in each theme on the web, when the page is served, then each `ProgressBar` is a `<progress>` with `max="100"` and its value, and Chromium computes its x, width, height (8 px), fully rounded corners and track background (`--primary` at 20%) as shadcn/ui's progress. |
| PROGRESS-02 | Given `ProgressBar`s at 33, 66 and 100 in each theme on the desktop, when they are shown, then each track is 8 px high, fully rounded and `--primary` at 20%, and its indicator `--primary`, as wide as shadcn/ui's visible indicator within 0.5 px (the value's share of the width). |
| PROGRESS-03 | Given the Progress page on the web, when a visitor presses `+10%` (a command raising the bound value from 60), then the answer's bound `<progress>` has the value 70 and the text `70% done`. |
