"""shadcn/ui's login and signup blocks (new-york-v4 login-01 to login-05, signup-01 to signup-05) composed from Cosmo's
components: their bodies, which fixtures.py writes as the /fidelity pages and the desktop tests' templates and pages.py as
the /blocks pages. The brand icons' paths are read from the pinned registry (tools/shadcn/registry/blocks)."""
import re
from pathlib import Path
from textwrap import dedent, indent

ROOT = Path(__file__).resolve().parents[3]
REGISTRY = ROOT / "tools" / "shadcn" / "registry" / "blocks"

LOGO = "M7 2h10 M5 6h14 M5,10 L19,10 A2,2 0 0 1 21,12 L21,20 A2,2 0 0 1 19,22 L5,22 A2,2 0 0 1 3,20 L3,12 A2,2 0 0 1 5,10 Z"


def plain(path):
    return re.sub(r"(-?\d*\.?\d+)e(-?\d+)", lambda match: f"{float(match.group(0)):.6f}".rstrip("0").rstrip("."), path)


def paths(block, form):
    return [plain(path) for path in re.findall(r'\sd="([^"]+)"', (REGISTRY / block / "components" / form).read_text())]


BRANDS = {
    "github": paths("login-02", "login-form.tsx")[0],
    "apple": paths("login-03", "login-form.tsx")[0],
    "google": paths("login-03", "login-form.tsx")[1],
    "meta": paths("login-04", "login-form.tsx")[2],
}

FG = "{DynamicResource CosmoForegroundBrush}"
MUTED = "{DynamicResource CosmoMutedForegroundBrush}"


def brand_icon(kind):
    return f'<Viewbox Width="16" Height="16"><Path Width="24" Height="24" Stretch="None" Fill="{FG}" Data="{BRANDS[kind]}" /></Viewbox>'


def logo(size=24, glyph=16, filled=True):
    background = ' Background="{DynamicResource CosmoPrimaryBrush}"' if filled else ""
    stroke = "CosmoPrimaryForegroundBrush" if filled else "CosmoForegroundBrush"
    return (f'<Border Width="{size}" Height="{size}" CornerRadius="{{DynamicResource CosmoRadiusMd}}"{background}>'
            f'<Viewbox Width="{glyph}" Height="{glyph}"><Path Theme="{{DynamicResource CosmoIcon}}" Stroke="{{DynamicResource {stroke}}}" Data="{LOGO}" /></Viewbox></Border>')


def brand(align="Left"):
    return (f'<Border a:Web.Href="#" HorizontalAlignment="{align}"><StackPanel Orientation="Horizontal" Spacing="8">{logo()}'
            f'<TextBlock Text="Acme Inc." FontSize="16" LineHeight="24" FontWeight="Medium" VerticalAlignment="Center" /></StackPanel></Border>')


def link(text, size=14, extra=""):
    return (f'<HyperlinkButton Classes="link" NavigateUri="#" Padding="0" Height="NaN" MinHeight="0" FontSize="{size}" FontWeight="Normal" '
            f'Foreground="{FG}"{extra}><TextBlock Text="{text}" TextWrapping="Wrap" /></HyperlinkButton>')


def description(parts, align="Center", margin=""):
    inlines = "".join(f'<Run Text="{part}" />' if not part.startswith("@") else f'<Run Text="{part[1:]}" a:Web.Href="#" TextDecorations="Underline" />' for part in parts)
    return f'<c:FieldDescription TextAlignment="{align}"{margin}>{inlines}</c:FieldDescription>'


def account(question, action, margin=""):
    return description([question + " ", "@" + action], margin=margin)


TERMS = description(["By clicking continue, you agree to our ", "@Terms of Service", " and ", "@Privacy Policy", "."], margin=' Margin="24,0"')


def textbox(name, kind="text", placeholder=None):
    hint = f' PlaceholderText="{placeholder}"' if placeholder else ""
    secret = ' PasswordChar="•"' if kind == "password" else ""
    web = f' a:Web.InputType="{kind}"' if kind == "email" else ""
    return f'<TextBox Name="{name}" a:Web.Id="{name}"{web}{hint}{secret} />'


def field(label, name, kind="text", placeholder=None, note=None):
    help = f'\n  <c:FieldDescription Text="{note}" />' if note else ""
    return f"""<c:Field>
  <c:FieldLabel Content="{label}" Target="{{Binding #{name}}}" />
  {textbox(name, kind, placeholder)}{help}
</c:Field>"""


def email(note=None):
    return field("Email", "email", "email", "m@example.com", note)


def password():
    return f"""<c:Field>
  <Grid ColumnDefinitions="Auto,*">
    <c:FieldLabel Content="Password" Target="{{Binding #password}}" VerticalAlignment="Center" />
    {link("Forgot your password?", extra=' Grid.Column="1" HorizontalAlignment="Right" VerticalAlignment="Center"')}
  </Grid>
  {textbox("password", "password")}
</c:Field>"""


def passwords():
    return f"""<c:Field>
  <Grid ColumnDefinitions="*,*" ColumnSpacing="16">
    {field("Password", "password", "password")}
    <Border Grid.Column="1">{field("Confirm Password", "confirm", "password")}</Border>
  </Grid>
  <c:FieldDescription Text="Must be at least 8 characters long." />
</c:Field>"""


def button(text, outline=False, icon=None):
    classes = "outline" + (" has-icon" if icon else "") if outline else ""
    attribute = f' Classes="{classes}"' if classes else ""
    if icon:
        return f'<Button{attribute} HorizontalAlignment="Stretch" HorizontalContentAlignment="Center"><StackPanel>{brand_icon(icon)}<TextBlock Text="{text}" /></StackPanel></Button>'
    return f'<Button{attribute} Content="{text}" HorizontalAlignment="Stretch" HorizontalContentAlignment="Center" />'


def icon_buttons(names, columns, spacing=16):
    cells = "".join(f'<Button Grid.Column="{i}" Classes="outline" HorizontalAlignment="Stretch" HorizontalContentAlignment="Center" AutomationProperties.Name="{name}">{brand_icon(kind)}</Button>'
                    for i, (kind, name) in enumerate(names))
    return f'<Grid ColumnDefinitions="{",".join(["*"] * columns)}" ColumnSpacing="{spacing}">{cells}</Grid>'


def wide_buttons(names):
    cells = "".join(f'<Border Grid.Column="{i}">{button(text, True, kind)}</Border>' for i, (kind, text) in enumerate(names))
    return f'<Grid ColumnDefinitions="*,*" ColumnSpacing="16">{cells}</Grid>'


def separator(text, card=False):
    classes = ' Classes="card"' if card else ""
    return f'<c:FieldSeparator{classes} Content="{text}" />'


def heading(title, subtitle, size=24, line=32, gap=4, subtitle_size=14, subtitle_line=20):
    return f"""<StackPanel Spacing="{gap}" HorizontalAlignment="Stretch">
  <TextBlock Text="{title}" FontSize="{size}" LineHeight="{line}" FontWeight="Bold" TextAlignment="Center" TextWrapping="Wrap" />
  <TextBlock Text="{subtitle}" FontSize="{subtitle_size}" LineHeight="{subtitle_line}" Foreground="{MUTED}" TextAlignment="Center" TextWrapping="Wrap" />
</StackPanel>"""


def card(title, subtitle, content, centered=False):
    if centered:
        header = (f'<c:CardHeader><c:CardHeader.Title><TextBlock Text="{title}" FontSize="20" LineHeight="28" TextAlignment="Center" /></c:CardHeader.Title>'
                  f'<c:CardHeader.Description><TextBlock Text="{subtitle}" TextAlignment="Center" /></c:CardHeader.Description></c:CardHeader>')
    else:
        header = f'<c:CardHeader Title="{title}" Description="{subtitle}" />'
    return f"""<c:Card>
  {header}
  <c:CardContent>
    <c:FieldGroup>
{indent_lines(content, 6)}
    </c:FieldGroup>
  </c:CardContent>
</c:Card>"""


def indent_lines(text, spaces):
    return "\n".join(" " * spaces + line for line in text.split("\n"))


def group(*parts):
    return "\n".join(parts)


def stack(*parts, spacing=12):
    return f'<c:Field>\n{indent_lines(group(*parts), 2)}\n</c:Field>' if spacing == 12 else f'<StackPanel Spacing="{spacing}">\n{indent_lines(group(*parts), 2)}\n</StackPanel>'


def page(body, muted=False):
    background = ' Background="{DynamicResource CosmoMutedBrush}"' if muted else ""
    return f"""
        <Border Classes="page" Padding="40"{background}>
{indent_lines(body, 10)}
        </Border>
        """


def column(width, body, spacing=None, classes=None):
    gap = f' Spacing="{spacing}"' if spacing else ""
    named = f' Classes="{classes}"' if classes else ""
    return f'<StackPanel{named} MaxWidth="{width}"{gap}>\n{indent_lines(body, 2)}\n</StackPanel>'


SHADE = """<Border.Resources>
  <ResourceDictionary>
    <ResourceDictionary.ThemeDictionaries>
      <ResourceDictionary x:Key="Light"><SolidColorBrush x:Key="PlaceholderShade" Color="Transparent" /></ResourceDictionary>
      <ResourceDictionary x:Key="Dark"><SolidColorBrush x:Key="PlaceholderShade" Color="#CC000000" /></ResourceDictionary>
    </ResourceDictionary.ThemeDictionaries>
  </ResourceDictionary>
</Border.Resources>"""


def image(corner):
    return (f'<Border Background="#EAEAEA" CornerRadius="{corner}" ClipToBounds="True">{SHADE}<Panel>'
            f'<Image Source="/Assets/placeholder.png" Stretch="UniformToFill" AutomationProperties.Name="Image" />'
            f'<Border Background="{{DynamicResource PlaceholderShade}}" /></Panel></Border>')


def split(form):
    return f"""<c:Card Padding="0" ClipToBounds="True">
  <c:CardContent Padding="0">
    <Grid ColumnDefinitions="*,*">
      <c:FieldGroup Classes="split" Margin="32">
{indent_lines(form, 8)}
      </c:FieldGroup>
      <Border Classes="cover" Grid.Column="1">{image("0,13,13,0")}</Border>
    </Grid>
  </c:CardContent>
</c:Card>"""


LOGIN_01 = card("Login to your account", "Enter your email below to login to your account", group(
    email(), password(),
    stack(button("Login"), button("Login with Google", True), account("Don't have an account?", "Sign up"))))

LOGIN_02 = group(
    heading("Login to your account", "Enter your email below to login to your account"),
    email(), password(), stack(button("Login")), separator("Or continue with"),
    stack(button("Login with GitHub", True, "github"), account("Don't have an account?", "Sign up")))

LOGIN_03 = card("Welcome back", "Login with your Apple or Google account", group(
    stack(button("Login with Apple", True, "apple"), button("Login with Google", True, "google")),
    separator("Or continue with", card=True), email(), password(),
    stack(button("Login"), account("Don't have an account?", "Sign up"))), centered=True)

LOGIN_04 = split(group(
    heading("Welcome back", "Login to your Acme Inc account", gap=8, subtitle_size=16, subtitle_line=24),
    email(), password(), stack(button("Login")), separator("Or continue with", card=True),
    icon_buttons([("apple", "Login with Apple"), ("google", "Login with Google"), ("meta", "Login with Meta")], 3),
    account("Don't have an account?", "Sign up")))

FIVE_HEADER = """<StackPanel Spacing="8">
  <Border a:Web.Href="#" AutomationProperties.Name="Acme Inc." HorizontalAlignment="Center">{logo}</Border>
  <TextBlock Text="Welcome to Acme Inc." FontSize="20" LineHeight="28" FontWeight="Bold" TextAlignment="Center" />
  {account}
</StackPanel>"""

LOGIN_05 = group(
    FIVE_HEADER.format(logo=logo(32, 24, False), account=account("Don't have an account?", "Sign up")),
    email(), stack(button("Login")), separator("Or"),
    wide_buttons([("apple", "Continue with Apple"), ("google", "Continue with Google")]))

CONTACT = "We'll use this to contact you. We will not share your email with anyone else."

SIGNUP_01 = card("Create an account", "Enter your information below to create your account", group(
    field("Full Name", "name", placeholder="John Doe"), email(CONTACT),
    field("Password", "password", "password", note="Must be at least 8 characters long."),
    field("Confirm Password", "confirm", "password", note="Please confirm your password."),
    '<c:FieldGroup>\n' + indent_lines(stack(button("Create Account"), button("Sign up with Google", True), account("Already have an account?", "Sign in", ' Margin="24,0"')), 2) + '\n</c:FieldGroup>'))

SIGNUP_02 = group(
    heading("Create your account", "Fill in the form below to create your account"),
    field("Full Name", "name", placeholder="John Doe"), email(CONTACT),
    field("Password", "password", "password", note="Must be at least 8 characters long."),
    field("Confirm Password", "confirm", "password", note="Please confirm your password."),
    stack(button("Create Account")), separator("Or continue with"),
    stack(button("Sign up with GitHub", True, "github"), account("Already have an account?", "Sign in", ' Margin="24,0"')))

SIGNUP_03 = card("Create your account", "Enter your email below to create your account", group(
    field("Full Name", "name", placeholder="John Doe"), email(), passwords(),
    stack(button("Create Account"), account("Already have an account?", "Sign in"))), centered=True)

SIGNUP_04 = split(group(
    heading("Create your account", "Enter your email below to create your account", gap=8),
    email(CONTACT), passwords(), stack(button("Create Account")), separator("Or continue with", card=True),
    icon_buttons([("apple", "Sign up with Apple"), ("google", "Sign up with Google"), ("meta", "Sign up with Meta")], 3),
    account("Already have an account?", "Sign in")))

SIGNUP_05 = group(
    FIVE_HEADER.format(logo=logo(32, 24, False), account=account("Already have an account?", "Sign in")),
    email(), stack(button("Create Account")), separator("Or"),
    wide_buttons([("apple", "Continue with Apple"), ("google", "Continue with Google")]))


def bare(form, width):
    return column(width, f'<c:FieldGroup>\n{indent_lines(form, 2)}\n</c:FieldGroup>')


def with_terms(body, width=384, classes=None):
    return column(width, group(body, TERMS), spacing=24, classes=classes)


FIXTURES = {
    "login-01": page(column(384, LOGIN_01)),
    "login-02": page(stack(brand(), bare(LOGIN_02, 320), spacing=16)),
    "login-03": page(column(384, group(brand("Center"), LOGIN_03, TERMS), spacing=24), muted=True),
    "login-04": page(with_terms(LOGIN_04, 896, "split"), muted=True),
    "login-05": page(with_terms(bare(LOGIN_05, 384))),
    "signup-01": page(column(384, SIGNUP_01)),
    "signup-02": page(stack(brand(), bare(SIGNUP_02, 320), spacing=16)),
    "signup-03": page(column(384, group(brand("Center"), SIGNUP_03, TERMS), spacing=24), muted=True),
    "signup-04": page(with_terms(SIGNUP_04, 896, "split"), muted=True),
    "signup-05": page(with_terms(bare(SIGNUP_05, 384))),
}

TITLES = {
    "login-01": "A simple login form.",
    "login-02": "A two column login page with a cover image.",
    "login-03": "Login page with a muted background color.",
    "login-04": "Login page with form and image.",
    "login-05": "A simple email-only login page.",
    "signup-01": "A simple signup form.",
    "signup-02": "A two column signup page with a cover image.",
    "signup-03": "Signup page with a muted background color.",
    "signup-04": "Signup page with form and image.",
    "signup-05": "A simple signup form with social providers.",
}


PAGE = """<UserControl xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:a="https://aioros.dev" xmlns:c="https://aioros.dev/cosmo"
             a:Page.Title="{title}" a:Page.Description="{lead}">
  <StackPanel Spacing="24">
    <StackPanel Spacing="10">
      <TextBlock Classes="h1" Text="{title}" />
      <TextBlock Classes="lead" Text="{lead}" />
    </StackPanel>
    <Border MinHeight="720" CornerRadius="{{DynamicResource CosmoRadiusXl}}" BorderThickness="1" BorderBrush="{{DynamicResource CosmoBorderBrush}}"
            Background="{{DynamicResource {background}}}" ClipToBounds="True" Container.Name="block" Container.Sizing="Width">
      <Border.Styles>
        <Style Selector="Border.page"><Setter Property="Padding" Value="40" /></Style>
        <Style Selector="StackPanel.split"><Setter Property="MaxWidth" Value="896" /></Style>
        <Style Selector="c|FieldGroup.split"><Setter Property="Margin" Value="32" /></Style>
        <ContainerQuery Name="block" Query="max-width:767">
          <Style Selector="Border.page"><Setter Property="Padding" Value="24" /></Style>
          <Style Selector="StackPanel.split"><Setter Property="MaxWidth" Value="384" /></Style>
          <Style Selector="c|FieldGroup.split"><Setter Property="Margin" Value="24" /><Setter Property="Grid.ColumnSpan" Value="2" /></Style>
          <Style Selector="Border.cover"><Setter Property="IsVisible" Value="False" /></Style>
        </ContainerQuery>
      </Border.Styles>
      <Border VerticalAlignment="Center">
{body}
      </Border>
    </Border>
  </StackPanel>
</UserControl>
"""

MUTED_PAGES = {"login-03", "login-04", "signup-03", "signup-04"}


def write_pages(pages):
    """Writes each block as a Gallery page, /blocks/{block}: the block in a frame as tall as a page, its content centered."""
    for name, body in FIXTURES.items():
        folder = pages / "blocks" / name
        folder.mkdir(parents=True, exist_ok=True)
        background = "CosmoMutedBrush" if name in MUTED_PAGES else "CosmoBackgroundBrush"
        for local in ('Classes="page" Padding="40"', 'Classes="split" MaxWidth="896"', 'Classes="split" Margin="32"'):
            body = body.replace(local, local.split(" ")[0])
        (folder / "Page.axaml").write_text(PAGE.format(title=name, lead=TITLES[name], background=background,
                                                       body=indent(dedent(body).strip("\n"), " " * 8)))
