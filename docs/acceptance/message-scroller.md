# Message Scroller

shadcn/ui's `message-scroller` (a chat transcript's scroll container on `@shadcn/react`'s `MessageScroller` 0.3.1: it
opens at its end or its start, follows new messages while at its end and shows a scroll-to-end button while it is not at
its end) as `c:MessageScroller`, a sealed `ContentControl` with `DefaultScrollPosition` (`End` or `Start`), the `:end`
pseudo-class while it is at its end and `:following` while it keeps the newest message in view (the scroll bar's 10 px
gutter hidden then, as shadcn/ui hides its bar while it scrolls on its own). shadcn/ui's bottom fade (`scroll-fade-b`) is
revealed by a scroll-driven animation that its capture does not paint, so neither does Cosmo. shadcn/ui's scroller
examples stream a mock chat (`@/lib/ai`) the registry does not hold, so the reference renders a static transcript in a
card (`tools/shadcn/app/src/scroller.tsx`): `message-scroller-end` opened at its end, `message-scroller-start` at its
start.

| AC | Criterion |
| --- | --- |
| MESSAGESCROLLER-01 | Given the `message-scroller-end` and `message-scroller-start` fixtures in each theme on the desktop, when they are laid out, then the card and the scroller's viewport stand at shadcn/ui's boxes within 1.5 px (3 px down the page) and every bubble in view at its box within 1.5 px across and 3 px down the page; the end fixture's transcript ends at the viewport's bottom, as wide as the viewport, `:end` and its button hidden; the start fixture's transcript keeps the 10 px gutter of the scroll bar (372 px wide) and its 32 px scroll-to-end button stands 16 px above the viewport's bottom, centered, at shadcn/ui's box within 1.5 px, on shadcn/ui's `--background`. |
| MESSAGESCROLLER-02 | Given the `message-scroller-start` fixture on the desktop, when the scroll-to-end button is clicked, then the viewport is scrolled to its end, the scroller is `:end` and its button hidden; and when a message is then added to the transcript, the viewport stays at its end with the new message's bubble inside it. |
| MESSAGESCROLLER-03 | Given the two fixtures in each theme on the web, when the page is served, then the transcript is the one scroll container (`overflow-y: auto`) in the fixture, at shadcn/ui's viewport box within 1.5 px (3 px wide), holding all nine messages, and a 32 px square button is named `Scroll to end`; the end fixture opens at its end (its last bubble 24 px above the viewport's bottom) with its button hidden (`a:Web.FollowsEnd` on the viewport, `a:Web.ScrollsToEnd` on the button, framework need 68), the start fixture at its start with its button shown. The scroller fills the card under its header (the card's items in a `Panel`, the header docked above), with no height of its own (framework need 69). |
| MESSAGESCROLLER-04 | Given the two fixtures on the web, when the start fixture's scroll-to-end button is clicked, then with no request the viewport scrolls to its end and the button hides; when the end fixture's viewport is scrolled up, then its button shows; and when a message is appended to the end fixture's transcript while it is at its end, then the viewport stays at its end with the new message in view (MESSAGESCROLLER-02 on the web, framework need 68). |
| MESSAGESCROLLER-05 | Given the `message-scroller-start` and `message-scroller-end` fixtures on the desktop, when they are laid out, then the start fixture's viewport fades its bottom over `min(12%, 40px)` of its height (opaque from its top) and the end fixture's, scrolled to its end, does not fade. Precedent: the `MessageScroller` of shadcn/ui's registry (`scroll-fade-b` on its viewport), Cosmo's THEME-10. |
| MESSAGESCROLLER-06 | Given the same fixtures on the web, when the page is rendered, then the transcript's scroll container carries Aioros's `sfb` scroll fade (`sfbr` as it follows its end, written from its end) and, scrolled to its end, shows no fade at its bottom while the start fixture's does (its computed mask's bottom length above 0). |
