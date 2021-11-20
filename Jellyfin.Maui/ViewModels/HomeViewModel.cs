using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Services;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Home view model.
/// </summary>
public class HomeViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/>.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    public HomeViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        NavigateCommand = new RelayCommand(NavigateAsync);
    }

    /// <summary>
    /// Gets the login command.
    /// </summary>
    public IRelayCommand NavigateCommand { get; }

    private void NavigateAsync()
    {
        _navigationService.Navigate<LibraryPage>(Guid.NewGuid());
    }
}
