using AsyncAwaitBestPractices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Jellyfin.Avalonia.Services;
using Jellyfin.Mvvm.ViewModels;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// The application shell.
/// </summary>
public partial class MainView : UserControl
{
    private TopLevel? _topLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();
        ToastHost.ItemsSource = App.Services.GetRequiredService<NotificationService>().Toasts;

        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        SizeChanged += (_, e) => UpdateScale(e.NewSize);
        AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel);
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    internal static double GetScale(Size size)
        => size.Width <= 0 || size.Height <= 0
            ? 1
            : Math.Clamp(Math.Min(size.Width / 1280, size.Height / 720), 0.55, 1.0);

    private void UpdateScale(Size size)
    {
        var scale = GetScale(size);
        ScaleHost.LayoutTransform = Math.Abs(scale - 1) < 0.001 ? null : new ScaleTransform(scale, scale);

        // Narrow layouts (phones in portrait) drop the details header above rows.
        Classes.Set("compact", size.Width / scale < 900);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.AddHandler(TopLevel.BackRequestedEvent, OnBackRequested);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(TopLevel.BackRequestedEvent, OnBackRequested);
        _topLevel = null;
        base.OnDetachedFromVisualTree(e);
    }

    private bool CanHandleBack()
        => ViewModel is { } vm && (vm.CanGoBack || vm.CurrentPage is IBackHandler);

    private void HandleBack()
    {
        ViewModel?.HandleBackAsync().SafeFireAndForget();
    }

    private void OnBackRequested(object? sender, RoutedEventArgs e)
    {
        // Leave the back request unhandled at the root so the platform can close the app.
        if (CanHandleBack())
        {
            e.Handled = true;
            HandleBack();
        }
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        // Keys in an open drop-down or flyout route through its owner, let the popup close itself.
        if (e.Source.IsInPopup())
        {
            return;
        }

        var isTextInput = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is TextBox;
        var isBack = e.Key is Key.Escape or Key.BrowserBack
            || (e.Key == Key.Back && !isTextInput);

        if (isBack && CanHandleBack())
        {
            e.Handled = true;
            HandleBack();
        }
    }

    private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsXButton1Pressed && CanHandleBack())
        {
            e.Handled = true;
            HandleBack();
        }
    }
}
