using Jellyfin.Maui.Services;
using Microsoft.Maui.Controls;

namespace Jellyfin.Maui.Pages
{
    /// <summary>
    /// The main page.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private readonly INavigationService _navigationService;
        private readonly IStateService _stateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
        /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
        public MainPage(
            INavigationService navigationService,
            IStateService stateService)
        {
            _navigationService = navigationService;
            _stateService = stateService;
        }

        /// <inheridoc />
        protected override void OnAppearing()
        {
            Redirect();
        }

        /// <inheridoc />
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            Redirect();
        }

        private void Redirect()
        {
            var state = _stateService.GetState();
            if (string.IsNullOrEmpty(state.Token))
            {
                _navigationService.Navigate<LoginPage>();
            }
            else
            {
                _navigationService.Navigate<HomePage>();
            }
        }
    }
}
