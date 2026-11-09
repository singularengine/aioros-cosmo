# Message

shadcn/ui's `message` (a chat row: an avatar at its bottom edge, 8 px from the content, the row reversed with
`align="end"`, the content stacking a header, bubbles and a footer 10 px apart, the avatar raised 32 px over a footer)
as `c:Message`, a sealed `ContentControl` with an `Avatar` and the `end` class, `c:MessageContent` (a sealed
`StackPanel`), `c:MessageHeader` and `c:MessageFooter` (sealed `TextBlock`s, 12 px medium `--muted-foreground`, 12 px
from the side) and `c:MessageGroup` (a sealed `StackPanel` 8 px apart).

| AC | Criterion |
| --- | --- |
| MESSAGE-01 | Given the `message-demo`, `message-group`, `message-header-footer` and `message-avatar` fixtures in each theme on the desktop, when they are laid out, then every bubble, header and footer of the messages stands at shadcn/ui's box within 1.5 px (3 px for a width set by its text, and 3 px down the page, where the desktop rounds each 22.75 px line), every avatar at the bottom of its row (32 px above it over a footer) and on the far side with `end`. |
| MESSAGE-02 | Given the same fixtures in each theme on the web, when Chromium lays them out, then every bubble, header and footer of the messages stands at shadcn/ui's box within 1.5 px (3 px for a width set by its text, and 3 px down the page, where the desktop rounds each 22.75 px line). |
