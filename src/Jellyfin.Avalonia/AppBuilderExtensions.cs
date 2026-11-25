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

    /// <summary>
    /// Raise the GPU texture cache from Avalonia's 28 MB default.
    /// </summary>
    /// <remarks>
    /// A window of posters with their mipmaps can exceed the default. Past the limit Skia evicts textures that are
    /// still on screen and uploads them again on every frame. Skia only frees a texture when the cache is over the
    /// limit or its image is freed, so the textures of off-screen images the image loader still caches stay too.
    /// </remarks>
    /// <param name="builder">The app builder.</param>
    /// <returns>The app builder, for chaining.</returns>
    public static AppBuilder WithGpuResourceCache(this AppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.With(new SkiaOptions { MaxGpuResourceSizeBytes = 128L * 1024 * 1024 });
    }
}
