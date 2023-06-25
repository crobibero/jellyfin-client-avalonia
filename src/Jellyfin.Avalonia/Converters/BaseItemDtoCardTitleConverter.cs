using System.Globalization;
using Jellyfin.Sdk;

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
            BaseItemKind.Episode => value.SeriesName,
            BaseItemKind.Season => value.SeriesName,
            _ => value?.Name?.ToString(culture) ?? string.Empty
        };
    }
}
