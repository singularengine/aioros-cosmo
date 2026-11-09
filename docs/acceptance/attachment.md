# Attachment

shadcn/ui's `attachment` (a file in a chat: a 14 px rounded card with its media, its title and description, and its
actions; `sm` and `xs` sizes, a `vertical` orientation for images, and the `idle`, `uploading`, `processing`, `error`
and `done` states) as `c:Attachment`, a sealed `TemplatedControl` with `Media`, `Title`, `Description`, `Actions` and
`State` (each state a pseudo-class), the classes `sm`, `xs`, `vertical` and `image`, and `c:AttachmentGroup`, an
`ItemsControl` scrolling its attachments sideways 12 px apart. The examples' remote photos are replaced by the Gallery's
own images, in the reference too.

| AC | Criterion |
| --- | --- |
| ATTACHMENT-01 | Given the `attachment-demo`, `attachment-states`, `attachment-sizes` and `attachment-group` fixtures in each theme on the desktop, when they are laid out, then every attachment, its media, its title, its description and its actions stand at shadcn/ui's boxes within 1.5 px (a text's width within 3 px, its place 3 px down the page) in the attachment's size and orientation, the visible ones of a group in its scrolled row. |
| ATTACHMENT-02 | Given the `attachment-states` fixture in each theme on the desktop, when it is laid out, then the idle attachment has a dashed `--border` outline, the error attachment a `--destructive` border at 30%, its media on `--destructive` at 10% in `--destructive` and its description in `--destructive` at 80%, and the others the card's `--border`. |
| ATTACHMENT-03 | Given the same four fixtures in each theme on the web, when Chromium lays them out, then every title and description stands at shadcn/ui's box within 1.5 px (a text's width within 3 px), and the error attachment's description takes shadcn/ui's color. |
| ATTACHMENT-04 | Given the `attachment-trigger` and `attachment-image` fixtures in each theme on the desktop, when they are laid out, then every attachment, its media, its title and its actions stand at shadcn/ui's boxes within 1.5 px (a title's width within 3 px), the vertical image attachments' actions at their top end; the trigger covers the whole card, the card turns `--muted` at 50% under the pointer, a click on an action leaves the dialog closed and a click on the card opens it; and in the `attachment-states` fixture the uploading and processing titles are swept by the shimmer (THEME-09) and no other title is. Precedent: shadcn/ui's `AttachmentTrigger` (an absolutely placed button filling the card under its actions, `has-[[data-slot=attachment-trigger]]:hover:bg-muted/50`) and the `shimmer` class of its uploading and processing titles. |
| ATTACHMENT-05 | Given the same fixtures in each theme on the web, when Chromium lays them out, then every title stands at shadcn/ui's box within 1.5 px (its width within 3 px); a click on the trigger card opens the dialog in the browser with no request; and the uploading and processing titles of `attachment-states` carry the shimmer utility (Aioros's TEXT-54). |
