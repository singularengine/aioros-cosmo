using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// The surface a <see cref="Dialog"/>, an <see cref="AlertDialog"/>, a <see cref="Sheet"/> or a <see cref="Drawer"/>
/// shows its content on (shadcn's <c>DialogContent</c>, <c>AlertDialogContent</c>, <c>SheetContent</c>,
/// <c>DrawerContent</c>), with the close button in its top right corner. Its classes name its kind (<c>dialog</c>,
/// <c>alert</c>, <c>sheet</c> with its side, <c>drawer</c>), for styles.
/// </summary>
[TemplatePart("PART_Close", typeof(Button))]
public sealed class DialogSurface : ContentControl
{
    /// <summary>Defines <see cref="ShowCloseButton"/>.</summary>
    public static readonly StyledProperty<bool> ShowCloseButtonProperty = AvaloniaProperty.Register<DialogSurface, bool>(nameof(ShowCloseButton), true);

    /// <summary>Defines the <see cref="CloseRequested"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> CloseRequestedEvent = RoutedEvent.Register<DialogSurface, RoutedEventArgs>(nameof(CloseRequested), RoutingStrategies.Bubble);

    private Button? _close;

    /// <summary>Whether the close button (shadcn's <c>showCloseButton</c>) is shown.</summary>
    public bool ShowCloseButton
    {
        get => GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    /// <summary>Raised when the close button or a button marked <c>c:Dialog.Closes</c> asks the surface to close.</summary>
    public event EventHandler<RoutedEventArgs>? CloseRequested
    {
        add => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_close is not null)
        {
            _close.Click -= OnClose;
        }

        _close = e.NameScope.Find<Button>("PART_Close");
        if (_close is not null)
        {
            _close.Click += OnClose;
        }
    }

    private void OnClose(object? sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(CloseRequestedEvent, this));
}

/// <summary>A dialog's header: its title and its description (shadcn's <c>DialogHeader</c>, <c>DialogTitle</c>, <c>DialogDescription</c>).</summary>
public sealed class DialogHeader : TemplatedControl
{
    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<DialogHeader, object?>(nameof(Title));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<object?> DescriptionProperty = AvaloniaProperty.Register<DialogHeader, object?>(nameof(Description));

    /// <summary>The dialog's title, in semibold.</summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The muted text under the title.</summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}

/// <summary>A dialog's footer: its buttons in a row at the end, 8 px apart (shadcn's <c>DialogFooter</c>); in a sheet or a drawer, a column.</summary>
public sealed class DialogFooter : StackPanel
{
}
