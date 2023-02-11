using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// App view model.
/// </summary>
public partial class AppViewModel : BaseViewModel
{
    [ObservableProperty]
    private string? _username;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    public AppViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateService stateService)
        : base(navigationService, applicationService)
    {
        Username = stateService.GetCurrentUser().Name;
    }

    /// <inheritdoc/>
    public override ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
