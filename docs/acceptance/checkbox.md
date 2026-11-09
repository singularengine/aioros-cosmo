# Checkbox

shadcn/ui's `checkbox` (Radix Checkbox) as Avalonia's `CheckBox` with Cosmo's control theme
(`src/Aioros.Cosmo/Forms/CheckBox.axaml`): a 16 px box, and its `Content` 12 px to its right in the label's look, as
shadcn's checkbox beside a `Label`. `IsThreeState` with `IsChecked="{x:Null}"` is Radix's `indeterminate`. Its brushes
are Fluent's lightweight-styling keys (`CheckBoxCheckBackgroundFillChecked`…), defined by Cosmo's token dictionaries.

| AC | Criterion |
| --- | --- |
| CHECKBOX-01 | Given the `checkbox-states` fixture, when the page is served, then each two-state `CheckBox` is a `<label>` holding an `<input type="checkbox">` with a form field name, `checked` when `IsChecked` is true, `disabled` when it is not enabled, and its content's text; the three-state one is written as Aioros's three states with the indeterminate one checked. |
| CHECKBOX-02 | Given the `checkbox-states` fixture in each theme on the web, when Chromium computes it, then each checkbox's text has shadcn/ui's label font size (14 px), weight (500) and color and the disabled one's opacity is 0.5; each two-state box is shadcn/ui's 16 px box with its border color, background (`--primary` when checked) and 4 px radius, and the indeterminate checkbox's box has shadcn/ui's border color, background and 4 px radius |
| CHECKBOX-03 | Given a `CheckBox` in each theme on the desktop, when it is unchecked, checked, indeterminate, disabled, and focused with the keyboard unchecked and checked, then its box is 16 px with a 4 px radius and resolves shadcn/ui's border brush, background and box shadow for that state (the checked box keeps its `--primary` border when focused), the check glyph shows in `--primary-foreground` when checked and in the foreground when indeterminate, the content starts 12 px after the box in a 14 px medium text, and the disabled checkbox has an opacity of 0.5. |
| CHECKBOX-04 | Given a `CheckBox` whose `IsChecked` is bound two way with a compiled binding and whose `Command` is bound, on the desktop, when it is clicked on its box, then on its content, then focused and Space is pressed, then it toggles each time, the model follows, and the command runs with its `CommandParameter`; a three-state checkbox clicked from unchecked goes checked, then indeterminate, then unchecked. |
| CHECKBOX-05 | Given the Checkbox page on the web, when a visitor checks its two-way bound checkbox and posts the form with the `Save` button, then the answer shows the checkbox checked and the command's status `Notifications on`. |
