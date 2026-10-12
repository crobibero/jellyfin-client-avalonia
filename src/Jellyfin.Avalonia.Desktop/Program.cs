using System.Globalization;
using System.Text;
using Avalonia;
using Jellyfin.Avalonia.LibVlc;
using Jellyfin.Avalonia.Services;
using Jellyfin.Mvvm.Playback;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Jellyfin.Avalonia.Desktop;

/// <summary>
/// The desktop entry point for Windows, macOS and Linux.
/// </summary>
internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        // Decoder and image threads otherwise spread native allocations across many arenas that never shrink.
        NativeHeap.LimitArenas(2);
        ConfigureLogging();

        try
        {
            App.ConfigurePlatformServices = services =>
            {
                services.AddLogging(builder => builder.AddSerilog(dispose: true));
                services.AddSingleton<IPlayerBackend, LibVlcPlayerBackend>();
                services.AddSingleton<IVideoViewFactory, LibVlcVideoViewFactory>();
            };

            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Avalonia configuration, don't remove; also used by visual designer.
    /// </summary>
    /// <returns>The app builder.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithFigtreeFont()
            .LogToTrace();

    private static void ConfigureLogging()
    {
        var logPath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
            AppConstants.Identifier,
            "logs",
            "jellyfin-avalonia-.log");

        const string OutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {SourceContext}: {Message}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate: OutputTemplate, formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                path: logPath,
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture,
                encoding: Encoding.UTF8,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10)
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}
