namespace Jellyfin.Mvvm.Models;

/// <summary>
/// The shape of an item card.
/// </summary>
public enum CardShape
{
    /// <summary>
    /// A 2:3 poster card.
    /// </summary>
    Poster = 0,

    /// <summary>
    /// A 16:9 thumbnail card.
    /// </summary>
    Thumb = 1,

    /// <summary>
    /// A 1:1 square card.
    /// </summary>
    Square = 2
}
