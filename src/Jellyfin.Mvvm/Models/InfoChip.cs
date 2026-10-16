using CommunityToolkit.Mvvm.ComponentModel;

namespace Jellyfin.Mvvm.Models;

/// <summary>
/// A metadata chip shown in the item info row.
/// </summary>
/// <remarks>
/// Chips are observable so a row that changes often, such as the header of the focused card, can update its chips
/// in place and keep their controls.
/// </remarks>
public sealed partial class InfoChip : ObservableObject
{
    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    private InfoChipStyle _style;

    [ObservableProperty]
    private InfoChipIcon _icon;

    /// <summary>
    /// Initializes a new instance of the <see cref="InfoChip"/> class.
    /// </summary>
    /// <param name="text">The chip text.</param>
    /// <param name="style">The chip style.</param>
    /// <param name="icon">An optional icon.</param>
    public InfoChip(string text, InfoChipStyle style = InfoChipStyle.Transparent, InfoChipIcon icon = InfoChipIcon.None)
    {
        _text = text;
        _style = style;
        _icon = icon;
    }

    /// <summary>
    /// Update a list of chips in place to match another, reusing the existing chips.
    /// </summary>
    /// <param name="target">The chips to update.</param>
    /// <param name="values">The chips to show.</param>
    public static void UpdateInPlace(IList<InfoChip> target, IReadOnlyList<InfoChip> values)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(values);

        for (var i = 0; i < values.Count; i++)
        {
            if (i < target.Count)
            {
                target[i].Text = values[i].Text;
                target[i].Style = values[i].Style;
                target[i].Icon = values[i].Icon;
            }
            else
            {
                target.Add(values[i]);
            }
        }

        while (target.Count > values.Count)
        {
            target.RemoveAt(target.Count - 1);
        }
    }
}

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
