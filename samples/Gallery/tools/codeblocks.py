"""shadcn/ui's code block examples (the radix base in the radix-rhea style, apps/v4/examples/radix at the pinned commit,
copied in tools/shadcn/registry/radix/examples), composed from Cosmo's c:CodeBlock: fixtures.py writes them as the
/fidelity pages and the desktop tests' templates, pages.py shows them on the code block's page.

The code is highlighted here, when the XAML is written, by the shiki shadcn/ui uses (tools/shadcn/highlight.mjs: github-light
and github-dark): each token becomes a Run on one of Cosmo's code brushes, so both targets draw the same XAML and the
browser runs no highlighter. The tokens are kept in highlighted.json, which highlight.mjs refreshes when the code changes."""
import json
import re
import subprocess
from pathlib import Path

import chat
from chat import ICONS

TOOLS = Path(__file__).resolve().parent
EXAMPLES = TOOLS.parents[2] / "tools" / "shadcn" / "registry" / "radix" / "examples"
CACHE = TOOLS / "highlighted.json"

BRUSHES = {
    ("#24292E", "#E1E4E8"): "CosmoCodeTextBrush",
    ("#D73A49", "#F97583"): "CosmoCodeKeywordBrush",
    ("#032F62", "#9ECBFF"): "CosmoCodeStringBrush",
    ("#6F42C1", "#B392F0"): "CosmoCodeFunctionBrush",
    ("#005CC5", "#79B8FF"): "CosmoCodeConstantBrush",
    ("#E36209", "#FFAB70"): "CosmoCodeVariableBrush",
    ("#6A737D", "#6A737D"): "CosmoCodeCommentBrush",
    ("#22863A", "#85E89D"): "CosmoCodeTagBrush",
    ("#24292E80", "#E1E4E880"): "CosmoCodeTextDimBrush",
}
ANSI = {"Black": ("#24292E", "#586069"), "Blue": ("#0366D6", "#2188FF"), "BrightBlack": ("#959DA5", "#959DA5"), "BrightBlue": ("#005CC5", "#79B8FF"),
        "BrightCyan": ("#3192AA", "#56D4DD"), "BrightGreen": ("#22863A", "#85E89D"), "BrightMagenta": ("#5A32A3", "#B392F0"), "BrightRed": ("#CB2431", "#F97583"),
        "BrightWhite": ("#D1D5DA", "#FAFBFC"), "BrightYellow": ("#B08800", "#FFEA7F"), "Cyan": ("#1B7C83", "#39C5CF"), "Green": ("#28A745", "#34D058"),
        "Magenta": ("#5A32A3", "#B392F0"), "Red": ("#D73A49", "#EA4A5A"), "White": ("#6A737D", "#D1D5DA"), "Yellow": ("#DBAB09", "#FFEA7F")}
for name, pair in ANSI.items():
    BRUSHES.setdefault(pair, f"CosmoCode{'Ansi' + name}Brush")

LUCIDE = {
    "terminal": "M4,17 L10,11 L4,5 M12,19 L20,19",
    "wrap-text": "M3,6 L21,6 M3 12h15a3 3 0 1 1 0 6h-4 M16,16 L14,18 L16,20 M3,18 L10,18",
    "download": "M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4 M7,10 L12,15 L17,10 M12,15 L12,3",
    "file-code": ICONS["file-code"],
    "rotate-ccw": ICONS["rotate-ccw"],
}


def source(example, name):
    """A template literal or a string constant of an example, its ${...} filled from the example's own constants."""
    text = (EXAMPLES / f"{example}.tsx").read_text()
    constants = dict(re.findall(r'^const (\w+) = "([^"]*)"$', text, re.M))
    literal = re.search(rf"^const {name} = `((?:[^`\\]|\\.)*)`$", text, re.M | re.S)
    value = literal.group(1) if literal else constants[name]
    return re.sub(r"\$\{(\w+)\}", lambda match: constants[match.group(1)], value)


def highlight(samples, cache=None):
    """The tokens of each {name: (code, language)}, from the cache or from shiki."""
    CACHE = TOOLS / cache if cache else globals()["CACHE"]
    cache = json.loads(CACHE.read_text()) if CACHE.exists() else {}
    wanted = {name: {"code": code, "language": language} for name, (code, language) in samples.items()}
    if any(cache.get(name, {}).get("source") != sample for name, sample in wanted.items()):
        shadcn = TOOLS.parents[2] / "tools" / "shadcn"
        tokens = json.loads(subprocess.run(["node", "highlight.mjs"], cwd=shadcn, input=json.dumps(wanted), capture_output=True, text=True, check=True).stdout)
        cache = {name: {"source": wanted[name], "lines": tokens[name]} for name in wanted}
        CACHE.write_text(json.dumps(cache, indent=1, ensure_ascii=False) + "\n")
    return {name: cache[name]["lines"] for name in wanted}


def escape(text):
    """The text as a XAML attribute value: escaped, and a leading brace marked as text rather than a markup extension."""
    value = text.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;").replace('"', "&quot;")
    return "{}" + value if value.startswith("{") else value


def icon(kind, size=16, brush="{Binding $parent[Button].Foreground}"):
    return f'<Viewbox Width="{size}" Height="{size}"><Path Theme="{{DynamicResource CosmoIcon}}" Stroke="{brush}" Data="{LUCIDE[kind]}" /></Viewbox>'


def text_line(text, classes=""):
    attribute = f' Classes="{classes}"' if classes else ""
    return f'<c:CodeBlockLine{attribute}><TextBlock Text="{escape(text) or " "}" /></c:CodeBlockLine>'


def token_line(tokens, classes=""):
    attribute = f' Classes="{classes}"' if classes else ""
    if not tokens:
        return f'<c:CodeBlockLine{attribute}><TextBlock Text=" " /></c:CodeBlockLine>'
    runs = "".join(f'<Run Text="{escape(token[0])}" Foreground="{{DynamicResource {BRUSHES[(token[1].upper(), token[2].upper())]}}}" />' for token in tokens)
    return f"<c:CodeBlockLine{attribute}><TextBlock>{runs}</TextBlock></c:CodeBlockLine>"


def content(lines, numbers=False, wrap=False):
    attributes = (' LineNumbers="True"' if numbers else "") + (' Wrap="True"' if wrap else "")
    body = "\n".join("      " + line for line in lines)
    return f"""<c:CodeBlockContent{attributes}>
    <StackPanel>
{body}
    </StackPanel>
  </c:CodeBlockContent>"""


def title(text, kind=None, size=16):
    if not kind:
        return f' Title="{escape(text)}"', ""
    return "", f'<c:CodeBlock.Title><StackPanel Orientation="Horizontal" Spacing="6">{icon(kind, size, "{DynamicResource CosmoForegroundBrush}")}<TextBlock Text="{escape(text)}" VerticalAlignment="Center" /></StackPanel></c:CodeBlock.Title>'


def actions(*buttons):
    return '<c:CodeBlock.Actions><StackPanel Orientation="Horizontal" Spacing="4">' + "".join(buttons) + "</StackPanel></c:CodeBlock.Actions>"


def action(kind, name):
    return f'<Button Classes="code-block" AutomationProperties.Name="{name}">{icon(kind, 12)}</Button>'


def copy(value):
    return f'<c:CodeBlockCopyButton Value="{escape(value).replace(chr(10), "&#10;")}" />'


def block(body, heading=None, kind=None, buttons=None, classes="", width=448, size=16):
    title_attribute, title_element = title(heading, kind, size) if heading else ("", "")
    attribute = title_attribute + (f' Classes="{classes}"' if classes else "")
    parts = title_element + (actions(*buttons) if buttons else "")
    size = f' Width="{width}" HorizontalAlignment="Left"' if width else ""
    return f"""<c:CodeBlock{size}{attribute}>
  {parts}
  {body}
</c:CodeBlock>"""


def plain(text, numbers=False, wrap=False, variants=None):
    return content([text_line(line, (variants or {}).get(index, "")) for index, line in enumerate(text.split("\n"))], numbers, wrap)


def diff(patch):
    variants = {"+": "added", "-": "removed"}
    return content([text_line(line[1:], variants.get(line[:1], "")) for line in patch.split("\n")])


def column(*blocks, spacing=16):
    return f'<StackPanel Spacing="{spacing}">\n' + "\n".join(blocks) + "\n</StackPanel>"


DEMO = source("code-block-demo", "code")
TOKENS = highlight({"code-block-demo": (DEMO, "ts"), "code-block-highlighting": (source("code-block-highlighting", "code"), "json")})
HEADER = source("code-block-header", "output")
SIZE = source("code-block-size", "output")

FIXTURES = {
    "code-block-demo": block(content([token_line(line, "highlighted" if 5 <= index <= 9 else "") for index, line in enumerate(TOKENS["code-block-demo"])], numbers=True),
                             "app/api/chat/route.ts", "file-code", [action("wrap-text", "Wrap lines"), action("download", "Download"), copy(DEMO)], width=512),
    "code-block-basic": block(plain(source("code-block-basic", "output"))),
    "code-block-header": block(plain(HEADER), "Terminal", "terminal", [copy(source("code-block-header", "command"))]),
    "code-block-line-numbers": block(plain(source("code-block-line-numbers", "code"), numbers=True)),
    "code-block-highlighted-lines": block(plain(source("code-block-highlighted-lines", "code"), numbers=True, variants={2: "highlighted", 3: "highlighted"})),
    "code-block-diff": block(diff(source("code-block-diff", "patch")), "components/greeting.tsx", buttons=[copy(source("code-block-diff", "patch"))]),
    "code-block-destructive": block(plain(source("code-block-destructive", "error"), wrap=True), "Error", buttons=[copy(source("code-block-destructive", "error"))], classes="destructive"),
    "code-block-size": column(block(plain(SIZE, wrap=True), "Terminal", "terminal", [copy(SIZE)]),
                              block(plain(SIZE, wrap=True), "Terminal", "terminal", [copy(SIZE)], classes="sm", size=14)),
    "code-block-highlighting": block(content([token_line(line) for line in TOKENS["code-block-highlighting"]]), "getWeather output"),
}


def objects(example, name):
    """The objects of an array constant of an example: their string, template, number-list and boolean fields."""
    text = (EXAMPLES / f"{example}.tsx").read_text()
    body = re.search(rf"^const {name}(?:: [^=]+)? = \[(.*?)^\]", text, re.M | re.S).group(1)
    found = []
    for chunk in re.split(r"^  \{$", body, flags=re.M)[1:]:
        item = {}
        for key, quoted, template, listed, flag in re.findall(r'^    (\w+): (?:"([^"]*)"|`(.*?)`|\[([\d, ]*)\]|(true|false))', chunk, re.M | re.S):
            item[key] = [int(n) for n in listed.split(",")] if listed else flag == "true" if flag else template if template else quoted
        found.append(item)
    return found


def lines_of(tokens, code):
    """The highlighted lines, or the code's lines as plain text for a language the highlighter does not list."""
    return [token_line(line) for line in tokens] if tokens is not None else [text_line(line) for line in code.split("\n")]


def floating(body, *buttons):
    return f"""<c:CodeBlock Width="448" HorizontalAlignment="Left">
  {actions(*buttons)}
  {body}
</c:CodeBlock>"""


def tagged(body, classes):
    return body.replace("<c:CodeBlockContent", f'<c:CodeBlockContent Classes="{classes}"', 1)


def show_more(code, tokens, scroll=False):
    body = tagged(content([token_line(line) for line in tokens], numbers=True), "show-more scroll" if scroll else "show-more")
    trigger = ('<c:CollapsibleTrigger Classes="ghost xs" HorizontalAlignment="Center"><StackPanel Orientation="Horizontal" Spacing="4">'
               f'<Viewbox Width="16" Height="16" Classes="chevron"><Path Theme="{{DynamicResource CosmoIcon}}" Stroke="{{Binding $parent[Button].Foreground}}" Data="{ICONS["chevron-down"]}" /></Viewbox>'
               '<TextBlock c:Collapsible.Shows="Closed" Text="Show more" /><TextBlock c:Collapsible.Shows="Open" Text="Show less" /></StackPanel></c:CollapsibleTrigger>')
    inner = block(body, "components/chat.tsx", buttons=[copy(code)]).replace("</c:CodeBlock>", f"  <c:CodeBlock.Footer>{trigger}</c:CodeBlock.Footer>\n</c:CodeBlock>")
    return f'<c:Collapsible Width="448" HorizontalAlignment="Left">\n{inner}\n</c:Collapsible>'


ACTIONS = source("code-block-actions", "output")
ANSI_OUTPUT = source("code-block-ansi", "output").replace("\\x1b", "\x1b")
FLOATING = source("code-block-floating-actions", "output")
LOGS = "\n".join(f"[{index + 1:02}] GET /api/chat 200 in {12 + ((index * 7) % 40)}ms" for index in range(40))
LANGUAGES = objects("code-block-languages", "samples")
SHOW_MORE = source("code-block-show-more", "code")
START = source("code-block-start-line", "code")
STREAMS = objects("code-block-streaming", "blocks")
FILES = objects("code-block-tabs", "files")
TYPESET = source("code-block-typeset", "code")
WRAP = source("code-block-wrap", "output")
RTL = source("code-block-rtl", "code")
ARABIC = "avares://Aioros.Cosmo/Assets/Fonts#Geist, avares://Aioros.Cosmo/Assets/Fonts#Cosmo Symbols, avares://Aioros.Cosmo/Assets/Fonts#Cosmo Emoji, avares://Gallery/Assets/Fonts#Noto Naskh Arabic"
CHAT = source("code-block-chat", "code")
MORE = highlight({
    "code-block-ansi": (ANSI_OUTPUT, "ansi"),
    **{f"code-block-languages-{index}": (sample["code"], sample["language"]) for index, sample in enumerate(LANGUAGES)},
    "code-block-show-more": (SHOW_MORE, "tsx"),
    "code-block-start-line": (START, "tsx"),
    **{f"code-block-streaming-{index}": ("\n".join(row[1:] for row in item["code"].split("\n")) if item.get("diff") else item["code"], item["language"]) for index, item in enumerate(STREAMS)},
    **{f"code-block-tabs-{index}": (item["code"], item["language"]) for index, item in enumerate(FILES)},
    "code-block-typeset": (TYPESET, "ts"),
    "code-block-rtl": (RTL, "bash"),
    "code-block-chat": (CHAT, "ts"),
}, cache="highlighted-more.json")


def stream_script():
    """Where each block, line and token of code-block-streaming starts in the stream (the blocks' code one after the other,
    a diff's markers included, as shadcn/ui's example slices it), for the Gallery's model to play back."""
    blocks, lines, tokens, offset = [], [], [], 0
    for index, item in enumerate(STREAMS):
        rows, marker = item["code"].split("\n"), 1 if item.get("diff") else 0
        start = offset
        for row_index, (row, highlighted) in enumerate(zip(rows, MORE[f"code-block-streaming-{index}"])):
            row_start = start + sum(len(row) + 1 for row in rows[:row_index])
            lines.append((row_start, index))
            at = row_start + marker
            for token in highlighted:
                tokens.append((at, token[0]))
                at += len(token[0])
        offset += len(item["code"])
        blocks.append((start, offset))
    return blocks, lines, tokens


def stream_block(index, item, lines, tokens):
    rows = item["code"].split("\n")
    variants = ({i: {"+": "added", "-": "removed"}.get(row[:1], "") for i, row in enumerate(rows)} if item.get("diff")
                else {i: "highlighted" for i in item.get("highlight", [])})
    numbered = []
    for i, line in enumerate(MORE[f"code-block-streaming-{index}"]):
        visible = f' IsVisible="{{Binding Lines[{lines.pop(0)}]}}"'
        classes = f' Classes="{variants[i]}"' if variants.get(i) else ""
        if not line:
            numbered.append(f'<c:CodeBlockLine{classes}{visible}><TextBlock Text=" " /></c:CodeBlockLine>')
            continue
        runs = "".join(f'<Run Text="{{Binding Tokens[{tokens.pop(0)}]}}" Foreground="{{DynamicResource {BRUSHES[(token[1].upper(), token[2].upper())]}}}" />' for token in line)
        numbered.append(f"<c:CodeBlockLine{classes}{visible}><TextBlock>{runs}</TextBlock></c:CodeBlockLine>")
    body = content(numbered, numbers=item.get("lineNumbers", False))
    value = "\n".join(row[1:] for row in rows) if item.get("diff") else item["code"]
    copy_button = f'<c:CodeBlockCopyButton Value="{escape(value).replace(chr(10), "&#10;")}" IsEnabled="{{Binding Copies[{index}]}}" />'
    return block(body, item.get("title"), buttons=[copy_button] if item.get("title") else None, width=None).replace("<c:CodeBlock", f'<c:CodeBlock IsVisible="{{Binding Blocks[{index}]}}"', 1)


def stream_model():
    """The generated script the Gallery's CodeBlockStreaming model plays back."""
    blocks, lines, tokens = stream_script()
    numbers = lambda values: ", ".join(str(value) for value in values)
    texts = ", ".join(json.dumps(text, ensure_ascii=False) for _, text in tokens)
    return f"""namespace Gallery.Models;

/// <summary>Where code-block-streaming's blocks, lines and tokens start in its stream, written by samples/Gallery/tools/codeblocks.py.</summary>
internal static class CodeBlockStreamingScript
{{
    public static readonly int[] BlockStarts = [{numbers(start for start, _ in blocks)}];

    public static readonly int[] BlockEnds = [{numbers(end for _, end in blocks)}];

    public static readonly int[] LineStarts = [{numbers(start for start, _ in lines)}];

    public static readonly int[] LineBlocks = [{numbers(block for _, block in lines)}];

    public static readonly int[] TokenStarts = [{numbers(start for start, _ in tokens)}];

    public static readonly string[] TokenTexts = [{texts}];
}}
"""


def stream_fixture():
    _, lines, tokens = stream_script()
    line_indices, token_indices = list(range(len(lines))), list(range(len(tokens)))
    blocks = "\n".join(stream_block(index, item, line_indices, token_indices) for index, item in enumerate(STREAMS))
    return (f'<StackPanel Width="448" HorizontalAlignment="Left" Spacing="16">\n  <StackPanel.DataContext><m:CodeBlockStreaming /></StackPanel.DataContext>\n' + blocks
            + f'\n<Button Classes="outline sm" Height="28" Padding="12,0" CornerRadius="16" HorizontalAlignment="Left" IsEnabled="{{Binding Idle}}" Command="{{Binding ReplayCommand}}"><StackPanel Orientation="Horizontal" Spacing="4"><Viewbox Width="16" Height="16"><Path Theme="{{DynamicResource CosmoIcon}}" Stroke="{{Binding $parent[Button].Foreground}}" Data="{LUCIDE["rotate-ccw"]}" /></Viewbox><TextBlock Text="Replay" /></StackPanel></Button>\n</StackPanel>')


(TOOLS.parent / "Gallery" / "Models" / "CodeBlockStreamingScript.cs").write_text(stream_model())


def tab_items():
    headers = "".join(f'<TabItem Header="{item["title"].split("/")[-1]}">{block(content([token_line(line) for line in MORE[f"code-block-tabs-{index}"]], numbers=True), item["title"], buttons=[copy(item["code"])], width=None)}</TabItem>'
                      for index, item in enumerate(FILES))
    return f'<TabControl Width="448" HorizontalAlignment="Left" SelectedIndex="0">{headers}</TabControl>'


def paragraph(*runs, size=16, line=28, margin=0):
    inlines = "".join(f'<InlineUIContainer><Border Classes="typeset-code"><TextBlock Text="{text}" /></Border></InlineUIContainer>' if code else f'<Run Text="{escape(text)}" />' for text, code in runs)
    return f'<TextBlock Classes="typeset" TextWrapping="Wrap" FontSize="{size}" LineHeight="{line}" Margin="0,{margin},0,0">{inlines}</TextBlock>'


FIXTURES.update({
    "code-block-actions": '<c:Collapsible IsOpen="True" Width="448" HorizontalAlignment="Left">' + block(tagged(plain(ACTIONS), "wrap-toggle"), "Terminal", "terminal", [f'<c:CollapsibleTrigger Classes="code-block" AutomationProperties.Name="Wrap lines">{icon("wrap-text", 12, "{Binding $parent[Button].Foreground}")}</c:CollapsibleTrigger>', action("download", "Download"), copy(ACTIONS)], width=None) + "</c:Collapsible>",
    "code-block-ansi": block(content([token_line(line) for line in MORE["code-block-ansi"]], wrap=True), "Terminal", "terminal", [copy(re.sub(r"\x1b\[[\d;]*m", "", ANSI_OUTPUT))]),
    "code-block-floating-actions": floating(plain(FLOATING), copy(FLOATING)),
    "code-block-height": block(tagged(plain(LOGS), "scroll-fade-y").replace("<c:CodeBlockContent", '<c:CodeBlockContent MaxHeight="192"', 1), "Logs", buttons=[copy(LOGS)]),
    "code-block-languages": column(*(block(content(lines_of(MORE[f"code-block-languages-{index}"], sample["code"])), sample["title"]) for index, sample in enumerate(LANGUAGES))),
    "code-block-show-more": show_more(SHOW_MORE, MORE["code-block-show-more"]),
    "code-block-show-more-scroll": show_more(SHOW_MORE, MORE["code-block-show-more"], scroll=True),
    "code-block-start-line": block(content([token_line(line) for line in MORE["code-block-start-line"]], numbers=True).replace('LineNumbers="True"', 'LineNumbers="True" StartLine="995"'), "app/dashboard/page.tsx"),
    "code-block-streaming": stream_fixture(),
    "code-block-tabs": tab_items(),
    "code-block-typeset": f'''<StackPanel Width="448" HorizontalAlignment="Left">
  <TextBlock Classes="typeset" Text="Stream a response" FontSize="18" FontWeight="SemiBold" LineHeight="26.1" />
  {paragraph(("Create a route handler that calls ", False), ("streamText", True), (" and returns the stream to the client.", False), margin=16)}
  {block(content([token_line(line) for line in MORE["code-block-typeset"]], numbers=True), "app/api/chat/route.ts", "file-code", [copy(TYPESET)], width=None).replace("<c:CodeBlock", '<c:CodeBlock Margin="0,17.5,0,0"', 1)}
  {paragraph(("The ", False), ("useChat", True), (" hook reads the stream on the client and renders the answer while it is written.", False), margin=20)}
</StackPanel>''',
    "code-block-wrap": column(block(plain(WRAP)), block(plain(WRAP, wrap=True))),
    "code-block-rtl": f'''<StackPanel Width="448" HorizontalAlignment="Left" Spacing="12" FlowDirection="RightToLeft" TextElement.FontFamily="{ARABIC}">
  <TextBlock Text="ثبّت الحزم، ثم شغّل خادم التطوير." LineHeight="20" />
  {block(content([token_line(line) for line in MORE["code-block-rtl"]], numbers=True), "الطرفية", "terminal", [copy(RTL)], width=None)}
</StackPanel>''',
})

FIXTURES["code-block-chat"] = f'''<StackPanel Width="448" HorizontalAlignment="Left" Spacing="16">
  {chat.message([chat.bubble("How do I stream a response?", "muted")], end=True, avatarless=True)}
  {chat.message(['<TextBlock Text="Create a route handler that returns the stream:" TextWrapping="Wrap" />',
                 block(content([token_line(line) for line in MORE["code-block-chat"]]), "app/api/chat/route.ts", buttons=[copy(CHAT)], width=None),
                 '<TextBlock Text="The client reads it with useChat." TextWrapping="Wrap" />'], avatarless=True).replace("<c:MessageContent>", '<c:MessageContent Spacing="12">', 1)}
</StackPanel>'''


def markdown(text, numbers=True):
    """A c:Markdown of a text, as an attribute value keeping its line breaks."""
    attribute = "" if numbers else ' LineNumbers="False"'
    return f'<c:Markdown Text="{escape(text).replace(chr(10), "&#10;")}"{attribute} />'


ANSWER = source("code-block-markdown", "response").replace("\\`", "`")
FENCE = re.search(r"```tsx\n(.*?)\n```", ANSWER, re.S).group(1)
highlight({"code-block-markdown": (FENCE, "tsx")}, cache="../Gallery/Assets/highlighted-markdown.json")

FIXTURES["code-block-markdown"] = f'''<StackPanel Width="448" HorizontalAlignment="Left" Spacing="16" Margin="0,24">
  <StackPanel.DataContext><m:CodeBlockMarkdownExample /></StackPanel.DataContext>
  {chat.message([chat.bubble("How do I debounce a search input in React?", "muted")], end=True, avatarless=True).replace("<c:Message", '<c:Message IsVisible="{Binding Chat.Asked}"', 1)}
  {chat.message(['<c:Markdown Text="{Binding Chat.Answer}" LineNumbers="False" />'], avatarless=True).replace("<c:Message>", '<c:Message IsVisible="{Binding Chat.Answering}">', 1)}
  <Button Classes="outline sm" Height="28" Padding="12,0" CornerRadius="16" HorizontalAlignment="Left" Content="Replay" IsVisible="{{Binding Chat.Idle}}" Command="{{Binding ReplayCommand}}" />
</StackPanel>'''
