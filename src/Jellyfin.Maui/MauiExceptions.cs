namespace Jellyfin.Maui;

/// <summary>
/// Global Maui exception handler.
/// </summary>
public static class MauiExceptions
{
#if WINDOWS
    private static Exception? _lastFirstChanceException;
#endif

    static MauiExceptions()
    {
        // This is the normal event expected, and should still be used.
        // It will fire for exceptions from iOS and Mac Catalyst,
        // and for exceptions on background threads from WinUI 3.

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            UnhandledException?.Invoke(sender, args);
        };

#if IOS || MACCATALYST

        // For iOS and Mac Catalyst
        // Exceptions will flow through AppDomain.CurrentDomain.UnhandledException,
        // but we need to set UnwindNativeCode to get it to work correctly.
        // See: https://github.com/xamarin/xamarin-macios/issues/15252
        ObjCRuntime.Runtime.MarshalManagedException += (_, args) =>
        {
            args.ExceptionMode = ObjCRuntime.MarshalManagedExceptionMode.UnwindNativeCode;
        };

#elif ANDROID

        // For Android:
        // All exceptions will flow through Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser,
        // and NOT through AppDomain.CurrentDomain.UnhandledException
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            UnhandledException?.Invoke(sender, new UnhandledExceptionEventArgs(args.Exception, true));
        };

#elif WINDOWS

        // For WinUI 3:
        //
        // * Exceptions on background threads are caught by AppDomain.CurrentDomain.UnhandledException,
        //   not by Microsoft.UI.Xaml.Application.Current.UnhandledException
        //   See: https://github.com/microsoft/microsoft-ui-xaml/issues/5221
        //
        // * Exceptions caught by Microsoft.UI.Xaml.Application.Current.UnhandledException have details removed,
        //   but that can be worked around by saved by trapping first chance exceptions
        //   See: https://github.com/microsoft/microsoft-ui-xaml/issues/7160
        AppDomain.CurrentDomain.FirstChanceException += (_, args) =>
        {
            _lastFirstChanceException = args.Exception;
        };

        Microsoft.UI.Xaml.Application.Current.UnhandledException += (sender, args) =>
        {
            var exception = args.Exception;

            if (exception.StackTrace is null)
            {
                exception = _lastFirstChanceException;
            }

            if (exception is not null)
            {
                UnhandledException?.Invoke(sender, new UnhandledExceptionEventArgs(exception, true));
            }
        };
#endif
    }

    /// <summary>
    /// The unhandled exception handler.
    /// </summary>
    public static event UnhandledExceptionEventHandler? UnhandledException;
}
