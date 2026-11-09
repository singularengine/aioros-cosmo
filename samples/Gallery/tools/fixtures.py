"""Writes the batch 4 reference fixtures once for both targets: the Gallery's /fidelity/{fixture} pages (the web) and the
desktop tests' fixture templates (tests/Aioros.Cosmo.Tests/Display/Fixtures.axaml), from the same XAML."""
import re
from pathlib import Path
from textwrap import dedent, indent

import blocks
import shells
import charts
import chat
import codeblocks
import traces
import questionnaires

ROOT = Path(__file__).resolve().parents[3]
PAGES = ROOT / "samples" / "Gallery" / "Gallery" / "Pages" / "fidelity"
TESTS = ROOT / "tests" / "Aioros.Cosmo.Tests" / "Display" / "Fixtures.axaml"

ICONS = {
    "circle-check": "M2,12 A10,10 0 1 1 22,12 A10,10 0 1 1 2,12 Z M9,12 L11,14 L15,10",
    "popcorn": "M18 8a2 2 0 0 0 0-4 2 2 0 0 0-4 0 2 2 0 0 0-4 0 2 2 0 0 0-4 0 2 2 0 0 0 0 4 M10,22 L9,8 M14,22 L15,8 M20 8c.5 0 .9.4.8 1l-2.6 12c-.1.5-.7 1-1.2 1H7c-.6 0-1.1-.4-1.2-1L3.2 9c-.1-.6.3-1 .8-1Z",
    "circle-alert": "M2,12 A10,10 0 1 1 22,12 A10,10 0 1 1 2,12 Z M12,8 L12,12 M12,16 L12.01,16",
    "folder-code": "M11 19h-6a2 2 0 0 1 -2 -2v-11a2 2 0 0 1 2 -2h4l3 3h7a2 2 0 0 1 2 2v4 M20 21l2 -2l-2 -2 M17 17l-2 2l2 2",
    "cloud": "M6.657 18c-2.572 0 -4.657 -2.007 -4.657 -4.483c0 -2.475 2.085 -4.482 4.657 -4.482c.393 -1.762 1.794 -3.2 3.675 -3.773c1.88 -.572 3.956 -.193 5.444 1c1.488 1.19 2.162 3.007 1.77 4.769h.99c1.913 0 3.464 1.56 3.464 3.486c0 1.927 -1.551 3.487 -3.465 3.487h-11.878",
    "arrow-up-right": "M7,7 L17,7 L17,17 M7,17 L17,7",
    "badge-check": "M3.85 8.62a4 4 0 0 1 4.78-4.77 4 4 0 0 1 6.74 0 4 4 0 0 1 4.78 4.78 4 4 0 0 1 0 6.74 4 4 0 0 1-4.77 4.78 4 4 0 0 1-6.75 0 4 4 0 0 1-4.78-4.77 4 4 0 0 1 0-6.76Z M9,12 L11,14 L15,10",
    "chevron-right": "M9,18 L15,12 L9,6",
    "arrow-left": "M12,19 L5,12 L12,5 M19,12 L5,12",
    "ellipsis": "M11,12 A1,1 0 1 1 13,12 A1,1 0 1 1 11,12 Z M18,12 A1,1 0 1 1 20,12 A1,1 0 1 1 18,12 Z M4,12 A1,1 0 1 1 6,12 A1,1 0 1 1 4,12 Z",
    "plus": "M5,12 L19,12 M12,5 L12,19",
    "minus": "M5,12 L19,12",
    "slash": "M22,2 L2,22",
    "calendar": "M8 2v4 M16 2v4 M5,4 L19,4 A2,2 0 0 1 21,6 L21,20 A2,2 0 0 1 19,22 L5,22 A2,2 0 0 1 3,20 L3,6 A2,2 0 0 1 5,4 Z M3 10h18",
    "smile": "M2,12 A10,10 0 1 1 22,12 A10,10 0 1 1 2,12 Z M8 14s1.5 2 4 2 4-2 4-2 M9,9 L9.01,9 M15,9 L15.01,9",
    "calculator": "M6,2 L18,2 A2,2 0 0 1 20,4 L20,20 A2,2 0 0 1 18,22 L6,22 A2,2 0 0 1 4,20 L4,4 A2,2 0 0 1 6,2 Z M8,6 L16,6 M16,14 L16,18 M16 10h.01 M12 10h.01 M8 10h.01 M12 14h.01 M8 14h.01 M12 18h.01 M8 18h.01",
    "user": "M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2 M8,7 A4,4 0 1 1 16,7 A4,4 0 1 1 8,7 Z",
    "credit-card": "M4,5 L20,5 A2,2 0 0 1 22,7 L22,17 A2,2 0 0 1 20,19 L4,19 A2,2 0 0 1 2,17 L2,7 A2,2 0 0 1 4,5 Z M2,10 L22,10",
    "settings": "M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z M9,12 A3,3 0 1 1 15,12 A3,3 0 1 1 9,12 Z",
    "square-terminal": "M7 11 l2-2-2-2 M11 13h4 M5,3 L19,3 A2,2 0 0 1 21,5 L21,19 A2,2 0 0 1 19,21 L5,21 A2,2 0 0 1 3,19 L3,5 A2,2 0 0 1 5,3 Z",
    "bot": "M12 8V4H8 M6,8 L18,8 A2,2 0 0 1 20,10 L20,18 A2,2 0 0 1 18,20 L6,20 A2,2 0 0 1 4,18 L4,10 A2,2 0 0 1 6,8 Z M2 14h2 M20 14h2 M15 13v2 M9 13v2",
    "book-open": "M12 7v14 M3 18a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1h5a4 4 0 0 1 4 4 4 4 0 0 1 4-4h5a1 1 0 0 1 1 1v13a1 1 0 0 1-1 1h-6a3 3 0 0 0-3 3 3 3 0 0 0-3-3z",
    "settings-2": "M20 7h-9 M14 17H5 M14,17 A3,3 0 1 1 20,17 A3,3 0 1 1 14,17 Z M4,7 A3,3 0 1 1 10,7 A3,3 0 1 1 4,7 Z",
    "frame": "M22,6 L2,6 M22,18 L2,18 M6,2 L6,22 M18,2 L18,22",
    "chart-pie": "M21 12c.552 0 1.005-.449.95-.998a10 10 0 0 0-8.953-8.951c-.55-.055-.998.398-.998.95v8a1 1 0 0 0 1 1z M21.21 15.89A10 10 0 1 1 8 2.83",
    "map": "M14.106 5.553a2 2 0 0 0 1.788 0l3.659-1.83A1 1 0 0 1 21 4.619v12.764a1 1 0 0 1-.553.894l-4.553 2.277a2 2 0 0 1-1.788 0l-4.212-2.106a2 2 0 0 0-1.788 0l-3.659 1.83A1 1 0 0 1 3 19.381V6.618a1 1 0 0 1 .553-.894l4.553-2.277a2 2 0 0 1 1.788 0z M15 5.764v15 M9 3.236v15",
    "gallery-vertical-end": "M7 2h10 M5 6h14 M5,10 L19,10 A2,2 0 0 1 21,12 L21,20 A2,2 0 0 1 19,22 L5,22 A2,2 0 0 1 3,20 L3,12 A2,2 0 0 1 5,10 Z",
    "chevrons-up-down": "M7 15 l5 5 5-5 M7 9 l5-5 5 5",
    "panel-left": "M5,3 L19,3 A2,2 0 0 1 21,5 L21,19 A2,2 0 0 1 19,21 L5,21 A2,2 0 0 1 3,19 L3,5 A2,2 0 0 1 5,3 Z M9 3v18",
    "arrow-up-down": "M21 16 l-4 4-4-4 M17 20V4 M3 8 l4-4 4 4 M7 4v16",
    "chevron-down": "M6 9 l6 6 6-6",
    "check": "M20 6 9 17l-5-5",
    "search": "M3,11 A8,8 0 1 1 19,11 A8,8 0 1 1 3,11 Z M21 21 l-4.3-4.3",
}


def icon(kind, size=16, brush="CosmoForegroundBrush", extra=""):
    return f'<Viewbox Width="{size}" Height="{size}"{extra}><Path Theme="{{DynamicResource CosmoIcon}}" Stroke="{{DynamicResource {brush}}}" Data="{ICONS[kind]}" /></Viewbox>'


def text(value, size=14, line=20, extra=""):
    return f'<TextBlock Text="{value}" FontSize="{size}" LineHeight="{line}"{extra} />'


def time_zones(name, selected):
    selection = f' SelectedIndex="{selected}"' if selected is not None else ' PlaceholderText="Select a timezone"'
    return f"""
        <Panel Height="440" HorizontalAlignment="Left" VerticalAlignment="Top">
          <ComboBox Name="{name}" a:Web.Id="{name}" Width="280" MaxDropDownHeight="384" VerticalAlignment="Top"{selection}>
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
            <ComboBoxItem Classes="label" Content="Australia &amp; Pacific" />
            <ComboBoxItem Content="Australian Western Standard Time (AWST)" />
            <ComboBoxItem Content="Australian Central Standard Time (ACST)" />
            <ComboBoxItem Content="Australian Eastern Standard Time (AEST)" />
            <ComboBoxItem Content="New Zealand Standard Time (NZST)" />
            <ComboBoxItem Content="Fiji Time (FJT)" />
            <ComboBoxItem Classes="label" Content="South America" />
            <ComboBoxItem Content="Argentina Time (ART)" />
            <ComboBoxItem Content="Bolivia Time (BOT)" />
            <ComboBoxItem Content="Brasilia Time (BRT)" />
            <ComboBoxItem Content="Chile Standard Time (CLT)" />
          </ComboBox>
        </Panel>
        """


FIXTURES = {
    "separator-demo": f"""
        <StackPanel>
          <StackPanel Spacing="4">
            {text("Radix Primitives", 14, 14, ' FontWeight="Medium"')}
            {text("An open-source UI component library.", extra=' Foreground="{DynamicResource CosmoMutedForegroundBrush}"')}
          </StackPanel>
          <Separator Margin="0,16" />
          <StackPanel Orientation="Horizontal" Height="20" Spacing="16">
            {text("Blog", extra=' VerticalAlignment="Center"')}
            <Separator Classes="vertical" />
            {text("Docs", extra=' VerticalAlignment="Center"')}
            <Separator Classes="vertical" />
            {text("Source", extra=' VerticalAlignment="Center"')}
          </StackPanel>
        </StackPanel>
        """,
    "skeleton-demo": """
        <StackPanel Spacing="32">
          <StackPanel Orientation="Horizontal" Spacing="16">
            <c:Skeleton Width="48" Height="48" CornerRadius="9999" VerticalAlignment="Center" />
            <StackPanel Spacing="8" VerticalAlignment="Center">
              <c:Skeleton Width="250" Height="16" />
              <c:Skeleton Width="200" Height="16" />
            </StackPanel>
          </StackPanel>
          <StackPanel Spacing="12">
            <c:Skeleton Width="250" Height="125" CornerRadius="{DynamicResource CosmoRadiusXl}" />
            <StackPanel Spacing="8">
              <c:Skeleton Width="250" Height="16" />
              <c:Skeleton Width="200" Height="16" />
            </StackPanel>
          </StackPanel>
        </StackPanel>
        """,
    "spinner-demo": """
        <StackPanel Spacing="24">
          <StackPanel Orientation="Horizontal" Spacing="24">
            <c:Spinner Width="12" Height="12" VerticalAlignment="Center" />
            <c:Spinner VerticalAlignment="Center" />
            <c:Spinner Width="24" Height="24" VerticalAlignment="Center" />
            <c:Spinner Width="32" Height="32" VerticalAlignment="Center" />
          </StackPanel>
          <StackPanel Orientation="Horizontal" Spacing="16">
            <Button Classes="sm has-icon" IsEnabled="False">
              <StackPanel><c:Spinner /><TextBlock Text="Loading..." /></StackPanel>
            </Button>
            <Button Classes="outline sm has-icon" IsEnabled="False">
              <StackPanel><c:Spinner /><TextBlock Text="Please wait" /></StackPanel>
            </Button>
          </StackPanel>
          <StackPanel Orientation="Horizontal" Spacing="16">
            <c:Badge><StackPanel><c:Spinner /><TextBlock Text="Syncing" /></StackPanel></c:Badge>
            <c:Badge Classes="secondary"><StackPanel><c:Spinner /><TextBlock Text="Updating" /></StackPanel></c:Badge>
            <c:Badge Classes="outline"><StackPanel><c:Spinner /><TextBlock Text="Processing" /></StackPanel></c:Badge>
          </StackPanel>
        </StackPanel>
        """,
    "kbd-demo": f"""
        <StackPanel Spacing="16">
          <c:KbdGroup>
            <c:Kbd Content="⌘" />
            <c:Kbd Content="⇧" />
            <c:Kbd Content="⌥" />
            <c:Kbd Content="⌃" />
          </c:KbdGroup>
          <c:KbdGroup>
            <c:Kbd Content="Ctrl" />
            {text("+", 16, 24)}
            <c:Kbd Content="B" />
          </c:KbdGroup>
          <StackPanel Orientation="Horizontal" Spacing="4">
            {text("Use", extra=' VerticalAlignment="Center" Foreground="{DynamicResource CosmoMutedForegroundBrush}"')}
            <c:KbdGroup>
              <c:Kbd Content="Ctrl + B" />
              <c:Kbd Content="Ctrl + K" />
            </c:KbdGroup>
            {text("to open the command palette", extra=' VerticalAlignment="Center" Foreground="{DynamicResource CosmoMutedForegroundBrush}"')}
          </StackPanel>
          <StackPanel Orientation="Horizontal" Spacing="16">
            <Button Classes="outline sm" Padding="12,0,8,0">
              <StackPanel><TextBlock Text="Accept" /><c:Kbd Content="⏎" /></StackPanel>
            </Button>
            <Button Classes="outline sm" Padding="12,0,8,0">
              <StackPanel><TextBlock Text="Cancel" /><c:Kbd Content="Esc" /></StackPanel>
            </Button>
          </StackPanel>
        </StackPanel>
        """,
    "avatar-demo": """
        <StackPanel Spacing="32">
          <StackPanel Orientation="Horizontal" Spacing="48">
            <c:Avatar Source="/Assets/avatar-shadcn.png" Content="CN" AutomationProperties.Name="@shadcn" />
            <c:Avatar Source="/Assets/avatar-evilrabbit.png" Content="ER" CornerRadius="{DynamicResource CosmoRadiusLg}" AutomationProperties.Name="@evilrabbit" />
            <c:AvatarGroup>
              <c:Avatar Source="/Assets/avatar-shadcn.png" Content="CN" AutomationProperties.Name="@shadcn" />
              <c:Avatar Source="/Assets/avatar-maxleiter.png" Content="LR" AutomationProperties.Name="@maxleiter" />
              <c:Avatar Source="/Assets/avatar-evilrabbit.png" Content="ER" AutomationProperties.Name="@evilrabbit" />
            </c:AvatarGroup>
          </StackPanel>
          <StackPanel Orientation="Horizontal" Spacing="48">
            <c:Avatar Classes="sm" Content="CN" />
            <c:Avatar Content="CN" />
            <c:Avatar Classes="lg" Content="CN" />
          </StackPanel>
        </StackPanel>
        """,
    "avatar-badge": f"""
        <StackPanel Orientation="Horizontal" Spacing="48">
          <c:Avatar Classes="sm" Source="/Assets/avatar-shadcn.png" Content="CN" AutomationProperties.Name="@shadcn">
            <c:Avatar.Badge><c:AvatarBadge /></c:Avatar.Badge>
          </c:Avatar>
          <c:Avatar Source="/Assets/avatar-shadcn.png" Content="CN" AutomationProperties.Name="@shadcn">
            <c:Avatar.Badge><c:AvatarBadge /></c:Avatar.Badge>
          </c:Avatar>
          <c:Avatar Classes="lg" Source="/Assets/avatar-shadcn.png" Content="CN" AutomationProperties.Name="@shadcn">
            <c:Avatar.Badge><c:AvatarBadge /></c:Avatar.Badge>
          </c:Avatar>
          <c:Avatar Content="ER">
            <c:Avatar.Badge><c:AvatarBadge>{icon("plus", size=8, brush="CosmoPrimaryForegroundBrush")}</c:AvatarBadge></c:Avatar.Badge>
          </c:Avatar>
          <c:Avatar Classes="lg" Content="ER">
            <c:Avatar.Badge><c:AvatarBadge>{icon("plus", size=8, brush="CosmoPrimaryForegroundBrush")}</c:AvatarBadge></c:Avatar.Badge>
          </c:Avatar>
        </StackPanel>
        """,
    "avatar-group-count": f"""
        <StackPanel Spacing="32">
          <c:AvatarGroup>
            <c:Avatar Source="/Assets/avatar-shadcn.png" Content="CN" AutomationProperties.Name="@shadcn" />
            <c:Avatar Source="/Assets/avatar-maxleiter.png" Content="LR" AutomationProperties.Name="@maxleiter" />
            <c:Avatar Source="/Assets/avatar-evilrabbit.png" Content="ER" AutomationProperties.Name="@evilrabbit" />
            <c:AvatarGroupCount Content="+3" />
          </c:AvatarGroup>
          <c:AvatarGroup>
            <c:Avatar Classes="sm" Content="CN" />
            <c:Avatar Classes="sm" Content="LR" />
            <c:AvatarGroupCount Content="+2" />
          </c:AvatarGroup>
          <c:AvatarGroup>
            <c:Avatar Classes="lg" Content="CN" />
            <c:Avatar Classes="lg" Content="LR" />
            <c:AvatarGroupCount>{icon("plus", size=20, brush="CosmoMutedForegroundBrush")}</c:AvatarGroupCount>
          </c:AvatarGroup>
        </StackPanel>
        """,
    "alert-demo": f"""
        <StackPanel Spacing="16" Width="576" HorizontalAlignment="Left">
          <c:Alert Title="Success! Your changes have been saved" Content="This is an alert with icon, title and description.">
            <c:Alert.Icon>{icon("circle-check")}</c:Alert.Icon>
          </c:Alert>
          <c:Alert Title="This Alert has a title and an icon. No description.">
            <c:Alert.Icon>{icon("popcorn")}</c:Alert.Icon>
          </c:Alert>
          <c:Alert Classes="destructive" Title="Unable to process your payment.">
            <c:Alert.Icon>{icon("circle-alert", brush="CosmoDestructiveBrush")}</c:Alert.Icon>
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
        """,
    "aspect-ratio-demo": """
        <c:AspectRatio Ratio="1.7777778" Width="450" HorizontalAlignment="Left">
          <Border CornerRadius="{DynamicResource CosmoRadiusLg}" Background="{DynamicResource CosmoMutedBrush}" ClipToBounds="True">
            <Image Source="/Assets/photo-landscape.png" Stretch="UniformToFill" AutomationProperties.Name="A landscape at dusk" />
          </Border>
        </c:AspectRatio>
        """,
    "typography-headings": """
        <StackPanel>
          <TextBlock Classes="typography h1" Text="Taxing Laughter: The Joke Tax Chronicles" />
          <Border Margin="0,40,0,0" Padding="0,0,0,8" BorderThickness="0,0,0,1" BorderBrush="{DynamicResource CosmoBorderBrush}">
            <TextBlock Classes="typography h2" Text="The People of the Kingdom" />
          </Border>
          <TextBlock Classes="typography h3" Margin="0,32,0,0" Text="The Joke Tax" />
          <TextBlock Classes="typography h4" Margin="0,24,0,0" Text="People stopped telling jokes" />
        </StackPanel>
        """,
    "typography-text": """
        <StackPanel>
          <TextBlock Classes="typography lead" Text="A modal dialog that interrupts the user with important content and expects a response." />
          <TextBlock Classes="typography p" Margin="0,24,0,0" Text="The king, seeing how much happier his subjects were, realized the error of his ways and repealed the joke tax." />
          <Border Classes="typography blockquote" Margin="0,24,0,0">
            <TextBlock Text="&quot;After all,&quot; he said, &quot;everyone enjoys a good joke, so it's only fair that they should pay for the privilege.&quot;" />
          </Border>
          <c:List Margin="0,16,0,24">
            <x:String>1st level of puns: 5 gold coins</x:String>
            <x:String>2nd level of jokes: 10 gold coins</x:String>
            <x:String>3rd level of one-liners : 20 gold coins</x:String>
          </c:List>
          <Border Classes="typography inline-code" Margin="0,-1.2">
            <TextBlock Text="@radix-ui/react-alert-dialog" />
          </Border>
          <TextBlock Classes="typography large" Margin="0,24,0,0" Text="Are you absolutely sure?" />
          <TextBlock Classes="typography small" Margin="0,16,0,0" Text="Email address" />
          <TextBlock Classes="typography muted" Margin="0,16,0,0" Text="Enter your email address." />
        </StackPanel>
        """,
    "empty-demo": f"""
        <c:Empty Title="No Projects Yet" Description="You haven't created any projects yet. Get started by creating your first project.">
          <c:Empty.Media>
            <c:EmptyMedia Classes="icon">{icon("folder-code", 24)}</c:EmptyMedia>
          </c:Empty.Media>
          <StackPanel Spacing="24">
            <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Center">
              <Button Content="Create Project" />
              <Button Classes="outline" Content="Import Project" />
            </StackPanel>
            <HyperlinkButton Classes="link sm has-icon" HorizontalAlignment="Center" Foreground="{{DynamicResource CosmoMutedForegroundBrush}}">
              <StackPanel><TextBlock Text="Learn More" />{icon("arrow-up-right", brush="CosmoMutedForegroundBrush")}</StackPanel>
            </HyperlinkButton>
          </StackPanel>
        </c:Empty>
        """,
    "empty-outline": f"""
        <c:Empty Classes="outline" Title="Cloud Storage Empty" Description="Upload files to your cloud storage to access them anywhere.">
          <c:Empty.Media>
            <c:EmptyMedia Classes="icon">{icon("cloud", 24)}</c:EmptyMedia>
          </c:Empty.Media>
          <Button Classes="outline sm" Content="Upload Files" />
        </c:Empty>
        """,
    "item-demo": f"""
        <StackPanel Spacing="24" Width="448" HorizontalAlignment="Left">
          <c:Item Classes="outline" Title="Basic Item" Description="A simple item with title and description.">
            <c:Item.Actions>
              <Button Classes="outline sm" Content="Action" />
            </c:Item.Actions>
          </c:Item>
          <c:Item Classes="outline sm link" Title="Your profile has been verified.">
            <c:Item.Media>
              <c:ItemMedia>{icon("badge-check", 20)}</c:ItemMedia>
            </c:Item.Media>
            <c:Item.Actions>
              {icon("chevron-right")}
            </c:Item.Actions>
          </c:Item>
        </StackPanel>
        """,
    "item-variant": """
        <StackPanel Spacing="24">
          <c:Item Title="Default Variant" Description="Standard styling with subtle background and borders.">
            <c:Item.Actions>
              <Button Classes="outline sm" Content="Open" />
            </c:Item.Actions>
          </c:Item>
          <c:Item Classes="outline" Title="Outline Variant" Description="Outlined style with clear borders and transparent background.">
            <c:Item.Actions>
              <Button Classes="outline sm" Content="Open" />
            </c:Item.Actions>
          </c:Item>
          <c:Item Classes="muted" Title="Muted Variant" Description="Subdued appearance with muted colors for secondary content.">
            <c:Item.Actions>
              <Button Classes="outline sm" Content="Open" />
            </c:Item.Actions>
          </c:Item>
        </StackPanel>
        """,
}


ACCORDION = [
    ("Product Information", "Our flagship product combines cutting-edge technology with sleek design. Built with premium materials, it offers unparalleled performance and reliability.", "Key features include advanced processing capabilities, and an intuitive user interface designed for both beginners and experts."),
    ("Shipping Details", "We offer worldwide shipping through trusted courier partners. Standard delivery takes 3-5 business days, while express shipping ensures delivery within 1-2 business days.", "All orders are carefully packaged and fully insured. Track your shipment in real-time through our dedicated tracking portal."),
    ("Return Policy", "We stand behind our products with a comprehensive 30-day return policy. If you're not completely satisfied, simply return the item in its original condition.", "Our hassle-free return process includes free return shipping and full refunds processed within 48 hours of receiving the returned item."),
]


def accordion():
    items = "\n".join(
        f"""  <Expander Header="{title}"{' IsExpanded="True"' if i == 0 else ''}>
    <StackPanel Spacing="16">
      <TextBlock Text="{first.replace("'", "&apos;")}" TextWrapping="Wrap" />
      <TextBlock Text="{second.replace("'", "&apos;")}" TextWrapping="Wrap" />
    </StackPanel>
  </Expander>"""
        for i, (title, first, second) in enumerate(ACCORDION))
    return f"<c:Accordion IsCollapsible=\"True\">\n{items}\n</c:Accordion>"


def row(text):
    return f'<Border CornerRadius="{{DynamicResource CosmoRadiusMd}}" BorderThickness="1" BorderBrush="{{DynamicResource CosmoBorderBrush}}" Padding="16,8"><TextBlock Text="{text}" FontFamily="{{DynamicResource CosmoFontMono}}" FontSize="14" LineHeight="20" /></Border>'


def collapsible(open_):
    return f"""<Expander Classes="collapsible" Width="350" Padding="0,0,16,0"{' IsExpanded="True"' if open_ else ''}>
  <Expander.Header>
    <StackPanel Spacing="8">
      <Grid Height="32">
        <TextBlock Text="@peduarte starred 3 repositories" FontSize="14" LineHeight="20" FontWeight="SemiBold" VerticalAlignment="Center" Margin="16,0,0,0" />
      </Grid>
      {row("@radix-ui/primitives")}
    </StackPanel>
  </Expander.Header>
  <StackPanel Spacing="8">
    {row("@radix-ui/colors")}
    {row("@stitches/react")}
  </StackPanel>
</Expander>"""


def group(buttons, extra=""):
    return f"<c:ButtonGroup{extra}>\n" + "\n".join("  " + b for b in buttons) + "\n</c:ButtonGroup>"


def outline(text, size=""):
    return f'<Button Classes="outline{(" " + size) if size else ""}" Content="{text}" />'


def icon_button(kind, classes, name):
    return f'<Button Classes="{classes}" AutomationProperties.Name="{name}">{icon(kind)}</Button>'


FIXTURES.update({
    "tabs-demo": f"""
        <StackPanel Width="384" HorizontalAlignment="Left">
          <TabControl SelectedIndex="0">
            <TabItem Header="Account">
              <c:Card>
                <c:CardHeader Title="Account" Description="Make changes to your account here. Click save when you're done." />
                <c:CardContent>
                  <StackPanel Spacing="24">
                    <StackPanel Spacing="12">
                      <Label Content="Name" />
                      <TextBox Text="Pedro Duarte" />
                    </StackPanel>
                    <StackPanel Spacing="12">
                      <Label Content="Username" />
                      <TextBox Text="@peduarte" />
                    </StackPanel>
                  </StackPanel>
                </c:CardContent>
                <c:CardFooter>
                  <Button Content="Save changes" HorizontalAlignment="Left" />
                </c:CardFooter>
              </c:Card>
            </TabItem>
            <TabItem Header="Password">
              <c:Card>
                <c:CardHeader Title="Password" />
              </c:Card>
            </TabItem>
          </TabControl>
        </StackPanel>
        """,
    "tabs-list": """
        <TabControl SelectedIndex="0">
          <TabItem Header="Overview" />
          <TabItem Header="Analytics" />
          <TabItem Header="Reports" />
          <TabItem Header="Notifications" IsEnabled="False" />
        </TabControl>
        """,
    "tabs-line": """
        <TabControl Classes="line" SelectedIndex="0">
          <TabItem Header="Overview" />
          <TabItem Header="Analytics" />
          <TabItem Header="Reports" />
        </TabControl>
        """,
    "tabs-vertical": """
        <TabControl TabStripPlacement="Left" SelectedIndex="0" Width="384" HorizontalAlignment="Left">
          <TabItem Header="Account">
            <TextBlock Text="Make changes to your account here." FontSize="14" LineHeight="20" Foreground="{DynamicResource CosmoMutedForegroundBrush}" TextWrapping="Wrap" />
          </TabItem>
          <TabItem Header="Password">
            <TextBlock Text="Change your password here." FontSize="14" LineHeight="20" Foreground="{DynamicResource CosmoMutedForegroundBrush}" TextWrapping="Wrap" />
          </TabItem>
          <TabItem Header="Notifications">
            <TextBlock Text="Choose what we email you about." FontSize="14" LineHeight="20" Foreground="{DynamicResource CosmoMutedForegroundBrush}" TextWrapping="Wrap" />
          </TabItem>
        </TabControl>
        """,
    "tabs-vertical-line": """
        <TabControl Classes="line" TabStripPlacement="Left" SelectedIndex="0">
          <TabItem Header="Account" />
          <TabItem Header="Password" />
          <TabItem Header="Notifications" />
        </TabControl>
        """,
    "accordion-demo": accordion(),
    "collapsible-demo": collapsible(False),
    "collapsible-open": collapsible(True),
    "breadcrumb-demo": """
        <c:Breadcrumb>
          <HyperlinkButton Content="Home" NavigateUri="https://ui.shadcn.com/" />
          <Button AutomationProperties.Name="Toggle menu">
            <c:BreadcrumbEllipsis Width="16" Height="16" />
          </Button>
          <HyperlinkButton Content="Components" NavigateUri="https://ui.shadcn.com/docs/components" />
          <c:BreadcrumbPage Text="Breadcrumb" />
        </c:Breadcrumb>
        """,
    "breadcrumb-separator": f"""
        <c:Breadcrumb>
          <c:Breadcrumb.SeparatorTemplate>
            <DataTemplate>{icon("slash", 14, "CosmoMutedForegroundBrush")}</DataTemplate>
          </c:Breadcrumb.SeparatorTemplate>
          <HyperlinkButton Content="Home" NavigateUri="https://ui.shadcn.com/" />
          <HyperlinkButton Content="Components" NavigateUri="https://ui.shadcn.com/docs/components" />
          <c:BreadcrumbPage Text="Breadcrumb" />
        </c:Breadcrumb>
        """,
    "pagination-demo": """
        <c:Pagination>
          <c:PaginationPrevious />
          <Button Classes="ghost icon" Content="1" />
          <Button Classes="outline icon" Content="2" />
          <Button Classes="ghost icon" Content="3" />
          <c:PaginationEllipsis />
          <c:PaginationNext />
        </c:Pagination>
        """,
    "button-group-demo": group([
        group([icon_button("arrow-left", "outline icon", "Go Back")]),
        group([outline("Archive"), outline("Report")]),
        group([outline("Snooze"), icon_button("ellipsis", "outline icon", "More Options")]),
    ]),
    "button-group-size": "<StackPanel Spacing=\"32\" HorizontalAlignment=\"Left\">\n" + "\n".join(
        group([outline(t, size) for t in labels] + [icon_button("plus", f"outline {icon_size}", "Add")])
        for labels, size, icon_size in [(["Small", "Button", "Group"], "sm", "icon-sm"), (["Default", "Button", "Group"], "", "icon"), (["Large", "Button", "Group"], "lg", "icon-lg")]) + "\n</StackPanel>",
    "button-group-orientation": "<StackPanel Orientation=\"Horizontal\" Spacing=\"32\" VerticalAlignment=\"Top\">\n" + "\n".join([
        group([icon_button("plus", "outline icon", "Add"), icon_button("minus", "outline icon", "Remove")], ' Orientation="Vertical" VerticalAlignment="Top" AutomationProperties.Name="Media controls"'),
        group(['<Button Classes="secondary sm" Content="Copy" />', '<Separator />', '<Button Classes="secondary sm" Content="Paste" />'], ' VerticalAlignment="Top"'),
        group(['<c:ButtonGroupText Content="https://" />', '<TextBox PlaceholderText="my-app" Width="160" />', outline("Go")], ' VerticalAlignment="Top"'),
    ]) + "\n</StackPanel>",
})


TAGS = "\n".join(f'<TextBlock Text="v1.2.0-beta.{n}" FontSize="14" LineHeight="20" />\n<Separator Margin="0,8" />' for n in range(50, 0, -1))

WORKS = [("binni", "Ornella Binni"), ("byrom", "Tom Byrom"), ("malyavko", "Vladimir Malyavko")]

INVOICES = [("INV001", "Paid", "$250.00", "Credit Card"), ("INV002", "Pending", "$150.00", "PayPal"), ("INV003", "Unpaid", "$350.00", "Bank Transfer"),
            ("INV004", "Paid", "$450.00", "Credit Card"), ("INV005", "Paid", "$550.00", "PayPal"), ("INV006", "Pending", "$200.00", "Bank Transfer"),
            ("INV007", "Unpaid", "$300.00", "Credit Card")]

COMPONENTS = [
    ("Alert Dialog", "A modal dialog that interrupts the user with important content and expects a response."),
    ("Hover Card", "For sighted users to preview content available behind a link."),
    ("Progress", "Displays an indicator showing the completion progress of a task, typically displayed as a progress bar."),
    ("Scroll-area", "Visually or semantically separates content."),
    ("Tabs", "A set of layered sections of content—known as tab panels—that are displayed one at a time."),
    ("Tooltip", "A popup that displays information related to an element when the element receives keyboard focus or the mouse hovers over it."),
]


def nav_link(title, description, width=246):
    return "\n".join([
        f'<Button Classes="navigation-menu-link" Width="{width}">',
        '  <StackPanel>',
        f'    <TextBlock Text="{title}" FontSize="14" LineHeight="14" FontWeight="Medium" />',
        f'    <TextBlock Text="{description}" FontSize="14" LineHeight="19.25" MaxLines="2" TextWrapping="Wrap" TextTrimming="CharacterEllipsis" Foreground="{{DynamicResource CosmoMutedForegroundBrush}}" />',
        '  </StackPanel>',
        '</Button>'])


def trigger(name, panel, items):
    return "\n".join([
        f'<c:NavigationMenuTrigger Content="{name}">',
        '  <c:NavigationMenuTrigger.Flyout>',
        '    <Flyout>',
        f'      {panel}',
        indent(items, " " * 8),
        '      </' + panel.split()[0][1:] + '>',
        '    </Flyout>',
        '  </c:NavigationMenuTrigger.Flyout>',
        '</c:NavigationMenuTrigger>'])


def navigation_menu():
    home = "\n".join(nav_link(t, d, 384) for t, d in [("Introduction", "Re-usable components built using Radix UI and Tailwind CSS."), ("Installation", "How to install dependencies and structure your app."), ("Typography", "Styles for headings, paragraphs, lists...etc")])
    components = "\n".join(nav_link(t, d) for t, d in COMPONENTS)
    lists = "\n".join(f'<Button Classes="navigation-menu-link"><StackPanel><TextBlock Text="{t}" FontWeight="Medium" /><TextBlock Text="{d}" Foreground="{{DynamicResource CosmoMutedForegroundBrush}}" /></StackPanel></Button>'
                      for t, d in [("Components", "Browse all components in the library."), ("Documentation", "Learn how to use the library."), ("Blog", "Read our latest blog posts.")])
    body = "\n".join([
        trigger("Home", '<StackPanel Spacing="8" Width="400">', home),
        trigger("Components", '<UniformGrid Columns="2" Width="500" ColumnSpacing="8" RowSpacing="8">', components),
        '<HyperlinkButton Classes="navigation-menu-trigger" Content="Docs" />',
        trigger("List", '<StackPanel Width="300">', lists),
    ])
    return "\n".join([
        '<Panel Height="340" VerticalAlignment="Top">',
        '  <c:NavigationMenu VerticalAlignment="Top">',
        indent(body, " " * 4),
        '  </c:NavigationMenu>',
        '</Panel>'])


def menubar():
    return """
        <Panel Height="300" VerticalAlignment="Top">
          <Menu VerticalAlignment="Top">
            <MenuItem Header="File">
              <MenuItem Header="New Tab" InputGesture="Ctrl+T" />
              <MenuItem Header="New Window" InputGesture="Ctrl+N" />
              <MenuItem Header="New Incognito Window" IsEnabled="False" />
              <MenuItem Header="-" />
              <MenuItem Header="Share">
                <MenuItem Header="Email link" />
                <MenuItem Header="Messages" />
                <MenuItem Header="Notes" />
              </MenuItem>
              <MenuItem Header="-" />
              <MenuItem Header="Print..." InputGesture="Ctrl+P" />
            </MenuItem>
            <MenuItem Header="Edit">
              <MenuItem Header="Undo" InputGesture="Ctrl+Z" />
              <MenuItem Header="Redo" InputGesture="Ctrl+Shift+Z" />
              <MenuItem Header="-" />
              <MenuItem Header="Cut" />
              <MenuItem Header="Copy" />
              <MenuItem Header="Paste" />
            </MenuItem>
            <MenuItem Header="View">
              <MenuItem Header="Always Show Bookmarks Bar" ToggleType="CheckBox" />
              <MenuItem Header="Always Show Full URLs" ToggleType="CheckBox" IsChecked="True" />
              <MenuItem Header="-" />
              <MenuItem Header="Reload" InputGesture="Ctrl+R" Classes="inset" />
              <MenuItem Header="Toggle Fullscreen" Classes="inset" />
            </MenuItem>
            <MenuItem Header="Profiles">
              <MenuItem Header="Andy" ToggleType="Radio" GroupName="profiles" />
              <MenuItem Header="Benoit" ToggleType="Radio" GroupName="profiles" IsChecked="True" />
              <MenuItem Header="Luis" ToggleType="Radio" GroupName="profiles" />
              <MenuItem Header="-" />
              <MenuItem Header="Add Profile..." Classes="inset" />
            </MenuItem>
          </Menu>
        </Panel>
        """


def figure(key, artist):
    return "\n".join([
        '<StackPanel>',
        '  <Border CornerRadius="{DynamicResource CosmoRadiusMd}" ClipToBounds="True">',
        f'    <Image Source="/Assets/art-{key}.png" Width="300" Height="400" Stretch="UniformToFill" AutomationProperties.Name="Photo by {artist}" />',
        '  </Border>',
        '  <TextBlock FontSize="12" LineHeight="16" Margin="0,8,0,0" Foreground="{DynamicResource CosmoMutedForegroundBrush}">',
        f'    <Run Text="Photo by " /><Run Text="{artist}" FontWeight="SemiBold" Foreground="{{DynamicResource CosmoForegroundBrush}}" />',
        '  </TextBlock>',
        '</StackPanel>'])


FIXTURES.update({
    "scroll-area-demo": "\n".join([
        '<ScrollViewer Classes="scroll-area" Width="192" Height="288" HorizontalAlignment="Left" BorderThickness="1"',
        '              BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusMd}">',
        '  <StackPanel Margin="16">',
        '    <TextBlock Text="Tags" FontSize="14" LineHeight="14" FontWeight="Medium" Margin="0,0,0,16" />',
        indent(TAGS, " " * 4),
        '  </StackPanel>',
        '</ScrollViewer>']),
    "scroll-area-horizontal": "\n".join([
        '<ScrollViewer Classes="scroll-area" Width="384" HorizontalAlignment="Left" HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Disabled"',
        '              BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusMd}">',
        '  <StackPanel Orientation="Horizontal" Spacing="16" Margin="16">',
        indent("\n".join(figure(key, artist) for key, artist in WORKS), " " * 4),
        '  </StackPanel>',
        '</ScrollViewer>']),
    "table-demo": "\n".join([
        '<StackPanel>',
        '  <TableView>',
        '    <TableView.Columns>',
        '      <TableViewColumn Header="Invoice" Width="100" x:DataType="m:Invoice">',
        '        <TableViewColumn.CellTemplate>',
        '          <DataTemplate x:DataType="m:Invoice"><TextBlock Text="{Binding Number}" FontWeight="Medium" /></DataTemplate>',
        '        </TableViewColumn.CellTemplate>',
        '      </TableViewColumn>',
        '      <TableViewColumn Header="Status" Binding="{Binding Status}" Width="128*" x:DataType="m:Invoice" />',
        '      <TableViewColumn Header="Method" Binding="{Binding Method}" Width="202*" x:DataType="m:Invoice" />',
        '      <TableViewColumn Header="Amount" Binding="{Binding Amount}" Width="162*" HorizontalContentAlignment="Right" x:DataType="m:Invoice" />',
        '    </TableView.Columns>',
        '    <TableView.Items>',
        *[f'      <m:Invoice Number="{n}" Status="{s}" Method="{m}" Amount="{a}" />' for n, s, a, m in INVOICES],
        '    </TableView.Items>',
        '  </TableView>',
        '  <c:TableFooter>',
        '    <DockPanel>',
        '      <TextBlock DockPanel.Dock="Right" Text="$2,500.00" />',
        '      <TextBlock Text="Total" />',
        '    </DockPanel>',
        '  </c:TableFooter>',
        '  <c:TableCaption Text="A list of your recent invoices." />',
        '</StackPanel>']),
    "navigation-menu-demo": navigation_menu(),
    "navigation-menu-open": navigation_menu(),
    "menubar-demo": menubar(),
    "menubar-open": menubar(),
})


def resizable_text(value, extra=""):
    return f'<TextBlock Text="{value}" FontSize="16" LineHeight="24" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center"{extra} />'


def resizable_frame(body, height):
    return "\n".join([
        f'<Border Width="450" Height="{height}" HorizontalAlignment="Left" BorderThickness="1" BorderBrush="{{DynamicResource CosmoBorderBrush}}"',
        '        CornerRadius="{DynamicResource CosmoRadiusLg}" ClipToBounds="True">',
        indent(body, "  "),
        '</Border>'])


def slide(number, wrap="4", size=36, line=40, square=True):
    content = f'<c:CardContent Padding="24"><TextBlock Text="{number}" FontSize="{size}" LineHeight="{line}" FontWeight="SemiBold" HorizontalAlignment="Center" VerticalAlignment="Center" /></c:CardContent>'
    card = f'<c:Card><c:AspectRatio Ratio="1">{content}</c:AspectRatio></c:Card>' if square else f'<c:Card VerticalAlignment="Top">{content}</c:Card>'
    return f'<Border Padding="{wrap}">{card}</Border>' if wrap else card


FIXTURES.update({
    "resizable-demo": resizable_frame("\n".join([
        '<c:ResizablePanelGroup>',
        f'  <c:ResizablePanel DefaultSize="50"><Panel Height="200">{resizable_text("One")}</Panel></c:ResizablePanel>',
        '  <c:ResizableHandle />',
        '  <c:ResizablePanel DefaultSize="50">',
        '    <c:ResizablePanelGroup Orientation="Vertical">',
        f'      <c:ResizablePanel DefaultSize="25">{resizable_text("Two")}</c:ResizablePanel>',
        '      <c:ResizableHandle />',
        f'      <c:ResizablePanel DefaultSize="75">{resizable_text("Three")}</c:ResizablePanel>',
        '    </c:ResizablePanelGroup>',
        '  </c:ResizablePanel>',
        '</c:ResizablePanelGroup>']), 202),
    "resizable-handle": resizable_frame("\n".join([
        '<c:ResizablePanelGroup>',
        f'  <c:ResizablePanel DefaultSize="25">{resizable_text("Sidebar")}</c:ResizablePanel>',
        '  <c:ResizableHandle WithHandle="True" />',
        f'  <c:ResizablePanel DefaultSize="75">{resizable_text("Content")}</c:ResizablePanel>',
        '</c:ResizablePanelGroup>']), 200),
    "resizable-vertical": resizable_frame("\n".join([
        '<c:ResizablePanelGroup Orientation="Vertical">',
        f'  <c:ResizablePanel DefaultSize="25">{resizable_text("Header")}</c:ResizablePanel>',
        '  <c:ResizableHandle />',
        f'  <c:ResizablePanel DefaultSize="75">{resizable_text("Content")}</c:ResizablePanel>',
        '</c:ResizablePanelGroup>']), 200),
    "carousel-demo": "\n".join([
        '<Carousel Width="320" HorizontalAlignment="Center">',
        *[f'  {slide(n)}' for n in range(1, 6)],
        '</Carousel>']),
    "carousel-api": "\n".join([
        '<StackPanel Width="320" HorizontalAlignment="Center">',
        '  <Carousel Name="Slides">',
        *[f'    {slide(n, wrap=None)}' for n in range(1, 6)],
        '  </Carousel>',
        '  <TextBlock Text="Slide 1 of 5" FontSize="14" LineHeight="20" Margin="0,8" HorizontalAlignment="Center" Foreground="{DynamicResource CosmoMutedForegroundBrush}" />',
        '</StackPanel>']),
    "carousel-orientation": "\n".join([
        '<Carousel Classes="vertical align-start" Width="320" Height="196" Margin="0,48" HorizontalAlignment="Center" ViewportFraction="0.7449">',
        *[f'  {slide(n, wrap="4,4,4,8", size=30, line=36, square=False)}' for n in range(1, 6)],
        '</Carousel>']),
})

def command_item(kind, label, shortcut=None, disabled=False):
    attributes = (f' Shortcut="{shortcut}"' if shortcut else "") + (' IsEnabled="False"' if disabled else "")
    return f'<c:CommandItem{attributes}><StackPanel>{icon(kind, brush="CosmoMutedForegroundBrush")}<TextBlock Text="{label}" /></StackPanel></c:CommandItem>'


def command(classes="outline", extra=""):
    return "\n".join([
        f'<c:Command Classes="{classes}"{extra}>',
        '  <c:CommandGroup Header="Suggestions">',
        f'    {command_item("calendar", "Calendar")}',
        f'    {command_item("smile", "Search Emoji")}',
        f'    {command_item("calculator", "Calculator", disabled=classes == "outline")}',
        '  </c:CommandGroup>',
        '  <c:CommandSeparator />',
        '  <c:CommandGroup Header="Settings">',
        f'    {command_item("user", "Profile", "⌘P")}',
        f'    {command_item("credit-card", "Billing", "⌘B")}',
        f'    {command_item("settings", "Settings", "⌘S")}',
        '  </c:CommandGroup>',
        '</c:Command>'])


def combobox(height=None):
    box = "\n".join([
        '<c:Combobox PlaceholderText="Select framework..." SearchPlaceholder="Search framework..." Empty="No framework found.">',
        *[f'  <x:String>{name}</x:String>' for name in ["Next.js", "SvelteKit", "Nuxt.js", "Remix", "Astro"]],
        '</c:Combobox>'])
    return box if height is None else f'<Panel Height="{height}" VerticalAlignment="Top">\n{indent(box, "  ")}\n</Panel>'


FIXTURES.update({
    "command-demo": command(),
    "command-filtered": command(),
    "command-dialog-open": "\n".join([
        '<Panel Height="432" VerticalAlignment="Top">',
        '  <StackPanel Orientation="Horizontal" Spacing="4" VerticalAlignment="Top">',
        '    <TextBlock Text="Press" FontSize="14" LineHeight="20" VerticalAlignment="Center" Foreground="{DynamicResource CosmoMutedForegroundBrush}" />',
        '    <Border Height="20" Padding="6,0" CornerRadius="4" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" Background="{DynamicResource CosmoMutedBrush}" VerticalAlignment="Center">',
        '      <StackPanel Orientation="Horizontal" Spacing="4" VerticalAlignment="Center">',
        '        <TextBlock Text="⌘" FontSize="12" FontFamily="{DynamicResource CosmoFontMono}" FontWeight="Medium" Foreground="{DynamicResource CosmoMutedForegroundBrush}" VerticalAlignment="Center" />',
        '        <TextBlock Text="J" FontSize="10" FontFamily="{DynamicResource CosmoFontMono}" FontWeight="Medium" Foreground="{DynamicResource CosmoMutedForegroundBrush}" VerticalAlignment="Center" />',
        '      </StackPanel>',
        '    </Border>',
        '  </StackPanel>',
        '  <c:CommandDialog Gesture="Ctrl+J">',
        indent(command(classes=""), "    "),
        '  </c:CommandDialog>',
        '</Panel>']),
    "combobox-demo": combobox(),
    "combobox-open": combobox(300),
})

def data_table():
    def column(width, header, cell, extra=""):
        return "\n".join([
            f'      <TableViewColumn Width="{width}"{extra}>',
            f'        <TableViewColumn.Header>{header}</TableViewColumn.Header>',
            '        <TableViewColumn.CellTemplate>',
            f'          <DataTemplate x:DataType="m:Payment">{cell}</DataTemplate>',
            '        </TableViewColumn.CellTemplate>',
            '      </TableViewColumn>'])
    menu = "\n".join([
        '<Button.Flyout>',
        '  <MenuFlyout Placement="BottomEdgeAlignedRight">',
        '    <MenuItem Header="Actions" Classes="label" />',
        '    <MenuItem Header="Copy payment ID" />',
        '    <Separator />',
        '    <MenuItem Header="View customer" />',
        '    <MenuItem Header="View payment details" />',
        '  </MenuFlyout>',
        '</Button.Flyout>'])
    return "\n".join([
        '<StackPanel x:DataType="m:PaymentTable">',
        '  <StackPanel.DataContext><m:PaymentTable /></StackPanel.DataContext>',
        '  <DockPanel Margin="0,16">',
        '    <Button DockPanel.Dock="Right" Classes="outline has-icon">',
        f'      <StackPanel><TextBlock Text="Columns" />{icon("chevron-down")}</StackPanel>',
        '      <Button.Flyout>',
        '        <MenuFlyout Placement="BottomEdgeAlignedRight">',
        '          <MenuItem Header="Status" ToggleType="CheckBox" IsChecked="{Binding Source.Columns[1].IsVisible, Mode=TwoWay}" />',
        '          <MenuItem Header="Email" ToggleType="CheckBox" IsChecked="{Binding Source.Columns[2].IsVisible, Mode=TwoWay}" />',
        '          <MenuItem Header="Amount" ToggleType="CheckBox" IsChecked="{Binding Source.Columns[3].IsVisible, Mode=TwoWay}" />',
        '        </MenuFlyout>',
        '      </Button.Flyout>',
        '    </Button>',
        '    <TextBox Width="384" HorizontalAlignment="Left" PlaceholderText="Filter emails..." a:Web.Delay="300" Text="{Binding Source.FilterText}" />',
        '  </DockPanel>',
        '  <Border BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusMd}" ClipToBounds="True">',
        '    <TableView ItemsSource="{Binding Source.Rows}" c:DataTable.SelectsRows="True" c:DataTable.State="{Binding Source.State}">',
        '    <TableView.Columns>',
        column("32.84", '<CheckBox IsChecked="{Binding Source.AllPageRowsSelected}" AutomationProperties.Name="Select all" />', '<CheckBox IsChecked="{Binding IsSelected}" AutomationProperties.Name="Select row" />'),
        column("119.66", 'Status', '<TextBlock Text="{Binding Status, Converter={x:Static m:PaymentTable.Capitalize}}" />', ' IsVisible="{Binding Source.Columns[1].IsVisible}"'),
        column("271.55", f'<Button Classes="ghost has-icon" Command="{{Binding Source.SortCommand}}" CommandParameter="email"><StackPanel><TextBlock Text="Email" />{icon("arrow-up-down")}</StackPanel></Button>', '<TextBlock Text="{Binding Email, Converter={x:Static m:PaymentTable.Lower}}" />', ' IsVisible="{Binding Source.Columns[2].IsVisible}"'),
        column("100.23", '<TextBlock Text="Amount" HorizontalAlignment="Right" />', '<TextBlock Text="{Binding Amount, Converter={x:Static m:PaymentTable.Dollars}}" FontWeight="Medium" HorizontalAlignment="Right" />', ' IsVisible="{Binding Source.Columns[3].IsVisible}" HorizontalContentAlignment="Stretch"'),
        column("65.72", '', f'<Button Classes="ghost icon-sm" HorizontalAlignment="Left" AutomationProperties.Name="Open menu">{icon("ellipsis")}{menu}</Button>'),
        '    </TableView.Columns>',
        '    </TableView>',
        '  </Border>',
        '  <DockPanel Margin="0,16">',
        '    <StackPanel DockPanel.Dock="Right" Orientation="Horizontal" Spacing="8">',
        '      <Button Classes="outline sm" Content="Previous" Command="{Binding Source.PreviousPageCommand}" />',
        '      <Button Classes="outline sm" Content="Next" Command="{Binding Source.NextPageCommand}" />',
        '    </StackPanel>',
        '    <TextBlock FontSize="14" LineHeight="20" VerticalAlignment="Center" Foreground="{DynamicResource CosmoMutedForegroundBrush}">',
        '      <TextBlock.Text>',
        '        <MultiBinding StringFormat="{}{0} of {1} row(s) selected.">',
        '          <Binding Path="Source.SelectedCount" />',
        '          <Binding Path="Source.FilteredCount" />',
        '        </MultiBinding>',
        '      </TextBlock.Text>',
        '    </TextBlock>',
        '  </DockPanel>',
        '</StackPanel>'])


FIXTURES.update({
    "data-table-demo": data_table(),
    "data-table-selected": data_table(),
})

def sidebar_button(kind, label, active=False, tooltip=True):
    attributes = (' IsActive="True"' if active else "") + (f' Tooltip="{label}"' if tooltip else "")
    return f'<c:SidebarMenuButton{attributes}><StackPanel>{icon(kind, brush="CosmoSidebarForegroundBrush")}<TextBlock Text="{label}" /></StackPanel></c:SidebarMenuButton>'


def sidebar_lg(media, title, subtitle, chevron=16):
    return "\n".join([
        '<c:SidebarMenuButton Classes="lg">',
        '  <DockPanel>',
        f'    {icon("chevrons-up-down", chevron, "CosmoSidebarForegroundBrush", " DockPanel.Dock=" + chr(34) + "Right" + chr(34) + " Margin=" + chr(34) + "8,0,0,0" + chr(34))}',
        f'    {media}',
        '    <StackPanel Margin="8,0,0,0" VerticalAlignment="Center">',
        f'      <TextBlock Text="{title}" FontSize="14" LineHeight="17.5" FontWeight="Medium" TextTrimming="CharacterEllipsis" />',
        f'      <TextBlock Text="{subtitle}" FontSize="12" LineHeight="15" TextTrimming="CharacterEllipsis" />',
        '    </StackPanel>',
        '  </DockPanel>',
        '</c:SidebarMenuButton>'])


def sidebar(open_=True, variant="Sidebar"):
    logo = f'<Border Width="32" Height="32" CornerRadius="{{DynamicResource CosmoRadiusLg}}" Background="{{DynamicResource CosmoSidebarPrimaryBrush}}">{icon("gallery-vertical-end", 16, "CosmoSidebarPrimaryForegroundBrush")}</Border>'
    avatar = '<c:Avatar Width="32" Height="32" Content="CN" CornerRadius="{DynamicResource CosmoRadiusLg}" />'
    platform = [("square-terminal", "Playground"), ("bot", "Models"), ("book-open", "Documentation"), ("settings-2", "Settings")]
    items = []
    for kind, label in platform:
        if label == "Playground":
            items += ['<c:SidebarMenuItem IsCollapsible="True" IsExpanded="True">', f'  {sidebar_button(kind, label, True)}', '  <c:SidebarMenuSub>',
                      *[f'    <c:SidebarMenuSubButton Content="{sub}" />' for sub in ["History", "Starred", "Settings"]], '  </c:SidebarMenuSub>', '</c:SidebarMenuItem>']
        else:
            items += ['<c:SidebarMenuItem IsCollapsible="True">', f'  {sidebar_button(kind, label)}', '</c:SidebarMenuItem>']
    projects = [f'<c:SidebarMenuItem>{sidebar_button(kind, label, tooltip=False)}</c:SidebarMenuItem>' for kind, label in [("frame", "Design Engineering"), ("chart-pie", "Sales &amp; Marketing"), ("map", "Travel")]]
    tile = '<c:AspectRatio Ratio="1.7778"><Border CornerRadius="{DynamicResource CosmoRadiusXl}" Background="{DynamicResource CosmoMutedHalfBrush}" /></c:AspectRatio>'
    return "\n".join([
        f'<c:SidebarProvider Height="600" MobileBreakpoint="0" IsOpen="{open_}">',
        f'  <c:Sidebar Collapsible="Icon" Variant="{variant}">',
        '    <c:SidebarHeader>',
        '      <c:SidebarMenu>',
        '        <c:SidebarMenuItem>',
        indent(sidebar_lg(logo, "Acme Inc", "Enterprise", 16), " " * 10),
        '        </c:SidebarMenuItem>',
        '      </c:SidebarMenu>',
        '    </c:SidebarHeader>',
        '    <c:SidebarContent>',
        '      <c:SidebarGroup>',
        '        <c:SidebarGroupLabel Content="Platform" />',
        '        <c:SidebarMenu>',
        indent("\n".join(items), " " * 10),
        '        </c:SidebarMenu>',
        '      </c:SidebarGroup>',
        '      <c:SidebarGroup Classes="icon-hidden">',
        '        <c:SidebarGroupLabel Content="Projects" />',
        '        <c:SidebarMenu>',
        indent("\n".join(projects), " " * 10),
        '        </c:SidebarMenu>',
        '      </c:SidebarGroup>',
        '    </c:SidebarContent>',
        '    <c:SidebarFooter>',
        '      <c:SidebarMenu>',
        '        <c:SidebarMenuItem>',
        indent(sidebar_lg(avatar, "shadcn", "m@example.com"), " " * 10),
        '        </c:SidebarMenuItem>',
        '      </c:SidebarMenu>',
        '    </c:SidebarFooter>',
        '    <c:SidebarRail />',
        '  </c:Sidebar>',
        '  <c:SidebarInset>',
        '    <DockPanel>',
        '      <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Height="64" Spacing="8" Margin="12,0,16,0">',
        '        <c:SidebarTrigger VerticalAlignment="Center" />',
        '        <Separator Classes="vertical" Height="16" Margin="0,0,8,0" VerticalAlignment="Center" />',
        '        <c:Breadcrumb VerticalAlignment="Center">',
        '          <HyperlinkButton Content="Platform" />',
        '          <c:BreadcrumbPage Text="Playground" />',
        '        </c:Breadcrumb>',
        '      </StackPanel>',
        '      <DockPanel Margin="16,0,16,16">',
        '        <UniformGrid DockPanel.Dock="Top" Columns="3" ColumnSpacing="16" Margin="0,0,0,16">',
        f'          {tile}',
        f'          {tile}',
        f'          {tile}',
        '        </UniformGrid>',
        '        <Border CornerRadius="{DynamicResource CosmoRadiusXl}" Background="{DynamicResource CosmoMutedHalfBrush}" />',
        '      </DockPanel>',
        '    </DockPanel>',
        '  </c:SidebarInset>',
        '</c:SidebarProvider>'])


FIXTURES.update({
    "sidebar-demo": sidebar(),
    "sidebar-icon": sidebar(open_=False),
    "sidebar-floating": sidebar(variant="Floating"),
    "sidebar-inset": sidebar(variant="Inset"),
})

def calendar(attributes="", extra="", children=""):
    body = [
        '<Border HorizontalAlignment="Left" BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" BoxShadow="{DynamicResource CosmoShadowSm}" Background="{DynamicResource CosmoBackgroundBrush}">',
        f'  <Calendar DisplayDate="2025-06-12"{attributes}{extra}' + (">" if children else " />"),
    ]
    if children:
        body += [f"    {children}", "  </Calendar>"]
    return "\n".join(body + ["</Border>"])


FIXTURES.update({
    "calendar-single": calendar(' SelectionMode="SingleDate" SelectedDate="2025-06-12"'),
    "calendar-range": calendar(' SelectionMode="SingleRange" c:CalendarSelection.RangeStart="2025-06-09" c:CalendarSelection.RangeEnd="2025-06-26"'),
    "calendar-multiple": calendar(' SelectionMode="MultipleRange"', children='<c:CalendarSelection.Dates><c:CalendarDates><sys:DateTime>2025-06-03</sys:DateTime><sys:DateTime>2025-06-12</sys:DateTime><sys:DateTime>2025-06-13</sys:DateTime><sys:DateTime>2025-06-24</sys:DateTime></c:CalendarDates></c:CalendarSelection.Dates>'),
    "calendar-disabled": calendar(' SelectionMode="SingleDate" SelectedDate="2025-06-12"', children='<Calendar.BlackoutDates><CalendarDateRange><x:Arguments><sys:DateTime>2025-05-01</sys:DateTime><sys:DateTime>2025-06-11</sys:DateTime></x:Arguments></CalendarDateRange></Calendar.BlackoutDates>'),
    "calendar-outside-hidden": calendar(' SelectionMode="SingleDate" SelectedDate="2025-06-12" Classes="outside-hidden"'),
    "calendar-dropdown": calendar(' SelectionMode="SingleDate" SelectedDate="2025-06-12" Classes="dropdown"'),
    "calendar-week-numbers": calendar(' SelectionMode="SingleDate" SelectedDate="2025-06-12" Classes="week-numbers"'),
})

FIXTURES.update({
    "date-picker-demo": '<CalendarDatePicker Width="240" DisplayDate="2025-06-12" />',
    "date-picker-open": '<Panel Height="372" VerticalAlignment="Top">\n  <CalendarDatePicker Width="240" DisplayDate="2025-06-12" SelectedDate="2025-06-12" />\n</Panel>',
    "date-picker-input": "\n".join([
        '<StackPanel Spacing="12">',
        '  <Label Content="Subscription Date" Padding="4,0" Target="Picker" />',
        '  <CalendarDatePicker Name="Picker" Classes="input" HorizontalAlignment="Stretch" DisplayDate="2025-06-01" SelectedDate="2025-06-01" />',
        '</StackPanel>']),
    "date-picker-range-open": '<Panel Height="372" VerticalAlignment="Top">\n  <c:DateRangePicker Width="300" RangeStart="2022-01-20" RangeEnd="2022-02-09" />\n</Panel>',
    "date-picker-presets-open": "\n".join([
        '<Panel Height="420" VerticalAlignment="Top">',
        '  <CalendarDatePicker Width="240" DisplayDate="2025-06-12" SelectedDate="2025-06-12">',
        '    <c:DatePresets.Items>',
        '      <c:DatePresetList>',
        '        <c:DatePreset Days="0">Today</c:DatePreset>',
        '        <c:DatePreset Days="1">Tomorrow</c:DatePreset>',
        '        <c:DatePreset Days="3">In 3 days</c:DatePreset>',
        '        <c:DatePreset Days="7">In a week</c:DatePreset>',
        '      </c:DatePresetList>',
        '    </c:DatePresets.Items>',
        '  </CalendarDatePicker>',
        '</Panel>']),
})

PAGE = """<UserControl xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:a="https://aioros.dev" xmlns:c="https://aioros.dev/cosmo" xmlns:m="using:Gallery.Models"{sys}{pictor}{model} a:Page.Title="Fidelity: {name}">
  <Border a:Web.Id="fixture" Width="640" Padding="24" HorizontalAlignment="Left" VerticalAlignment="Top" Background="{{DynamicResource CosmoBackgroundBrush}}">
{body}
  </Border>
</UserControl>
"""

SHELL = PAGE.replace('Width="640" Padding="24"', f'Width="{shells.WIDTH}" Height="{shells.HEIGHT}" Padding="0" ClipToBounds="True"')

DICTIONARY = """<ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:a="https://aioros.dev" xmlns:c="https://aioros.dev/cosmo" xmlns:m="using:Gallery.Models" xmlns:sys="using:System" xmlns:p="https://aioros.dev/pictor">
{templates}
</ResourceDictionary>
"""


FIXTURES.update({
    "button-extra-sizes": "\n".join([
        '<WrapPanel ItemSpacing="8" LineSpacing="8">',
        '  <Button Classes="xs" Content="Extra small" />',
        f'  <Button Classes="xs has-icon"><StackPanel>{icon("chevron-right", 12, "CosmoPrimaryForegroundBrush")}<TextBlock Text="Next" /></StackPanel></Button>',
        *[f'  <Button Classes="{size}" AutomationProperties.Name="Next">{icon("chevron-right", 12 if size == "icon-xs" else 16, "CosmoPrimaryForegroundBrush")}</Button>' for size in ["icon-xs", "icon-sm", "icon-lg"]],
        '  <Button Classes="outline xs" Content="Extra small" />',
        *[f'  <Button Classes="outline {size}" AutomationProperties.Name="Next">{icon("chevron-right", 12 if size == "icon-xs" else 16)}</Button>' for size in ["icon-xs", "icon-sm", "icon-lg"]],
        '</WrapPanel>']),
})

FIXTURES.update({
    "field-content": """
        <c:FieldGroup Width="448" HorizontalAlignment="Left">
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
        """,
    "field-responsive": """
        <StackPanel Spacing="24">
          <c:FieldGroup Width="560" HorizontalAlignment="Left">
            <c:Field Classes="responsive">
              <c:FieldContent>
                <c:FieldLabel Content="Name" Target="{Binding #wide}" />
                <c:FieldDescription Text="Provide your full name for identification." />
              </c:FieldContent>
              <TextBox Name="wide" a:Web.Id="wide" MinWidth="200" PlaceholderText="Evil Rabbit" />
            </c:Field>
          </c:FieldGroup>
          <c:FieldGroup Width="400" HorizontalAlignment="Left">
            <c:Field Classes="responsive">
              <c:FieldContent>
                <c:FieldLabel Content="Name" Target="{Binding #narrow}" />
                <c:FieldDescription Text="Provide your full name for identification." />
              </c:FieldContent>
              <TextBox Name="narrow" a:Web.Id="narrow" MinWidth="200" PlaceholderText="Evil Rabbit" />
            </c:Field>
          </c:FieldGroup>
        </StackPanel>
        """,
    "button-invalid": """
        <StackPanel Spacing="16" HorizontalAlignment="Left">
          <Button Classes="outline" Content="Pick a date" HorizontalAlignment="Left">
            <DataValidationErrors.Error>
              <sys:Exception>
                <x:Arguments>
                  <x:String>Pick a date.</x:String>
                </x:Arguments>
              </sys:Exception>
            </DataValidationErrors.Error>
          </Button>
          <c:Field Classes="invalid" HorizontalAlignment="Left">
            <Button Classes="outline" Content="Choose a plan" />
          </c:Field>
          <Button Content="Submit" HorizontalAlignment="Left">
            <DataValidationErrors.Error>
              <sys:Exception>
                <x:Arguments>
                  <x:String>Fix the errors above.</x:String>
                </x:Arguments>
              </sys:Exception>
            </DataValidationErrors.Error>
          </Button>
        </StackPanel>
        """,
})

FIXTURES.update({
    "badge-links": """
        <WrapPanel ItemSpacing="8" LineSpacing="8">
          <HyperlinkButton Theme="{DynamicResource CosmoBadgeLink}" NavigateUri="#" Content="Badge" />
          <HyperlinkButton Theme="{DynamicResource CosmoBadgeLink}" Classes="secondary" NavigateUri="#" Content="Badge" />
          <HyperlinkButton Theme="{DynamicResource CosmoBadgeLink}" Classes="destructive" NavigateUri="#" Content="Badge" />
          <HyperlinkButton Theme="{DynamicResource CosmoBadgeLink}" Classes="outline" NavigateUri="#" Content="Badge" />
          <HyperlinkButton Theme="{DynamicResource CosmoBadgeLink}" Classes="ghost" NavigateUri="#" Content="Badge" />
          <HyperlinkButton Theme="{DynamicResource CosmoBadgeLink}" Classes="link" NavigateUri="#" Content="Badge" />
        </WrapPanel>
        """,
    "card-separated": """
        <c:Card Width="384" HorizontalAlignment="Left">
          <c:CardHeader Classes="border" Title="Team members" Description="Invite your team to collaborate." />
          <c:CardContent>
            <TextBlock Text="Members can edit every project." FontSize="14" LineHeight="20" TextWrapping="Wrap" />
          </c:CardContent>
          <c:CardFooter Classes="border">
            <Button Content="Invite" HorizontalAlignment="Left" />
          </c:CardFooter>
        </c:Card>
        """,
})

FIXTURES.update({
    "select-scrollable": time_zones("zone", None),
    "select-scrolled": time_zones("zone", 25),
    "slider-range": """
        <StackPanel Orientation="Horizontal" Spacing="48" VerticalAlignment="Top">
          <c:Field Width="360" VerticalAlignment="Top">
            <c:FieldTitle Text="Price Range" />
            <c:FieldDescription>
              <Run Text="Set your budget range ($" /><Run Text="200" FontWeight="Medium" /><Run Text=" -" /><Run Text="800" FontWeight="Medium" /><Run Text=")." />
            </c:FieldDescription>
            <c:RangeSlider Margin="0,8,0,0" Minimum="0" Maximum="1000" TickFrequency="10" IsSnapToTickEnabled="True" LowerValue="200" UpperValue="800" AutomationProperties.Name="Price Range" />
          </c:Field>
          <c:RangeSlider Orientation="Vertical" Maximum="100" TickFrequency="1" IsSnapToTickEnabled="True" LowerValue="20" UpperValue="60" VerticalAlignment="Top" AutomationProperties.Name="Levels" />
        </StackPanel>
        """,
})

FIXTURES.update({
    "input-file": """
        <StackPanel Spacing="12" Width="384" HorizontalAlignment="Left">
          <Label Content="Picture" Target="{Binding #picture}" />
          <a:FilePicker Name="picture" a:Web.Id="picture" Content="Picture" />
        </StackPanel>
        """,
    "item-header-footer": """
        <StackPanel Width="448" HorizontalAlignment="Left">
          <c:Item Classes="outline" Title="Acme Inc." Description="Design system subscription, October.">
            <c:Item.Header>
              <c:ItemHeader>
                <TextBlock Text="Invoice #1042" FontSize="14" LineHeight="20" FontWeight="Medium" />
                <c:Badge Classes="secondary" Content="Paid" />
              </c:ItemHeader>
            </c:Item.Header>
            <c:Item.Actions>
              <Button Classes="outline sm" Content="View" />
            </c:Item.Actions>
            <c:Item.Footer>
              <c:ItemFooter>
                <TextBlock Text="Due Oct 31" FontSize="14" LineHeight="20" Foreground="{DynamicResource CosmoMutedForegroundBrush}" />
                <TextBlock Text="$249.00" FontSize="14" LineHeight="20" FontWeight="Medium" />
              </c:ItemFooter>
            </c:Item.Footer>
          </c:Item>
        </StackPanel>
        """,
})

FIXTURES.update(charts.FIXTURES)
FIXTURES.update(blocks.FIXTURES)
FIXTURES.update(shells.FIXTURES)
FIXTURES.update(chat.FIXTURES)
FIXTURES.update(codeblocks.FIXTURES)
FIXTURES.update(traces.FIXTURES)
FIXTURES.update(questionnaires.FIXTURES)


LIVE = {"CodeBlockStreaming", "CodeBlockMarkdownExample", "TraceThinkingExample", "TraceToolCallsExample", "TraceDemoExample"}
"""The models that play a stream back: their pages stream each change to the browser (Aioros's live pages)."""


def write():
    charts.MODEL.write_text(charts.model())
    templates = []
    for name, xaml in FIXTURES.items():
        body = dedent(xaml).strip("\n")
        folder = PAGES / name
        folder.mkdir(parents=True, exist_ok=True)
        model = re.search(r'\n *<[A-Za-z:]+\.DataContext><m:([A-Za-z]+) /></[A-Za-z:]+\.DataContext>', body)
        page = body.replace(model.group(0), "") if model else body
        datatype = f' x:DataType="m:{model.group(1)}"' if model else ""
        datatype += ' a:Web.Live="True"' if model and model.group(1) in LIVE else ""
        sys = ' xmlns:sys="using:System"' if "sys:" in body else ""
        pictor = ' xmlns:p="https://aioros.dev/pictor"' if "<p:" in body else ""
        frame = SHELL if name in shells.FIXTURES else PAGE
        (folder / "Page.axaml").write_text(frame.format(name=name, body=indent(page, " " * 4), model=datatype, sys=sys, pictor=pictor))
        typed = re.sub(r"^<([\w:.]+)", lambda root: f'<{root.group(1)} x:DataType="m:{model.group(1)}"', body, count=1) if model and not re.match(r"^<[^>]*x:DataType=", body) else body
        templates.append(f'  <DataTemplate x:Key="{name}">\n{indent(re.sub(r' a:Drag\.[A-Za-z]+="[^"]*"', "", typed).replace("avares://Gallery/", "avares://Aioros.Cosmo.Tests/"), " " * 4)}\n  </DataTemplate>')
    TESTS.parent.mkdir(parents=True, exist_ok=True)
    TESTS.write_text(DICTIONARY.format(templates="\n".join(templates)))


if __name__ == "__main__":
    write()
