using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Aioros.Cosmo.Chat;

/// <summary>Where a <see cref="MessageScroller"/> opens, shadcn/ui's <c>defaultScrollPosition</c>.</summary>
public enum MessageScrollPosition
{
    /// <summary>At the newest message, following the transcript as it grows.</summary>
    End,

    /// <summary>At the first message.</summary>
    Start,
}

/// <summary>
/// shadcn/ui's MessageScroller: a chat transcript's scroll container that opens at its
/// <see cref="DefaultScrollPosition"/>, keeps the newest message in view while it follows the end (<c>:following</c>,
/// its 10 px scroll bar gutter hidden as shadcn/ui hides the bar then) and shows a scroll-to-end button 16 px above its
/// bottom while it is not at its end (<c>:end</c> otherwise).
/// </summary>
[TemplatePart("PART_Viewport", typeof(ScrollViewer))]
[TemplatePart("PART_Button", typeof(Button))]
[PseudoClasses(":end", ":following")]
public sealed class MessageScroller : ContentControl
{
    /// <summary>Defines the <see cref="DefaultScrollPosition"/> property.</summary>
    public static readonly StyledProperty<MessageScrollPosition> DefaultScrollPositionProperty =
        AvaloniaProperty.Register<MessageScroller, MessageScrollPosition>(nameof(DefaultScrollPosition));

    private ScrollViewer? viewport;
    private Button? button;

    /// <summary>Where the transcript opens: at its end (the default) or at its start.</summary>
    public MessageScrollPosition DefaultScrollPosition
    {
        get => GetValue(DefaultScrollPositionProperty);
        set => SetValue(DefaultScrollPositionProperty, value);
    }

    /// <summary>Scrolls to the newest message and follows the transcript from there.</summary>
    public void ScrollToEnd()
    {
        PseudoClasses.Set(":following", true);
        viewport?.ScrollToEnd();
    }

    /// <summary>Scrolls to the first message.</summary>
    public void ScrollToStart()
    {
        PseudoClasses.Set(":following", false);
        viewport?.ScrollToHome();
    }

    /// <summary>Takes the template's viewport and button.</summary>
    /// <param name="e">The applied template.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (viewport is not null)
        {
            viewport.ScrollChanged -= OnScrollChanged;
        }

        if (button is not null)
        {
            button.Click -= OnButtonClick;
        }

        viewport = e.NameScope.Find<ScrollViewer>("PART_Viewport");
        button = e.NameScope.Find<Button>("PART_Button");
        if (viewport is not null)
        {
            viewport.ScrollChanged += OnScrollChanged;
        }

        if (button is not null)
        {
            button.Click += OnButtonClick;
        }

        PseudoClasses.Set(":following", DefaultScrollPosition == MessageScrollPosition.End);
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e) => ScrollToEnd();

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (viewport is null)
        {
            return;
        }

        var resized = e.ExtentDelta.Y != 0 || e.ViewportDelta.Y != 0;
        var following = PseudoClasses.Contains(":following") && resized;
        if (following)
        {
            viewport.ScrollToEnd();
        }

        var end = viewport.Offset.Y >= viewport.Extent.Height - viewport.Viewport.Height - 1;
        PseudoClasses.Set(":end", end);
        if (!following && e.OffsetDelta.Y != 0)
        {
            PseudoClasses.Set(":following", end);
        }
    }
}
