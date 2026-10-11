using Android.App;
using Android.Content.PM;
using Avalonia.Android;
using static Android.Content.Intent;

namespace Jellyfin.Avalonia.Android;

/// <summary>
/// The main activity, launchable from both the phone and the Android TV (leanback) launcher.
/// </summary>
[Activity(
    Label = "Jellyfin",
    Theme = "@style/JellyfinTheme",
    Icon = "@mipmap/ic_launcher",
    MainLauncher = true,
    Exported = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Navigation)]
[IntentFilter([ActionMain], Categories = [CategoryLauncher, CategoryLeanbackLauncher])]
public class MainActivity : AvaloniaMainActivity
{
}
