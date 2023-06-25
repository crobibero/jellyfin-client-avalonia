using System.Globalization;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Converts the incoming value to a <see cref="bool"/> indicating whether or not the value is not null.
/// </summary>
public class IsNotNullConverter : BaseConverterOneWay<object?, bool>
{
    /// <inheritdoc />
    protected override bool ConvertFrom(object? value, CultureInfo? culture)
        => value is not null;
}
