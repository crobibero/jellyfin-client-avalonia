using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Avalonia.Services;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.ViewModels;

/// <summary>
/// The main window view model.
/// </summary>
public partial class MainWindowViewModel : BaseViewModel
{
    [ObservableProperty]
    private BaseViewModel? _currentPage;

    [ObservableProperty]
    private bool _authenticated;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/>.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="serviceProvider">Instance of the <see cref="IServiceProvider"/> interface.</param>
    public MainWindowViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IServiceProvider serviceProvider)
        : base(navigationService, applicationService)
    {
        // TODO refactor to not require manual initialize.
        (navigationService as NavigationService)!.Initialize(this);

        // TODO move to InitializeAsync.
        CurrentPage = serviceProvider.GetRequiredService<LoadingViewModel>();
    }

    /// <inheritdoc />
    public override ValueTask InitializeAsync()
        => ValueTask.CompletedTask;
}
