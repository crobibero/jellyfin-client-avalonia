using BlurHashSharp;
using Jellyfin.Mvvm.Imaging;

namespace Jellyfin.Mvvm.Tests;

/// <summary>
/// Hashes made by BlurHashSharp, the encoder the server and the client use, decode back to the source.
/// </summary>
public class BlurHashRoundTripTests
{
    [Theory]
    [InlineData(0xFF, 0x00, 0x00)]
    [InlineData(0x33, 0x66, 0x99)]
    [InlineData(0x00, 0x00, 0x00)]
    [InlineData(0xFF, 0xFF, 0xFF)]
    public void Decode_SolidColor_RoundTrips(byte r, byte g, byte b)
    {
        var source = new byte[8 * 8 * 4];
        for (var i = 0; i < source.Length; i += 4)
        {
            source[i] = b;
            source[i + 1] = g;
            source[i + 2] = r;
            source[i + 3] = 255;
        }

        // One component, so the hash only holds the average color.
        var blurHash = CoreBlurHashEncoder.Encode(1, 1, 8, 8, source, 8 * 4, PixelFormat.BGR888x);
        var pixels = new byte[4 * 4 * 4];

        Assert.Equal(6, blurHash.Length);
        Assert.True(BlurHashDecoder.TryDecode(blurHash, 4, 4, pixels));
        for (var i = 0; i < pixels.Length; i += 4)
        {
            Assert.Equal([b, g, r, 255], pixels[i..(i + 4)]);
        }
    }

    [Fact]
    public void Decode_Gradient_KeepsDirection()
    {
        // Dark on the left, light on the right.
        const int Size = 16;
        var source = new byte[Size * Size * 4];
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var value = (byte)(x * 255 / (Size - 1));
                source.AsSpan(((y * Size) + x) * 4, 4).Fill(value);
            }
        }

        var blurHash = CoreBlurHashEncoder.Encode(4, 3, Size, Size, source, Size * 4, PixelFormat.BGR888x);
        var pixels = new byte[Size * Size * 4];

        Assert.True(BlurHashDecoder.TryDecode(blurHash, Size, Size, pixels));
        var row = (Size / 2) * Size * 4;
        Assert.True(pixels[row] < 64);
        Assert.True(pixels[row + ((Size - 1) * 4)] > 192);
    }
}
