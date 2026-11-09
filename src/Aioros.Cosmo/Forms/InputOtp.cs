using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Aioros.Cosmo.Forms;

/// <summary>One box of an <see cref="InputOtp"/>: its character, whether the caret is in it, and its place in its group.</summary>
/// <param name="Character">The character typed in the box, or an empty string.</param>
/// <param name="IsActive">Whether the caret is in the box (the next character goes there) while the control has the focus.</param>
/// <param name="IsFirst">Whether the box opens its group (rounded on the left, with a left border).</param>
/// <param name="IsLast">Whether the box closes its group (rounded on the right).</param>
/// <param name="HasSeparator">Whether a separator comes before the box (it opens a group after another).</param>
public sealed record OtpSlot(string Character, bool IsActive, bool IsFirst, bool IsLast, bool HasSeparator);

/// <summary>
/// shadcn/ui's InputOTP: <see cref="Length"/> boxes, grouped by <see cref="GroupSize"/> with a separator between groups,
/// showing the characters of <see cref="Text"/> (two-way bindable) typed into a hidden text box that covers them.
/// </summary>
[TemplatePart("PART_TextBox", typeof(TextBox))]
public sealed class InputOtp : TemplatedControl
{
    /// <summary>Defines <see cref="Text"/>.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<InputOtp, string?>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines <see cref="Length"/>.</summary>
    public static readonly StyledProperty<int> LengthProperty = AvaloniaProperty.Register<InputOtp, int>(nameof(Length), 6);

    /// <summary>Defines <see cref="GroupSize"/>.</summary>
    public static readonly StyledProperty<int> GroupSizeProperty = AvaloniaProperty.Register<InputOtp, int>(nameof(GroupSize));

    /// <summary>Defines <see cref="Slots"/>.</summary>
    public static readonly DirectProperty<InputOtp, IReadOnlyList<OtpSlot>> SlotsProperty =
        AvaloniaProperty.RegisterDirect<InputOtp, IReadOnlyList<OtpSlot>>(nameof(Slots), otp => otp.Slots);

    private IReadOnlyList<OtpSlot> _slots = [];

    static InputOtp()
    {
        FocusableProperty.OverrideDefaultValue<InputOtp>(false);
        TextProperty.Changed.AddClassHandler<InputOtp>((otp, _) => otp.Refresh());
        LengthProperty.Changed.AddClassHandler<InputOtp>((otp, _) => otp.Refresh());
        GroupSizeProperty.Changed.AddClassHandler<InputOtp>((otp, _) => otp.Refresh());
        IsKeyboardFocusWithinProperty.Changed.AddClassHandler<InputOtp>((otp, _) => otp.Refresh());
    }

    /// <summary>Builds the boxes of an empty code of the default length.</summary>
    public InputOtp() => Refresh();

    /// <summary>The code typed so far, at most <see cref="Length"/> characters.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>How many boxes the code has (6 by default).</summary>
    public int Length
    {
        get => GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    /// <summary>How many boxes each group holds, a separator between groups; 0 (the default) keeps one group.</summary>
    public int GroupSize
    {
        get => GetValue(GroupSizeProperty);
        set => SetValue(GroupSizeProperty, value);
    }

    /// <summary>The boxes, rebuilt as the text, the length, the groups and the focus change.</summary>
    public IReadOnlyList<OtpSlot> Slots
    {
        get => _slots;
        private set => SetAndRaise(SlotsProperty, ref _slots, value);
    }

    private void Refresh()
    {
        var text = Text ?? string.Empty;
        var length = Math.Max(1, Length);
        var group = GroupSize > 0 ? GroupSize : length;
        Slots = [.. Enumerable.Range(0, length).Select(index => new OtpSlot(
            index < text.Length ? text[index].ToString() : string.Empty,
            IsKeyboardFocusWithin && index == Math.Min(text.Length, length - 1),
            index % group == 0,
            index % group == group - 1 || index == length - 1,
            index > 0 && index % group == 0))];
    }
}
