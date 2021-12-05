using AsyncAwaitBestPractices;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.Pages.Facades;

/// <summary>
/// The base content page.
/// </summary>
/// <typeparam name="TViewModel">The type of view model.</typeparam>
public abstract class BaseContentPage<TViewModel> : ContentPage
    where TViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseContentPage{TViewModel}"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the view model.</param>
    protected BaseContentPage(TViewModel viewModel)
        : this(viewModel, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseContentPage{TViewModel}"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the view model.</param>
    /// <param name="pageTitle">The page title.</param>
    protected BaseContentPage(TViewModel viewModel, string pageTitle)
    {
        BindingContext = ViewModel = viewModel;
        Title = pageTitle;

#pragma warning disable CA2214 // Do not call overridable methods in constructors
        InitializeLayout();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
    }

    /// <summary>
    /// Gets the view model.
    /// </summary>
    protected TViewModel ViewModel { get; }

    /// <inheritdoc />
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        ViewModel.InitializeAsync().SafeFireAndForget();
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

    /// <summary>
    /// Called during construction to initialize the layout.
    /// </summary>
    protected abstract void InitializeLayout();
}
