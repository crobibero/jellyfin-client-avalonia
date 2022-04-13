using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace Jellyfin.Maui.Converters;

/// <summary>
/// Converts the incoming balue to a <see cref="bool"/> indicating whether the value is not null.
/// </summary>
public class IsObjectNotNullConverter : BaseConverterOneWay<object?, bool>
{
    /// <inheritdoc/>
    public override bool ConvertFrom(object? value, CultureInfo? culture) => value is not null;
}
