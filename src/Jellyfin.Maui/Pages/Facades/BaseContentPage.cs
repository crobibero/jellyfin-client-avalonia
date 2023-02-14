using Jellyfin.Mvvm.ViewModels.Facades;
using UraniumUI.Pages;

namespace Jellyfin.Maui.Pages.Facades;

/// <summary>
/// The base content page.
/// </summary>
/// <typeparam name="TViewModel">The type of view model.</typeparam>
public abstract class BaseContentPage<TViewModel> : UraniumContentPage
    where TViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseContentPage{TViewModel}"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the view model.</param>
    protected BaseContentPage(TViewModel viewModel)
    {
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
        ViewModel.Loading = true;
        await ViewModel.InitializeAsync().ConfigureAwait(true);
        ViewModel.Loading = false;

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
