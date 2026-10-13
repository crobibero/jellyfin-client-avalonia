using System.Globalization;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Jellyfin.Mvvm.Playback;

/// <summary>
/// Applies the commands remote controls, such as the web client, send to this session.
/// </summary>
public sealed class RemoteControlService : IDisposable
{
    // Matches volumeUp in jellyfin-web's htmlAudioPlayer and htmlVideoPlayer plugins.
    private const int VolumeStep = 2;

    private static readonly (GeneralCommand_Name Name, GeneralCommandType Type)[] _commands =
    [
        (GeneralCommand_Name.SetAudioStreamIndex, GeneralCommandType.SetAudioStreamIndex),
        (GeneralCommand_Name.SetSubtitleStreamIndex, GeneralCommandType.SetSubtitleStreamIndex),
        (GeneralCommand_Name.SetMaxStreamingBitrate, GeneralCommandType.SetMaxStreamingBitrate),
        (GeneralCommand_Name.SetVolume, GeneralCommandType.SetVolume),
        (GeneralCommand_Name.VolumeUp, GeneralCommandType.VolumeUp),
        (GeneralCommand_Name.VolumeDown, GeneralCommandType.VolumeDown),
        (GeneralCommand_Name.Mute, GeneralCommandType.Mute),
        (GeneralCommand_Name.Unmute, GeneralCommandType.Unmute),
        (GeneralCommand_Name.ToggleMute, GeneralCommandType.ToggleMute),
        (GeneralCommand_Name.DisplayMessage, GeneralCommandType.DisplayMessage),
    ];

    private readonly IServerEventService _serverEventService;
    private readonly PlaybackManager _playbackManager;
    private readonly ILibraryService _libraryService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RemoteControlService> _logger;
    private bool _started;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteControlService"/> class.
    /// </summary>
    /// <param name="serverEventService">The server event service.</param>
    /// <param name="playbackManager">The playback manager.</param>
    /// <param name="libraryService">The library service.</param>
    /// <param name="navigationService">The navigation service.</param>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="logger">The logger.</param>
    public RemoteControlService(
        IServerEventService serverEventService,
        PlaybackManager playbackManager,
        ILibraryService libraryService,
        INavigationService navigationService,
        INotificationService notificationService,
        ILogger<RemoteControlService> logger)
    {
        _serverEventService = serverEventService;
        _playbackManager = playbackManager;
        _libraryService = libraryService;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the general commands this client handles, reported to the server with the client capabilities.
    /// </summary>
    public static IReadOnlyList<GeneralCommandType> SupportedCommands { get; } = [.. _commands.Select(c => c.Type)];

    /// <summary>
    /// Start handling remote commands.
    /// </summary>
    public void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;
        _serverEventService.PlayRequested += OnPlayRequested;
        _serverEventService.PlaystateRequested += OnPlaystateRequested;
        _serverEventService.GeneralCommandReceived += OnGeneralCommandReceived;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _serverEventService.PlayRequested -= OnPlayRequested;
        _serverEventService.PlaystateRequested -= OnPlaystateRequested;
        _serverEventService.GeneralCommandReceived -= OnGeneralCommandReceived;
    }

    internal static string? GetArgument(GeneralCommand command, string name)
    {
        if (command.Arguments?.AdditionalData is not { } arguments
            || !arguments.TryGetValue(name, out var value))
        {
            return null;
        }

        return value switch
        {
            null => null,
            string text => text,
            UntypedString text => text.GetValue(),

            // Kiota parses ISO 8601 strings into a DateTime before they get here, so the text is rebuilt in that form.
            DateTime { Kind: DateTimeKind.Unspecified, TimeOfDay.Ticks: 0 } date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime date => date.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture),
            DateTimeOffset date => date.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    internal static int? GetIntArgument(GeneralCommand command, string name)
        => int.TryParse(GetArgument(command, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;

    // StartIndex counts the requested ids. A start item the server did not return starts at the next one that it did.
    internal static int GetStartIndex(IReadOnlyList<Guid?> requestedIds, int? startIndex, IReadOnlyList<BaseItemDto> items)
    {
        var returned = items.Where(i => i.Id is not null).Select(i => i.Id!.Value).ToHashSet();
        var index = requestedIds
            .Take(Math.Max(startIndex ?? 0, 0))
            .Count(id => id is { } value && returned.Contains(value));
        return Math.Clamp(index, 0, Math.Max(items.Count - 1, 0));
    }

    private void OnPlayRequested(object? sender, PlayRequest request)
        => PlayAsync(request).SafeFireAndForget(ex => _logger.LogWarning(ex, "Unable to handle remote play request"));

    private void OnPlaystateRequested(object? sender, PlaystateRequest request)
        => HandlePlaystateAsync(request).SafeFireAndForget(ex => _logger.LogWarning(ex, "Unable to handle remote playstate command"));

    private void OnGeneralCommandReceived(object? sender, GeneralCommand command)
        => HandleGeneralCommandAsync(command).SafeFireAndForget(ex => _logger.LogWarning(ex, "Unable to handle remote command"));

    private async Task PlayAsync(PlayRequest request)
    {
        var ids = request.ItemIds?.OfType<Guid>().ToList() ?? [];
        if (ids.Count == 0)
        {
            return;
        }

        if (request.PlayCommand == PlayRequest_PlayCommand.PlayInstantMix)
        {
            _logger.LogInformation("Remote instant mix is not supported");
            return;
        }

        var items = await GetItemsAsync(ids).ConfigureAwait(true);
        if (items.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Remote {Command} of {Count} items", request.PlayCommand, items.Count);
        if (request.PlayCommand is PlayRequest_PlayCommand.PlayNext or PlayRequest_PlayCommand.PlayLast
            && _playbackManager.CurrentItem is not null)
        {
            await _playbackManager.EnqueueAsync(items, request.PlayCommand == PlayRequest_PlayCommand.PlayNext).ConfigureAwait(true);
            return;
        }

        var playbackRequest = new PlaybackRequest(
            items,
            GetStartIndex(request.ItemIds ?? [], request.StartIndex, items),
            request.StartPositionTicks,
            Shuffle: request.PlayCommand == PlayRequest_PlayCommand.PlayShuffle)
        {
            MediaSourceId = request.MediaSourceId,
            AudioStreamIndex = request.AudioStreamIndex,
            SubtitleStreamIndex = request.SubtitleStreamIndex,
        };

        await _navigationService.NavigateAsync<PlayerViewModel>(playbackRequest).ConfigureAwait(true);
    }

    /// <summary>
    /// Get the items in the order they were requested.
    /// </summary>
    private async Task<IReadOnlyList<BaseItemDto>> GetItemsAsync(IReadOnlyList<Guid> ids)
    {
        var result = await _libraryService.GetItemsAsync(new ItemQuery { Ids = ids }).ConfigureAwait(true);
        var items = (result.Items ?? []).Where(i => i.Id is not null).ToDictionary(i => i.Id!.Value);
        return [.. ids.Where(items.ContainsKey).Select(id => items[id])];
    }

    private async Task HandlePlaystateAsync(PlaystateRequest request)
    {
        if (_playbackManager.CurrentItem is null)
        {
            return;
        }

        var player = _playbackManager.Player;
        switch (request.Command)
        {
            case PlaystateRequest_Command.Stop:
                await _playbackManager.StopAsync().ConfigureAwait(true);
                break;
            case PlaystateRequest_Command.Pause:
                player?.Pause();
                break;
            case PlaystateRequest_Command.Unpause:
                player?.Play();
                break;
            case PlaystateRequest_Command.PlayPause:
                _playbackManager.TogglePause();
                break;
            case PlaystateRequest_Command.NextTrack:
                await _playbackManager.NextAsync().ConfigureAwait(true);
                break;
            case PlaystateRequest_Command.PreviousTrack:
                await _playbackManager.PreviousAsync().ConfigureAwait(true);
                break;
            case PlaystateRequest_Command.Seek when request.SeekPositionTicks is { } ticks:
                _playbackManager.Seek(TimeSpan.FromTicks(ticks));
                break;
            case PlaystateRequest_Command.Rewind:
                _playbackManager.SkipBack();
                break;
            case PlaystateRequest_Command.FastForward:
                _playbackManager.SkipForward();
                break;
        }
    }

    private async Task HandleGeneralCommandAsync(GeneralCommand command)
    {
        var player = _playbackManager.Player;
        switch (command.Name)
        {
            case GeneralCommand_Name.DisplayMessage:
                ShowMessage(GetArgument(command, "Header"), GetArgument(command, "Text"));
                break;
            case GeneralCommand_Name.SetAudioStreamIndex when GetIntArgument(command, "Index") is { } index:
                await _playbackManager.SelectAudioStreamAsync(index).ConfigureAwait(true);
                break;
            case GeneralCommand_Name.SetSubtitleStreamIndex when GetIntArgument(command, "Index") is { } index:
                await _playbackManager.SelectSubtitleStreamAsync(index < 0 ? null : index).ConfigureAwait(true);
                break;
            case GeneralCommand_Name.SetMaxStreamingBitrate:
                await _playbackManager.SetMaxBitrateAsync(GetIntArgument(command, "MaxBitrate") is { } bitrate and > 0 ? bitrate : null).ConfigureAwait(true);
                break;
            case GeneralCommand_Name.SetVolume when GetIntArgument(command, "Volume") is { } volume:
                _playbackManager.SetVolume(volume);
                break;
            case GeneralCommand_Name.VolumeUp when player is not null:
                _playbackManager.SetVolume(player.Volume + VolumeStep);
                break;
            case GeneralCommand_Name.VolumeDown when player is not null:
                _playbackManager.SetVolume(player.Volume - VolumeStep);
                break;
            case GeneralCommand_Name.Mute:
                _playbackManager.SetMuted(true);
                break;
            case GeneralCommand_Name.Unmute:
                _playbackManager.SetMuted(false);
                break;
            case GeneralCommand_Name.ToggleMute when player is not null:
                _playbackManager.SetMuted(!player.IsMuted);
                break;
            default:
                _logger.LogDebug("Ignoring remote command {Command}", command.Name);
                break;
        }
    }

    private void ShowMessage(string? header, string? text)
    {
        var message = (string.IsNullOrEmpty(header), string.IsNullOrEmpty(text)) switch
        {
            (true, true) => null,
            (true, false) => text,
            (false, true) => header,
            _ => header + ": " + text
        };

        if (message is not null)
        {
            _notificationService.Show(message);
        }
    }
}
