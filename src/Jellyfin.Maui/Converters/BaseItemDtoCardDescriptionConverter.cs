using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace Jellyfin.Maui.Converters;

/// <summary>
/// BaseItemDto Card Description Converter.
/// </summary>
public class BaseItemDtoCardDescriptionConverter : BaseConverterOneWay<BaseItemDto?, string?>
{
    /// <inheritdoc/>
    public override string? DefaultConvertReturnValue { get; set; }

    /// <inheritdoc/>
    public override string? ConvertFrom(BaseItemDto? value, CultureInfo? culture)
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
