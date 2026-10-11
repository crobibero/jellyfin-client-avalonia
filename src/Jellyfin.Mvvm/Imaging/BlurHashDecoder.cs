namespace Jellyfin.Mvvm.Imaging;

/// <summary>
/// Decodes blurhash strings into pixels.
/// </summary>
/// <remarks>
/// See https://github.com/woltapp/blurhash/blob/master/Algorithm.md.
/// </remarks>
public static class BlurHashDecoder
{
    internal const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#$%*+,-.:;=?@[]^_{|}~";

    /// <summary>
    /// Decode a blurhash into opaque BGRA pixels.
    /// </summary>
    /// <param name="blurHash">The blurhash.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="destination">The pixel buffer, at least <c>width * height * 4</c> bytes.</param>
    /// <param name="punch">The contrast multiplier.</param>
    /// <returns>Whether the blurhash was valid.</returns>
    public static bool TryDecode(string? blurHash, int width, int height, Span<byte> destination, double punch = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, width * height * 4);

        if (blurHash is not { Length: >= 6 } || !TryDecode83(blurHash.AsSpan(0, 1), out var sizeFlag))
        {
            return false;
        }

        var componentsX = (sizeFlag % 9) + 1;
        var componentsY = (sizeFlag / 9) + 1;
        if (blurHash.Length != 4 + (2 * componentsX * componentsY)
            || !TryDecode83(blurHash.AsSpan(1, 1), out var quantisedMaximum)
            || !TryDecode83(blurHash.AsSpan(2, 4), out var dc))
        {
            return false;
        }

        var maximum = (quantisedMaximum + 1) / 166.0 * punch;
        var colors = new double[componentsX * componentsY * 3];
        colors[0] = SrgbToLinear(dc >> 16);
        colors[1] = SrgbToLinear((dc >> 8) & 255);
        colors[2] = SrgbToLinear(dc & 255);

        for (var i = 1; i < componentsX * componentsY; i++)
        {
            if (!TryDecode83(blurHash.AsSpan(4 + (i * 2), 2), out var ac))
            {
                return false;
            }

            colors[i * 3] = DecodeAc(ac / (19 * 19), maximum);
            colors[(i * 3) + 1] = DecodeAc((ac / 19) % 19, maximum);
            colors[(i * 3) + 2] = DecodeAc(ac % 19, maximum);
        }

        var cosX = new double[componentsX * width];
        for (var i = 0; i < componentsX; i++)
        {
            for (var x = 0; x < width; x++)
            {
                cosX[(i * width) + x] = Math.Cos(Math.PI * x * i / width);
            }
        }

        var cosY = new double[componentsY * height];
        for (var j = 0; j < componentsY; j++)
        {
            for (var y = 0; y < height; y++)
            {
                cosY[(j * height) + y] = Math.Cos(Math.PI * y * j / height);
            }
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                double r = 0, g = 0, b = 0;
                for (var j = 0; j < componentsY; j++)
                {
                    for (var i = 0; i < componentsX; i++)
                    {
                        var basis = cosX[(i * width) + x] * cosY[(j * height) + y];
                        var color = ((j * componentsX) + i) * 3;
                        r += colors[color] * basis;
                        g += colors[color + 1] * basis;
                        b += colors[color + 2] * basis;
                    }
                }

                var pixel = ((y * width) + x) * 4;
                destination[pixel] = LinearToSrgb(b);
                destination[pixel + 1] = LinearToSrgb(g);
                destination[pixel + 2] = LinearToSrgb(r);
                destination[pixel + 3] = 255;
            }
        }

        return true;
    }

    private static bool TryDecode83(ReadOnlySpan<char> value, out int result)
    {
        result = 0;
        foreach (var character in value)
        {
            var digit = Characters.IndexOf(character, StringComparison.Ordinal);
            if (digit < 0)
            {
                return false;
            }

            result = (result * 83) + digit;
        }

        return true;
    }

    private static double DecodeAc(int quantised, double maximum)
    {
        var value = (quantised - 9) / 9.0;
        return Math.CopySign(value * value, value) * maximum;
    }

    internal static double SrgbToLinear(int value)
    {
        var v = value / 255.0;
        return v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
    }

    internal static byte LinearToSrgb(double value)
    {
        var v = Math.Clamp(value, 0, 1);
        var srgb = v <= 0.0031308 ? v * 12.92 : (1.055 * Math.Pow(v, 1 / 2.4)) - 0.055;
        return (byte)Math.Round(srgb * 255);
    }
}
