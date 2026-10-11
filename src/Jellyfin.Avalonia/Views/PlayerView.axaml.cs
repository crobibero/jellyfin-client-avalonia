using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Jellyfin.Avalonia.Controls;
using Jellyfin.Avalonia.Services;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Preferences;
using Jellyfin.Mvvm.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// The video and audio player.
/// </summary>
public partial class PlayerView : PageView
{
    private readonly IVideoViewFactory _videoViewFactory;
    private PlayerViewModel? _viewModel;
    private IMediaPlayer? _attachedPlayer;
    private PlaybackManager? _subscribedManager;
    private IDisposable? _sleepInhibition;
    private MainView? _transparentShell;
    private IBrush? _shellBackground;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerView"/> class.
    /// </summary>
    public PlayerView()
    {
        InitializeComponent();
        _videoViewFactory = App.Services.GetRequiredService<IVideoViewFactory>();

        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, (_, _) => _viewModel?.ReportActivity(), RoutingStrategies.Tunnel, handledEventsToo: true);
        InputLayer.Tapped += OnInputTapped;
        InputLayer.DoubleTapped += (_, _) => ToggleFullScreen();

        SeekSlider.AddHandler(PointerPressedEvent, (_, _) => _viewModel?.BeginSeek(), RoutingStrategies.Tunnel, handledEventsToo: true);
        SeekSlider.AddHandler(PointerReleasedEvent, (_, _) => _viewModel?.EndSeek(), RoutingStrategies.Tunnel, handledEventsToo: true);
        SeekSlider.AddHandler(PointerCaptureLostEvent, (_, _) => _viewModel?.EndSeek(), RoutingStrategies.Bubble, handledEventsToo: true);
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        UnsubscribeManager();
        _viewModel = DataContext as PlayerViewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        if (this.IsAttachedToVisualTree())
        {
            SubscribeManager();
        }
    }

    /// <inheritdoc />
    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeManager();
        UpdateTransparency();

        try
        {
            _sleepInhibition ??= TopLevel.GetTopLevel(this) is { } topLevel
                ? await topLevel.RequestPlatformInhibition(PlatformInhibitionType.AppSleep, "Playing media")
                : null;
        }
        catch (NotSupportedException)
        {
            // The platform can't keep the screen awake.
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // The manager is a singleton, a subscription left behind would keep this page alive.
        UnsubscribeManager();
        AttachPlayer(null);
        _sleepInhibition?.Dispose();
        _sleepInhibition = null;
        RestoreShellBackground();

        if (TopLevel.GetTopLevel(this) is MainWindow { WindowState: WindowState.FullScreen } window)
        {
            window.ToggleFullScreen();
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void OnManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlaybackManager.Player))
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_subscribedManager is not null)
                {
                    AttachPlayer(_subscribedManager.Player);
                }
            });
        }
    }

    private void SubscribeManager()
    {
        if (_subscribedManager is not null || _viewModel is null)
        {
            return;
        }

        _subscribedManager = _viewModel.Manager;
        _subscribedManager.PropertyChanged += OnManagerPropertyChanged;
        AttachPlayer(_subscribedManager.Player);
    }

    private void UnsubscribeManager()
    {
        if (_subscribedManager is not null)
        {
            _subscribedManager.PropertyChanged -= OnManagerPropertyChanged;
            _subscribedManager = null;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(PlayerViewModel.ZoomMode):
                ApplyZoom();
                break;
            case nameof(PlayerViewModel.IsVideo):
                UpdateTransparency();
                break;
            case nameof(PlayerViewModel.IsNextUpVisible) when _viewModel?.IsNextUpVisible == true:
                Dispatcher.UIThread.Post(() => PlayNextButton.Focus(NavigationMethod.Directional), DispatcherPriority.Background);
                break;
            case nameof(PlayerViewModel.HasPlaybackError) when _viewModel?.HasPlaybackError == true:
                Dispatcher.UIThread.Post(() => ErrorCloseButton.Focus(NavigationMethod.Directional), DispatcherPriority.Background);
                break;
            case nameof(PlayerViewModel.IsSkipSegmentVisible) when _viewModel is { IsSkipSegmentVisible: true, IsOverlayVisible: false }:
                Dispatcher.UIThread.Post(() => SkipSegmentButton.Focus(NavigationMethod.Directional), DispatcherPriority.Background);
                break;
            case nameof(PlayerViewModel.IsMenuOpen) when _viewModel?.IsMenuOpen == true:
                Dispatcher.UIThread.Post(FocusMenu, DispatcherPriority.Background);
                break;
            case nameof(PlayerViewModel.IsMenuOpen):
            case nameof(PlayerViewModel.IsOverlayVisible) when _viewModel?.IsOverlayVisible == true:
                Dispatcher.UIThread.Post(EnsureControlsFocused, DispatcherPriority.Background);
                break;
        }
    }

    private void AttachPlayer(IMediaPlayer? player)
    {
        if (ReferenceEquals(player, _attachedPlayer))
        {
            return;
        }

        _attachedPlayer = player;
        VideoHost.Content = player is null ? null : _videoViewFactory.CreateVideoView(player);
        ApplyZoom();
    }

    private void ApplyZoom()
    {
        if (VideoHost.Content is IVideoView videoView && _viewModel is not null)
        {
            videoView.VideoStretch = _viewModel.ZoomMode switch
            {
                ZoomMode.AutoCrop => Stretch.UniformToFill,
                ZoomMode.Stretch => Stretch.Fill,
                _ => Stretch.Uniform
            };
        }
    }

    /// <summary>
    /// Platforms that render video behind the canvas need a transparent page and shell.
    /// </summary>
    private void UpdateTransparency()
    {
        var transparent = _videoViewFactory.RendersBehindContent && _viewModel?.IsVideo == true && this.IsAttachedToVisualTree();
        Background = transparent ? new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)) : Brushes.Black;

        if (!transparent)
        {
            RestoreShellBackground();
            return;
        }

        if (_transparentShell is null && this.FindAncestorOfType<MainView>() is { } shell)
        {
            _transparentShell = shell;
            _shellBackground = shell.Background;
            shell.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }
    }

    private void RestoreShellBackground()
    {
        if (_transparentShell is not null)
        {
            _transparentShell.Background = _shellBackground;
            _transparentShell = null;
        }
    }

    private void FocusMenu()
    {
        var first = MenuItems.GetVisualDescendants().OfType<Button>().FirstOrDefault();
        first?.Focus(NavigationMethod.Directional);
    }

    private void EnsureControlsFocused()
    {
        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
        if (focused is null || !this.IsVisualAncestorOf(focused) || !focused.IsEffectivelyVisible)
        {
            PlayPauseButton.Focus(NavigationMethod.Directional);
        }
    }

    private void OnInputTapped(object? sender, TappedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (_viewModel.IsMenuOpen)
        {
            _viewModel.CloseMenuCommand.Execute(null);
        }
        else if (_viewModel.IsOverlayVisible && e.Pointer.Type != PointerType.Mouse)
        {
            _viewModel.IsOverlayVisible = false;
        }
        else
        {
            _viewModel.ReportActivity();
        }
    }

    private void ToggleFullScreen()
    {
        if (TopLevel.GetTopLevel(this) is MainWindow window)
        {
            window.ToggleFullScreen();
        }
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        var vm = _viewModel;
        if (vm is null)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.MediaPlayPause or Key.Pause or Key.Play:
                vm.TogglePlayPauseCommand.Execute(null);
                e.Handled = true;
                return;
            case Key.MediaNextTrack:
                vm.NextCommand.Execute(null);
                e.Handled = true;
                return;
            case Key.MediaPreviousTrack:
                vm.PreviousCommand.Execute(null);
                e.Handled = true;
                return;
            case Key.MediaStop:
                vm.CloseCommand.Execute(null);
                e.Handled = true;
                return;
            case Key.F when e.Source is not TextBox:
                ToggleFullScreen();
                e.Handled = true;
                return;
        }

        // When the controls are hidden the remote controls playback directly, like the Android TV client.
        var controlsHidden = !vm.IsOverlayVisible && !vm.IsMenuOpen && !vm.IsNextUpVisible && !vm.HasPlaybackError;
        if (controlsHidden)
        {
            switch (e.Key)
            {
                case Key.Left:
                    vm.PreviewSeek(forward: false);
                    e.Handled = true;
                    return;
                case Key.Right:
                    vm.PreviewSeek(forward: true);
                    e.Handled = true;
                    return;
                case Key.Space:
                    vm.TogglePlayPauseCommand.Execute(null);
                    e.Handled = true;
                    return;
                case Key.Enter when vm.IsSkipSegmentVisible:
                    vm.SkipSegmentCommand.Execute(null);
                    e.Handled = true;
                    return;
                case Key.Up or Key.Down or Key.Enter:
                    vm.ReportActivity();
                    e.Handled = true;
                    return;
            }
        }

        if (e.Key == Key.Space && e.Source is not Button)
        {
            vm.TogglePlayPauseCommand.Execute(null);
            e.Handled = true;
        }

        // Seek with the arrow keys while the slider is focused.
        if (e.Source == SeekSlider && e.Key is Key.Left or Key.Right)
        {
            if (e.Key == Key.Left)
            {
                vm.SkipBackCommand.Execute(null);
            }
            else
            {
                vm.SkipForwardCommand.Execute(null);
            }

            e.Handled = true;
        }

        vm.ReportActivity();
    }
}
