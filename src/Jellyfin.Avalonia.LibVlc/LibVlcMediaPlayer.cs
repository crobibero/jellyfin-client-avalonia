using System.Globalization;
using System.Runtime.InteropServices;
using Jellyfin.Mvvm.Playback;
using LibVLCSharp.Shared;
using Microsoft.Extensions.Logging;
using VlcMediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace Jellyfin.Avalonia.LibVlc;

#pragma warning disable CA2213 // The player and media are disposed on a background thread in Dispose.

/// <summary>
/// A <see cref="IMediaPlayer"/> backed by libVLC, rendering video into a <see cref="VideoFrameBuffer"/>.
/// </summary>
public sealed class LibVlcMediaPlayer : IMediaPlayer
{
    private const uint MaxWidth = 3840;
    private const uint MaxHeight = 2160;

    private readonly LibVLC _libVlc;
    private readonly VlcMediaPlayer _player;
    private readonly ILogger _logger;

    // Keep delegates alive while libVLC holds the function pointers.
    private readonly VlcMediaPlayer.LibVLCVideoFormatCb _formatCallback;
    private readonly VlcMediaPlayer.LibVLCVideoCleanupCb _cleanupCallback;
    private readonly VlcMediaPlayer.LibVLCVideoLockCb _lockCallback;
    private readonly VlcMediaPlayer.LibVLCVideoDisplayCb _displayCallback;

    // Calls that can block run in order on the thread pool, so none can reach a released player.
    private readonly Lock _queueLock = new();
    private Task _queueTail = Task.CompletedTask;

    private Media? _media;
    private MediaLoadRequest? _request;
    private bool _pendingTrackSelection;
    private PlayerState _state;
    private long _lengthMs;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibVlcMediaPlayer"/> class.
    /// </summary>
    /// <param name="libVlc">The libVLC instance.</param>
    /// <param name="logger">The logger.</param>
    public LibVlcMediaPlayer(LibVLC libVlc, ILogger logger)
    {
        _libVlc = libVlc;
        _logger = logger;
        _player = new VlcMediaPlayer(libVlc)
        {
            EnableHardwareDecoding = true,
            EnableKeyInput = false,
            EnableMouseInput = false,
        };

        _formatCallback = OnVideoFormat;
        _cleanupCallback = OnVideoCleanup;
        _lockCallback = OnVideoLock;
        _displayCallback = OnVideoDisplay;
        _player.SetVideoFormatCallbacks(_formatCallback, _cleanupCallback);
        _player.SetVideoCallbacks(_lockCallback, null, _displayCallback);

        _player.Opening += (_, _) => SetState(PlayerState.Opening);
        _player.Buffering += OnBuffering;
        _player.Playing += OnPlaying;
        _player.Paused += (_, _) => SetState(PlayerState.Paused);
        _player.EndReached += (_, _) => SetState(PlayerState.Ended);
        _player.EncounteredError += OnError;
        _player.TimeChanged += (_, _) => PositionChanged?.Invoke(this, EventArgs.Empty);
        _player.LengthChanged += (_, e) =>
        {
            _lengthMs = e.Length;
            PositionChanged?.Invoke(this, EventArgs.Empty);
        };
        _player.ESAdded += OnElementaryStreamAdded;
    }

    /// <inheritdoc />
    public event EventHandler? StateChanged;

    /// <inheritdoc />
    public event EventHandler? PositionChanged;

    /// <inheritdoc />
    public event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// Gets the frame buffer video is rendered into.
    /// </summary>
    public VideoFrameBuffer Frames { get; } = new();

    /// <inheritdoc />
    public PlayerState State => _state;

    /// <inheritdoc />
    public TimeSpan Position => _disposed ? TimeSpan.Zero : TimeSpan.FromMilliseconds(Math.Max(0, _player.Time));

    /// <inheritdoc />
    public TimeSpan Duration
    {
        get
        {
            var length = _lengthMs > 0 ? _lengthMs : _player.Length;
            if (length > 0)
            {
                return TimeSpan.FromMilliseconds(length);
            }

            return _request?.Duration ?? TimeSpan.Zero;
        }
    }

    /// <inheritdoc />
    public bool CanSeek => _player.IsSeekable;

    /// <inheritdoc />
    public int Volume
    {
        get => _player.Volume;
        set => _player.Volume = Math.Clamp(value, 0, 100);
    }

    /// <inheritdoc />
    public bool IsMuted
    {
        get => _player.Mute;
        set => _player.Mute = value;
    }

    /// <inheritdoc />
    public float Speed
    {
        get => _player.Rate;
        set => _player.SetRate(value);
    }

    /// <inheritdoc />
    public ValueTask LoadAsync(MediaLoadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ObjectDisposedException.ThrowIf(_disposed, this);

        _request = request;
        _lengthMs = 0;
        _pendingTrackSelection = true;

        var media = new Media(_libVlc, request.Url);
        media.AddOption(":http-user-agent=" + AppConstants.Identifier + "/" + AppConstants.Version);
        media.AddOption(":http-reconnect");

        if (request.StartPosition > TimeSpan.Zero)
        {
            media.AddOption(":start-time=" + request.StartPosition.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        }

        if (request.AudioTrackPosition is { } audio)
        {
            media.AddOption(":audio-track=" + audio.ToString(CultureInfo.InvariantCulture));
        }

        if (request.SubtitleTrackPosition is { } subtitle)
        {
            media.AddOption(":sub-track=" + subtitle.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            // Subtitles are selected by the server's stream selection, never by libVLC.
            media.AddOption(":no-sub-autodetect-file");
        }

        var previous = _media;
        _media = media;

        // libVLC blocks while switching media, keep the UI thread free.
        return new ValueTask(Enqueue(() =>
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                _player.Play(media);
            }

            previous?.Dispose();
        }));
    }

    /// <inheritdoc />
    public void Play()
    {
        if (_disposed)
        {
            return;
        }

        if (_state == PlayerState.Ended && _media is { } media)
        {
            Post(() => _player.Play(media), "Unable to restart playback");
            return;
        }

        _player.SetPause(false);
    }

    /// <inheritdoc />
    public void Pause()
    {
        if (!_disposed)
        {
            _player.SetPause(true);
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        Post(_player.Stop, "Unable to stop playback");
        SetState(PlayerState.Idle);
    }

    /// <inheritdoc />
    public void Seek(TimeSpan position)
    {
        if (_disposed)
        {
            return;
        }

        if (_state == PlayerState.Ended)
        {
            Play();
        }

        _player.Time = (long)position.TotalMilliseconds;
    }

    /// <inheritdoc />
    public void SelectAudioTrack(int position)
    {
        if (_disposed)
        {
            return;
        }

        var tracks = _player.AudioTrackDescription.Where(t => t.Id >= 0).ToList();
        if (position >= 0 && position < tracks.Count)
        {
            _player.SetAudioTrack(tracks[position].Id);
        }
    }

    /// <inheritdoc />
    public void SelectSubtitleTrack(int? position)
    {
        if (_disposed)
        {
            return;
        }

        if (position is null)
        {
            _player.SetSpu(-1);
            return;
        }

        var tracks = _player.SpuDescription.Where(t => t.Id >= 0).ToList();
        if (position >= 0 && position < tracks.Count)
        {
            _player.SetSpu(tracks[position.Value].Id);
        }
    }

    /// <inheritdoc />
    public void LoadExternalSubtitle(ExternalSubtitle subtitle)
    {
        ArgumentNullException.ThrowIfNull(subtitle);
        if (_disposed)
        {
            return;
        }

        _player.AddSlave(MediaSlaveType.Subtitle, subtitle.Url.AbsoluteUri, true);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        var media = _media;
        _media = null;

        // Stopping can block for a moment, never do it on the UI thread.
        Post(
            () =>
            {
                _player.Stop();
                _player.Dispose();
                media?.Dispose();
                Frames.Dispose();
                NativeHeap.Trim();
            },
            "Error disposing player",
            runAfterDispose: true);
    }

    private Task Enqueue(Action action, bool runAfterDispose = false)
    {
        lock (_queueLock)
        {
            _queueTail = _queueTail.ContinueWith(
                _ =>
                {
                    if (!_disposed || runAfterDispose)
                    {
                        action();
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);
            return _queueTail;
        }
    }

    private void Post(Action action, string errorMessage, bool runAfterDispose = false)
        => Enqueue(
            () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "{Message}", errorMessage);
                }
            },
            runAfterDispose);

    private void SetState(PlayerState state)
    {
        if (_state == state)
        {
            return;
        }

        _state = state;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnBuffering(object? sender, MediaPlayerBufferingEventArgs e)
    {
        if (e.Cache < 100)
        {
            if (_state is PlayerState.Opening or PlayerState.Playing)
            {
                SetState(PlayerState.Buffering);
            }
        }
        else if (_state == PlayerState.Buffering)
        {
            SetState(_player.IsPlaying ? PlayerState.Playing : PlayerState.Paused);
        }
    }

    private void OnPlaying(object? sender, EventArgs e)
    {
        SetState(PlayerState.Playing);
        ApplyPendingTrackSelection();
    }

    private void OnElementaryStreamAdded(object? sender, MediaPlayerESAddedEventArgs e)
    {
        if (e.Type == TrackType.Text && _pendingTrackSelection)
        {
            Post(ApplyPendingTrackSelection, "Unable to apply track selection");
        }
    }

    private void ApplyPendingTrackSelection()
    {
        var request = _request;
        if (request is null || !_pendingTrackSelection)
        {
            return;
        }

        _pendingTrackSelection = false;
        Post(
            () =>
            {
                if (request.ExternalSubtitle is { } external)
                {
                    LoadExternalSubtitle(external);
                }
                else if (request.SubtitleTrackPosition is null)
                {
                    _player.SetSpu(-1);
                }
            },
            "Unable to apply track selection");
    }

    private void OnError(object? sender, EventArgs e)
    {
        SetState(PlayerState.Error);
        ErrorOccurred?.Invoke(this, "libVLC was unable to play the stream");
    }

    private uint OnVideoFormat(ref IntPtr opaque, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines)
    {
        // Downscale very large video to limit memory bandwidth, keeping the aspect ratio.
        if (width > MaxWidth || height > MaxHeight)
        {
            var scale = Math.Min((double)MaxWidth / width, (double)MaxHeight / height);
            width = (uint)(width * scale) & ~1u;
            height = (uint)(height * scale) & ~1u;
        }

        // RV32 is BGRA in memory on little endian systems.
        Marshal.Copy("RV32"u8.ToArray(), 0, chroma, 4);
        Frames.Configure((int)width, (int)height);
        pitches = width * 4;
        lines = height;
        return 1;
    }

    private void OnVideoCleanup(ref IntPtr opaque)
    {
    }

    private IntPtr OnVideoLock(IntPtr opaque, IntPtr planes)
    {
        Marshal.WriteIntPtr(planes, Frames.GetBackBuffer());
        return IntPtr.Zero;
    }

    private void OnVideoDisplay(IntPtr opaque, IntPtr picture) => Frames.Present();
}
