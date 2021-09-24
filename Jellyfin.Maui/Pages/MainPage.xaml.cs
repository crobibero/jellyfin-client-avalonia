#nullable disable

using System.Threading.Tasks;
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
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            await RedirectAsync().ConfigureAwait(false);
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            await RedirectAsync().ConfigureAwait(false);
        }

        private async Task RedirectAsync()
        {
            var state = _stateService.GetState();
            if (string.IsNullOrEmpty(state.Token))
            {
                await _navigationService.NavigateAsync<LoginPage>()
                    .ConfigureAwait(false);
            }
            else
            {
                await _navigationService.NavigateAsync<HomePage>()
                    .ConfigureAwait(false);
            }
        }
    }
}
