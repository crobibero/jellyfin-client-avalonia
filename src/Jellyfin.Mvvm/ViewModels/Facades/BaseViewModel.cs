using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Mvvm.ViewModels.Facades;

/// <summary>
/// Base view model.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    private object? _parameter;
    private bool _initialized;

    [ObservableProperty]
    private bool _loading = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    protected BaseViewModel(INavigationService navigationService, IApplicationService applicationService)
    {
        NavigationService = navigationService;
        ApplicationService = applicationService;
    }

    /// <summary>
    /// Gets a value indicating whether an error is shown.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Gets a value indicating whether the main toolbar is shown for this view model.
    /// </summary>
    public virtual bool ShowToolbar => true;

    /// <summary>
    /// Gets the toolbar item highlighted for this view model.
    /// </summary>
    public virtual ToolbarItem ActiveToolbarItem => ToolbarItem.None;

    /// <summary>
    /// Gets a value indicating whether the page shows an item backdrop. Other pages clear it.
    /// </summary>
    public virtual bool UsesBackdrop => false;

    /// <summary>
    /// Gets a value indicating whether the view model is kept in the back stack when navigating away.
    /// </summary>
    public virtual bool KeepInBackStack => true;

    /// <summary>
    /// Gets the navigation service.
    /// </summary>
    protected INavigationService NavigationService { get; }

    /// <summary>
    /// Gets the application service.
    /// </summary>
    protected IApplicationService ApplicationService { get; }

    /// <summary>
    /// Called when the view model is navigated to.
    /// </summary>
    /// <param name="parameter">The navigation parameter.</param>
    /// <param name="mode">The navigation mode.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnNavigatedToAsync(object? parameter, NavigationMode mode)
    {
        if (!_initialized)
        {
            _initialized = true;
            _parameter = parameter;
            await RunInitializeAsync().ConfigureAwait(true);
        }
        else
        {
            try
            {
                await OnReturnedAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }

    /// <summary>
    /// Called when the view model is navigated away from.
    /// </summary>
    public virtual void OnNavigatedFrom()
    {
    }

    /// <summary>
    /// Initialize the view model.
    /// </summary>
    /// <param name="parameter">The navigation parameter.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    protected abstract ValueTask InitializeAsync(object? parameter);

    /// <summary>
    /// Called when the user navigates back to this view model.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    protected virtual ValueTask OnReturnedAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Handle an error during initialization.
    /// </summary>
    /// <param name="exception">The exception.</param>
    protected virtual void OnError(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ErrorMessage = exception.Message;
    }

    /// <summary>
    /// Run a callback on the UI thread.
    /// </summary>
    /// <param name="action">The callback.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected Task OnUiThreadAsync(Action action) => ApplicationService.DispatchAsync(action);

    [RelayCommand]
    private async Task RetryAsync() => await RunInitializeAsync().ConfigureAwait(true);

    [RelayCommand]
    private async Task GoBackAsync() => await NavigationService.GoBackAsync().ConfigureAwait(true);

    [RelayCommand]
    private async Task NavigateToItemAsync(object? target)
    {
        switch (target)
        {
            case ItemCardModel { Kind: CardKind.Chapter, Item: not null } chapter:
                await NavigationService.NavigateAsync<PlayerViewModel>(new PlaybackRequest([chapter.Item], 0, chapter.StartPositionTicks)).ConfigureAwait(true);
                break;
            case ItemCardModel { Kind: CardKind.Person } person:
                await NavigationService.NavigateAsync<ItemDetailsViewModel>(person.Id).ConfigureAwait(true);
                break;
            case ItemCardModel { Item: not null } card:
                await NavigateToItemAsync(card.Item).ConfigureAwait(true);
                break;
            case BaseItemDto item:
                if (item.Type is BaseItemDto_Type.CollectionFolder or BaseItemDto_Type.UserView or BaseItemDto_Type.Folder or BaseItemDto_Type.Genre or BaseItemDto_Type.MusicGenre)
                {
                    await NavigationService.NavigateAsync<LibraryViewModel>(item).ConfigureAwait(true);
                }
                else if (item.Type is BaseItemDto_Type.Audio && item.Id is not null)
                {
                    await NavigationService.NavigateAsync<PlayerViewModel>(new PlaybackRequest([item], 0, null)).ConfigureAwait(true);
                }
                else if (item.Id is not null)
                {
                    await NavigationService.NavigateAsync<ItemDetailsViewModel>(item.Id.Value).ConfigureAwait(true);
                }

                break;
            case Guid id:
                await NavigationService.NavigateAsync<ItemDetailsViewModel>(id).ConfigureAwait(true);
                break;
        }
    }

    private async Task RunInitializeAsync()
    {
        Loading = true;
        ErrorMessage = null;
        try
        {
            await InitializeAsync(_parameter).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            OnError(ex);
        }
        finally
        {
            Loading = false;
        }
    }
}
