# Label

shadcn/ui's `label` as Avalonia's `Label` with Cosmo's control theme (`src/Aioros.Cosmo/Forms/Label.axaml`); `Target`
names the control it labels (`htmlFor`).

| AC | Criterion |
| --- | --- |
| LABEL-01 | Given the `input-states` fixture in the light and the dark theme on the web, when the page is served, then the email field's label is a real `<label for="email">` (its `Target` has `a:Web.Id="email"`) and Chromium computes its font size (14 px), weight (500), color and box (on its 14 px line, Aioros `0.1.0-main.58e3744`'s TEXT-33) as shadcn/ui's label within 0.5 px |
| LABEL-02 | Given a `Label` in each theme on the desktop, when it is shown, then it has no padding, a 14 px medium font on a 14 px line, the foreground of its parent (shadcn's label sets no color: a label in an invalid `c:Field` turns destructive), the 0.5 opacity of a disabled label, and clicking it moves the keyboard focus to its `Target`. |
| LABEL-03 | Given a `Label` whose `Content` is bound with a `StringFormat` to a view model on the desktop, when the model's value changes, then the label shows the formatted new value. |
| LABEL-04 | Given the Input page on the web, when it is served, then its label whose `Content` is bound with `StringFormat='Name ({0} unread)'` (without a `Target`) is a `<label>` holding `Name (3 unread)`, the model's value formatted. |
| LABEL-05 | Given the `input-file` fixture in each theme on the web, when Chromium computes its "Picture" label, then its line height is 14 px and its box 14 px high at shadcn/ui's position within 1 px (`leading-none`). |
