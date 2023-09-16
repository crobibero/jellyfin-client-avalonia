using System.Collections;
using System.Globalization;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Converts the incoming value to a <see cref="bool"/> indicating whether or not the value is null or empty.
/// </summary>
public class IsListNullOrEmptyConverter : BaseConverterOneWay<IEnumerable?, bool>
{
    /// <inheritdoc />
    protected override bool ConvertFrom(IEnumerable? value, CultureInfo? culture)
        => IsListNullOrEmpty(value);

    internal static bool IsListNullOrEmpty(IEnumerable? value) => value switch
    {
        null => true,
        _ => !value.GetEnumerator().MoveNext()
    };
}
