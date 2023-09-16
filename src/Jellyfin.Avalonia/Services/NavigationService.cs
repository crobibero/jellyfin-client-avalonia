using Jellyfin.Avalonia.ViewModels;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels;
using Jellyfin.Mvvm.ViewModels.Login;
using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Implementation of the <see cref="INavigationService"/>.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private MainWindowViewModel _mainWindowViewModel = null!;
    private ContentNavigationViewModel? _contentNavigationViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    /// <param name="serviceProvider">Instance of the <see cref="IServiceProvider"/> interface.</param>
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Initialize the navigation service.
    /// </summary>
    /// <param name="mainWindowViewModel">The main window view model.</param>
    public void Initialize(MainWindowViewModel mainWindowViewModel)
        => _mainWindowViewModel = mainWindowViewModel;

    /// <inheritdoc />
    public void NavigateToServerSelectPage()
    {
        if (_mainWindowViewModel.CurrentPage is not ServerSelectViewModel)
        {
            _mainWindowViewModel.CurrentPage = _serviceProvider.GetRequiredService<ServerSelectViewModel>();
        }
    }

    /// <inheritdoc />
    public void NavigateToAddServerPage()
    {
        if (_mainWindowViewModel.CurrentPage is not AddServerViewModel)
        {
            _mainWindowViewModel.CurrentPage = _serviceProvider.GetRequiredService<AddServerViewModel>();
        }
    }

    /// <inheritdoc />
    public void NavigateToUserSelectPage()
    {
        if (_mainWindowViewModel.CurrentPage is not SelectUserViewModel)
        {
            _mainWindowViewModel.CurrentPage = _serviceProvider.GetRequiredService<SelectUserViewModel>();
        }
    }

    /// <inheritdoc />
    public void NavigateToLoginPage()
    {
        if (_mainWindowViewModel.CurrentPage is not LoginViewModel)
        {
            _mainWindowViewModel.CurrentPage = _serviceProvider.GetRequiredService<LoginViewModel>();
        }
    }

    /// <inheritdoc />
    public void NavigateHome()
    {
        if (_mainWindowViewModel.CurrentPage is not ContentNavigationViewModel
            || _contentNavigationViewModel is null)
        {
            _contentNavigationViewModel ??= _serviceProvider.GetRequiredService<ContentNavigationViewModel>();
            _mainWindowViewModel.CurrentPage = _contentNavigationViewModel;
        }

        _contentNavigationViewModel.CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
    }

    /// <inheritdoc />
    public void NavigateToItemView(BaseItemDto item)
    {
        if (_mainWindowViewModel.CurrentPage is not ContentNavigationViewModel
            || _contentNavigationViewModel is null)
        {
            NavigateHome();
            return;
        }

        ArgumentNullException.ThrowIfNull(item);

        var itemViewModel = _serviceProvider.GetRequiredService<ItemViewModel>();
        itemViewModel.Initialize(item.Id);
        _contentNavigationViewModel.CurrentPage = itemViewModel;
    }
}
