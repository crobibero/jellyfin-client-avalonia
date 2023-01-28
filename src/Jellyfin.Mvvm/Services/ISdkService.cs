namespace Jellyfin.Maui.Services;

public interface ISdkService
{
    /// <summary>
    /// Perform platform-specific initialization.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asyncronous operation.</returns>
    ValueTask InitializeAsync();
}
