using Jellyfin.Mvvm;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.Logging;
using UraniumUI.Pages;

namespace Jellyfin.Maui.Pages.Facades;

/// <summary>
/// The base content page.
/// </summary>
/// <typeparam name="TViewModel">The type of view model.</typeparam>
public abstract class BaseContentPage<TViewModel> : UraniumContentPage
    where TViewModel : BaseViewModel
{
    private readonly ILogger<BaseContentPage<TViewModel>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseContentPage{TViewModel}"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the view model.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{BaseContentPage}"/>.</param>
    protected BaseContentPage(TViewModel viewModel, ILogger<BaseContentPage<TViewModel>> logger)
    {
        _logger = logger;
        BindingContext = ViewModel = viewModel;
        Title = null;
    }

    /// <summary>
    /// Gets the view model.
    /// </summary>
    protected TViewModel ViewModel { get; }

    /// <inheritdoc />
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        try
        {
            ViewModel.State = ContentState.Loading;
            await ViewModel.InitializeAsync().ConfigureAwait(true);
            ViewModel.State = ContentState.Success;
        }
        catch (Exception ex)
        {
            ViewModel.State = ContentState.Error;
            _logger.LogError(ex, "Error initializing view model");
        }

        base.OnNavigatedTo(args);
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        if (ViewModel is IDisposable disposableViewModel)
        {
            disposableViewModel.Dispose();
        }

        base.OnDisappearing();
    }
}
