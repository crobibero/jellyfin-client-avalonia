using System.Globalization;
using System.Runtime.Versioning;
using Avalonia.Threading;
using Jellyfin.Mvvm.Playback;

namespace Jellyfin.Avalonia.Browser;

/// <summary>
/// Plays media in the page's video element. The element state is polled, keeping the interop simple.
/// </summary>
[SupportedOSPlatform("browser")]
internal sealed class BrowserMediaPlayer : IMediaPlayer
{
    private readonly DispatcherTimer _timer;
    private PlayerState _state;
    private TimeSpan _position;
    private TimeSpan _duration;
    private TimeSpan? _knownDuration;
    private string _lastError = string.Empty;
    private int _volume = 100;
    private bool _muted;
    private float _speed = 1;
    private bool _loading;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserMediaPlayer"/> class.
    /// </summary>
    public BrowserMediaPlayer()
    {
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, (_, _) => Poll());
    }

    /// <inheritdoc />
    public event EventHandler? StateChanged;

    /// <inheritdoc />
    public event EventHandler? PositionChanged;

    /// <inheritdoc />
    public event EventHandler<string>? ErrorOccurred;

    /// <inheritdoc />
    public PlayerState State => _state;

    /// <inheritdoc />
    public TimeSpan Position => _position;

    /// <inheritdoc />
    public TimeSpan Duration => _duration > TimeSpan.Zero ? _duration : _knownDuration ?? TimeSpan.Zero;

    /// <inheritdoc />
    public bool CanSeek => true;

    /// <inheritdoc />
    public int Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            BrowserInterop.SetVolume(value);
        }
    }

    /// <inheritdoc />
    public bool IsMuted
    {
        get => _muted;
        set
        {
            _muted = value;
            BrowserInterop.SetMuted(value);
        }
    }

    /// <inheritdoc />
    public float Speed
    {
        get => _speed;
        set
        {
            _speed = value;
            BrowserInterop.SetRate(value);
        }
    }

    /// <inheritdoc />
    public async ValueTask LoadAsync(MediaLoadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _knownDuration = request.Duration;
        _lastError = string.Empty;
        _loading = true;
        SetState(PlayerState.Opening);
        _timer.Start();

        try
        {
            await BrowserInterop.Load(request.Url.AbsoluteUri, request.IsHls, request.StartPosition.TotalSeconds).ConfigureAwait(true);
        }
        finally
        {
            _loading = false;
        }

        if (request.ExternalSubtitle is { } subtitle)
        {
            LoadExternalSubtitle(subtitle);
        }
        else if (request.SubtitleTrackPosition is { } position)
        {
            BrowserInterop.SelectTextTrack(position);
        }
        else
        {
            BrowserInterop.SelectTextTrack(-1);
        }
    }

    /// <inheritdoc />
    public void Play() => BrowserInterop.Play();

    /// <inheritdoc />
    public void Pause() => BrowserInterop.Pause();

    /// <inheritdoc />
    public void Stop()
    {
        _timer.Stop();
        BrowserInterop.Stop();
        SetState(PlayerState.Idle);
    }

    /// <inheritdoc />
    public void Seek(TimeSpan position) => BrowserInterop.Seek(position.TotalSeconds);

    /// <inheritdoc />
    public void SelectAudioTrack(int position) => BrowserInterop.SelectAudioTrack(position);

    /// <inheritdoc />
    public void SelectSubtitleTrack(int? position) => BrowserInterop.SelectTextTrack(position ?? -1);

    /// <inheritdoc />
    public void LoadExternalSubtitle(ExternalSubtitle subtitle)
    {
        ArgumentNullException.ThrowIfNull(subtitle);
        BrowserInterop.AddTextTrack(subtitle.Url.AbsoluteUri, subtitle.Language);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _timer.Stop();
        BrowserInterop.Stop();
    }

    private void Poll()
    {
        var parts = BrowserInterop.GetState().Split('|', 6);
        if (parts.Length < 6)
        {
            return;
        }

        _position = TimeSpan.FromSeconds(double.Parse(parts[0], CultureInfo.InvariantCulture));
        _duration = TimeSpan.FromSeconds(double.Parse(parts[1], CultureInfo.InvariantCulture));
        var paused = parts[2] == "1";
        var ended = parts[3] == "1";
        var buffering = parts[4] == "1";
        var error = parts[5];

        PositionChanged?.Invoke(this, EventArgs.Empty);

        if (!string.IsNullOrEmpty(error) && !string.Equals(error, _lastError, StringComparison.Ordinal))
        {
            _lastError = error;
            SetState(PlayerState.Error);
            ErrorOccurred?.Invoke(this, error);
            return;
        }

        if (_loading)
        {
            return;
        }

        var state = ended ? PlayerState.Ended
            : buffering ? PlayerState.Buffering
            : paused ? PlayerState.Paused
            : PlayerState.Playing;
        SetState(state);
    }

    private void SetState(PlayerState state)
    {
        if (_state == state)
        {
            return;
        }

        _state = state;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
