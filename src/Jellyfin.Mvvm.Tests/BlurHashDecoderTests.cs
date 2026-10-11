using Jellyfin.Mvvm.Imaging;

namespace Jellyfin.Mvvm.Tests;

public class BlurHashDecoderTests
{
    private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#$%*+,-.:;=?@[]^_{|}~";

    [Theory]
    [InlineData(0xFF, 0x00, 0x00)]
    [InlineData(0x33, 0x66, 0x99)]
    [InlineData(0x00, 0x00, 0x00)]
    [InlineData(0xFF, 0xFF, 0xFF)]
    public void TryDecode_SingleComponent_FillsAverageColor(byte r, byte g, byte b)
    {
        // One component, so the hash only holds the average color.
        var blurHash = "00" + Encode83((r << 16) | (g << 8) | b, 4);
        var pixels = new byte[4 * 4 * 4];

        Assert.True(BlurHashDecoder.TryDecode(blurHash, 4, 4, pixels));

        for (var i = 0; i < pixels.Length; i += 4)
        {
            Assert.Equal([b, g, r, 255], pixels[i..(i + 4)]);
        }
    }

    [Fact]
    public void TryDecode_ReferenceHash_Succeeds()
    {
        var pixels = new byte[32 * 32 * 4];

        Assert.True(BlurHashDecoder.TryDecode("LEHV6nWB2yk8pyo0adR*.7kCMdnj", 32, 32, pixels));
        Assert.All(Enumerable.Range(0, 32 * 32), i => Assert.Equal(255, pixels[(i * 4) + 3]));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("LEHV6")]
    [InlineData("LEHV6nWB2yk8pyo0adR*.7kCMdn")]
    [InlineData("LEHV6nWB2yk8pyo0adR*.7kCMdnjj")]
    [InlineData("LEHV6nWB2yk8pyo0adR*.7kCMd\"j")]
    public void TryDecode_InvalidHash_ReturnsFalse(string? blurHash)
    {
        Assert.False(BlurHashDecoder.TryDecode(blurHash, 4, 4, new byte[4 * 4 * 4]));
    }

    private static string Encode83(int value, int length)
    {
        var result = new char[length];
        for (var i = length - 1; i >= 0; i--)
        {
            result[i] = Characters[value % 83];
            value /= 83;
        }

        return new string(result);
    }
}
