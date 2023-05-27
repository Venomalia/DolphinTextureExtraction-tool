using AuroraLib.Common;
using AuroraLib.Texture.PixelFormats;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.BlockFormats
{
    /// <summary>
    /// Represents a block of RGB5A3 pixels. Each block has a size of 4x4 pixels.
    /// </summary>
    public readonly struct RGB5A3Block : IBlock<RGB5A3>
    {
        /// <inheritdoc />
        public int BlockWidth => 4;

        /// <inheritdoc />
        public int BlockHeight => 4;

        /// <inheritdoc />
        public int BitsPerPixel => 16;

        /// <inheritdoc />
        public void DecodeBlock(ReadOnlySpan<byte> data, Span<RGB5A3> pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = MemoryMarshal.AsRef<RGB5A3>(data.Slice(i * 2, 2));
                pixels[i].PackedValue = pixels[i].PackedValue.Swap();
            }
        }

        /// <inheritdoc />
        public void EncodeBlock(Span<RGB5A3> pixels, Span<byte> data)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                ushort value = pixels[i].PackedValue.Swap();
                MemoryMarshal.Write(data.Slice(i * 2, 2), ref value);
            }
        }
    }
}
