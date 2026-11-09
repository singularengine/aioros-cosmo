using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>The resizable, carousel, command and combobox pages' model.</summary>
public sealed partial class CompositeExamples : ObservableObject
{
    public CompositeExamples()
    {
        Slides.CollectionChanged += (_, _) => OnPropertyChanged(nameof(SlideText));
        Framework = Frameworks[3];
    }

    [ObservableProperty]
    public partial double SidebarSize { get; set; } = 25;

    public string SizeText => string.Create(CultureInfo.InvariantCulture, $"Sidebar: {SidebarSize:0}%");

    public ObservableCollection<int> Slides { get; } = [1, 2, 3, 4, 5];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SlideText))]
    public partial int SlideIndex { get; set; }

    public string SlideText => string.Create(CultureInfo.InvariantCulture, $"Slide {SlideIndex + 1} of {Slides.Count}");

    partial void OnSidebarSizeChanged(double value) => OnPropertyChanged(nameof(SizeText));

    [ObservableProperty]
    public partial string? CommandSearch { get; set; }

    [ObservableProperty]
    public partial string? CommandValue { get; set; }

    [ObservableProperty]
    public partial string Opened { get; set; } = "Nothing yet";

    [ObservableProperty]
    public partial bool PaletteOpen { get; set; }

    public IReadOnlyList<CommandPage> CommandPages { get; } = [new("Accordion", "/components/accordion"), new("Calendar", "/components/calendar"), new("Sidebar", "/components/sidebar")];

    public IReadOnlyList<string> Frameworks { get; } = ["Next.js", "SvelteKit", "Nuxt.js", "Remix", "Astro"];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameworkText))]
    public partial string? Framework { get; set; }

    public string FrameworkText => Framework is { } chosen ? $"Selected: {chosen}" : "Nothing selected";

    public ObservableCollection<object> ChosenFrameworks { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SidebarText))]
    public partial bool SidebarOpen { get; set; } = true;

    public string SidebarText => SidebarOpen ? "The model says: expanded" : "The model says: collapsed";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlaygroundText))]
    public partial bool PlaygroundOpen { get; set; } = true;

    public string PlaygroundText => PlaygroundOpen ? "Playground: open" : "Playground: closed";

    [RelayCommand]
    private void ToggleSidebar() => SidebarOpen = !SidebarOpen;

    [RelayCommand]
    private void ResetSize() => SidebarSize = 25;

    [RelayCommand]
    private void Open(string? path)
    {
        Opened = path ?? Opened;
        PaletteOpen = false;
    }

    [RelayCommand]
    private void TogglePalette() => PaletteOpen = !PaletteOpen;

    [RelayCommand]
    private void AddSlide() => Slides.Add(Slides.Count + 1);
}

/// <summary>A page the command's model gives as an item.</summary>
public sealed record CommandPage(string Title, string Path);
