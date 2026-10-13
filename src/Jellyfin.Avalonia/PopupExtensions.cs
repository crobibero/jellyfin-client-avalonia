using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Jellyfin.Avalonia;

/// <summary>
/// Popup helpers for routed input.
/// </summary>
internal static class PopupExtensions
{
    /// <summary>
    /// Gets a value indicating whether an event source is inside an open drop-down or flyout.
    /// </summary>
    /// <remarks>
    /// Desktop hosts popups in their own <see cref="PopupRoot"/> window. Browser and mobile have no popup windows and host
    /// them in an <see cref="OverlayPopupHost"/> inside the main top level.
    /// </remarks>
    /// <param name="source">The event source.</param>
    /// <returns>Whether the source is inside a popup.</returns>
    public static bool IsInPopup(this object? source)
        => source is Visual visual && visual.GetSelfAndVisualAncestors().Any(v => v is PopupRoot or OverlayPopupHost);
}
