using System.Text.Json;
using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.Pages.Login;
using Jellyfin.Maui.ViewModels;
using Jellyfin.Maui.ViewModels.Facades;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class NavigationService : INavigationService
{
    private Application _application = Application.Current!;
    private NavigationPage? _loginNavigationPage;

    /// <inheritdoc />
    public void NavigateToUserSelectPage()
    {
        if (_loginNavigationPage is null)
        {
            NavigateToServerSelectPage();
            return;
        }

        Application.Current?.Dispatcher.Dispatch(() =>
        {
            var userSelectPage = InternalServiceProvider.GetService<SelectUserPage>();
            _loginNavigationPage.PushAsync(userSelectPage).SafeFireAndForget();
        });
    }

    /// <inheritdoc />
    public void NavigateToLoginPage()
    {
        if (_loginNavigationPage is null)
        {
            NavigateToServerSelectPage();
            return;
        }

        Application.Current?.Dispatcher.Dispatch(() =>
        {
            var loginPage = InternalServiceProvider.GetService<LoginPage>();
            _loginNavigationPage.PushAsync(loginPage).SafeFireAndForget();
        });
    }

    /// <inheritdoc />
    public void NavigateToServerSelectPage()
    {
        if (_loginNavigationPage is null)
        {
            Application.Current?.Dispatcher.Dispatch(() =>
            {
                var serverSelectPage = InternalServiceProvider.GetService<SelectServerPage>();
                _loginNavigationPage = new NavigationPage(serverSelectPage);
                _application.MainPage = _loginNavigationPage;
            });
        }
        else
        {
            Application.Current?.Dispatcher.Dispatch(() => _loginNavigationPage.PopToRootAsync(true).SafeFireAndForget());
        }
    }

    /// <inheritdoc />
    public void NavigateToAddServerPage()
    {
        if (_loginNavigationPage is null)
        {
            NavigateToServerSelectPage();
            return;
        }

        Application.Current?.Dispatcher.Dispatch(() =>
        {
            var addServerPage = InternalServiceProvider.GetService<AddServerPage>();
            _loginNavigationPage.PushAsync(addServerPage).SafeFireAndForget();
        });
    }

    /// <inheritdoc />
    public void NavigateHome()
    {
        Application.Current?.Dispatcher.Dispatch(() =>
        {
            if (Shell.Current == null)
            {
                _application.MainPage = InternalServiceProvider.GetService<AppShell>();
            }
            else
            {
                Shell.Current.GoToAsync($"//{nameof(HomePage)}", true);
            }
        });
    }

    /// <inheritdoc />
    public void NavigateToItemView(BaseItemDto item)
    {
        switch (item.Type)
        {
            case BaseItemKind.CollectionFolder:
                Navigate<LibraryViewModel>(item);
                break;
            default:
                Navigate<ItemViewModel>(item);
                break;
        }
    }

    private void Navigate<TViewModel>(BaseItemDto item)
        where TViewModel : BaseItemViewModel
    {
        var pageType = typeof(NavigationService).Assembly.GetTypes()
            .FirstOrDefault(x => typeof(BaseContentPage<TViewModel>).IsAssignableFrom(x));

        if (pageType == null)
        {
#pragma warning disable CA2254
            InternalServiceProvider.GetService<ILogger>().LogWarning($"The ViewModel '{typeof(TViewModel).Name}' is not associated with a BaseContentPage<>");
#pragma warning restore CA2254
            return;
        }

        Application.Current?.Dispatcher.Dispatch(() =>
        {
            Shell.Current.GoToAsync($"{pageType.Name}?args={Uri.EscapeDataString(JsonSerializer.Serialize(item))}", true);
        });
    }
}
