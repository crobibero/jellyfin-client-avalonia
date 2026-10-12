using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Jellyfin.Avalonia.Behaviors;

/// <summary>
/// Keeps the keyboard focus highlight on a combo box after its drop-down closes.
/// </summary>
/// <remarks>
/// When an item is chosen the combo box focuses it, then the drop-down closes and focus falls back to the
/// combo box without a navigation method, which clears :focus-visible. Remote and keyboard users then lose
/// track of the focused control.
/// </remarks>
public static class ComboBoxFocusBehavior
{
    private static readonly AttachedProperty<bool> WasFocusVisibleProperty =
        AvaloniaProperty.RegisterAttached<ComboBox, bool>("WasFocusVisible", typeof(ComboBoxFocusBehavior));

    /// <summary>
    /// Applies the behavior to every combo box.
    /// </summary>
    public static void Register()
    {
        ComboBox.IsDropDownOpenProperty.Changed.AddClassHandler<ComboBox>(OnIsDropDownOpenChanged);
        InputElement.KeyDownEvent.AddClassHandler<ComboBoxItem>(OnItemKeyDown, RoutingStrategies.Tunnel);
    }

    private static void OnIsDropDownOpenChanged(ComboBox comboBox, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            comboBox.SetValue(WasFocusVisibleProperty, comboBox.Classes.Contains(":focus-visible"));
            return;
        }

        // Closed without a choice, for example with Escape, while focus is still in the drop-down.
        if (comboBox.GetValue(WasFocusVisibleProperty))
        {
            comboBox.ClearValue(WasFocusVisibleProperty);
            comboBox.Focus(NavigationMethod.Directional);
        }
    }

    private static void OnItemKeyDown(ComboBoxItem item, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Space)
            || ItemsControl.ItemsControlFromItemContainer(item) is not ComboBox { IsDropDownOpen: true } comboBox
            || !comboBox.GetValue(WasFocusVisibleProperty))
        {
            return;
        }

        // Select in this order so focus never passes through the drop-down without a navigation method.
        var index = comboBox.IndexFromContainer(item);
        comboBox.Focus(NavigationMethod.Directional);
        comboBox.SetCurrentValue(ComboBox.IsDropDownOpenProperty, false);
        comboBox.SelectedIndex = index;
        e.Handled = true;
    }
}
