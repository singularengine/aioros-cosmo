"""shadcn/ui's chat components (marker, bubble, message, attachment, message scroller) in their examples (apps/v4/examples/radix at the pinned
commit), composed from Cosmo's controls: fixtures.py writes them as the /fidelity pages and the desktop tests' templates,
pages.py shows them on the components' pages. The message scroller's examples stream a mock chat the registry does not
hold, so it shows the reference app's support transcript (tools/shadcn/app/src/scroller.tsx)."""

ICONS = {
    "book-open-check": "M12 21V7 M16 12 l2 2 4-4 M22 6V4a1 1 0 0 0-1-1h-5a4 4 0 0 0-4 4 4 4 0 0 0-4-4H3a1 1 0 0 0-1 1v13a1 1 0 0 0 1 1h6a3 3 0 0 1 3 3 3 3 0 0 1 3-3h6a1 1 0 0 0 1-1v-1.3",
    "check": "M20 6 9 17l-5-5",
    "clock": "M2,12 A10,10 0 1 1 22,12 A10,10 0 1 1 2,12 Z M12,6 L12,12 L16,14",
    "file-code": "M10 12.5 8 15l2 2.5 M14 12.5 l2 2.5-2 2.5 M14 2v4a2 2 0 0 0 2 2h4 M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7z",
    "file-text": "M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7Z M14 2v4a2 2 0 0 0 2 2h4 M10 9H8 M16 13H8 M16 17H8",
    "file-warning": "M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7Z M12 9v4 M12 17h.01",
    "git-branch": "M6,3 L6,15 M15,6 A3,3 0 1 1 21,6 A3,3 0 1 1 15,6 Z M3,18 A3,3 0 1 1 9,18 A3,3 0 1 1 3,18 Z M18 9a9 9 0 0 1-9 9",
    "refresh-cw": "M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8 M21 3v5h-5 M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16 M8 16H3v5",
    "search": "M3,11 A8,8 0 1 1 19,11 A8,8 0 1 1 3,11 Z M21 21 l-4.3-4.3",
    "table": "M12 3v18 M5,3 L19,3 A2,2 0 0 1 21,5 L21,19 A2,2 0 0 1 19,21 L5,21 A2,2 0 0 1 3,19 L3,5 A2,2 0 0 1 5,3 Z M3 9h18 M3 15h18",
    "x": "M18 6 6 18 M6 6 l12 12",
    "rotate-ccw": "M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8 M3 3v5h5",
    "file-search": "M14 2v4a2 2 0 0 0 2 2h4 M4.268 21a2 2 0 0 0 1.727 1H18a2 2 0 0 0 2-2V7l-5-5H6a2 2 0 0 0-2 2v3 M9 18 l-1.5-1.5 M2,14 A3,3 0 1 1 8,14 A3,3 0 1 1 2,14 Z",
    "copy": "M10,8 L20,8 A2,2 0 0 1 22,10 L22,20 A2,2 0 0 1 20,22 L10,22 A2,2 0 0 1 8,20 L8,10 A2,2 0 0 1 10,8 Z M4 16c-1.1 0-2-.9-2-2V4c0-1.1.9-2 2-2h10c1.1 0 2 .9 2 2",
    "chevron-down": "M6 9 l6 6 6-6",
}


def icon(kind, size=16, brush="CosmoMutedForegroundBrush"):
    return f'<Viewbox Width="{size}" Height="{size}"><Path Theme="{{DynamicResource CosmoIcon}}" Stroke="{{DynamicResource {brush}}}" Data="{ICONS[kind]}" /></Viewbox>'


def current_icon(kind, size=16, classes=""):
    """An icon in its presenter's color, as an SVG in currentColor follows its text under the pointer."""
    attribute = f' Classes="{classes}"' if classes else ""
    return f'<Viewbox Width="{size}" Height="{size}"{attribute}><Path Theme="{{DynamicResource CosmoIcon}}" Stroke="{{Binding $parent[ContentPresenter].Foreground}}" Data="{ICONS[kind]}" /></Viewbox>'


def column(spacing, *parts, width=384, align="Left"):
    body = "\n".join("  " + part for part in parts)
    return f"""
        <StackPanel Width="{width}" HorizontalAlignment="{align}" Margin="0,48" Spacing="{spacing}">
{body}
        </StackPanel>
        """


def marker(content, kind=None, classes="", spinner=False):
    attribute = f' Classes="{classes}"' if classes else ""
    if spinner:
        return f'<c:Marker{attribute} Content="{content}"><c:Marker.Icon><c:Spinner Width="16" Height="16" /></c:Marker.Icon></c:Marker>'
    if kind:
        return f'<c:Marker{attribute} Content="{content}"><c:Marker.Icon>{icon(kind)}</c:Marker.Icon></c:Marker>'
    return f'<c:Marker{attribute} Content="{content}" />'


def bubble(text, variant="", end=False, reactions=None, reactions_start=False):
    classes = " ".join(part for part in (variant, "end" if end else "", "reactions-start" if reactions_start else "") if part)
    attribute = f' Classes="{classes}"' if classes else ""
    if reactions:
        faces = "".join(f'<TextBlock Text="{face}" />' for face in reactions.split())
        return f'<c:Bubble{attribute} Content="{text}"><c:Bubble.Reactions><StackPanel Orientation="Horizontal" Spacing="4">{faces}</StackPanel></c:Bubble.Reactions></c:Bubble>'
    return f'<c:Bubble{attribute} Content="{text}" />'


def group(*bubbles):
    return "<c:BubbleGroup>" + "".join(bubbles) + "</c:BubbleGroup>"


def avatar(initials):
    return f'<c:Message.Avatar><c:Avatar Content="{initials}" /></c:Message.Avatar>' if initials else '<c:Message.Avatar><Border Width="32" /></c:Message.Avatar>'


def message(content, initials=None, end=False, avatarless=False):
    attribute = ' Classes="end"' if end else ""
    picture = "" if avatarless else avatar(initials)
    return f'<c:Message{attribute}>{picture}<c:MessageContent>{"".join(content)}</c:MessageContent></c:Message>'


MARKERS = {
    "marker-demo": column(32, marker("Switched to a new branch", "git-branch"), marker("Thinking...", spinner=True),
                          marker("Conversation compacted", classes="separator"), marker("Explored 4 files", "search")),
    "marker-variants": column(32, marker("A default marker for inline notes."), marker("A separator marker", classes="separator"),
                              marker("A border marker for row boundaries.", classes="border")),
    "marker-icon": column(48, marker("Switched to a new branch", "git-branch"), marker("Explored 4 files", "search", "separator"),
                          marker("Syncing completed", "book-open-check", "vertical")),
    "marker-border": column(12, marker("Switched to release-candidate", "git-branch", "border"), marker("Reviewed 8 related files", "search", "border"),
                            marker("Opened implementation notes", "file-text", "border")),
}

MARKERS.update({
    "marker-shimmer": column(32, marker("Thinking...", classes="shimmer"), marker("Reading 4 files", classes="separator shimmer")),
    "marker-status": column(32, marker("Compacting conversation", spinner=True), marker("Running tests", classes="separator", spinner=True)),
    "marker-link-button": column(32,
                                 f'<c:MarkerLink NavigateUri="#links-and-buttons" Content="View the pull request"><c:MarkerLink.Icon>{current_icon("git-branch")}</c:MarkerLink.Icon></c:MarkerLink>',
                                 f'<c:MarkerButton Content="Revert this change"><c:MarkerButton.Icon>{current_icon("rotate-ccw")}</c:MarkerButton.Icon></c:MarkerButton>'),
})

COLLAPSED = """The accessibility review found two focus states that were visually too subtle in dark mode.

I checked the dialog, menu, and drawer paths because each one renders focusable controls inside a layered surface.

The dialog and drawer are fine. The menu needs the hover and focus tokens split so keyboard focus stays visible when the pointer is not involved.

I also recommend keeping the change in the style file instead of the primitive so the other themes can choose their own focus treatment later."""


def lines(text):
    return text.replace("&", "&amp;").replace('"', "&quot;").replace("\n", "&#10;")


def collapsible_bubble():
    preview = COLLAPSED[:180] + "..."
    return f"""<c:Bubble Classes="muted end">
          <c:Collapsible>
            <TextBlock c:Collapsible.Shows="Closed" TextWrapping="Wrap" LineHeight="22.75" Text="{lines(preview)}" />
            <TextBlock c:Collapsible.Shows="Open" TextWrapping="Wrap" LineHeight="22.75" Text="{lines(COLLAPSED)}" />
            <c:CollapsibleTrigger Classes="link" Padding="0" HorizontalAlignment="Left" Foreground="{{DynamicResource CosmoMutedForegroundBrush}}">
              <StackPanel Orientation="Horizontal" Spacing="4">
                <TextBlock c:Collapsible.Shows="Closed" Text="Show more" />
                <TextBlock c:Collapsible.Shows="Open" Text="Show less" />
                <Viewbox Width="16" Height="16" Classes="chevron"><Path Theme="{{DynamicResource CosmoIcon}}" Stroke="{{Binding $parent[Button].Foreground}}" Data="{ICONS["chevron-down"]}" /></Viewbox>
              </StackPanel>
            </c:CollapsibleTrigger>
          </c:Collapsible>
        </c:Bubble>"""


BUBBLES = {
    "bubble-demo": column(32, bubble("Hey there! what&apos;s up?", end=True),
                          group(bubble("Hey! Want to see chat bubbles?", "muted"),
                                bubble("I can group messages, switch sides, and keep the whole thread easy to scan.", "muted", reactions="👍")),
                          bubble("Sure. Hit me with your best demo.", end=True),
                          bubble("Yes. You are reading a demo that is demoing itself. Very meta. Very on-brand.", "muted", reactions="👍 🔥 👀 +2")),
    "bubble-variants": column(48, bubble("This is the default primary bubble."), bubble("This is the secondary variant.", "secondary", end=True),
                              bubble("This one is muted. It uses a lower emphasis color for the chat bubble.", "muted", reactions="👍"),
                              bubble("This one is tinted. The tint is a softer color derived from the primary color.", "tinted", end=True),
                              bubble("We can also use an outlined variant.", "outline"),
                              bubble("Or a destructive variant with a reaction.", "destructive", end=True, reactions="🔥"),
                              bubble("Ghost bubbles work for assistant text and other content that should not be framed. They are full width and can take the full width of the container.", "ghost")),
    "bubble-alignment": column(32, bubble("This bubble is aligned to the start. This is the default alignment.", "muted"),
                               bubble("This bubble is aligned to the end. Use this for user messages.", end=True)),
    "bubble-group-demo": column(32, bubble("Can you tell me what&apos;s the issue?", "muted"),
                                group(bubble("You tell me!", end=True), bubble("It worked yesterday. You broke it!", end=True),
                                      bubble("Find the bug and fix it.", end=True, reactions="👀", reactions_start=True)),
                                bubble("Want me to diff yesterday&apos;s you against today&apos;s you? It&apos;s a bit embarrassing.", "muted")),
}

BUBBLES.update({
    "bubble-link-button": column(32, bubble("How can I help you today?", "muted"),
                                 group(*(f'<c:Bubble Classes="tinted end"><Button Content="{text}" /></c:Bubble>' for text in (
                                     "I forgot my password", "I need help with my subscription", "Something else. Talk to a human.")))),
    "bubble-collapsible": column(32, bubble("How can I help you today?", "muted"), collapsible_bubble()),
})

FOOTER = '<c:MessageFooter Text="Delivered" />'

MESSAGES = {
    "message-demo": column(24, message([bubble("Deploying to prod real quick.")], "ME", end=True),
                           message([bubble("It&apos;s 4:55 PM. On a Friday.", "muted")], "R"),
                           message([bubble("It&apos;s a one-line change."), FOOTER], "ME", end=True),
                           message([group(bubble("It&apos;s always a one-line change 😭.", "muted"), bubble("Alright, let me take a look.", "muted", reactions="👍"))], "R"),
                           '<c:Marker><c:Marker.Content><TextBlock><Run Text="Oliver" FontWeight="Medium" /><Run Text=" is typing..." /></TextBlock></c:Marker.Content></c:Marker>'),
    "message-group": column(24, '<c:MessageGroup>' + message([bubble("I checked the registry addresses.", "muted")])
                            + message([bubble("The component and example JSON now live under the UI registry.", "muted")], "CN") + '</c:MessageGroup>'),
    "message-header-footer": column(32, message(['<c:MessageHeader Text="Olivia" />', bubble("I already checked the logs.", "muted")], avatarless=True),
                                    message([bubble("Send the report to the team. Ping @shadcn if you need help."),
                                             '<c:MessageFooter><Run Text="Read " /><Run Text="Yesterday" FontWeight="Normal" /></c:MessageFooter>'], end=True, avatarless=True)),
    "message-avatar": column(24, message([bubble("The build failed during dependency installation.", "muted")], "R"),
                             message([bubble("Can you share the exact error?")], "R", end=True),
                             message([group(bubble("Here&apos;s the error from the logs", "muted"),
                                            bubble("Something went wrong with the build. The libraries are not installed correctly. Try running the build again.", "muted"))], "R")),
}

def action(kind, name):
    return f'<Button Classes="ghost icon-xs" AutomationProperties.Name="{name}">{icon(kind, 12, "CosmoForegroundBrush")}</Button>'


def attachment(title, description=None, media=None, state=None, classes="", actions=(), width=None, stretch=True):
    attributes = f' Title="{title}"' + (f' Description="{description}"' if description else "")
    attributes += f' State="{state}"' if state else ""
    attributes += f' Classes="{classes}"' if classes else ""
    attributes += f' Width="{width}"' if width else (' HorizontalAlignment="Stretch"' if stretch else "")
    parts = f"<c:Attachment.Media>{media}</c:Attachment.Media>" if media else ""
    if actions:
        parts += '<c:Attachment.Actions><StackPanel Orientation="Horizontal">' + "".join(action(kind, name) for kind, name in actions) + "</StackPanel></c:Attachment.Actions>"
    return f"<c:Attachment{attributes}>{parts}</c:Attachment>"


def file_icon(kind, size=16):
    return icon(kind, size, "CosmoForegroundBrush")


def picture(name):
    return f'<Image Source="/Assets/{name}.png" Stretch="UniformToFill" />'


SPINNER = '<c:Spinner Width="16" Height="16" />'

ATTACHMENTS = {
    "attachment-demo": column(12,
                              '<c:AttachmentGroup>' + "".join(attachment(title, meta, picture(art), classes="vertical image", stretch=False) for title, meta, art in (
                                  ("workspace.png", "PNG · 820 KB", "art-binni"), ("desk-reference.jpg", "JPG · 1.1 MB", "art-byrom"),
                                  ("office-reference.jpg", "JPG · 940 KB", "art-malyavko"))) + '</c:AttachmentGroup>',
                              attachment("sales-dashboard.pdf", "Uploading · 64%", SPINNER, "Uploading", actions=[("x", "Cancel upload")]),
                              attachment("message-renderer.tsx", "TypeScript · 12 KB", file_icon("file-code"), actions=[("x", "Remove message-renderer.tsx")]),
                              align="Center"),
    "attachment-states": column(8,
                                attachment("selected-file.pdf", "Ready to upload", file_icon("clock"), "Idle", actions=[("x", "Remove selected-file.pdf")]),
                                attachment("design-system.zip", "Uploading · 64%", SPINNER, "Uploading", actions=[("x", "Cancel upload")]),
                                attachment("market-research.pdf", "Processing document", file_icon("file-text"), "Processing", actions=[("x", "Remove market-research.pdf")]),
                                attachment("financial-model.xlsx", "Upload failed. Try again.", file_icon("file-warning"), "Error",
                                           actions=[("refresh-cw", "Retry upload"), ("x", "Remove financial-model.xlsx")]),
                                attachment("uploaded-report.pdf", "Uploaded · 1.8 MB", file_icon("check"), "Done", actions=[("x", "Remove uploaded-report.pdf")]),
                                align="Center"),
    "attachment-sizes": column(12,
                               attachment("Default attachment", "PDF · 2.4 MB", file_icon("file-text")),
                               attachment("Small attachment", "PDF · 2.4 MB", file_icon("file-text"), classes="sm"),
                               attachment("Extra small attachment", None, file_icon("file-text", 14), classes="xs"),
                               align="Center"),
    "attachment-group": column(0,
                               '<c:AttachmentGroup>' + "".join(attachment(title, meta, media, classes=classes, width=256, actions=[("x", f"Remove {title}")]) for title, meta, media, classes in (
                                   ("briefing-notes.pdf", "PDF · 1.4 MB", file_icon("file-text"), ""), ("workspace.png", "PNG · 820 KB", picture("art-binni"), "image"),
                                   ("customers.csv", "CSV · 18 KB", file_icon("table"), ""), ("renderer.tsx", "TSX · 12 KB", file_icon("file-code"), ""))) + '</c:AttachmentGroup>',
                               align="Center"),
}

def trigger_attachment():
    return f"""<StackPanel>
          <c:Attachment Title="research-summary.pdf" Description="Open preview dialog" HorizontalAlignment="Stretch">
            <c:Attachment.Media>{file_icon("file-search")}</c:Attachment.Media>
            <c:Attachment.Actions><StackPanel Orientation="Horizontal">{action("copy", "Copy link")}{action("x", "Remove research-summary.pdf")}</StackPanel></c:Attachment.Actions>
            <c:Attachment.Trigger><Button x:Name="previewTrigger" Theme="{{DynamicResource CosmoAttachmentTrigger}}" AutomationProperties.Name="Preview research-summary.pdf" /></c:Attachment.Trigger>
          </c:Attachment>
          <c:Dialog Trigger="{{Binding #previewTrigger}}" MaxWidth="448">
            <c:DialogHeader Title="research-summary.pdf" Description="The attachment trigger fills the card and opens the dialog, while the actions stay independently clickable above it." />
          </c:Dialog>
        </StackPanel>"""


def image_attachment(title, meta, art):
    return f"""<c:Attachment Title="{title}" Description="{meta}" Classes="vertical image">
              <c:Attachment.Media>{picture(art)}</c:Attachment.Media>
              <c:Attachment.Actions>{action("x", f"Remove {title}")}</c:Attachment.Actions>
              <c:Attachment.Trigger><HyperlinkButton Theme="{{DynamicResource CosmoAttachmentTrigger}}" NavigateUri="/Assets/{art}.png" AutomationProperties.Name="Open {title}" /></c:Attachment.Trigger>
            </c:Attachment>"""


ATTACHMENTS.update({
    "attachment-trigger": column(0, trigger_attachment(), align="Center"),
    "attachment-image": column(0, '<c:AttachmentGroup HorizontalAlignment="Center">' + "".join(image_attachment(*art) for art in (
        ("workspace.png", "PNG · 820 KB", "art-binni"), ("desk-reference.jpg", "JPG · 1.1 MB", "art-byrom"),
        ("office-reference.jpg", "JPG · 940 KB", "art-malyavko"))) + '</c:AttachmentGroup>', align="Center"),
})

TRANSCRIPT = [
    (True, "Deploying to prod real quick."), (False, "It&apos;s 4:55 PM. On a Friday."), (True, "It&apos;s a one-line change."),
    (False, "It&apos;s always a one-line change. Alright, let me take a look."), (True, "The build failed during dependency installation."),
    (False, "Can you share the exact error?"), (True, "Something went wrong with the build. The libraries are not installed correctly."),
    (False, "Try running the build again. I cleared the cache."), (True, "It worked. Thanks!"),
]


def scroller(position):
    rows = "\n".join(f"        {message([bubble(text, '' if mine else 'muted', end=mine)], end=mine, avatarless=True)}" for mine, text in TRANSCRIPT)
    return f"""
<c:Card Width="384" Height="400" Padding="0" ClipToBounds="True" HorizontalAlignment="Center" VerticalAlignment="Top">
  <c:Card.ItemsPanel><ItemsPanelTemplate><Panel /></ItemsPanelTemplate></c:Card.ItemsPanel>
  <DockPanel>
    <Border DockPanel.Dock="Top" Padding="24,16,24,24" BorderThickness="0,0,0,1" BorderBrush="{{DynamicResource CosmoBorderBrush}}">
      <StackPanel Spacing="4">
        <TextBlock Text="Support" FontWeight="SemiBold" LineHeight="16" />
        <TextBlock Text="A conversation about a failed deploy." FontSize="14" LineHeight="20" Foreground="{{DynamicResource CosmoMutedForegroundBrush}}" />
      </StackPanel>
    </Border>
    <c:MessageScroller DefaultScrollPosition="{position}">
      <StackPanel Margin="24" Spacing="16">
{rows}
      </StackPanel>
    </c:MessageScroller>
  </DockPanel>
</c:Card>
"""


SCROLLERS = {
    "message-scroller-end": scroller("End"),
    "message-scroller-start": scroller("Start"),
}

BUBBLES["bubble-markdown"] = column(32,
    '<c:Bubble Classes="muted end"><c:Markdown Text="Hello! Are you actually **thinking**?" /></c:Bubble>',
    '<c:Bubble Classes="ghost"><c:Markdown Text="Ghost bubbles work for assistant text, **markdown**, and other content that should not be framed.&#10;&#10;'
    'This is perfect for assistant messages that should not have a frame and can take the full width of the container. You can also render `code` in it.&#10;&#10;'
    'Ghost bubbles are full width and can take the full width of the container.&#10;" /></c:Bubble>')

FIXTURES = {**MARKERS, **BUBBLES, **MESSAGES, **ATTACHMENTS, **SCROLLERS}

