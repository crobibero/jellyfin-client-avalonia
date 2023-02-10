using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.Pages.Login;
using Jellyfin.Maui.ViewModels;
using Jellyfin.Maui.ViewModels.Facades;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class ShellNavigationService : INavigationService
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

        _application.Dispatcher.Dispatch(() =>
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

        _application.Dispatcher.Dispatch(() =>
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
            _application.Dispatcher.Dispatch(() =>
            {
                var serverSelectPage = InternalServiceProvider.GetService<SelectServerPage>();
                _application.MainPage = _loginNavigationPage = new NavigationPage(serverSelectPage);
            });
        }
        else
        {
            _application.Dispatcher.Dispatch(() => _loginNavigationPage.PopToRootAsync(true).SafeFireAndForget());
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

        _application.Dispatcher.Dispatch(() =>
        {
            var addServerPage = InternalServiceProvider.GetService<AddServerPage>();
            _loginNavigationPage.PushAsync(addServerPage).SafeFireAndForget();
        });
    }

    /// <inheritdoc />
    public void NavigateHome()
    {
        _application.Dispatcher.Dispatch(() =>
        {
            if (Shell.Current is null)
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
                Navigate<LibraryViewModel>(item.Id);
                break;
            default:
                Navigate<ItemViewModel>(item.Id);
                break;
        }
    }

    private void Navigate<TViewModel>(Guid itemId)
        where TViewModel : BaseItemViewModel
    {
        var pageType = typeof(ShellNavigationService).Assembly.GetTypes()
            .FirstOrDefault(x => typeof(BaseContentPage<TViewModel>).IsAssignableFrom(x));

        if (pageType is null)
        {
            InternalServiceProvider.GetService<ILogger>().LogWarning("The ViewModel '{Name}' is not associated with a BaseContentPage<>", typeof(TViewModel).Name);
            return;
        }

        _application.Dispatcher.Dispatch(() =>
        {
            Shell.Current.GoToAsync($"{pageType.Name}?itemId={itemId}", true);
        });
    }
}
