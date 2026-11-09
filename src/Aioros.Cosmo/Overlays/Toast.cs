using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Converters;

namespace Aioros.Cosmo.Overlays;

/// <summary>The kind of a <see cref="Toast"/>, which picks its icon (sonner's <c>toast</c>, <c>toast.success</c>…).</summary>
public enum ToastKind
{
    /// <summary>No icon.</summary>
    Default,

    /// <summary>A circle with a check.</summary>
    Success,

    /// <summary>A circle with an i.</summary>
    Info,

    /// <summary>A triangle with an exclamation mark.</summary>
    Warning,

    /// <summary>An octagon with an x.</summary>
    Error,

    /// <summary>A spinning loader; the toast stays until its kind changes (sonner's <c>toast.promise</c> while pending).</summary>
    Loading,
}

/// <summary>
/// A toast a <see cref="Toaster"/> shows (sonner's <c>toast()</c>): a title, an optional description, a kind and an
/// optional action button and close button. Its properties raise change notifications, so a pending toast can turn into its result in
/// place (<see cref="Resolve"/>, sonner's <c>toast.promise</c>).
/// </summary>
public sealed class Toast : INotifyPropertyChanged
{
    private string? _title;
    private string? _description;
    private ToastKind _kind;
    private bool _closable;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The toast's title, in medium weight.</summary>
    public string? Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    /// <summary>The text under the title.</summary>
    public string? Description
    {
        get => _description;
        set => Set(ref _description, value);
    }

    /// <summary>The toast's kind and icon.</summary>
    public ToastKind Kind
    {
        get => _kind;
        set => Set(ref _kind, value, nameof(Type), nameof(ShowsCloseButton));
    }

    /// <summary>Whether the toast shows sonner's close button at its corner, the <see cref="Toaster"/>'s choice when unset.</summary>
    public bool? CloseButton { get; init; }

    /// <summary>Whether the close button stands on the toast now: asked for, and the toast is not loading (as sonner's).</summary>
    public bool ShowsCloseButton => _closable && Kind != ToastKind.Loading;

    /// <summary>Whether the toaster showing the toast gives it a close button.</summary>
    internal bool Closable
    {
        get => _closable;
        set => Set(ref _closable, value, nameof(ShowsCloseButton));
    }

    /// <summary>The action button's text (sonner's <c>action.label</c>); no button without it.</summary>
    public string? ActionText { get; init; }

    /// <summary>The command the action button runs (sonner's <c>action.onClick</c>); the toast closes after it.</summary>
    public ICommand? ActionCommand { get; init; }

    /// <summary>The parameter of <see cref="ActionCommand"/>.</summary>
    public object? ActionParameter { get; init; }

    /// <summary>How long the toast stays, the <see cref="Toaster"/>'s duration (4 s, sonner's) when unset.</summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>The Avalonia notification type of the toast's kind (information for the default, info and loading kinds).</summary>
    public NotificationType Type => Kind switch
    {
        ToastKind.Success => NotificationType.Success,
        ToastKind.Warning => NotificationType.Warning,
        ToastKind.Error => NotificationType.Error,
        _ => NotificationType.Information,
    };

    /// <summary>Turns a pending toast into its result: its new kind and title (and description), and lets it expire.</summary>
    public void Resolve(ToastKind kind, string? title, string? description = null)
    {
        Title = title;
        Description = description ?? Description;
        Kind = kind;
    }

    /// <summary>The toast's title and description, as text.</summary>
    public override string ToString() => Description is null ? Title ?? string.Empty : $"{Title} {Description}";

    private void Set<T>(ref T field, T value, string? also = null, string? again = null, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        foreach (var changed in new[] { name, also, again }.OfType<string>())
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changed));
        }
    }
}

/// <summary>Converters the toast's template uses.</summary>
public static class ToastConverters
{
    /// <summary>Whether a <see cref="ToastKind"/> is the one named by the converter's parameter.</summary>
    public static readonly IValueConverter Is = new FuncValueConverter<ToastKind, string, bool>((kind, name) => string.Equals(kind.ToString(), name, StringComparison.Ordinal));

    /// <summary>Whether a <see cref="ToastKind"/> is not the one named by the converter's parameter.</summary>
    public static readonly IValueConverter IsNot = new FuncValueConverter<ToastKind, string, bool>((kind, name) => !string.Equals(kind.ToString(), name, StringComparison.Ordinal));
}
