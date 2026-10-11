namespace Jellyfin.Mvvm.Models;

/// <summary>
/// A metadata chip shown in the item info row.
/// </summary>
/// <param name="Text">The chip text.</param>
/// <param name="Style">The chip style.</param>
/// <param name="Icon">An optional icon.</param>
public sealed record InfoChip(string Text, InfoChipStyle Style = InfoChipStyle.Transparent, InfoChipIcon Icon = InfoChipIcon.None);

/// <summary>
/// The style of an info chip.
/// </summary>
public enum InfoChipStyle
{
    /// <summary>
    /// Plain text without a background.
    /// </summary>
    Transparent = 0,

    /// <summary>
    /// Light outlined badge.
    /// </summary>
    Default = 1,

    /// <summary>
    /// Green badge.
    /// </summary>
    Green = 2,

    /// <summary>
    /// Red badge.
    /// </summary>
    Red = 3
}

/// <summary>
/// The icon of an info chip.
/// </summary>
public enum InfoChipIcon
{
    /// <summary>
    /// No icon.
    /// </summary>
    None = 0,

    /// <summary>
    /// Community rating star.
    /// </summary>
    Star = 1,

    /// <summary>
    /// Fresh critic rating.
    /// </summary>
    CriticFresh = 2,

    /// <summary>
    /// Rotten critic rating.
    /// </summary>
    CriticRotten = 3,

    /// <summary>
    /// Runtime clock.
    /// </summary>
    Time = 4
}
