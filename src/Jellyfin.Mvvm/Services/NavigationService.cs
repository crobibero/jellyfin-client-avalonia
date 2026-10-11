using System.Diagnostics.CodeAnalysis;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Mvvm.Services;

/// <inheritdoc />
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IApplicationService _applicationService;
    private readonly ILogger<NavigationService> _logger;
    private readonly Stack<BaseViewModel> _backStack = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="applicationService">The application service.</param>
    /// <param name="logger">The logger.</param>
    public NavigationService(IServiceProvider serviceProvider, IApplicationService applicationService, ILogger<NavigationService> logger)
    {
        _serviceProvider = serviceProvider;
        _applicationService = applicationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler? Navigated;

    /// <inheritdoc />
    public BaseViewModel? Current { get; private set; }

    /// <inheritdoc />
    public bool CanGoBack => _backStack.Count > 0;

    /// <inheritdoc />
    public Task NavigateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(object? parameter = null)
        where TViewModel : BaseViewModel
        => NavigateInternalAsync<TViewModel>(parameter, clearStack: false);

    /// <inheritdoc />
    public Task NavigateRootAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(object? parameter = null)
        where TViewModel : BaseViewModel
        => NavigateInternalAsync<TViewModel>(parameter, clearStack: true);

    /// <inheritdoc />
    public async Task<bool> GoBackAsync()
    {
        BaseViewModel? target = null;
        await _applicationService.DispatchAsync(() =>
        {
            if (_backStack.Count == 0)
            {
                return;
            }

            var previous = Current;
            target = _backStack.Pop();
            Current = target;
            previous?.OnNavigatedFrom();
            DisposeViewModel(previous);
            Navigated?.Invoke(this, EventArgs.Empty);
        }).ConfigureAwait(true);

        if (target is null)
        {
            return false;
        }

        _logger.LogDebug("Navigated back to {ViewModel}", target.GetType().Name);
        await target.OnNavigatedToAsync(null, NavigationMode.Back).ConfigureAwait(true);
        return true;
    }

    private async Task NavigateInternalAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(object? parameter, bool clearStack)
        where TViewModel : BaseViewModel
    {
        TViewModel? viewModel = null;
        await _applicationService.DispatchAsync(() =>
        {
            // The root provider keeps every disposable transient it resolves until it is disposed itself,
            // so page view models are created outside of it and disposed by DisposeViewModel.
            viewModel = ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider);
            var previous = Current;
            previous?.OnNavigatedFrom();

            if (clearStack)
            {
                while (_backStack.Count > 0)
                {
                    DisposeViewModel(_backStack.Pop());
                }

                DisposeViewModel(previous);
            }
            else if (previous is not null)
            {
                if (previous.KeepInBackStack)
                {
                    _backStack.Push(previous);
                }
                else
                {
                    DisposeViewModel(previous);
                }
            }

            Current = viewModel;
            Navigated?.Invoke(this, EventArgs.Empty);
        }).ConfigureAwait(true);

        _logger.LogDebug("Navigated to {ViewModel}", typeof(TViewModel).Name);
        await viewModel!.OnNavigatedToAsync(parameter, NavigationMode.New).ConfigureAwait(true);
    }

    private static void DisposeViewModel(BaseViewModel? viewModel)
    {
        if (viewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
