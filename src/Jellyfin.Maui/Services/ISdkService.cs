namespace Jellyfin.Maui.Services;

internal interface ISdkService
{
    /// <summary>
    /// Perform platform-specific initialization.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asyncronous operation.</returns>
    ValueTask InitializeAsync();
}
