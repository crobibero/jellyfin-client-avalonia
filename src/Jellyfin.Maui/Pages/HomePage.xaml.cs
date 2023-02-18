using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.Services;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// The add server page.
/// </summary>
public partial class HomePage : BaseContentPage<HomeViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="HomeViewModel"/>.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{HomePage}"/>.</param>
    public HomePage(HomeViewModel viewModel, ILogger<HomePage> logger)
        : base(viewModel, logger)
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Shell.Current is null && !ToolbarItems.Any())
        {
            ToolbarItems.Add(new ToolbarItem
            {
                IconImageSource = "logout.png",
                Command = new Command(() =>
                {
                    InternalServiceProvider.GetService<IAuthenticationService>().Logout();
                    InternalServiceProvider.GetService<INavigationService>().NavigateToServerSelectPage();
                }),
            });
        }
    }
}
