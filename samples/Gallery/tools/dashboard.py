"""shadcn/ui's dashboard-01 block composed from Cosmo's components: the inset sidebar, the site header, the section cards,
the visitors chart, and the document's table with its tabs, columns menu, pagination and the section viewer. Its data lives in
the Gallery's DashboardBlock model (the registry's data.json and chart data); shells.py frames it as a 1280 x 800 shell."""
from shells import block_indent, button, data, dock, esc, group, icon, icon_label, lines, menu, more_button, more_menu, nav_user, pinned, secondary

DRAG = ' a:Drag.Item="{Binding Id}"'

STYLES = lines(
    '<Style Selector="c|SidebarMenuButton.quick">',
    '  <Setter Property="Background" Value="{DynamicResource CosmoPrimaryBrush}" />',
    '  <Setter Property="Foreground" Value="{DynamicResource CosmoPrimaryForegroundBrush}" />',
    '  <Setter Property="MinWidth" Value="32" />',
    '</Style>',
    '<Style Selector="c|SidebarMenuButton.quick:pointerover, c|SidebarMenuButton.quick:pressed">',
    '  <Setter Property="Background" Value="{DynamicResource CosmoButtonPrimaryHoverBrush}" />',
    '  <Setter Property="Foreground" Value="{DynamicResource CosmoPrimaryForegroundBrush}" />',
    '</Style>',
    '<Style Selector="c|Card.section">',
    '  <Setter Property="Background">',
    '    <LinearGradientBrush StartPoint="0%,100%" EndPoint="0%,0%">',
    '      <GradientStop Offset="0" Color="{DynamicResource CosmoSectionCardBottomColor}" />',
    '      <GradientStop Offset="1" Color="{DynamicResource CosmoSectionCardTopColor}" />',
    '    </LinearGradientBrush>',
    '  </Setter>',
    '</Style>',
    '<Style Selector="ListBox.toggle-group.range > ListBoxItem">',
    '  <Setter Property="Padding" Value="16,0" />',
    '</Style>',
    '<Style Selector="TextBox.cell">',
    '  <Setter Property="Height" Value="32" />',
    '  <Setter Property="MinHeight" Value="32" />',
    '  <Setter Property="Width" Value="64" />',
    '  <Setter Property="HorizontalContentAlignment" Value="Right" />',
    '  <Setter Property="TextAlignment" Value="Right" />',
    '  <Setter Property="Background" Value="Transparent" />',
    '  <Setter Property="BorderBrush" Value="Transparent" />',
    '</Style>',
    '<Style Selector="TextBox.cell /template/ Border#PART_BorderElement">',
    '  <Setter Property="BoxShadow" Value="none" />',
    '</Style>',
    '<Style Selector="TextBox.cell:pointerover /template/ Border#PART_BorderElement">',
    '  <Setter Property="Background" Value="{DynamicResource CosmoInputHoverBackgroundBrush}" />',
    '</Style>',
    '<Style Selector="c|Badge.cell">',
    '  <Setter Property="Padding" Value="6,2" />',
    '  <Setter Property="Foreground" Value="{DynamicResource CosmoMutedForegroundBrush}" />',
    '</Style>',
    '<Style Selector="c|Badge.count">',
    '  <Setter Property="Width" Value="20" />',
    '  <Setter Property="Height" Value="20" />',
    '  <Setter Property="Padding" Value="4,0" />',
    '  <Setter Property="CornerRadius" Value="9999" />',
    '  <Setter Property="Background" Value="{DynamicResource CosmoMutedForegroundThirdBrush}" />',
    '</Style>')

RESOURCES = lines(
    '<ResourceDictionary>',
    '  <ResourceDictionary.ThemeDictionaries>',
    '    <ResourceDictionary x:Key="Light"><SolidColorBrush x:Key="DashboardDoneBrush" Color="#00C950" /></ResourceDictionary>',
    '    <ResourceDictionary x:Key="Dark"><SolidColorBrush x:Key="DashboardDoneBrush" Color="#05DF72" /></ResourceDictionary>',
    '  </ResourceDictionary.ThemeDictionaries>',
    '  <c:ChartConfig x:Key="VisitorsConfig"><c:ChartConfigItem Key="Visitors" Label="Visitors" /><c:ChartConfigItem Key="Desktop" Label="Desktop" Color="PictorPrimary" /><c:ChartConfigItem Key="Mobile" Label="Mobile" Color="PictorPrimary" /></c:ChartConfig>',
    '</ResourceDictionary>')


def sidebar(source):
    """The inset sidebar: the company's link, Quick Create beside the inbox, the main links, the documents, the secondary links pinned to the foot and the user."""
    brand = lines(
        '<c:SidebarMenu>',
        '  <c:SidebarMenuItem>',
        f'    <c:SidebarMenuButton Padding="6"><DockPanel>{icon("IconInnerShadowTop", dock("Left") + chr(32) + "Margin=" + chr(34) + "0,0,8,0" + chr(34), size=20)}<TextBlock Text="Acme Inc." FontSize="16" LineHeight="24" FontWeight="SemiBold" VerticalAlignment="Center" /></DockPanel></c:SidebarMenuButton>',
        '  </c:SidebarMenuItem>',
        '</c:SidebarMenu>')
    quick = lines(
        '<c:SidebarMenu>',
        '  <c:SidebarMenuItem>',
        '    <DockPanel>',
        f'      <Button Classes="outline icon" Width="32" Height="32" Margin="8,0,0,0" DockPanel.Dock="Right" AutomationProperties.Name="Inbox">{icon("IconMail", brush="CosmoForegroundBrush")}</Button>',
        f'      <c:SidebarMenuButton Classes="quick" Tooltip="Quick Create"><DockPanel>{icon("IconCirclePlusFilled", dock("Left") + chr(32) + "Margin=" + chr(34) + "0,0,8,0" + chr(34), brush="CosmoPrimaryForegroundBrush")}<TextBlock Text="Quick Create" VerticalAlignment="Center" /></DockPanel></c:SidebarMenuButton>',
        '    </DockPanel>',
        '  </c:SidebarMenuItem>',
        '</c:SidebarMenu>')
    links = menu([f'<c:SidebarMenuItem>{button(item["icon"], item["title"])}</c:SidebarMenuItem>' for item in source["navMain"]])
    main = lines('<c:SidebarGroup>', '  <StackPanel Spacing="8">', block_indent(quick, 4), block_indent(links, 4), '  </StackPanel>', '</c:SidebarGroup>')
    entries = [("Open", "IconFolder"), ("Share", "IconShare3"), None, ("Delete", "IconTrash", "destructive")]
    documents = group(menu([lines('<c:SidebarMenuItem>', f'  {button(item["icon"], item["name"], tooltip=False)}', more_menu(entries, 96), '</c:SidebarMenuItem>') for item in source["documents"]]
                           + [more_button("IconDots")]), "Documents", "icon-hidden")
    return lines(
        '<c:Sidebar Variant="Inset" ExpandedWidth="288">',
        '  <c:SidebarHeader>',
        block_indent(brand, 4),
        '  </c:SidebarHeader>',
        '  <c:SidebarContent>',
        block_indent(pinned(main, documents, secondary(source["navSecondary"], size=None)), 4),
        '  </c:SidebarContent>',
        '  <c:SidebarFooter>',
        block_indent(nav_user(dashboard=True), 4),
        '  </c:SidebarFooter>',
        '</c:Sidebar>')


HEADER = lines(
    '<Border DockPanel.Dock="Top" Height="48" BorderThickness="0,0,0,1" BorderBrush="{DynamicResource CosmoBorderBrush}">',
    '  <DockPanel Margin="24,0">',
    '    <HyperlinkButton DockPanel.Dock="Right" Classes="ghost sm" Content="GitHub" NavigateUri="https://github.com/shadcn-ui/ui/tree/main/apps/v4/app/(examples)/dashboard" VerticalAlignment="Center" />',
    '    <StackPanel Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">',
    '      <c:SidebarTrigger Margin="-4,0,0,0" VerticalAlignment="Center" />',
    '      <Separator Classes="vertical" Height="16" Margin="8,0" VerticalAlignment="Center" />',
    '      <TextBlock Text="Documents" FontSize="16" LineHeight="24" FontWeight="Medium" VerticalAlignment="Center" />',
    '    </StackPanel>',
    '  </DockPanel>',
    '</Border>')

CARDS = [
    ("Total Revenue", "$1,250.00", "IconTrendingUp", "+12.5%", "Trending up this month", "Visitors for the last 6 months"),
    ("New Customers", "1,234", "IconTrendingDown", "-20%", "Down 20% this period", "Acquisition needs attention"),
    ("Active Accounts", "45,678", "IconTrendingUp", "+12.5%", "Strong user retention", "Engagement exceed targets"),
    ("Growth Rate", "4.5%", "IconTrendingUp", "+4.5%", "Steady performance increase", "Meets growth projections"),
]


def section_card(index, card):
    title, value, trend, change, line, note = card
    position = f' Grid.Row="{index // 2}" Grid.Column="{index % 2}"'
    return lines(
        f'<c:Card Classes="section"{position}>',
        '  <c:CardHeader>',
        '    <c:CardHeader.Title>',
        '      <StackPanel Spacing="8">',
        f'        <TextBlock Text="{title}" FontSize="14" LineHeight="20" FontWeight="Normal" Foreground="{{DynamicResource CosmoMutedForegroundBrush}}" />',
        f'        <TextBlock Text="{esc(value)}" FontSize="30" LineHeight="36" FontWeight="SemiBold" FontFeatures="+tnum" />',
        '      </StackPanel>',
        '    </c:CardHeader.Title>',
        f'    <c:CardHeader.Action><c:Badge Classes="outline"><StackPanel>{icon(trend, size=12, brush="CosmoForegroundBrush")}<TextBlock Text="{change}" /></StackPanel></c:Badge></c:CardHeader.Action>',
        '  </c:CardHeader>',
        '  <c:CardFooter>',
        '    <StackPanel Spacing="6">',
        f'      <StackPanel Orientation="Horizontal" Spacing="8"><TextBlock Text="{line}" FontSize="14" LineHeight="20" FontWeight="Medium" />{icon(trend, " VerticalAlignment=" + chr(34) + "Center" + chr(34), brush="CosmoForegroundBrush")}</StackPanel>',
        f'      <TextBlock Text="{note}" FontSize="14" LineHeight="20" Foreground="{{DynamicResource CosmoMutedForegroundBrush}}" />',
        '    </StackPanel>',
        '  </c:CardFooter>',
        '</c:Card>')


def cards():
    return lines('<Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto" ColumnSpacing="16" RowSpacing="16" Margin="24,0">',
                 *[block_indent(section_card(index, card), 2) for index, card in enumerate(CARDS)], '</Grid>')


def area(key, top, bottom):
    return lines(
        f'<p:Area DataKey="{key}" Type="Natural" StackId="a" IsAnimationActive="False">',
        '  <p:Area.Fill>',
        '    <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">',
        f'      <GradientStop Offset="0.05" Color="{{DynamicResource {top}}}" />',
        f'      <GradientStop Offset="0.95" Color="{{DynamicResource {bottom}}}" />',
        '    </LinearGradientBrush>',
        '  </p:Area.Fill>',
        '</p:Area>')


CHART = lines(
    '<c:Card Margin="24,0">',
    '  <c:CardHeader Title="Total Visitors" Description="Total for the last 3 months">',
    '    <c:CardHeader.Action>',
    '      <ListBox Classes="toggle-group outline range" SelectionMode="Single,AlwaysSelected" SelectedIndex="{Binding Visitors.TimeRange}" AutomationProperties.Name="Time range">',
    '        <ListBoxItem Content="Last 3 months" />',
    '        <ListBoxItem Content="Last 30 days" />',
    '        <ListBoxItem Content="Last 7 days" />',
    '      </ListBox>',
    '    </c:CardHeader.Action>',
    '  </c:CardHeader>',
    '  <c:CardContent Padding="24,24,24,0">',
    '    <p:AreaChart c:ChartConfig.Config="{StaticResource VisitorsConfig}" Data="{Binding Visitors.Range}" Height="250" Margin="0,0,0,0">',
    '      <p:CartesianGrid Vertical="False" />',
    '      <p:XAxis DataKey="Date" TickLine="False" AxisLine="False" TickMargin="8" MinTickGap="32" TickFormat="{}{0:MMM d}" />',
    '      <p:Tooltip Cursor="False" LabelFormat="{}{0:MMM d}"><p:Tooltip.ContentTemplate><c:ChartTooltipContent Indicator="Dot" /></p:Tooltip.ContentTemplate></p:Tooltip>',
    block_indent(area("Mobile", "CosmoPrimaryTopColor", "CosmoPrimaryBottomColor"), 6),
    block_indent(area("Desktop", "CosmoPrimaryColor", "CosmoPrimaryBottomColor"), 6),
    '    </p:AreaChart>',
    '  </c:CardContent>',
    '</c:Card>')


def column(width, header, cell, extra=""):
    return lines(
        f'<TableViewColumn Width="{width}"{extra}>',
        f'  <TableViewColumn.Header>{header}</TableViewColumn.Header>',
        '  <TableViewColumn.CellTemplate>',
        f'    <DataTemplate x:DataType="m:DashboardSection">{cell}</DataTemplate>',
        '  </TableViewColumn.CellTemplate>',
        '</TableViewColumn>')


def visible(index):
    return f' IsVisible="{{Binding Sections.Columns[{index}].IsVisible}}"'



ROW_MENU = lines(
    '<Button.Flyout>',
    '  <MenuFlyout Placement="BottomEdgeAlignedRight" FlyoutPresenterClasses="w-32">',
    '    <MenuItem Header="Edit" />',
    '    <MenuItem Header="Make a copy" />',
    '    <MenuItem Header="Favorite" />',
    '    <Separator />',
    '    <MenuItem Header="Delete" Classes="destructive" />',
    '  </MenuFlyout>',
    '</Button.Flyout>')


def table():
    reviewer = lines(
        '<Panel>',
        '  <TextBlock Text="{Binding Reviewer}" IsVisible="{Binding IsAssigned}" VerticalAlignment="Center" />',
        '  <ComboBox Classes="sm" Width="152" PlaceholderText="Assign reviewer" SelectedItem="{Binding Assignee}" IsVisible="{Binding !IsAssigned}" AutomationProperties.Name="Reviewer">',
        '    <x:String>Eddie Lake</x:String>',
        '    <x:String>Jamik Tashpulatov</x:String>',
        '  </ComboBox>',
        '</Panel>')
    status = (f'<c:Badge Classes="outline cell"><StackPanel>'
              f'<Panel>{icon("IconCircleCheckFilled", " IsVisible=" + chr(34) + "{Binding IsDone}" + chr(34), size=16, brush="DashboardDoneBrush")}'
              f'{icon("IconLoader", " IsVisible=" + chr(34) + "{Binding !IsDone}" + chr(34), size=16, brush="CosmoMutedForegroundBrush")}</Panel>'
              '<TextBlock Text="{Binding Status}" /></StackPanel></c:Badge>')
    columns = [
        column("44", "", f'<Border Classes="grip" Width="28" Height="28" CornerRadius="{{DynamicResource CosmoRadiusMd}}" Background="Transparent" HorizontalAlignment="Left"{DRAG} AutomationProperties.Name="Drag to reorder">{icon("IconGripVertical", size=12, brush="CosmoMutedForegroundBrush")}</Border>'),
        column("24", '<CheckBox IsChecked="{Binding Sections.AllPageRowsSelected}" HorizontalAlignment="Center" AutomationProperties.Name="Select all" />',
               '<CheckBox IsChecked="{Binding IsSelected}" HorizontalAlignment="Center" AutomationProperties.Name="Select row" />'),
        column("306.27", "Header", f'<Button Classes="link" Padding="0" Content="{{Binding Header}}" Foreground="{{DynamicResource CosmoForegroundBrush}}" Command="{{Binding View}}" />'),
        column("144", "Section Type", '<Border Width="128" HorizontalAlignment="Left"><c:Badge Classes="outline cell" Content="{Binding Type}" HorizontalAlignment="Left" /></Border>', visible(3)),
        column("105.06", "Status", status, visible(4)),
        column("80", '<TextBlock Text="Target" HorizontalAlignment="Right" />', '<TextBox Classes="cell" Text="{Binding Target}" AutomationProperties.Name="Target" />', visible(5) + ' HorizontalContentAlignment="Stretch"'),
        column("80", '<TextBlock Text="Limit" HorizontalAlignment="Right" />', '<TextBox Classes="cell" Text="{Binding Limit}" AutomationProperties.Name="Limit" />', visible(6) + ' HorizontalContentAlignment="Stretch"'),
        column("168", "Reviewer", reviewer, visible(7)),
        column("48", "", f'<Button Classes="ghost icon" Width="32" Height="32" HorizontalAlignment="Left" AutomationProperties.Name="Open menu">{icon("IconDotsVertical", brush="CosmoMutedForegroundBrush")}\n{ROW_MENU}</Button>'),
    ]
    columns_menu = "\n".join(
        f'    <MenuItem Header="{key.capitalize()}" ToggleType="CheckBox" IsChecked="{{Binding Sections.Columns[{index}].IsVisible, Mode=TwoWay}}" />'
        for index, key in [(3, "type"), (4, "status"), (5, "target"), (6, "limit"), (7, "reviewer")])
    pager = lines(
        '<DockPanel Margin="16,0">',
        '  <StackPanel DockPanel.Dock="Right" Orientation="Horizontal" Spacing="32">',
        '    <StackPanel Orientation="Horizontal" Spacing="8">',
        '      <Label Content="Rows per page" Target="{Binding #rows}" VerticalAlignment="Center" />',
        '      <ComboBox Name="rows" Classes="sm" Width="80" ItemsSource="{Binding PageSizes}" SelectedItem="{Binding Sections.PageSize}" />',
        '    </StackPanel>',
        '    <TextBlock FontSize="14" LineHeight="20" FontWeight="Medium" VerticalAlignment="Center">',
        '      <TextBlock.Text><MultiBinding StringFormat="Page {0} of {1}"><Binding Path="Sections.PageNumber" /><Binding Path="Sections.PageCount" /></MultiBinding></TextBlock.Text>',
        '    </TextBlock>',
        '    <StackPanel Orientation="Horizontal" Spacing="8">',
        f'      <Button Classes="outline icon" Width="32" Height="32" Command="{{Binding Sections.FirstPageCommand}}" AutomationProperties.Name="Go to first page">{icon("IconChevronsLeft", brush="CosmoForegroundBrush")}</Button>',
        f'      <Button Classes="outline icon" Width="32" Height="32" Command="{{Binding Sections.PreviousPageCommand}}" AutomationProperties.Name="Go to previous page">{icon("IconChevronLeft", brush="CosmoForegroundBrush")}</Button>',
        f'      <Button Classes="outline icon" Width="32" Height="32" Command="{{Binding Sections.NextPageCommand}}" AutomationProperties.Name="Go to next page">{icon("IconChevronRight", brush="CosmoForegroundBrush")}</Button>',
        f'      <Button Classes="outline icon" Width="32" Height="32" Command="{{Binding Sections.LastPageCommand}}" AutomationProperties.Name="Go to last page">{icon("IconChevronsRight", brush="CosmoForegroundBrush")}</Button>',
        '    </StackPanel>',
        '  </StackPanel>',
        '  <TextBlock FontSize="14" LineHeight="20" VerticalAlignment="Center" Foreground="{DynamicResource CosmoMutedForegroundBrush}">',
        '    <TextBlock.Text><MultiBinding StringFormat="{}{0} of {1} row(s) selected."><Binding Path="Sections.SelectedCount" /><Binding Path="Sections.FilteredCount" /></MultiBinding></TextBlock.Text>',
        '  </TextBlock>',
        '</DockPanel>')
    outline = lines(
        '<StackPanel Spacing="16">',
        '  <Border BorderThickness="1" BorderBrush="{DynamicResource CosmoBorderBrush}" CornerRadius="{DynamicResource CosmoRadiusLg}" ClipToBounds="True" a:Drag.Target="sections" a:Drag.Command="{Binding Move}">',
        '    <TableView Classes="muted-header" ItemsSource="{Binding Sections.Rows}" c:DataTable.SelectsRows="True" c:DataTable.State="{Binding Sections.State}">',
        '      <TableView.Columns>',
        block_indent("\n".join(columns), 8),
        '      </TableView.Columns>',
        '    </TableView>',
        '  </Border>',
        block_indent(pager, 2),
        '</StackPanel>')
    placeholder = '<c:AspectRatio Ratio="1.7778"><Rectangle Stroke="{DynamicResource CosmoBorderBrush}" StrokeThickness="1" StrokeDashArray="3,3" RadiusX="8" RadiusY="8" /></c:AspectRatio>'
    tab = lambda title, count=None: f'<TabItem><TabItem.Header><StackPanel Orientation="Horizontal" Spacing="6"><TextBlock Text="{title}" VerticalAlignment="Center" />' + (f'<c:Badge Classes="secondary count" Content="{count}" />' if count else "") + '</StackPanel></TabItem.Header>'
    return lines(
        '<Panel Margin="24,0">',
        '  <TabControl SelectedIndex="{Binding Tab}">',
        f'    {tab("Outline")}',
        block_indent(outline, 6),
        '    </TabItem>',
        f'    {tab("Past Performance", 3)}{placeholder}</TabItem>',
        f'    {tab("Key Personnel", 2)}{placeholder}</TabItem>',
        f'    {tab("Focus Documents")}{placeholder}</TabItem>',
        '  </TabControl>',
        '  <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Right" VerticalAlignment="Top">',
        '    <Button Classes="outline sm has-icon">',
        f'      <StackPanel>{icon("IconLayoutColumns", brush="CosmoForegroundBrush")}<TextBlock Text="Customize Columns" />{icon("IconChevronDown", brush="CosmoForegroundBrush")}</StackPanel>',
        '      <Button.Flyout>',
        '        <MenuFlyout Placement="BottomEdgeAlignedRight" FlyoutPresenterClasses="w-56">',
        columns_menu,
        '        </MenuFlyout>',
        '      </Button.Flyout>',
        '    </Button>',
        f'    <Button Classes="outline sm has-icon"><StackPanel>{icon("IconPlus", brush="CosmoForegroundBrush")}<TextBlock Text="Add Section" /></StackPanel></Button>',
        '  </StackPanel>',
        '</Panel>')


def field(label, control):
    return lines('<StackPanel Spacing="12">', f'  <Label Content="{label}" />', f'  {control}', '</StackPanel>')


def select(name, value, options, placeholder):
    items = "".join(f"<x:String>{option}</x:String>" for option in options)
    return f'<ComboBox HorizontalAlignment="Stretch" PlaceholderText="{placeholder}" SelectedItem="{{Binding {value}}}" AutomationProperties.Name="{name}">{items}</ComboBox>'


def viewer():
    """The section viewer: a drawer from the right with the section's header, a six months chart, the trend, and its fields."""
    chart = lines(
        '<p:AreaChart c:ChartConfig.Config="{StaticResource VisitorsConfig}" Data="{Binding Months}" Height="200" Padding="0,0,10,0">',
        '  <p:CartesianGrid Vertical="False" />',
        '  <p:XAxis DataKey="Month" Hide="True" />',
        '  <p:Tooltip Cursor="False"><p:Tooltip.ContentTemplate><c:ChartTooltipContent Indicator="Dot" /></p:Tooltip.ContentTemplate></p:Tooltip>',
        '  <p:Area DataKey="Mobile" Type="Natural" StackId="a" Fill="{DynamicResource PictorPrimary}" FillOpacity="0.6" Stroke="{DynamicResource PictorPrimary}" IsAnimationActive="False" />',
        '  <p:Area DataKey="Desktop" Type="Natural" StackId="a" Fill="{DynamicResource PictorPrimary}" FillOpacity="0.4" Stroke="{DynamicResource PictorPrimary}" IsAnimationActive="False" />',
        '</p:AreaChart>')
    form = lines(
        '<StackPanel Spacing="16">',
        block_indent(field("Header", '<TextBox Text="{Binding Header, Mode=OneWay}" AutomationProperties.Name="Header" />'), 2),
        '  <Grid ColumnDefinitions="*,*" ColumnSpacing="16">',
        block_indent(field("Type", select("Type", "Type, Mode=OneWay", ["Table of Contents", "Executive Summary", "Technical Approach", "Design", "Capabilities", "Focus Documents", "Narrative", "Cover Page"], "Select a type")), 4),
        block_indent(field("Status", select("Status", "Status, Mode=OneWay", ["Done", "In Progress", "Not Started"], "Select a status")).replace("<StackPanel Spacing", '<StackPanel Grid.Column="1" Spacing', 1), 4),
        '  </Grid>',
        '  <Grid ColumnDefinitions="*,*" ColumnSpacing="16">',
        block_indent(field("Target", '<TextBox Text="{Binding Target}" AutomationProperties.Name="Target" />'), 4),
        block_indent(field("Limit", '<TextBox Text="{Binding Limit}" AutomationProperties.Name="Limit" />').replace("<StackPanel Spacing", '<StackPanel Grid.Column="1" Spacing', 1), 4),
        '  </Grid>',
        block_indent(field("Reviewer", select("Reviewer", "Reviewer, Mode=OneWay", ["Eddie Lake", "Jamik Tashpulatov", "Emily Whalen"], "Select a reviewer")), 2),
        '</StackPanel>')
    return lines(
        '<c:Drawer Side="Right" IsOpen="{Binding ViewerOpen}">',
        '  <DockPanel DataContext="{Binding Viewing}" x:DataType="m:DashboardSection">',
        '    <StackPanel DockPanel.Dock="Bottom" Spacing="8" Margin="16">',
        '      <Button Content="Submit" HorizontalAlignment="Stretch" HorizontalContentAlignment="Center" c:Dialog.Closes="True" />',
        '      <Button Classes="outline" Content="Done" HorizontalAlignment="Stretch" HorizontalContentAlignment="Center" c:Dialog.Closes="True" />',
        '    </StackPanel>',
        '    <c:DialogHeader DockPanel.Dock="Top" Padding="16" Title="{Binding Header}" Description="Showing total visitors for the last 6 months" />',
        '    <ScrollViewer Theme="{DynamicResource CosmoScrollArea}" HorizontalScrollBarVisibility="Disabled">',
        '      <StackPanel Spacing="16" Margin="16,0">',
        block_indent(chart, 8),
        '        <Separator />',
        '        <StackPanel Spacing="8">',
        f'          <StackPanel Orientation="Horizontal" Spacing="8"><TextBlock Text="Trending up by 5.2% this month" FontWeight="Medium" LineHeight="14" />{icon("IconTrendingUp", brush="CosmoForegroundBrush")}</StackPanel>',
        '          <TextBlock Text="Showing total visitors for the last 6 months. This is just some random text to test the layout. It spans multiple lines and should wrap around." TextWrapping="Wrap" Foreground="{DynamicResource CosmoMutedForegroundBrush}" />',
        '        </StackPanel>',
        '        <Separator />',
        block_indent(form, 8),
        '      </StackPanel>',
        '    </ScrollViewer>',
        '  </DockPanel>',
        '</c:Drawer>')


def dashboard():
    source = data("dashboard-01")
    return lines(
        '<Panel Width="1280" Height="800" x:DataType="m:DashboardBlock" c:OverlayHost.IsHost="True">',
        '  <Panel.DataContext><m:DashboardBlock /></Panel.DataContext>',
        '  <Panel.Resources>',
        block_indent(RESOURCES, 4),
        '  </Panel.Resources>',
        '  <Panel.Styles>',
        block_indent(STYLES, 4),
        '  </Panel.Styles>',
        '  <c:SidebarProvider MobileBreakpoint="0">',
        block_indent(sidebar(source), 4),
        '    <c:SidebarInset>',
        '      <DockPanel>',
        block_indent(HEADER, 8),
        '        <ScrollViewer Theme="{DynamicResource CosmoScrollArea}" HorizontalScrollBarVisibility="Disabled">',
        '          <StackPanel Spacing="24" Margin="0,24">',
        block_indent(cards(), 12),
        block_indent(CHART, 12),
        block_indent(table(), 12),
        '          </StackPanel>',
        '        </ScrollViewer>',
        '      </DockPanel>',
        '    </c:SidebarInset>',
        '  </c:SidebarProvider>',
        block_indent(viewer(), 2),
        '</Panel>')
