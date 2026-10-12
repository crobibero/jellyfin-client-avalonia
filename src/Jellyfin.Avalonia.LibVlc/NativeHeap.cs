using System.Runtime.InteropServices;

namespace Jellyfin.Avalonia.LibVlc;

/// <summary>
/// Tunes the glibc native heap on Linux.
/// </summary>
/// <remarks>
/// libavcodec decodes on many threads, and glibc gives threads their own malloc arenas. Frame buffers freed when a
/// player stops stay resident in those arenas.
/// </remarks>
public static partial class NativeHeap
{
    // M_ARENA_MAX from malloc.h.
    private const int ArenaMaxOption = -8;

    /// <summary>
    /// Limit the number of malloc arenas. Call early in Main, arenas created before the call still count toward the limit.
    /// Does nothing outside glibc based Linux.
    /// </summary>
    /// <param name="count">The maximum number of arenas.</param>
    public static void LimitArenas(int count)
        => Invoke(() => _ = MallOpt(ArenaMaxOption, count));

    /// <summary>
    /// Release free native heap memory to the operating system. Does nothing outside glibc based Linux.
    /// </summary>
    public static void Trim()
        => Invoke(() => _ = MallocTrim(0));

    private static void Invoke(Action action)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        try
        {
            action();
        }
        catch (DllNotFoundException)
        {
            // Not glibc, for example musl.
        }
        catch (EntryPointNotFoundException)
        {
            // A libc without the function.
        }
    }

    [LibraryImport("libc.so.6", EntryPoint = "malloc_trim")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static partial int MallocTrim(nuint pad);

    [LibraryImport("libc.so.6", EntryPoint = "mallopt")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static partial int MallOpt(int param, int value);
}
