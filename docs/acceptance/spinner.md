# Spinner

shadcn/ui's `spinner` (Lucide's `loader-circle`, `size-4 animate-spin`, `role="status"`, `aria-label="Loading"`) as
`c:Spinner` (a sealed `TemplatedControl`): 16 px, its parent's text color, turning once a second.

| AC | Criterion |
| --- | --- |
| SPINNER-01 | Given the `spinner-demo` fixture in each theme on the web, when the page is served, then each spinner is an element named `Loading` (`aria-label`) holding Lucide's `loader-circle` as SVG, Chromium computes the four spinners 12, 16, 24 and 32 px square and the spinners in the disabled buttons and in the badges 16 and 12 px at shadcn/ui's positions within 1 px, each turning with a CSS animation (1 s, linear, infinite, `rotate(0deg)` to `rotate(360deg)`), Tailwind's `animate-spin`; each spinner has `role="status"` (framework need 41, resolved in Aioros `0.1.0-preview.408.cc55357`). |
| SPINNER-02 | Given the `spinner-demo` fixture in each theme on the desktop, when it is shown, then each `c:Spinner` has shadcn/ui's box within 1 px, strokes `loader-circle` with its parent's text color (the primary button's `--primary-foreground`, the outline badge's `--foreground`), is named `Loading` with a polite live setting for assistive technologies, and turns: an infinite 1 s linear animation of its `RotateTransform.Angle` from 0 to 360 about its center. |
| SPINNER-03 | Given a view whose model runs a command that can wait (a `TaskCompletionSource` the test resolves), when the button's command starts, then the button is disabled and shows a spinner and the bound text `Saving...`, and once the command ends the spinner hides and the button reads `Save` again (bound `IsVisible`, `StringFormat`, compiled bindings, no code-behind). |
