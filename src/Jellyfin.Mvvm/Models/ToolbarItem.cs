namespace Jellyfin.Mvvm.Models;

/// <summary>
/// The main toolbar items.
/// </summary>
public enum ToolbarItem
{
    /// <summary>
    /// No item is active.
    /// </summary>
    None = 0,

    /// <summary>
    /// The home item.
    /// </summary>
    Home = 1,

    /// <summary>
    /// The search item.
    /// </summary>
    Search = 2,

    /// <summary>
    /// The libraries item.
    /// </summary>
    Libraries = 3,

    /// <summary>
    /// The settings item.
    /// </summary>
    Settings = 4
}
