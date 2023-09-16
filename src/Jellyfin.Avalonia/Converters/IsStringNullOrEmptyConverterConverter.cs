using System.Globalization;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Converts the incoming value to a <see cref="bool"/> indicating whether or not the value is null or empty.
/// </summary>
public class IsStringNullOrEmptyConverterConverter : BaseConverterOneWay<string?, bool>
{
    /// <inheritdoc />
    protected override bool ConvertFrom(string? value, CultureInfo? culture)
        => string.IsNullOrEmpty(value);
}
