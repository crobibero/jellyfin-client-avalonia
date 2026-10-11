using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Implementation of the <see cref="IApplicationService"/>.
/// </summary>
public class ApplicationService : IApplicationService
{
    /// <inheritdoc />
    public void Post(Action action) => Dispatcher.UIThread.Post(action);

    /// <inheritdoc />
    public async Task DispatchAsync(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(action);
    }

    /// <inheritdoc />
    public bool IsUiThread() => Dispatcher.UIThread.CheckAccess();

    /// <inheritdoc />
    public void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
