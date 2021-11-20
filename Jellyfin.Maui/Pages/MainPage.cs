using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Services;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Main page.
/// </summary>
public class MainPage : ContentPage
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

        GoCommand = new RelayCommand(() => Redirect());
        Content = new StackLayout
        {
            new Button {
                Text = "Go",
                Command = GoCommand
            }
        };
    }

    /*/// <inheridoc />
    protected override void OnAppearing()
    {
        Redirect();
    }*/

    private IRelayCommand GoCommand { get; }

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
