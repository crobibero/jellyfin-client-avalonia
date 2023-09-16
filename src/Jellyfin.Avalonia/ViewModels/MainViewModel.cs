using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Avalonia.ViewModels;

/// <summary>
/// Main view model.
/// </summary>
public class MainViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    public MainViewModel(INavigationService navigationService, IApplicationService applicationService)
        : base(navigationService, applicationService)
    {
    }

    /// <inheritdoc />
    public override ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
