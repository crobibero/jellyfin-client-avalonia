using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.Pages.Login;
using Jellyfin.Maui.Services;

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
    public AppShell(
        INavigationService navigationService,
        IAuthenticationService authenticationService)
    {
        InitializeComponent();

        _navigationService = navigationService;
        _authenticationService = authenticationService;

        this.Items.Add(_tabBar = new FlyoutItem
        {
            FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem,
        });

        _tabBar.Items.Add(new ShellContent
        {
            Content = InternalServiceProvider.GetService<HomePage>(),
            Route = nameof(HomePage),
            Title = Strings.Home,
            Icon = new FontImageSource
            {
                Glyph = "⌂",
            },
        });

        _tabBar.Items.Add(new ShellContent
        {
            Content = new SettingsPage(),
            Route = nameof(SettingsPage),
            Title = Strings.Settings,
            Icon = new FontImageSource
            {
                Glyph = "⚙",
            },
        });

        this.Items.Add(new MenuItem
        {
            Text = Strings.SwitchUser,
            IconImageSource = "logout.png",
            Command = new Command(() =>
            {
                _authenticationService.Logout();
                _navigationService.NavigateToServerSelectPage();
            }),
        });

        var allTypesOfPages = from x in typeof(AppShell).Assembly.GetTypes()
                                let y = x.BaseType
                                where !x.IsAbstract && !x.IsInterface
                                && y != null && y.IsGenericType
                                && (y.GetGenericTypeDefinition() == typeof(BaseContentPage<>) || y.GetGenericTypeDefinition() == typeof(BaseContentIdPage<>))
                                select x;

        foreach (var pageType in allTypesOfPages)
        {
            Routing.RegisterRoute(pageType.Name, pageType);
        }
    }
}
