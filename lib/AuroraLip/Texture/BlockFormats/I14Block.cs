using AuroraLib.Texture.PixelFormats;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.BlockFormats
{
    /// <summary>
    /// Represents a block of I14 (Intensity 14-bit) pixels. Each block has a size of 8x4 pixels.
    /// </summary>
    public readonly struct I14Block : IBlock<I16>
    {
        /// <inheritdoc />
        public int BlockWidth => 4;

        /// <inheritdoc />
        public int BlockHeight => 4;

        /// <inheritdoc />
        public int BitsPerPixel => 16;

        /// <inheritdoc />
        public void DecodeBlock(ReadOnlySpan<byte> data, Span<I16> pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = MemoryMarshal.AsRef<I16>(data.Slice(i * 2, 2));
                pixels[i].PackedValue = BitConverterX.Swap(pixels[i].PackedValue);
                pixels[i].PackedValue &= 0x3FFF;
            }
        }

        /// <inheritdoc />
        public void EncodeBlock(Span<I16> pixels, Span<byte> data)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].PackedValue &= 0x3FFF;
                ushort value = BitConverterX.Swap(pixels[i].PackedValue);
                MemoryMarshal.Write(data.Slice(i * 2, 2), ref value);
            }
        }
    }
}
