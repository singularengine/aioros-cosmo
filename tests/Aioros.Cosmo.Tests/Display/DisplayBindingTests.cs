using Aioros.Cosmo.Display;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Spinner = Aioros.Cosmo.Display.Spinner;

namespace Aioros.Cosmo.Tests.Display;

public sealed class DisplayBindingTests(DesktopSession desktop)
{
    private static T Named<T>(Control view, string name) where T : Control => view.FindControl<T>(name)!;

    private static List<string> Texts(Control control) => [.. control.GetVisualDescendants().OfType<TextBlock>().Where(text => text.IsEffectivelyVisible).Select(text => text.Text ?? string.Empty)];

    private Task<T> ViewAsync<T>(Func<Window, DisplayView, DisplayModel, T> act) => desktop.RunAsync(() =>
    {
        var model = new DisplayModel();
        var view = new DisplayView { DataContext = model };
        var window = Windows.Show(view, ThemeVariant.Light);
        var result = act(window, view, model);
        window.Close();
        return result;
    });

    [Fact]
    [Trait("AC", "SEPARATOR-03")]
    public async Task TheSeparatorFollowsTheModelAndTheBorderTokenAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var wide = Named<Separator>(view, "WideSeparator");
            var inRow = Named<Separator>(view, "BoundSeparator");
            var before = wide.Bounds.Size;
            model.Vertical = true;
            Windows.Settle();
            var vertical = inRow.Bounds.Size;
            model.HideSeparator = true;
            Windows.Settle();
            var hidden = (wide.IsVisible, inRow.IsVisible);
            model.HideSeparator = false;
            window.Resources["CosmoBorderBrush"] = Brushes.Red;
            Windows.Settle();
            return (before, vertical, hidden, Windows.Paint(inRow.Background));
        });

        Assert.Equal(new Avalonia.Size(592, 1), found.before);
        Assert.Equal(new Avalonia.Size(1, 20), found.vertical);
        Assert.Equal((false, false), found.hidden);
        Assert.Equal(Testing.CssColor.Parse("#FF0000"), found.Item4);
    }

    [Fact]
    [Trait("AC", "SKELETON-03")]
    public async Task SkeletonsStandInUntilTheModelHasLoadedAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var loading = (Named<StackPanel>(view, "Skeletons").IsVisible, Named<ItemsControl>(view, "LoadedFiles").IsVisible);
            model.Finish.Execute(null);
            Windows.Settle();
            return (loading, Named<StackPanel>(view, "Skeletons").IsVisible, Texts(Named<ItemsControl>(view, "LoadedFiles")));
        });

        Assert.Equal((true, false), found.loading);
        Assert.False(found.Item2);
        Assert.Equal(["Cosmo", "Pictor", "Aioros"], found.Item3);
    }

    [Fact]
    [Trait("AC", "SPINNER-03")]
    public async Task TheButtonSpinsWhileItsCommandRunsAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var button = Named<Button>(view, "SaveButton");
            var spinner = Named<Spinner>(view, "SaveSpinner");
            var text = Named<TextBlock>(view, "SaveText");
            var idle = (spinner.IsVisible, text.Text);
            Windows.Click(window, button);
            var saving = (button.IsEffectivelyEnabled, spinner.IsVisible, text.Text);
            model.EndSave();
            Windows.Settle();
            return (idle, saving, (button.IsEffectivelyEnabled, spinner.IsVisible, text.Text), model.Save.Running?.IsCompletedSuccessfully);
        });

        Assert.Equal((false, "Save"), found.idle);
        Assert.Equal((false, true, "Saving..."), found.saving);
        Assert.Equal((true, false, "Save"), found.Item3);
        Assert.True(found.Item4);
    }

    [Fact]
    [Trait("AC", "KBD-04")]
    public async Task TheKeysFollowTheModelThroughTheirTemplateAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var keys = Named<ItemsControl>(view, "Keys");
            model.Keys.Insert(1, "Shift");
            Windows.Settle();
            var shown = keys.GetVisualDescendants().OfType<Kbd>().ToList();
            return (keys.ItemsPanelRoot is KbdGroup { Spacing: 4 }, shown.Select(key => key.Content as string).ToList(), Windows.Box(shown[1], keys).X - Windows.Box(shown[0], keys).Right);
        });

        Assert.True(found.Item1);
        Assert.Equal(["Ctrl", "Shift", "K"], found.Item2);
        Assert.Equal(4, found.Item3);
    }

    [Fact]
    [Trait("AC", "AVATAR-03")]
    public async Task AvatarsFollowTheModelsMembersAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var picture = new Bitmap(AssetLoader.Open(new Uri("avares://Aioros.Cosmo.Tests/Assets/avatar-shadcn.png")));
            model.Members.Add(new MemberModel("Shad Cn", picture));
            model.Members.Add(new MemberModel("Max Leiter", picture));
            Windows.Settle();
            var avatars = () => Named<AvatarGroup>(view, "Members").GetVisualDescendants().OfType<Avatar>().ToList();
            var before = avatars().Select(avatar => avatar.Source is not null).ToList();
            model.ClearPicture.Execute(null);
            model.AddMember.Execute(null);
            Windows.Settle();
            var after = avatars();
            return (before, after.Select(avatar => (avatar.Source is null, avatar.Content as string, Windows.Part<Border>(avatar, "PART_Fallback").IsVisible)).ToList(), Windows.Box(after[2], view).X - Windows.Box(after[1], view).Right);
        });

        Assert.Equal([true, true], found.before);
        Assert.Equal([(true, "SC", true), (false, "ML", false), (true, "RP", true)], found.Item2);
        Assert.Equal(-8, found.Item3);
    }

    [Fact]
    [Trait("AC", "ALERT-03")]
    public async Task TheAlertFollowsTheSavesOutcomeAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var alert = Named<Alert>(view, "BoundAlert");
            model.Fail.Execute(null);
            Windows.Settle();
            var failed = (alert.Classes.Contains("destructive"), alert.Title as string, Named<TextBlock>(view, "AlertError").IsVisible, Windows.Paint(Windows.Part<ContentPresenter>(alert, "PART_Title").Foreground));
            model.Succeed.Execute(null);
            Windows.Settle();
            return (failed, (alert.Classes.Contains("destructive"), alert.Title as string, Texts(Windows.Part<ContentPresenter>(alert, "PART_Description")).Single()));
        });

        Assert.Equal((true, "Unable to save.", true), (found.failed.Item1, found.failed.Item2, found.failed.Item3));
        Assert.Equal(Testing.CssColor.Parse("#E7000B"), found.failed.Item4);
        Assert.Equal((false, "Saved", "Saved 1 time(s)."), found.Item2);
    }

    [Fact]
    [Trait("AC", "ASPECTRATIO-03")]
    public async Task TheRatioFollowsTheModelAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var ratio = Named<AspectRatio>(view, "BoundRatio");
            var wide = ratio.Bounds.Height;
            model.Ratio = 1;
            Windows.Settle();
            return (wide, ratio.Bounds.Height);
        });

        Assert.Equal((253.125, 450.0), found);
    }

    [Fact]
    [Trait("AC", "TYPOGRAPHY-03")]
    public async Task TypographyFollowsTheModelAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var heading = Named<TextBlock>(view, "LevelsHeading");
            var before = heading.Text;
            model.ShowLead = false;
            model.AddLevel.Execute(null);
            Windows.Settle();
            var items = Named<List>(view, "LevelList").GetVisualDescendants().OfType<ListItem>().ToList();
            return (before, heading.Text, Named<TextBlock>(view, "Lead").IsVisible, items.Count, items.All(item => item.GetVisualDescendants().OfType<Ellipse>().Single().IsVisible), Texts(items[2]).Single());
        });

        Assert.Equal(("2 levels", "3 levels", false, 3, true, "Level 3"), found);
    }

    [Fact]
    [Trait("AC", "EMPTY-03")]
    public async Task TheEmptyStateShowsWhileTheCollectionIsEmptyAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var empty = Named<Empty>(view, "NoProjects");
            var before = empty.IsVisible;
            Windows.Click(window, Named<Button>(view, "CreateProject"));
            var created = (empty.IsVisible, Texts(Named<ItemsControl>(view, "Projects")));
            model.Projects.Clear();
            Windows.Settle();
            return (before, created, empty.IsVisible);
        });

        Assert.True(found.before);
        Assert.False(found.created.Item1);
        Assert.Equal(["Cosmo"], found.created.Item2);
        Assert.True(found.Item3);
    }

    [Fact]
    [Trait("AC", "ITEM-03")]
    public async Task AnItemsActionRemovesItAsync()
    {
        var found = await ViewAsync((window, view, model) =>
        {
            var list = Named<ItemsControl>(view, "FileItems");
            var descriptions = list.GetVisualDescendants().OfType<Item>().Select(item => item.Description as string).ToList();
            var remove = list.GetVisualDescendants().OfType<Item>().ElementAt(1).GetVisualDescendants().OfType<Button>().Single();
            Windows.Click(window, remove);
            return (descriptions, model.Files.Select(file => file.Name).ToList(), list.GetVisualDescendants().OfType<Item>().Select(item => item.Title as string).ToList(), list.ItemsPanelRoot is ItemGroup);
        });

        Assert.Equal(["12 files", "7 files", "31 files"], found.descriptions);
        Assert.Equal(["Cosmo", "Aioros"], found.Item2);
        Assert.Equal(["Cosmo", "Aioros"], found.Item3);
        Assert.True(found.Item4);
    }
}
