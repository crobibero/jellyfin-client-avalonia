using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.ViewModels;

/// <summary>
/// Navigation view model.
/// </summary>
public partial class ContentNavigationViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private BaseViewModel? _currentPage;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentNavigationViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="serviceProvider">Instance of the <see cref="IServiceProvider"/> interface.</param>
    public ContentNavigationViewModel(INavigationService navigationService, IApplicationService applicationService, IServiceProvider serviceProvider)
        : base(navigationService, applicationService)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public override ValueTask InitializeAsync()
    {
        CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
        return ValueTask.CompletedTask;
    }
}
