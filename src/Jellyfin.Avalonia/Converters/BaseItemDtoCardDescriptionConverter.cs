using System.Globalization;
using Jellyfin.Sdk;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Converts the <see cref="BaseItemDto"/> to a description.
/// </summary>
public class BaseItemDtoCardDescriptionConverter : BaseConverterOneWay<BaseItemDto?, string?>
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
            BaseItemKind.Episode => $"S{value.ParentIndexNumber} E{value.IndexNumber} {value.Name}",
            BaseItemKind.Season => value.SeasonName,
            _ => value.ProductionYear?.ToString(culture) ?? string.Empty
        };
    }
}
