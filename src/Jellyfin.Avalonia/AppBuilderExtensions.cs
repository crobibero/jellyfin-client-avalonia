using Avalonia;
using Avalonia.Media.Fonts;

namespace Jellyfin.Avalonia;

/// <summary>
/// Extensions for <see cref="AppBuilder"/>.
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Register the bundled Figtree font, the typeface of the Jellyfin design tokens, as <c>fonts:Figtree</c>.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <returns>The app builder, for chaining.</returns>
    public static AppBuilder WithFigtreeFont(this AppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ConfigureFonts(fontManager => fontManager.AddFontCollection(
            new EmbeddedFontCollection(
                new Uri("fonts:Figtree", UriKind.Absolute),
                new Uri("avares://Jellyfin.Avalonia/Resources/Fonts", UriKind.Absolute))));
    }
}
