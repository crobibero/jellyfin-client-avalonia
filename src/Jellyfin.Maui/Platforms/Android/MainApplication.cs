namespace Jellyfin.Maui.Platforms.Android;

[Application]
public class MainApplication : MauiApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainApplication"/> class.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <param name="ownership">The handle owner.</param>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        var version = typeof(MauiProgram).Assembly.GetName().Version?.ToString() ?? "0.0.0.1";
        // TODO only 1 token per DeviceId is allowed...
        this.Services.GetRequiredService<SdkClientSettings>()
            .InitializeClientSettings(
                "Jellyfin Maui Android",
                version,
                "Android",
                Guid.NewGuid().ToString("N"));
    }

    /// <inheritdoc />
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
