namespace Jellyfin.Mvvm.Playback.Profile;

/// <summary>
/// Codec and container names as used by the Jellyfin server.
/// </summary>
public static class Codec
{
    /// <summary>
    /// Container names.
    /// </summary>
    public static class Container
    {
        /// <summary>3GP.</summary>
        public const string ThreeGp = "3gp";

        /// <summary>ASF.</summary>
        public const string Asf = "asf";

        /// <summary>AVI.</summary>
        public const string Avi = "avi";

        /// <summary>DVR-MS.</summary>
        public const string DvrMs = "dvr-ms";

        /// <summary>FLV.</summary>
        public const string Flv = "flv";

        /// <summary>HLS.</summary>
        public const string Hls = "hls";

        /// <summary>M2TS.</summary>
        public const string M2ts = "m2ts";

        /// <summary>M2V.</summary>
        public const string M2v = "m2v";

        /// <summary>M4V.</summary>
        public const string M4v = "m4v";

        /// <summary>MKV.</summary>
        public const string Mkv = "mkv";

        /// <summary>MOV.</summary>
        public const string Mov = "mov";

        /// <summary>MP3.</summary>
        public const string Mp3 = "mp3";

        /// <summary>MP4.</summary>
        public const string Mp4 = "mp4";

        /// <summary>MPEG.</summary>
        public const string Mpeg = "mpeg";

        /// <summary>MPEG-TS.</summary>
        public const string MpegTs = "mpegts";

        /// <summary>MPG.</summary>
        public const string Mpg = "mpg";

        /// <summary>OGM.</summary>
        public const string Ogm = "ogm";

        /// <summary>OGV.</summary>
        public const string Ogv = "ogv";

        /// <summary>TS.</summary>
        public const string Ts = "ts";

        /// <summary>VOB.</summary>
        public const string Vob = "vob";

        /// <summary>WebM.</summary>
        public const string WebM = "webm";

        /// <summary>WMV.</summary>
        public const string Wmv = "wmv";

        /// <summary>WTV.</summary>
        public const string Wtv = "wtv";

        /// <summary>XviD.</summary>
        public const string Xvid = "xvid";
    }

    /// <summary>
    /// Audio codec names.
    /// </summary>
    public static class Audio
    {
        /// <summary>AAC.</summary>
        public const string Aac = "aac";

        /// <summary>AAC LATM.</summary>
        public const string AacLatm = "aac_latm";

        /// <summary>AC-3.</summary>
        public const string Ac3 = "ac3";

        /// <summary>ALAC.</summary>
        public const string Alac = "alac";

        /// <summary>APE.</summary>
        public const string Ape = "ape";

        /// <summary>DCA.</summary>
        public const string Dca = "dca";

        /// <summary>DTS.</summary>
        public const string Dts = "dts";

        /// <summary>E-AC-3.</summary>
        public const string Eac3 = "eac3";

        /// <summary>FLAC.</summary>
        public const string Flac = "flac";

        /// <summary>MLP.</summary>
        public const string Mlp = "mlp";

        /// <summary>MP2.</summary>
        public const string Mp2 = "mp2";

        /// <summary>MP3.</summary>
        public const string Mp3 = "mp3";

        /// <summary>MPA.</summary>
        public const string Mpa = "mpa";

        /// <summary>OGA.</summary>
        public const string Oga = "oga";

        /// <summary>OGG.</summary>
        public const string Ogg = "ogg";

        /// <summary>Opus.</summary>
        public const string Opus = "opus";

        /// <summary>PCM.</summary>
        public const string Pcm = "pcm";

        /// <summary>PCM A-law.</summary>
        public const string PcmAlaw = "pcm_alaw";

        /// <summary>PCM mu-law.</summary>
        public const string PcmMulaw = "pcm_mulaw";

        /// <summary>PCM signed 16-bit little endian.</summary>
        public const string PcmS16Le = "pcm_s16le";

        /// <summary>PCM signed 20-bit little endian.</summary>
        public const string PcmS20Le = "pcm_s20le";

        /// <summary>PCM signed 24-bit little endian.</summary>
        public const string PcmS24Le = "pcm_s24le";

        /// <summary>TrueHD.</summary>
        public const string TrueHd = "truehd";

        /// <summary>Vorbis.</summary>
        public const string Vorbis = "vorbis";

        /// <summary>WAV.</summary>
        public const string Wav = "wav";

        /// <summary>WebM audio.</summary>
        public const string WebMa = "webma";

        /// <summary>WMA.</summary>
        public const string Wma = "wma";

        /// <summary>WMA v2.</summary>
        public const string Wmav2 = "wmav2";
    }

    /// <summary>
    /// Video codec names.
    /// </summary>
    public static class Video
    {
        /// <summary>H.264 / AVC.</summary>
        public const string H264 = "h264";

        /// <summary>H.265 / HEVC.</summary>
        public const string Hevc = "hevc";

        /// <summary>MPEG.</summary>
        public const string Mpeg = "mpeg";

        /// <summary>MPEG-2.</summary>
        public const string Mpeg2Video = "mpeg2video";

        /// <summary>MPEG-4 part 2.</summary>
        public const string Mpeg4 = "mpeg4";

        /// <summary>VP8.</summary>
        public const string Vp8 = "vp8";

        /// <summary>VP9.</summary>
        public const string Vp9 = "vp9";

        /// <summary>AV1.</summary>
        public const string Av1 = "av1";

        /// <summary>VC-1.</summary>
        public const string Vc1 = "vc1";
    }

    /// <summary>
    /// Subtitle format names.
    /// </summary>
    public static class Subtitle
    {
        /// <summary>ASS.</summary>
        public const string Ass = "ass";

        /// <summary>DVB subtitles.</summary>
        public const string DvbSub = "dvbsub";

        /// <summary>DVD subtitles.</summary>
        public const string DvdSub = "dvdsub";

        /// <summary>IDX/SUB.</summary>
        public const string Idx = "idx";

        /// <summary>PGS.</summary>
        public const string Pgs = "pgs";

        /// <summary>PGS (ffmpeg name).</summary>
        public const string PgsSub = "pgssub";

        /// <summary>SAMI.</summary>
        public const string Smi = "smi";

        /// <summary>SubRip.</summary>
        public const string Srt = "srt";

        /// <summary>SSA.</summary>
        public const string Ssa = "ssa";

        /// <summary>MicroDVD.</summary>
        public const string Sub = "sub";

        /// <summary>SubRip (ffmpeg name).</summary>
        public const string SubRip = "subrip";

        /// <summary>WebVTT.</summary>
        public const string Vtt = "vtt";

        /// <summary>SMIL.</summary>
        public const string Smil = "smil";

        /// <summary>TTML.</summary>
        public const string Ttml = "ttml";

        /// <summary>WebVTT (ffmpeg name).</summary>
        public const string WebVtt = "webvtt";
    }
}
