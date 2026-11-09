"""shadcn/ui's app shell blocks (new-york-v4 dashboard-01, sidebar-01 to sidebar-16) composed from Cosmo's components: their
bodies, which fixtures.py writes as the /fidelity pages and the desktop tests' templates at 1280 x 800, and pages.py as the
/blocks pages. Their texts, icons and data are read from the pinned registry (tools/shadcn/registry/blocks)."""
import json
import re
from pathlib import Path
from textwrap import dedent, indent

ROOT = Path(__file__).resolve().parents[3]
REGISTRY = ROOT / "tools" / "shadcn" / "registry" / "blocks"

WIDTH = 1280
HEIGHT = 800

from shell_icons import ICONS


def source(block, name="components/app-sidebar.tsx"):
    return (REGISTRY / block / name).read_text()


def literal(text):
    """A TypeScript object literal of the registry's sample data as Python data: unquoted keys quoted, identifiers (icons) kept as their names."""
    text = re.sub(r"//[^\n]*", "", text)
    text = re.sub(r'([{,]\s*)([A-Za-z_][A-Za-z0-9_]*)\s*:', r'\1"\2":', text)
    text = re.sub(r':\s*([A-Z][A-Za-z0-9]*)\s*([,}\]])', r': "\1"\2', text)
    text = re.sub(r",\s*([}\]])", r"\1", text)
    return json.loads(text)


def data(block, name="components/app-sidebar.tsx", variable="data"):
    text = source(block, name)
    after = text.index(f"const {variable} =")
    start = min(index for index in (text.find("{", after), text.find("[", after)) if index >= 0)
    depth = 0
    for index in range(start, len(text)):
        depth += {"{": 1, "}": -1, "[": 1, "]": -1}.get(text[index], 0)
        if depth == 0:
            return literal(text[start:index + 1])
    raise ValueError(block)


def esc(value):
    return value.replace("&", "&amp;").replace('"', "&quot;").replace("<", "&lt;")


def icon(kind, extra="", size=16, brush="CosmoSidebarForegroundBrush"):
    """A Lucide or Tabler icon by its React name, as Cosmo draws icons: its geometry in a Viewbox, stroked (or filled) with a brush."""
    path, filled = ICONS[kind]
    color = f"{{DynamicResource {brush}}}"
    paint = f'Fill="{color}" StrokeThickness="0"' if filled else f'Stroke="{color}"'
    return f'<Viewbox Width="{size}" Height="{size}"{extra}><Path Theme="{{DynamicResource CosmoIcon}}" {paint} Data="{path}" /></Viewbox>'


def lines(*parts):
    return "\n".join(part for part in parts if part)


def block_indent(text, spaces):
    return indent(text, " " * spaces)


# The documentation shells (sidebar-01 to sidebar-06)

LOGO = ('<Border Width="32" Height="32" CornerRadius="{DynamicResource CosmoRadiusLg}" Background="{DynamicResource CosmoSidebarPrimaryBrush}">'
        + icon("GalleryVerticalEnd", brush="CosmoSidebarPrimaryForegroundBrush") + '</Border>')


def docs_title(version, binding=False):
    text = '{Binding VersionText}' if binding else f"v{version}"
    return lines(
        '<StackPanel Margin="8,0,0,0" Spacing="2" VerticalAlignment="Center">',
        '  <TextBlock Text="Documentation" FontWeight="Medium" LineHeight="14" />',
        f'  <TextBlock Text="{text}" LineHeight="14" />',
        '</StackPanel>')


def version_switcher(versions):
    shows = {"1.0.1": "ShowsStable", "1.1.0-alpha": "ShowsAlpha", "2.0.0-beta1": "ShowsBeta"}
    items = "\n".join(
        f'      <MenuItem Command="{{Binding SelectVersion}}" CommandParameter="{v}"><MenuItem.Header><DockPanel>{icon("Check", dock("Right") + " Margin=" + chr(34) + "8,0,0,0" + chr(34) + " IsVisible=" + chr(34) + "{Binding " + shows[v] + "}" + chr(34), brush="CosmoPopoverForegroundBrush")}<TextBlock Text="v{v}" /></DockPanel></MenuItem.Header></MenuItem>'
        for v in versions)
    return lines(
        '<c:SidebarMenu>',
        '  <c:SidebarMenuItem>',
        '    <c:SidebarMenuButton Classes="lg">',
        '      <DockPanel>',
        f'        {icon("ChevronsUpDown", " DockPanel.Dock=" + chr(34) + "Right" + chr(34) + " Margin=" + chr(34) + "8,0,0,0" + chr(34))}',
        f'        {LOGO}',
        block_indent(docs_title(versions[0], binding=True), 8),
        '      </DockPanel>',
        '      <c:SidebarMenuButton.Flyout>',
        '        <MenuFlyout Placement="BottomEdgeAlignedLeft">',
        items,
        '        </MenuFlyout>',
        '      </c:SidebarMenuButton.Flyout>',
        '    </c:SidebarMenuButton>',
        '  </c:SidebarMenuItem>',
        '</c:SidebarMenu>')


def docs_brand():
    return lines(
        '<c:SidebarMenu>',
        '  <c:SidebarMenuItem>',
        '    <c:SidebarMenuButton Classes="lg">',
        '      <DockPanel>',
        f'        {LOGO}',
        block_indent(docs_title("1.0.0"), 8),
        '      </DockPanel>',
        '    </c:SidebarMenuButton>',
        '  </c:SidebarMenuItem>',
        '</c:SidebarMenu>')


SEARCH = lines(
    '<Panel Margin="8,0">',
    '  <c:SidebarInput Padding="32,4,12,4" PlaceholderText="Search the docs..." AutomationProperties.Name="Search" />',
    '  ' + icon("Search", ' HorizontalAlignment="Left" VerticalAlignment="Center" Margin="8,0,0,0" Opacity="0.5" IsHitTestVisible="False"', brush="CosmoForegroundBrush"),
    '</Panel>')

TILE = '<c:AspectRatio Ratio="1.7778"><Border CornerRadius="{DynamicResource CosmoRadiusXl}" Background="{DynamicResource CosmoMutedHalfBrush}" /></c:AspectRatio>'


def crumbs(link="Build Your Application", page="Data Fetching"):
    return lines(
        '<c:Breadcrumb VerticalAlignment="Center">',
        f'  <HyperlinkButton Content="{esc(link)}" NavigateUri="#" />',
        f'  <c:BreadcrumbPage Text="{esc(page)}" />',
        '</c:Breadcrumb>')


def header(border=True, padding="16,0", trigger_margin="-4,0,0,0", content=None, sticky=False, full_rule=False):
    """shadcn's inset header: `flex h-16 shrink-0 items-center gap-2 px-4` with the trigger, a 16 px rule and the breadcrumb;
    a block that sizes its rule with a bare `h-4` gets the separator's own `data-[orientation=vertical]:h-full` instead, as tall as the header."""
    rule = ' BorderThickness="0,0,0,1" BorderBrush="{DynamicResource CosmoBorderBrush}"' if border else ""
    background = ' Background="{DynamicResource CosmoBackgroundBrush}"' if sticky else ""
    return lines(
        f'<Border DockPanel.Dock="Top" Height="64" Padding="{padding}"{rule}{background}>',
        '  <StackPanel Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">',
        f'    <c:SidebarTrigger Margin="{trigger_margin}" VerticalAlignment="Center" />',
        f'    <Separator Classes="vertical" Height="{(63 if border else 64) if full_rule else 16}" Margin="0,0,8,0" VerticalAlignment="Center" />',
        block_indent(content or crumbs(), 4),
        '  </StackPanel>',
        '</Border>')


def tiles(margin="16"):
    return lines(
        f'<DockPanel Margin="{margin}">',
        '  <UniformGrid DockPanel.Dock="Top" Columns="3" ColumnSpacing="16" Margin="0,0,0,16">',
        f'    {TILE}', f'    {TILE}', f'    {TILE}',
        '  </UniformGrid>',
        '  <Border CornerRadius="{DynamicResource CosmoRadiusXl}" Background="{DynamicResource CosmoMutedHalfBrush}" />',
        '</DockPanel>')


def shell(sidebar, inset, provider="", model=None, styles=None):
    context = f"\n  <c:SidebarProvider.DataContext><m:{model} /></c:SidebarProvider.DataContext>" if model else ""
    return lines(
        f'<c:SidebarProvider Width="{WIDTH}" Height="{HEIGHT}" MobileBreakpoint="0"{provider}{f' x:DataType="m:{model}"' if model else ""}>{context}',
        lines('  <c:SidebarProvider.Styles>', block_indent(styles, 4), '  </c:SidebarProvider.Styles>') if styles else None,
        block_indent(sidebar, 2),
        '  <c:SidebarInset>',
        '    <DockPanel>',
        block_indent(inset, 6),
        '    </DockPanel>',
        '  </c:SidebarInset>',
        '</c:SidebarProvider>')


def sidebar(header_part, content, footer=None, rail=True, attributes=""):
    return lines(
        f'<c:Sidebar{attributes}>',
        '  <c:SidebarHeader>',
        block_indent(header_part, 4),
        '  </c:SidebarHeader>',
        '  <c:SidebarContent>',
        block_indent(content, 4),
        '  </c:SidebarContent>',
        lines('  <c:SidebarFooter>', block_indent(footer, 4), '  </c:SidebarFooter>') if footer else None,
        '  <c:SidebarRail />' if rail else None,
        '</c:Sidebar>')


def menu_button(title, active=False, classes=None, extra=""):
    attributes = (' IsActive="True"' if active else "") + (f' Classes="{classes}"' if classes else "") + extra
    return f'<c:SidebarMenuButton{attributes} Content="{esc(title)}" />'


def sub_buttons(items):
    return [f'<c:SidebarMenuSubButton Content="{esc(item["title"])}"{" IsActive=" + chr(34) + "True" + chr(34) if item.get("isActive") else ""} />' for item in items]


def docs_groups(nav):
    """sidebar-01: a labelled group of links per section."""
    groups = []
    for group in nav:
        items = "\n".join(f'    <c:SidebarMenuItem>{menu_button(item["title"], item.get("isActive", False))}</c:SidebarMenuItem>' for item in group["items"])
        groups.append(lines('<c:SidebarGroup>', f'  <c:SidebarGroupLabel Content="{esc(group["title"])}" />', '  <c:SidebarMenu>', items, '  </c:SidebarMenu>', '</c:SidebarGroup>'))
    return "\n".join(groups)


def docs_collapsible_groups(nav):
    """sidebar-02: a collapsible group per section, its label the trigger (text-sm, the chevron turning 90 degrees), open at first."""
    groups = []
    for group in nav:
        items = "\n".join(f'      <c:SidebarMenuItem>{menu_button(item["title"], item.get("isActive", False))}</c:SidebarMenuItem>' for item in group["items"])
        groups.append(lines(
            '<Expander Classes="sidebar-group" IsExpanded="True">',
            f'  <Expander.Header><TextBlock Text="{esc(group["title"])}" /></Expander.Header>',
            '  <c:SidebarMenu>', items, '  </c:SidebarMenu>',
            '</Expander>'))
    return lines('<StackPanel>', block_indent("\n".join(groups), 2), '</StackPanel>')


def docs_tree(nav, floating=False):
    """sidebar-03 and sidebar-04: each section a medium link over a sub-menu of its pages."""
    items = []
    for group in nav:
        sub = sub_buttons(group.get("items", []))
        submenu = lines(f'  <c:SidebarMenuSub{" Classes=" + chr(34) + "flat" + chr(34) if floating else ""}>', *[f"    {line}" for line in sub], '  </c:SidebarMenuSub>') if sub else None
        items.append(lines('<c:SidebarMenuItem>', f'  {menu_button(group["title"], extra=" FontWeight=" + chr(34) + "Medium" + chr(34))}', submenu, '</c:SidebarMenuItem>'))
    spacing = ' Classes="loose"' if floating else ""
    return lines('<c:SidebarGroup>', f'  <c:SidebarMenu{spacing}>', block_indent("\n".join(items), 4), '  </c:SidebarMenu>', '</c:SidebarGroup>')


def docs_accordion(nav):
    """sidebar-05: each section a collapsible item with a plus or minus, the second open."""
    items = []
    for index, group in enumerate(nav):
        sub = sub_buttons(group.get("items", []))
        expanded = ' IsExpanded="True"' if index == 1 else ""
        items.append(lines(
            f'<c:SidebarMenuItem IsCollapsible="True"{expanded} Classes="plus">',
            f'  {menu_button(group["title"])}',
            lines('  <c:SidebarMenuSub>', *[f"    {line}" for line in sub], '  </c:SidebarMenuSub>') if sub else None,
            '</c:SidebarMenuItem>'))
    return lines('<c:SidebarGroup>', '  <c:SidebarMenu>', block_indent("\n".join(items), 4), '  </c:SidebarMenu>', '</c:SidebarGroup>')


def docs_dropdowns(nav):
    """sidebar-06: each section a button with an ellipsis that opens its pages in a menu to its right."""
    items = []
    for group in nav:
        entries = "\n".join(f'        <MenuItem Header="{esc(item["title"])}" />' for item in group.get("items", []))
        items.append(lines(
            '<c:SidebarMenuItem>',
            '  <c:SidebarMenuButton>',
            f'    <DockPanel>{icon("MoreHorizontal", " DockPanel.Dock=" + chr(34) + "Right" + chr(34))}<TextBlock Text="{esc(group["title"])}" /></DockPanel>',
            '    <c:SidebarMenuButton.Flyout>',
            '      <MenuFlyout Placement="RightEdgeAlignedTop">',
            entries,
            '      </MenuFlyout>',
            '    </c:SidebarMenuButton.Flyout>',
            '  </c:SidebarMenuButton>',
            '</c:SidebarMenuItem>'))
    return lines('<c:SidebarGroup>', '  <c:SidebarMenu>', block_indent("\n".join(items), 4), '  </c:SidebarMenu>', '</c:SidebarGroup>')


OPT_IN = lines(
    '<c:Card Classes="flat" Margin="4" Padding="0,16">',
    '  <c:Card.ItemsPanel><ItemsPanelTemplate><StackPanel Spacing="8" /></ItemsPanelTemplate></c:Card.ItemsPanel>',
    '  <c:CardHeader Padding="16,0" Description="Opt-in to receive updates and news about the sidebar.">',
    '    <c:CardHeader.Title><TextBlock Text="Subscribe to our newsletter" FontSize="14" LineHeight="14" /></c:CardHeader.Title>',
    '  </c:CardHeader>',
    '  <c:CardContent Padding="16,0">',
    '    <StackPanel Spacing="10">',
    '      <c:SidebarInput PlaceholderText="Email" a:Web.InputType="email" AutomationProperties.Name="Email" />',
    '      <Button Classes="sm sidebar-primary" Content="Subscribe" HorizontalAlignment="Stretch" HorizontalContentAlignment="Center" />',
    '    </StackPanel>',
    '  </c:CardContent>',
    '</c:Card>')


def docs():
    one = data("sidebar-01")
    fixtures = {
        "sidebar-01": shell(sidebar(lines(version_switcher(one["versions"]), SEARCH), docs_groups(one["navMain"])), lines(header(), tiles()), model="DocsExample"),
        "sidebar-02": shell(sidebar(lines(version_switcher(data("sidebar-02")["versions"]), SEARCH), docs_collapsible_groups(data("sidebar-02")["navMain"])),
                            lines(header(sticky=True, full_rule=True), lines('<StackPanel Margin="16" Spacing="16">', *['  <Border Height="48" CornerRadius="{DynamicResource CosmoRadiusLg}" Background="{DynamicResource CosmoMutedHalfBrush}" />'] * 24, '</StackPanel>')),
                            model="DocsExample"),
        "sidebar-03": shell(sidebar(docs_brand(), docs_tree(data("sidebar-03")["navMain"])), lines(header(padding="12,0", trigger_margin="0", full_rule=True), tiles())),
        "sidebar-04": shell(sidebar(docs_brand(), docs_tree(data("sidebar-04")["navMain"], floating=True), rail=False, attributes=' Variant="Floating" ExpandedWidth="304"'),
                            lines(header(border=False), tiles("16,0,16,16"))),
        "sidebar-05": shell(sidebar(lines(docs_brand(), SEARCH), docs_accordion(data("sidebar-05")["navMain"])), lines(header(), tiles())),
        "sidebar-06": shell(sidebar(docs_brand(), docs_dropdowns(data("sidebar-06")["navMain"]), footer=OPT_IN), lines(header(), tiles())),
    }
    return fixtures


# The app shells (sidebar-07 to sidebar-16)

def dock(side):
    return f' DockPanel.Dock="{side}"'


def two_lines(title, subtitle, title_binding=False, subtitle_binding=False):
    first = "{Binding ActiveTeam.Name}" if title_binding else esc(title)
    second = "{Binding ActiveTeam.Plan}" if subtitle_binding else esc(subtitle)
    return lines(
        '<StackPanel Margin="8,0,0,0" VerticalAlignment="Center">',
        f'  <TextBlock Text="{first}" FontWeight="Medium" LineHeight="17.5" TextTrimming="CharacterEllipsis" />',
        f'  <TextBlock Text="{second}" FontSize="12" LineHeight="15" TextTrimming="CharacterEllipsis" />',
        '</StackPanel>')


def logo_tile(kind=None, binding=False, size=32, glyph=16, radius="CosmoRadiusLg"):
    logos = [("GalleryVerticalEnd", "ShowsGallery"), ("AudioWaveform", "ShowsWaveform"), ("Command", "ShowsCommand")]
    inner = ("<Panel>" + "".join(icon(logo, " IsVisible=" + chr(34) + "{Binding " + shows + "}" + chr(34), size=glyph, brush="CosmoSidebarPrimaryForegroundBrush") for logo, shows in logos) + "</Panel>"
             if binding else icon(kind, size=glyph, brush="CosmoSidebarPrimaryForegroundBrush"))
    return f'<Border Width="{size}" Height="{size}" CornerRadius="{{DynamicResource {radius}}}" Background="{{DynamicResource CosmoSidebarPrimaryBrush}}">{inner}</Border>'


def team_menu(placement="RightEdgeAlignedTop"):
    return lines(
        f'<MenuFlyout Placement="{placement}">',
        '  <MenuItem Classes="label" Header="Teams" />',
        '  <MenuItem Header="Acme Inc" InputGesture="Ctrl+D1" Command="{Binding SelectTeam}" CommandParameter="Acme Inc" />',
        '  <MenuItem Header="Acme Corp." InputGesture="Ctrl+D2" Command="{Binding SelectTeam}" CommandParameter="Acme Corp." />',
        '  <MenuItem Header="Evil Corp." InputGesture="Ctrl+D3" Command="{Binding SelectTeam}" CommandParameter="Evil Corp." />',
        '  <Separator />',
        f'  <MenuItem Header="Add team"><MenuItem.Icon>{icon("Plus", brush="CosmoPopoverForegroundBrush")}</MenuItem.Icon></MenuItem>',
        '</MenuFlyout>')


def team_switcher():
    """sidebar-07's team switcher: a large button with the team's logo, name and plan whose menu, at its right, picks the team."""
    return lines(
        '<c:SidebarMenu>',
        '  <c:SidebarMenuItem>',
        '    <c:SidebarMenuButton Classes="lg">',
        '      <DockPanel>',
        f'        {icon("ChevronsUpDown", dock("Right") + " Margin=" + chr(34) + "8,0,0,0" + chr(34))}',
        f'        {logo_tile(binding=True)}',
        block_indent(two_lines("", "", True, True), 8),
        '      </DockPanel>',
        '      <c:SidebarMenuButton.Flyout>',
        block_indent(team_menu(), 8),
        '      </c:SidebarMenuButton.Flyout>',
        '    </c:SidebarMenuButton>',
        '  </c:SidebarMenuItem>',
        '</c:SidebarMenu>')


def team_compact():
    """sidebar-10's team switcher: a button as wide as its content, a 20 px logo, the name and a faded chevron, its menu below."""
    return lines(
        '<c:SidebarMenu>',
        '  <c:SidebarMenuItem>',
        '    <c:SidebarMenuButton Padding="6,6" HorizontalAlignment="Left">',
        '      <StackPanel Orientation="Horizontal" Spacing="8">',
        f'        {logo_tile(binding=True, size=20, glyph=12, radius="CosmoRadiusMd")}',
        '        <TextBlock Text="{Binding ActiveTeam.Name}" FontWeight="Medium" TextTrimming="CharacterEllipsis" VerticalAlignment="Center" />',
        f'        {icon("ChevronDown", " Opacity=" + chr(34) + "0.5" + chr(34) + " VerticalAlignment=" + chr(34) + "Center" + chr(34))}',
        '      </StackPanel>',
        '      <c:SidebarMenuButton.Flyout>',
        block_indent(team_menu("BottomEdgeAlignedLeft"), 8),
        '      </c:SidebarMenuButton.Flyout>',
        '    </c:SidebarMenuButton>',
        '  </c:SidebarMenuItem>',
        '</c:SidebarMenu>')


def brand(kind="Command", name="Acme Inc", plan="Enterprise"):
    """A large link with the company's logo, name and plan (sidebar-08, sidebar-09 and sidebar-16's header)."""
    return lines(
        '<c:SidebarMenu>',
        '  <c:SidebarMenuItem>',
        f'    <c:SidebarMenuButton Classes="lg" Tooltip="{name}">',
        '      <DockPanel>',
        f'        {logo_tile(kind)}',
        block_indent(two_lines(name, plan), 8),
        '      </DockPanel>',
        '    </c:SidebarMenuButton>',
        '  </c:SidebarMenuItem>',
        '</c:SidebarMenu>')


def user_menu(placement="RightEdgeAlignedBottom", dashboard=False):
    entries = ([("Account", "IconUserCircle"), ("Billing", "IconCreditCard"), ("Notifications", "IconNotification")] if dashboard
               else [("Account", "BadgeCheck"), ("Billing", "CreditCard"), ("Notifications", "Bell")])
    item = lambda header, kind: f'  <MenuItem Header="{header}"><MenuItem.Icon>{icon(kind, brush="CosmoPopoverForegroundBrush")}</MenuItem.Icon></MenuItem>'
    return lines(
        f'<MenuFlyout Placement="{placement}">',
        '  <MenuItem Classes="label">',
        '    <MenuItem.Header>',
        '      <StackPanel Orientation="Horizontal" Spacing="8">',
        '        <c:Avatar Width="32" Height="32" Content="CN" CornerRadius="{DynamicResource CosmoRadiusLg}" />',
        '        <StackPanel VerticalAlignment="Center">',
        '          <TextBlock Text="shadcn" FontWeight="Medium" FontSize="14" LineHeight="17.5" />',
        '          <TextBlock Text="m@example.com" FontWeight="Normal" FontSize="12" LineHeight="15" />',
        '        </StackPanel>',
        '      </StackPanel>',
        '    </MenuItem.Header>',
        '  </MenuItem>',
        '  <Separator />',
        None if dashboard else item("Upgrade to Pro", "Sparkles"),
        None if dashboard else '  <Separator />',
        *[item(header, kind) for header, kind in entries],
        '  <Separator />',
        item("Log out", "IconLogout" if dashboard else "LogOut"),
        '</MenuFlyout>')


def nav_user(placement="RightEdgeAlignedBottom", dashboard=False):
    """The user's large button (avatar, name, e-mail) whose menu opens at its right: account, billing, notifications, log out."""
    end = icon("IconDotsVertical", dock("Right") + ' Margin="8,0,0,0"') if dashboard else icon("ChevronsUpDown", dock("Right") + ' Margin="8,0,0,0"')
    return lines(
        '<c:SidebarMenu>',
        '  <c:SidebarMenuItem>',
        '    <c:SidebarMenuButton Classes="lg" Tooltip="shadcn">',
        '      <DockPanel>',
        f'        {end}',
        '        <c:Avatar Width="32" Height="32" Content="CN" CornerRadius="{DynamicResource CosmoRadiusLg}" />',
        block_indent(two_lines("shadcn", "m@example.com"), 8),
        '      </DockPanel>',
        '      <c:SidebarMenuButton.Flyout>',
        block_indent(user_menu(placement, dashboard), 8),
        '      </c:SidebarMenuButton.Flyout>',
        '    </c:SidebarMenuButton>',
        '  </c:SidebarMenuItem>',
        '</c:SidebarMenu>')


def icon_label(kind, title, emoji=None):
    media = f'<TextBlock DockPanel.Dock="Left" Margin="0,0,8,0" VerticalAlignment="Center" Text="{emoji}" />' if emoji else icon(kind, dock("Left") + ' Margin="0,0,8,0"')
    return f'<DockPanel>{media}<TextBlock Text="{esc(title)}" VerticalAlignment="Center" TextTrimming="CharacterEllipsis" /></DockPanel>'


def button(kind, title, active=False, tooltip=True, classes=None, emoji=None, extra=""):
    attributes = (' IsActive="True"' if active else "") + (f' Tooltip="{esc(title)}"' if tooltip else "") + (f' Classes="{classes}"' if classes else "") + extra
    return f'<c:SidebarMenuButton{attributes}>{icon_label(kind, title, emoji)}</c:SidebarMenuButton>'


def group(content, label=None, classes=None, extra=""):
    attributes = (f' Classes="{classes}"' if classes else "") + extra
    return lines(f'<c:SidebarGroup{attributes}>', f'  <c:SidebarGroupLabel Content="{label}" />' if label else None, block_indent(content, 2), '</c:SidebarGroup>')


def menu(items, extra=""):
    return lines(f'<c:SidebarMenu{extra}>', block_indent("\n".join(items), 2), '</c:SidebarMenu>')


def platform_collapsible(items):
    """sidebar-07's Platform: each item's button opens its sub-menu (the chevron at its end turning), the first open."""
    out = []
    for item in items:
        expanded = ' IsExpanded="True"' if item.get("isActive") else ""
        out.append(lines(
            f'<c:SidebarMenuItem IsCollapsible="True"{expanded}>',
            f'  {button(item["icon"], item["title"])}',
            '  <c:SidebarMenuSub>', *[f'    <c:SidebarMenuSubButton Content="{esc(sub["title"])}" />' for sub in item.get("items", [])], '  </c:SidebarMenuSub>',
            '</c:SidebarMenuItem>'))
    return group(menu(out), "Platform")


def platform_actions(items):
    """sidebar-08's Platform: each item a link whose chevron action, at its end, opens its sub-menu, the first open."""
    out = []
    for item in items:
        expanded = ' IsExpanded="True"' if item.get("isActive") else ""
        out.append(lines(
            f'<c:SidebarMenuItem IsCollapsible="True"{expanded}>',
            f'  {button(item["icon"], item["title"])}',
            f'  <c:SidebarMenuAction Toggles="True" AutomationProperties.Name="Toggle">{icon("ChevronRight")}</c:SidebarMenuAction>',
            '  <c:SidebarMenuSub>', *[f'    <c:SidebarMenuSubButton Content="{esc(sub["title"])}" />' for sub in item.get("items", [])], '  </c:SidebarMenuSub>',
            '</c:SidebarMenuItem>'))
    return group(menu(out), "Platform")


def more_menu(entries, width=None):
    rows = []
    for entry in entries:
        if entry is None:
            rows.append('    <Separator />')
            continue
        header, kind, *rest = entry
        classes = ' Classes="destructive"' if rest and rest[0] == "destructive" else ""
        brush = "CosmoDestructiveBrush" if classes else "CosmoMutedForegroundBrush"
        rows.append(f'    <MenuItem Header="{esc(header)}"{classes}><MenuItem.Icon>{icon(kind, brush=brush)}</MenuItem.Icon></MenuItem>')
    return lines(
        '  <c:SidebarMenuAction Classes="show-on-hover" AutomationProperties.Name="More">',
        f'    {icon("IconDots" if width == 96 else "MoreHorizontal")}',
        '    <c:SidebarMenuAction.Flyout>',
        '      <MenuFlyout Placement="RightEdgeAlignedTop">',
        block_indent("\n".join(rows), 4),
        '      </MenuFlyout>',
        '    </c:SidebarMenuAction.Flyout>',
        '  </c:SidebarMenuAction>')


def more_button(kind="MoreHorizontal", dim=True):
    faded = ' Opacity="0.7"' if dim else ""
    return f'<c:SidebarMenuItem><c:SidebarMenuButton{faded}>{icon_label(kind, "More")}</c:SidebarMenuButton></c:SidebarMenuItem>'


def projects(items, share="Forward", dim=True):
    """sidebar-07's Projects: links with a More action shown under the pointer, its menu at their right, then a faded More."""
    entries = [("View Project", "Folder"), ("Share Project", share), None, ("Delete Project", "Trash2")]
    out = [lines('<c:SidebarMenuItem>', f'  {button(item["icon"], item["name"], tooltip=False)}', more_menu(entries), '</c:SidebarMenuItem>') for item in items]
    return group(menu(out + [more_button(dim=dim)]), "Projects", "icon-hidden")


def secondary(items, size="sm", extra=' DockPanel.Dock="Bottom"'):
    """The secondary links pinned to the sidebar's foot (shadcn's mt-auto), small with sidebar-08's and sidebar-16's."""
    out = []
    for item in items:
        badge = f'\n  <c:SidebarMenuBadge Content="{item["badge"]}" />' if item.get("badge") else ""
        out.append(f'<c:SidebarMenuItem>\n  {button(item["icon"], item["title"], tooltip=False, classes=size)}{badge}\n</c:SidebarMenuItem>')
    return group(menu(out), extra=extra)


def pinned(*groups):
    """A sidebar's content with its last group pinned to the foot: a dock whose groups stack from the top, 8 px apart."""
    return lines(
        '<c:SidebarContent.ItemsPanel><ItemsPanelTemplate><DockPanel LastChildFill="False" VerticalSpacing="8" /></ItemsPanelTemplate></c:SidebarContent.ItemsPanel>',
        *[group_text.replace("<c:SidebarGroup", '<c:SidebarGroup DockPanel.Dock="Top"', 1) if 'DockPanel.Dock=' not in group_text.split("\n", 1)[0] else group_text for group_text in groups])


def apps():
    seven, eight, sixteen = data("sidebar-07"), data("sidebar-08"), data("sidebar-16")
    fixtures = {
        "sidebar-07": shell(sidebar(team_switcher(), lines(platform_collapsible(seven["navMain"]), projects(seven["projects"])), footer=nav_user(), attributes=' Collapsible="Icon"'),
                            lines(header(border=False), tiles("16,0,16,16")), model="ShellExample"),
        "sidebar-08": shell(sidebar(brand(), pinned(platform_actions(eight["navMain"]), projects(eight["projects"], "Share", dim=False), secondary(eight["navSecondary"])),
                                    footer=nav_user(), rail=False, attributes=' Variant="Inset"'),
                            lines(header(border=False), tiles("16,0,16,16"))),
    }
    return fixtures



def mails():
    """sidebar-09's second sidebar: the folder's title with the unread switch over a search field, then its mails."""
    return lines(
        '<DockPanel>',
        '  <StackPanel DockPanel.Dock="Top" Spacing="14" Margin="16">',
        '    <DockPanel>',
        '      <StackPanel DockPanel.Dock="Right" Orientation="Horizontal" Spacing="8">',
        '        <Label Content="Unreads" Target="{Binding #unreads}" VerticalAlignment="Center" FontWeight="Normal" />',
        '        <ToggleSwitch Name="unreads" IsChecked="{Binding Unreads}" />',
        '      </StackPanel>',
        '      <TextBlock Text="{Binding Folder}" FontSize="16" LineHeight="24" FontWeight="Medium" Foreground="{DynamicResource CosmoForegroundBrush}" VerticalAlignment="Center" />',
        '    </DockPanel>',
        '    <c:SidebarInput PlaceholderText="Type to search..." AutomationProperties.Name="Search" />',
        '  </StackPanel>',
        '  <Border DockPanel.Dock="Top" Height="1" Background="{DynamicResource CosmoSidebarBorderBrush}" />',
        '  <ScrollViewer Theme="{DynamicResource CosmoScrollArea}" HorizontalScrollBarVisibility="Disabled">',
        '    <ItemsControl ItemsSource="{Binding Mails}">',
        '      <ItemsControl.ItemTemplate>',
        '        <DataTemplate x:DataType="m:ShellMail">',
        '          <Button Classes="mail" Padding="16" HorizontalAlignment="Stretch" HorizontalContentAlignment="Stretch">',
        '            <Grid RowDefinitions="Auto,Auto,Auto" RowSpacing="8">',
        '              <DockPanel>',
        '                <TextBlock DockPanel.Dock="Right" Text="{Binding Date}" FontSize="12" LineHeight="15" VerticalAlignment="Center" />',
        '                <TextBlock Text="{Binding Name}" LineHeight="17.5" />',
        '              </DockPanel>',
        '              <TextBlock Grid.Row="1" Text="{Binding Subject}" FontWeight="Medium" LineHeight="17.5" />',
        '              <TextBlock Grid.Row="2" Text="{Binding Teaser}" Width="260" HorizontalAlignment="Left" FontSize="12" LineHeight="15" TextWrapping="Wrap" MaxLines="2" TextTrimming="CharacterEllipsis" />',
        '            </Grid>',
        '          </Button>',
        '        </DataTemplate>',
        '      </ItemsControl.ItemTemplate>',
        '    </ItemsControl>',
        '  </ScrollViewer>',
        '</DockPanel>')


def rail(nav):
    """sidebar-09's first sidebar: a 49 px rail of the logo, the folders (the chosen one active, each opening the sidebar) and the user."""
    items = [lines(
        '<c:SidebarMenuItem>',
        f'  <c:SidebarMenuButton Classes="rail" Tooltip="{item["title"]}" ToolTip.Tip="{item["title"]}" ToolTip.Placement="Right" Command="{{Binding SelectFolder}}" CommandParameter="{item["title"]}"',
        f'                       IsActive="{{Binding In{item["title"]}}}">{icon_label(item["icon"], item["title"])}</c:SidebarMenuButton>',
        '</c:SidebarMenuItem>') for item in nav]
    return lines(
        '<Border DockPanel.Dock="Left" Width="49" BorderThickness="0,0,1,0" BorderBrush="{DynamicResource CosmoSidebarBorderBrush}">',
        '  <DockPanel>',
        '    <c:SidebarHeader DockPanel.Dock="Top" Margin="8">',
        block_indent(brand().replace('Classes="lg"', 'Classes="lg rail"'), 6),
        '    </c:SidebarHeader>',
        '    <c:SidebarFooter DockPanel.Dock="Bottom" Margin="8">',
        block_indent(nav_user().replace('Classes="lg"', 'Classes="lg rail"'), 6),
        '    </c:SidebarFooter>',
        '    <c:SidebarContent>',
        block_indent(group(menu(items)), 6),
        '    </c:SidebarContent>',
        '  </DockPanel>',
        '</Border>')


def favorites(items):
    """sidebar-10's Favorites: emoji links with a More action under the pointer, then a faded More."""
    entries = [("Remove from Favorites", "StarOff"), None, ("Copy Link", "Link"), ("Open in New Tab", "ArrowUpRight"), None, ("Delete", "Trash2")]
    out = [lines('<c:SidebarMenuItem>', f'  {button(None, item["name"], tooltip=False, emoji=item["emoji"], extra=" ToolTip.Tip=" + chr(34) + esc(item["name"]) + chr(34))}', more_menu(entries), '</c:SidebarMenuItem>') for item in items]
    return group(menu(out + [more_button()]), "Favorites", "icon-hidden")


def workspaces(items):
    """sidebar-10's Workspaces: emoji links whose chevron, over the emoji under the pointer, opens their pages, with a plus action at their end."""
    out = []
    for item in items:
        out.append(lines(
            '<c:SidebarMenuItem IsCollapsible="True">',
            f'  {button(None, item["name"], tooltip=False, emoji=item["emoji"])}',
            f'  <c:SidebarMenuAction Toggles="True" Classes="show-on-hover start" AutomationProperties.Name="Toggle">{icon("ChevronRight", brush="CosmoSidebarAccentForegroundBrush")}</c:SidebarMenuAction>',
            f'  <c:SidebarMenuAction Classes="show-on-hover" AutomationProperties.Name="Add">{icon("Plus")}</c:SidebarMenuAction>',
            '  <c:SidebarMenuSub>',
            *[f'    <c:SidebarMenuSubButton><StackPanel Orientation="Horizontal" Spacing="8"><TextBlock Text="{page["emoji"]}" /><TextBlock Text="{esc(page["name"])}" TextTrimming="CharacterEllipsis" /></StackPanel></c:SidebarMenuSubButton>' for page in item["pages"]],
            '  </c:SidebarMenuSub>',
            '</c:SidebarMenuItem>'))
    return group(menu(out + [more_button()]), "Workspaces", extra=' DockPanel.Dock="Top"')


def notes_main(items):
    return menu([f'<c:SidebarMenuItem>{button(item["icon"], item["title"], active=item.get("isActive", False), tooltip=False)}</c:SidebarMenuItem>' for item in items])


def nav_actions(groups):
    """sidebar-10's page actions: the edit date, a star and a More button whose popover, open from the start, lists the actions in groups."""
    sections = []
    for index, entries in enumerate(groups):
        border = ' BorderThickness="0,0,0,1" BorderBrush="{DynamicResource CosmoBorderBrush}"' if index < len(groups) - 1 else ""
        items = [f'<c:SidebarMenuItem><c:SidebarMenuButton>{icon_label(entry["icon"], entry["label"])}</c:SidebarMenuButton></c:SidebarMenuItem>' for entry in entries]
        sections.append(f'<Border Padding="8"{border}>\n{block_indent(menu(items), 2)}\n</Border>')
    return lines(
        '<StackPanel DockPanel.Dock="Right" Orientation="Horizontal" Spacing="8" Margin="12,0" VerticalAlignment="Center">',
        '  <TextBlock Text="Edit Oct 08" FontWeight="Medium" Foreground="{DynamicResource CosmoMutedForegroundBrush}" VerticalAlignment="Center" />',
        f'  <Button Classes="ghost icon-sm" Width="28" Height="28" AutomationProperties.Name="Star">{icon("Star", brush="CosmoForegroundBrush")}</Button>',
        '  <Button Classes="ghost icon-sm" Width="28" Height="28" AutomationProperties.Name="More" c:Popover.IsOpen="{Binding ActionsOpen}">',
        f'    {icon("MoreHorizontal", brush="CosmoForegroundBrush")}',
        '    <Button.Flyout>',
        '      <Flyout Placement="BottomEdgeAlignedRight" FlyoutPresenterClasses="actions" OverlayDismissEventPassThrough="True">',
        '        <StackPanel Classes="actions">',
        block_indent("\n".join(sections), 10),
        '        </StackPanel>',
        '      </Flyout>',
        '    </Button.Flyout>',
        '  </Button>',
        '</StackPanel>')


def page_header(content, height=56, padding="12,0", right=None, border=False, trigger_margin="0", sticky=False):
    rule = ' BorderThickness="0,0,0,1" BorderBrush="{DynamicResource CosmoBorderBrush}"' if border else ""
    background = ' Background="{DynamicResource CosmoBackgroundBrush}"' if sticky else ""
    return lines(
        f'<DockPanel DockPanel.Dock="Top" Height="{height}"{background}>',
        f'  <Border DockPanel.Dock="Bottom" Height="1" Background="{{DynamicResource CosmoBorderBrush}}" />' if border else None,
        block_indent(right, 2) if right else None,
        f'  <StackPanel Orientation="Horizontal" Spacing="8" Margin="{padding}" VerticalAlignment="Center">',
        f'    <c:SidebarTrigger Margin="{trigger_margin}" VerticalAlignment="Center" />',
        '    <Separator Classes="vertical" Height="16" Margin="0,0,8,0" VerticalAlignment="Center" />',
        block_indent(content, 4),
        '  </StackPanel>',
        '</DockPanel>')


def page_title(text):
    return f'<c:Breadcrumb VerticalAlignment="Center"><c:BreadcrumbPage Text="{esc(text)}" TextTrimming="CharacterEllipsis" MaxLines="1" /></c:Breadcrumb>'


def notes_body(height="*"):
    tall = '<Border Height="1000" MaxWidth="768" CornerRadius="{DynamicResource CosmoRadiusXl}" Background="{DynamicResource CosmoMutedHalfBrush}" />' if height == "tall" else '<Border MaxWidth="768" CornerRadius="{DynamicResource CosmoRadiusXl}" Background="{DynamicResource CosmoMutedHalfBrush}" />'
    body = lines(
        f'<DockPanel Margin="{"16" if height == "tall" else "16,40"}">',
        '  <Border DockPanel.Dock="Top" Height="96" MaxWidth="768" Margin="0,0,0,16" CornerRadius="{DynamicResource CosmoRadiusXl}" Background="{DynamicResource CosmoMutedHalfBrush}" />',
        f'  {tall}',
        '</DockPanel>')
    return lines('<ScrollViewer Theme="{DynamicResource CosmoScrollArea}" HorizontalScrollBarVisibility="Disabled">', block_indent(body, 2), '</ScrollViewer>') if height == "tall" else body


def file_tree(item, active="button.tsx", opened=("components", "ui")):
    """sidebar-11's file tree: a file is a button, a folder a collapsible item whose chevron, before its icon, turns as it opens."""
    if isinstance(item, str):
        flag = ' IsActive="True" Classes="file"' if item == active else ' Classes="file"'
        return f'<c:SidebarMenuButton{flag}>{icon_label("File", item)}</c:SidebarMenuButton>'
    name, *children = item
    expanded = ' IsExpanded="True"' if name in opened else ""
    return lines(
        f'<c:SidebarMenuItem IsCollapsible="True" Classes="tree"{expanded}>',
        f'  <c:SidebarMenuButton><StackPanel>{icon("ChevronRight", " Classes=" + chr(34) + "chevron" + chr(34))}{icon("Folder")}<TextBlock Text="{esc(name)}" /></StackPanel></c:SidebarMenuButton>',
        '  <c:SidebarMenuSub>',
        block_indent("\n".join(file_tree(child, active, opened) for child in children), 4),
        '  </c:SidebarMenuSub>',
        '</c:SidebarMenuItem>')


def changes(items):
    return [f'<c:SidebarMenuItem>\n  <c:SidebarMenuButton>{icon_label("File", item["file"])}</c:SidebarMenuButton>\n  <c:SidebarMenuBadge Content="{item["state"]}" />\n</c:SidebarMenuItem>' for item in items]


FROZEN = ' DisplayDate="2026-10-16" SelectedDate="2026-10-16" IsTodayHighlighted="False"'


def date_picker():
    """sidebar-12's calendar: the month in 33 px columns, today on --sidebar-primary (frozen on 2026-10-16 for the fixture, as the reference's clock)."""
    return f'<c:SidebarGroup Margin="0,8"><Calendar Classes="sidebar" Background="Transparent" SelectionMode="SingleDate"{FROZEN} /></c:SidebarGroup>'


def calendars(items, radius=4):
    """sidebar-12's calendars: a collapsible group per list, the first open, its label the trigger, each calendar a check tile (the first two checked) and its name."""
    out = []
    for index, calendar in enumerate(items):
        rows = []
        for position, name in enumerate(calendar["items"]):
            checked = f' Classes="calendar-check on" CornerRadius="{radius}"' if position < 2 else f' Classes="calendar-check" CornerRadius="{radius}"'
            rows.append(f'    <c:SidebarMenuItem><c:SidebarMenuButton><StackPanel><Border{checked}>{icon("Check", size=12, brush="CosmoSidebarPrimaryForegroundBrush")}</Border><TextBlock Text="{name}" /></StackPanel></c:SidebarMenuButton></c:SidebarMenuItem>')
        out.append(lines(
            f'<Expander Classes="sidebar-group calendars"{" IsExpanded=" + chr(34) + "True" + chr(34) if index == 0 else ""}>',
            f'  <Expander.Header><TextBlock Text="{calendar["name"]}" /></Expander.Header>',
            '  <c:SidebarMenu>', *rows, '  </c:SidebarMenu>',
            '</Expander>',
            '<c:SidebarSeparator Margin="0" />'))
    return "\n".join(out)


def calendar_sidebar(attributes="", radius=4):
    new = f'<c:SidebarMenu><c:SidebarMenuItem>{button("Plus", "New Calendar", tooltip=False)}</c:SidebarMenuItem></c:SidebarMenu>'
    return lines(
        f'<c:Sidebar{attributes}>',
        '  <Border Height="64" BorderThickness="0,0,0,1" BorderBrush="{DynamicResource CosmoSidebarBorderBrush}">',
        '    <c:SidebarHeader Margin="8">',
        block_indent(nav_user("RightEdgeAlignedTop"), 6),
        '    </c:SidebarHeader>',
        '  </Border>',
        '  <c:SidebarContent>',
        f'    {date_picker()}',
        '    <c:SidebarSeparator Margin="0" />',
        block_indent(calendars(data("sidebar-12")["calendars"], radius), 4),
        '  </c:SidebarContent>',
        '  <c:SidebarFooter>',
        f'    {new}',
        '  </c:SidebarFooter>',
        '  <c:SidebarRail />' if "None" not in attributes else None,
        '</c:Sidebar>')


def calendar_body():
    tile = '<c:AspectRatio Ratio="1"><Border CornerRadius="{DynamicResource CosmoRadiusXl}" Background="{DynamicResource CosmoMutedHalfBrush}" /></c:AspectRatio>'
    return lines('<ScrollViewer Theme="{DynamicResource CosmoScrollArea}" HorizontalScrollBarVisibility="Disabled">', '  <UniformGrid Columns="5" ColumnSpacing="16" RowSpacing="16" Margin="16">', *[f'    {tile}'] * 20, '  </UniformGrid>', '</ScrollViewer>')


def settings_dialog(nav):
    """sidebar-13: a settings dialog, open at first, its sidebar of sections beside the section's breadcrumb and content."""
    items = [f'<c:SidebarMenuItem>{button(item["icon"], item["name"], active=item["name"] == "Messages & media", tooltip=False)}</c:SidebarMenuItem>' for item in nav]
    tiles = "\n".join(['        <c:AspectRatio Ratio="1.7778" MaxWidth="768" HorizontalAlignment="Left"><Border CornerRadius="{DynamicResource CosmoRadiusXl}" Background="{DynamicResource CosmoMutedHalfBrush}" /></c:AspectRatio>'] * 10)
    return lines(
        f'<Panel Width="{WIDTH}" Height="{HEIGHT}" Background="{{DynamicResource CosmoBackgroundBrush}}" c:OverlayHost.IsHost="True" x:DataType="m:ShellExample">',
        '  <Panel.DataContext><m:ShellExample /></Panel.DataContext>',
        '  <Button Classes="sm" Content="Open Dialog" Command="{Binding OpenSettings}" HorizontalAlignment="Center" VerticalAlignment="Center" />',
        '  <c:Dialog Classes="flush" MaxWidth="800" IsOpen="{Binding SettingsOpen}" AutomationProperties.Name="Settings" AutomationProperties.HelpText="Customize your settings here.">',
        '    <c:SidebarProvider MobileBreakpoint="0" Height="498">',
        '      <c:Sidebar Collapsible="None">',
        '        <c:SidebarContent>',
        block_indent(group(menu(items)), 10),
        '        </c:SidebarContent>',
        '      </c:Sidebar>',
        '      <DockPanel Height="480" VerticalAlignment="Top">',
        '        <StackPanel DockPanel.Dock="Top" Height="64" Orientation="Horizontal" Margin="16,0">',
        block_indent(crumbs("Settings", "Messages & media"), 10),
        '        </StackPanel>',
        '        <ScrollViewer Theme="{DynamicResource CosmoScrollArea}" HorizontalScrollBarVisibility="Disabled">',
        '          <StackPanel Spacing="16" Margin="16,0,16,16">',
        tiles,
        '          </StackPanel>',
        '        </ScrollViewer>',
        '      </DockPanel>',
        '    </c:SidebarProvider>',
        '  </c:Dialog>',
        '</Panel>')


def table_of_contents(nav):
    items = []
    for entry in nav:
        sub = sub_buttons(entry.get("items", []))
        items.append(lines('<c:SidebarMenuItem>', f'  {menu_button(entry["title"], extra=" FontWeight=" + chr(34) + "Medium" + chr(34))}',
                           lines('  <c:SidebarMenuSub>', *[f"    {line}" for line in sub], '  </c:SidebarMenuSub>') if sub else None, '</c:SidebarMenuItem>'))
    return group(menu(items), "Table of Contents")


def site_header():
    """sidebar-16's site header across the top: a 32 px ghost toggle, the rule, the breadcrumb and the search field at its end."""
    return lines(
        '<Border DockPanel.Dock="Top" Height="57" BorderThickness="0,0,0,1" BorderBrush="{DynamicResource CosmoBorderBrush}" Background="{DynamicResource CosmoBackgroundBrush}">',
        '  <DockPanel Margin="16,0">',
        '    <Panel DockPanel.Dock="Right" Width="192" VerticalAlignment="Center">',
        '      <c:SidebarInput Height="32" Padding="28,4,12,4" PlaceholderText="Type to search..." AutomationProperties.Name="Search" />',
        '      ' + icon("Search", ' HorizontalAlignment="Left" VerticalAlignment="Center" Margin="8,0,0,0" Opacity="0.5" IsHitTestVisible="False"', brush="CosmoForegroundBrush"),
        '    </Panel>',
        '    <StackPanel Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">',
        '      <c:SidebarTrigger Width="32" Height="32" VerticalAlignment="Center" />',
        '      <Separator Classes="vertical" Height="16" Margin="0,0,8,0" VerticalAlignment="Center" />',
        block_indent(crumbs(), 6),
        '    </StackPanel>',
        '  </DockPanel>',
        '</Border>')


MAIL_STYLES = lines(
    '<Style Selector="Button.mail">',
    '  <Setter Property="Background" Value="Transparent" />',
    '  <Setter Property="Foreground" Value="{DynamicResource CosmoSidebarForegroundBrush}" />',
    '  <Setter Property="CornerRadius" Value="0" />',
    '  <Setter Property="BorderThickness" Value="0,0,0,1" />',
    '  <Setter Property="BorderBrush" Value="{DynamicResource CosmoSidebarBorderBrush}" />',
    '  <Setter Property="FontWeight" Value="Normal" />',
    '  <Setter Property="Height" Value="NaN" />',
    '</Style>',
    '<Style Selector="Button.mail:pointerover">',
    '  <Setter Property="Background" Value="{DynamicResource CosmoSidebarAccentBrush}" />',
    '  <Setter Property="Foreground" Value="{DynamicResource CosmoSidebarAccentForegroundBrush}" />',
    '</Style>',
    '<Style Selector="c|SidebarMenuButton.rail.lg">',
    '  <Setter Property="Height" Value="32" />',
    '  <Setter Property="Padding" Value="0" />',
    '</Style>')

NOTES_STYLES = lines(
    '<Style Selector="c|SidebarMenuAction.start">',
    '  <Setter Property="HorizontalAlignment" Value="Left" />',
    '  <Setter Property="Margin" Value="8,6,0,0" />',
    '  <Setter Property="Background" Value="{DynamicResource CosmoSidebarAccentBrush}" />',
    '  <Setter Property="Foreground" Value="{DynamicResource CosmoSidebarAccentForegroundBrush}" />',
    '</Style>',
    '<Style Selector="FlyoutPresenter.actions">',
    '  <Setter Property="Width" Value="224" />',
    '  <Setter Property="Padding" Value="0" />',
    '</Style>')

TREE_STYLES = lines(
    '<Style Selector="c|SidebarMenuItem.tree > c|SidebarMenuButton /template/ Viewbox#PART_Chevron">',
    '  <Setter Property="IsVisible" Value="False" />',
    '</Style>',
    '<Style Selector="c|SidebarMenuItem.tree > c|SidebarMenuButton Viewbox.chevron">',
    '  <Setter Property="RenderTransform" Value="none" />',
    '</Style>',
    '<Style Selector="c|SidebarMenuItem.tree:expanded > c|SidebarMenuButton Viewbox.chevron">',
    '  <Setter Property="RenderTransform" Value="rotate(90deg)" />',
    '</Style>',
    '<Style Selector="c|SidebarMenuButton.file:active">',
    '  <Setter Property="Background" Value="Transparent" />',
    '</Style>')

CALENDAR_STYLES = lines(
    '<Style Selector="Expander.calendars">',
    '  <Setter Property="Margin" Value="8,0" />',
    '</Style>',
    '<Style Selector="Border.calendar-check">',
    '  <Setter Property="Width" Value="16" />',
    '  <Setter Property="Height" Value="16" />',
    '  <Setter Property="BorderThickness" Value="1" />',
    '  <Setter Property="BorderBrush" Value="{DynamicResource CosmoSidebarBorderBrush}" />',
    '</Style>',
    '<Style Selector="Border.calendar-check > Viewbox">',
    '  <Setter Property="IsVisible" Value="False" />',
    '</Style>',
    '<Style Selector="Border.calendar-check.on">',
    '  <Setter Property="Background" Value="{DynamicResource CosmoSidebarPrimaryBrush}" />',
    '  <Setter Property="BorderBrush" Value="{DynamicResource CosmoSidebarPrimaryBrush}" />',
    '</Style>',
    '<Style Selector="Border.calendar-check.on > Viewbox">',
    '  <Setter Property="IsVisible" Value="True" />',
    '</Style>')


def more_apps():
    nine, ten, eleven, sixteen = data("sidebar-09"), data("sidebar-10"), data("sidebar-11"), data("sidebar-16")
    actions = data("sidebar-10", "components/nav-actions.tsx")
    notes_sidebar = lambda: sidebar(lines(team_compact(), notes_main(ten["navMain"])),
                                    pinned(favorites(ten["favorites"]), workspaces(ten["workspaces"]), secondary(ten["navSecondary"], size=None)),
                                    attributes=' Classes="borderless"')
    nine_header = lines(
        '<Border DockPanel.Dock="Top" Padding="16" BorderThickness="0,0,0,1" BorderBrush="{DynamicResource CosmoBorderBrush}" Background="{DynamicResource CosmoBackgroundBrush}">',
        '  <StackPanel Orientation="Horizontal" Spacing="8">',
        '    <c:SidebarTrigger Margin="-4,0,0,0" VerticalAlignment="Center" />',
        '    <Separator Classes="vertical" Height="16" Margin="0,0,8,0" VerticalAlignment="Center" />',
        block_indent(crumbs("All Inboxes", "Inbox"), 4),
        '  </StackPanel>',
        '</Border>')
    rows = lines('<ScrollViewer Theme="{DynamicResource CosmoScrollArea}" HorizontalScrollBarVisibility="Disabled">', '  <StackPanel Margin="16" Spacing="16">',
                 *['    <Border Height="48" CornerRadius="{DynamicResource CosmoRadiusLg}" Background="{DynamicResource CosmoMutedHalfBrush}" />'] * 24, '  </StackPanel>', '</ScrollViewer>')
    return {
        "sidebar-09": shell(lines('<c:Sidebar Collapsible="Icon" ExpandedWidth="350" ClipToBounds="True">', '  <DockPanel Grid.RowSpan="3">', block_indent(rail(nine["navMain"]), 4), block_indent(mails(), 4), '  </DockPanel>', '</c:Sidebar>'),
                            lines(nine_header, rows), provider=' IsOpen="{Binding SidebarOpen}"', model="ShellExample", styles=MAIL_STYLES),
        "sidebar-10": shell(notes_sidebar(), lines(page_header(page_title("Project Management & Task Tracking"), right=nav_actions(actions)), notes_body()), model="ShellExample", styles=NOTES_STYLES),
        "sidebar-11": shell(lines('<c:Sidebar>', '  <c:SidebarContent>', block_indent(group(menu(changes(eleven["changes"])), "Changes"), 4),
                                  block_indent(group(menu([file_tree(item) for item in eleven["tree"]]), "Files"), 4), '  </c:SidebarContent>', '  <c:SidebarRail />', '</c:Sidebar>'),
                            lines(header(content=crumbs3()), tiles()), styles=TREE_STYLES),
        "sidebar-12": shell(calendar_sidebar(), lines(header(sticky=True, content=page_title("October 2024")), calendar_body()), styles=CALENDAR_STYLES),
        "sidebar-13": settings_dialog(data("sidebar-13", "components/settings-dialog.tsx")["nav"]),
        "sidebar-14": shell(lines('<c:Sidebar Side="Right">', '  <c:SidebarContent>', block_indent(table_of_contents(data("sidebar-14")["navMain"]), 4), '  </c:SidebarContent>', '  <c:SidebarRail />', '</c:Sidebar>'),
                            lines(right_header(), tiles())),
        "sidebar-15": shell(lines(notes_sidebar(), calendar_sidebar(' Side="Right" Collapsible="None" Classes="bordered"', radius=2)),
                            lines(page_header(page_title("Project Management & Task Tracking"), sticky=True), notes_body("tall")), model="ShellExample", styles=lines(NOTES_STYLES, CALENDAR_STYLES)),
        "sidebar-16": shell(lines(site_header(), sidebar(brand(), pinned(platform_actions(sixteen["navMain"]), projects(sixteen["projects"], "Share", dim=False), secondary(sixteen["navSecondary"])),
                                                         footer=nav_user(), rail=False)),
                            tiles()),
    }


def crumbs3():
    return lines(
        '<c:Breadcrumb VerticalAlignment="Center">',
        '  <HyperlinkButton Content="components" NavigateUri="#" />',
        '  <HyperlinkButton Content="ui" NavigateUri="#" />',
        '  <c:BreadcrumbPage Text="button.tsx" />',
        '</c:Breadcrumb>')


def right_header():
    return lines(
        '<DockPanel DockPanel.Dock="Top" Height="64">',
        '  <Border DockPanel.Dock="Bottom" Height="1" Background="{DynamicResource CosmoBorderBrush}" />',
        '  <c:SidebarTrigger DockPanel.Dock="Right" Margin="0,0,12,0" VerticalAlignment="Center" RenderTransform="rotate(180deg)" />',
        '  <StackPanel Orientation="Horizontal" Margin="16,0" VerticalAlignment="Center">',
        block_indent(crumbs(), 4),
        '  </StackPanel>',
        '</DockPanel>')



def dashboards():
    import dashboard
    return {"dashboard-01": dashboard.dashboard()}


FIXTURES = {**docs(), **apps(), **more_apps(), **dashboards()}

TITLES = {
    "dashboard-01": "A dashboard with sidebar, charts and data table.",
    "sidebar-01": "A simple sidebar with navigation grouped by section.",
    "sidebar-02": "A sidebar with collapsible sections.",
    "sidebar-03": "A sidebar with submenus.",
    "sidebar-04": "A floating sidebar with submenus.",
    "sidebar-05": "A sidebar with collapsible submenus.",
    "sidebar-06": "A sidebar with submenus as dropdowns.",
    "sidebar-07": "A sidebar that collapses to icons.",
    "sidebar-08": "An inset sidebar with secondary navigation.",
    "sidebar-09": "Collapsible nested sidebars.",
    "sidebar-10": "A sidebar in a popover.",
    "sidebar-11": "A sidebar with a collapsible file tree.",
    "sidebar-12": "A sidebar with a calendar.",
    "sidebar-13": "A sidebar in a dialog.",
    "sidebar-14": "A sidebar on the right.",
    "sidebar-15": "A left and right sidebar.",
    "sidebar-16": "A sidebar with a sticky site header.",
}

PAGE = """<UserControl xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:a="https://aioros.dev" xmlns:c="https://aioros.dev/cosmo" xmlns:m="using:Gallery.Models"{pictor}{datatype}
             a:Page.Title="{title}" a:Page.Description="{lead}">
  <StackPanel Spacing="24">
    <StackPanel Spacing="10">
      <TextBlock Classes="h1" Text="{title}" />
      <TextBlock Classes="lead" Text="{lead}" />
    </StackPanel>
    <Border Height="{height}" CornerRadius="{{DynamicResource CosmoRadiusXl}}" BorderThickness="1" BorderBrush="{{DynamicResource CosmoBorderBrush}}" ClipToBounds="True">
{body}
    </Border>
  </StackPanel>
</UserControl>
"""


def write_pages(pages):
    """Writes each shell block as a Gallery page, /blocks/{block}: the block in a frame as tall as the reference's viewport, as wide as the page."""
    for name, body in FIXTURES.items():
        folder = pages / "blocks" / name
        folder.mkdir(parents=True, exist_ok=True)
        model = re.search(r'<[A-Za-z:]+\.DataContext><m:([A-Za-z]+) /></[A-Za-z:]+\.DataContext>', body)
        datatype = f' x:DataType="m:{model.group(1)}"' if model else ""
        fluid = re.sub(FROZEN, "", body.replace(f' Width="{WIDTH}" Height="{HEIGHT}"', "", 1))
        fluid = re.sub(r'\n *<[A-Za-z:]+\.DataContext><m:[A-Za-z]+ /></[A-Za-z:]+\.DataContext>', "", fluid)
        pictor = ' xmlns:p="https://aioros.dev/pictor"' if "<p:" in body else ""
        (folder / "Page.axaml").write_text(PAGE.format(title=name, lead=TITLES[name], datatype=datatype, pictor=pictor, height=HEIGHT,
                                                       body=indent(dedent(fluid).strip("\n"), " " * 6)))
