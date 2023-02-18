using CommunityToolkit.Maui.Core.Primitives;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Video playback page.
/// </summary>
public partial class VideoPlaybackPage : BaseContentIdPage<VideoPlaybackViewModel>
{
    private readonly ILogger<VideoPlaybackPage> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoPlaybackPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="VideoPlaybackViewModel"/>.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{VideoPlaybackPage}"/> interface.</param>
    public VideoPlaybackPage(VideoPlaybackViewModel viewModel, ILogger<VideoPlaybackPage> logger)
        : base(viewModel)
    {
        _logger = logger;

        InitializeComponent();
    }

    private void OnPlayPause(object? sender, EventArgs e)
    {
        _logger.LogDebug("Start {FunctionName} {CurrentState}", nameof(OnPlayPause), MediaElement.CurrentState);

        try
        {
            switch (MediaElement.CurrentState)
            {
                case MediaElementState.Stopped or MediaElementState.Paused:
                    MediaElement.Play();
                    break;
                case MediaElementState.Playing:
                    MediaElement.Pause();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing play state");
        }

        _logger.LogDebug("End {FunctionName} {CurrentState}", nameof(OnPlayPause), MediaElement.CurrentState);
    }

    private void PageUnloaded(object sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }

        MediaElement.Handler?.DisconnectHandler();
    }
}
