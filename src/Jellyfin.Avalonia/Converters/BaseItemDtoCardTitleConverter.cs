using System.Globalization;
using Jellyfin.Sdk.Generated.Models;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Converts the <see cref="BaseItemDto"/> to a title.
/// </summary>
public class BaseItemDtoCardTitleConverter : BaseConverterOneWay<BaseItemDto?, string?>
{
    /// <inheritdoc />
    protected override string? ConvertFrom(BaseItemDto? value, CultureInfo? culture)
    {
        if (value is null)
        {
            return null;
        }

        return value.Type switch
        {
            BaseItemDto_Type.Episode => value.SeriesName,
            BaseItemDto_Type.Season => value.SeriesName,
            _ => value?.Name?.ToString(culture) ?? string.Empty
        };
    }
}
