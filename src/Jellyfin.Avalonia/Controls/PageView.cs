using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Jellyfin.Avalonia.Controls;

/// <summary>
/// Base class for pages. Handles initial focus and restores focus when navigating back.
/// </summary>
public class PageView : UserControl
{
    /// <summary>
    /// Defines the IsInitialFocus attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsInitialFocusProperty =
        AvaloniaProperty.RegisterAttached<PageView, Control, bool>("IsInitialFocus");

    private WeakReference<IInputElement>? _lastFocused;

    // Item repeaters recycle elements, so the last focused element may show another item by the time the
    // page is shown again. The item it showed is kept to find the element that shows it now.
    private WeakReference<object>? _lastFocusedItem;
    private NavigationMethod _lastFocusMethod;
    private bool _focusTouched;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageView"/> class.
    /// </summary>
    public PageView()
    {
        AddHandler(GotFocusEvent, OnChildGotFocus, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(LostFocusEvent, OnChildLostFocus, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(KeyDownEvent, OnKeyDown);
        AddHandler(KeyDownEvent, (_, _) => _focusTouched = true, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerPressedEvent, (_, _) => _focusTouched = true, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    /// <summary>
    /// Gets the IsInitialFocus value.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The value.</returns>
    public static bool GetIsInitialFocus(Control control) => control.GetValue(IsInitialFocusProperty);

    /// <summary>
    /// Sets the IsInitialFocus value. The first visible control with this flag receives focus when the page is shown.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value.</param>
    public static void SetIsInitialFocus(Control control, bool value) => control.SetValue(IsInitialFocusProperty, value);

    /// <summary>
    /// Move focus into the page if it isn't already there.
    /// </summary>
    public void EnsureFocus()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var focused = topLevel?.FocusManager?.GetFocusedElement() as Visual;
        if (focused is not null && this.IsVisualAncestorOf(focused))
        {
            return;
        }

        if (GetLastFocused() is { } last)
        {
            last.Focus(NavigationMethod.Directional);
            return;
        }

        var initial = this.GetVisualDescendants()
            .OfType<Control>()
            .FirstOrDefault(c => GetIsInitialFocus(c) && c.IsEffectivelyVisible && c.Focusable)
            ?? this.GetVisualDescendants()
                .OfType<Button>()
                .FirstOrDefault(c => c.Classes.Contains("card") && c.IsEffectivelyVisible);

        initial?.Focus(NavigationMethod.Directional);
    }

    /// <summary>
    /// Move focus to the first initial focus control, unless the user has moved focus since the page was first shown.
    /// Call this when initial focus controls appear after loading, because focus may already have settled elsewhere.
    /// </summary>
    public void FocusInitialIfUntouched()
    {
        if (_focusTouched || !this.IsAttachedToVisualTree())
        {
            return;
        }

        this.GetVisualDescendants()
            .OfType<Control>()
            .FirstOrDefault(c => GetIsInitialFocus(c) && c.IsEffectivelyVisible && c.Focusable)
            ?.Focus(NavigationMethod.Directional);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ScheduleFocus();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Returning to the page restores the last focused control, which takes precedence over initial focus.
        _focusTouched = true;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Schedule moving focus into the page once content is laid out.
    /// </summary>
    protected void ScheduleFocus()
    {
        // Content is often populated asynchronously, retry a few times.
        var attempts = 0;
        DispatcherTimer.Run(
            () =>
            {
                if (!this.IsAttachedToVisualTree())
                {
                    return false;
                }

                EnsureFocus();
                var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
                return (focused is null || !this.IsVisualAncestorOf(focused)) && ++attempts < 20;
            },
            TimeSpan.FromMilliseconds(150));
    }

    private IInputElement? GetLastFocused()
    {
        if (_lastFocused is null || !_lastFocused.TryGetTarget(out var last))
        {
            return null;
        }

        object? item = null;
        if (_lastFocusedItem?.TryGetTarget(out item) == true && last is Control control && !ReferenceEquals(control.DataContext, item))
        {
            // The element was recycled for another item; focus the element now showing the original one.
            return this.GetVisualDescendants()
                .OfType<Control>()
                .FirstOrDefault(c => ReferenceEquals(c.DataContext, item) && c.Focusable && c.IsEffectivelyVisible);
        }

        return last is Visual visual && this.IsVisualAncestorOf(visual) && last.IsEffectivelyVisible && last.Focusable ? last : null;
    }

    private void OnChildGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (e.Source is IInputElement element)
        {
            _lastFocused = new WeakReference<IInputElement>(element);
            _lastFocusMethod = e.NavigationMethod;
            _lastFocusedItem = element is Control { DataContext: { } item } && !ReferenceEquals(item, DataContext)
                ? new WeakReference<object>(item)
                : null;
        }
    }

    private void OnChildLostFocus(object? sender, FocusChangedEventArgs e)
    {
        // Async commands disable their button while they run, and Avalonia clears focus from a disabled element.
        // Without a focused element the next key press starts from the top, so focus returns once the button is enabled again.
        if (e.NewFocusedElement is not null || e.Source is not InputElement { IsEffectivelyEnabled: false } element)
        {
            return;
        }

        var method = _lastFocusMethod;
        element.PropertyChanged += OnEnabledChanged;

        void OnEnabledChanged(object? s, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property != IsEffectivelyEnabledProperty || !element.IsEffectivelyEnabled)
            {
                return;
            }

            element.PropertyChanged -= OnEnabledChanged;
            var focusManager = TopLevel.GetTopLevel(this)?.FocusManager;
            if (focusManager is not null && focusManager.GetFocusedElement() is null && this.IsVisualAncestorOf(element))
            {
                element.Focus(method);
            }
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        NavigationDirection? direction = e.Key switch
        {
            Key.Left => NavigationDirection.Left,
            Key.Right => NavigationDirection.Right,
            Key.Up => NavigationDirection.Up,
            Key.Down => NavigationDirection.Down,
            _ => null
        };

        var focusManager = TopLevel.GetTopLevel(this)?.FocusManager;
        if (direction is null || focusManager?.GetFocusedElement() is not Visual focused || !this.IsVisualAncestorOf(focused))
        {
            return;
        }

        // The page scrolls under the toolbar, so a toolbar button can be nearer than a page control scrolled out of view above.
        // Prefer the page's own controls and leave the page only when it has nothing in that direction.
        if (focusManager.FindNextElement(direction.Value, new FindNextElementOptions { SearchRoot = this }) is { } next)
        {
            e.Handled = next.Focus(NavigationMethod.Directional, e.KeyModifiers);
            return;
        }

        // Nothing above in the page: reveal the content above the topmost control, like a header, before leaving the page.
        if (direction == NavigationDirection.Up
            && focused.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault(s => s.Offset.Y > 0 && this.IsVisualAncestorOf(s)) is { } scrollViewer)
        {
            scrollViewer.Offset = scrollViewer.Offset.WithY(0);
            e.Handled = true;
        }
    }
}
