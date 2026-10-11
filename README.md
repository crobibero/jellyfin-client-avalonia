## Jellyfin Avalonia

[![Build Desktop](https://github.com/crobibero/jellyfin-client-avalonia/actions/workflows/build-desktop.yml/badge.svg)](https://github.com/crobibero/jellyfin-client-avalonia/actions/workflows/build-desktop.yml)
[![Build Android](https://github.com/crobibero/jellyfin-client-avalonia/actions/workflows/build-android.yml/badge.svg)](https://github.com/crobibero/jellyfin-client-avalonia/actions/workflows/build-android.yml)
[![Build iOS](https://github.com/crobibero/jellyfin-client-avalonia/actions/workflows/build-ios.yml/badge.svg)](https://github.com/crobibero/jellyfin-client-avalonia/actions/workflows/build-ios.yml)
[![Build Browser](https://github.com/crobibero/jellyfin-client-avalonia/actions/workflows/build-browser.yml/badge.svg)](https://github.com/crobibero/jellyfin-client-avalonia/actions/workflows/build-browser.yml)

![GitHub](https://img.shields.io/github/license/crobibero/jellyfin-client-avalonia)

An experimental Jellyfin client written in C# with [Avalonia](https://avaloniaui.net/) 12 and .NET 10. One code base targets Windows, macOS, Linux, Android (phones, tablets, Android TV and Fire TV), iOS, and web browsers.

## Platforms

| Platform | Project | Player | Status |
| --- | --- | --- | --- |
| Windows, macOS, Linux | `Jellyfin.Avalonia.Desktop` | libVLC | Built and run on Linux |
| Android, Android TV | `Jellyfin.Avalonia.Android` | libVLC with MediaCodec | Built and run on an Android TV emulator |
| iOS, iPadOS | `Jellyfin.Avalonia.iOS` | libVLC with VideoToolbox | Builds on macOS only; not run |
| Web browsers | `Jellyfin.Avalonia.Browser` | HTML5 video and hls.js | Built and run in Chromium |

Desktop builds bundle libVLC on Windows and macOS through the `VideoLAN.LibVLC.*` packages. Linux builds use the system libVLC, so install `vlc` or `libvlc` from your distribution.

## Building

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download). All commands run from `src`.

Desktop:

```sh
dotnet run --project Jellyfin.Avalonia.Desktop
dotnet test --project Jellyfin.Mvvm.Tests
dotnet publish Jellyfin.Avalonia.Desktop -c Release -r linux-x64
```

`Jellyfin.Avalonia.Desktop.slnf` opens the desktop projects without the mobile workloads.

Android needs the `android` workload and a JDK 21. The `InstallAndroidDependencies` target downloads the Android SDK:

```sh
dotnet workload install android
dotnet build Jellyfin.Avalonia.Android -t:InstallAndroidDependencies -p:AcceptAndroidSdkLicenses=True
dotnet publish Jellyfin.Avalonia.Android -c Release -f net10.0-android
```

iOS needs macOS, Xcode, and the `ios` workload:

```sh
dotnet workload install ios
dotnet build Jellyfin.Avalonia.iOS -f net10.0-ios -r iossimulator-arm64
```

The browser build needs the `wasm-tools` workload. Serve the published `wwwroot` folder from any static web server:

```sh
dotnet workload install wasm-tools
dotnet publish Jellyfin.Avalonia.Browser -c Release -o artifacts/browser
python3 -m http.server -d artifacts/browser/wwwroot 8080
```

The browser client calls the Jellyfin API across origins. Jellyfin allows this by default (`Access-Control-Allow-Origin: *`). hls.js 1.7.3 is loaded from jsDelivr when a transcoded stream needs it.
