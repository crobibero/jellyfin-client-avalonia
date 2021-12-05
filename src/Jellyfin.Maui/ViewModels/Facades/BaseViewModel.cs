using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.ViewModels.Facades;

/// <summary>
/// Base view model.
/// </summary>
public abstract class BaseViewModel : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    protected BaseViewModel(INavigationService navigationService)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _navigationService = navigationService;

        NavigateToItemCommand = new RelayCommand(DoNavigateToItemCommand);
    }

    /// <summary>
    /// Gets the cancellation token which denotes the view model has been disposed.
    /// </summary>
    protected CancellationToken ViewModelCancellationToken => _cancellationTokenSource.Token;

    /// <summary>
    /// Gets or sets the selected BaseItemDto.
    /// </summary>
    public BaseItemDto? SelectedItem { get; set; }

    /// <summary>
    /// Gets or sets the navigate to library command.
    /// </summary>
    public IRelayCommand? NavigateToItemCommand { get; protected set; }

    /// <summary>
    /// Initialize the view model.
    /// </summary>
    /// <returns>The task.</returns>
    public abstract ValueTask InitializeAsync();

    /// <summary>
    /// Ensure Observable Collection is thread-safe.
    /// </summary>
    /// <remarks>
    ///  https://codetraveler.io/2019/09/11/using-observablecollection-in-a-multi-threaded-xamarin-forms-application/.
    /// </remarks>
    /// <param name="collection">The collection.</param>
    /// <param name="context">The context.</param>
    /// <param name="accessMethod">The access method.</param>
    /// <param name="writeAccess">Whether to enable write access.</param>
    protected static void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
    {
        Device.BeginInvokeOnMainThread(accessMethod);
    }

    /// <summary>
    /// Dispose.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose all resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }

    private void DoNavigateToItemCommand()
    {
        if (SelectedItem is null)
        {
            return;
        }

        _navigationService.NavigateToItemView(SelectedItem.Type, SelectedItem.Id);
    }
}
