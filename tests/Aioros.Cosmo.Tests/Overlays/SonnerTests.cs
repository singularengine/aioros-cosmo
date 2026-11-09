using System.Collections.ObjectModel;
using Aioros.Cosmo.Overlays;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Presenters;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Path = Avalonia.Controls.Shapes.Path;

namespace Aioros.Cosmo.Tests.Overlays;

public sealed class SonnerTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static readonly string[] Titles = ["One", "Two", "Three", "Four"];

    private static List<NotificationCard> Cards(Window window) => [.. window.GetVisualDescendants().OfType<NotificationCard>()];

    /// <summary>Plays the exits of the toasts closing, on the window's clock: a toast leaves once its 400 ms exit has played (SONNER-07).</summary>
    private static void Leave()
    {
        Windows.Clock!.Pulse(TimeSpan.Zero);
        Windows.Clock.Pulse(TimeSpan.FromMilliseconds(400));
        Windows.Settle();
    }

    private static T Clocked<T>(Func<T> body)
    {
        Windows.Clocked = true;
        try
        {
            return body();
        }
        finally
        {
            Windows.Clocked = false;
        }
    }

    private static (Window Window, Toaster Toaster, ObservableCollection<Toast> Toasts) Show(ManualTime time)
    {
        var toasts = new ObservableCollection<Toast>();
        var toaster = new Toaster { Toasts = toasts, TimeProvider = time };
        var window = Windows.Fixture(new Panel { Children = { OverlayFixtures.Outline("Show Toast"), toaster } }, 400, ThemeVariant.Light);
        return (window, toaster, toasts);
    }

    [Fact]
    [Trait("AC", "SONNER-01")]
    public async Task TheStackIsSonnersToastsAsShadcnStylesThemAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("sonner-stack", name);
            var front = reference.First(element => element.Slot == "toast");
            var description = reference.First(element => element.Slot == "toast-description");
            var action = reference.First(element => element.Slot == "toast-action");
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.SonnerStack(), variant);
                var cards = Cards(window);
                var card = cards[^1];
                var errors = PopoverTests.Surface($"{name} toast", Windows.Part<Border>(card, "PART_Root"), card.Padding, window, front, 1.5, 1).ToList();
                var texts = card.GetVisualDescendants().OfType<TextBlock>().ToList();
                var look = (texts[0].FontSize, (int)texts[0].FontWeight, texts[0].LineHeight, texts[1].FontSize, texts[1].LineHeight);
                if (look != (13, 500, 19.5, 13, 18.2) || !Windows.Paint(texts[1].Foreground).Near(CssColor.Parse(description.Color)))
                {
                    errors.Add($"{name}: texts {look} {Windows.Paint(texts[1].Foreground)}, shadcn {description.Color}");
                }

                var undo = card.GetVisualDescendants().OfType<Button>().Single(button => button.Name != "ToastClose");
                var presenter = Windows.Part<ContentPresenter>(undo, "PART_ContentPresenter");
                if (undo.Bounds.Height != 24 || undo.CornerRadius.TopLeft != 4 || !Windows.Paint(presenter.Background).Near(CssColor.Parse(action.Background)) || !Windows.Paint(presenter.Foreground).Near(CssColor.Parse(action.Color)))
                {
                    errors.Add($"{name}: action {undo.Bounds.Height} {undo.CornerRadius.TopLeft} {Windows.Paint(presenter.Background)} {Windows.Paint(presenter.Foreground)}");
                }

                for (var behind = 1; behind <= 2; behind++)
                {
                    var matrix = cards[^(behind + 1)].RenderTransform?.Value ?? Matrix.Identity;
                    var hidden = Windows.Part<ContentPresenter>(cards[^(behind + 1)], "PART_ContentPresenter").Opacity;
                    if (Math.Abs(matrix.M11 - (1 - (0.05 * behind))) > 0.001 || Math.Abs(matrix.M32 + (14 * behind)) > 0.001 || hidden != 0)
                    {
                        errors.Add($"{name}: toast {behind} behind {matrix} content {hidden}");
                    }
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "SONNER-02")]
    public async Task ToastsExpirePauseExpandAndActAsync()
    {
        var found = await desktop.RunAsync(() => Clocked(() =>
        {
            var time = new ManualTime();
            var (window, _, toasts) = Show(time);
            var steps = new List<string>();
            toasts.Add(new Toast { Title = "Saved" });
            Windows.Settle();
            time.Advance(TimeSpan.FromSeconds(3.9));
            Windows.Settle();
            steps.Add($"3.9 s {Cards(window).Count}");
            time.Advance(TimeSpan.FromSeconds(0.2));
            Windows.Settle();
            Leave();
            steps.Add($"4.1 s {Cards(window).Count} {toasts.Count}");
            toasts.Add(new Toast { Title = "First" });
            toasts.Add(new Toast { Title = "Second" });
            Windows.Settle();
            Windows.Hover(window, Cards(window)[^1]);
            var boxes = Cards(window).Select(card => Windows.Box(card, window)).ToList();
            time.Advance(TimeSpan.FromSeconds(10));
            Windows.Settle();
            steps.Add($"hovered {Cards(window).Count} gap {boxes[1].Y - boxes[0].Bottom}");
            Windows.Away(window, new Point(2, 2));
            time.Advance(TimeSpan.FromSeconds(4));
            Windows.Settle();
            Leave();
            steps.Add($"left {Cards(window).Count}");
            foreach (var title in Titles)
            {
                toasts.Add(new Toast { Title = title });
            }

            Windows.Settle();
            steps.Add($"visible {Cards(window).Count(card => card.Opacity == 1)} of {Cards(window).Count}");
            toasts.Clear();
            Windows.Settle();
            Leave();
            var ran = new List<object?>();
            toasts.Add(new Toast { Title = "Deleted", ActionText = "Undo", ActionCommand = new Command(ran.Add), ActionParameter = "undo" });
            Windows.Settle();
            Windows.Click(window, Cards(window)[^1].GetVisualDescendants().OfType<Button>().Single(button => button.Name != "ToastClose"));
            Leave();
            steps.Add($"action {string.Join(",", ran)} {Cards(window).Count}");
            window.Close();
            return steps;
        }));

        Assert.Equal(["3.9 s 1", "4.1 s 0 0", "hovered 2 gap 14", "left 0", "visible 3 of 4", "action undo 0"], found);
    }

    [Fact]
    [Trait("AC", "SONNER-03")]
    public async Task KindsShowTheirIconsAndALoadingToastResolvesInPlaceAsync()
    {
        var found = await desktop.RunAsync(() => Clocked(() =>
        {
            var time = new ManualTime();
            var model = new OverlaysModel();
            var view = new OverlaysView { DataContext = model };
            view.FindControl<Toaster>("ViewToaster")!.TimeProvider = time;
            var window = Windows.Show(view, ThemeVariant.Light);
            var kinds = new[] { ToastKind.Default, ToastKind.Success, ToastKind.Info, ToastKind.Warning, ToastKind.Error };
            foreach (var kind in kinds)
            {
                model.Toasts.Add(new Toast { Title = kind.ToString(), Kind = kind });
            }

            Windows.Settle();
            var icons = Cards(window).Select(Icon).ToList();
            time.Advance(TimeSpan.FromSeconds(5));
            Windows.Settle();
            Leave();
            var loading = new Toast { Title = "Loading...", Kind = ToastKind.Loading };
            model.Toasts.Add(loading);
            Windows.Settle();
            var pending = Icon(Cards(window).Single());
            time.Advance(TimeSpan.FromSeconds(30));
            Windows.Settle();
            var stays = Cards(window).Count;
            loading.Resolve(ToastKind.Success, "Event has been created");
            Windows.Settle();
            var resolved = $"{Icon(Cards(window).Single())} {Cards(window).Single().GetVisualDescendants().OfType<TextBlock>().First().Text}";
            time.Advance(TimeSpan.FromSeconds(4));
            Windows.Settle();
            Leave();
            var after = (Cards(window).Count, model.Toasts.Count);
            window.Close();
            return (string.Join(",", icons), pending, stays, resolved, after);
        }));

        Assert.Equal(("none,success,info,warning,error", "loading", 1, "success Event has been created", (0, 0)), found);
    }

    private static string Icon(NotificationCard card)
    {
        var icon = card.GetVisualDescendants().OfType<Viewbox>().First(box => box.Name == "ToastIcon");
        if (!icon.IsVisible)
        {
            return "none";
        }

        var shown = icon.GetVisualDescendants().OfType<Path>().Single(path => path.IsVisible);
        return shown.Classes.Contains("spin") ? "loading" : (string)shown.Tag!;
    }
}
