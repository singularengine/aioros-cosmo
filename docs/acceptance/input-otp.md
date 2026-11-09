# Input OTP

shadcn/ui's `input-otp` as Cosmo's `c:InputOtp` (`src/Aioros.Cosmo/Forms/InputOtp.cs`): `Length` boxes grouped by
`GroupSize` with a separator, showing `Text` (two-way), typed into a hidden `TextBox` that covers them, as the
`input-otp` library's hidden input.

| AC | Criterion |
| --- | --- |
| INPUTOTP-01 | Given the `input-otp` fixture in each theme on the web, when Chromium computes it, then each box's box (x, y, 36 by 36 px), border color, the first box's radius and the shown characters (`1`, `2`, `3`) are shadcn/ui's, and the code is posted through one `<input>` with a form field name and `maxlength` its length. |
| INPUTOTP-02 | Given a `c:InputOtp` of length 6 in groups of 3 on the desktop, its `Text` bound two way, when it is shown, focused and `1`, `2`, `3` are typed, then Backspace, then the model sets `987654`, then each box is 36 px with its group's borders and radii and a separator 24 px wide between the groups, the boxes show the typed characters, the active box (the next one to fill) shows shadcn/ui's ring while focused, the model follows the typing, and the boxes show the model's code; a disabled one has an opacity of 0.5. |
| INPUTOTP-03 | Given the Input OTP page on the web, when a visitor types `123456` into its bound code and posts the form with the `Verify` button, then the answer shows the command's status `Verified 123456` and the six boxes holding the digits. |
| INPUTOTP-04 | Given the `input-otp` fixture on the web, when the visitor focuses the first code's input (holding `123`) and types `4`, then the fourth slot shows `4` and the fifth slot takes the active look the desktop gives it (the `--ring` border and the 3 px ring), with no request; when the input loses the focus, no slot keeps the active look. |
