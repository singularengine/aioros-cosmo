using Cards = Aioros.Cosmo.Overlays.HoverCard;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Overlays;

/// <summary>One of the reference app's overlay fixtures built on the desktop: its content, its height and how a user opens it.</summary>
internal sealed record OverlayFixture(string Name, Control Content, double Height, Action<Window> Open, IReadOnlyList<IStyle> Styles, bool Host = false);

/// <summary>The overlay fixtures of <c>tools/shadcn/app/src/overlays.tsx</c>, built as the Gallery's <c>/fidelity</c> pages build them.</summary>
internal static class OverlayFixtures
{
    public static Style Width(string type, string name, double width) => type switch
    {
        "FlyoutPresenter" => new Style(x => x.OfType<FlyoutPresenter>().Class(name)) { Setters = { new Setter(Layoutable.WidthProperty, width) } },
        _ => new Style(x => x.OfType<MenuFlyoutPresenter>().Class(name)) { Setters = { new Setter(Layoutable.WidthProperty, width) } },
    };

    public static Button Outline(string text) => new() { Content = text, Classes = { "outline" }, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };

    public static TextBlock Text(string text, double size = 14, double line = 20, FontWeight weight = FontWeight.Normal, string? brush = null)
    {
        var block = new TextBlock { Text = text, FontSize = size, LineHeight = line, FontWeight = weight, TextWrapping = TextWrapping.Wrap };
        if (brush is not null)
        {
            block[!TextBlock.ForegroundProperty] = block.GetResourceObservable(brush).ToBinding();
        }

        return block;
    }

    public static OverlayFixture PopoverOpen()
    {
        var fields = new StackPanel { Spacing = 8 };
        foreach (var (label, value) in new[] { ("Width", "100%"), ("Max. width", "300px"), ("Height", "25px"), ("Max. height", "none") })
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*"), ColumnSpacing = 16 };
            var box = new TextBox { Text = value, Height = 32, MinHeight = 32, Padding = new Thickness(12, 4) };
            Grid.SetColumn(box, 1);
            Grid.SetColumnSpan(box, 2);
            row.Children.Add(new Label { Content = label, VerticalAlignment = VerticalAlignment.Center });
            row.Children.Add(box);
            fields.Children.Add(row);
        }

        var content = new StackPanel
        {
            Spacing = 16,
            Children =
            {
                new StackPanel { Spacing = 8, Children = { Text("Dimensions", 16, 16, FontWeight.Medium), Text("Set the dimensions for the layer.", brush: "CosmoMutedForegroundBrush") } },
                fields,
            },
        };
        var flyout = new Flyout { Content = content, Placement = PlacementMode.BottomEdgeAlignedLeft };
        flyout.FlyoutPresenterClasses.Add("w-80");
        var trigger = Outline("Open popover");
        trigger.Flyout = flyout;
        return new("popover-open", trigger, 348, window => Windows.Click(window, trigger), [Width("FlyoutPresenter", "w-80", 320)]);
    }

    public static OverlayFixture TooltipOpen()
    {
        var trigger = Outline("Hover");
        trigger.VerticalAlignment = VerticalAlignment.Bottom;
        ToolTip.SetTip(trigger, "Add to library");
        return new("tooltip-open", new Panel { Height = 96, Children = { trigger } }, 144, window => Windows.Hover(window, trigger), []);
    }

    public static OverlayFixture KbdTooltipOpen()
    {
        var trigger = Outline("Save");
        trigger.Classes.Add("sm");
        trigger.VerticalAlignment = VerticalAlignment.Bottom;
        ToolTip.SetTip(trigger, new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 3,
            Children = { new TextBlock { Text = "Save Changes", VerticalAlignment = VerticalAlignment.Center }, new Aioros.Cosmo.Display.Kbd { Content = "S" } },
        });
        return new("kbd-tooltip-open", new Panel { Height = 96, Children = { trigger } }, 144, window => Windows.Hover(window, trigger), []);
    }

    public static OverlayFixture HoverCardOpen()
    {
        var avatar = new Border { Width = 32, Height = 32, CornerRadius = new CornerRadius(16), VerticalAlignment = VerticalAlignment.Top, Child = Text("VC", brush: "CosmoMutedForegroundBrush") };
        ((TextBlock)avatar.Child).HorizontalAlignment = HorizontalAlignment.Center;
        ((TextBlock)avatar.Child).VerticalAlignment = VerticalAlignment.Center;
        avatar[!Border.BackgroundProperty] = avatar.GetResourceObservable("CosmoMutedBrush").ToBinding();
        var details = new StackPanel
        {
            Spacing = 4,
            Children = { Text("@nextjs", weight: FontWeight.SemiBold), Text("The React Framework – created and maintained by @vercel."), Text("Joined December 2021", 12, 16, brush: "CosmoMutedForegroundBrush") },
        };
        var card = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), ColumnSpacing = 16, Children = { avatar, details } };
        Grid.SetColumn(details, 1);
        var trigger = new HyperlinkButton { Content = "@nextjs", Classes = { "link" }, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        Cards.SetOpenDelay(trigger, 0);
        Cards.SetPlacement(trigger, PlacementMode.BottomEdgeAlignedLeft);
        Cards.SetCard(trigger, card);
        return new("hover-card-open", trigger, 228, window => Windows.Hover(window, trigger), [new Style(x => x.OfType<FlyoutPresenter>().Class("hover-card")) { Setters = { new Setter(Layoutable.WidthProperty, 320.0) } }]);
    }

    public static MenuItem Item(string header, string? gesture = null, bool enabled = true, params string[] classes)
    {
        var item = new MenuItem { Header = header, IsEnabled = enabled, InputGesture = gesture is null ? null : Avalonia.Input.KeyGesture.Parse(gesture) };
        item.Classes.AddRange(classes);
        return item;
    }

    public static OverlayFixture DropdownMenuOpen()
    {
        var invite = Item("Invite users");
        invite.Items.Add(Item("Email"));
        invite.Items.Add(Item("Message"));
        invite.Items.Add(new Separator());
        invite.Items.Add(Item("More..."));
        var menu = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedLeft };
        menu.FlyoutPresenterClasses.Add("w-56");
        foreach (var item in new Control[]
        {
            Item("My Account", classes: "label"), Item("Profile", "Shift+Ctrl+P"), Item("Billing", "Ctrl+B"), Item("Settings", "Ctrl+S"), Item("Keyboard shortcuts", "Ctrl+K"), new Separator(),
            Item("Team"), invite, Item("New Team", "Ctrl+T"), new Separator(), Item("GitHub"), Item("Support"), Item("API", enabled: false), new Separator(), Item("Log out", "Shift+Ctrl+Q"),
        })
        {
            menu.Items.Add(item);
        }

        var trigger = Outline("Open");
        trigger.Flyout = menu;
        return new("dropdown-menu-open", trigger, 488, window => Windows.Click(window, trigger), [Width("MenuFlyoutPresenter", "w-56", 224)]);
    }

    public static OverlayFixture DropdownMenuCheckboxes()
    {
        var menu = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedLeft };
        menu.FlyoutPresenterClasses.Add("w-56");
        menu.Items.Add(Item("Appearance", classes: "label"));
        menu.Items.Add(new Separator());
        menu.Items.Add(new MenuItem { Header = "Status Bar", ToggleType = MenuItemToggleType.CheckBox, IsChecked = true });
        menu.Items.Add(new MenuItem { Header = "Activity Bar", ToggleType = MenuItemToggleType.CheckBox, IsEnabled = false });
        menu.Items.Add(new MenuItem { Header = "Panel", ToggleType = MenuItemToggleType.CheckBox });
        var trigger = Outline("Open");
        trigger.Flyout = menu;
        return new("dropdown-menu-checkboxes", trigger, 248, window => Windows.Click(window, trigger), [Width("MenuFlyoutPresenter", "w-56", 224)]);
    }

    public static OverlayFixture ContextMenuOpen()
    {
        var tools = Item("More Tools", classes: "inset");
        foreach (var item in new Control[] { Item("Save Page..."), Item("Create Shortcut..."), Item("Name Window..."), new Separator(), Item("Developer Tools"), new Separator(), Item("Delete", classes: "destructive") })
        {
            tools.Items.Add(item);
        }

        var context = new ContextMenu { Classes = { "w-52" } };
        foreach (var item in new Control[]
        {
            Item("Back", "Ctrl+OemOpenBrackets", true, "inset"), Item("Forward", "Ctrl+OemCloseBrackets", false, "inset"), Item("Reload", "Ctrl+R", true, "inset"), tools, new Separator(),
            new MenuItem { Header = "Show Bookmarks", ToggleType = MenuItemToggleType.CheckBox, IsChecked = true }, new MenuItem { Header = "Show Full URLs", ToggleType = MenuItemToggleType.CheckBox }, new Separator(),
            Item("People", null, true, "label", "inset"), new MenuItem { Header = "Pedro Duarte", ToggleType = MenuItemToggleType.Radio, GroupName = "people", IsChecked = true }, new MenuItem { Header = "Colm Tuite", ToggleType = MenuItemToggleType.Radio, GroupName = "people" },
        })
        {
            context.Items.Add(item);
        }

        var area = new Border { Width = 300, Height = 150, CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, ContextMenu = context, Child = Text("Right click here") };
        ((TextBlock)area.Child).HorizontalAlignment = HorizontalAlignment.Center;
        ((TextBlock)area.Child).VerticalAlignment = VerticalAlignment.Center;
        area[!Border.BorderBrushProperty] = area.GetResourceObservable("CosmoBorderBrush").ToBinding();
        return new("context-menu-open", area, 448, window => Windows.RightClick(window, area), [new Style(x => x.OfType<ContextMenu>().Class("w-52")) { Setters = { new Setter(Layoutable.WidthProperty, 208.0) } }]);
    }

    public static StackPanel Field(string label, string value) => new() { Spacing = 12, Children = { new Label { Content = label }, new TextBox { Text = value } } };

    public static Button Primary(string text, bool closes = false)
    {
        var button = new Button { Content = text };
        Aioros.Cosmo.Overlays.Dialog.SetCloses(button, closes);
        return button;
    }

    public static Button Secondary(string text, bool closes = false)
    {
        var button = Primary(text, closes);
        button.Classes.Add("outline");
        return button;
    }

    private static OverlayFixture Modal(string name, Control dialog, double height, string trigger, Action open)
    {
        var button = Outline(trigger);
        button.Click += (_, _) => open();
        return new(name, new Panel { Children = { button, dialog } }, height, window => Windows.Click(window, button), [], true);
    }

    public static OverlayFixture DialogOpen()
    {
        var dialog = new Aioros.Cosmo.Overlays.Dialog
        {
            MaxWidth = 425,
            Content = new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    new Aioros.Cosmo.Overlays.DialogHeader { Title = "Edit profile", Description = "Make changes to your profile here. Click save when you're done." },
                    new StackPanel { Spacing = 16, Children = { Field("Name", "Pedro Duarte"), Field("Username", "@peduarte") } },
                    new Aioros.Cosmo.Overlays.DialogFooter { Children = { Secondary("Cancel", true), Primary("Save changes") } },
                },
            },
        };
        return Modal("dialog-open", dialog, 480, "Open Dialog", () => dialog.IsOpen = true);
    }

    public static OverlayFixture AlertDialogOpen()
    {
        var cancel = Secondary("Cancel", true);
        cancel.IsCancel = true;
        var dialog = new Aioros.Cosmo.Overlays.AlertDialog
        {
            Content = new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    new Aioros.Cosmo.Overlays.DialogHeader { Title = "Are you absolutely sure?", Description = "This action cannot be undone. This will permanently delete your account and remove your data from our servers." },
                    new Aioros.Cosmo.Overlays.DialogFooter { Children = { cancel, Primary("Continue", true) } },
                },
            },
        };
        return Modal("alert-dialog-open", dialog, 360, "Show Dialog", () => dialog.IsOpen = true);
    }

    public static OverlayFixture SheetOpen()
    {
        var body = new StackPanel { Spacing = 24, Margin = new Thickness(16, 0), Children = { Field("Name", "Pedro Duarte"), Field("Username", "@peduarte") } };
        var footer = new Aioros.Cosmo.Overlays.DialogFooter { Children = { Primary("Save changes"), Secondary("Close", true) } };
        Grid.SetRow(body, 1);
        Grid.SetRow(footer, 2);
        var sheet = new Aioros.Cosmo.Overlays.Sheet
        {
            Content = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*,Auto"),
                RowSpacing = 16,
                Children = { new Aioros.Cosmo.Overlays.DialogHeader { Title = "Edit profile", Description = "Make changes to your profile here. Click save when you're done." }, body, footer },
            },
        };
        return Modal("sheet-open", sheet, 600, "Open", () => sheet.IsOpen = true);
    }

    public static OverlayFixture SheetTop()
    {
        var sheet = new Aioros.Cosmo.Overlays.Sheet
        {
            Side = Aioros.Cosmo.Overlays.SheetSide.Top,
            Content = new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    new Aioros.Cosmo.Overlays.DialogHeader { Title = "Edit profile", Description = "Make changes to your profile here. Click save when you're done." },
                    new Aioros.Cosmo.Overlays.DialogFooter { Children = { Primary("Save changes", true) } },
                },
            },
        };
        return Modal("sheet-top", sheet, 360, "top", () => sheet.IsOpen = true);
    }

    public static OverlayFixture DrawerOpen()
    {
        static Button Round(string glyph, string name) => new() { Classes = { "outline", "icon" }, Width = 32, Height = 32, CornerRadius = new CornerRadius(16), Content = Text(glyph, 16, 16), VerticalAlignment = VerticalAlignment.Center, [AutomationProperties.NameProperty] = name };
        var minus = Round("−", "Decrease");
        var plus = Round("+", "Increase");
        var goal = new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "350", FontSize = 72, LineHeight = 72, FontWeight = FontWeight.Bold, LetterSpacing = -3.6, HorizontalAlignment = HorizontalAlignment.Center },
                Text("CALORIES/DAY", 11.2, 16, brush: "CosmoMutedForegroundBrush"),
            },
        };
        ((TextBlock)goal.Children[1]).HorizontalAlignment = HorizontalAlignment.Center;
        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), ColumnSpacing = 8, Margin = new Thickness(16, 16, 16, 0), Children = { minus, goal, plus } };
        Grid.SetColumn(goal, 1);
        Grid.SetColumn(plus, 2);
        var drawer = new Aioros.Cosmo.Overlays.Drawer
        {
            Content = new StackPanel
            {
                MaxWidth = 384,
                Children =
                {
                    new Aioros.Cosmo.Overlays.DialogHeader { Title = "Move Goal", Description = "Set your daily activity goal." },
                    row,
                    new Aioros.Cosmo.Overlays.DialogFooter { Children = { Primary("Submit"), Secondary("Cancel", true) } },
                },
            },
        };
        return Modal("drawer-open", drawer, 600, "Open Drawer", () => drawer.IsOpen = true);
    }

    public static OverlayFixture SonnerStack()
    {
        var toasts = new System.Collections.ObjectModel.ObservableCollection<Aioros.Cosmo.Overlays.Toast>();
        var toaster = new Aioros.Cosmo.Overlays.Toaster { Toasts = toasts };
        var trigger = Outline("Show Toast");
        return new("sonner-stack", new Panel { Children = { trigger, toaster } }, 360, _ =>
        {
            toasts.Add(new Aioros.Cosmo.Overlays.Toast { Title = "Event has been created", Description = "Sunday, December 03, 2023 at 9:00 AM" });
            toasts.Add(new Aioros.Cosmo.Overlays.Toast { Title = "Event has been created", Kind = Aioros.Cosmo.Overlays.ToastKind.Success });
            toasts.Add(new Aioros.Cosmo.Overlays.Toast { Title = "Event has been created", Description = "Sunday, December 03, 2023 at 9:00 AM", ActionText = "Undo" });
            Windows.Settle();
        }, []);
    }

    public static OverlayFixture SonnerTypes()
    {
        var toasts = new System.Collections.ObjectModel.ObservableCollection<Aioros.Cosmo.Overlays.Toast>();
        var toaster = new Aioros.Cosmo.Overlays.Toaster { Toasts = toasts, IsExpanded = true };
        var trigger = Outline("Info");
        return new("sonner-types", new Panel { Children = { trigger, toaster } }, 260, _ =>
        {
            toasts.Add(new Aioros.Cosmo.Overlays.Toast { Title = "Be at the area 10 minutes before the event time", Kind = Aioros.Cosmo.Overlays.ToastKind.Info });
            Windows.Settle();
        }, []);
    }

    public static OverlayFixture SonnerClose()
    {
        var toasts = new System.Collections.ObjectModel.ObservableCollection<Aioros.Cosmo.Overlays.Toast>();
        var toaster = new Aioros.Cosmo.Overlays.Toaster { Toasts = toasts, CloseButton = true };
        var trigger = Outline("Show Toast");
        return new("sonner-close", new Panel { Children = { trigger, toaster } }, 260, _ =>
        {
            toasts.Add(new Aioros.Cosmo.Overlays.Toast { Title = "Event has been created", Description = "Sunday, December 03, 2023 at 9:00 AM", Duration = TimeSpan.FromDays(1) });
            Windows.Settle();
        }, []);
    }

    /// <summary>Shows a fixture in a theme, its styles added to the window, and opens its overlay.</summary>
    public static Window Show(OverlayFixture fixture, ThemeVariant variant)
    {
        var window = Windows.Fixture(fixture.Content, fixture.Height, variant, fixture.Host);
        window.Styles.AddRange(fixture.Styles);
        Windows.Settle();
        fixture.Open(window);
        return window;
    }
}
