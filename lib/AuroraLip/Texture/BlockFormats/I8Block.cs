using AuroraLib.Texture.PixelFormats;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.BlockFormats
{
    /// <summary>
    /// Represents a block of I8 (Intensity 8-bit) pixels. Each block has a size of 8x4 pixels.
    /// </summary>
    public readonly struct I8Block : IBlock<I8>
    {
        /// <inheritdoc />
        public int BlockWidth => 8;

        /// <inheritdoc />
        public int BlockHeight => 4;

        /// <inheritdoc />
        public int BitsPerPixel => 8;

        /// <inheritdoc />
        public void DecodeBlock(ReadOnlySpan<byte> data, Span<I8> pixels)
        {
            ReadOnlySpan<I8> temp = MemoryMarshal.Cast<byte, I8>(data);
            temp.CopyTo(pixels);
        }

        /// <inheritdoc />
        public void EncodeBlock(Span<I8> pixels, Span<byte> data)
        {
            ReadOnlySpan<byte> temp = MemoryMarshal.Cast<I8, byte>(pixels);
            temp.CopyTo(data);
        }

    }
}
