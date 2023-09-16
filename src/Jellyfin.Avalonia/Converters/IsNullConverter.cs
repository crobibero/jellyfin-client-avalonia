using System.Globalization;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Converts the incoming value to a <see cref="bool"/> indicating whether or not the value is null.
/// </summary>
public class IsNullConverter : BaseConverterOneWay<object?, bool>
{
    /// <inheritdoc />
    protected override bool ConvertFrom(object? value, CultureInfo? culture)
        => value is null;
}
