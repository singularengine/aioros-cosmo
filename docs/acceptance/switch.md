# Switch

shadcn/ui's `switch` (Radix Switch) as Avalonia's `ToggleSwitch` with Cosmo's control theme
(`src/Aioros.Cosmo/Forms/ToggleSwitch.axaml`): a 32 by 18.4 px track with a 16 px thumb (`sm`: 24 by 14 px and 12 px),
its `OnContent` and `OffContent` empty, and its `Content`, when set, 8 px to its right (shadcn's switch beside a `Label`).
Its brushes are Fluent's lightweight-styling keys (`ToggleSwitchFillOn`…), defined by Cosmo's token dictionaries.

| AC | Criterion |
| --- | --- |
| SWITCH-01 | Given the `switch-states` fixture, when the page is served, then each `ToggleSwitch` is an `<input type="checkbox" role="switch">` with a form field name and its `a:Web.Id`, `checked` when on and `disabled` when it is not enabled, and the `Label` beside it is a `<label for>` that switch's id. |
| SWITCH-02 | Given the `switch-states` fixture in each theme on the web, when Chromium computes it, then each switch's track is shadcn/ui's 32 by 18.4 px track within 1 px, `--input` (`--input` at 80% in dark) when off and `--primary` when on, the disabled switch's opacity is 0.5, and each label's font and color are shadcn/ui's |
| SWITCH-03 | Given a `ToggleSwitch` in each theme on the desktop, when it is off, on, disabled, small, and focused with the keyboard, then its track and thumb have shadcn/ui's sizes (32 by 19 px after layout rounding of 18.4 px, 16 px thumb; 24 by 14 px and 12 px with `sm`), the track resolves shadcn/ui's background, border and box shadow for that state, the thumb `--background` (`--foreground` in dark) off and `--background` (`--primary-foreground` in dark) on, 1 px from the track's left edge off and 15 px on (11 px with `sm`), and the disabled switch has an opacity of 0.5. |
| SWITCH-04 | Given a `ToggleSwitch` whose `IsChecked` is bound two way and whose `Command` is bound, on the desktop, when it is clicked, then focused and Space is pressed, then it toggles each time, the model follows and the command runs with its parameter. |
| SWITCH-05 | Given the Switch page on the web, when a visitor turns its bound switch on and posts the form with the `Save` button, then the answer has the switch on and shows the command's status `Airplane mode on`. |
