using Jellyfin.Maui.Services;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels;
using UraniumUI.Icons.MaterialIcons;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// AppShell (On Windows, tabs are not rendered at the bottom: https://github.com/dotnet/maui/issues/6369).
/// </summary>
public partial class AppShell : Shell
{
    private readonly FlyoutItem _tabBar;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppShell"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    /// <param name="viewModel">Instance of the view model.</param>
    public AppShell(
        INavigationService navigationService,
        IAuthenticationService authenticationService,
        AppViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();

        _navigationService = navigationService;
        _authenticationService = authenticationService;

        Items.Add(_tabBar = new FlyoutItem
        {
            Title = Strings.Home,
            FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem,
            Icon = new FontImageSource
            {
                FontFamily = nameof(MaterialRegular),
                Glyph = MaterialRegular.Home
            },
        });

        _tabBar.Items.Add(new ShellContent
        {
            Content = InternalServiceProvider.GetService<HomePage>(),
            Route = nameof(HomePage),
            Title = Strings.Home,
            Icon = new FontImageSource
            {
                FontFamily = nameof(MaterialRegular),
                Glyph = MaterialRegular.Home
            },
        });

        _tabBar.Items.Add(new ShellContent
        {
            Content = new SettingsPage(),
            Route = nameof(SettingsPage),
            Title = Strings.Settings,
            Icon = new FontImageSource
            {
                FontFamily = nameof(MaterialRegular),
                Glyph = MaterialRegular.Settings
            },
        });

        Items.Add(new MenuItem
        {
            Text = Strings.SwitchUser,
            IconImageSource = new FontImageSource
            {
                FontFamily = nameof(MaterialRegular),
                Glyph = MaterialRegular.Logout
            },
            Command = new Command(() =>
            {
                _authenticationService.Logout();
                _navigationService.NavigateToServerSelectPage();
            }),
        });

        Items.Add(new MenuItem
        {
            Text = "Player",
            IconImageSource = new FontImageSource
            {
                FontFamily = nameof(MaterialRegular),
                Glyph = MaterialRegular.Play_arrow
            },
            Command = new Command(() =>
            {
                _navigationService.NavigateToPlayer(Guid.NewGuid());
            })
        });
    }
}
