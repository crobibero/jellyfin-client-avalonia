using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Avalonia.ViewModels;

/// <summary>
/// Navigation view model.
/// </summary>
public partial class ContentNavigationViewModel : BaseViewModel
{
    [ObservableProperty]
    private BaseViewModel? _currentPage;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentNavigationViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    public ContentNavigationViewModel(INavigationService navigationService, IApplicationService applicationService)
        : base(navigationService, applicationService)
    {
    }

    /// <inheritdoc />
    public override ValueTask InitializeAsync()
        => ValueTask.CompletedTask;
}
