using AuroraLib.Texture.PixelFormats;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.BlockFormats
{
    /// <summary>
    /// Represents a block of IA4 pixels. Each block has a size of 8x4 pixels.
    /// </summary>
    public readonly struct IA4Block : IBlock<IA4>
    {
        /// <inheritdoc />
        public int BlockWidth => 8;

        /// <inheritdoc />
        public int BlockHeight => 4;

        /// <inheritdoc />
        public int BitsPerPixel => 8;

        /// <inheritdoc />
        public void DecodeBlock(ReadOnlySpan<byte> data, Span<IA4> pixels)
        {
            ReadOnlySpan<IA4> temp = MemoryMarshal.Cast<byte, IA4>(data);
            temp.CopyTo(pixels);
        }

        /// <inheritdoc />
        public void EncodeBlock(Span<IA4> pixels, Span<byte> data)
        {
            ReadOnlySpan<byte> temp = MemoryMarshal.Cast<IA4, byte>(pixels);
            temp.CopyTo(data);
        }
    }
}
