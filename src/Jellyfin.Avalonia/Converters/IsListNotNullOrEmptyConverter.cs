using System.Collections;
using System.Globalization;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Converts the incoming value to a <see cref="bool"/> indicating whether or not the value is not null and not empty.
/// </summary>
public class IsListNotNullOrEmptyConverter : BaseConverterOneWay<IEnumerable?, bool>
{
    /// <inheritdoc />
    protected override bool ConvertFrom(IEnumerable? value, CultureInfo? culture)
        => !IsListNullOrEmptyConverter.IsListNullOrEmpty(value);
}
