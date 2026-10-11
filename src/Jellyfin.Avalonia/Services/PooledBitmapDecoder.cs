using System.Buffers;
using AsyncImageLoader.Core.Decoding;
using Avalonia.Media.Imaging;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Decodes downloaded images through a pooled buffer.
/// </summary>
/// <remarks>
/// The default decoder copies each response into a <see cref="MemoryStream"/> that starts empty and doubles as it
/// grows, which allocated about three times the image size per card and was the largest allocation source while
/// scrolling. <see cref="Bitmap"/> decodes eagerly and copies the encoded bytes, so the buffer can be returned afterwards.
/// </remarks>
public sealed class PooledBitmapDecoder : IBitmapDecoder
{
    // Most card images fit, and the buffer stays below the large object heap threshold.
    private const int InitialBufferSize = 64 * 1024;

    /// <inheritdoc />
    public async Task<Bitmap> DecodeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.CanSeek)
        {
            return new Bitmap(stream);
        }

        var buffer = ArrayPool<byte>.Shared.Rent(InitialBufferSize);
        try
        {
            var length = 0;
            while (true)
            {
                if (length == buffer.Length)
                {
                    var larger = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
                    buffer.AsSpan(0, length).CopyTo(larger);
                    ArrayPool<byte>.Shared.Return(buffer);
                    buffer = larger;
                }

                var read = await stream.ReadAsync(buffer.AsMemory(length), cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }

                length += read;
            }

            cancellationToken.ThrowIfCancellationRequested();
            using var encoded = new MemoryStream(buffer, 0, length, writable: false);
            return new Bitmap(encoded);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
