namespace Jellyfin.Mvvm.Imaging;

/// <summary>
/// Encodes pixels into blurhash strings.
/// </summary>
/// <remarks>
/// See https://github.com/woltapp/blurhash/blob/master/Algorithm.md.
/// </remarks>
public static class BlurHashEncoder
{
    /// <summary>
    /// Encode BGRA pixels into a blurhash.
    /// </summary>
    /// <param name="source">The pixels, <c>width * height * 4</c> bytes. Alpha is ignored.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="componentsX">The number of horizontal components, 1 to 9.</param>
    /// <param name="componentsY">The number of vertical components, 1 to 9.</param>
    /// <returns>The blurhash.</returns>
    public static string Encode(ReadOnlySpan<byte> source, int width, int height, int componentsX = 4, int componentsY = 4)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfLessThan(source.Length, width * height * 4);
        ArgumentOutOfRangeException.ThrowIfLessThan(componentsX, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(componentsX, 9);
        ArgumentOutOfRangeException.ThrowIfLessThan(componentsY, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(componentsY, 9);

        var linear = new double[width * height * 3];
        for (var p = 0; p < width * height; p++)
        {
            linear[p * 3] = BlurHashDecoder.SrgbToLinear(source[(p * 4) + 2]);
            linear[(p * 3) + 1] = BlurHashDecoder.SrgbToLinear(source[(p * 4) + 1]);
            linear[(p * 3) + 2] = BlurHashDecoder.SrgbToLinear(source[p * 4]);
        }

        var factors = new double[componentsX * componentsY * 3];
        for (var j = 0; j < componentsY; j++)
        {
            for (var i = 0; i < componentsX; i++)
            {
                double r = 0, g = 0, b = 0;
                for (var y = 0; y < height; y++)
                {
                    var cosY = Math.Cos(Math.PI * j * y / height);
                    for (var x = 0; x < width; x++)
                    {
                        var basis = Math.Cos(Math.PI * i * x / width) * cosY;
                        var pixel = ((y * width) + x) * 3;
                        r += basis * linear[pixel];
                        g += basis * linear[pixel + 1];
                        b += basis * linear[pixel + 2];
                    }
                }

                var scale = (i == 0 && j == 0 ? 1.0 : 2.0) / (width * height);
                var factor = ((j * componentsX) + i) * 3;
                factors[factor] = r * scale;
                factors[factor + 1] = g * scale;
                factors[factor + 2] = b * scale;
            }
        }

        var hash = new char[4 + (2 * componentsX * componentsY)];
        Encode83(componentsX - 1 + ((componentsY - 1) * 9), hash.AsSpan(0, 1));

        var maximum = 1.0;
        if (factors.Length > 3)
        {
            var actualMaximum = 0.0;
            for (var k = 3; k < factors.Length; k++)
            {
                actualMaximum = Math.Max(actualMaximum, Math.Abs(factors[k]));
            }

            var quantisedMaximum = Math.Clamp((int)Math.Floor((actualMaximum * 166) - 0.5), 0, 82);
            maximum = (quantisedMaximum + 1) / 166.0;
            Encode83(quantisedMaximum, hash.AsSpan(1, 1));
        }
        else
        {
            Encode83(0, hash.AsSpan(1, 1));
        }

        var dc = (BlurHashDecoder.LinearToSrgb(factors[0]) << 16)
            | (BlurHashDecoder.LinearToSrgb(factors[1]) << 8)
            | BlurHashDecoder.LinearToSrgb(factors[2]);
        Encode83(dc, hash.AsSpan(2, 4));

        for (var k = 1; k < componentsX * componentsY; k++)
        {
            var ac = (QuantiseAc(factors[k * 3], maximum) * 19 * 19)
                + (QuantiseAc(factors[(k * 3) + 1], maximum) * 19)
                + QuantiseAc(factors[(k * 3) + 2], maximum);
            Encode83(ac, hash.AsSpan(4 + (k * 2), 2));
        }

        return new string(hash);
    }

    private static int QuantiseAc(double value, double maximum)
    {
        var normalised = value / maximum;
        return Math.Clamp((int)Math.Floor((Math.CopySign(Math.Sqrt(Math.Abs(normalised)), normalised) * 9) + 9.5), 0, 18);
    }

    private static void Encode83(int value, Span<char> destination)
    {
        for (var i = destination.Length - 1; i >= 0; i--)
        {
            destination[i] = BlurHashDecoder.Characters[value % 83];
            value /= 83;
        }
    }
}
