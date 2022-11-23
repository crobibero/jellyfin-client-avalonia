using Jellyfin.Maui.Services;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// The loading page / pseudo splash screen. Used to initialize all necessary services asynchronously without blocking the ui thread.
/// </summary>
public partial class LoadingPage : ContentPage
{
    private bool busy;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingPage"/> class.
    /// </summary>
    public LoadingPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (busy)
        {
            return;
        }

        busy = true;

        await InternalServiceProvider.GetService<ISdkService>().InitializeAsync();

#if DEBUG
        await Task.Delay(1000); // mock slow perfs
#endif

        InternalServiceProvider.GetService<INavigationService>().NavigateToMainPage();

        busy = false;
    }
}
