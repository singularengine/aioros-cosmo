using System.Globalization;
using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Navigation;

/// <summary>Where a <see cref="SidebarProvider"/> keeps its open state between runs (shadcn's <c>sidebar_state</c> cookie).</summary>
public interface ISidebarStateStore
{
    /// <summary>The stored open state for <paramref name="key"/>, or null when none is stored.</summary>
    ValueTask<bool?> LoadAsync(string key, CancellationToken cancellationToken);

    /// <summary>Stores the open state for <paramref name="key"/>.</summary>
    ValueTask SaveAsync(string key, bool open, CancellationToken cancellationToken);
}

/// <summary>An <see cref="ISidebarStateStore"/> in a small text file (one <c>key=True</c> line per sidebar), for desktop apps.</summary>
public sealed class SidebarFileStore(string path) : ISidebarStateStore
{
    /// <summary>The file the states are kept in.</summary>
    public string Path { get; } = path;

    /// <inheritdoc />
    public async ValueTask<bool?> LoadAsync(string key, CancellationToken cancellationToken) =>
        (await ReadAsync(cancellationToken)).TryGetValue(key, out var open) ? open : null;

    /// <inheritdoc />
    public async ValueTask SaveAsync(string key, bool open, CancellationToken cancellationToken)
    {
        var states = await ReadAsync(cancellationToken);
        states[key] = open;
        if (System.IO.Path.GetDirectoryName(Path) is { Length: > 0 } folder)
        {
            Directory.CreateDirectory(folder);
        }

        await File.WriteAllLinesAsync(Path, states.Select(state => string.Create(CultureInfo.InvariantCulture, $"{state.Key}={state.Value}")), cancellationToken);
    }

    private async Task<Dictionary<string, bool>> ReadAsync(CancellationToken cancellationToken) => File.Exists(Path)
        ? (await File.ReadAllLinesAsync(Path, cancellationToken)).Select(line => line.Split('=', 2)).Where(parts => parts.Length == 2 && bool.TryParse(parts[1], out _))
            .GroupBy(parts => parts[0], StringComparer.Ordinal).ToDictionary(group => group.Key, group => bool.Parse(group.Last()[1]), StringComparer.Ordinal)
        : new Dictionary<string, bool>(StringComparer.Ordinal);
}

/// <summary>
/// shadcn/ui's SidebarProvider: lays its <see cref="Sidebar"/> beside its <see cref="SidebarInset"/> (the sidebar's width
/// animating over 200 ms as it collapses), docked as a <see cref="DockPanel"/> docks: each sidebar on its side, any other
/// child but the last by its <c>DockPanel.Dock</c> (a header across the top, as shadcn's sidebar-16), and the last filling
/// the rest; a second sidebar that does not collapse sits on its own side (shadcn's sidebar-15). Below <see cref="MobileBreakpoint"/> (the window's width, as shadcn's
/// <c>useIsMobile</c> reads <c>innerWidth</c>) shows the sidebar as a sheet over the inset. <see cref="IsOpen"/> (two way)
/// is the desktop state, <see cref="IsMobileOpen"/> (two way) the sheet's; <see cref="Toggle"/>, the
/// <see cref="SidebarTrigger"/>, the <see cref="SidebarRail"/> and <see cref="ToggleGesture"/> (Ctrl+B, or Meta+B) switch
/// the one that applies. With a <see cref="StateStore"/>, the open state is restored and kept under <see cref="StateKey"/>.
/// </summary>
public sealed class SidebarProvider : Panel
{
    /// <summary>Defines <see cref="IsOpen"/>.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<SidebarProvider, bool>(nameof(IsOpen), true, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines <see cref="IsMobileOpen"/>.</summary>
    public static readonly StyledProperty<bool> IsMobileOpenProperty = AvaloniaProperty.Register<SidebarProvider, bool>(nameof(IsMobileOpen), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines <see cref="MobileBreakpoint"/>.</summary>
    public static readonly StyledProperty<double> MobileBreakpointProperty = AvaloniaProperty.Register<SidebarProvider, double>(nameof(MobileBreakpoint), 768);

    /// <summary>Defines <see cref="IsMobile"/>.</summary>
    public static readonly DirectProperty<SidebarProvider, bool> IsMobileProperty = AvaloniaProperty.RegisterDirect<SidebarProvider, bool>(nameof(IsMobile), provider => provider.IsMobile);

    /// <summary>Defines <see cref="ToggleGesture"/>.</summary>
    public static readonly StyledProperty<KeyGesture?> ToggleGestureProperty =
        AvaloniaProperty.Register<SidebarProvider, KeyGesture?>(nameof(ToggleGesture), new KeyGesture(Key.B, KeyModifiers.Control));

    /// <summary>Defines <see cref="StateStore"/>.</summary>
    public static readonly StyledProperty<ISidebarStateStore?> StateStoreProperty = AvaloniaProperty.Register<SidebarProvider, ISidebarStateStore?>(nameof(StateStore));

    /// <summary>Defines <see cref="StateKey"/>.</summary>
    public static readonly StyledProperty<string> StateKeyProperty = AvaloniaProperty.Register<SidebarProvider, string>(nameof(StateKey), "sidebar_state");

    private TopLevel? _top;
    private bool _mobile;
    private bool _restoring;

    static SidebarProvider()
    {
        ClipToBoundsProperty.OverrideDefaultValue<SidebarProvider>(true);
        AffectsMeasure<SidebarProvider>(IsOpenProperty, IsMobileOpenProperty);
        IsOpenProperty.Changed.AddClassHandler<SidebarProvider>((provider, change) => provider.Opened(change.GetNewValue<bool>()));
        IsMobileOpenProperty.Changed.AddClassHandler<SidebarProvider>((provider, _) => provider.Mark());
        MobileBreakpointProperty.Changed.AddClassHandler<SidebarProvider>((provider, _) => provider.Measure());
        StateStoreProperty.Changed.AddClassHandler<SidebarProvider>((provider, _) => provider.Restore());
        StateKeyProperty.Changed.AddClassHandler<SidebarProvider>((provider, change) => Web.SetFlagCookie(provider, change.GetNewValue<string>()));
        MobileBreakpointProperty.Changed.AddClassHandler<SidebarProvider>((provider, change) => Web.SetBreakpoint(provider, change.GetNewValue<double>()));
    }

    /// <summary>
    /// Keeps the children's states in step with the provider's; on the web the open state is kept in the
    /// <see cref="StateKey"/> cookie and the sidebar becomes a sheet below <see cref="MobileBreakpoint"/>.
    /// </summary>
    public SidebarProvider()
    {
        Children.CollectionChanged += (_, _) => Mark();
        ToggleCommand = new Toggler(this);
        Web.SetFlagCookie(this, StateKey);
        Web.SetBreakpoint(this, MobileBreakpoint);
    }

    /// <summary>Whether the sidebar is expanded on wide windows (shadcn's <c>open</c> and <c>onOpenChange</c>), two way.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Whether the sidebar's sheet is open on narrow windows (shadcn's <c>openMobile</c>), two way.</summary>
    public bool IsMobileOpen
    {
        get => GetValue(IsMobileOpenProperty);
        set => SetValue(IsMobileOpenProperty, value);
    }

    /// <summary>The window width under which the sidebar becomes a sheet (shadcn's 768 px).</summary>
    public double MobileBreakpoint
    {
        get => GetValue(MobileBreakpointProperty);
        set => SetValue(MobileBreakpointProperty, value);
    }

    /// <summary>Whether the window is narrower than <see cref="MobileBreakpoint"/>.</summary>
    public bool IsMobile
    {
        get => _mobile;
        private set => SetAndRaise(IsMobileProperty, ref _mobile, value);
    }

    /// <summary>The keys that toggle the sidebar anywhere in its window (shadcn's ⌘B or Ctrl+B); Meta stands for Ctrl too.</summary>
    public KeyGesture? ToggleGesture
    {
        get => GetValue(ToggleGestureProperty);
        set => SetValue(ToggleGestureProperty, value);
    }

    /// <summary>Where the open state is kept between runs, if anywhere.</summary>
    public ISidebarStateStore? StateStore
    {
        get => GetValue(StateStoreProperty);
        set => SetValue(StateStoreProperty, value);
    }

    /// <summary>The key the open state is kept under (shadcn's cookie name, <c>sidebar_state</c>).</summary>
    public string StateKey
    {
        get => GetValue(StateKeyProperty);
        set => SetValue(StateKeyProperty, value);
    }

    /// <summary>Toggles the sidebar: the sheet on narrow windows, the expanded state otherwise.</summary>
    public System.Windows.Input.ICommand ToggleCommand { get; }

    /// <summary>The provider's sidebar, if any.</summary>
    public Sidebar? Sidebar => Children.OfType<Sidebar>().FirstOrDefault();

    /// <summary>Toggles the sidebar (shadcn's <c>toggleSidebar</c>).</summary>
    public void Toggle()
    {
        if (IsMobile)
        {
            SetCurrentValue(IsMobileOpenProperty, !IsMobileOpen);
        }
        else
        {
            SetCurrentValue(IsOpenProperty, !IsOpen);
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _top = TopLevel.GetTopLevel(this);
        if (_top is not null)
        {
            _top.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
            _top.PropertyChanged += OnTopChanged;
        }

        Restore();
        Measure();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_top is not null)
        {
            _top.RemoveHandler(KeyDownEvent, OnKeyDown);
            _top.PropertyChanged -= OnTopChanged;
            _top = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var (width, height, used, tallest) = (0d, 0d, new Size(), 0d);
        var filler = Filler;
        foreach (var child in Children)
        {
            var rest = new Size(Math.Max(0, availableSize.Width - used.Width), Math.Max(0, availableSize.Height - used.Height));
            if (child is Sidebar sidebar && Overlays(sidebar))
            {
                child.Measure(availableSize);
                tallest = Math.Max(tallest, child.DesiredSize.Height);
                continue;
            }

            child.Measure(child is Sidebar ? new Size(double.PositiveInfinity, rest.Height) : rest);
            if (child == filler)
            {
                (width, height) = (Math.Max(width, used.Width + child.DesiredSize.Width), Math.Max(height, used.Height + child.DesiredSize.Height));
            }
            else if (Dock(child) is global::Avalonia.Controls.Dock.Left or global::Avalonia.Controls.Dock.Right)
            {
                height = Math.Max(height, used.Height + child.DesiredSize.Height);
                used = used.WithWidth(used.Width + child.DesiredSize.Width);
            }
            else
            {
                width = Math.Max(width, used.Width + child.DesiredSize.Width);
                used = used.WithHeight(used.Height + child.DesiredSize.Height);
            }
        }

        return new Size(Math.Max(width, used.Width), Math.Max(tallest, Math.Max(height, used.Height)));
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var free = new Rect(finalSize);
        var filler = Filler;
        foreach (var child in Children)
        {
            if (child is Sidebar sidebar && Overlays(sidebar))
            {
                child.Arrange(new Rect(finalSize));
                continue;
            }

            if (child == filler)
            {
                child.Arrange(free);
                continue;
            }

            var size = child.DesiredSize;
            var (slot, left) = Dock(child) switch
            {
                global::Avalonia.Controls.Dock.Left => (free.WithWidth(Math.Min(size.Width, free.Width)), free.Width),
                global::Avalonia.Controls.Dock.Right => (new Rect(free.Right - Math.Min(size.Width, free.Width), free.Y, Math.Min(size.Width, free.Width), free.Height), free.Width),
                global::Avalonia.Controls.Dock.Top => (free.WithHeight(Math.Min(size.Height, free.Height)), free.Height),
                _ => (new Rect(free.X, free.Bottom - Math.Min(size.Height, free.Height), free.Width, Math.Min(size.Height, free.Height)), free.Height),
            };
            child.Arrange(slot);
            free = Dock(child) switch
            {
                global::Avalonia.Controls.Dock.Left => new Rect(slot.Right, free.Y, Math.Max(0, left - slot.Width), free.Height),
                global::Avalonia.Controls.Dock.Right => free.WithWidth(Math.Max(0, left - slot.Width)),
                global::Avalonia.Controls.Dock.Top => new Rect(free.X, slot.Bottom, free.Width, Math.Max(0, left - slot.Height)),
                _ => free.WithHeight(Math.Max(0, left - slot.Height)),
            };
        }

        return finalSize;
    }

    private Control? Filler => Children.LastOrDefault(static child => child is not Navigation.Sidebar);

    private static global::Avalonia.Controls.Dock Dock(Control child) => child is Sidebar sidebar
        ? sidebar.Side == SidebarSide.Right ? global::Avalonia.Controls.Dock.Right : global::Avalonia.Controls.Dock.Left
        : DockPanel.GetDock(child);

    private bool Overlays(Sidebar sidebar) => IsMobile && sidebar.Collapsible != SidebarCollapsible.None;

    internal void Mark()
    {
        var sidebar = Sidebar;
        foreach (var each in Children.OfType<Sidebar>())
        {
            each.Follow(IsOpen, IsMobile, IsMobileOpen);
        }

        PseudoClasses.Set(":inset", sidebar?.Variant == SidebarVariant.Inset && !IsMobile);
        foreach (var inset in Children.OfType<SidebarInset>())
        {
            inset.Follow(sidebar?.Variant == SidebarVariant.Inset && !IsMobile, !IsOpen);
        }
    }

    /// <summary>The store's work in flight (restoring, then each save in order), for whoever must wait for it.</summary>
    internal Task Persisting { get; private set; } = Task.CompletedTask;

    private void Opened(bool open)
    {
        Mark();
        if (StateStore is { } store && !_restoring)
        {
            Persisting = SaveAsync(Persisting, store, StateKey, open);
        }
    }

    private static async Task SaveAsync(Task previous, ISidebarStateStore store, string key, bool open)
    {
        await previous;
        await store.SaveAsync(key, open, CancellationToken.None);
    }

    private void Restore()
    {
        if (StateStore is { } store)
        {
            Persisting = RestoreAsync(Persisting, store, StateKey);
        }
    }

    private async Task RestoreAsync(Task previous, ISidebarStateStore store, string key)
    {
        await previous;
        if (await store.LoadAsync(key, CancellationToken.None) is { } open)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _restoring = true;
                SetCurrentValue(IsOpenProperty, open);
                _restoring = false;
            });
        }
    }

    private void Measure()
    {
        var mobile = _top is not null && _top.Bounds.Width > 0 && _top.Bounds.Width < MobileBreakpoint;
        if (mobile != IsMobile)
        {
            IsMobile = mobile;
            if (!mobile)
            {
                SetCurrentValue(IsMobileOpenProperty, false);
            }

            InvalidateMeasure();
        }

        Mark();
    }

    private void OnTopChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty)
        {
            Dispatcher.UIThread.Post(Measure);
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsMobile && IsMobileOpen && !e.Handled)
        {
            e.Handled = true;
            SetCurrentValue(IsMobileOpenProperty, false);
            return;
        }

        if (ToggleGesture is not { } gesture || e.Key != gesture.Key)
        {
            return;
        }

        var expected = gesture.KeyModifiers;
        var macStyle = (expected & KeyModifiers.Control) != 0 && e.KeyModifiers == ((expected & ~KeyModifiers.Control) | KeyModifiers.Meta);
        if (e.KeyModifiers == expected && this.GetVisualDescendants().OfType<SidebarTrigger>().Any(trigger => gesture.Equals(trigger.HotKey) && trigger.IsEffectivelyEnabled))
        {
            return;
        }

        if (e.KeyModifiers == expected || macStyle)
        {
            e.Handled = true;
            Toggle();
        }
    }

    private sealed class Toggler(SidebarProvider provider) : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => provider.Toggle();
    }
}
