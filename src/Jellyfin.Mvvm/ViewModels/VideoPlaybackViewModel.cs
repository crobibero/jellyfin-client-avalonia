using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// Video playback view model.
/// </summary>
public partial class VideoPlaybackViewModel : BaseItemViewModel
{
    [ObservableProperty]
    private string? _playbackUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoPlaybackViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    public VideoPlaybackViewModel(INavigationService navigationService, IApplicationService applicationService)
        : base(navigationService, applicationService)
    {
    }

    /// <inheritdoc/>
    public override async ValueTask InitializeAsync()
    {
        await Task.Delay(2_000).ConfigureAwait(true);
        PlaybackUrl = @"https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
    }
}
