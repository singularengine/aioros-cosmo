# Toggle

shadcn/ui's `toggle` (Radix Toggle) as Avalonia's `ToggleButton` with Cosmo's control theme
(`src/Aioros.Cosmo/Forms/ToggleButton.axaml`): `outline`, `sm` and `lg` are classes. Its state brushes are Fluent's
lightweight-styling keys (`ToggleButtonBackgroundChecked`…), defined by Cosmo's token dictionaries.

| AC | Criterion |
| --- | --- |
| TOGGLE-01 | Given the `toggle-states` fixture, when the page is served, then each `ToggleButton` is a `<label>` holding a hidden `<input type="checkbox">` with a form field name, `checked` when the toggle is on and `disabled` when it is not enabled, and its content (an icon, or an icon and a text). |
| TOGGLE-02 | Given the `toggle-states` fixture in each theme on the web, when Chromium computes each toggle, then its box (x, y, width, height: 36 px, 32 px with `sm`, 40 px with `lg`), background, text color, border color and width, corner radius, font and opacity are shadcn/ui's, and under the pointer its background and text color are shadcn/ui's hover look (`--accent-foreground` for `outline`) |
| TOGGLE-03 | Given a `ToggleButton` of each variant and size in each theme on the desktop, when it is off, on, under the pointer, focused with the keyboard and disabled, then it resolves shadcn/ui's background, foreground, border and box shadow for that state, its height and padding are shadcn/ui's (36 px and 8 px; 32 px and 6 px with `sm`; 40 px and 10 px with `lg`), and a disabled toggle has an opacity of 0.5. |
| TOGGLE-04 | Given a `ToggleButton` whose `IsChecked` is bound two way and whose `Command` is bound with a parameter, on the desktop, when it is clicked and then focused and Space is pressed, then it turns on and off, the model follows and the command runs with its parameter each time. |
