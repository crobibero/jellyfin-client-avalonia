using System.Buffers;
using AsyncImageLoader.Core.Decoding;
using Avalonia.Media.Imaging;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Reads downloaded images into a pooled buffer and decodes them with the platform <see cref="IImageDecoder"/>.
/// </summary>
/// <remarks>
/// The default decoder copies each response into a <see cref="MemoryStream"/> that starts empty and reallocates each
/// time it doubles. Decoders copy the encoded bytes, so the buffer can be returned once decoding completes.
/// </remarks>
public sealed class PooledBitmapDecoder : IBitmapDecoder
{
    // Most card images fit, and the buffer stays below the large object heap threshold.
    private const int InitialBufferSize = 64 * 1024;

    private readonly IImageDecoder _decoder;

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledBitmapDecoder"/> class.
    /// </summary>
    /// <param name="decoder">The platform image decoder.</param>
    public PooledBitmapDecoder(IImageDecoder decoder)
    {
        _decoder = decoder;
    }

    /// <inheritdoc />
    public async Task<Bitmap> DecodeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream is MemoryStream memory && memory.TryGetBuffer(out var contents))
        {
            return await _decoder.DecodeAsync(contents.AsMemory((int)memory.Position), cancellationToken).ConfigureAwait(false);
        }

        // One spare byte lets a known length finish on the read that returns 0 instead of growing the buffer.
        var buffer = ArrayPool<byte>.Shared.Rent(stream.CanSeek ? (int)(stream.Length - stream.Position) + 1 : InitialBufferSize);
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
            return await _decoder.DecodeAsync(buffer.AsMemory(0, length), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
