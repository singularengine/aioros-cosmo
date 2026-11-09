using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Threading;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// shadcn/ui's Toaster (sonner): shows each <see cref="Toast"/> added to <see cref="Toasts"/> (a model's collection, an
/// <see cref="INotifyCollectionChanged"/>) at a corner of its window, with a close button when asked, in an Avalonia <see cref="WindowNotificationManager"/>
/// themed as sonner's stack, for <see cref="Duration"/> (4 s), paused while the pointer is over the stack; a loading toast
/// stays until it is resolved. Each card's content is an Avalonia <see cref="Notification"/> whose expiration is the toast's
/// duration (zero while loading), shown through the toast's template. A closed toast is removed from the collection. Declare it once, anywhere in the window.
/// </summary>
public sealed class Toaster : Control
{
    /// <summary>Defines <see cref="Toasts"/>.</summary>
    public static readonly StyledProperty<IEnumerable?> ToastsProperty = AvaloniaProperty.Register<Toaster, IEnumerable?>(nameof(Toasts));

    /// <summary>Defines <see cref="Position"/>.</summary>
    public static readonly StyledProperty<NotificationPosition> PositionProperty = AvaloniaProperty.Register<Toaster, NotificationPosition>(nameof(Position), NotificationPosition.BottomRight);

    /// <summary>Defines <see cref="Duration"/>.</summary>
    public static readonly StyledProperty<TimeSpan> DurationProperty = AvaloniaProperty.Register<Toaster, TimeSpan>(nameof(Duration), TimeSpan.FromSeconds(4));

    /// <summary>Defines <see cref="IsExpanded"/>.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty = ToastDeck.IsExpandedProperty.AddOwner<Toaster>();

    /// <summary>Defines <see cref="CloseButton"/>.</summary>
    public static readonly StyledProperty<bool> CloseButtonProperty = AvaloniaProperty.Register<Toaster, bool>(nameof(CloseButton));

    private readonly Dictionary<Toast, Lifetime> _shown = [];
    private readonly List<Toast> _waiting = [];
    private WindowNotificationManager? _manager;
    private ToastDeck? _stack;
    private INotifyCollectionChanged? _watched;
    private bool _ready;

    static Toaster()
    {
        ToastsProperty.Changed.AddClassHandler<Toaster>((toaster, _) => toaster.Watch());
        PositionProperty.Changed.AddClassHandler<Toaster>((toaster, change) =>
        {
            if (toaster._manager is { } manager)
            {
                manager.Position = change.GetNewValue<NotificationPosition>();
            }
        });
        IsExpandedProperty.Changed.AddClassHandler<Toaster>((toaster, change) =>
        {
            if (toaster._stack is { } stack)
            {
                stack.IsExpanded = change.GetNewValue<bool>();
            }
        });
    }

    /// <summary>Creates a toaster, which takes no room where it is declared.</summary>
    public Toaster() => IsVisible = false;

    /// <summary>The toasts to show: a model's collection of <see cref="Toast"/>s, read as they are added.</summary>
    public IEnumerable? Toasts
    {
        get => GetValue(ToastsProperty);
        set => SetValue(ToastsProperty, value);
    }

    /// <summary>The corner the toasts stand at (bottom right, sonner's default).</summary>
    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>How long a toast stays (4 s, sonner's), unless the toast sets its own.</summary>
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>Whether the toasts always stand apart (sonner's <c>expand</c>).</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Whether each toast shows sonner's close button at its corner (sonner's <c>closeButton</c>), unless the toast says otherwise.</summary>
    public bool CloseButton
    {
        get => GetValue(CloseButtonProperty);
        set => SetValue(CloseButtonProperty, value);
    }

    /// <summary>The clock the toasts expire by (the system's by default; a test's fake one in tests).</summary>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <summary>Closes a toast now, as its expiry would.</summary>
    public void Close(Toast toast)
    {
        if (_shown.TryGetValue(toast, out var lifetime))
        {
            _manager?.Close(lifetime.Notification);
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_manager is not null || TopLevel.GetTopLevel(this) is not { } top)
        {
            return;
        }

        _manager = new WindowNotificationManager(top) { Position = Position, MaxItems = int.MaxValue };
        _manager.DataTemplates.Add(new FuncDataTemplate<Notification>((notification, _) => View(notification), supportsRecycling: false));
        _manager.TemplateApplied += OnTemplateApplied;
        Watch();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        foreach (var lifetime in _shown.Values)
        {
            lifetime.Dispose();
        }

        _shown.Clear();
        _waiting.Clear();
        _ready = false;
        if (_manager is { } manager)
        {
            manager.TemplateApplied -= OnTemplateApplied;
            (manager.Parent as Panel)?.Children.Remove(manager);
        }

        _manager = null;
    }

    private void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        _stack = e.NameScope.Find<ToastDeck>("PART_Items");
        if (_stack is not null)
        {
            _stack.IsExpanded = IsExpanded;
            _stack.TimeProvider = TimeProvider;
            _stack.PointerEntered += (_, _) => Pause(true);
            _stack.PointerExited += (_, _) => Pause(false);
        }

        _ready = true;
        foreach (var toast in _waiting.ToList())
        {
            Show(toast);
        }

        _waiting.Clear();
    }

    private void Watch()
    {
        if (_watched is not null)
        {
            _watched.CollectionChanged -= OnCollectionChanged;
        }

        _watched = Toasts as INotifyCollectionChanged;
        if (_watched is not null && _manager is not null)
        {
            _watched.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var toast in e.NewItems?.OfType<Toast>() ?? [])
        {
            if (_ready)
            {
                Show(toast);
            }
            else
            {
                _waiting.Add(toast);
            }
        }

        var gone = e.Action == NotifyCollectionChangedAction.Reset
            ? _shown.Keys.Where(toast => Toasts is not IList list || !list.Contains(toast)).ToList()
            : e.OldItems?.OfType<Toast>().ToList() ?? [];
        foreach (var toast in gone)
        {
            Close(toast);
        }
    }

    private void Show(Toast toast)
    {
        if (_manager is null || _shown.ContainsKey(toast))
        {
            return;
        }

        toast.Closable = toast.CloseButton ?? CloseButton;
        var view = new ContentControl { Content = toast, Classes = { "toast" }, ClipToBounds = false };
        view[!ContentControl.ContentTemplateProperty] = view.GetResourceObservable("CosmoToastTemplate").ToBinding();
        view.AddHandler(Button.ClickEvent, (_, _) => Dispatcher.UIThread.Post(() => Close(toast)), Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);
        var expiration = toast.Kind == ToastKind.Loading ? TimeSpan.Zero : toast.Duration ?? Duration;
        var lifetime = new Lifetime(this, toast, view, new Notification(toast.Title, toast.Description, toast.Type, expiration));
        _shown[toast] = lifetime;
        _manager.Show(lifetime.Notification, toast.Type, TimeSpan.Zero, null, () => Closed(toast));
        lifetime.Start();
    }

    private ContentControl? View(Notification notification) =>
        _shown.Values.FirstOrDefault(lifetime => ReferenceEquals(lifetime.Notification, notification))?.View;

    private void Closed(Toast toast)
    {
        if (_shown.Remove(toast, out var lifetime))
        {
            lifetime.Dispose();
        }

        if (Toasts is IList list && list.Contains(toast))
        {
            list.Remove(toast);
        }
    }

    private void Pause(bool paused)
    {
        foreach (var lifetime in _shown.Values)
        {
            lifetime.Pause(paused);
        }
    }

    /// <summary>A shown toast's time left: running, paused under the pointer, or waiting while the toast is loading.</summary>
    private sealed class Lifetime : IDisposable
    {
        private readonly Toaster _toaster;
        private readonly Toast _toast;
        private ITimer? _timer;
        private TimeSpan _left;
        private long _started;
        private bool _paused;

        public ContentControl View { get; }

        public Notification Notification { get; }

        public Lifetime(Toaster toaster, Toast toast, ContentControl view, Notification notification)
        {
            Notification = notification;
            _toaster = toaster;
            _toast = toast;
            View = view;
            _left = toast.Duration ?? toaster.Duration;
            toast.PropertyChanged += OnToastChanged;
        }

        public void Start()
        {
            if (_toast.Kind == ToastKind.Loading || _paused || _timer is not null)
            {
                return;
            }

            _started = _toaster.TimeProvider.GetTimestamp();
            _timer = _toaster.TimeProvider.CreateTimer(_ => Dispatcher.UIThread.Post(Expire), null, _left, Timeout.InfiniteTimeSpan);
        }

        public void Pause(bool paused)
        {
            _paused = paused;
            if (paused && _timer is not null)
            {
                _left -= _toaster.TimeProvider.GetElapsedTime(_started);
                _timer.Dispose();
                _timer = null;
            }
            else if (!paused)
            {
                Start();
            }
        }

        public void Dispose()
        {
            _toast.PropertyChanged -= OnToastChanged;
            _timer?.Dispose();
            _timer = null;
        }

        private void Expire() => _toaster.Close(_toast);

        private void OnToastChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Toast.Kind))
            {
                Start();
            }
        }
    }
}
