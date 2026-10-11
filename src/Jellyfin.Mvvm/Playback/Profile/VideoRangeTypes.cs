namespace Jellyfin.Mvvm.Playback.Profile;

/// <summary>
/// Video range type names as used by the Jellyfin server.
/// </summary>
public static class VideoRangeTypes
{
    /// <summary>Standard dynamic range.</summary>
    public const string Sdr = "SDR";

    /// <summary>HDR10.</summary>
    public const string Hdr10 = "HDR10";

    /// <summary>HDR10+.</summary>
    public const string Hdr10Plus = "HDR10Plus";

    /// <summary>Hybrid log-gamma.</summary>
    public const string Hlg = "HLG";

    /// <summary>Dolby Vision without a fallback layer (profile 5).</summary>
    public const string Dovi = "DOVI";

    /// <summary>Dolby Vision with an HDR10 fallback (profile 8.1).</summary>
    public const string DoviWithHdr10 = "DOVIWithHDR10";

    /// <summary>Dolby Vision with an HLG fallback (profile 8.4).</summary>
    public const string DoviWithHlg = "DOVIWithHLG";

    /// <summary>Dolby Vision with an SDR fallback (profile 8.2).</summary>
    public const string DoviWithSdr = "DOVIWithSDR";

    /// <summary>Dolby Vision with an enhancement layer (profile 7).</summary>
    public const string DoviWithEl = "DOVIWithEL";

    /// <summary>Dolby Vision with an HDR10+ fallback.</summary>
    public const string DoviWithHdr10Plus = "DOVIWithHDR10Plus";

    /// <summary>Dolby Vision with an enhancement layer and HDR10+ metadata.</summary>
    public const string DoviWithElHdr10Plus = "DOVIWithELHDR10Plus";

    /// <summary>Invalid Dolby Vision metadata.</summary>
    public const string DoviInvalid = "DOVIInvalid";
}
