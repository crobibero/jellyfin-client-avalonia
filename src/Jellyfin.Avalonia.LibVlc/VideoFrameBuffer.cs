using System.Runtime.InteropServices;

namespace Jellyfin.Avalonia.LibVlc;

/// <summary>
/// A double buffered BGRA frame buffer written by the decoder and read by the UI.
/// </summary>
public sealed unsafe class VideoFrameBuffer : IDisposable
{
    private readonly Lock _sync = new();
    private IntPtr _front;
    private IntPtr _back;
    private bool _disposed;

    /// <summary>
    /// Raised on the decoder thread when a new frame is available.
    /// </summary>
    public event EventHandler? FrameAvailable;

    /// <summary>
    /// Gets the frame width.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the frame height.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the frame stride in bytes.
    /// </summary>
    public int Stride { get; private set; }

    /// <summary>
    /// Gets the frame sequence number, increased for every displayed frame.
    /// </summary>
    public long Sequence { get; private set; }

    /// <summary>
    /// Allocate buffers for a frame size.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void Configure(int width, int height)
    {
        lock (_sync)
        {
            Free();
            Width = width;
            Height = height;
            Stride = width * 4;
            var size = (nuint)(Stride * height);
            _front = (IntPtr)NativeMemory.AllocZeroed(size);
            _back = (IntPtr)NativeMemory.AllocZeroed(size);
        }
    }

    /// <summary>
    /// Gets the buffer the decoder writes the next frame into.
    /// </summary>
    /// <returns>The buffer pointer.</returns>
    public IntPtr GetBackBuffer() => _back;

    /// <summary>
    /// Publish the back buffer as the newest frame.
    /// </summary>
    public void Present()
    {
        lock (_sync)
        {
            (_front, _back) = (_back, _front);
            Sequence++;
        }

        FrameAvailable?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Copy the newest frame into a destination buffer.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <param name="destinationStride">The destination stride.</param>
    /// <param name="width">The expected width.</param>
    /// <param name="height">The expected height.</param>
    /// <returns><c>true</c> when the frame was copied.</returns>
    public bool CopyTo(IntPtr destination, int destinationStride, int width, int height)
    {
        lock (_sync)
        {
            if (_front == IntPtr.Zero || width != Width || height != Height)
            {
                return false;
            }

            var source = (byte*)_front;
            var target = (byte*)destination;
            if (destinationStride == Stride)
            {
                Buffer.MemoryCopy(source, target, (long)destinationStride * height, (long)Stride * height);
            }
            else
            {
                var rowBytes = Math.Min(Stride, destinationStride);
                for (var y = 0; y < height; y++)
                {
                    Buffer.MemoryCopy(source + ((long)y * Stride), target + ((long)y * destinationStride), destinationStride, rowBytes);
                }
            }

            return true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Free();
        }
    }

    private void Free()
    {
        if (_front != IntPtr.Zero)
        {
            NativeMemory.Free((void*)_front);
            _front = IntPtr.Zero;
        }

        if (_back != IntPtr.Zero)
        {
            NativeMemory.Free((void*)_back);
            _back = IntPtr.Zero;
        }

        Width = 0;
        Height = 0;
    }
}
