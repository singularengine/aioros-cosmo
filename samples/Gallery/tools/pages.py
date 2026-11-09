"""Writes the Gallery's component pages: each example's XAML is shown in its preview and, escaped, in its code block."""
import re
from html import escape
from pathlib import Path
from textwrap import dedent, indent

ROOT = Path(__file__).resolve().parent.parent / "Gallery" / "Pages" / "components"

HEAD = """<UserControl xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:a="https://aioros.dev" xmlns:c="https://aioros.dev/cosmo" xmlns:m="using:Gallery.Models"{datatype}
             a:Page.Title="{title}" a:Page.Description="{lead}">{styles}
  <StackPanel Spacing="32" MaxWidth="880" HorizontalAlignment="Left">
    <StackPanel Spacing="10">
      <TextBlock Classes="h1" Text="{title}" />
      <TextBlock Classes="lead" Text="{lead}" />
      <WrapPanel ItemSpacing="8" LineSpacing="8">
{links}
      </WrapPanel>
    </StackPanel>
"""

TAIL = """  </StackPanel>
</UserControl>
"""


def code(xaml):
    lines = dedent(xaml).strip("\n")
    return escape(lines, quote=True).replace("\n", "&#10;")


PHONE = 288


def fluid(xaml):
    """A demo whose root is wider than a phone's preview takes its width as a MaxWidth and stretches up to it (shadcn/ui's
    w-full max-w-*), so it follows a phone; returns the demo and whether it did."""
    body = dedent(xaml).strip("\n")
    end = re.match(r'\s*<[^>\n]*>?', body).end()
    root = body[:end]
    if (wide := re.search(r' MaxWidth="(\d+)"', root)) and int(wide.group(1)) > PHONE:
        return xaml, True
    width = re.search(r' Width="(\d+)"', root)
    if not width or int(width.group(1)) <= PHONE:
        return xaml, False
    root = root.replace(width.group(0), f' MaxWidth="{width.group(1)}"', 1)
    root = re.sub(r' HorizontalAlignment="(Left|Center)"', "", root, count=1)
    return root + body[end:], True


def played(xaml, model, path):
    """A fixture whose model plays a stream back, bound to that model on the page's own (one shared instance, which the
    page's live stream and its posts both reach)."""
    for tag in ("StackPanel", "DockPanel"):
        context = f"<{tag}.DataContext><m:{model} /></{tag}.DataContext>"
        if context in xaml:
            xaml = re.sub(r"\n *" + re.escape(context), "", xaml).replace(f"<{tag} ", f'<{tag} DataContext="{{Binding {path}}}" ', 1)
    return xaml


def phone_wide(xaml):
    """A demo whose inner blocks are 448 px wide (shadcn/ui's w-full max-w-md inside a column) takes that width as a
    MaxWidth on each, so it follows a phone."""
    return re.sub(r' Width="448"( HorizontalAlignment="(Left|Center)")?', ' MaxWidth="448"', xaml)


def example(title, xaml, text=None, align="Center", wrap=False, shown=None):
    if "<c:Card" in xaml:
        align = "Stretch"
    xaml, stretched = fluid(xaml)
    shown = fluid(shown)[0] if shown else xaml
    if stretched:
        align = "Stretch"
    body = indent(dedent(xaml).strip("\n"), " " * 10)
    wrapping = ' TextWrapping="Wrap"' if wrap else ""
    note = f'\n      <TextBlock Classes="sm muted" Text="{escape(text, quote=True)}"{wrapping} />' if text else ""
    return f"""    <StackPanel Spacing="12">
      <TextBlock Classes="h2" Text="{escape(title, quote=True)}" />{note}
      <Border Classes="preview">
        <Border HorizontalAlignment="{align}" VerticalAlignment="Center">
{body}
        </Border>
      </Border>
      <Border Classes="code"><TextBlock Text="{code(shown)}" /></Border>
    </StackPanel>
"""


def table(title, rows):
    cells = "\n".join(
        f"""        <Border Classes="row"><Grid ColumnDefinitions="200,*" ColumnSpacing="16"><TextBlock Classes="sm" FontWeight="Medium" Text="{escape(name, quote=True)}" /><TextBlock Grid.Column="1" Classes="sm muted" Text="{escape(what, quote=True)}"{' TextWrapping="Wrap"' if wrap else ""} /></Grid></Border>"""
        for name, what, *wrap in rows
    )
    return f"""    <StackPanel Spacing="12">
      <TextBlock Classes="h2" Text="{escape(title, quote=True)}" />
      <StackPanel>
{cells}
      </StackPanel>
    </StackPanel>
"""


def badge_link(text):
    return f'        <c:Badge Classes="secondary" Content="{escape(text, quote=True)}" />'


COUNT = """
  <UserControl.Styles>
    <Style Selector="c|Badge.count">
      <Setter Property="Height" Value="20" />
      <Setter Property="MinWidth" Value="20" />
      <Setter Property="Padding" Value="4,0" />
      <Setter Property="FontFeatures" Value="+tnum" />
      <Setter Property="FontFamily" Value="{DynamicResource CosmoFontMono}" />
    </Style>
  </UserControl.Styles>"""


def page(route, title, lead, links, sections, styles="", datatype=None, namespaces="", extra=""):
    folder = ROOT / route
    folder.mkdir(parents=True, exist_ok=True)
    data = namespaces + (f' x:DataType="m:{datatype}"' if datatype else "")
    content = HEAD.format(title=title, lead=escape(lead, quote=True), links="\n".join(badge_link(l) for l in links), styles=styles, datatype=data) + "".join(sections) + extra + TAIL
    (folder / "Page.axaml").write_text(content)


page("button", "Button", "Displays a button or a component that looks like a button.", ["shadcn/ui: button", "Avalonia: Button, HyperlinkButton", "Web: <button>, <a>"], [
    example("Usage", """
        <Button Content="Button" />
        """),
    example("Outline", """
        <Button Classes="outline" Content="Outline" />
        """),
    example("Secondary", """
        <Button Classes="secondary" Content="Secondary" />
        """),
    example("Ghost", """
        <Button Classes="ghost" Content="Ghost" />
        """),
    example("Destructive", """
        <Button Classes="destructive" Content="Destructive" />
        """),
    example("Link", """
        <Button Classes="link" Content="Link" />
        """),
    example("Icon", """
        <Button Classes="outline icon" AutomationProperties.Name="Next">
          <a:Icon Kind="chevron-right" />
        </Button>
        """, "An icon-only button is square; name it for assistive technologies."),
    example("With icon", """
        <WrapPanel ItemSpacing="8" LineSpacing="8">
          <Button Classes="has-icon">
            <StackPanel>
              <a:Icon Kind="mail" />
              <TextBlock Text="Login with Email" />
            </StackPanel>
          </Button>
          <Button Classes="outline sm has-icon">
            <StackPanel>
              <a:Icon Kind="plus" />
              <TextBlock Text="New project" />
            </StackPanel>
          </Button>
        </WrapPanel>
        """, "Content in a StackPanel is laid out in a row with shadcn's gap (8 px, 6 px for sm, 4 px for xs). The has-icon class gives the tighter padding shadcn applies when a button holds an icon."),
    example("Sizes", """
        <StackPanel Spacing="12">
          <WrapPanel ItemSpacing="8" LineSpacing="8">
            <Button Classes="outline xs" Content="Extra small" />
            <Button Classes="outline sm" Content="Small" />
            <Button Classes="outline" Content="Default" />
            <Button Classes="outline lg" Content="Large" />
          </WrapPanel>
          <WrapPanel ItemSpacing="8" LineSpacing="8">
            <Button Classes="outline icon-xs" AutomationProperties.Name="Add"><a:Icon Kind="plus" /></Button>
            <Button Classes="outline icon-sm" AutomationProperties.Name="Add"><a:Icon Kind="plus" /></Button>
            <Button Classes="outline icon" AutomationProperties.Name="Add"><a:Icon Kind="plus" /></Button>
            <Button Classes="outline icon-lg" AutomationProperties.Name="Add"><a:Icon Kind="plus" /></Button>
          </WrapPanel>
        </StackPanel>
        """),
    example("Disabled", """
        <WrapPanel ItemSpacing="8" LineSpacing="8">
          <Button IsEnabled="False" Content="Default" />
          <Button IsEnabled="False" Classes="outline" Content="Outline" />
          <Button IsEnabled="False" Classes="secondary" Content="Secondary" />
        </WrapPanel>
        """),
    example("As a link", """
        <WrapPanel ItemSpacing="8" LineSpacing="8">
          <HyperlinkButton NavigateUri="/components/badge" Content="Go to Badge" />
          <HyperlinkButton Classes="outline" NavigateUri="/components/card" Content="Go to Card" />
        </WrapPanel>
        """, "shadcn's Button asChild with a link: a HyperlinkButton takes every Button class and is written as an <a href> on the web."),
    example("Command", """
        <StackPanel Orientation="Horizontal" Spacing="12">
          <Button Classes="outline" Command="{Binding SaveCommand}" CommandParameter="draft" Content="Save draft" />
          <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Status}" />
        </StackPanel>
        """, "Commands and their parameters run on the server when the button posts on the web, and on click on the desktop: see Binding and MVVM."),
    table("API", [
        ("(no class)", "The default variant: primary background, primary foreground; 90% primary on hover."),
        ("destructive", "Destructive background (60% in dark), white text."),
        ("outline", "Background with a border and shadow-xs; accent on hover (input at 30% and 50% in dark)."),
        ("secondary", "Secondary background; 80% on hover."),
        ("ghost", "Transparent; accent on hover (50% in dark)."),
        ("link", "Primary text, no background."),
        ("xs, sm, lg", "Heights 24, 32 and 40 px (default 36) with shadcn's paddings."),
        ("icon, icon-xs, icon-sm, icon-lg", "Square buttons of 36, 24, 32 and 40 px."),
        ("has-icon", "The tighter horizontal padding shadcn gives a button holding an icon (has-[>svg])."),
        ("Command, CommandParameter", "Avalonia's ICommand source: bound commands run on post (web) and click (desktop)."),
        ("Content, ContentTemplate", "Any content: text, an icon and text in a StackPanel, or a DataTemplate for a model."),
    ]),
], datatype="ButtonExamples")

page("badge", "Badge", "Displays a badge or a component that looks like a badge.", ["shadcn/ui: badge", "Cosmo: c:Badge (ContentControl)"], [
    example("Usage", """
        <c:Badge Content="Badge" />
        """),
    example("Variants", """
        <WrapPanel ItemSpacing="8" LineSpacing="8">
          <c:Badge Content="Default" />
          <c:Badge Classes="secondary" Content="Secondary" />
          <c:Badge Classes="destructive" Content="Destructive" />
          <c:Badge Classes="outline" Content="Outline" />
          <c:Badge Classes="ghost" Content="Ghost" />
          <c:Badge Classes="link" Content="Link" />
        </WrapPanel>
        """),
    example("With icon", """
        <WrapPanel ItemSpacing="8" LineSpacing="8">
          <c:Badge Classes="secondary">
            <StackPanel>
              <a:Icon Kind="check" Size="12" />
              <TextBlock Text="Verified" />
            </StackPanel>
          </c:Badge>
          <c:Badge Classes="outline">
            <StackPanel>
              <a:Icon Kind="trending-up" Size="12" />
              <TextBlock Text="+12.5%" />
            </StackPanel>
          </c:Badge>
        </WrapPanel>
        """),
    example("Custom", """
        <WrapPanel ItemSpacing="8" LineSpacing="8">
          <c:Badge Classes="count" Content="8" />
          <c:Badge Classes="count destructive" Content="99" />
          <c:Badge Classes="count outline" Content="20+" />
          <c:Badge Background="#2B7FFF" Foreground="White" Content="Custom color" />
        </WrapPanel>
        """, "Like className in shadcn's examples, a style or a local value customizes a badge: the count class here is a page style (Height 20, MinWidth 20, Padding 4,0, tabular numbers)."),
    example("Link", """
        <WrapPanel ItemSpacing="8" LineSpacing="8">
          <HyperlinkButton Theme="{DynamicResource CosmoBadgeLink}" NavigateUri="/components/badge" Content="Default" />
          <HyperlinkButton Theme="{DynamicResource CosmoBadgeLink}" Classes="secondary" NavigateUri="/components/badge" Content="Secondary" />
          <HyperlinkButton Theme="{DynamicResource CosmoBadgeLink}" Classes="outline" NavigateUri="/components/badge" Content="Outline" />
          <HyperlinkButton Theme="{DynamicResource CosmoBadgeLink}" Classes="link" NavigateUri="/components/badge" Content="Link" />
        </WrapPanel>
        """, "shadcn's <Badge asChild><a href>: a HyperlinkButton with the CosmoBadgeLink theme is an <a href> with the badge's look, its hover per variant and a focus ring."),
    table("API", [
        ("(no class)", "Primary background and foreground."),
        ("secondary", "Secondary background and foreground."),
        ("destructive", "Destructive background (60% in dark), white text."),
        ("outline", "Transparent with the border color and the foreground."),
        ("ghost", "Transparent, foreground text."),
        ("link", "Transparent, primary text."),
        ("Content, ContentTemplate", "Text, an icon with text in a StackPanel (4 px apart), or a DataTemplate."),
        ("Classes.destructive=\"{Binding …}\"", "Any class can be bound, with a converter, to choose the variant from the model."),
        ("Theme=\"{DynamicResource CosmoBadgeLink}\"", "On a HyperlinkButton: a badge that is a link, the same classes choosing its variant."),
    ]),
], styles=COUNT)

page("card", "Card", "Displays a card with header, content, and footer.", ["shadcn/ui: card", "Cosmo: c:Card and its parts"], [
    example("Usage", """
        <c:Card MaxWidth="384">
          <c:CardHeader Title="Login to your account" Description="Enter your email below to login to your account">
            <c:CardHeader.Action>
              <Button Classes="link" Content="Sign Up" />
            </c:CardHeader.Action>
          </c:CardHeader>
          <c:CardContent>
            <TextBlock Classes="sm" Text="Your session expires in 30 days on this device." />
          </c:CardContent>
          <c:CardFooter>
            <StackPanel Spacing="8">
              <Button Content="Login" HorizontalAlignment="Stretch" />
              <Button Classes="outline" Content="Login with Google" HorizontalAlignment="Stretch" />
            </StackPanel>
          </c:CardFooter>
        </c:Card>
        """),
    example("Header only", """
        <c:Card MaxWidth="384">
          <c:CardHeader Title="Notifications" Description="You have 3 unread messages." />
          <c:CardContent>
            <TextBlock Classes="sm" Text="Push notifications are sent to your phone." />
          </c:CardContent>
        </c:Card>
        """, "Without a description or an action the header keeps no space for them."),
    example("Separated header and footer", """
        <c:Card MaxWidth="384">
          <c:CardHeader Classes="border" Title="Team members" Description="Invite your team to collaborate." />
          <c:CardContent>
            <TextBlock Classes="sm" Text="Members can edit every project." />
          </c:CardContent>
          <c:CardFooter Classes="border">
            <Button Content="Invite" />
          </c:CardFooter>
        </c:Card>
        """, "The border class rules the header off with a bottom border 24 px below it, and the footer with a top border 24 px above it (shadcn's border-b and border-t)."),
    example("Stat", """
        <c:Card MaxWidth="320">
          <c:CardHeader Title="Total revenue" Description="January to June 2026">
            <c:CardHeader.Action>
              <c:Badge Classes="outline">
                <StackPanel>
                  <a:Icon Kind="trending-up" Size="12" />
                  <TextBlock Text="+12.5%" />
                </StackPanel>
              </c:Badge>
            </c:CardHeader.Action>
          </c:CardHeader>
          <c:CardContent>
            <TextBlock Classes="stat" Text="$45,231.89" />
          </c:CardContent>
          <c:CardFooter>
            <TextBlock Classes="sm muted" Text="Trending up this month" />
          </c:CardFooter>
        </c:Card>
        """),
    table("API", [
        ("c:Card", "The surface: card background, border, 14 px radius (radius-xl), shadow-sm, 24 px above and below; its parts stack 24 px apart."),
        ("c:CardHeader", "Title (semibold), Description (14 px, muted) and Action (top right); each can be bound."),
        ("c:CardContent", "Content inset 24 px on both sides."),
        ("c:CardFooter", "Content inset 24 px on both sides, centered vertically."),
    ]),
])

page("label", "Label", "Renders an accessible label associated with controls.", ["shadcn/ui: label", "Avalonia: Label", "Web: <label for>"], [
    example("Usage", """
        <StackPanel Orientation="Horizontal" Spacing="8">
          <CheckBox Name="terms" a:Web.Id="terms" />
          <Label Content="Accept terms and conditions" Target="{Binding #terms}" />
        </StackPanel>
        """, "Target names the control the label is for: on the web it is the label's for attribute (the target needs an a:Web.Id), on the desktop clicking the label focuses the target."),
    example("With an input", """
        <StackPanel Spacing="12" Width="320">
          <Label Content="Email" Target="{Binding #email}" />
          <TextBox Name="email" a:Web.Id="email" PlaceholderText="Email" />
        </StackPanel>
        """),
    example("Disabled", """
        <Label Content="Disabled label" IsEnabled="False" />
        """),
    table("API", [
        ("Content", "Text or any content; it can be bound (with a StringFormat) when the label has no Target."),
        ("Target", "The control the label names: its for attribute on the web."),
        ("(look)", "14 px medium text on a 14 px line, no padding, the color of its parent (a label in an invalid field turns destructive)."),
    ]),
])

page("input", "Input", "Displays a form input field or a component that looks like an input field.", ["shadcn/ui: input", "Avalonia: TextBox", "Web: <input>"], [
    example("Usage", """
        <TextBox PlaceholderText="Email" Width="320" />
        """),
    example("With label", """
        <StackPanel Spacing="12" Width="320">
          <Label Content="Email" Target="{Binding #email}" />
          <TextBox Name="email" a:Web.Id="email" a:Web.InputType="email" PlaceholderText="Email" />
        </StackPanel>
        """),
    example("Password", """
        <TextBox PasswordChar="•" PlaceholderText="Password" Width="320" />
        """),
    example("Disabled", """
        <TextBox IsEnabled="False" PlaceholderText="Email" Width="320" />
        """),
    example("Invalid", """
        <TextBox Text="not-an-email" Width="320">
          <DataValidationErrors.Error>
            <sys:Exception>
              <x:Arguments>
                <x:String>Enter a valid email address.</x:String>
              </x:Arguments>
            </sys:Exception>
          </DataValidationErrors.Error>
        </TextBox>
        """, "A data validation error (from INotifyDataErrorInfo, an exception or set as here) gives the destructive border and shows the message below, as shadcn's aria-invalid input with a FieldError."),
    example("Binding", """
        <StackPanel Spacing="12" Width="320">
          <Label Content="{Binding Unread, StringFormat='Name ({0} unread)'}" />
          <TextBox Text="{Binding Name}" PlaceholderText="Your name" AutomationProperties.Name="Name" />
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Command="{Binding GreetCommand}" Content="Greet" />
            <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Greeting}" />
          </StackPanel>
        </StackPanel>
        """, "Text is bound two way: the web posts the field, binds it into the model and runs the command; the desktop updates the model as you type."),
    example("File", """
        <StackPanel Spacing="12" Width="384">
          <Label Content="Picture" Target="{Binding #picture}" />
          <a:FilePicker Name="picture" Content="Picture" />
        </StackPanel>
        """, "An a:FilePicker takes the input's look: the chosen file's name replaces No file chosen, on the desktop and on the web."),
    example("File, translated", """
        <a:FilePicker Width="384" Content="Foto" c:FileInput.ChooseText="Elegir archivo" c:FileInput.EmptyText="Ningún archivo" />
        """, "The browser's file input writes these texts in the page's language; c:FileInput.ChooseText and c:FileInput.EmptyText set them on both targets."),
    table("API", [
        ("Text", "Two-way bindable text."),
        ("PlaceholderText", "The placeholder, in the muted foreground."),
        ("PasswordChar", "Masks the text: an <input type=password> on the web."),
        ("a:Web.InputType", "The input's type on the web (email, tel, url, search…)."),
        ("IsEnabled", "Disabled: 50% opacity."),
        ("DataValidationErrors", "Errors show the destructive border and the message below (shadcn's aria-invalid with FieldError)."),
        ("a:FilePicker", "shadcn's <Input type=file>: File or Files bound two way, FileTypes as the accepted types."),
        ("c:FileInput.ChooseText, c:FileInput.EmptyText", "The file input's texts (Choose File, No file chosen), to translate them."),
    ]),
], datatype="InputExamples", namespaces=' xmlns:sys="using:System"')

page("textarea", "Textarea", "Displays a form textarea or a component that looks like a textarea.", ["shadcn/ui: textarea", "Avalonia: TextBox AcceptsReturn", "Web: <textarea>"], [
    example("Usage", """
        <TextBox AcceptsReturn="True" PlaceholderText="Type your message here." Width="384" />
        """),
    example("With label", """
        <StackPanel Spacing="12" Width="384">
          <Label Content="Your message" Target="{Binding #message}" />
          <TextBox Name="message" a:Web.Id="message" AcceptsReturn="True" PlaceholderText="Type your message here." />
        </StackPanel>
        """),
    example("Disabled", """
        <TextBox AcceptsReturn="True" IsEnabled="False" PlaceholderText="Type your message here." Width="384" />
        """),
    example("Binding", """
        <StackPanel Spacing="8" Width="384">
          <TextBox AcceptsReturn="True" Text="{Binding Message}" PlaceholderText="Type your message here." AutomationProperties.Name="Message" />
          <TextBlock Classes="sm muted" Text="{Binding Message.Length, StringFormat='{}{0}/100 characters'}" />
        </StackPanel>
        """),
    table("API", [
        ("AcceptsReturn", "True makes the TextBox shadcn's textarea: 64 px minimum height, 8 px vertical padding, wrapping, growing with its content."),
        ("Text", "Two-way bindable text."),
    ]),
], datatype="InputExamples")

page("field", "Field", "Combine labels, controls, and help text to compose accessible form fields and grouped inputs.", ["shadcn/ui: field", "Cosmo: c:Field, c:FieldGroup, c:FieldSet, c:FieldContent, c:FieldLabel, c:FieldTitle, c:FieldLegend, c:FieldDescription, c:FieldError"], [
    example("Usage", """
        <c:FieldGroup Width="448">
          <c:Field>
            <c:FieldLabel Content="Username" Target="{Binding #username}" />
            <TextBox Name="username" a:Web.Id="username" PlaceholderText="evilrabbit" />
            <c:FieldDescription Text="Choose a unique username for your account." />
          </c:Field>
          <c:Field Classes="invalid">
            <c:FieldLabel Content="Email" />
            <TextBox Text="not-an-email" />
            <c:FieldError Text="Enter a valid email address." />
          </c:Field>
        </c:FieldGroup>
        """),
    example("Field set", """
        <c:FieldSet Width="448">
          <c:FieldLegend Text="Notifications" />
          <c:FieldDescription Text="Choose what we email you about." />
          <c:Field Orientation="Horizontal">
            <CheckBox Name="news" a:Web.Id="news" IsChecked="True" />
            <c:FieldLabel Content="Product news" FontWeight="Normal" Target="{Binding #news}" />
          </c:Field>
          <c:Field Orientation="Horizontal">
            <CheckBox Name="tips" a:Web.Id="tips" />
            <c:FieldLabel Content="Tips and tutorials" FontWeight="Normal" Target="{Binding #tips}" />
          </c:Field>
        </c:FieldSet>
        """),
    example("Content column", """
        <c:FieldGroup Width="448">
          <c:Field Orientation="Horizontal">
            <c:FieldContent>
              <c:FieldLabel Content="Multi-factor authentication" Target="{Binding #mfa}" />
              <c:FieldDescription Text="Enable multi-factor authentication. If you do not have a two-factor device, you can use a one-time code sent to your email." />
            </c:FieldContent>
            <ToggleSwitch Name="mfa" a:Web.Id="mfa" />
          </c:Field>
          <c:Field Orientation="Horizontal">
            <c:FieldContent>
              <c:FieldTitle Text="Kubernetes" />
              <c:FieldDescription Text="Run GPU workloads on a K8s configured cluster." />
            </c:FieldContent>
            <CheckBox Name="k8s" a:Web.Id="k8s" />
          </c:Field>
        </c:FieldGroup>
        """, "A FieldContent takes the row its control leaves; the control stands at the row's top beside it."),
    example("Responsive", """
        <c:FieldGroup Width="560">
          <c:Field Classes="responsive">
            <c:FieldContent>
              <c:FieldLabel Content="Name" Target="{Binding #fullname}" />
              <c:FieldDescription Text="Provide your full name for identification." />
            </c:FieldContent>
            <TextBox Name="fullname" a:Web.Id="fullname" MinWidth="200" PlaceholderText="Evil Rabbit" />
          </c:Field>
        </c:FieldGroup>
        """, "The responsive class makes a row when its field group is at least 448 px wide and a stack below it, on both targets; narrow the window to see it stack."),
    example("Conditional parts", """
        <c:FieldGroup Width="448">
          <c:Field Orientation="Horizontal">
            <ToggleSwitch Name="updates" a:Web.Id="updates" IsChecked="{Binding Updates}" />
            <c:FieldLabel Content="Product updates" Target="{Binding #updates}" />
          </c:Field>
          <c:Field>
            <c:FieldLabel Content="Email" Target="{Binding #updatesmail}" />
            <TextBox Name="updatesmail" a:Web.Id="updatesmail" PlaceholderText="m@example.com" />
            <c:FieldDescription IsVisible="{Binding Updates}" Text="We send one email a month." />
          </c:Field>
          <c:Field>
            <c:FieldLabel Content="Name" Target="{Binding #updatesname}" />
            <TextBox Name="updatesname" a:Web.Id="updatesname" PlaceholderText="Evil Rabbit" />
          </c:Field>
          <Button Content="Save" Command="{Binding SaveUpdatesCommand}" HorizontalAlignment="Left" />
        </c:FieldGroup>
        """, "A part whose IsVisible is bound leaves no gap while it is hidden, on both targets: switch the updates off and save, and the description goes with its 12 px."),
    example("Invalid control", """
        <c:Field Classes="invalid">
          <c:FieldLabel Content="Plan" />
          <Button Classes="outline" Content="Choose a plan" HorizontalAlignment="Left" />
          <c:FieldError Text="Choose a plan to continue." />
        </c:Field>
        """, "A button that is an invalid field's control, or that has an error of its own, takes the destructive border and focus ring."),
    example("Validation", """
        <c:FieldGroup Width="448">
          <c:Field Classes.invalid="{Binding TitleInvalid}">
            <c:FieldLabel Content="Bug title" />
            <TextBox Text="{Binding Title}" PlaceholderText="Login button not working on mobile" AutomationProperties.Name="Bug title" />
            <c:FieldDescription Text="At least 5 characters." />
          </c:Field>
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Command="{Binding SubmitCommand}" Content="Submit" />
            <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Status}" />
          </StackPanel>
        </c:FieldGroup>
        """, "The view model is an ObservableValidator: its data annotations put errors on the bound TextBox, shown below it in the destructive color; the invalid class, bound to the model, turns the label destructive. See the Forms example."),
    table("API", [
        ("c:Field", "A Grid of its shown parts: label, control, description and error 12 px apart (Spacing); Orientation=Horizontal puts them in a row, centered, where a FieldContent (or else a label) takes the room the others leave; the responsive class turns it into a row when its field group is at least 448 px wide; the invalid class (bindable: Classes.invalid) turns its texts and its control destructive."),
        ("c:FieldGroup", "Fields 28 px apart (16 px for a nested group); a container its responsive fields follow."),
        ("c:FieldSet", "A legend, its description and fields 24 px apart; a <fieldset> on the web."),
        ("c:FieldContent", "A label or title and its description, 6 px apart, beside its control in a row."),
        ("c:FieldTitle", "A field's title where its label would be: 14 px medium on a 19.25 px line."),
        ("c:FieldLabel", "A Label on the field's 19.25 px line; Target links it to its control."),
        ("c:FieldLegend", "16 px medium title of a field set, its <legend> on the web; the label class makes it 14 px."),
        ("c:FieldDescription", "14 px muted helper text on a 21 px line."),
        ("c:FieldError", "14 px destructive message; a control's data validation errors are shown the same way automatically."),
    ]),
], datatype="FormExample")

page("native-select", "Native Select", "A styled native HTML select element with consistent design system integration.", ["shadcn/ui: native-select", "Avalonia: ComboBox", "Web: <select>"], [
    example("Usage", """
        <ComboBox Classes="native" SelectedIndex="0">
          <ComboBoxItem Content="Select status" />
          <ComboBoxItem Content="Todo" />
          <ComboBoxItem Content="In Progress" />
          <ComboBoxItem Content="Done" />
          <ComboBoxItem Content="Cancelled" />
        </ComboBox>
        """, "On the web Aioros writes every ComboBox as a native <select>, which is what shadcn's NativeSelect is; on the desktop it opens Cosmo's drop-down."),
    example("Small", """
        <ComboBox Classes="native sm" SelectedIndex="0">
          <ComboBoxItem Content="Todo" />
          <ComboBoxItem Content="Done" />
        </ComboBox>
        """),
    example("Disabled", """
        <ComboBox Classes="native" SelectedIndex="0" IsEnabled="False">
          <ComboBoxItem Content="Todo" />
        </ComboBox>
        """),
    table("API", [
        ("native", "The chevron 14 px from the right edge, as NativeSelect places it."),
        ("sm", "32 px high."),
        ("SelectedIndex, SelectedItem", "Two-way bindable selection; a post selects the chosen option."),
    ]),
])

page("select", "Select", "Displays a list of options for the user to pick from—triggered by a button.", ["shadcn/ui: select", "Avalonia: ComboBox", "Web: <select>"], [
    example("Usage", """
        <ComboBox PlaceholderText="Select a fruit" Width="180">
          <ComboBoxItem Classes="label" Content="Fruits" />
          <ComboBoxItem Content="Apple" />
          <ComboBoxItem Content="Banana" />
          <ComboBoxItem Content="Blueberry" />
          <ComboBoxItem Content="Grapes" />
          <ComboBoxItem Content="Pineapple" />
        </ComboBox>
        """, "The label class is shadcn's SelectLabel and the separator class its SelectSeparator. Keyboard: arrows move, a letter jumps to the first item starting with it, Space opens, Return and Escape close."),
    example("Scrollable", """
        <ComboBox PlaceholderText="Select a timezone" Width="280">
          <ComboBoxItem Classes="label" Content="North America" />
          <ComboBoxItem Content="Eastern Standard Time (EST)" />
          <ComboBoxItem Content="Central Standard Time (CST)" />
          <ComboBoxItem Content="Mountain Standard Time (MST)" />
          <ComboBoxItem Content="Pacific Standard Time (PST)" />
          <ComboBoxItem Content="Alaska Standard Time (AKST)" />
          <ComboBoxItem Content="Hawaii Standard Time (HST)" />
          <ComboBoxItem Classes="label" Content="Europe &amp; Africa" />
          <ComboBoxItem Content="Greenwich Mean Time (GMT)" />
          <ComboBoxItem Content="Central European Time (CET)" />
          <ComboBoxItem Content="Eastern European Time (EET)" />
          <ComboBoxItem Content="Western European Summer Time (WEST)" />
          <ComboBoxItem Content="Central Africa Time (CAT)" />
          <ComboBoxItem Content="East Africa Time (EAT)" />
          <ComboBoxItem Classes="label" Content="Asia" />
          <ComboBoxItem Content="Moscow Time (MSK)" />
          <ComboBoxItem Content="India Standard Time (IST)" />
          <ComboBoxItem Content="China Standard Time (CST)" />
          <ComboBoxItem Content="Japan Standard Time (JST)" />
          <ComboBoxItem Content="Korea Standard Time (KST)" />
          <ComboBoxItem Content="Indonesia Central Standard Time (WITA)" />
        </ComboBox>
        """, "A list taller than MaxDropDownHeight (384) shows shadcn's scroll buttons instead of a scroll bar: hovering one scrolls the list an item at a time."),
    example("Groups", """
        <ComboBox PlaceholderText="Select a food" Width="180">
          <ComboBoxItem Classes="label" Content="Fruits" />
          <ComboBoxItem Content="Apple" />
          <ComboBoxItem Content="Banana" />
          <ComboBoxItem Classes="separator" />
          <ComboBoxItem Classes="label" Content="Vegetables" />
          <ComboBoxItem Content="Carrot" />
          <ComboBoxItem Content="Leek" />
        </ComboBox>
        """),
    example("Small and disabled", """
        <WrapPanel ItemSpacing="12" LineSpacing="12">
          <ComboBox Classes="sm" SelectedIndex="0" Width="180">
            <ComboBoxItem Content="Banana" />
          </ComboBox>
          <ComboBox SelectedIndex="0" Width="180" IsEnabled="False">
            <ComboBoxItem Content="Banana" />
          </ComboBox>
        </WrapPanel>
        """),
    example("Binding and templates", """
        <StackPanel Spacing="12">
          <ComboBox ItemsSource="{Binding Fruits}" SelectedItem="{Binding Fruit}" PlaceholderText="Pick a fruit" Width="200" AutomationProperties.Name="Fruit">
            <ComboBox.ItemTemplate>
              <DataTemplate x:DataType="m:Fruit">
                <StackPanel Orientation="Horizontal" Spacing="8">
                  <Ellipse Width="8" Height="8" Fill="{Binding Color}" VerticalAlignment="Center" />
                  <TextBlock Text="{Binding Name}" />
                </StackPanel>
              </DataTemplate>
            </ComboBox.ItemTemplate>
          </ComboBox>
          <TextBlock Classes="sm muted" Text="{Binding Picked}" />
        </StackPanel>
        """, "Items from the model with an ItemTemplate and SelectedItem bound two way; on the web the options show each item's text."),
    table("API", [
        ("ItemsSource, ItemTemplate", "Items from the model, drawn by a data template on the desktop."),
        ("SelectedItem, SelectedValue", "Two-way bindable selection (SelectedValueBinding picks the value)."),
        ("PlaceholderText", "Shown in the muted foreground while nothing is selected."),
        ("label, separator", "ComboBoxItem classes for shadcn's SelectLabel and SelectSeparator."),
        ("sm", "32 px high."),
    ]),
], datatype="SelectExamples")

page("checkbox", "Checkbox", "A control that allows the user to toggle between checked and not checked.", ["shadcn/ui: checkbox", "Avalonia: CheckBox", "Web: <input type=checkbox>"], [
    example("Usage", """
        <CheckBox Content="Accept terms and conditions" />
        """, "The content sits 12 px after the box in the label's look, as shadcn's checkbox beside a Label."),
    example("States", """
        <StackPanel Spacing="16">
          <CheckBox Content="Enable notifications" IsChecked="True" />
          <CheckBox Content="Select all" IsThreeState="True" IsChecked="{x:Null}" />
          <CheckBox Content="Disabled" IsEnabled="False" />
        </StackPanel>
        """, "IsThreeState with IsChecked null is Radix's indeterminate state."),
    example("With text", """
        <Grid ColumnDefinitions="Auto,*" ColumnSpacing="12">
          <CheckBox Name="terms2" a:Web.Id="terms2" VerticalAlignment="Top" />
          <StackPanel Grid.Column="1" Spacing="8">
            <Label Content="Accept terms and conditions" Target="{Binding #terms2}" />
            <TextBlock Classes="sm muted" Text="By clicking this checkbox, you agree to the terms and conditions." TextWrapping="Wrap" />
          </StackPanel>
        </Grid>
        """),
    example("Binding", """
        <StackPanel Spacing="12">
          <CheckBox Content="Email me about new features" IsChecked="{Binding Notifications}" />
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Classes="outline" Command="{Binding SaveCommand}" Content="Save" />
            <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Status}" />
          </StackPanel>
        </StackPanel>
        """, "IsChecked is bound two way: on the web the post binds it before the command runs."),
    table("API", [
        ("IsChecked", "Two-way bindable; null with IsThreeState is indeterminate."),
        ("Content", "The label beside the box (14 px medium)."),
        ("Command, CommandParameter", "Runs on each toggle (desktop) or with the post (web)."),
        ("Lightweight styling", "Fluent's CheckBox… brush keys, defined by Cosmo, recolor it."),
    ]),
], datatype="ChoiceExamples")

RESOURCES = """
  <UserControl.Resources>
    <m:Is x:Key="Is" />
  </UserControl.Resources>"""

page("radio-group", "Radio Group", "A set of checkable buttons—known as radio buttons—where no more than one of the buttons can be checked at a time.", ["shadcn/ui: radio-group", "Avalonia: RadioButton", "Web: <input type=radio>"], [
    example("Usage", """
        <StackPanel Spacing="12">
          <RadioButton Content="Default" />
          <RadioButton Content="Comfortable" IsChecked="True" />
          <RadioButton Content="Compact" />
        </StackPanel>
        """, "Radio buttons in one panel form a group (or share a GroupName); 12 px apart as shadcn's gap-3."),
    example("Disabled", """
        <StackPanel Spacing="12">
          <RadioButton Content="Enabled" IsChecked="True" />
          <RadioButton Content="Disabled" IsEnabled="False" />
        </StackPanel>
        """),
    example("Binding", """
        <StackPanel Spacing="12">
          <RadioButton Content="Default" IsChecked="{Binding Density, Converter={StaticResource Is}, ConverterParameter=Default}" />
          <RadioButton Content="Comfortable" IsChecked="{Binding Density, Converter={StaticResource Is}, ConverterParameter=Comfortable}" />
          <RadioButton Content="Compact" IsChecked="{Binding Density, Converter={StaticResource Is}, ConverterParameter=Compact}" />
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Classes="outline" Command="{Binding ApplyCommand}" Content="Apply" />
            <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Status}" />
          </StackPanel>
        </StackPanel>
        """, "Each IsChecked is bound two way through a converter comparing the model's enum with the radio's value."),
    table("API", [
        ("IsChecked", "Two-way bindable; one radio of a group is checked."),
        ("Content", "The label beside the circle (14 px medium)."),
        ("GroupName", "Groups radios across panels."),
    ]),
], datatype="ChoiceExamples", styles=RESOURCES)

page("switch", "Switch", "A control that allows the user to toggle between checked and not checked.", ["shadcn/ui: switch", "Avalonia: ToggleSwitch", "Web: <input role=switch>"], [
    example("Usage", """
        <StackPanel Orientation="Horizontal" Spacing="8">
          <ToggleSwitch Name="airplane" a:Web.Id="airplane" />
          <Label Content="Airplane Mode" Target="{Binding #airplane}" />
        </StackPanel>
        """, "As in shadcn, the switch and its Label sit side by side 8 px apart; a ToggleSwitch's own Content also goes to its right on the desktop."),
    example("States", """
        <StackPanel Spacing="16">
          <ToggleSwitch IsChecked="True" AutomationProperties.Name="On" />
          <ToggleSwitch IsEnabled="False" AutomationProperties.Name="Disabled" />
          <ToggleSwitch Classes="sm" IsChecked="True" AutomationProperties.Name="Small" />
        </StackPanel>
        """),
    example("Binding", """
        <StackPanel Spacing="12">
          <StackPanel Orientation="Horizontal" Spacing="8">
            <ToggleSwitch Name="mode" a:Web.Id="mode" IsChecked="{Binding AirplaneMode}" />
            <Label Content="Airplane Mode" Target="{Binding #mode}" />
          </StackPanel>
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Classes="outline" Command="{Binding SaveAirplaneCommand}" Content="Save" />
            <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Status}" />
          </StackPanel>
        </StackPanel>
        """),
    table("API", [
        ("IsChecked", "Two-way bindable."),
        ("sm", "The small switch: 24 by 14 px with a 12 px thumb."),
        ("Content", "Optional, 8 px to the right of the switch on the desktop."),
        ("Lightweight styling", "Fluent's ToggleSwitch… brush keys, defined by Cosmo, recolor it."),
    ]),
], datatype="ChoiceExamples")

page("toggle", "Toggle", "A two-state button that can be either on or off.", ["shadcn/ui: toggle", "Avalonia: ToggleButton", "Web: <label> with a checkbox"], [
    example("Usage", """
        <ToggleButton AutomationProperties.Name="Toggle bookmark"><a:Icon Kind="star" Size="16" /></ToggleButton>
        """),
    example("Outline", """
        <ToggleButton Classes="outline" AutomationProperties.Name="Toggle notifications"><a:Icon Kind="bell" Size="16" /></ToggleButton>
        """),
    example("With text", """
        <ToggleButton Classes="outline">
          <StackPanel>
            <a:Icon Kind="heart" Size="16" />
            <TextBlock Text="Favorite" VerticalAlignment="Center" />
          </StackPanel>
        </ToggleButton>
        """),
    example("Sizes", """
        <StackPanel Orientation="Horizontal" Spacing="8">
          <ToggleButton Classes="sm" AutomationProperties.Name="Small"><a:Icon Kind="heart" Size="16" /></ToggleButton>
          <ToggleButton AutomationProperties.Name="Default"><a:Icon Kind="heart" Size="16" /></ToggleButton>
          <ToggleButton Classes="lg" AutomationProperties.Name="Large"><a:Icon Kind="heart" Size="16" /></ToggleButton>
        </StackPanel>
        """),
    example("Disabled", """
        <ToggleButton IsEnabled="False" AutomationProperties.Name="Disabled"><a:Icon Kind="heart" Size="16" /></ToggleButton>
        """),
    example("Binding", """
        <StackPanel Orientation="Horizontal" Spacing="12">
          <ToggleButton IsChecked="{Binding Bold}" Content="B" AutomationProperties.Name="Bold" />
          <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Bold, StringFormat='Bold: {0}'}" />
        </StackPanel>
        """),
    table("API", [
        ("IsChecked", "Two-way bindable on state."),
        ("outline", "A border and shadow-xs; accent on hover."),
        ("sm, lg", "32 and 40 px (36 by default)."),
        ("Command, CommandParameter", "Runs on each toggle (desktop) or with the post (web)."),
    ]),
], datatype="ChoiceExamples")

page("toggle-group", "Toggle Group", "A set of two-state buttons that can be toggled on or off.", ["shadcn/ui: toggle-group", "Avalonia: ListBox (toggle-group class)", "Web: radios or checkboxes"], [
    example("Usage", """
        <ListBox Classes="toggle-group outline" SelectionMode="Multiple,Toggle">
          <ListBoxItem AutomationProperties.Name="Heart"><a:Icon Kind="heart" Size="16" /></ListBoxItem>
          <ListBoxItem AutomationProperties.Name="Star"><a:Icon Kind="star" Size="16" /></ListBoxItem>
          <ListBoxItem AutomationProperties.Name="Bell"><a:Icon Kind="bell" Size="16" /></ListBoxItem>
        </ListBox>
        """, "A ListBox with the toggle-group class: SelectionMode Multiple,Toggle is shadcn's type=multiple, Toggle (single, deselectable) its type=single."),
    example("Single", """
        <ListBox Classes="toggle-group" SelectionMode="Toggle" SelectedIndex="1">
          <ListBoxItem Content="Left" />
          <ListBoxItem Content="Center" />
          <ListBoxItem Content="Right" />
        </ListBox>
        """),
    example("Spacing and size", """
        <ListBox Classes="toggle-group outline sm spacing" SelectionMode="Toggle" SelectedIndex="0">
          <ListBoxItem Content="Day" />
          <ListBoxItem Content="Week" />
          <ListBoxItem Content="Month" />
        </ListBox>
        """),
    example("Binding and templates", """
        <StackPanel Spacing="12">
          <ListBox Classes="toggle-group outline" SelectionMode="Multiple,Toggle" ItemsSource="{Binding FormatChoices}" AutomationProperties.Name="Formats">
            <ListBox.ItemTemplate>
              <DataTemplate x:DataType="x:String">
                <TextBlock Text="{Binding}" />
              </DataTemplate>
            </ListBox.ItemTemplate>
          </ListBox>
          <ListBox Classes="toggle-group" SelectionMode="Toggle" ItemsSource="{Binding Alignments}" SelectedItem="{Binding Alignment}" AutomationProperties.Name="Alignment" />
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Classes="outline" Command="{Binding AlignCommand}" Content="Apply" />
            <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Status}" />
          </StackPanel>
        </StackPanel>
        """, "Items from the model with an ItemTemplate; the single group's SelectedItem is bound two way (a bound SelectedItems is not planned on the web yet)."),
    table("API", [
        ("toggle-group", "Gives the ListBox the toggle group's look."),
        ("SelectionMode", "Toggle (type=single, deselectable) or Multiple,Toggle (type=multiple)."),
        ("outline, sm, lg, spacing", "Bordered items, 32 or 40 px items, items 8 px apart each rounded."),
        ("SelectedItem, SelectedItems, ItemTemplate", "The ListBox's own selection and templates, bindable."),
    ]),
], datatype="ChoiceExamples")

page("slider", "Slider", "An input where the user selects a value from within a given range.", ["shadcn/ui: slider", "Avalonia: Slider", "Cosmo: c:RangeSlider", "Web: <input type=range>"], [
    example("Usage", """
        <Slider Value="50" Maximum="100" TickFrequency="1" IsSnapToTickEnabled="True" Width="320" AutomationProperties.Name="Volume" />
        """, "TickFrequency with IsSnapToTickEnabled is shadcn's step."),
    example("Range", """
        <c:RangeSlider LowerValue="25" UpperValue="75" Maximum="100" TickFrequency="1" IsSnapToTickEnabled="True" Width="320" AutomationProperties.Name="Range" />
        """, "c:RangeSlider: two thumbs as Radix's: a press moves the closest thumb, the values stay ordered, Shift and the page keys step by ten, Home and End move the first and the last thumb."),
    example("Vertical", """
        <Slider Orientation="Vertical" Value="40" Maximum="100" AutomationProperties.Name="Level" />
        """),
    example("Disabled", """
        <Slider Value="33" Maximum="100" IsEnabled="False" Width="320" AutomationProperties.Name="Locked" />
        """),
    example("Binding", """
        <StackPanel Spacing="12" Width="320">
          <Label Content="{Binding Volume, StringFormat='Volume: {0:0}'}" />
          <Slider Value="{Binding Volume}" Maximum="100" TickFrequency="1" IsSnapToTickEnabled="True" AutomationProperties.Name="Bound volume" />
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Classes="outline" Command="{Binding ApplyCommand}" Content="Apply" />
            <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Status}" />
          </StackPanel>
        </StackPanel>
        """, "Value is bound two way: the arrows, the pointer and touch move it on the desktop; the web posts the range input's value."),
    example("Range binding", """
        <StackPanel Spacing="12" Width="320">
          <Label Content="{Binding Low, StringFormat='Budget from ${0:0}'}" />
          <c:RangeSlider LowerValue="{Binding Low}" UpperValue="{Binding High}" Maximum="1000" TickFrequency="10" IsSnapToTickEnabled="True" AutomationProperties.Name="Budget" />
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Classes="outline" Command="{Binding ApplyRangeCommand}" Content="Apply budget" />
            <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Status}" />
          </StackPanel>
        </StackPanel>
        """, "LowerValue and UpperValue are bound two way; the web posts both thumbs' values, in order."),
    table("API", [
        ("Value, Minimum, Maximum", "Two-way bindable value in its range."),
        ("TickFrequency, IsSnapToTickEnabled", "The step."),
        ("Orientation", "Vertical: 6 px wide, at least 176 px high."),
        ("c:RangeSlider", "Two thumbs: LowerValue and UpperValue (two way, kept ordered), Minimum, Maximum, TickFrequency, IsSnapToTickEnabled, Orientation."),
    ]),
], datatype="RangeExamples")

page("progress", "Progress", "Displays an indicator showing the completion progress of a task, typically displayed as a progress bar.", ["shadcn/ui: progress", "Avalonia: ProgressBar", "Web: <progress>"], [
    example("Usage", """
        <ProgressBar Value="33" Width="320" />
        """),
    example("Binding", """
        <StackPanel Spacing="12" Width="320">
          <ProgressBar Value="{Binding Done}" AutomationProperties.Name="Upload" />
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Classes="outline" Command="{Binding AdvanceCommand}" Content="+10%" />
            <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Done, StringFormat='{}{0:0}% done'}" />
          </StackPanel>
        </StackPanel>
        """),
    table("API", [
        ("Value, Minimum, Maximum", "Bindable progress; 8 px high, primary on primary at 20%."),
        ("IsIndeterminate", "No indicator, as Radix's null value."),
    ]),
], datatype="RangeExamples")

page("input-group", "Input Group", "Display additional information or actions to an input or textarea.", ["shadcn/ui: input-group", "Cosmo: c:InputGroup, c:InputGroupAddon"], [
    example("Usage", """
        <c:InputGroup Width="384">
          <TextBox PlaceholderText="Search..." />
          <c:InputGroupAddon><a:Icon Kind="search" Size="16" /></c:InputGroupAddon>
          <c:InputGroupAddon Classes="inline-end" Content="12 results" />
        </c:InputGroup>
        """, "Addons go before the text box by default, after it with inline-end and below a textarea with block-end."),
    example("Text", """
        <c:InputGroup Width="384">
          <TextBox PlaceholderText="example.com" />
          <c:InputGroupAddon><TextBlock Text="https://" FontWeight="Normal" /></c:InputGroupAddon>
        </c:InputGroup>
        """),
    example("Textarea", """
        <c:InputGroup Width="384">
          <TextBox AcceptsReturn="True" PlaceholderText="Ask, Search or Chat..." />
          <c:InputGroupAddon Classes="block-end">
            <DockPanel>
              <Button DockPanel.Dock="Right" Classes="icon-xs" CornerRadius="9999" AutomationProperties.Name="Send"><a:Icon Kind="plus" Size="14" /></Button>
              <TextBlock Text="52% used" FontWeight="Normal" VerticalAlignment="Center" />
            </DockPanel>
          </c:InputGroupAddon>
        </c:InputGroup>
        """),
    example("Binding", """
        <c:InputGroup Width="384">
          <TextBox Text="{Binding Query}" MaxLength="24" PlaceholderText="Username" AutomationProperties.Name="Username" />
          <c:InputGroupAddon Classes="inline-end" Content="{Binding Count}" />
        </c:InputGroup>
        """),
    table("API", [
        ("c:InputGroup", "The bordered field (36 px, input border, shadow-xs; the ring while its text box has the focus)."),
        ("c:InputGroupAddon", "Muted 14 px medium content; inline-end and block-end classes place it."),
    ]),
], datatype="RangeExamples")

page("input-otp", "Input OTP", "Accessible one-time password component with copy paste functionality.", ["shadcn/ui: input-otp", "Cosmo: c:InputOtp"], [
    example("Usage", """
        <c:InputOtp Length="6" GroupSize="3" AutomationProperties.Name="Code" />
        """, "Length boxes, a separator every GroupSize; the characters are typed into a hidden text box that covers them."),
    example("Disabled", """
        <c:InputOtp Length="4" IsEnabled="False" AutomationProperties.Name="Locked code" />
        """),
    example("Binding", """
        <StackPanel Spacing="12">
          <c:InputOtp Length="6" Text="{Binding Code}" AutomationProperties.Name="Bound code" />
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Classes="outline" Command="{Binding VerifyCommand}" Content="Verify" />
            <TextBlock Classes="sm muted" VerticalAlignment="Center" Text="{Binding Status}" />
          </StackPanel>
        </StackPanel>
        """),
    table("API", [
        ("Text", "Two-way bindable code."),
        ("Length, GroupSize", "How many boxes, and how many per group (0: one group)."),
    ]),
], datatype="RangeExamples")

FIELD_ROWS = "\n".join(
    f"""            <Label Grid.Row="{i}" Content="{label}" VerticalAlignment="Center" />
            <TextBox Grid.Row="{i}" Grid.Column="1" Grid.ColumnSpan="2" Text="{value}" Height="32" MinHeight="32" Padding="12,4" AutomationProperties.Name="{label}" />"""
    for i, (label, value) in enumerate([("Width", "100%"), ("Max. width", "300px"), ("Height", "25px"), ("Max. height", "none")])
)

page("popover", "Popover", "Displays rich content in a portal, triggered by a button.", ["shadcn/ui: popover", "Avalonia: Flyout", "Web: popover API"], [
    example("Usage", f"""
        <Button Classes="outline" Content="Open popover">
          <Button.Flyout>
            <Flyout Placement="Bottom" FlyoutPresenterClasses="w-80">
              <StackPanel Spacing="16">
                <StackPanel Spacing="8">
                  <TextBlock Text="Dimensions" FontSize="16" LineHeight="16" FontWeight="Medium" />
                  <TextBlock Classes="sm muted" Text="Set the dimensions for the layer." TextWrapping="Wrap" />
                </StackPanel>
                <Grid ColumnDefinitions="*,*,*" RowDefinitions="Auto,Auto,Auto,Auto" ColumnSpacing="16" RowSpacing="8">
{FIELD_ROWS}
                </Grid>
              </StackPanel>
            </Flyout>
          </Button.Flyout>
        </Button>
        """, "Button.Flyout is the trigger; the presenter is shadcn's PopoverContent (288 px, w-72; the w-80 class is an app style). Return opens it with the focus on its first field, Escape and a click outside close it, and the focus goes back to the button."),
    example("Placement", """
        <StackPanel Orientation="Horizontal" Spacing="8">
          <Button Classes="outline" Content="Bottom start">
            <Button.Flyout><Flyout Placement="BottomEdgeAlignedLeft"><TextBlock Classes="sm" Text="Aligned with the trigger's left edge." TextWrapping="Wrap" /></Flyout></Button.Flyout>
          </Button>
          <Button Classes="outline" Content="Top">
            <Button.Flyout><Flyout Placement="Top" VerticalOffset="-4"><TextBlock Classes="sm" Text="Above the trigger, 4 px away." TextWrapping="Wrap" /></Flyout></Button.Flyout>
          </Button>
        </StackPanel>
        """, "Flyouts sit 4 px from their trigger (Radix's sideOffset): Cosmo sets VerticalOffset to 4 by default, so a flyout above its trigger takes -4."),
    example("Binding", """
        <StackPanel Spacing="12">
          <Button Classes="outline" Content="Edit layer">
            <Button.Flyout>
              <Flyout>
                <StackPanel Spacing="12">
                  <TextBox Text="{Binding Width}" AutomationProperties.Name="Layer width" />
                  <TextBox Text="{Binding Height}" AutomationProperties.Name="Layer height" />
                  <Button Content="Apply" Command="{Binding ApplyCommand}" CommandParameter="popover" HorizontalAlignment="Right" />
                </StackPanel>
              </Flyout>
            </Button.Flyout>
          </Button>
          <TextBlock Classes="sm muted" Text="{Binding Status}" />
        </StackPanel>
        """, "The content binds the page's model (compiled bindings) and runs its commands; c:Popover.IsOpen binds the open state two way on the desktop."),
    table("API", [
        ("Button.Flyout, Flyout", "The trigger and the content (FlyoutBase.AttachedFlyout for other controls)."),
        ("Placement", "Bottom (centered, Radix's default), BottomEdgeAlignedLeft (align start), Top…"),
        ("FlyoutPresenterClasses", "Classes for the presenter, styled by the app (w-80)."),
        ("c:Popover.IsOpen", "The open state, two way (Radix's open and onOpenChange)."),
    ]),
], styles="""
  <UserControl.Styles>
    <Style Selector="FlyoutPresenter.w-80">
      <Setter Property="Width" Value="320" />
    </Style>
  </UserControl.Styles>""", datatype="OverlayExamples")

page("tooltip", "Tooltip", "A popup that displays information related to an element when the element receives keyboard focus or the mouse hovers over it.", ["shadcn/ui: tooltip", "Avalonia: ToolTip", "Web: role=tooltip"], [
    example("Usage", """
        <Button Classes="outline" Content="Hover" ToolTip.Tip="Add to library" />
        """, "Above its trigger with shadcn's arrow, at once (shadcn's TooltipProvider has no delay); Tab onto the button shows it and Escape hides it."),
    example("Icon button", """
        <Button Classes="outline icon" AutomationProperties.Name="Add to library" ToolTip.Tip="Add to library">
          <a:Icon Kind="plus" Size="16" />
        </Button>
        """),
    example("Delay", """
        <StackPanel Orientation="Horizontal" Spacing="8">
          <Button Classes="outline" Content="Bold" ToolTip.Tip="Bold" ToolTip.ShowDelay="700" />
          <Button Classes="outline" Content="Italic" ToolTip.Tip="Italic" ToolTip.ShowDelay="700" />
          <Button Classes="outline" Content="Underline" ToolTip.Tip="Underline" ToolTip.ShowDelay="700" />
        </StackPanel>
        """, "ToolTip.ShowDelay is Radix's delayDuration (700 ms here); moving to the next button within ToolTip.BetweenShowDelay (300 ms, Radix's skipDelayDuration) shows its tip at once."),
    example("Binding", """
        <StackPanel Orientation="Horizontal" Spacing="12">
          <Button Classes="outline" Content="Inbox" ToolTip.Tip="{Binding Unread, StringFormat='{}{0} unread messages'}" />
          <Button Classes="ghost" Content="Mark one read" Command="{Binding ReadCommand}" />
        </StackPanel>
        """, "The tip binds the model through a string format."),
    table("API", [
        ("ToolTip.Tip", "The tip's content: text, a control, or a ToolTip with a ContentTemplate."),
        ("ToolTip.ShowDelay, BetweenShowDelay", "Radix's delayDuration (0 in Cosmo, as shadcn) and skipDelayDuration (300 ms)."),
        ("ToolTip.Placement", "Top in Cosmo (Radix's side); the arrow is drawn for the top side."),
    ]),
], datatype="OverlayExamples")

HOVER_CARD = """
              <Grid ColumnDefinitions="Auto,*" ColumnSpacing="16">
                <Border Width="32" Height="32" CornerRadius="16" Background="{DynamicResource CosmoMutedBrush}" VerticalAlignment="Top">
                  <TextBlock Classes="sm muted" Text="VC" HorizontalAlignment="Center" VerticalAlignment="Center" />
                </Border>
                <StackPanel Grid.Column="1" Spacing="4">
                  <TextBlock Classes="sm" Text="{Binding User.Handle}" FontWeight="SemiBold" />
                  <TextBlock Classes="sm" Text="{Binding User.About}" TextWrapping="Wrap" />
                  <TextBlock Classes="xs muted" Text="{Binding User.Joined, StringFormat='Joined {0}'}" />
                </StackPanel>
              </Grid>"""

page("hover-card", "Hover Card", "For sighted users to preview content available behind a link.", ["shadcn/ui: hover-card", "Cosmo: c:HoverCard"], [
    example("Usage", f"""
        <HyperlinkButton Classes="link" Content="@nextjs" NavigateUri="https://nextjs.org" c:HoverCard.Placement="Bottom">
          <c:HoverCard.Card>{HOVER_CARD}
          </c:HoverCard.Card>
        </HyperlinkButton>
        """, "The card opens after the pointer rests 700 ms on the link (or the link takes the focus with Tab), stays while the pointer is over the link or the card, and closes 300 ms after it left both."),
    example("Delays", """
        <Button Classes="outline" Content="Quick card" c:HoverCard.OpenDelay="0" c:HoverCard.CloseDelay="0">
          <c:HoverCard.Card>
            <TextBlock Classes="sm" Text="Opens at once, closes as the pointer leaves." TextWrapping="Wrap" />
          </c:HoverCard.Card>
        </Button>
        """),
    table("API", [
        ("c:HoverCard.Card", "The card's content, in the page's data context (compiled bindings)."),
        ("c:HoverCard.OpenDelay, CloseDelay", "Radix's openDelay (700 ms) and closeDelay (300 ms)."),
        ("c:HoverCard.Placement", "Bottom (centered) by default."),
        ("FlyoutPresenter.hover-card", "The card's surface (256 px, w-64), restylable by the app."),
    ]),
], datatype="OverlayExamples")

MENU_STYLES = """
  <UserControl.Styles>
    <Style Selector="MenuFlyoutPresenter.w-56">
      <Setter Property="Width" Value="224" />
    </Style>
    <Style Selector="ContextMenu.w-52">
      <Setter Property="Width" Value="208" />
    </Style>
  </UserControl.Styles>"""

page("dropdown-menu", "Dropdown Menu", "Displays a menu to the user—such as a set of actions or functions—triggered by a button.", ["shadcn/ui: dropdown-menu", "Avalonia: MenuFlyout", "Web: popover menu"], [
    example("Usage", """
        <Button Classes="outline" Content="Open">
          <Button.Flyout>
            <MenuFlyout Placement="BottomEdgeAlignedLeft" FlyoutPresenterClasses="w-56">
              <MenuItem Classes="label" Header="My Account" />
              <MenuItem Header="Profile" InputGesture="Shift+Ctrl+P" />
              <MenuItem Header="Billing" InputGesture="Ctrl+B" />
              <MenuItem Header="Settings" InputGesture="Ctrl+S" />
              <MenuItem Header="Keyboard shortcuts" InputGesture="Ctrl+K" />
              <Separator />
              <MenuItem Header="Team" />
              <MenuItem Header="Invite users">
                <MenuItem Header="Email" />
                <MenuItem Header="Message" />
                <Separator />
                <MenuItem Header="More..." />
              </MenuItem>
              <MenuItem Header="New Team" InputGesture="Ctrl+T" />
              <Separator />
              <MenuItem Header="GitHub" />
              <MenuItem Header="Support" />
              <MenuItem Header="API" IsEnabled="False" />
              <Separator />
              <MenuItem Header="Log out" InputGesture="Shift+Ctrl+Q" />
            </MenuFlyout>
          </Button.Flyout>
        </Button>
        """, "The label class is shadcn's DropdownMenuLabel, InputGesture its DropdownMenuShortcut (in the platform's notation), a MenuItem with items a submenu. Keyboard: Return, Space or Down open it on the first item, the arrows stop at the ends, a letter jumps to the next item starting with it, Right and Left open and close submenus, Escape closes it."),
    example("Checkboxes", """
        <Button Classes="outline" Content="Open">
          <Button.Flyout>
            <MenuFlyout FlyoutPresenterClasses="w-56">
              <MenuItem Classes="label" Header="Appearance" />
              <Separator />
              <MenuItem Header="Status Bar" ToggleType="CheckBox" IsChecked="True" />
              <MenuItem Header="Activity Bar" ToggleType="CheckBox" IsEnabled="False" />
              <MenuItem Header="Panel" ToggleType="CheckBox" />
            </MenuFlyout>
          </Button.Flyout>
        </Button>
        """),
    example("Radio group", """
        <Button Classes="outline" Content="Open">
          <Button.Flyout>
            <MenuFlyout FlyoutPresenterClasses="w-56">
              <MenuItem Classes="label" Header="Panel Position" />
              <Separator />
              <MenuItem Header="Top" ToggleType="Radio" GroupName="position" />
              <MenuItem Header="Bottom" ToggleType="Radio" GroupName="position" IsChecked="True" />
              <MenuItem Header="Right" ToggleType="Radio" GroupName="position" />
            </MenuFlyout>
          </Button.Flyout>
        </Button>
        """),
    example("Binding and commands", """
        <StackPanel Spacing="12">
          <Button Classes="outline" Content="Actions">
            <Button.Flyout>
              <MenuFlyout>
                <MenuItem Header="Duplicate" Command="{Binding ChooseCommand}" CommandParameter="Duplicate" InputGesture="Ctrl+D" />
                <MenuItem Header="Archive" Command="{Binding ChooseCommand}" CommandParameter="Archive" />
                <Separator />
                <MenuItem Classes="destructive" Header="Delete" Command="{Binding DeleteCommand}" />
              </MenuFlyout>
            </Button.Flyout>
          </Button>
          <TextBlock Classes="sm muted" Text="{Binding Choice}" />
        </StackPanel>
        """, "Items run the model's commands with their parameters; Delete's CanExecute is false, so it is disabled. The destructive class is shadcn's variant=destructive."),
    table("API", [
        ("Button.Flyout, MenuFlyout", "The trigger and the menu; ItemsSource and ItemTemplate list items from the model."),
        ("MenuItem", "Header, Icon, InputGesture (shortcut), Command and CommandParameter, items for a submenu."),
        ("ToggleType, IsChecked, GroupName", "Checkbox and radio items (bind IsChecked with Mode=TwoWay)."),
        ("label, inset, destructive", "MenuItem classes: shadcn's DropdownMenuLabel, inset and variant=destructive."),
        ("Separator", "shadcn's DropdownMenuSeparator."),
    ]),
], styles=MENU_STYLES, datatype="OverlayExamples")

page("context-menu", "Context Menu", "Displays a menu to the user—such as a set of actions or functions—triggered by a right click.", ["shadcn/ui: context-menu", "Avalonia: ContextMenu"], [
    example("Usage", """
        <Border Width="300" Height="150" CornerRadius="8" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" Background="Transparent">
          <Border.ContextMenu>
            <ContextMenu Classes="w-52">
              <MenuItem Classes="inset" Header="Back" InputGesture="Ctrl+OemOpenBrackets" />
              <MenuItem Classes="inset" Header="Forward" InputGesture="Ctrl+OemCloseBrackets" IsEnabled="False" />
              <MenuItem Classes="inset" Header="Reload" InputGesture="Ctrl+R" />
              <MenuItem Classes="inset" Header="More Tools">
                <MenuItem Header="Save Page..." />
                <MenuItem Header="Create Shortcut..." />
                <MenuItem Header="Name Window..." />
                <Separator />
                <MenuItem Header="Developer Tools" />
                <Separator />
                <MenuItem Classes="destructive" Header="Delete" />
              </MenuItem>
              <Separator />
              <MenuItem Header="Show Bookmarks" ToggleType="CheckBox" IsChecked="True" />
              <MenuItem Header="Show Full URLs" ToggleType="CheckBox" />
              <Separator />
              <MenuItem Classes="label inset" Header="People" />
              <MenuItem Header="Pedro Duarte" ToggleType="Radio" GroupName="people" IsChecked="True" />
              <MenuItem Header="Colm Tuite" ToggleType="Radio" GroupName="people" />
            </ContextMenu>
          </Border.ContextMenu>
          <TextBlock Classes="sm" Text="Right click here" HorizontalAlignment="Center" VerticalAlignment="Center" />
        </Border>
        """, "A right click (or Shift+F10, or the Apps key) opens the menu at the pointer."),
    example("Binding and commands", """
        <StackPanel Spacing="12">
          <Border Width="300" Height="100" CornerRadius="8" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" Background="Transparent">
            <Border.ContextMenu>
              <ContextMenu>
                <MenuItem Header="Copy" Command="{Binding ChooseCommand}" CommandParameter="Copy" />
                <MenuItem Header="Paste" Command="{Binding ChooseCommand}" CommandParameter="Paste" />
              </ContextMenu>
            </Border.ContextMenu>
            <TextBlock Classes="sm muted" Text="Right click for actions" HorizontalAlignment="Center" VerticalAlignment="Center" />
          </Border>
          <TextBlock Classes="sm muted" Text="{Binding Choice}" />
        </StackPanel>
        """),
    table("API", [
        ("Control.ContextMenu, ContextMenu", "The menu of any control, with the dropdown menu's items, classes and separators."),
        ("HorizontalOffset", "2 px right of the pointer, as Radix's."),
    ]),
], styles=MENU_STYLES, datatype="OverlayExamples")

PROFILE_FIELDS = """
              <StackPanel Spacing="16">
                <StackPanel Spacing="12">
                  <Label Content="Name" />
                  <TextBox Text="{Binding Name}" AutomationProperties.Name="Name" />
                </StackPanel>
                <StackPanel Spacing="12">
                  <Label Content="Username" />
                  <TextBox Text="{Binding Username}" AutomationProperties.Name="Username" />
                </StackPanel>
              </StackPanel>"""

def opened(xaml, text):
    return example("Opened in the browser", xaml, text, wrap=True)


TRIGGER = ("Trigger", "The button that opens it; on the web the browser opens it with no request.", True)

page("dialog", "Dialog", "A window overlaid on either the primary window or another dialog window, rendering the content underneath inert.", ["shadcn/ui: dialog", "Cosmo: c:Dialog", "Web: <dialog> (showModal)"], [
    example("Usage", f"""
        <StackPanel Spacing="12">
          <Button Classes="outline" Content="Open Dialog" Command="{{Binding EditCommand}}" />
          <c:Dialog IsOpen="{{Binding EditOpen}}" MaxWidth="425">
            <StackPanel Spacing="16">
              <c:DialogHeader Title="Edit profile" Description="Make changes to your profile here. Click save when you're done." />{PROFILE_FIELDS}
              <c:DialogFooter>
                <Button Classes="outline" Content="Cancel" Command="{{Binding AnswerCommand}}" CommandParameter="cancel" />
                <Button Content="Save changes" Command="{{Binding AnswerCommand}}" CommandParameter="save" />
              </c:DialogFooter>
            </StackPanel>
          </c:Dialog>
          <TextBlock Classes="sm muted" Text="{{Binding Outcome}}" />
        </StackPanel>
        """, "The trigger's command opens the dialog (IsOpen, two way) and awaits its answer; the footer's commands answer and close it. The focus moves to the first field and stays in the dialog; Escape, the close button and a click outside close it. On the web the open dialog is a modal <dialog> written by Aioros for the pending command."),
    example("Without a close button", """
        <StackPanel Spacing="12">
          <Button Classes="outline" Content="Share" Command="{Binding ShareCommand}" />
          <c:Dialog IsOpen="{Binding ShareOpen}" ShowCloseButton="False" MaxWidth="448">
            <StackPanel Spacing="16">
              <c:DialogHeader Title="Share link" Description="Anyone who has this link will be able to view this." />
              <TextBox Text="https://ui.shadcn.com/docs/installation" IsReadOnly="True" AutomationProperties.Name="Link" />
              <c:DialogFooter HorizontalAlignment="Left">
                <Button Classes="secondary" Content="Close" c:Dialog.Closes="True" />
              </c:DialogFooter>
            </StackPanel>
          </c:Dialog>
        </StackPanel>
        """, "ShowCloseButton is shadcn's showCloseButton; c:Dialog.Closes on a button is its DialogClose."),
    opened("""
        <StackPanel Spacing="12">
          <Button x:Name="aboutTrigger" Classes="outline" Content="About" />
          <c:Dialog Trigger="{Binding #aboutTrigger}" MaxWidth="425">
            <StackPanel Spacing="16">
              <c:DialogHeader Title="About Cosmo" Description="shadcn/ui's components in Avalonia XAML, on the desktop and on the web." />
              <c:DialogFooter>
                <Button Classes="outline" Content="Done" c:Dialog.Closes="True" />
              </c:DialogFooter>
            </StackPanel>
          </c:Dialog>
        </StackPanel>
        """, "Trigger is Radix's DialogTrigger: on the web the browser opens the dialog with no request, and Escape, a click outside, the close button and c:Dialog.Closes close it there; IsOpen, unbound here, stays in the browser."),
    table("API", [
        TRIGGER,
        ("IsOpen", "Two-way bindable open state (Radix's open and onOpenChange)."),
        ("Content, ContentTemplate", "What the dialog shows, on the page's model."),
        ("c:DialogHeader Title, Description", "shadcn's DialogHeader, DialogTitle and DialogDescription."),
        ("c:DialogFooter", "The buttons in a row at the end."),
        ("ShowCloseButton, IsLightDismissEnabled", "The close button, and whether a click outside closes it."),
        ("c:Dialog.Closes", "A button that closes the dialog it is in (DialogClose)."),
        ("c:OverlayHost.IsHost", "The panel dialogs cover (the window's root in the Gallery)."),
    ]),
], datatype="OverlayExamples")

page("alert-dialog", "Alert Dialog", "A modal dialog that interrupts the user with important content and expects a response.", ["shadcn/ui: alert-dialog", "Cosmo: c:AlertDialog", "Web: <dialog> (showModal)"], [
    example("Usage", """
        <StackPanel Spacing="12">
          <Button Classes="outline" Content="Show Dialog" Command="{Binding ConfirmCommand}" />
          <c:AlertDialog IsOpen="{Binding ConfirmOpen}">
            <StackPanel Spacing="16">
              <c:DialogHeader Title="Are you absolutely sure?" Description="This action cannot be undone. This will permanently delete your account and remove your data from our servers." />
              <c:DialogFooter>
                <Button Classes="outline" Content="Cancel" IsCancel="True" Command="{Binding AnswerCommand}" CommandParameter="cancel" />
                <Button Content="Continue" Command="{Binding AnswerCommand}" CommandParameter="continue" />
              </c:DialogFooter>
            </StackPanel>
          </c:AlertDialog>
          <TextBlock Classes="sm muted" Text="{Binding Outcome}" />
        </StackPanel>
        """, "No close button and no dismissal by a click outside: the focus starts on the cancel button (IsCancel), and Escape cancels."),
    example("Small", """
        <StackPanel Spacing="12">
          <Button Classes="outline" Content="Show small dialog" Command="{Binding AllowCommand}" />
          <c:AlertDialog Classes="sm" IsOpen="{Binding AllowOpen}">
            <StackPanel Spacing="16">
              <c:DialogHeader Title="Allow accessory to connect?" Description="Do you want to allow the USB accessory to connect to this device?" />
              <c:DialogFooter>
                <Button Classes="outline" Content="Don't allow" IsCancel="True" Command="{Binding AnswerCommand}" CommandParameter="denied" />
                <Button Content="Allow" Command="{Binding AnswerCommand}" CommandParameter="allowed" />
              </c:DialogFooter>
            </StackPanel>
          </c:AlertDialog>
        </StackPanel>
        """, "The sm class is shadcn's small size (320 px)."),
    opened("""
        <StackPanel Spacing="12">
          <Button x:Name="discardTrigger" Classes="outline" Content="Discard draft" />
          <c:AlertDialog Trigger="{Binding #discardTrigger}">
            <StackPanel Spacing="16">
              <c:DialogHeader Title="Discard the draft?" Description="The draft is gone once you discard it." />
              <c:DialogFooter>
                <Button Classes="outline" Content="Keep" c:Dialog.Closes="True" />
                <Button Content="Discard" c:Dialog.Closes="True" />
              </c:DialogFooter>
            </StackPanel>
          </c:AlertDialog>
        </StackPanel>
        """, "Trigger is Radix's AlertDialogTrigger: on the web the browser opens the alert dialog with no request, and Escape and c:Dialog.Closes close it there; a click outside does not."),
    table("API", [
        TRIGGER,
        ("IsOpen, Content", "As the dialog's."),
        ("Button.IsCancel", "The cancel answer, focused when the dialog opens."),
        ("sm", "The small alert dialog (320 px)."),
    ]),
], datatype="OverlayExamples")

page("sheet", "Sheet", "Extends the Dialog component to display content that complements the main content of the screen.", ["shadcn/ui: sheet", "Cosmo: c:Sheet", "Web: <dialog> (showModal)"], [
    example("Usage", f"""
        <StackPanel Spacing="12">
          <Button Classes="outline" Content="Open" Command="{{Binding SheetCommand}}" CommandParameter="Right" />
          <c:Sheet IsOpen="{{Binding SheetOpen}}" Side="{{Binding Side}}">
            <Grid RowDefinitions="Auto,*,Auto" RowSpacing="16">
              <c:DialogHeader Title="Edit profile" Description="Make changes to your profile here. Click save when you're done." />
              <StackPanel Grid.Row="1" Spacing="24" Margin="16,0">
                <StackPanel Spacing="12">
                  <Label Content="Name" />
                  <TextBox Text="{{Binding Name}}" AutomationProperties.Name="Name" />
                </StackPanel>
                <StackPanel Spacing="12">
                  <Label Content="Username" />
                  <TextBox Text="{{Binding Username}}" AutomationProperties.Name="Username" />
                </StackPanel>
              </StackPanel>
              <c:DialogFooter Grid.Row="2">
                <Button Content="Save changes" Command="{{Binding AnswerCommand}}" CommandParameter="save" />
                <Button Classes="outline" Content="Close" Command="{{Binding AnswerCommand}}" CommandParameter="close" />
              </c:DialogFooter>
            </Grid>
          </c:Sheet>
          <TextBlock Classes="sm muted" Text="{{Binding Outcome}}" />
        </StackPanel>
        """, "A side sheet is three quarters of the window, 384 px at most; its header and footer are inset 16 px."),
    example("Side", """
        <WrapPanel ItemSpacing="8" LineSpacing="8">
          <Button Classes="outline" Content="top" Command="{Binding SheetCommand}" CommandParameter="Top" />
          <Button Classes="outline" Content="right" Command="{Binding SheetCommand}" CommandParameter="Right" />
          <Button Classes="outline" Content="bottom" Command="{Binding SheetCommand}" CommandParameter="Bottom" />
          <Button Classes="outline" Content="left" Command="{Binding SheetCommand}" CommandParameter="Left" />
        </WrapPanel>
        """, "The buttons open the Usage sheet at each side (Side is bound to the model)."),
    opened("""
        <StackPanel Spacing="12">
          <Button x:Name="settingsTrigger" Classes="outline" Content="Settings" />
          <c:Sheet Trigger="{Binding #settingsTrigger}" Side="Right">
            <StackPanel Spacing="16" Margin="16">
              <c:DialogHeader Title="Settings" Description="Opened by the browser, closed by Escape, a click outside or the close button." />
              <Button Classes="outline" Content="Done" c:Dialog.Closes="True" HorizontalAlignment="Left" />
            </StackPanel>
          </c:Sheet>
        </StackPanel>
        """, "Trigger is Radix's DialogTrigger: on the web the browser opens the sheet with no request and closes it there; IsOpen, unbound here, stays in the browser."),
    table("API", [
        TRIGGER,
        ("IsOpen, Content", "As the dialog's."),
        ("Side", "Right (default), Left, Top, Bottom."),
    ]),
], datatype="OverlayExamples")

page("drawer", "Drawer", "A panel that slides up from the bottom of the screen.", ["shadcn/ui: drawer", "Cosmo: c:Drawer", "Web: <dialog> (showModal)"], [
    example("Usage", """
        <StackPanel Spacing="12">
          <Button Classes="outline" Content="Open Drawer" Command="{Binding DrawerCommand}" />
          <c:Drawer IsOpen="{Binding DrawerOpen}">
            <StackPanel MaxWidth="384">
              <c:DialogHeader Title="Move Goal" Description="Set your daily activity goal." />
              <Grid ColumnDefinitions="Auto,*,Auto" ColumnSpacing="8" Margin="16,16,16,0">
                <Button Classes="outline icon" Width="32" Height="32" CornerRadius="16" VerticalAlignment="Center" AutomationProperties.Name="Decrease" Command="{Binding AdjustCommand}" CommandParameter="-10">
                  <a:Icon Kind="minus" Size="16" />
                </Button>
                <StackPanel Grid.Column="1">
                  <TextBlock Text="{Binding Goal}" FontSize="72" LineHeight="72" FontWeight="Bold" LetterSpacing="-3.6" HorizontalAlignment="Center" />
                  <TextBlock Classes="muted" Text="CALORIES/DAY" FontSize="11.2" HorizontalAlignment="Center" />
                </StackPanel>
                <Button Grid.Column="2" Classes="outline icon" Width="32" Height="32" CornerRadius="16" VerticalAlignment="Center" AutomationProperties.Name="Increase" Command="{Binding AdjustCommand}" CommandParameter="10">
                  <a:Icon Kind="plus" Size="16" />
                </Button>
              </Grid>
              <c:DialogFooter>
                <Button Content="Submit" Command="{Binding AnswerCommand}" CommandParameter="submit" />
                <Button Classes="outline" Content="Cancel" c:Dialog.Closes="True" />
              </c:DialogFooter>
            </StackPanel>
          </c:Drawer>
          <TextBlock Classes="sm muted" Text="{Binding Outcome}" />
        </StackPanel>
        """, "vaul's bottom drawer: rounded top corners, the handle, 80% of the window at most; Escape, a click outside and a drag down past a quarter of its height (or a flick) close it, as vaul's. The goal's buttons run commands while the drawer stays open."),
    opened("""
        <StackPanel Spacing="12">
          <Button x:Name="shortcutsTrigger" Classes="outline" Content="Shortcuts" />
          <c:Drawer Trigger="{Binding #shortcutsTrigger}">
            <StackPanel Spacing="16" Margin="16">
              <c:DialogHeader Title="Shortcuts" Description="Ctrl+K opens the palette and Ctrl+B folds the sidebar." />
              <Button Classes="outline" Content="Done" c:Dialog.Closes="True" HorizontalAlignment="Left" />
            </StackPanel>
          </c:Drawer>
        </StackPanel>
        """, "Trigger is vaul's DrawerTrigger: on the web the browser opens the drawer with no request and closes it there; IsOpen, unbound here, stays in the browser."),
    table("API", [
        TRIGGER,
        ("IsOpen, Content", "As the dialog's."),
        ("c:Dialog.Closes", "A button that closes the drawer (DrawerClose)."),
    ]),
], datatype="OverlayExamples")

page("sonner", "Sonner", "An opinionated toast component.", ["shadcn/ui: sonner", "Cosmo: c:Toaster, c:Toast", "Web: notices (popover)"], [
    example("Usage", """
        <StackPanel Spacing="12">
          <Button Classes="outline" Content="Show Toast" Command="{Binding ToastCommand}" />
          <TextBlock Classes="sm muted" Text="{Binding Undone}" />
        </StackPanel>
        """, "The command adds a c:Toast to the model's collection, which the page's c:Toaster shows at the window's bottom right corner for 4 s (paused under the pointer); its Undo action runs a command. Drag a toast down or right to swipe it away, as sonner's."),
    example("Types", """
        <WrapPanel ItemSpacing="8" LineSpacing="8">
          <Button Classes="outline" Content="Default" Command="{Binding KindCommand}" CommandParameter="Default" />
          <Button Classes="outline" Content="Success" Command="{Binding KindCommand}" CommandParameter="Success" />
          <Button Classes="outline" Content="Info" Command="{Binding KindCommand}" CommandParameter="Info" />
          <Button Classes="outline" Content="Warning" Command="{Binding KindCommand}" CommandParameter="Warning" />
          <Button Classes="outline" Content="Error" Command="{Binding KindCommand}" CommandParameter="Error" />
          <Button Classes="outline" Content="Promise" Command="{Binding PromiseCommand}" />
        </WrapPanel>
        """, "Kind picks sonner's icon; a loading toast stays until Resolve turns it into its result (sonner's toast.promise)."),
    example("Stack", """
        <Button Classes="outline" Content="Show three toasts" Command="{Binding StackCommand}" />
        """, "The newest toast in front, two more peeking 14 px behind it; under the pointer the stack expands."),
    example("Close button", """
        <Button Classes="outline" Content="Show with close button" Command="{Binding ClosableCommand}" />
        """, "A toast with CloseButton (or every toast of a toaster with CloseButton) shows sonner's close button at its corner; in the browser it closes the toast with no request."),
    table("API", [
        ("c:Toaster Toasts", "The model's collection of c:Toast; declare the toaster once (here at the end of the page)."),
        ("Position, Duration, IsExpanded, CloseButton", "The corner (bottom right), how long a toast stays (4 s), sonner's expand, sonner's close button on every toast."),
        ("c:Toast", "Title, Description, Kind, ActionText, ActionCommand, ActionParameter, Duration, CloseButton; Resolve for promises."),
    ]),
], datatype="OverlayExamples", extra="""    <c:Toaster Toasts="{Binding Toasts}" />
""")

page("separator", "Separator", "Visually or semantically separates content.", ["shadcn/ui: separator", "Avalonia: Separator", "Web: <hr>"], [
    example("Usage", """
        <StackPanel Width="320">
          <StackPanel Spacing="4">
            <TextBlock Text="Radix Primitives" FontSize="14" LineHeight="14" FontWeight="Medium" />
            <TextBlock Classes="sm muted" Text="An open-source UI component library." />
          </StackPanel>
          <Separator Margin="0,16" />
          <StackPanel Orientation="Horizontal" Height="20" Spacing="16">
            <TextBlock Classes="sm" Text="Blog" VerticalAlignment="Center" />
            <Separator Classes="vertical" />
            <TextBlock Classes="sm" Text="Docs" VerticalAlignment="Center" />
            <Separator Classes="vertical" />
            <TextBlock Classes="sm" Text="Source" VerticalAlignment="Center" />
          </StackPanel>
        </StackPanel>
        """, "A 1 px line in --border; the vertical class turns it into a 1 px column as high as its row."),
    example("Binding", """
        <StackPanel Spacing="12" Width="320">
          <CheckBox Content="Show the separator" IsChecked="{Binding ShowSeparator}" />
          <Separator IsVisible="{Binding ShowSeparator}" />
          <TextBlock Classes="sm muted" Text="{Binding ShowSeparator, StringFormat='Separator shown: {0}'}" />
        </StackPanel>
        """, "IsVisible and the vertical class bind as any Avalonia property; overriding CosmoBorderBrush restyles every separator."),
    table("API", [
        ("Classes", "vertical: a 1 px column as high as its row."),
        ("Background", "--border by default (CosmoBorderBrush)."),
    ]),
], datatype="DisplayExamples")

page("skeleton", "Skeleton", "Use to show a placeholder while content is loading.", ["shadcn/ui: skeleton", "Cosmo: c:Skeleton", "Web: <div>"], [
    example("Usage", """
        <StackPanel Orientation="Horizontal" Spacing="16">
          <c:Skeleton Width="48" Height="48" CornerRadius="9999" VerticalAlignment="Center" />
          <StackPanel Spacing="8" VerticalAlignment="Center">
            <c:Skeleton Width="250" Height="16" />
            <c:Skeleton Width="200" Height="16" />
          </StackPanel>
        </StackPanel>
        """, "A box in --accent that pulses (opacity 1, 0.5, 1 over 2 s); size it with Width and Height, round it with CornerRadius."),
    example("Card", """
        <StackPanel Spacing="12">
          <c:Skeleton Width="250" Height="125" CornerRadius="{DynamicResource CosmoRadiusXl}" />
          <StackPanel Spacing="8">
            <c:Skeleton Width="250" Height="16" />
            <c:Skeleton Width="200" Height="16" />
          </StackPanel>
        </StackPanel>
        """),
    example("Binding", """
        <StackPanel Spacing="12" Width="320">
          <Button Classes="outline" Content="Toggle loading" Command="{Binding LoadCommand}" HorizontalAlignment="Left" />
          <StackPanel Spacing="8" IsVisible="{Binding Loading}">
            <c:Skeleton Width="250" Height="16" />
            <c:Skeleton Width="200" Height="16" />
          </StackPanel>
          <ItemsControl ItemsSource="{Binding Projects}" IsVisible="{Binding !Loading}">
            <ItemsControl.ItemTemplate>
              <DataTemplate x:DataType="m:Project">
                <TextBlock Classes="sm" Text="{Binding Name}" />
              </DataTemplate>
            </ItemsControl.ItemTemplate>
          </ItemsControl>
        </StackPanel>
        """, "Skeletons stand in while the model loads; the items show through their template once it is done."),
    table("API", [
        ("Width, Height, CornerRadius", "The placeholder's size and corners (--radius-md by default)."),
        ("Background", "--accent by default."),
    ]),
], datatype="DisplayExamples")

page("spinner", "Spinner", "An indicator that can be used to show a loading state.", ["shadcn/ui: spinner", "Cosmo: c:Spinner", "Web: <svg>"], [
    example("Usage", """
        <StackPanel Orientation="Horizontal" Spacing="24">
          <c:Spinner Width="12" Height="12" VerticalAlignment="Center" />
          <c:Spinner VerticalAlignment="Center" />
          <c:Spinner Width="24" Height="24" VerticalAlignment="Center" />
          <c:Spinner Width="32" Height="32" VerticalAlignment="Center" />
        </StackPanel>
        """, "Lucide's loader-circle turning once a second in its parent's text color, named Loading for assistive technologies."),
    example("Button", """
        <StackPanel Orientation="Horizontal" Spacing="16">
          <Button Classes="sm has-icon" IsEnabled="False">
            <StackPanel><c:Spinner /><TextBlock Text="Loading..." /></StackPanel>
          </Button>
          <Button Classes="outline sm has-icon" IsEnabled="False">
            <StackPanel><c:Spinner /><TextBlock Text="Please wait" /></StackPanel>
          </Button>
        </StackPanel>
        """),
    example("Badge", """
        <StackPanel Orientation="Horizontal" Spacing="16">
          <c:Badge><StackPanel><c:Spinner /><TextBlock Text="Syncing" /></StackPanel></c:Badge>
          <c:Badge Classes="secondary"><StackPanel><c:Spinner /><TextBlock Text="Updating" /></StackPanel></c:Badge>
          <c:Badge Classes="outline"><StackPanel><c:Spinner /><TextBlock Text="Processing" /></StackPanel></c:Badge>
        </StackPanel>
        """, "In a badge the spinner is 12 px."),
    example("Binding", """
        <StackPanel Orientation="Horizontal" Spacing="12">
          <Button Classes="outline has-icon" Command="{Binding LoadCommand}">
            <StackPanel>
              <c:Spinner IsVisible="{Binding Loading}" />
              <TextBlock Text="{Binding Loading, Converter={x:Static m:DisplayExamples.LoadingText}}" />
            </StackPanel>
          </Button>
        </StackPanel>
        """, "The spinner follows the model's state; the label comes through a converter."),
    table("API", [
        ("Width, Height", "16 px by default."),
        ("Foreground", "Inherited from its parent (the button's or badge's text)."),
    ]),
], datatype="DisplayExamples")

page("kbd", "Kbd", "Used to display textual user input from keyboard.", ["shadcn/ui: kbd", "Cosmo: c:Kbd, c:KbdGroup", "Web: <div>"], [
    example("Usage", """
        <StackPanel Spacing="16">
          <c:KbdGroup>
            <c:Kbd Content="⌘" />
            <c:Kbd Content="⇧" />
            <c:Kbd Content="⌥" />
            <c:Kbd Content="⌃" />
          </c:KbdGroup>
          <c:KbdGroup>
            <c:Kbd Content="Ctrl" />
            <TextBlock Text="+" />
            <c:Kbd Content="B" />
          </c:KbdGroup>
        </StackPanel>
        """, "A key is 20 px high on --muted; a group puts its keys 4 px apart."),
    example("Button", """
        <StackPanel Orientation="Horizontal" Spacing="16">
          <Button Classes="outline sm" Padding="12,0,8,0">
            <StackPanel><TextBlock Text="Accept" /><c:Kbd Content="⏎" /></StackPanel>
          </Button>
          <Button Classes="outline sm" Padding="12,0,8,0">
            <StackPanel><TextBlock Text="Cancel" /><c:Kbd Content="Esc" /></StackPanel>
          </Button>
        </StackPanel>
        """),
    example("Tooltip", """
        <Button Classes="outline" Content="Save">
          <ToolTip.Tip>
            <StackPanel Orientation="Horizontal" Spacing="3">
              <TextBlock Text="Save Changes" VerticalAlignment="Center" />
              <c:Kbd Content="S" />
            </StackPanel>
          </ToolTip.Tip>
        </Button>
        """),
    example("Binding", """
        <StackPanel Spacing="12">
          <StackPanel Orientation="Horizontal" Spacing="12">
            <CheckBox Content="Use Mac keys" IsChecked="{Binding UseMacKeys}" />
            <Button Classes="outline sm" Content="Apply" Command="{Binding ApplyKeysCommand}" />
          </StackPanel>
          <c:KbdGroup>
            <c:Kbd><TextBlock Text="{Binding UseMacKeys, Converter={x:Static m:DisplayExamples.MacKey}}" /></c:Kbd>
            <c:Kbd Content="K" />
          </c:KbdGroup>
          <TextBlock Classes="sm muted" Text="{Binding KeysStatus}" />
        </StackPanel>
        """, "The key's Content binds through a converter; post the toggle to switch."),
    table("API", [
        ("c:Kbd Content", "The key; text or any content."),
        ("c:KbdGroup", "A StackPanel of keys 4 px apart."),
    ]),
], datatype="DisplayExamples")

page("avatar", "Avatar", "An image element with a fallback for representing the user.", ["shadcn/ui: avatar", "Cosmo: c:Avatar, c:AvatarBadge, c:AvatarGroup, c:AvatarGroupCount", "Web: <img>"], [
    example("Usage", """
        <StackPanel Orientation="Horizontal" Spacing="48">
          <c:Avatar Source="/Assets/avatar-shadcn.png" Content="CN" AutomationProperties.Name="@shadcn" />
          <c:Avatar Source="/Assets/avatar-evilrabbit.png" Content="ER" CornerRadius="{DynamicResource CosmoRadiusLg}" AutomationProperties.Name="@evilrabbit" />
          <c:AvatarGroup>
            <c:Avatar Source="/Assets/avatar-shadcn.png" Content="CN" />
            <c:Avatar Source="/Assets/avatar-maxleiter.png" Content="LR" />
            <c:Avatar Source="/Assets/avatar-evilrabbit.png" Content="ER" />
          </c:AvatarGroup>
        </StackPanel>
        """, "The image fills a 32 px circle; a group overlaps its avatars by 8 px, each ringed with --background."),
    example("Fallback and sizes", """
        <StackPanel Orientation="Horizontal" Spacing="48">
          <c:Avatar Classes="sm" Content="CN" />
          <c:Avatar Content="CN" />
          <c:Avatar Classes="lg" Content="CN" />
        </StackPanel>
        """, "Without an image the fallback shows on --muted; sm is 24 px, lg 40 px."),
    example("Badge", """
        <StackPanel Orientation="Horizontal" Spacing="48">
          <c:Avatar Classes="sm" Source="/Assets/avatar-shadcn.png" Content="CN" AutomationProperties.Name="@shadcn">
            <c:Avatar.Badge><c:AvatarBadge /></c:Avatar.Badge>
          </c:Avatar>
          <c:Avatar Source="/Assets/avatar-shadcn.png" Content="CN" AutomationProperties.Name="@shadcn">
            <c:Avatar.Badge><c:AvatarBadge Background="#16A34A" /></c:Avatar.Badge>
          </c:Avatar>
          <c:Avatar Classes="lg" Content="ER">
            <c:Avatar.Badge>
              <c:AvatarBadge><a:Icon Kind="plus" Size="8" /></c:AvatarBadge>
            </c:Avatar.Badge>
          </c:Avatar>
        </StackPanel>
        """, "A dot in the avatar's bottom right corner, inside its circle: 8, 10 or 12 px with the avatar's size, its icon hidden in a sm avatar; Background customizes it."),
    example("Group count", """
        <StackPanel Spacing="24">
          <c:AvatarGroup>
            <c:Avatar Source="/Assets/avatar-shadcn.png" Content="CN" />
            <c:Avatar Source="/Assets/avatar-maxleiter.png" Content="LR" />
            <c:Avatar Source="/Assets/avatar-evilrabbit.png" Content="ER" />
            <c:AvatarGroupCount Content="+3" />
          </c:AvatarGroup>
          <c:AvatarGroup>
            <c:Avatar Classes="lg" Content="CN" />
            <c:Avatar Classes="lg" Content="LR" />
            <c:AvatarGroupCount><a:Icon Kind="plus" Size="20" /></c:AvatarGroupCount>
          </c:AvatarGroup>
        </StackPanel>
        """, "The count closes the group as large as its avatars (24, 32 or 40 px), overlapping the last one by 8 px."),
    example("Binding", """
        <StackPanel Spacing="12">
          <c:AvatarGroup ItemsSource="{Binding Members}">
            <c:AvatarGroup.ItemTemplate>
              <DataTemplate x:DataType="m:Member">
                <c:Avatar><TextBlock Text="{Binding Initials}" /></c:Avatar>
              </DataTemplate>
            </c:AvatarGroup.ItemTemplate>
          </c:AvatarGroup>
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Classes="outline sm" Content="Add member" Command="{Binding AddMemberCommand}" />
            <TextBlock Classes="sm muted" Text="{Binding MemberCount}" VerticalAlignment="Center" />
          </StackPanel>
        </StackPanel>
        """, "Members come from the model through an item template, each avatar's content (the fallback) a text bound to their initials. Source and Content bind on both targets."),
    table("API", [
        ("Source", "The image (IImage); the fallback shows while it is null."),
        ("Content", "The fallback: what shows without an image, usually initials."),
        ("Classes", "sm (24 px), lg (40 px); CornerRadius for rounded squares."),
        ("c:AvatarGroup", "An ItemsControl overlapping its avatars (items or an ItemTemplate)."),
        ("Badge", "An avatar's badge, usually a c:AvatarBadge (its content an icon, its Background customizable)."),
        ("c:AvatarGroupCount", "The number of avatars a group leaves out, or an icon, as large as the group's avatars."),
    ]),
], datatype="DisplayExamples")

page("alert", "Alert", "Displays a callout for user attention.", ["shadcn/ui: alert", "Cosmo: c:Alert", "Web: <div>"], [
    example("Usage", """
        <StackPanel Spacing="16" Width="560">
          <c:Alert Title="Success! Your changes have been saved" Content="This is an alert with icon, title and description.">
            <c:Alert.Icon><a:Icon Kind="circle-check" Size="16" /></c:Alert.Icon>
          </c:Alert>
          <c:Alert Title="This Alert has a title and an icon. No description.">
            <c:Alert.Icon><a:Icon Kind="popcorn" Size="16" /></c:Alert.Icon>
          </c:Alert>
          <c:Alert Classes="destructive" Title="Unable to process your payment.">
            <c:Alert.Icon><a:Icon Kind="circle-alert" Size="16" /></c:Alert.Icon>
            <StackPanel Spacing="4">
              <TextBlock Text="Please verify your billing information and try again." LineHeight="22.75" TextWrapping="Wrap" />
              <c:List Classes="inside" TextBlock.LineHeight="20">
                <x:String>Check your card details</x:String>
                <x:String>Ensure sufficient funds</x:String>
                <x:String>Verify billing address</x:String>
              </c:List>
            </StackPanel>
          </c:Alert>
        </StackPanel>
        """, "Icon and Title are properties, the content is the description; destructive colors it with --destructive."),
    example("Binding", """
        <StackPanel Spacing="12" Width="420">
          <TextBox Text="{Binding Name}" PlaceholderText="Your name" AutomationProperties.Name="Name" />
          <Button Content="Save" Command="{Binding SaveCommand}" HorizontalAlignment="Left" />
          <c:Alert Classes.destructive="{Binding Failed}" Title="{Binding AlertTitle}" Content="{Binding AlertMessage}">
            <c:Alert.Icon><a:Icon Kind="info" Size="16" /></c:Alert.Icon>
          </c:Alert>
        </StackPanel>
        """, "The variant is a bound class, the title and description bind to the model."),
    table("API", [
        ("Icon, Title, Content", "The 16 px icon, the medium title and the description."),
        ("Classes", "destructive."),
    ]),
], datatype="DisplayExamples")

page("aspect-ratio", "Aspect Ratio", "Displays content within a desired ratio.", ["shadcn/ui: aspect-ratio", "Cosmo: c:AspectRatio", "Web: sized box"], [
    example("Usage", """
        <c:AspectRatio Ratio="1.7777778" Width="450">
          <Border CornerRadius="{DynamicResource CosmoRadiusLg}" Background="{DynamicResource CosmoMutedBrush}" ClipToBounds="True">
            <Image Source="/Assets/photo-landscape.png" Stretch="UniformToFill" AutomationProperties.Name="A landscape at dusk" />
          </Border>
        </c:AspectRatio>
        """, "The child is as wide as the room given and Ratio (16:9 here) times less high."),
    example("Square", """
        <c:AspectRatio Ratio="1" Width="200">
          <Border CornerRadius="{DynamicResource CosmoRadiusLg}" Background="{DynamicResource CosmoMutedBrush}" />
        </c:AspectRatio>
        """),
    table("API", [
        ("Ratio", "Width divided by height (1 by default); bindable."),
    ]),
], datatype="DisplayExamples")

page("calendar", "Calendar", "A date field component that allows users to enter and edit date.", ["shadcn/ui: calendar", "Avalonia: Calendar", "Cosmo: c:CalendarSelection, c:CalendarDays", "Web: the month's table"], [
    example("Usage", """
        <Border BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" BoxShadow="{DynamicResource CosmoShadowSm}">
          <Calendar DisplayDate="2025-06-12" SelectionMode="SingleDate" SelectedDate="{Binding Day}" />
        </Border>
        """, "Avalonia's Calendar in Cosmo's theme; SelectedDate binds two way, on the desktop and on the web, where Aioros computes the month for each request."),
    example("Range", """
        <Border BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" BoxShadow="{DynamicResource CosmoShadowSm}">
          <Calendar DisplayDate="2025-06-12" SelectionMode="SingleRange" c:CalendarSelection.RangeStart="{Binding From}" c:CalendarSelection.RangeEnd="{Binding To}" />
        </Border>
        """, "react-day-picker's mode=range: the ends and the band between them; RangeStart and RangeEnd bind two way."),
    example("Multiple", """
        <Border BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" BoxShadow="{DynamicResource CosmoShadowSm}">
          <Calendar DisplayDate="2025-06-12" SelectionMode="MultipleRange" c:CalendarSelection.Dates="{Binding Days}" />
        </Border>
        """, "mode=multiple: Dates binds two way; Ctrl or Shift adds days on the desktop, as Avalonia's MultipleRange."),
    example("Disabled days", """
        <Border BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" BoxShadow="{DynamicResource CosmoShadowSm}">
          <Calendar DisplayDate="2025-06-12" SelectionMode="SingleDate" SelectedDate="2025-06-12">
            <Calendar.BlackoutDates>
              <CalendarDateRange><x:Arguments><sys:DateTime>2025-05-01</sys:DateTime><sys:DateTime>2025-06-11</sys:DateTime></x:Arguments></CalendarDateRange>
            </Calendar.BlackoutDates>
          </Calendar>
        </Border>
        """, "disabled={{ before }}: BlackoutDates, muted at 50%."),
    example("Outside days hidden", """
        <Border BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" BoxShadow="{DynamicResource CosmoShadowSm}">
          <Calendar Classes="outside-hidden" DisplayDate="2025-06-12" SelectionMode="SingleDate" SelectedDate="{Binding Inside}" />
        </Border>
        """, "showOutsideDays={false}: the outside-hidden class."),
    example("Month and year selects", """
        <Calendar Classes="dropdown" DisplayDate="2025-06-12" SelectedDate="{Binding Chosen}" />
        """, "The dropdown class puts the month and year selects in the caption, as shadcn/ui's captionLayout=\"dropdown\"; the years run from a hundred before today to today."),
    example("Week numbers", """
        <Calendar Classes="week-numbers" DisplayDate="2025-06-12" SelectedDate="{Binding Numbered}" />
        """, "The week-numbers class numbers each week in the culture's rule before its days, as shadcn/ui's showWeekNumber."),
    table("API", [
        ("SelectedDate", "The selected day of a SingleDate calendar; two way (Avalonia)."),
        ("c:CalendarSelection.RangeStart, RangeEnd", "The selected range of a SingleRange calendar; two way."),
        ("c:CalendarSelection.Dates", "The selected days of a MultipleRange calendar; two way (c:CalendarDates in XAML)."),
        ("BlackoutDates", "Days that cannot be selected, shown at 50% (Avalonia)."),
        ("outside-hidden", "Hides the other months' days."),
    ]),
], datatype="DateExamples", namespaces=' xmlns:sys="using:System"')

page("date-picker", "Date Picker", "A date picker component with range and presets.", ["shadcn/ui: date-picker", "Avalonia: CalendarDatePicker", "Web: <input type=date>"], [
    example("Usage", """
        <CalendarDatePicker Width="240" DisplayDate="2025-06-12" />
        """, "An outline button with the calendar icon; it opens a popover with the calendar and shows the chosen day as MMMM d, yyyy."),
    example("Chosen", """
        <CalendarDatePicker Width="240" DisplayDate="2025-06-12" SelectedDate="2025-06-12" />
        """, "SelectedDate binds two way; on the web the browser's own date input posts it (framework need 64)."),
    example("Input", """
        <StackPanel Spacing="12" Width="320">
          <Label Content="Subscription Date" Padding="4,0" Target="Subscription" />
          <CalendarDatePicker Name="Subscription" Classes="input" HorizontalAlignment="Stretch" DisplayDate="2025-06-01" SelectedDate="2025-06-01" />
        </StackPanel>
        """, "The input class: the date can be typed, the ghost button opens the calendar."),
    example("Range", """
        <c:DateRangePicker Width="300" RangeStart="2022-01-20" RangeEnd="2022-02-09" />
        """, "Two months side by side; a click adds the day to the range as react-day-picker's mode=\"range\" does. RangeStart and RangeEnd bind two way."),
    example("Presets", """
        <CalendarDatePicker Width="240" DisplayDate="2025-06-12" SelectedDate="2025-06-12">
          <c:DatePresets.Items>
            <c:DatePresetList>
              <c:DatePreset Days="0">Today</c:DatePreset>
              <c:DatePreset Days="1">Tomorrow</c:DatePreset>
              <c:DatePreset Days="3">In 3 days</c:DatePreset>
              <c:DatePreset Days="7">In a week</c:DatePreset>
            </c:DatePresetList>
          </c:DatePresets.Items>
        </CalendarDatePicker>
        """, "c:DatePresets.Items puts a select of days from today above the calendar."),
    table("API", [
        ("SelectedDate", "The chosen day; two way (Avalonia)."),
        ("PlaceholderText", "The text while no day is chosen (Pick a date by default)."),
        ("CustomDateFormatString", "How the day is written (MMMM d, yyyy; MMMM dd, yyyy for input)."),
        ("input", "The input variant."),
    ]),
])

page("typography", "Typography", "Styles for headings, paragraphs, lists...etc", ["shadcn/ui: typography", "Avalonia: TextBlock classes", "Web: <h1>, <h2>, <p>, <ul>"], [
    example("h1", """
        <TextBlock Classes="typography h1" Text="Taxing Laughter: The Joke Tax Chronicles" />
        """, "Every typography class goes with typography, as in Classes=typography h1."),
    example("h2", """
        <Border Padding="0,0,0,8" BorderThickness="0,0,0,1" BorderBrush="{DynamicResource CosmoBorderBrush}">
          <TextBlock Classes="typography h2" Text="The People of the Kingdom" />
        </Border>
        """, "shadcn's h2 example draws its rule with border-b pb-2; here a Border does."),
    example("h3 and h4", """
        <StackPanel Spacing="16">
          <TextBlock Classes="typography h3" Text="The Joke Tax" />
          <TextBlock Classes="typography h4" Text="People stopped telling jokes" />
        </StackPanel>
        """),
    example("p and blockquote", """
        <StackPanel Spacing="24" Width="520">
          <TextBlock Classes="typography p" Text="The king, seeing how much happier his subjects were, realized the error of his ways and repealed the joke tax." />
          <Border Classes="typography blockquote">
            <TextBlock Text="&quot;After all,&quot; he said, &quot;everyone enjoys a good joke, so it's only fair that they should pay for the privilege.&quot;" />
          </Border>
        </StackPanel>
        """),
    example("List and inline code", """
        <StackPanel Spacing="16" Width="520">
          <c:List>
            <x:String>1st level of puns: 5 gold coins</x:String>
            <x:String>2nd level of jokes: 10 gold coins</x:String>
            <x:String>3rd level of one-liners : 20 gold coins</x:String>
          </c:List>
          <Border Classes="typography inline-code">
            <TextBlock Text="@radix-ui/react-alert-dialog" />
          </Border>
        </StackPanel>
        """),
    example("Lead, large, small, muted", """
        <StackPanel Spacing="16" Width="520">
          <TextBlock Classes="typography lead" Text="A modal dialog that interrupts the user with important content and expects a response." />
          <TextBlock Classes="typography large" Text="Are you absolutely sure?" />
          <TextBlock Classes="typography small" Text="Email address" />
          <TextBlock Classes="typography muted" Text="Enter your email address." />
        </StackPanel>
        """),
    example("Binding", """
        <StackPanel Spacing="12" Width="520">
          <TextBlock Classes="typography h4" Text="{Binding Levels.Count, StringFormat='{}{0} levels of jokes'}" />
          <c:List ItemsSource="{Binding Levels}" />
          <Button Classes="outline sm" Content="Add a level" Command="{Binding AddLevelCommand}" HorizontalAlignment="Left" />
        </StackPanel>
        """, "Headings bind with StringFormat; a c:List takes ItemsSource and ItemTemplate."),
    example("Balanced lines", """
        <TextBlock Classes="typography p" Width="360" TextWrapping="Wrap" a:Web.TextWrap="balance"
                   Text="The king, seeing how much happier his subjects were, realized the error of his ways and repealed the joke tax." />
        """, "a:Web.TextWrap=balance evens the lines' widths, as shadcn's text-balance: CSS's text-wrap on the web, Cosmo's balancing on the desktop."),
    table("API", [
        ("TextBlock classes", "typography with h1, h2, h3, h4, p, lead, large, small or muted."),
        ("Border classes", "typography with blockquote or inline-code, around a TextBlock."),
        ("c:List", "A bulleted ItemsControl; inside keeps the bullets inside (list-inside)."),
        ("a:Web.TextWrap", "balance evens a text's lines (shadcn's text-balance), inherited by the texts inside."),
    ]),
], datatype="DisplayExamples")

page("empty", "Empty", "Use the Empty component to display an empty state.", ["shadcn/ui: empty", "Cosmo: c:Empty, c:EmptyMedia", "Web: <div>"], [
    example("Usage", """
        <c:Empty Title="No Projects Yet" Description="You haven't created any projects yet. Get started by creating your first project." Width="560">
          <c:Empty.Media>
            <c:EmptyMedia Classes="icon"><a:Icon Kind="folder-code" Size="24" /></c:EmptyMedia>
          </c:Empty.Media>
          <StackPanel Spacing="24">
            <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Center">
              <Button Content="Create Project" />
              <Button Classes="outline" Content="Import Project" />
            </StackPanel>
            <HyperlinkButton Classes="link sm has-icon" HorizontalAlignment="Center" Foreground="{DynamicResource CosmoMutedForegroundBrush}" NavigateUri="https://ui.shadcn.com/docs/components/empty">
              <StackPanel><TextBlock Text="Learn More" /><a:Icon Kind="arrow-up-right" /></StackPanel>
            </HyperlinkButton>
          </StackPanel>
        </c:Empty>
        """, "Media, Title and Description are properties; the content holds the actions."),
    example("Outline", """
        <c:Empty Classes="outline" Title="Cloud Storage Empty" Description="Upload files to your cloud storage to access them anywhere." Width="560">
          <c:Empty.Media>
            <c:EmptyMedia Classes="icon"><a:Icon Kind="cloud" Size="24" /></c:EmptyMedia>
          </c:Empty.Media>
          <Button Classes="outline sm" Content="Upload Files" />
        </c:Empty>
        """, "outline draws the dashed border."),
    example("Binding", """
        <StackPanel Spacing="12" Width="560">
          <ItemsControl ItemsSource="{Binding Projects}">
            <ItemsControl.ItemTemplate>
              <DataTemplate x:DataType="m:Project">
                <TextBlock Classes="sm" Text="{Binding Name}" />
              </DataTemplate>
            </ItemsControl.ItemTemplate>
          </ItemsControl>
          <c:Empty Classes="outline" Title="No projects" Description="Every project was removed." IsVisible="{Binding !Projects.Count}">
            <Button Classes="outline sm" Content="Restore" Command="{Binding RestoreCommand}" />
          </c:Empty>
        </StackPanel>
        """, "The empty state shows when the bound collection is empty; its action runs a command."),
    table("API", [
        ("Media, Title, Description", "The picture, title and description of the empty state."),
        ("Content", "The actions."),
        ("Classes", "outline; c:EmptyMedia takes icon."),
    ]),
], datatype="DisplayExamples")

page("item", "Item", "A versatile component that you can use to display any content.", ["shadcn/ui: item", "Cosmo: c:Item, c:ItemMedia, c:ItemGroup, c:ItemHeader, c:ItemFooter, c:ItemSeparator", "Web: <div>"], [
    example("Usage", """
        <StackPanel Spacing="24" Width="448">
          <c:Item Classes="outline" Title="Basic Item" Description="A simple item with title and description.">
            <c:Item.Actions>
              <Button Classes="outline sm" Content="Action" />
            </c:Item.Actions>
          </c:Item>
          <c:Item Classes="outline sm link" Title="Your profile has been verified." a:Web.Href="/components/avatar">
            <c:Item.Media>
              <c:ItemMedia><a:Icon Kind="badge-check" Size="20" /></c:ItemMedia>
            </c:Item.Media>
            <c:Item.Actions>
              <a:Icon Kind="chevron-right" Size="16" />
            </c:Item.Actions>
          </c:Item>
        </StackPanel>
        """, "Media, Title, Description and Actions are properties; link gives an item that navigates its hover look."),
    example("Header and footer", """
        <StackPanel Width="448">
          <c:Item Classes="outline" Title="Acme Inc." Description="Design system subscription, October.">
            <c:Item.Header>
              <c:ItemHeader>
                <TextBlock Classes="sm" FontWeight="Medium" Text="Invoice #1042" />
                <c:Badge Classes="secondary" Content="Paid" />
              </c:ItemHeader>
            </c:Item.Header>
            <c:Item.Actions><Button Classes="outline sm" Content="View" /></c:Item.Actions>
            <c:Item.Footer>
              <c:ItemFooter>
                <TextBlock Classes="sm muted" Text="Due Oct 31" />
                <TextBlock Classes="sm" FontWeight="Medium" Text="$249.00" />
              </c:ItemFooter>
            </c:Item.Footer>
          </c:Item>
        </StackPanel>
        """, "Header and Footer are full rows above and below the item; c:ItemHeader and c:ItemFooter put their first and last children at their ends."),
    example("Group with separators", """
        <c:ItemGroup Width="448">
          <c:Item Title="Inbox" Description="12 unread messages." />
          <c:ItemSeparator />
          <c:Item Title="Archive" Description="Everything you kept." />
        </c:ItemGroup>
        """, "A c:ItemSeparator is a separator with no margin between grouped items."),
    example("Variants", """
        <StackPanel Spacing="24" Width="560">
          <c:Item Title="Default Variant" Description="Standard styling with subtle background and borders.">
            <c:Item.Actions><Button Classes="outline sm" Content="Open" /></c:Item.Actions>
          </c:Item>
          <c:Item Classes="outline" Title="Outline Variant" Description="Outlined style with clear borders and transparent background.">
            <c:Item.Actions><Button Classes="outline sm" Content="Open" /></c:Item.Actions>
          </c:Item>
          <c:Item Classes="muted" Title="Muted Variant" Description="Subdued appearance with muted colors for secondary content.">
            <c:Item.Actions><Button Classes="outline sm" Content="Open" /></c:Item.Actions>
          </c:Item>
        </StackPanel>
        """),
    example("Binding", """
        <StackPanel Spacing="12" Width="448">
          <ItemsControl ItemsSource="{Binding Projects}">
            <ItemsControl.ItemsPanel>
              <ItemsPanelTemplate><c:ItemGroup Spacing="8" /></ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemTemplate>
              <DataTemplate x:DataType="m:Project">
                <c:Item Classes="outline sm" Title="{Binding Name}" Description="{Binding Updated}">
                  <c:Item.Actions>
                    <Button Classes="outline sm" Content="Remove" Command="{Binding $parent[ItemsControl].((m:DisplayExamples)DataContext).RemoveCommand}" CommandParameter="{Binding}" />
                  </c:Item.Actions>
                </c:Item>
              </DataTemplate>
            </ItemsControl.ItemTemplate>
          </ItemsControl>
          <TextBlock Classes="sm muted" Text="{Binding ProjectCount}" />
        </StackPanel>
        """, "Items come from the model through an item template; each action runs a command with its item. The template binds each item's Title and Description."),
    table("API", [
        ("Media, Title, Description, Actions", "The item's parts; Content takes the title's and description's place when set."),
        ("Classes", "outline, muted, sm, link; c:ItemMedia takes icon or image."),
        ("c:ItemGroup", "A StackPanel of items; c:ItemSeparator between them."),
        ("Header, Footer", "Full rows above and below the item: c:ItemHeader and c:ItemFooter, their first and last children at their ends."),
    ]),
], datatype="DisplayExamples")

page("tabs", "Tabs", "A set of layered sections of content—known as tab panels—that are displayed one at a time.", ["shadcn/ui: tabs", "Avalonia: TabControl, TabItem", "Web: role=tablist buttons"], [
    example("Usage", """
        <TabControl Width="384" SelectedIndex="0">
          <TabItem Header="Account">
            <c:Card>
              <c:CardHeader Title="Account" Description="Make changes to your account here. Click save when you're done." />
              <c:CardContent>
                <StackPanel Spacing="12">
                  <Label Content="Name" />
                  <TextBox Text="Pedro Duarte" AutomationProperties.Name="Name" />
                </StackPanel>
              </c:CardContent>
              <c:CardFooter>
                <Button Content="Save changes" />
              </c:CardFooter>
            </c:Card>
          </TabItem>
          <TabItem Header="Password">
            <c:Card>
              <c:CardHeader Title="Password" Description="Change your password here. After saving, you'll be logged out." />
              <c:CardContent>
                <StackPanel Spacing="12">
                  <Label Content="New password" />
                  <TextBox PasswordChar="•" AutomationProperties.Name="New password" />
                </StackPanel>
              </c:CardContent>
            </c:Card>
          </TabItem>
        </TabControl>
        """, "Avalonia's TabControl: the strip is shadcn's TabsList, each TabItem a trigger; Right, Left, Home and End move and select."),
    example("Disabled", """
        <TabControl SelectedIndex="0">
          <TabItem Header="Overview" />
          <TabItem Header="Analytics" />
          <TabItem Header="Reports" />
          <TabItem Header="Notifications" IsEnabled="False" />
        </TabControl>
        """),
    example("Line", """
        <TabControl Classes="line" SelectedIndex="0">
          <TabItem Header="Overview" />
          <TabItem Header="Analytics" />
          <TabItem Header="Reports" />
        </TabControl>
        """, "The line class is shadcn's variant=\"line\": no strip, the selected tab marked by a line below it."),
    example("Vertical", """
        <TabControl TabStripPlacement="Left" SelectedIndex="0" Width="384">
          <TabItem Header="Account">
            <TextBlock Classes="sm muted" Text="Make changes to your account here." TextWrapping="Wrap" />
          </TabItem>
          <TabItem Header="Password">
            <TextBlock Classes="sm muted" Text="Change your password here." TextWrapping="Wrap" />
          </TabItem>
          <TabItem Header="Notifications">
            <TextBlock Classes="sm muted" Text="Choose what we email you about." TextWrapping="Wrap" />
          </TabItem>
        </TabControl>
        """, "TabStripPlacement=\"Left\" is shadcn's orientation=\"vertical\": the strip on the left, its tabs full width, the page beside it; Up and Down move. With the line class the selected tab's line stands on its right."),
    example("Binding", """
        <StackPanel Spacing="12" Width="384">
          <TabControl ItemsSource="{Binding Tabs}" SelectedIndex="{Binding SelectedTab}">
            <TabControl.ItemTemplate>
              <DataTemplate x:DataType="m:TabPage">
                <TextBlock Text="{Binding Header}" />
              </DataTemplate>
            </TabControl.ItemTemplate>
            <TabControl.ContentTemplate>
              <DataTemplate x:DataType="m:TabPage">
                <c:Card>
                  <c:CardContent>
                    <StackPanel Spacing="8">
                      <TextBlock Text="{Binding Title}" FontSize="16" FontWeight="SemiBold" />
                      <TextBlock Classes="sm muted" Text="{Binding Body}" />
                    </StackPanel>
                  </c:CardContent>
                </c:Card>
              </DataTemplate>
            </TabControl.ContentTemplate>
          </TabControl>
          <StackPanel Orientation="Horizontal" Spacing="12">
            <Button Content="Save password" Command="{Binding SaveCommand}" CommandParameter="Password" />
            <TextBlock Classes="sm muted" Text="{Binding Status}" VerticalAlignment="Center" />
          </StackPanel>
        </StackPanel>
        """, "Tabs from the model: headers through ItemTemplate, pages through ContentTemplate, the selection two way."),
    table("API", [
        ("TabItem Header, Content", "The trigger's label and the tab's page."),
        ("line", "On the TabControl: shadcn's line variant."),
        ("TabStripPlacement=\"Left\"", "shadcn's vertical orientation."),
        ("SelectedIndex, SelectedItem", "Two way."),
        ("ItemsSource, ItemTemplate, ContentTemplate", "Tabs from the model."),
    ]),
], datatype="NavigationExamples")

page("accordion", "Accordion", "A vertically stacked set of interactive headings that each reveal a section of content.", ["shadcn/ui: accordion", "Cosmo: c:Accordion of Expanders", "Web: <details>"], [
    example("Usage", """
        <c:Accordion IsCollapsible="True" Width="560">
          <Expander Header="Product Information" IsExpanded="True">
            <StackPanel Spacing="16">
              <TextBlock Text="Our flagship product combines cutting-edge technology with sleek design. Built with premium materials, it offers unparalleled performance and reliability." TextWrapping="Wrap" />
              <TextBlock Text="Key features include advanced processing capabilities, and an intuitive user interface designed for both beginners and experts." TextWrapping="Wrap" />
            </StackPanel>
          </Expander>
          <Expander Header="Shipping Details">
            <TextBlock Text="We offer worldwide shipping through trusted courier partners. Standard delivery takes 3-5 business days, while express shipping ensures delivery within 1-2 business days." TextWrapping="Wrap" />
          </Expander>
          <Expander Header="Return Policy">
            <TextBlock Text="We stand behind our products with a comprehensive 30-day return policy. If you're not completely satisfied, simply return the item in its original condition." TextWrapping="Wrap" />
          </Expander>
        </c:Accordion>
        """, "One item opens at a time (IsMultiple for several); IsCollapsible lets the open item close; the content's height animates over 200 ms."),
    example("Binding", """
        <StackPanel Spacing="12" Width="560">
          <c:Accordion IsMultiple="True">
            <Expander Header="Is it accessible?" IsExpanded="{Binding AccessibleOpen}">
              <TextBlock Text="{Binding FirstAnswer}" TextWrapping="Wrap" />
            </Expander>
            <Expander Header="Is it styled?">
              <TextBlock Text="{Binding SecondAnswer}" TextWrapping="Wrap" />
            </Expander>
          </c:Accordion>
          <Button Classes="outline sm" Content="Toggle the first answer" Command="{Binding ToggleAnswerCommand}" HorizontalAlignment="Left" />
        </StackPanel>
        """, "An item's IsExpanded binds two way. A c:Accordion also takes ItemsSource with ItemTemplate (the header) and ContentTemplate (the answer), written through their templates on both targets."),
    table("API", [
        ("IsMultiple", "Several items open at once (Radix's type=multiple)."),
        ("IsCollapsible", "The open item of a single accordion can close."),
        ("ItemsSource, ItemTemplate, ContentTemplate", "Items from the model."),
        ("Expander IsExpanded", "Two way; Header and Content for literal items."),
    ]),
], datatype="NavigationExamples")

page("collapsible", "Collapsible", "An interactive component which expands/collapses a panel.", ["shadcn/ui: collapsible", "Avalonia: Expander with collapsible", "Web: <details>"], [
    example("Usage", """
        <Expander Classes="collapsible" Width="350" Padding="0,0,16,0" IsExpanded="{Binding Starred}">
          <Expander.Header>
            <StackPanel Spacing="8">
              <Grid Height="32">
                <TextBlock Text="@peduarte starred 3 repositories" FontSize="14" FontWeight="SemiBold" VerticalAlignment="Center" Margin="16,0,0,0" />
              </Grid>
              <Border CornerRadius="{DynamicResource CosmoRadiusMd}" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" Padding="16,8">
                <TextBlock Text="@radix-ui/primitives" FontFamily="{DynamicResource CosmoFontMono}" FontSize="14" />
              </Border>
            </StackPanel>
          </Expander.Header>
          <StackPanel Spacing="8">
            <Border CornerRadius="{DynamicResource CosmoRadiusMd}" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" Padding="16,8">
              <TextBlock Text="@radix-ui/colors" FontFamily="{DynamicResource CosmoFontMono}" FontSize="14" />
            </Border>
            <Border CornerRadius="{DynamicResource CosmoRadiusMd}" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" Padding="16,8">
              <TextBlock Text="@stitches/react" FontFamily="{DynamicResource CosmoFontMono}" FontSize="14" />
            </Border>
          </StackPanel>
        </Expander>
        """, "An Expander with the collapsible class: the header toggles it, the chevrons sit at its top right (inset by Padding); IsExpanded binds two way."),
    table("API", [
        ("IsExpanded", "Two way (Radix's open and onOpenChange)."),
        ("Header, Content", "The trigger and the panel."),
        ("Padding", "Insets the toggle from the header's top right."),
    ]),
], datatype="NavigationExamples")

page("breadcrumb", "Breadcrumb", "Displays the path to the current resource using a hierarchy of links.", ["shadcn/ui: breadcrumb", "Cosmo: c:Breadcrumb", "Web: <a href>"], [
    example("Usage", """
        <c:Breadcrumb>
          <HyperlinkButton Content="Home" a:Web.Href="/" />
          <Button AutomationProperties.Name="Toggle menu">
            <c:BreadcrumbEllipsis Width="16" Height="16" />
            <Button.Flyout>
              <MenuFlyout Placement="BottomEdgeAlignedLeft">
                <MenuItem Header="Documentation" a:Web.Href="/installation" />
                <MenuItem Header="Themes" a:Web.Href="/themes" />
                <MenuItem Header="GitHub" a:Web.Href="/" />
              </MenuFlyout>
            </Button.Flyout>
          </Button>
          <HyperlinkButton Content="Components" a:Web.Href="/components/button" />
          <c:BreadcrumbPage Text="Breadcrumb" />
        </c:Breadcrumb>
        """, "Links, a menu button with collapsed items and the current page; the separators come between them."),
    example("Custom separator", """
        <c:Breadcrumb>
          <c:Breadcrumb.SeparatorTemplate>
            <DataTemplate><a:Icon Kind="slash" Size="14" /></DataTemplate>
          </c:Breadcrumb.SeparatorTemplate>
          <HyperlinkButton Content="Home" a:Web.Href="/" />
          <HyperlinkButton Content="Components" a:Web.Href="/components/button" />
          <c:BreadcrumbPage Text="Breadcrumb" />
        </c:Breadcrumb>
        """),
    example("Binding", """
        <StackPanel Spacing="12">
          <c:Breadcrumb ItemsSource="{Binding Crumbs}">
            <c:Breadcrumb.ItemTemplate>
              <DataTemplate x:DataType="m:Crumb">
                <HyperlinkButton Content="{Binding Name}" a:Web.Href="{Binding Href}" />
              </DataTemplate>
            </c:Breadcrumb.ItemTemplate>
          </c:Breadcrumb>
          <Button Classes="outline sm" Content="Go deeper" Command="{Binding DeeperCommand}" HorizontalAlignment="Left" />
        </StackPanel>
        """, "Crumbs come from the model through an item template, each a link to its bound address; a separator stands between them, none before the first, on both targets."),
    table("API", [
        ("Items, ItemsSource, ItemTemplate", "Links (HyperlinkButton), a menu Button, c:BreadcrumbEllipsis, c:BreadcrumbPage."),
        ("SeparatorTemplate", "What separates the items (chevron-right by default)."),
    ]),
], datatype="NavigationExamples")

page("pagination", "Pagination", "Pagination with page navigation, next and previous links.", ["shadcn/ui: pagination", "Cosmo: c:Pagination", "Web: buttons"], [
    example("Usage", """
        <c:Pagination>
          <c:PaginationPrevious />
          <Button Classes="ghost icon" Content="1" />
          <Button Classes="outline icon" Content="2" />
          <Button Classes="ghost icon" Content="3" />
          <c:PaginationEllipsis />
          <c:PaginationNext />
        </c:Pagination>
        """, "As shadcn's, a pagination is its items: Previous, page buttons (ghost, the current one outline), an ellipsis and Next."),
    example("Binding", """
        <StackPanel Spacing="12">
          <StackPanel Orientation="Horizontal" Spacing="4" HorizontalAlignment="Center">
            <c:PaginationPrevious Command="{Binding PreviousCommand}" />
            <c:Pagination ItemsSource="{Binding Pages}">
              <c:Pagination.ItemTemplate>
                <DataTemplate x:DataType="m:PageLink">
                  <Button Classes="icon" Classes.ghost="{Binding IsOther}" Classes.outline="{Binding IsCurrent}" Content="{Binding Number}"
                          Command="{Binding $parent[ItemsControl].((m:NavigationExamples)DataContext).GoToCommand}" CommandParameter="{Binding Number}" />
                </DataTemplate>
              </c:Pagination.ItemTemplate>
            </c:Pagination>
            <c:PaginationEllipsis />
            <c:PaginationNext Command="{Binding NextCommand}" />
          </StackPanel>
          <TextBlock Classes="sm muted" HorizontalAlignment="Center" Text="{Binding Page, StringFormat='Page {0} of 10'}" />
        </StackPanel>
        """, "Pages from the model through an item template: the current one takes the outline class, each button runs the model's command with its number."),
    table("API", [
        ("c:Pagination", "An ItemsControl of the pagination's buttons, centered, 4 px apart."),
        ("c:PaginationPrevious, c:PaginationNext", "Ghost buttons with their chevron; Command, CommandParameter, Content."),
        ("c:PaginationEllipsis", "The hidden pages' ellipsis."),
    ]),
], datatype="NavigationExamples")

page("button-group", "Button Group", "A container that groups related buttons together with consistent styling.", ["shadcn/ui: button-group", "Cosmo: c:ButtonGroup", "Web: flex row"], [
    example("Usage", """
        <c:ButtonGroup>
          <c:ButtonGroup>
            <Button Classes="outline icon" AutomationProperties.Name="Go Back"><a:Icon Kind="arrow-right" /></Button>
          </c:ButtonGroup>
          <c:ButtonGroup>
            <Button Classes="outline" Content="Archive" />
            <Button Classes="outline" Content="Report" />
          </c:ButtonGroup>
          <c:ButtonGroup>
            <Button Classes="outline" Content="Snooze" />
            <Button Classes="outline icon" AutomationProperties.Name="More Options">
              <a:Icon Kind="ellipsis" />
              <Button.Flyout>
                <MenuFlyout Placement="BottomEdgeAlignedRight">
                  <MenuItem Header="Mark as Read" />
                  <MenuItem Header="Archive" />
                  <Separator />
                  <MenuItem Header="Trash" Classes="destructive" />
                </MenuFlyout>
              </Button.Flyout>
            </Button>
          </c:ButtonGroup>
        </c:ButtonGroup>
        """, "Children are joined: inner corners square, shared borders single; nested groups sit 8 px apart."),
    example("Sizes", """
        <StackPanel Spacing="16">
          <c:ButtonGroup>
            <Button Classes="outline sm" Content="Small" />
            <Button Classes="outline sm" Content="Button" />
            <Button Classes="outline sm" Content="Group" />
          </c:ButtonGroup>
          <c:ButtonGroup>
            <Button Classes="outline lg" Content="Large" />
            <Button Classes="outline lg" Content="Button" />
            <Button Classes="outline lg" Content="Group" />
          </c:ButtonGroup>
        </StackPanel>
        """),
    example("Orientation, separator and text", """
        <WrapPanel ItemSpacing="32" LineSpacing="16">
          <c:ButtonGroup Orientation="Vertical" VerticalAlignment="Top" AutomationProperties.Name="Media controls">
            <Button Classes="outline icon" AutomationProperties.Name="Add"><a:Icon Kind="plus" /></Button>
            <Button Classes="outline icon" AutomationProperties.Name="Remove"><a:Icon Kind="minus" /></Button>
          </c:ButtonGroup>
          <c:ButtonGroup VerticalAlignment="Top">
            <Button Classes="secondary sm" Content="Copy" />
            <Separator />
            <Button Classes="secondary sm" Content="Paste" />
          </c:ButtonGroup>
          <c:ButtonGroup VerticalAlignment="Top">
            <c:ButtonGroupText Content="https://" />
            <TextBox PlaceholderText="my-app" Width="160" AutomationProperties.Name="Address" />
            <Button Classes="outline" Content="Go" />
          </c:ButtonGroup>
        </WrapPanel>
        """),
    example("Binding", """
        <StackPanel Spacing="12">
          <c:ButtonGroup>
            <Button Classes="outline" Classes.secondary="{Binding ArchiveChosen}" Content="Archive" Command="{Binding ChooseCommand}" CommandParameter="Archive" />
            <Button Classes="outline" Classes.secondary="{Binding ReportChosen}" Content="Report" Command="{Binding ChooseCommand}" CommandParameter="Report" />
            <Button Classes="outline" Classes.secondary="{Binding SnoozeChosen}" Content="Snooze" Command="{Binding ChooseCommand}" CommandParameter="Snooze" />
          </c:ButtonGroup>
          <TextBlock Classes="sm muted" Text="{Binding Action}" />
        </StackPanel>
        """, "Each button runs a command with its parameter; the chosen one takes a bound class."),
    table("API", [
        ("Orientation", "Horizontal (default) or Vertical."),
        ("Children", "Buttons, TextBox, ComboBox, c:ButtonGroupText, Separator, nested c:ButtonGroup."),
    ]),
], datatype="NavigationExamples")

TAG_ROWS = "\n".join(f'      <TextBlock Classes="sm" Text="v1.2.0-beta.{n}" />\n      <Separator Margin="0,8" />' for n in range(50, 40, -1))

page("scroll-area", "Scroll Area", "Augments native scroll functionality for custom, cross-browser styling.", ["shadcn/ui: scroll-area", "Avalonia: ScrollViewer.scroll-area", "Web: overflow: auto"], [
    example("Usage", f"""
        <ScrollViewer Classes="scroll-area" Width="192" Height="288" BorderThickness="1"
                      BorderBrush="{{DynamicResource CosmoBorderBrush}}" CornerRadius="{{DynamicResource CosmoRadiusMd}}">
          <StackPanel Margin="16">
            <TextBlock Classes="sm" Text="Tags" FontWeight="Medium" Margin="0,0,0,16" />
{TAG_ROWS}
          </StackPanel>
        </ScrollViewer>
        """, "The scroll bars show while the pointer is over the area, thin and rounded as Radix's."),
    example("Horizontal", """
        <ScrollViewer Classes="scroll-area" Width="384" HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Disabled"
                      BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusMd}">
          <StackPanel Orientation="Horizontal" Spacing="16" Margin="16">
            <StackPanel>
              <Border CornerRadius="{DynamicResource CosmoRadiusMd}" ClipToBounds="True">
                <Image Source="/Assets/art-binni.png" Width="150" Height="200" Stretch="UniformToFill" />
              </Border>
              <TextBlock Classes="xs muted" Margin="0,8,0,0" Text="Photo by Ornella Binni" />
            </StackPanel>
            <StackPanel>
              <Border CornerRadius="{DynamicResource CosmoRadiusMd}" ClipToBounds="True">
                <Image Source="/Assets/art-byrom.png" Width="150" Height="200" Stretch="UniformToFill" />
              </Border>
              <TextBlock Classes="xs muted" Margin="0,8,0,0" Text="Photo by Tom Byrom" />
            </StackPanel>
            <StackPanel>
              <Border CornerRadius="{DynamicResource CosmoRadiusMd}" ClipToBounds="True">
                <Image Source="/Assets/art-malyavko.png" Width="150" Height="200" Stretch="UniformToFill" />
              </Border>
              <TextBlock Classes="xs muted" Margin="0,8,0,0" Text="Photo by Vladimir Malyavko" />
            </StackPanel>
          </StackPanel>
        </ScrollViewer>
        """),
    example("Binding", """
        <ScrollViewer Classes="scroll-area" Width="192" Height="200" BorderThickness="1"
                      BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusMd}">
          <ItemsControl ItemsSource="{Binding Tags}" Margin="16">
            <ItemsControl.ItemTemplate>
              <DataTemplate x:DataType="x:String">
                <StackPanel>
                  <TextBlock Classes="sm" Text="{Binding}" />
                  <Separator Margin="0,8" />
                </StackPanel>
              </DataTemplate>
            </ItemsControl.ItemTemplate>
          </ItemsControl>
        </ScrollViewer>
        """, "The tags come from the model through an item template."),
    table("API", [
        ("ScrollViewer.scroll-area", "Radix's scroll area: the content clipped to the rounded border, thin bars shown on hover."),
        ("HorizontalScrollBarVisibility", "Auto for a horizontal area, as shadcn's ScrollBar orientation=\"horizontal\"."),
        ("Offset", "The scrolled position, bindable two way on the desktop."),
    ]),
], datatype="StructureExamples")

page("table", "Table", "A responsive table component.", ["shadcn/ui: table", "Avalonia: TableView", "Cosmo: c:TableFooter, c:TableCaption", "Web: <table>"], [
    example("Usage", """
        <StackPanel Width="592">
          <TableView ItemsSource="{Binding Invoices}" SelectedItem="{Binding SelectedInvoice}">
            <TableView.Columns>
              <TableViewColumn Header="Invoice" Width="100" x:DataType="m:Invoice">
                <TableViewColumn.CellTemplate>
                  <DataTemplate x:DataType="m:Invoice"><TextBlock Text="{Binding Number}" FontWeight="Medium" /></DataTemplate>
                </TableViewColumn.CellTemplate>
              </TableViewColumn>
              <TableViewColumn Header="Status" Binding="{Binding Status}" Width="128*" x:DataType="m:Invoice" />
              <TableViewColumn Header="Method" Binding="{Binding Method}" Width="202*" x:DataType="m:Invoice" />
              <TableViewColumn Header="Amount" Binding="{Binding Amount}" Width="162*" HorizontalContentAlignment="Right" x:DataType="m:Invoice" />
            </TableView.Columns>
          </TableView>
          <c:TableFooter>
            <DockPanel>
              <TextBlock DockPanel.Dock="Right" Text="{Binding Total}" />
              <TextBlock Text="Total" />
            </DockPanel>
          </c:TableFooter>
          <c:TableCaption Text="A list of your recent invoices." />
        </StackPanel>
        """, "Avalonia's TableView in Cosmo's themes; the footer and caption follow it."),
    example("Binding", """
        <StackPanel Spacing="12" Width="592">
          <TableView ItemsSource="{Binding Invoices}">
            <TableView.Columns>
              <TableViewColumn Header="Invoice" Binding="{Binding Number}" Width="100" x:DataType="m:Invoice" />
              <TableViewColumn Header="Status" Width="*" x:DataType="m:Invoice">
                <TableViewColumn.CellTemplate>
                  <DataTemplate x:DataType="m:Invoice">
                    <c:Badge Classes="outline" HorizontalAlignment="Left"><TextBlock Text="{Binding Status}" /></c:Badge>
                  </DataTemplate>
                </TableViewColumn.CellTemplate>
              </TableViewColumn>
              <TableViewColumn Header="Amount" Binding="{Binding Amount}" Width="*" HorizontalContentAlignment="Right" x:DataType="m:Invoice" />
            </TableView.Columns>
          </TableView>
          <c:TableFooter>
            <DockPanel>
              <TextBlock DockPanel.Dock="Right" Text="{Binding Total}" />
              <TextBlock Text="{Binding Count}" />
            </DockPanel>
          </c:TableFooter>
          <Button Classes="outline sm" Content="Add invoice" Command="{Binding AddInvoiceCommand}" HorizontalAlignment="Left" />
        </StackPanel>
        """, "Rows from an observable collection, a cell template for the status, a command adding a row and a computed total."),
    table("API", [
        ("TableView", "Header, rows (hover, selection), cells: ItemsSource, Columns, SelectedItem."),
        ("TableViewColumn", "Header, Binding or CellTemplate, Width (pixels or stars), HorizontalContentAlignment."),
        ("c:TableFooter", "The footer row: muted background, top border, medium text."),
        ("c:TableCaption", "The muted caption under the table."),
    ]),
], datatype="StructureExamples")


def card_slide(content="{Binding}", wrap=True):
    card = f"""<c:Card><c:AspectRatio Ratio="1"><c:CardContent Padding="24"><TextBlock Text="{content}" FontSize="36" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" /></c:CardContent></c:AspectRatio></c:Card>"""
    return f'<Border Padding="4">{card}</Border>' if wrap else card


page("resizable", "Resizable", "Accessible resizable panel groups and layouts with keyboard support.", ["shadcn/ui: resizable", "Cosmo: c:ResizablePanelGroup, c:ResizablePanel, c:ResizableHandle", "Avalonia: Grid, GridSplitter", "Web: CSS grid"], [
    example("Usage", """
        <Border Width="450" Height="202" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" ClipToBounds="True">
          <c:ResizablePanelGroup>
            <c:ResizablePanel DefaultSize="50">
              <TextBlock Text="One" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" />
            </c:ResizablePanel>
            <c:ResizableHandle />
            <c:ResizablePanel DefaultSize="50">
              <c:ResizablePanelGroup Orientation="Vertical">
                <c:ResizablePanel DefaultSize="25">
                  <TextBlock Text="Two" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" />
                </c:ResizablePanel>
                <c:ResizableHandle />
                <c:ResizablePanel DefaultSize="75">
                  <TextBlock Text="Three" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" />
                </c:ResizablePanel>
              </c:ResizablePanelGroup>
            </c:ResizablePanel>
          </c:ResizablePanelGroup>
        </Border>
        """, "A group is a Grid; drag a handle, or focus it and use the arrows (5%), Home and End."),
    example("Vertical", """
        <Border Width="450" Height="200" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" ClipToBounds="True">
          <c:ResizablePanelGroup Orientation="Vertical">
            <c:ResizablePanel DefaultSize="25">
              <TextBlock Text="Header" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" />
            </c:ResizablePanel>
            <c:ResizableHandle />
            <c:ResizablePanel DefaultSize="75">
              <TextBlock Text="Content" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" />
            </c:ResizablePanel>
          </c:ResizablePanelGroup>
        </Border>
        """),
    example("Handle", """
        <Border Width="450" Height="200" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" ClipToBounds="True">
          <c:ResizablePanelGroup>
            <c:ResizablePanel DefaultSize="25">
              <TextBlock Text="Sidebar" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" />
            </c:ResizablePanel>
            <c:ResizableHandle WithHandle="True" />
            <c:ResizablePanel DefaultSize="75">
              <TextBlock Text="Content" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" />
            </c:ResizablePanel>
          </c:ResizablePanelGroup>
        </Border>
        """, "WithHandle draws the grip on the line."),
    example("Binding", """
        <StackPanel Spacing="12" MaxWidth="450">
          <Border Height="160" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" ClipToBounds="True">
            <c:ResizablePanelGroup>
              <c:ResizablePanel Size="{Binding SidebarSize}" MinSize="15" MaxSize="50" IsCollapsible="True">
                <TextBlock Text="{Binding SizeText}" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" />
              </c:ResizablePanel>
              <c:ResizableHandle WithHandle="True" />
              <c:ResizablePanel>
                <TextBlock Text="Content" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" />
              </c:ResizablePanel>
            </c:ResizablePanelGroup>
          </Border>
          <Button Classes="outline sm" Content="Reset to 25%" Command="{Binding ResetSizeCommand}" HorizontalAlignment="Left" />
        </StackPanel>
        """, "Size binds two way: dragging updates the model, the command resizes the panel; MinSize and MaxSize keep it between 15% and 50%, Enter collapses it."),
    table("API", [
        ("c:ResizablePanelGroup", "Orientation (Horizontal, Vertical): a Grid of panels and handles."),
        ("c:ResizablePanel", "DefaultSize, Size (two way), MinSize, MaxSize, IsCollapsible, CollapsedSize: percentages of the group."),
        ("c:ResizableHandle", "A GridSplitter: WithHandle; arrows by 5%, Home, End, Enter to collapse, F6 to the next handle."),
    ]),
], datatype="CompositeExamples")

page("carousel", "Carousel", "A carousel with motion and swipe built using Embla.", ["shadcn/ui: carousel", "Avalonia: Carousel", "Web: scroll-snap"], [
    example("Usage", f"""
        <Carousel Width="320" Margin="48,0">
          {card_slide("1")}
          {card_slide("2")}
          {card_slide("3")}
          {card_slide("4")}
          {card_slide("5")}
        </Carousel>
        """, "Avalonia's Carousel in Cosmo's theme: the previous and next buttons, Left and Right, swipe and the mouse's drag (Embla's)."),
    example("Orientation", f"""
        <Carousel Classes="vertical" Width="320" Height="200" Margin="0,48">
          <Border Padding="4"><c:Card><c:CardContent Padding="24"><TextBlock Text="1" FontSize="30" FontWeight="SemiBold" HorizontalAlignment="Center" /></c:CardContent></c:Card></Border>
          <Border Padding="4"><c:Card><c:CardContent Padding="24"><TextBlock Text="2" FontSize="30" FontWeight="SemiBold" HorizontalAlignment="Center" /></c:CardContent></c:Card></Border>
          <Border Padding="4"><c:Card><c:CardContent Padding="24"><TextBlock Text="3" FontSize="30" FontWeight="SemiBold" HorizontalAlignment="Center" /></c:CardContent></c:Card></Border>
        </Carousel>
        """, "The vertical class slides up and down and turns the buttons."),
    example("Binding", f"""
        <StackPanel Width="320" Margin="48,0" Spacing="8">
          <Carousel Classes="indicators" ItemsSource="{{Binding Slides}}" SelectedIndex="{{Binding SlideIndex}}" WrapSelection="True">
            <Carousel.ItemTemplate>
              <DataTemplate x:DataType="x:Int32">
                {card_slide(wrap=False)}
              </DataTemplate>
            </Carousel.ItemTemplate>
          </Carousel>
          <TextBlock Classes="sm muted" Text="{{Binding SlideText}}" HorizontalAlignment="Center" />
          <Button Classes="outline sm" Content="Add a slide" Command="{{Binding AddSlideCommand}}" HorizontalAlignment="Center" />
        </StackPanel>
        """, "Slides from the model with an item template, SelectedIndex two way, WrapSelection as Embla's loop and the indicators class for dots."),
    table("API", [
        ("Carousel", "ItemsSource, ItemTemplate, SelectedIndex (two way), WrapSelection (loop), ViewportFraction, IsSwipeEnabled, PageTransition."),
        ("Classes", "vertical, indicators, align-start (slides narrower than the viewport start at its edge)."),
        ("c:CarouselIndicators", "The dots: Count, SelectedIndex (two way)."),
    ]),
], datatype="CompositeExamples")


COMMAND_ITEMS = """
            <c:CommandGroup Header="Suggestions">
              <c:CommandItem><StackPanel><a:Icon Kind="calendar" /><TextBlock Text="Calendar" /></StackPanel></c:CommandItem>
              <c:CommandItem><StackPanel><a:Icon Kind="smile" /><TextBlock Text="Search Emoji" /></StackPanel></c:CommandItem>
              <c:CommandItem IsEnabled="False"><StackPanel><a:Icon Kind="calculator" /><TextBlock Text="Calculator" /></StackPanel></c:CommandItem>
            </c:CommandGroup>
            <c:CommandSeparator />
            <c:CommandGroup Header="Settings">
              <c:CommandItem Shortcut="⌘P"><StackPanel><a:Icon Kind="user" /><TextBlock Text="Profile" /></StackPanel></c:CommandItem>
              <c:CommandItem Shortcut="⌘B"><StackPanel><a:Icon Kind="credit-card" /><TextBlock Text="Billing" /></StackPanel></c:CommandItem>
              <c:CommandItem Shortcut="⌘S"><StackPanel><a:Icon Kind="settings" /><TextBlock Text="Settings" /></StackPanel></c:CommandItem>
            </c:CommandGroup>"""

page("command", "Command", "Fast, composable, unstyled command menu.", ["shadcn/ui: command", "Cosmo: c:Command, c:CommandGroup, c:CommandItem, c:CommandDialog", "Web: <input> and buttons"], [
    example("Usage", f"""
        <c:Command Classes="outline" Width="450">{COMMAND_ITEMS}
        </c:Command>
        """, "Type to filter and rank the items as cmdk does; Up, Down, Home, End and Enter move and choose."),
    example("Dialog", f"""
        <StackPanel Spacing="8">
          <TextBlock Classes="sm muted" Text="Press Ctrl+J, or the button." />
          <Button x:Name="OpenPalette" Classes="outline sm" Content="Open the palette" Command="{{Binding TogglePaletteCommand}}" HorizontalAlignment="Left" />
          <c:CommandDialog Gesture="Ctrl+J" Trigger="{{Binding #OpenPalette}}" IsOpen="{{Binding PaletteOpen}}">
            <c:Command>{COMMAND_ITEMS}
            </c:Command>
          </c:CommandDialog>
        </StackPanel>
        """, "CommandDialog opens and closes with its Gesture anywhere in the window, as its Trigger's hot key (so the web presses the button too); IsOpen binds two way."),
    example("Shortcut only", f"""
        <StackPanel Spacing="8">
          <TextBlock Classes="sm muted" Text="Press Ctrl+I." />
          <c:CommandDialog Gesture="Ctrl+I">
            <c:Command>{COMMAND_ITEMS}
            </c:Command>
          </c:CommandDialog>
        </StackPanel>
        """, "A dialog without a Trigger keeps its Gesture: the desktop handles it in the window, the web opens and closes the dialog in the browser."),
    example("Binding", """
        <StackPanel Spacing="12" Width="450">
          <c:Command Classes="outline" Text="{Binding CommandSearch}" SelectedValue="{Binding CommandValue}" Placeholder="Search the Gallery...">
            <c:CommandGroup Header="Pages">
              <c:CommandItem Content="Dashboard" Command="{Binding OpenCommand}" CommandParameter="/examples/dashboard" />
              <c:CommandItem Content="Forms" Command="{Binding OpenCommand}" CommandParameter="/examples/forms" />
              <c:CommandItem Content="Themes" Command="{Binding OpenCommand}" CommandParameter="/themes" />
              <c:CommandItem Content="Binding and MVVM" Command="{Binding OpenCommand}" CommandParameter="/binding" />
            </c:CommandGroup>
            <c:CommandGroup Header="Components" ItemsSource="{Binding CommandPages}" ItemCommand="{Binding $parent[c:Command].((m:CompositeExamples)DataContext).OpenCommand, x:DataType=m:CommandPage}"
                            ItemCommandParameter="{Binding Path, x:DataType=m:CommandPage}">
              <c:CommandGroup.ItemTemplate>
                <DataTemplate x:DataType="m:CommandPage">
                  <TextBlock Text="{Binding Title}" />
                </DataTemplate>
              </c:CommandGroup.ItemTemplate>
            </c:CommandGroup>
          </c:Command>
          <TextBlock Classes="sm muted" Text="{Binding Opened, StringFormat='Opened: {0}'}" />
          <TextBlock Classes="sm muted" Text="{Binding CommandValue, StringFormat='Value: {0}', FallbackValue='Value: none'}" />
        </StackPanel>
        """, "Each item runs the model's command with its page; Text and SelectedValue bind two way; the Components group's items come from the model with an ItemTemplate, each running ItemCommand with ItemCommandParameter. Typing filters and ranks the items on both targets, and the web posts the text, the value and the chosen item."),
    table("API", [
        ("c:Command", "Text and SelectedValue (two way), Placeholder, Empty, ShouldFilter, IsLoop, Filter; the outline class is the demo's border and shadow."),
        ("c:CommandGroup", "Header, items or ItemsSource with ItemTemplate, ItemCommand and ItemCommandParameter (cmdk's onSelect)."),
        ("c:CommandItem", "Content, Value, Keywords, Shortcut, Command and CommandParameter, IsEnabled."),
        ("c:CommandSeparator", "A line between groups, hidden while searching."),
        ("c:CommandDialog", "IsOpen (two way), Gesture (Ctrl+K), Trigger, Title."),
    ]),
], datatype="CompositeExamples")

page("combobox", "Combobox", "Autocomplete input and command palette with a list of suggestions.", ["shadcn/ui: combobox", "Cosmo: c:Combobox (Popover + Command)", "Web: a popover"], [
    example("Usage", """
        <c:Combobox PlaceholderText="Select framework..." SearchPlaceholder="Search framework..." Empty="No framework found.">
          <x:String>Next.js</x:String>
          <x:String>SvelteKit</x:String>
          <x:String>Nuxt.js</x:String>
          <x:String>Remix</x:String>
          <x:String>Astro</x:String>
        </c:Combobox>
        """, "Choosing an item selects it and closes the popover; choosing it again clears it."),
    example("Binding", """
        <StackPanel Spacing="8">
          <c:Combobox PlaceholderText="Select framework..." SearchPlaceholder="Search framework..." Empty="No framework found."
                      ItemsSource="{Binding Frameworks}" SelectedItem="{Binding Framework}" />
          <TextBlock Classes="sm muted" Text="{Binding FrameworkText}" />
        </StackPanel>
        """, "Items from the model, SelectedItem two way (Remix chosen by the model); objects take an ItemTemplate or DisplayMemberBinding."),
    example("Multiple", """
        <c:Combobox Width="320" PlaceholderText="Select frameworks..." SearchPlaceholder="Search framework..." SelectionMode="Multiple"
                    ItemsSource="{Binding Frameworks}" SelectedItems="{Binding ChosenFrameworks}" />
        """, "SelectionMode Multiple keeps the popover open and shows the chosen items as badges; SelectedItems binds to the model's collection."),
    table("API", [
        ("c:Combobox", "ItemsSource, ItemTemplate or DisplayMemberBinding, SelectedItem, SelectedItems, SelectionMode, IsDropDownOpen (two way), PlaceholderText, SearchPlaceholder, Empty."),
    ]),
], datatype="CompositeExamples")


DATA_TABLE = """
        <StackPanel Width="592">
              <DockPanel Margin="0,16">
            <Button DockPanel.Dock="Right" Classes="outline has-icon">
              <StackPanel><TextBlock Text="Columns" /><Viewbox Width="16" Height="16"><Path Theme="{DynamicResource CosmoIcon}" Stroke="{DynamicResource CosmoForegroundBrush}" Data="M6 9 l6 6 6-6" /></Viewbox></StackPanel>
              <Button.Flyout>
                <MenuFlyout Placement="BottomEdgeAlignedRight">
                  <MenuItem Header="Status" ToggleType="CheckBox" IsChecked="{Binding Source.Columns[1].IsVisible, Mode=TwoWay}" />
                  <MenuItem Header="Email" ToggleType="CheckBox" IsChecked="{Binding Source.Columns[2].IsVisible, Mode=TwoWay}" />
                  <MenuItem Header="Amount" ToggleType="CheckBox" IsChecked="{Binding Source.Columns[3].IsVisible, Mode=TwoWay}" />
                </MenuFlyout>
              </Button.Flyout>
            </Button>
            <TextBox Margin="0,0,8,0" PlaceholderText="Filter emails..." a:Web.Delay="300" Text="{Binding Source.FilterText}" />
          </DockPanel>
          <Border BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusMd}" ClipToBounds="True">
            <TableView ItemsSource="{Binding Source.Rows}" c:DataTable.SelectsRows="True" c:DataTable.State="{Binding Source.State}">
            <TableView.Columns>
              <TableViewColumn Width="32.84">
                <TableViewColumn.Header><CheckBox IsChecked="{Binding Source.AllPageRowsSelected}" AutomationProperties.Name="Select all" /></TableViewColumn.Header>
                <TableViewColumn.CellTemplate>
                  <DataTemplate x:DataType="m:Payment"><CheckBox IsChecked="{Binding IsSelected}" AutomationProperties.Name="Select row" /></DataTemplate>
                </TableViewColumn.CellTemplate>
              </TableViewColumn>
              <TableViewColumn Width="119.66" IsVisible="{Binding Source.Columns[1].IsVisible}">
                <TableViewColumn.Header>Status</TableViewColumn.Header>
                <TableViewColumn.CellTemplate>
                  <DataTemplate x:DataType="m:Payment"><TextBlock Text="{Binding Status, Converter={x:Static m:PaymentTable.Capitalize}}" /></DataTemplate>
                </TableViewColumn.CellTemplate>
              </TableViewColumn>
              <TableViewColumn Width="271.55" IsVisible="{Binding Source.Columns[2].IsVisible}">
                <TableViewColumn.Header><Button Classes="ghost has-icon" Command="{Binding Source.SortCommand}" CommandParameter="email"><StackPanel><TextBlock Text="Email" /><Viewbox Width="16" Height="16"><Path Theme="{DynamicResource CosmoIcon}" Stroke="{DynamicResource CosmoForegroundBrush}" Data="M21 16 l-4 4-4-4 M17 20V4 M3 8 l4-4 4 4 M7 4v16" /></Viewbox></StackPanel></Button></TableViewColumn.Header>
                <TableViewColumn.CellTemplate>
                  <DataTemplate x:DataType="m:Payment"><TextBlock Text="{Binding Email, Converter={x:Static m:PaymentTable.Lower}}" /></DataTemplate>
                </TableViewColumn.CellTemplate>
              </TableViewColumn>
              <TableViewColumn Width="100.23" IsVisible="{Binding Source.Columns[3].IsVisible}" HorizontalContentAlignment="Stretch">
                <TableViewColumn.Header><TextBlock Text="Amount" HorizontalAlignment="Right" /></TableViewColumn.Header>
                <TableViewColumn.CellTemplate>
                  <DataTemplate x:DataType="m:Payment"><TextBlock Text="{Binding Amount, Converter={x:Static m:PaymentTable.Dollars}}" FontWeight="Medium" HorizontalAlignment="Right" /></DataTemplate>
                </TableViewColumn.CellTemplate>
              </TableViewColumn>
              <TableViewColumn Width="65.72">
                <TableViewColumn.Header></TableViewColumn.Header>
                <TableViewColumn.CellTemplate>
                  <DataTemplate x:DataType="m:Payment"><Button Classes="ghost icon-sm" HorizontalAlignment="Left" AutomationProperties.Name="Open menu"><Viewbox Width="16" Height="16"><Path Theme="{DynamicResource CosmoIcon}" Stroke="{DynamicResource CosmoForegroundBrush}" Data="M11,12 A1,1 0 1 1 13,12 A1,1 0 1 1 11,12 Z M18,12 A1,1 0 1 1 20,12 A1,1 0 1 1 18,12 Z M4,12 A1,1 0 1 1 6,12 A1,1 0 1 1 4,12 Z" /></Viewbox><Button.Flyout>
          <MenuFlyout Placement="BottomEdgeAlignedRight">
            <MenuItem Header="Actions" Classes="label" />
            <MenuItem Header="Copy payment ID" />
            <Separator />
            <MenuItem Header="View customer" />
            <MenuItem Header="View payment details" />
          </MenuFlyout>
        </Button.Flyout></Button></DataTemplate>
                </TableViewColumn.CellTemplate>
              </TableViewColumn>
            </TableView.Columns>
            </TableView>
          </Border>
          <DockPanel Margin="0,16">
            <StackPanel DockPanel.Dock="Right" Orientation="Horizontal" Spacing="8">
              <Button Classes="outline sm" Content="Previous" Command="{Binding Source.PreviousPageCommand}" />
              <Button Classes="outline sm" Content="Next" Command="{Binding Source.NextPageCommand}" />
            </StackPanel>
            <TextBlock FontSize="14" LineHeight="20" VerticalAlignment="Center" Foreground="{DynamicResource CosmoMutedForegroundBrush}">
              <TextBlock.Text>
                <MultiBinding StringFormat="{}{0} of {1} row(s) selected.">
                  <Binding Path="Source.SelectedCount" />
                  <Binding Path="Source.FilteredCount" />
                </MultiBinding>
              </TextBlock.Text>
            </TextBlock>
          </DockPanel>
        </StackPanel>
        """

page("data-table", "Data Table", "Powerful table and datagrids built using TanStack Table.", ["shadcn/ui: data-table", "Avalonia: TableView", "Cosmo: DataTableSource, c:DataTable.SelectsRows, c:DataTable.State", "Web: <table>"], [
    example("Usage", DATA_TABLE, "Columns in XAML, the state in the view model's DataTableSource: filter, sort (the Email header), paging (five a page), row selection and column visibility."),
    table("API", [
        ("DataTableSource<T>", "Items, Rows (the page), Columns, FilterText, FilterColumn, SortColumn, SortDescending, SortCommand, PageIndex, PageSize, PreviousPageCommand, NextPageCommand, FilteredCount, SelectedCount, AllPageRowsSelected."),
        ("IDataTableRow", "IsSelected, which the row's checkbox binds two way."),
        ("DataTableColumn", "Key, Header, CanSort, CanHide, IsVisible (two way)."),
        ("c:DataTable.SelectsRows", "On a TableView: selected rows take the selected look."),
        ("c:DataTable.State", "On a TableView, bound to the source's State: the page, sort, filter, columns, selection and order travel across posts on the web."),
    ]),
], datatype="PaymentLedger")


def sidebar_example(collapsible="Icon", variant="Sidebar", bound=False):
    is_open = ' IsOpen="{Binding SidebarOpen}"' if bound else ""
    expanded = "{Binding PlaygroundOpen}" if bound else "True"
    width = "" if bound else ' Width="600"'
    return f"""
        <Border{width} Height="400" BorderThickness="1" BorderBrush="{{DynamicResource CosmoBorderBrush}}" CornerRadius="{{DynamicResource CosmoRadiusLg}}" ClipToBounds="True">
          <c:SidebarProvider{is_open}>
            <c:Sidebar Collapsible="{collapsible}" Variant="{variant}">
              <c:SidebarHeader>
                <c:SidebarMenu>
                  <c:SidebarMenuItem>
                    <c:SidebarMenuButton Classes="lg" Tooltip="Acme Inc">
                      <DockPanel>
                        <Border Width="32" Height="32" CornerRadius="{{DynamicResource CosmoRadiusLg}}" Background="{{DynamicResource CosmoSidebarPrimaryBrush}}">
                          <a:Icon Kind="gallery-vertical-end" Foreground="{{DynamicResource CosmoSidebarPrimaryForegroundBrush}}" />
                        </Border>
                        <StackPanel Margin="8,0,0,0" VerticalAlignment="Center">
                          <TextBlock Text="Acme Inc" FontWeight="Medium" />
                          <TextBlock Text="Enterprise" FontSize="12" />
                        </StackPanel>
                      </DockPanel>
                    </c:SidebarMenuButton>
                  </c:SidebarMenuItem>
                </c:SidebarMenu>
              </c:SidebarHeader>
              <c:SidebarContent>
                <c:SidebarGroup>
                  <c:SidebarGroupLabel Content="Platform" />
                  <c:SidebarMenu>
                    <c:SidebarMenuItem IsCollapsible="True" IsExpanded="{expanded}">
                      <c:SidebarMenuButton Tooltip="Playground" IsActive="True"><StackPanel><a:Icon Kind="square-terminal" /><TextBlock Text="Playground" /></StackPanel></c:SidebarMenuButton>
                      <c:SidebarMenuSub>
                        <c:SidebarMenuSubButton Content="History" />
                        <c:SidebarMenuSubButton Content="Starred" />
                      </c:SidebarMenuSub>
                    </c:SidebarMenuItem>
                    <c:SidebarMenuItem>
                      <c:SidebarMenuButton Tooltip="Models"><StackPanel><a:Icon Kind="bot" /><TextBlock Text="Models" /></StackPanel></c:SidebarMenuButton>
                    </c:SidebarMenuItem>
                    <c:SidebarMenuItem>
                      <c:SidebarMenuButton Tooltip="Documentation"><StackPanel><a:Icon Kind="book-open" /><TextBlock Text="Documentation" /></StackPanel></c:SidebarMenuButton>
                    </c:SidebarMenuItem>
                  </c:SidebarMenu>
                </c:SidebarGroup>
              </c:SidebarContent>
              <c:SidebarRail />
            </c:Sidebar>
            <c:SidebarInset>
              <StackPanel Margin="12,14" Spacing="12">
                <StackPanel Orientation="Horizontal" Spacing="8">
                  <c:SidebarTrigger />
                  <Separator Classes="vertical" Height="16" VerticalAlignment="Center" />
                  <TextBlock Classes="sm" Text="Playground" VerticalAlignment="Center" />
                </StackPanel>
                <Border Height="240" CornerRadius="{{DynamicResource CosmoRadiusXl}}" Background="{{DynamicResource CosmoMutedHalfBrush}}" />
              </StackPanel>
            </c:SidebarInset>
          </c:SidebarProvider>
        </Border>
        """


page("sidebar", "Sidebar", "A composable, themeable and customizable sidebar component.", ["shadcn/ui: sidebar", "Cosmo: c:SidebarProvider, c:Sidebar and its parts", "Web: <nav>"], [
    example("Collapsible to icons", sidebar_example(), "The trigger, the rail and Ctrl+B collapse it to its icons (with tips); under 768 px it is a sheet."),
    example("Floating", sidebar_example(variant="Floating"), "Variant Floating: a rounded, bordered panel 8 px in."),
    example("Inset", sidebar_example(variant="Inset"), "Variant Inset: the content is a card on the sidebar's color."),
    example("Offcanvas", sidebar_example(collapsible="Offcanvas"), "Collapsible Offcanvas: it slides out of the window."),
    example("Binding", f"""
        <StackPanel Spacing="12" MaxWidth="600">
{indent(dedent(sidebar_example(bound=True)).strip(chr(10)), "          ")}
          <WrapPanel ItemSpacing="12" LineSpacing="8">
            <Button Classes="outline sm" Content="Toggle from the model" Command="{{Binding ToggleSidebarCommand}}" />
            <TextBlock Classes="sm muted" Text="{{Binding SidebarText}}" VerticalAlignment="Center" />
            <TextBlock Classes="sm muted" Text="{{Binding PlaygroundText}}" VerticalAlignment="Center" />
          </WrapPanel>
        </StackPanel>
        """, "IsOpen and the Playground item's IsExpanded bind two way; a StateStore (SidebarFileStore on the desktop) keeps IsOpen between runs under StateKey."),
    table("API", [
        ("c:SidebarProvider", "IsOpen and IsMobileOpen (two way), IsMobile, MobileBreakpoint (768), ToggleGesture (Ctrl+B), ToggleCommand, StateStore, StateKey."),
        ("c:Sidebar", "Side (Left, Right), Variant (Sidebar, Floating, Inset), Collapsible (Offcanvas, Icon, None); SidebarHeader, SidebarContent, SidebarFooter, SidebarRail."),
        ("c:SidebarGroup", "SidebarGroupLabel and a SidebarMenu; the icon-hidden class hides it while collapsed to icons."),
        ("c:SidebarMenuItem", "IsCollapsible, IsExpanded (two way): a SidebarMenuButton (IsActive, Tooltip, sm, lg, outline), SidebarMenuSub, SidebarMenuAction, SidebarMenuBadge."),
        ("c:SidebarTrigger, c:SidebarInset", "The toggle button and the main area beside the sidebar."),
    ]),
], datatype="CompositeExamples")

NAV_LINKS = "\n".join(f"""                  <Button Classes="navigation-menu-link" Width="246">
                    <StackPanel>
                      <TextBlock Classes="sm" Text="{title}" FontWeight="Medium" />
                      <TextBlock Classes="sm muted" Text="{text}" TextWrapping="Wrap" MaxLines="2" TextTrimming="CharacterEllipsis" />
                    </StackPanel>
                  </Button>""" for title, text in [("Alert Dialog", "A modal dialog that interrupts the user with important content and expects a response."), ("Hover Card", "For sighted users to preview content available behind a link."), ("Progress", "Displays an indicator showing the completion progress of a task."), ("Tabs", "A set of layered sections of content, displayed one at a time.")])

page("navigation-menu", "Navigation Menu", "A collection of links for navigating websites.", ["shadcn/ui: navigation-menu", "Cosmo: c:NavigationMenu, c:NavigationMenuTrigger", "Web: popovers"], [
    example("Usage", f"""
        <Panel Height="260" VerticalAlignment="Top">
          <c:NavigationMenu VerticalAlignment="Top">
            <c:NavigationMenuTrigger Content="Components">
              <c:NavigationMenuTrigger.Flyout>
                <Flyout>
                  <UniformGrid Columns="2" Width="500" ColumnSpacing="8" RowSpacing="8">
{NAV_LINKS}
                  </UniformGrid>
                </Flyout>
              </c:NavigationMenuTrigger.Flyout>
            </c:NavigationMenuTrigger>
            <HyperlinkButton Classes="navigation-menu-trigger" Content="Docs" a:Web.Href="/components/navigation-menu" />
          </c:NavigationMenu>
        </Panel>
        """, "Hover a trigger to open its content after Radix's 200 ms, or click it; Left and Right move between items."),
    example("Moving between items", """
        <Panel Height="200" VerticalAlignment="Top">
          <c:NavigationMenu VerticalAlignment="Top">
            <c:NavigationMenuTrigger Content="Overview">
              <c:NavigationMenuTrigger.Flyout>
                <Flyout>
                  <StackPanel Width="240" Spacing="4">
                    <TextBlock Classes="sm" FontWeight="Medium" Text="Introduction" />
                    <TextBlock Classes="sm muted" TextWrapping="Wrap" Text="Re-usable components built using Radix UI and Tailwind CSS." />
                  </StackPanel>
                </Flyout>
              </c:NavigationMenuTrigger.Flyout>
            </c:NavigationMenuTrigger>
            <c:NavigationMenuTrigger Content="Primitives">
              <c:NavigationMenuTrigger.Flyout>
                <Flyout>
                  <StackPanel Width="240" Spacing="4">
                    <TextBlock Classes="sm" FontWeight="Medium" Text="Alert Dialog" />
                    <TextBlock Classes="sm muted" TextWrapping="Wrap" Text="A modal dialog that interrupts the user with important content." />
                  </StackPanel>
                </Flyout>
              </c:NavigationMenuTrigger.Flyout>
            </c:NavigationMenuTrigger>
          </c:NavigationMenu>
        </Panel>
        """, "With one item open, move onto the next: its content slides in from the side of the one it replaces while that one slides out, as Radix's data-motion plays them."),
    example("Binding", """
        <StackPanel Spacing="12">
          <c:NavigationMenu>
            <c:NavigationMenuTrigger Content="Getting started">
              <c:NavigationMenuTrigger.Flyout>
                <Flyout>
                  <StackPanel Width="400">
                  <Button Classes="navigation-menu-link" Content="Components" Command="{Binding NavigateCommand}" CommandParameter="/docs/components" />
                  <ItemsControl ItemsSource="{Binding Links}">
                    <ItemsControl.ItemTemplate>
                      <DataTemplate x:DataType="m:MenuLink">
                        <Button Classes="navigation-menu-link" Command="{Binding $parent[ItemsControl].((m:StructureExamples)DataContext).NavigateCommand}" CommandParameter="{Binding Page}">
                          <StackPanel>
                            <TextBlock Classes="sm" Text="{Binding Title}" FontWeight="Medium" />
                            <TextBlock Classes="sm muted" Text="{Binding Description}" TextWrapping="Wrap" />
                          </StackPanel>
                        </Button>
                      </DataTemplate>
                    </ItemsControl.ItemTemplate>
                  </ItemsControl>
                  </StackPanel>
                </Flyout>
              </c:NavigationMenuTrigger.Flyout>
            </c:NavigationMenuTrigger>
          </c:NavigationMenu>
          <TextBlock Classes="sm muted" Text="{Binding CurrentPage, StringFormat='Current page: {0}'}" />
        </StackPanel>
        """, "Links from the model through an item template, each running the model's command with its page; the content closes when a link is chosen."),
    table("API", [
        ("c:NavigationMenu", "The row of items: DelayDuration (200 ms), CloseDelay (150 ms), Time (TimeProvider)."),
        ("c:NavigationMenuTrigger", "A trigger whose Flyout is its content, opened under its start."),
        ("Button.navigation-menu-link", "A link of the content: title and description, accent on hover."),
        ("HyperlinkButton.navigation-menu-trigger", "A plain link in the row, as shadcn's navigationMenuTriggerStyle."),
    ]),
], datatype="StructureExamples")

page("menubar", "Menubar", "A visually persistent menu common in desktop applications that provides quick access to a consistent set of commands.", ["shadcn/ui: menubar", "Avalonia: Menu, MenuItem", "Web: <menu> with popovers"], [
    example("Usage", """
        <Panel Height="240" VerticalAlignment="Top" Width="400">
          <Menu VerticalAlignment="Top">
            <MenuItem Header="File">
              <MenuItem Header="New Tab" InputGesture="Ctrl+T" />
              <MenuItem Header="New Window" InputGesture="Ctrl+N" />
              <MenuItem Header="New Incognito Window" IsEnabled="False" />
              <Separator />
              <MenuItem Header="Share">
                <MenuItem Header="Email link" />
                <MenuItem Header="Messages" />
                <MenuItem Header="Notes" />
              </MenuItem>
              <Separator />
              <MenuItem Header="Print..." InputGesture="Ctrl+P" />
            </MenuItem>
            <MenuItem Header="Edit">
              <MenuItem Header="Undo" InputGesture="Ctrl+Z" />
              <MenuItem Header="Redo" InputGesture="Ctrl+Shift+Z" />
              <Separator />
              <MenuItem Header="Cut" />
              <MenuItem Header="Copy" />
              <MenuItem Header="Paste" />
            </MenuItem>
            <MenuItem Header="Profiles">
              <MenuItem Header="Andy" ToggleType="Radio" GroupName="gallery-profiles" />
              <MenuItem Header="Benoit" ToggleType="Radio" GroupName="gallery-profiles" IsChecked="True" />
              <MenuItem Header="Luis" ToggleType="Radio" GroupName="gallery-profiles" />
            </MenuItem>
          </Menu>
        </Panel>
        """, "Left and Right move between menus, Down enters a menu, Escape closes it."),
    example("Binding", """
        <StackPanel Spacing="12" Width="400">
          <Menu>
            <MenuItem Header="File">
              <MenuItem Header="New Tab" Command="{Binding ActCommand}" CommandParameter="New Tab" />
              <MenuItem Header="Print..." Command="{Binding ActCommand}" CommandParameter="Print" />
            </MenuItem>
            <MenuItem Header="View">
              <MenuItem Header="Always Show Bookmarks Bar" ToggleType="CheckBox" IsChecked="{Binding ShowBookmarks, Mode=TwoWay}" />
            </MenuItem>
          </Menu>
          <TextBlock Classes="sm muted" Text="{Binding LastAction, StringFormat='Last action: {0}'}" />
        </StackPanel>
        """, "Items run the model's command with their parameter; the checkable item's state is the model's, two way."),
    table("API", [
        ("Menu", "The bar: its top-level items open their menus 8 px below."),
        ("MenuItem", "Header, InputGesture, Command, ToggleType (CheckBox, Radio), Classes inset and destructive, submenus."),
    ]),
], datatype="StructureExamples")

import charts

CHART_SECTIONS = [
    ("Area Chart", "chart-area-default", "An area of one series with the line indicator in its tooltip."),
    ("Area Chart - Gradient", "chart-area-gradient", "Stacked areas filled with gradients from the chart tokens (CosmoChart1TopColor to CosmoChart1BottomColor)."),
    ("Area Chart - Stacked", "chart-area-stacked", None),
    ("Area Chart - Interactive", "chart-area-interactive", "The select chooses the time range; the chart's Data binds to the model's filtered days, and the legend is ChartLegendContent."),
    ("Bar Chart", "chart-bar-default", None),
    ("Bar Chart - Multiple", "chart-bar-multiple", "The dashed indicator."),
    ("Bar Chart - Horizontal", "chart-bar-horizontal", "Layout=Vertical with a hidden number axis."),
    ("Bar Chart - Label", "chart-bar-label", None),
    ("Bar Chart - Interactive", "chart-bar-interactive", "The header's toggles are a ListBox with the chart-toggles class; each choice shows its series' chart."),
    ("Line Chart", "chart-line-default", None),
    ("Line Chart - Multiple", "chart-line-multiple", None),
    ("Line Chart - Dots", "chart-line-dots", None),
    ("Line Chart - Interactive", "chart-line-interactive", None),
    ("Pie Chart", "chart-pie-simple", "Each slice's color is a Cell of the palette."),
    ("Pie Chart - Donut", "chart-pie-donut", None),
    ("Pie Chart - Donut with Text", "chart-pie-donut-text", "The center text is a Label's ContentTemplate."),
    ("Pie Chart - Legend", "chart-pie-legend", "ChartLegendContent with Spacing 8 and ItemBasis 0.25 (shadcn's gap-2 *:basis-1/4)."),
    ("Pie Chart - Default Legend", "chart-pie-default-legend", "A plain p:Legend under CosmoChartsTheme is ChartLegendContent; give it a Template, or set Template and ContentTemplate to {x:Null} for Pictor's default."),
    ("Radar Chart", "chart-radar-default", None),
    ("Radar Chart - Dots", "chart-radar-dots", None),
    ("Radar Chart - Multiple", "chart-radar-multiple", None),
    ("Radial Chart", "chart-radial-simple", None),
    ("Radial Chart - Text", "chart-radial-text", None),
    ("Radial Chart - Stacked", "chart-radial-stacked", None),
    ("Tooltip - Default", "chart-tooltip-default", "ChartTooltipContent: the label, then a line per series with its dot, name and value."),
    ("Tooltip - Line Indicator", "chart-tooltip-indicator-line", "Indicator=Line."),
    ("Tooltip - No Indicator", "chart-tooltip-indicator-none", "HideIndicator=True."),
    ("Tooltip - Formatters", "chart-tooltip-formatters", "A line per series however many there are; ChartConfig names and colors the series by data key; converters write the ticks (TickFormatter), the values (Formatter, the series' name as the parameter) and the label (LabelFormatter)."),
]

def chart_example(name):
    body = charts.FIXTURES.get(name) or charts.EXAMPLES[name]
    for model, path in (("ChartSamples", "Samples"), ("ChartInteractive", "Interactive")):
        context = f"<Border.DataContext><m:{model} /></Border.DataContext>"
        if context in body:
            body = body.replace("\n" + " " * 10 + context, "").replace(f'<Border x:DataType="m:{model}">', f'<Border MaxWidth="592" DataContext="{{Binding {path}}}" x:DataType="m:{model}">', 1)
    return body


page("chart", "Chart", "Beautiful charts. Built using Aioros.Pictor. Copy and paste into your apps.", ["shadcn/ui: chart", "Aioros.Pictor: Recharts' charts in XAML", "Web: <svg>"], [
    *[example(title, chart_example(name), text) for title, name, text in CHART_SECTIONS],
    table("API", [
        ("c:CosmoChartsTheme", "Add after CosmoTheme in the application's styles: chart text in Cosmo's font and the chart-toggles list box."),
        ("c:ChartTooltipContent", "A tooltip's ContentTemplate: Indicator (Dot, Line, Dashed), HideLabel, HideIndicator; the label and a line per series, bringing its lines' template."),
        ("c:ChartConfig", "shadcn's ChartConfig as a resource of c:ChartConfigItem (Key, Label, Color: a theme key such as PictorPalette1, or a CSS color), given to a chart with c:ChartConfig.Config: each series of that data key takes the label as its Name and the color as its fill or stroke."),
        ("TickFormatter, Formatter, LabelFormatter", "Pictor's formatter functions: value converters on an axis and a tooltip (a function from code through FuncValueConverter); composite formats (TickFormat, Format, LabelFormat) stay for what a format says. A converter class with no settings keeps the page's plan on the web."),
        ("c:ChartLegendContent", "A legend's Template: the entries in a centered row; Spacing (16) and ItemBasis."),
        ("c:ChartLegendItem", "A legend's ContentTemplate: an 8 px square of the series' color and its name."),
        ("PictorPalette1 to PictorPalette5", "The chart tokens --chart-1 to --chart-5 as Pictor's palette, in every color theme, light and dark."),
        ("CosmoChart1TopColor, CosmoChart1BottomColor", "A chart token at 0.8 and 0.1, the stops of the area gradients (1 to 5)."),
    ]),
], datatype="ChartExamples", namespaces=' xmlns:p="https://aioros.dev/pictor"')

import chat


def chat_example(title, name, text=None):
    xaml = chat.FIXTURES[name].replace('Margin="0,48" ', "")
    root = re.match(r"\n*( *)", xaml).group(1)
    shown = "\n".join(line.removeprefix(root) for line in xaml.split("\n"))
    return example(title, xaml, text, shown=shown)


page("marker", "Marker", "Displays an inline status, system note, bordered row, or labeled separator in a conversation.", ["shadcn/ui: marker", "Avalonia: ContentControl", "Web: <div>"], [
    chat_example("Usage", "marker-demo", "c:Marker with an Icon; a Spinner as the icon shows work in progress."),
    chat_example("Variants", "marker-variants", "The separator and border classes."),
    chat_example("Icon", "marker-icon", "The vertical class puts the icon above the content."),
    chat_example("Border", "marker-border"),
    chat_example("Shimmer", "marker-shimmer", "a:Web.Shimmer sweeps a highlight through the text on both targets, still when the visitor asks for less motion."),
    chat_example("Status", "marker-status", "A marker of work in progress: a spinner and a shimmering text."),
    chat_example("Link button", "marker-link-button", "c:MarkerLink is a link with the marker's look; on the web an <a href>."),
    table("API", [
        ("Content", "The marker's text or content."),
        ("Icon", "The 16 px icon before the content."),
        ("c:MarkerLink", "A marker that is a link (NavigateUri), its Icon before its content."),
        ("separator, border, vertical", "A line each side of the content, a bottom border, the icon above the content."),
    ]),
])

page("bubble", "Bubble", "Displays conversational content in a message bubble. Supports variants, alignment, grouping and reactions.", ["shadcn/ui: bubble", "Avalonia: ContentControl", "Web: <div>"], [
    chat_example("Usage", "bubble-demo", "Bubbles take at most 80% of their row; c:BubbleGroup stacks them 8 px apart."),
    chat_example("Variants", "bubble-variants", "secondary, muted, tinted, outline, ghost and destructive; the tinted color is generated from --primary in every theme."),
    chat_example("Alignment", "bubble-alignment", "The end class puts a bubble at the end of its row."),
    chat_example("Group", "bubble-group-demo", "Reactions overlap a bubble's bottom edge, 12 px from its end (its start with reactions-start)."),
    chat_example("Link and button", "bubble-link-button", "A bubble's content can be a link or a button, which takes the bubble's look."),
    chat_example("Collapsible", "bubble-collapsible", "c:Collapsible shows a preview and the whole message behind Show more; on the web the browser switches it with no request."),
    chat_example("Markdown", "bubble-markdown", "c:Markdown writes its Text as paragraphs, lists, inline code and code blocks, 16 px apart."),
    table("API", [
        ("Content", "The bubble's text or content."),
        ("Reactions", "The reactions shown on the bubble's bottom edge."),
        ("secondary, muted, tinted, outline, ghost, destructive", "The variants; the primary color otherwise."),
        ("end, reactions-start", "The bubble at the end of its row; its reactions at its start."),
        ("c:Collapsible, c:CollapsibleTrigger", "Parts shown while open or closed (Collapsible.Shows), switched by the trigger; IsOpen binds two way."),
        ("c:Markdown: Text, LineNumbers", "CommonMark written as Cosmo's controls; fenced blocks are code blocks, highlighted by the CosmoCodeHighlighter resource."),
    ]),
])

page("message", "Message", "Displays a message in a conversation, with optional avatar, header, footer, and alignment.", ["shadcn/ui: message", "Avalonia: ContentControl", "Web: <div>"], [
    chat_example("Usage", "message-demo", "c:Message with an Avatar and a c:MessageContent; the end class reverses the row, a c:MessageFooter raises the avatar 32 px."),
    chat_example("Group", "message-group", "c:MessageGroup stacks messages 8 px apart; an empty avatar keeps the bubbles aligned."),
    chat_example("Header and footer", "message-header-footer"),
    chat_example("Avatar", "message-avatar"),
    table("API", [
        ("Avatar", "The avatar at the row's bottom edge, 8 px from the content."),
        ("end", "The avatar on the far side and the content's parts at its end."),
        ("c:MessageContent, c:MessageHeader, c:MessageFooter, c:MessageGroup", "The content 10 px apart, its header and footer, and messages 8 px apart."),
    ]),
])

page("attachment", "Attachment", "Displays a file or image attachment with media, metadata, upload state, and actions.", ["shadcn/ui: attachment", "Avalonia: TemplatedControl", "Web: <div>"], [
    chat_example("Usage", "attachment-demo", "The vertical and image classes show pictures above their names; c:AttachmentGroup scrolls its attachments sideways."),
    chat_example("States", "attachment-states", "State: Idle (dashed), Uploading, Processing, Error and Done, each a pseudo-class."),
    chat_example("Sizes", "attachment-sizes", "The sm and xs classes."),
    chat_example("Group", "attachment-group", "The group fades its edges as it scrolls (a:Web.ScrollFade x)."),
    chat_example("Trigger", "attachment-trigger", "An attachment can be a button that opens the file."),
    chat_example("Image", "attachment-image", "The image class fills the media with a picture."),
    table("API", [
        ("Media, Title, Description, Actions", "The file's icon or image, its name, its details and its buttons."),
        ("State", "Done, Idle, Uploading, Processing or Error."),
        ("sm, xs, vertical, image", "The sizes, the media above the texts, an image filling the media."),
    ]),
])

page("message-scroller", "Message Scroller", "A scroll container for chat transcripts that opens at the newest message, follows new messages and offers a scroll-to-end button.", ["shadcn/ui: message-scroller", "Avalonia: ContentControl", "Web: <div>"], [
    chat_example("Usage", "message-scroller-end", "c:MessageScroller opens at its end and keeps the newest message in view while it is there."),
    chat_example("Start", "message-scroller-start", "DefaultScrollPosition=\"Start\" opens at the first message; the button scrolls to the end. The transcript fades at its bottom while there is more below."),
    table("API", [
        ("Content", "The transcript: messages stacked in a panel."),
        ("DefaultScrollPosition", "End (the default) or Start."),
        ("ScrollToEnd(), ScrollToStart()", "Scroll to the newest or the first message; at the end the scroller follows new messages."),
        (":end, :following", "At its end (the button hidden); keeping the newest message in view (the scroll bar's gutter hidden)."),
    ]),
])

import codeblocks
import traces

page("code-block", "Code Block", "Displays code with a header, actions, line numbers, highlighted lines and diffs, highlighted when the XAML is written.", ["shadcn/ui: code-block (radix-rhea)", "Avalonia: ContentControl", "Web: <div>"], [
    example("Usage", codeblocks.FIXTURES["code-block-demo"], "c:CodeBlock with a Title, Actions and a c:CodeBlockContent of c:CodeBlockLines; the tokens are Runs on Cosmo's code brushes (github-light and github-dark), written by shiki with the page."),
    example("Basic", codeblocks.FIXTURES["code-block-basic"]),
    example("Header", codeblocks.FIXTURES["code-block-header"], "c:CodeBlockCopyButton copies its Value and shows a check for two seconds."),
    example("Line numbers", codeblocks.FIXTURES["code-block-line-numbers"], "LineNumbers numbers the lines from StartLine."),
    example("Highlighted lines", codeblocks.FIXTURES["code-block-highlighted-lines"], "The highlighted class tints a line."),
    example("Diff", codeblocks.FIXTURES["code-block-diff"], "The added and removed classes mark a line with + or −."),
    example("Destructive", codeblocks.FIXTURES["code-block-destructive"], "The destructive class; Wrap wraps long lines."),
    example("Size", codeblocks.FIXTURES["code-block-size"].replace('<StackPanel Spacing="16">', '<StackPanel Spacing="16" MaxWidth="448">', 1)
            .replace(' Width="448" HorizontalAlignment="Left"', ""), "The sm class."),
    example("Highlighting", codeblocks.FIXTURES["code-block-highlighting"]),
    example("Actions", phone_wide(codeblocks.FIXTURES["code-block-actions"]), "A wrap toggle (a c:Collapsible whose open state wraps the lines) beside the copy button."),
    example("Floating actions", phone_wide(codeblocks.FIXTURES["code-block-floating-actions"]), "The floating class shows the actions over the code while it is pointed at or focused."),
    example("Show more", phone_wide(codeblocks.FIXTURES["code-block-show-more"]), "A collapsible block 256 px high until Show more opens it."),
    example("Show more and scroll", phone_wide(codeblocks.FIXTURES["code-block-show-more-scroll"])),
    example("Height", phone_wide(codeblocks.FIXTURES["code-block-height"]), "MaxHeight scrolls the code inside the block; the code fades at its scrolled edges."),
    example("Start line", phone_wide(codeblocks.FIXTURES["code-block-start-line"]), "StartLine numbers an excerpt from its first line."),
    example("Wrap", phone_wide(codeblocks.FIXTURES["code-block-wrap"])),
    example("Languages", phone_wide(codeblocks.FIXTURES["code-block-languages"])),
    example("Tabs", phone_wide(codeblocks.FIXTURES["code-block-tabs"]), "Tabs switch the language shown."),
    example("ANSI", phone_wide(codeblocks.FIXTURES["code-block-ansi"]), "Terminal output in the ANSI colors of every theme."),
    example("Right to left", phone_wide(codeblocks.FIXTURES["code-block-rtl"]), "The header mirrors in a right-to-left block; the code stays left to right."),
    example("Typeset", phone_wide(codeblocks.FIXTURES["code-block-typeset"]), "Code blocks and inline code in prose."),
    example("Markdown", phone_wide(played(codeblocks.FIXTURES["code-block-markdown"], "CodeBlockMarkdownExample", "Markdown")), "c:Markdown writes fenced blocks as code blocks; Replay streams the answer in, its fence highlighted as far as it has come."),
    example("Chat", phone_wide(codeblocks.FIXTURES["code-block-chat"])),
    example("Streaming", phone_wide(played(codeblocks.FIXTURES["code-block-streaming"], "CodeBlockStreaming", "Streaming")), "Replay streams the blocks in again from the page's model, three characters every 16 ms; on the web the page streams each change."),
    example("In a trace", phone_wide(traces.FIXTURES["code-block-trace"])),
    table("API", [
        ("Title, Actions, Footer", "The header's title and buttons (the code-block class) and the row below the code."),
        ("destructive, sm", "The destructive tint and the smaller size."),
        ("c:CodeBlockContent: LineNumbers, StartLine, Wrap", "Numbered lines from StartLine; long lines wrapped instead of scrolling."),
        ("c:CodeBlockLine: highlighted, added, removed", "A tinted line, an added line (+) and a removed line (−)."),
        ("floating", "The actions over the code, shown under the pointer or the focus."),
    ]),
], datatype="CodeBlockPlayback", namespaces=' a:Web.Live="True"')

page("trace", "Trace", "Displays an agent's steps as compact lines, with status, details and collapsible steps.", ["shadcn/ui: trace (radix-rhea)", "Avalonia: ContentControl, Expander", "Web: <div>, <details>"], [
    example("Static", traces.FIXTURES["trace-static"], "c:Trace stacks c:TraceLines 8 px apart; a line's Icon, title and Description."),
    example("Status", traces.FIXTURES["trace-status"], "Status: Done, Running, Error and Pending, each a pseudo-class."),
    example("Description", traces.FIXTURES["trace-description"], "A long title is cut short with an ellipsis; the description keeps its width."),
    example("Group", traces.FIXTURES["trace-group"]),
    example("Collapsible", traces.FIXTURES["trace-collapsible"], "An Expander with the trace class opens a step below its trigger."),
    example("Steps", traces.FIXTURES["trace-steps"], "Steps inside a step; Trailing puts an icon at a line's end."),
    example("Link", traces.FIXTURES["trace-link"], "A line's title can be a link."),
    example("Dialog", traces.FIXTURES["trace-dialog"], "A line's title can be a button that opens a dialog with the step's details."),
    example("Sheet", traces.FIXTURES["trace-sheet"]),
    example("Commit", traces.FIXTURES["trace-commit"]),
    example("Open", traces.FIXTURES["trace-open"], "A button opens a step from outside the trace."),
    example("Sources", traces.FIXTURES["trace-sources"]),
    example("Turn", traces.FIXTURES["trace-turn"], "An error step marks its collapsible step's trigger red."),
    example("Thinking", played(traces.FIXTURES["trace-thinking"], "TraceThinkingExample", "Thinking"), "Send message streams the reasoning, open while it runs, then the answer; Reset clears it."),
    example("Tool calls", played(traces.FIXTURES["trace-tool-calls"], "TraceToolCallsExample", "ToolCalls")),
    example("Demo", played(traces.FIXTURES["trace-demo"], "TraceDemoExample", "Demo")),
    table("API", [
        ("c:TraceLine: Icon, Description, Trailing", "The 16 px icon, the detail after the title and an icon at the end; the content is the title."),
        ("Status", "Done, Running, Error or Pending."),
        ("Expander.trace", "A collapsible step: the header is its trigger, the content opens behind a left line."),
    ]),
], datatype="TracePlayback", namespaces=' a:Web.Live="True"')

import questionnaires

questionnaire_names = set()


def questionnaire_example(title, name, text=None):
    def rename(match):
        unique, n = match.group(1), 2
        while unique in questionnaire_names:
            unique, n = f"{match.group(1)}-{n}", n + 1
        questionnaire_names.add(unique)
        return f'<c:QuestionnaireItem Name="{unique}"'
    body = re.sub(r'\n *' + re.escape(questionnaires.MODEL), '', questionnaires.FIXTURES[name])
    return example(title, phone_wide(re.sub(r'<c:QuestionnaireItem Name="([^"]+)"', rename, body)), text)


page("questionnaire", "Questionnaire", "Asks one question at a time, with choices, a freeform answer, shortcuts, validation and navigation.", ["shadcn/ui: questionnaire (radix-rhea)", "Avalonia: ItemsControl, RadioButton, CheckBox", "Web: <input type=radio>"], [
    questionnaire_example("Usage", "questionnaire-demo", "c:Questionnaire shows its c:QuestionnaireItems one at a time; Next checks a required question, Skip leaves an optional one. On the web the choices are native radio buttons in their cards."),
    questionnaire_example("Multiple", "questionnaire-multiple", "c:QuestionnaireCheck gives several answers."),
    questionnaire_example("Freeform", "questionnaire-freeform", "c:QuestionnaireInput takes a typed answer; Shortcuts=\"Letters\" checks a choice with its key."),
    questionnaire_example("Progress", "questionnaire-progress", "ProgressFormat and the segmented class."),
    questionnaire_example("Navigation state", "questionnaire-navigation-state", "RequiresAnswer keeps Next disabled until the question is answered, in the browser with no request."),
    questionnaire_example("Skip", "questionnaire-skip"),
    questionnaire_example("Shortcuts", "questionnaire-shortcuts", "Shortcuts bound to the host: none, letters or numbers."),
    questionnaire_example("Validation", "questionnaire-validation", "SubmitCommand checks the answers together and gives a question its Message."),
    questionnaire_example("Conditional", "questionnaire-conditional", "A question enabled by an earlier answer joins the questions."),
    questionnaire_example("Controlled", "questionnaire-controlled", "CurrentIndex bound to the host."),
    questionnaire_example("Resume", "questionnaire-resume", "Opens on a later question with earlier answers; ResetContent restores them."),
    questionnaire_example("Card", "questionnaire-card", "The card class."),
    questionnaire_example("Animated", "questionnaire-animated", "The animated class lets a question enter as it is shown."),
    questionnaire_example("Dialog", "questionnaire-dialog"),
    table("API", [
        ("CurrentIndex, ShowProgress, Shortcuts", "The question shown, the progress line, and the choices' keys (Letters or Numbers)."),
        ("PreviousContent, SkipContent, NextContent, SubmitContent", "The actions' contents."),
        ("Submitted, SubmitCommand", "Raised and run on a valid Submit, with the answers by question name."),
        ("c:QuestionnaireItem: Title, Description, IsRequired", "A question; an optional one can be skipped."),
        ("c:QuestionnaireChoice, c:QuestionnaireCheck: Value, Description", "A choice of one answer or of several."),
        ("ProgressFormat, Segments, RequiresAnswer", "The progress text ({0} of {1}), one flag per question for the segmented class, and Next disabled until an answer."),
        ("ResetContent, Reset()", "A button restoring the answers the questionnaire opened with."),
        ("c:QuestionnaireItem: Message, IsEnabled", "An error the host gives after checking the answers; a disabled question is left out."),
        ("card, segmented, secondary-next, animated", "The card layout, the progress in bars, a secondary Next, the questions' entrance."),
    ]),
], datatype="QuestionnaireExamples")

import blocks
import shells
blocks.write_pages(ROOT.parent)
shells.write_pages(ROOT.parent)

import fixtures
fixtures.write()

import index
index.write()
