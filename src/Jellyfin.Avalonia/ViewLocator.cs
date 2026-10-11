using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Jellyfin.Avalonia.Views;
using Jellyfin.Avalonia.Views.Login;
using Jellyfin.Mvvm.ViewModels;
using Jellyfin.Mvvm.ViewModels.Facades;
using Jellyfin.Mvvm.ViewModels.Login;

namespace Jellyfin.Avalonia;

/// <summary>
/// Resolves views for view models. Views are cached per view model so returning to a page keeps its scroll and focus state.
/// </summary>
public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> _factories = new()
    {
        [typeof(StartupViewModel)] = () => new StartupView(),
        [typeof(ServerSelectViewModel)] = () => new ServerSelectView(),
        [typeof(AddServerViewModel)] = () => new AddServerView(),
        [typeof(SelectUserViewModel)] = () => new SelectUserView(),
        [typeof(LoginViewModel)] = () => new LoginView(),
        [typeof(HomeViewModel)] = () => new HomeView(),
        [typeof(LibraryViewModel)] = () => new LibraryView(),
        [typeof(ItemDetailsViewModel)] = () => new ItemDetailsView(),
        [typeof(SearchViewModel)] = () => new SearchView(),
        [typeof(SettingsViewModel)] = () => new SettingsView(),
        [typeof(DeviceProfileViewModel)] = () => new DeviceProfileView(),
        [typeof(PlayerViewModel)] = () => new PlayerView(),
    };

    private readonly ConditionalWeakTable<BaseViewModel, Control> _cache = new();

    /// <inheritdoc />
    public Control? Build(object? param)
    {
        if (param is not BaseViewModel viewModel)
        {
            return null;
        }

        if (_cache.TryGetValue(viewModel, out var cached))
        {
            return cached;
        }

        if (!_factories.TryGetValue(viewModel.GetType(), out var factory))
        {
            return new TextBlock { Text = "Missing view for " + viewModel.GetType().Name };
        }

        var view = factory();
        view.DataContext = viewModel;
        _cache.AddOrUpdate(viewModel, view);
        return view;
    }

    /// <inheritdoc />
    public bool Match(object? data) => data is BaseViewModel;
}
