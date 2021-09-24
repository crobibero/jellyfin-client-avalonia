using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Services;

namespace Jellyfin.Maui.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public HomeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            NavigateCommand = new AsyncRelayCommand(NavigateAsync);
        }

        /// <summary>
        /// Gets the login command.
        /// </summary>
        public IAsyncRelayCommand NavigateCommand { get; }

        private async Task NavigateAsync()
        {
            await _navigationService.NavigateAsync<LibraryPage>(Guid.NewGuid())
                .ConfigureAwait(false);
        }
    }
}
