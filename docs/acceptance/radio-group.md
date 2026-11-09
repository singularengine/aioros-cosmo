# Radio Group

shadcn/ui's `radio-group` (Radix Radio Group) as Avalonia's `RadioButton`s in a panel (12 px apart, shadcn's `gap-3`)
with Cosmo's control theme (`src/Aioros.Cosmo/Forms/RadioButton.axaml`): a 16 px circle with a 7.33 px dot, and its
`Content` 12 px to its right in the label's look. Down and Right check and focus the next radio of the group, Up and Left the previous one, looping (Radix's keys; Cosmo registers them for `RadioButton`). Its brushes are Fluent's lightweight-styling keys
(`RadioButtonOuterEllipseStroke`…), defined by Cosmo's token dictionaries and read by Aioros's web writer.

| AC | Criterion |
| --- | --- |
| RADIO-01 | Given the `radio-group` fixture, when the page is served, then the three `RadioButton`s are `<label>`s holding `<input type="radio">`s that share one form field name with the values 0, 1 and 2, the checked one `checked`, the disabled one `disabled`, each with its content's text. |
| RADIO-02 | Given the `radio-group` fixture in each theme on the web, when Chromium computes it, then each radio's text has shadcn/ui's label font size, weight and color, each circle is shadcn/ui's 16 px circle at its box within 1 px with the `--input` border, the checked one's dot is `--primary`, and the disabled radio's opacity is 0.5 |
| RADIO-03 | Given `RadioButton`s in each theme on the desktop, when one is unchecked, checked, disabled and focused with the keyboard, then its circle is 16 px and resolves shadcn/ui's border brush, background and box shadow for that state, the checked one shows shadcn's 7.33 px `--primary` dot centered (8 px after layout rounding), the content starts 12 px after the circle in a 14 px medium text, and the disabled radio has an opacity of 0.5. |
| RADIO-04 | Given three `RadioButton`s in a panel on the desktop, the second one's `IsChecked` bound two way to a view model, when the first is clicked, then the arrow keys are pressed, then the second is clicked, then exactly one radio is checked at each step, the arrows move the check to the next and previous radio, and the model follows the second radio. |
| RADIO-05 | Given the Radio Group page on the web, when a visitor checks `Compact` and posts the form with the `Apply` button, then the answer has `Compact` checked and shows the bound model's status `Density: Compact`. |
