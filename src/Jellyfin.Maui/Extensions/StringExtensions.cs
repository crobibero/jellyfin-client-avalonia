namespace Jellyfin.Maui.Extensions;

/// <summary>
/// String extensions.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Convert the provided string to a <see cref="Uri"/>.
    /// </summary>
    /// <param name="uriString">The uri string.</param>
    /// <returns>The uri.</returns>
    public static Uri ToUri(this string uriString)
        => new(uriString);
}
