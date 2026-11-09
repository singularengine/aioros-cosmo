"""shadcn/ui's trace examples (the radix base in the radix-rhea style, apps/v4/examples/radix at the pinned commit, copied in
tools/shadcn/registry/radix/examples), composed from Cosmo's c:Trace, c:TraceLine and the trace Expander: fixtures.py
writes them as the /fidelity pages and the desktop tests' templates, pages.py shows them on the trace's page."""
import re

from chat import ICONS
import chat
from codeblocks import LUCIDE, block, content, plain, source, text_line

PATHS = {
    **ICONS,
    **LUCIDE,
    "git-branch": "M6,3 L6,15 M15,6 A3,3 0 1 1 21,6 A3,3 0 1 1 15,6 Z M3,18 A3,3 0 1 1 9,18 A3,3 0 1 1 3,18 Z M18 9a9 9 0 0 1-9 9",
    "circle-check": "M2,12 A10,10 0 1 1 22,12 A10,10 0 1 1 2,12 Z M9 12 l2 2 4-4",
    "circle-alert": "M2,12 A10,10 0 1 1 22,12 A10,10 0 1 1 2,12 Z M12,8 L12,12 M12,16 L12.01,16",
    "circle": "M2,12 A10,10 0 1 1 22,12 A10,10 0 1 1 2,12 Z",
    "file-pen": "M12.5 22H18a2 2 0 0 0 2-2V7l-5-5H6a2 2 0 0 0-2 2v9.5 M14 2v4a2 2 0 0 0 2 2h4 M13.378 15.626a1 1 0 1 0-3.004-3.004l-5.01 5.012a2 2 0 0 0-.506.854l-.837 2.87a.5.5 0 0 0 .62.62l2.87-.837a2 2 0 0 0 .854-.506z",
    "chevron-right": "M9 18 l6-6-6-6",
}


def icon(kind):
    if kind == "loader-circle":
        return '<c:Spinner Width="16" Height="16" />'
    return f'<Viewbox Width="16" Height="16"><Path Theme="{{DynamicResource CosmoIcon}}" Stroke="{{Binding $parent[c:TraceLine].Foreground}}" Data="{PATHS[kind]}" /></Viewbox>'


def line(title, kind=None, status=None, description=None, mono=False, trailing=None):
    attributes = f' Content="{title}"' + (f' Status="{status}"' if status else "")
    parts = f"<c:TraceLine.Icon>{icon(kind)}</c:TraceLine.Icon>" if kind else ""
    if description and mono:
        parts += f'<c:TraceLine.Description><TextBlock Text="{description}" FontFamily="{{DynamicResource CosmoFontMono}}" /></c:TraceLine.Description>'
    elif description:
        attributes += f' Description="{description}"'
    if trailing:
        parts += f"<c:TraceLine.Trailing>{icon(trailing)}</c:TraceLine.Trailing>"
    return f"<c:TraceLine{attributes}>{parts}</c:TraceLine>" if parts else f"<c:TraceLine{attributes} />"


def trace(*parts, width=384):
    size = f' Width="{width}" HorizontalAlignment="Left"' if width else ""
    body = "\n".join("  " + part for part in parts)
    return f"<c:Trace{size}>\n{body}\n</c:Trace>"


def collapsible(header, content, open_=False, width=None):
    size = f' Width="{width}" HorizontalAlignment="Left"' if width else ""
    expanded = ' IsExpanded="True"' if open_ else ""
    return f'<Expander Classes="trace"{size}{expanded}>\n  <Expander.Header>{header}</Expander.Header>\n  {content}\n</Expander>'


def small_code(text):
    return block(plain(text, wrap=True), classes="sm", width=None)


def badges(*names):
    return '<WrapPanel ItemSpacing="6" LineSpacing="6">' + "".join(f'<c:Badge Classes="secondary" Content="{name}" />' for name in names) + "</WrapPanel>"


FIXTURES = {
    "trace-static": trace(line("Created branch fix/frozen-lockfile", "git-branch")),
    "trace-status": trace(line("Ran pnpm install", "circle-check"), line("Failed to run pnpm lint", "circle-alert", "Error"),
                          line("Running pnpm build", "loader-circle", "Running"), line("Run pnpm test", "circle", "Pending")),
    "trace-description": trace(line("Edited components/settings/notification-preferences-form.tsx", "file-pen", description="+48 −12", mono=True),
                               line("Ran pnpm test", "terminal", description="14s"), line("Searched for useChat", "search", description="8 results")),
    "trace-group": trace(line("Searched for minimumReleaseAge", "search"), line("Read pnpm-workspace.yaml", "file-text"), line("Ran pnpm install", "terminal")),
    "trace-collapsible": collapsible(line("Ran node --version", "terminal"), small_code("$ node --version\nv24.18.0"), width=384),
    "trace-steps": collapsible(line("Looking into the slow deploy", status="Running"), trace(
        collapsible(line("Searched for the checkout build log", "search"), badges("vercel.com", "github.com")),
        '<Border a:Web.Href="#steps" HorizontalAlignment="Left">' + line("Read the last twelve deploys", "file-text", trailing="chevron-right") + "</Border>",
        collapsible(line("Searching for the cache key", "search", "Running"), small_code("cache key image optimization")),
        width=None), open_=True, width=384),
}

FIXTURES["code-block-trace"] = trace(
    collapsible(line("Ran node --version", "terminal"), small_code(source("code-block-trace", "output")), open_=True),
    collapsible(line("Failed to run pnpm dev", "terminal", "Error"), block(plain(source("code-block-trace", "error"), wrap=True), classes="sm destructive", width=None), open_=True),
    width=448)




def lucide(name):
    """A Lucide icon's path data, read from lucide-react 0.474.0 (tools/shadcn's), its circles, lines, polylines and rects as paths."""
    import re
    from pathlib import Path
    text = (Path(__file__).resolve().parents[3] / "tools" / "shadcn" / "node_modules" / "lucide-react" / "dist" / "esm" / "icons" / f"{name}.js").read_text()
    parts = []
    for kind, body in re.findall(r'\[\s*"(\w+)",\s*\{(.*?)\}\s*\]', text, re.S):
        attrs = dict(re.findall(r'(\w+): "([^"]*)"', body))
        if kind == "path":
            parts.append(attrs["d"])
        elif kind == "circle":
            cx, cy, r = (float(attrs[k]) for k in ("cx", "cy", "r"))
            parts.append(f"M{cx - r:g},{cy:g} A{r:g},{r:g} 0 1 1 {cx + r:g},{cy:g} A{r:g},{r:g} 0 1 1 {cx - r:g},{cy:g} Z")
        elif kind == "line":
            parts.append(f'M{attrs["x1"]},{attrs["y1"]} L{attrs["x2"]},{attrs["y2"]}')
        elif kind == "polyline":
            points = attrs["points"].split()
            parts.append("M" + " L".join(points))
        elif kind == "rect":
            x, y, w, h = (float(attrs[k]) for k in ("x", "y", "width", "height"))
            r = float(attrs.get("rx", 0))
            parts.append(f"M{x + r:g},{y:g} L{x + w - r:g},{y:g} A{r:g},{r:g} 0 0 1 {x + w:g},{y + r:g} L{x + w:g},{y + h - r:g} A{r:g},{r:g} 0 0 1 {x + w - r:g},{y + h:g} "
                         f"L{x + r:g},{y + h:g} A{r:g},{r:g} 0 0 1 {x:g},{y + h - r:g} L{x:g},{y + r:g} A{r:g},{r:g} 0 0 1 {x + r:g},{y:g} Z")
    return " ".join(parts)


for _name in ("globe", "arrow-up-right", "folder-search", "git-commit-horizontal", "file-plus", "file-text", "brain", "terminal"):
    PATHS.setdefault(_name, lucide(_name))


def acting(tag, line_, attributes=""):
    """A trace line that acts: inside a link or a button of the trace class, as shadcn/ui's lines inside an a or a trigger."""
    return f'<{tag} Classes="trace"{attributes}>{line_}</{tag}>'


def link(title, kind, href="#link", trailing="arrow-up-right"):
    """A line in a link; a link to another site is named as opening in a new tab, as shadcn/ui's screen-reader text names it."""
    name = f' AutomationProperties.Name="{title} (opens in a new tab)"' if href.startswith("http") else ""
    return acting("HyperlinkButton", line(title, kind, trailing=trailing), f' NavigateUri="{href}"{name}')


def opener(name, title, kind, overlay, description=None, mono=False):
    """A line that opens a dialog or a sheet (a trigger of the trace class named for it) and the overlay."""
    return acting("Button", line(title, kind, description=description, mono=mono, trailing="chevron-right"), f' x:Name="{name}"') + overlay.replace("TRIGGER", f"{{Binding #{name}}}")


def paragraph(text):
    return f'<TextBlock Text="{text}" TextWrapping="Wrap" Foreground="{{DynamicResource CosmoForegroundBrush}}" />'


SOURCES = [("Install Astro | docs.astro.build", "https://docs.astro.build/en/install-and-setup/"), ("create-astro | npmjs.com", "https://www.npmjs.com/package/create-astro"),
           ("Settings: minimumReleaseAge | pnpm.io", "https://pnpm.io/settings#minimumreleaseage")]
RESULTS = [("Caching build outputs", "https://nextjs.org/docs/app/guides/caching"), ("Image optimization reference", "https://nextjs.org/docs/app/api-reference/components/image"),
           ("Deploy hooks and cache keys", "https://vercel.com/docs/deploy-hooks")]
DIFFS = [("app/login/page.tsx", False, 2, 2, '''@@ -14,7 +14,7 @@ export default async function LoginPage({
   searchParams,
 }) {
   const { next } = await searchParams
-  const redirectTo =
-    next && next.startsWith("/") ? next : "/"
+  const redirectTo = safeRedirect(next)
+
   return <LoginForm redirectTo={redirectTo} />
 }'''), ("lib/auth/redirect.ts", True, 7, 0, '''@@ -0,0 +1,7 @@
+export function safeRedirect(next: string | undefined) {
+  if (!next || !next.startsWith("/") || next.startsWith("//")) {
+    return "/"
+  }
+
+  return next
+}''')]


def diff_lines(diff):
    lines = []
    for row in diff.split("\n"):
        if row.startswith("@@"):
            lines.append(text_line(row, "hunk"))
        else:
            lines.append(text_line(row[1:], {"+": "added", "-": "removed"}.get(row[:1], "")))
    return content(lines)


def committed(index, path, added, additions, deletions, diff):
    sheet = f'''<c:Sheet Trigger="TRIGGER"><StackPanel Spacing="16"><c:DialogHeader Title="{path}" Description="{additions} additions, {deletions} deletions" Classes="mono" />
      {block(diff_lines(diff), width=None).replace("<c:CodeBlock", '<c:CodeBlock Margin="24,0,24,24"', 1)}</StackPanel></c:Sheet>'''
    return opener(f"commitFile{index}", f'{"Added" if added else "Edited"} {path}', "file-plus" if added else "file-pen", sheet, f"+{additions} −{deletions}", mono=True)


def column(*parts, spacing=16, width=384):
    size = f' Width="{width}" HorizontalAlignment="Left"' if width else ""
    return f'<StackPanel{size} Spacing="{spacing}">\n' + "\n".join(parts) + "\n</StackPanel>"


def asked(question, *answer, replay="Replay"):
    return f'''<StackPanel Width="384" HorizontalAlignment="Left" Spacing="16" Margin="0,24">
  {chat.message([chat.bubble(question, "muted")], end=True, avatarless=True)}
  {chat.message(list(answer), avatarless=True).replace("<c:MessageContent>", '<c:MessageContent Spacing="16">', 1)}
  <Button Classes="outline sm" Height="28" Padding="12,0" CornerRadius="16" HorizontalAlignment="Left" Content="{replay}" />
</StackPanel>'''


def shown(xaml, binding):
    """The element shown while a model's flag is set."""
    return re.sub(r"^<([\w:.]+)", lambda match: f'<{match.group(1)} IsVisible="{{Binding {binding}}}"', xaml, count=1)


def bound_line(title, kind, status, running_kind=None, running=None):
    """A trace line whose title and look follow the model: its icon a spinner while it runs when running_kind says so."""
    icons = icon(kind) if not running_kind else f'<Panel>{shown(icon(running_kind), running)}{shown(icon(kind), running.replace("Running", "Done"))}</Panel>'
    return f'<c:TraceLine Content="{{Binding {title}}}" Status="{{Binding {status}}}"><c:TraceLine.Icon>{icons}</c:TraceLine.Icon></c:TraceLine>'


def thinking(kind, running_kind=None):
    """The reasoning row: a plain line before its first word, then a collapsible open while it runs, as shadcn/ui's ThinkingRow."""
    header = bound_line("Chat.ThinkingTitle", kind, "Chat.ReasoningStatus", running_kind, "Chat.ReasoningRunning")
    plain_line = shown(header, "Chat.ReasoningEmpty")
    folding = collapsible(header, '<TextBlock Text="{Binding Chat.Reasoning}" TextWrapping="Wrap" />').replace('<Expander Classes="trace"', '<Expander Classes="trace" IsVisible="{Binding Chat.ReasoningShown}" IsExpanded="{Binding Chat.ReasoningOpen, Mode=TwoWay}"', 1)
    return plain_line + "\n" + folding


def tool(index, command, done, failed=False, running_kind="terminal", open_=False):
    """A tool call: a running line while it runs, then its collapsible output."""
    running = shown(line(f"Running {command}", running_kind, "Running"), f"Chat.ToolsRunning[{index}]")
    return running + "\n" + shown(done, f"Chat.ToolsShown[{index}]")


def streamed(model, question, parts, answer, action="Replay", fill=False):
    """A scripted turn bound to its Gallery model: the question, the answer's parts and text, and the action below them."""
    user = shown(chat.message([chat.bubble(question, "muted")], end=True, avatarless=True), "Chat.Asked")
    reply = shown(chat.message([parts, shown(paragraph("{Binding Chat.Answer}"), "Chat.HasAnswer")], avatarless=True).replace("<c:MessageContent>", '<c:MessageContent Spacing="16">', 1), "Chat.Answering")
    button = f'<Button Classes="outline sm" Height="28" Padding="12,0" CornerRadius="16" HorizontalAlignment="Left" Content="{action}" IsVisible="{{Binding Chat.Idle}}" Command="{{Binding {"Act" if fill else "Replay"}Command}}" />'
    context = f"<StackPanel.DataContext><m:{model} /></StackPanel.DataContext>" if not fill else f"<DockPanel.DataContext><m:{model} /></DockPanel.DataContext>"
    if fill:
        return f'''<DockPanel Width="384" HorizontalAlignment="Left" MinHeight="240" Margin="0,24">
  {context}
  {button.replace("<Button", '<Button DockPanel.Dock="Bottom" VerticalAlignment="Bottom" Margin="0,16,0,0"', 1)}
  <StackPanel Spacing="16" VerticalAlignment="Top">{user}{reply}</StackPanel>
</DockPanel>'''
    return f'''<StackPanel Width="384" HorizontalAlignment="Left" Spacing="16" Margin="0,24">
  {context}
  {user}
  {reply}
  {button}
</StackPanel>'''


DEMO_ANSWER = "The last commit bumped a version in package.json without updating pnpm-lock.yaml, so the frozen install fails. Run pnpm install locally and commit the lockfile."

FIXTURES.update({
    "trace-link": trace(link("Read apps/v4/lib/docs.ts", "file-text", trailing="chevron-right"), link("Install Astro | docs.astro.build", "globe", SOURCES[0][1])),
    "trace-sources": collapsible(line("Used 3 sources", "globe"), trace(*(link(title, "globe", href) for title, href in SOURCES), width=None), open_=True, width=384),
    "trace-dialog": trace(opener("webSearch", "Searched the web for cache keys", "globe", '''<c:Dialog Trigger="TRIGGER" MaxWidth="512"><StackPanel Spacing="16">
      <c:DialogHeader Title="Searched the web for cache keys" Description="Three results were used in the response." />''' + trace(*(link(title, "globe", url) for title, url in RESULTS), width=None) + "</StackPanel></c:Dialog>")),
    "trace-sheet": trace(opener("exploredFiles", "Explored 4 files", "folder-search", '''<c:Sheet Trigger="TRIGGER"><StackPanel Spacing="16">
      <c:DialogHeader Title="Explored 4 files" Description="Files read while preparing the response." />
      <StackPanel Spacing="8" Margin="24,0">''' + "".join(f'<TextBlock Text="{path}" FontFamily="{{DynamicResource CosmoFontMono}}" FontSize="12" />' for path in ("apps/v4/lib/docs.ts", "apps/v4/lib/components.ts", "apps/v4/next.config.mjs", "apps/v4/package.json")) + "</StackPanel></StackPanel></c:Sheet>")),
    "trace-commit": trace(collapsible(line("Committed a1b2c3: Fix the login redirect", "git-commit-horizontal"), trace(*(committed(i, *d) for i, d in enumerate(DIFFS)), width=None), open_=True)),
    "trace-open": '<c:Collapsible Width="384" HorizontalAlignment="Left" Spacing="16">\n' + collapsible(line("Ran node --version", "terminal"), small_code("$ node --version\nv24.18.0"), open_=True)
                  + "\n" + collapsible(line("Ran pnpm --version", "terminal"), small_code("$ pnpm --version\n10.33.4")).replace('<Expander Classes="trace"', '<Expander Classes="trace" IsExpanded="{Binding $parent[c:Collapsible].IsOpen, Mode=TwoWay}"', 1)
                  + '\n<c:CollapsibleTrigger Classes="outline sm" Height="28" Padding="12,0" CornerRadius="16" HorizontalAlignment="Left"><StackPanel Orientation="Horizontal">'
                  + '<TextBlock c:Collapsible.Shows="Closed" Text="Show the pnpm output" /><TextBlock c:Collapsible.Shows="Open" Text="Hide the pnpm output" /></StackPanel></c:CollapsibleTrigger>\n</c:Collapsible>',
    "trace-turn": collapsible(line("Worked for 2m 4s, 1 command failed", status="Error"), column(
        trace(collapsible(line("Thought", "brain", description="12s"), '<TextBlock Text="I should check which package managers are installed before I scaffold anything." TextWrapping="Wrap" />'),
              collapsible(line("Ran 2 commands, 1 failed", "terminal", "Error"), trace(line("Ran node --version", "terminal"),
                          collapsible(line("Failed to run pnpm --version", "terminal", "Error"), block(plain("command not found: pnpm", wrap=True), classes="sm destructive", width=None)), width=None), open_=True), width=None),
        paragraph("Node is installed and pnpm is not. Reading the docs config.").replace('Foreground="{DynamicResource CosmoForegroundBrush}" ', ""),
        trace(link("Read apps/v4/lib/docs.ts", "file-text", "#turn", "chevron-right"), width=None), spacing=8, width=None), open_=True, width=384),
    "trace-demo": streamed("TraceDemoExample", "Why does pnpm install fail in CI?", trace(
        thinking("circle-check", "loader-circle"),
        tool(0, "pnpm --version", collapsible(line("Ran pnpm --version", "circle-check"), small_code("10.18.0")), running_kind="loader-circle"),
        tool(1, "git diff --stat HEAD~1 -- package.json pnpm-lock.yaml", collapsible(line("Ran git diff --stat HEAD~1 -- package.json pnpm-lock.yaml", "circle-check"), small_code(" package.json | 2 +-\n 1 file changed, 1 insertion(+), 1 deletion(-)"), open_=True), running_kind="loader-circle"),
        tool(2, "pnpm install --frozen-lockfile", collapsible(line("Failed to run pnpm install --frozen-lockfile", "circle-alert", "Error"), block(plain('ERR_PNPM_OUTDATED_LOCKFILE  Cannot install with "frozen-lockfile" because pnpm-lock.yaml is not up to date with package.json'.replace('"', "&quot;"), wrap=True), classes="sm destructive", width=None)), running_kind="loader-circle"),
        shown(collapsible(bound_line("Chat.SourcesTitle", "globe", "Chat.SourcesStatus", "loader-circle", "Chat.SourcesRunning"), trace(link("pnpm install | pnpm.io", "globe", "https://pnpm.io/cli/install#--frozen-lockfile"), link("Continuous Integration | pnpm.io", "globe", "https://pnpm.io/continuous-integration"), width=None)), "Chat.SourcesShown"),
        width=None), DEMO_ANSWER),
    "trace-thinking": streamed("TraceThinkingExample", "Why is the checkout deploy so slow?", trace(thinking("brain"), width=None), None, "{Binding Chat.Action}", fill=True),
    "trace-tool-calls": streamed("TraceToolCallsExample", "Which package managers are installed?", trace(
        tool(0, "node --version", collapsible(line("Ran node --version", "terminal"), small_code("v24.18.0"))),
        tool(1, "npm --version", collapsible(line("Ran npm --version", "terminal"), small_code("11.6.2"))),
        tool(2, "pnpm --version", collapsible(line("Failed to run pnpm --version", "terminal", "Error"), small_code("command not found: pnpm"))),
        width=None), None, "{Binding Chat.Action}", fill=True),
})
