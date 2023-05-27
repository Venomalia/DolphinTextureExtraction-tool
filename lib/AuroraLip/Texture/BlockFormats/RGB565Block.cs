using AuroraLib.Common;
using AuroraLib.Texture.PixelFormats;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.BlockFormats
{
    /// <summary>
    /// Represents a block of RGB565 pixels. Each block has a size of 4x4 pixels.
    /// </summary>
    public readonly struct RGB565Block : IBlock<RGB565>
    {
        /// <inheritdoc />
        public int BlockWidth => 4;

        /// <inheritdoc />
        public int BlockHeight => 4;

        /// <inheritdoc />
        public int BitsPerPixel => 16;

        /// <inheritdoc />
        public void DecodeBlock(ReadOnlySpan<byte> data, Span<RGB565> pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = MemoryMarshal.AsRef<RGB565>(data.Slice(i * 2, 2));
                pixels[i].PackedValue = pixels[i].PackedValue.Swap();
            }
        }

        /// <inheritdoc />
        public void EncodeBlock(Span<RGB565> pixels, Span<byte> data)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                ushort value = pixels[i].PackedValue.Swap();
                MemoryMarshal.Write(data.Slice(i * 2, 2), ref value);
            }
        }
    }
}
