using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace Jellyfin.Maui.Services;

/// <summary>
/// Service provider.
/// </summary>
public static class ServiceProvider
{
    private static IServiceProvider Current =>
#if WINDOWS10_0_17763_0_OR_GREATER
            MauiWinUIApplication.Current.Services;
#elif ANDROID
            MauiApplication.Current.Services;
#elif IOS || MACCATALYST
        MauiUIApplicationDelegate.Current.Services;
#else
			throw new NotImplementedException();
#endif

    /// <summary>
    /// Gets the service.
    /// </summary>
    /// <typeparam name="T">The type of service.</typeparam>
    /// <returns>The service.</returns>
    public static T GetService<T>()
        where T : notnull
        => Current.GetRequiredService<T>();

}
