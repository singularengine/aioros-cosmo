using Aioros.Cosmo.Overlays;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Overlays;

public sealed class DialogTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static readonly string[] Answers = ["Continue", "Cancel"];

    internal static DialogSurface Surface(Window window) => window.GetVisualDescendants().OfType<DialogSurface>().First();

    internal static Border Mask(Window window) => window.GetVisualDescendants().OfType<Border>().First(border => border.Classes.Contains("cosmo-overlay"));

    /// <summary>The difference between a control's box in the window and shadcn/ui's element, within the given tolerances.</summary>
    internal static IEnumerable<string> Box(string what, Control control, Control root, ElementMetrics expected, double horizontal = 0.5, double vertical = 2.5)
    {
        var box = Windows.Box(control, root);
        if (Math.Abs(box.X - expected.X) > horizontal || Math.Abs(box.Width - expected.Width) > horizontal || Math.Abs(box.Y - expected.Y) > vertical || Math.Abs(box.Height - expected.Height) > vertical)
        {
            yield return $"{what}: {box}, shadcn {expected.X:0.##},{expected.Y:0.##} {expected.Width:0.##}x{expected.Height:0.##}";
        }
    }

    /// <summary>The differences of an open header's title and description from shadcn/ui's.</summary>
    internal static IEnumerable<string> Header(string what, DialogSurface surface, Control root, ElementMetrics title, ElementMetrics description)
    {
        var header = surface.GetVisualDescendants().OfType<DialogHeader>().First();
        var titlePart = Windows.Part<ContentPresenter>(header, "PART_Title");
        var descriptionPart = Windows.Part<ContentPresenter>(header, "PART_Description");
        var look = (titlePart.FontSize, (int)titlePart.FontWeight, titlePart.GetValue(TextBlock.LineHeightProperty), descriptionPart.FontSize, descriptionPart.GetValue(TextBlock.LineHeightProperty));
        var shadcn = (PopoverTests.Px(title.FontSize), int.Parse(title.FontWeight, System.Globalization.CultureInfo.InvariantCulture), PopoverTests.Px(title.LineHeight!), PopoverTests.Px(description.FontSize), PopoverTests.Px(description.LineHeight!));
        if (look != shadcn || !Windows.Paint(descriptionPart.Foreground).Near(CssColor.Parse(description.Color)))
        {
            yield return $"{what}: header {look} {Windows.Paint(descriptionPart.Foreground)}, shadcn {shadcn} {description.Color}";
        }

        foreach (var difference in Box($"{what} title", titlePart, root, title, 1).Concat(Box($"{what} description", descriptionPart, root, description, 1)))
        {
            yield return difference;
        }
    }

    /// <summary>Clicks a point of the window, as a click on the mask outside the surface.</summary>
    internal static void Corner(Window window, Point point)
    {
        window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Windows.Settle();
    }

    [Fact]
    [Trait("AC", "DIALOG-01")]
    public async Task TheOpenDialogIsShadcnsDialogContentAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("dialog-open", name);
            var content = reference.Single(element => element.Slot == "dialog-content");
            var title = reference.Single(element => element.Slot == "dialog-title");
            var description = reference.Single(element => element.Slot == "dialog-description");
            var cancel = reference.Single(element => element.Text == "Cancel");
            var save = reference.Single(element => element.Text == "Save changes");
            var close = reference.Single(element => element.Slot == "dialog-close" && element.Text == "Close");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.DialogOpen(), variant);
                var surface = Surface(window);
                var mask = Mask(window);
                var errors = PopoverTests.Surface($"{name} dialog", Windows.Part<Border>(surface, "PART_Root"), surface.Padding, window, content).ToList();
                if (mask.Bounds.Size != new Size(640, 480) || !Windows.Paint(mask.Background).Near(new CssColor(0, 0, 0, 0.5)))
                {
                    errors.Add($"{name}: mask {mask.Bounds.Size} {Windows.Paint(mask.Background)}");
                }

                errors.AddRange(Header(name, surface, window, title, description));
                var buttons = surface.GetVisualDescendants().OfType<DialogFooter>().First().Children.OfType<Button>().ToList();
                errors.AddRange(Box($"{name} cancel", buttons[0], window, cancel, 1.5).Concat(Box($"{name} save", buttons[1], window, save, 1.5)));
                var closer = Windows.Part<Button>(surface, "PART_Close");
                errors.AddRange(Box($"{name} close", closer, window, close));
                var opacity = closer.Opacity;
                Windows.Hover(window, closer);
                if (opacity != 0.7 || closer.Opacity != 1)
                {
                    errors.Add($"{name}: close opacity {opacity} then {closer.Opacity}");
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "DIALOG-02")]
    public async Task TheDialogTrapsTheFocusAndClosesAsRadixsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var fixture = OverlayFixtures.DialogOpen();
            var window = OverlayFixtures.Show(fixture, ThemeVariant.Light);
            var dialog = ((Panel)fixture.Content).Children.OfType<Dialog>().Single();
            var trigger = ((Panel)fixture.Content).Children.OfType<Button>().Single();
            string Focused() => Windows.Focused(window) switch
            {
                TextBox box => box.Text ?? string.Empty,
                Button { Name: "PART_Close" } => "close",
                Button button => button.Content as string ?? string.Empty,
                var other => other?.GetType().Name ?? "nothing",
            };
            var steps = new List<string> { $"open {Focused()}" };
            for (var i = 0; i < 5; i++)
            {
                Windows.Key(window, Key.Tab);
            }

            steps.Add($"tab {Focused()}");
            window.KeyPress(Key.Tab, RawInputModifiers.Shift, PhysicalKey.Tab, null);
            window.KeyRelease(Key.Tab, RawInputModifiers.Shift, PhysicalKey.Tab, null);
            Windows.Settle();
            steps.Add($"shift+tab {Focused()}");
            Windows.Key(window, Key.Escape);
            steps.Add($"escape {dialog.IsOpen} {ReferenceEquals(Windows.Focused(window), trigger)}");
            foreach (var (what, close) in new (string, Action)[]
            {
                ("mask", () => Corner(window, new Point(10, 470))),
                ("close", () => Windows.Click(window, Windows.Part<Button>(Surface(window), "PART_Close"))),
                ("closes", () => Windows.Click(window, Surface(window).GetVisualDescendants().OfType<DialogFooter>().First().Children.OfType<Button>().First())),
            })
            {
                Windows.Click(window, trigger);
                close();
                steps.Add($"{what} {dialog.IsOpen}");
            }

            dialog.IsLightDismissEnabled = false;
            Windows.Click(window, trigger);
            Corner(window, new Point(10, 470));
            steps.Add($"kept {dialog.IsOpen}");
            dialog.IsOpen = false;
            window.Close();
            var hostless = new Dialog { Content = new TextBlock { Text = "Anywhere" } };
            var plain = Windows.Fixture(new StackPanel { Children = { hostless } }, 400, ThemeVariant.Light);
            hostless.IsOpen = true;
            Windows.Settle();
            var mask = Mask(plain);
            steps.Add($"overlay {mask.Bounds.Size} {Avalonia.Controls.Primitives.OverlayLayer.GetOverlayLayer(plain)?.IsVisualAncestorOf(mask)}");
            plain.Close();
            return steps;
        });

        Assert.Equal(["open Pedro Duarte", "tab Pedro Duarte", "shift+tab close", "escape False True", "mask False", "close False", "closes False", "kept True", "overlay 640, 400 True"], found);
    }

    [Fact]
    [Trait("AC", "DIALOG-03")]
    public async Task ACommandAwaitsTheDialogsAnswerAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new OverlaysModel();
            var view = new OverlaysView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            Windows.Click(window, view.FindControl<Button>("DialogTrigger")!);
            var surface = Surface(window);
            var texts = surface.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text).Where(text => text is "Edit Pedro Duarte" or "Signed in as @nextjs").ToList();
            var box = surface.GetVisualDescendants().OfType<TextBox>().First();
            box.Text = "Colm Tuite";
            Windows.Settle();
            var renamed = model.Name;
            Windows.Click(window, surface.GetVisualDescendants().OfType<Button>().First(button => button.Content as string == "Save"));
            var saved = (model.DialogOpen, model.Outcome, model.OpenDialog.Running?.IsCompleted);
            Windows.Click(window, view.FindControl<Button>("DialogTrigger")!);
            Surface(window).GetVisualDescendants().OfType<TextBox>().First().Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Escape);
            var escaped = (model.DialogOpen, model.Outcome);
            window.Close();
            return (string.Join(",", texts), renamed, saved, escaped);
        });

        Assert.Equal(("Edit Pedro Duarte,Signed in as @nextjs", "Colm Tuite", (false, (string?)"save", (bool?)true), (false, (string?)"closed")), found);
    }

    [Fact]
    [Trait("AC", "ALERTDIALOG-01")]
    public async Task TheOpenAlertDialogIsShadcnsAlertDialogContentAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("alert-dialog-open", name);
            var content = reference.Single(element => element.Slot == "alert-dialog-content");
            var title = reference.Single(element => element.Slot == "alert-dialog-title");
            var description = reference.Single(element => element.Slot == "alert-dialog-description");
            var cancel = reference.Single(element => element.Slot == "alert-dialog-cancel");
            var action = reference.Single(element => element.Slot == "alert-dialog-action");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var fixture = OverlayFixtures.AlertDialogOpen();
                var window = OverlayFixtures.Show(fixture, variant);
                var surface = Surface(window);
                var errors = PopoverTests.Surface($"{name} alert", Windows.Part<Border>(surface, "PART_Root"), surface.Padding, window, content).ToList();
                var header = surface.GetVisualDescendants().OfType<DialogHeader>().First();
                var titlePart = Windows.Part<ContentPresenter>(header, "PART_Title");
                errors.AddRange(Box($"{name} title", titlePart, window, title with { Width = titlePart.Bounds.Width }, 1));
                errors.AddRange(Box($"{name} description", Windows.Part<ContentPresenter>(header, "PART_Description"), window, description, 1));
                if (titlePart.GetValue(TextBlock.LineHeightProperty) != 28 || Windows.Part<Button>(surface, "PART_Close").IsVisible)
                {
                    errors.Add($"{name}: title line {titlePart.GetValue(TextBlock.LineHeightProperty)}, close shown {Windows.Part<Button>(surface, "PART_Close").IsVisible}");
                }

                var buttons = surface.GetVisualDescendants().OfType<DialogFooter>().First().Children.OfType<Button>().ToList();
                errors.AddRange(Box($"{name} cancel", buttons[0], window, cancel, 1.5).Concat(Box($"{name} continue", buttons[1], window, action, 1.5)));
                var dialog = ((Panel)fixture.Content).Children.OfType<AlertDialog>().Single();
                dialog.IsOpen = false;
                dialog.Classes.Add("sm");
                dialog.IsOpen = true;
                Windows.Settle();
                if (Surface(window).Bounds.Width != 320)
                {
                    errors.Add($"{name}: sm alert {Surface(window).Bounds.Width} px, shadcn 320");
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "ALERTDIALOG-02")]
    public async Task TheAlertDialogFocusesCancelAndIgnoresTheMaskAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var fixture = OverlayFixtures.AlertDialogOpen();
            var window = OverlayFixtures.Show(fixture, ThemeVariant.Light);
            var dialog = ((Panel)fixture.Content).Children.OfType<AlertDialog>().Single();
            var trigger = ((Panel)fixture.Content).Children.OfType<Button>().Single();
            var steps = new List<string> { $"focus {(Windows.Focused(window) as Button)?.Content}" };
            Corner(window, new Point(10, 350));
            steps.Add($"mask {dialog.IsOpen}");
            Windows.Key(window, Key.Escape);
            steps.Add($"escape {dialog.IsOpen} {ReferenceEquals(Windows.Focused(window), trigger)}");
            Windows.Click(window, trigger);
            Windows.Click(window, Surface(window).GetVisualDescendants().OfType<DialogFooter>().First().Children.OfType<Button>().Last());
            steps.Add($"continue {dialog.IsOpen}");
            window.Close();
            return steps;
        });

        Assert.Equal(["focus Cancel", "mask True", "escape False True", "continue False"], found);
    }

    [Fact]
    [Trait("AC", "ALERTDIALOG-03")]
    public async Task ACommandAwaitsTheAlertDialogsAnswerAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new OverlaysModel();
            var view = new OverlaysView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var answers = new List<string?>();
            foreach (var answer in Answers)
            {
                Windows.Click(window, view.FindControl<Button>("ConfirmTrigger")!);
                Windows.Click(window, Surface(window).GetVisualDescendants().OfType<Button>().First(button => button.Content as string == answer));
                answers.Add($"{model.Outcome} {model.ConfirmOpen}");
            }

            window.Close();
            return answers;
        });

        Assert.Equal(["continue False", "cancel False"], found);
    }
}
