using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Jellyfin.Avalonia.Controls;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Avalonia.Behaviors;

/// <summary>
/// Connects item card buttons to the page view model: selecting a card navigates to it,
/// focusing or hovering a card updates the details header and backdrop.
/// </summary>
public static class CardBehavior
{
    private const double FocusMargin = 24;

    /// <summary>
    /// Defines the IsItemCard attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsItemCardProperty =
        AvaloniaProperty.RegisterAttached<Button, bool>("IsItemCard", typeof(CardBehavior));

    static CardBehavior()
    {
        IsItemCardProperty.Changed.AddClassHandler<Button>(OnIsItemCardChanged);
    }

    /// <summary>
    /// Gets the IsItemCard value.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <returns>The value.</returns>
    public static bool GetIsItemCard(Button button) => button.GetValue(IsItemCardProperty);

    /// <summary>
    /// Sets the IsItemCard value.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="value">The value.</param>
    public static void SetIsItemCard(Button button, bool value) => button.SetValue(IsItemCardProperty, value);

    private static void OnIsItemCardChanged(Button button, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            button.Click += OnClick;
            button.GotFocus += OnGotFocus;
            button.PointerMoved += OnPointerMoved;
        }
        else
        {
            button.Click -= OnClick;
            button.GotFocus -= OnGotFocus;
            button.PointerMoved -= OnPointerMoved;
        }
    }

    private static BaseViewModel? GetPageViewModel(Visual visual)
        => visual.FindAncestorOfType<PageView>()?.DataContext as BaseViewModel;

    private static void OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ItemCardModel card } button
            && GetPageViewModel(button) is { } viewModel
            && viewModel.NavigateToItemCommand.CanExecute(card))
        {
            viewModel.NavigateToItemCommand.Execute(card);
        }
    }

    private static void OnGotFocus(object? sender, FocusChangedEventArgs e)
    {
        // A focused card scales up past its layout bounds, so keep a margin around it in view.
        if (sender is Button button && e.NavigationMethod is NavigationMethod.Directional or NavigationMethod.Tab)
        {
            button.BringIntoView(new Rect(button.Bounds.Size).Inflate(FocusMargin));
        }

        Focus(sender);
    }

    // PointerEntered also fires when a scrolling row moves a card under a resting pointer, pulling the header
    // away from keyboard focus. PointerMoved only fires when the pointer moves.
    private static void OnPointerMoved(object? sender, PointerEventArgs e) => Focus(sender);

    private static void Focus(object? sender)
    {
        if (sender is Button { DataContext: ItemCardModel card } button
            && GetPageViewModel(button) is BrowseViewModel viewModel)
        {
            viewModel.FocusCardCommand.Execute(card);
        }
    }
}
