using AuroraLib.Texture.PixelFormats;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.BlockFormats
{
    /// <summary>
    /// Represents a block of IA8 pixels. Each block has a size of 4x4 pixels.
    /// </summary>
    public readonly struct IA8Block : IBlock<IA8>
    {
        /// <inheritdoc />
        public int BlockWidth => 4;

        /// <inheritdoc />
        public int BlockHeight => 4;

        /// <inheritdoc />
        public int BitsPerPixel => 16;

        /// <inheritdoc />
        public void DecodeBlock(ReadOnlySpan<byte> data, Span<IA8> pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = MemoryMarshal.AsRef<IA8>(data.Slice(i * 2, 2));
                pixels[i].PackedValue = BitConverterX.Swap(pixels[i].PackedValue);
            }
        }

        /// <inheritdoc />
        public void EncodeBlock(Span<IA8> pixels, Span<byte> data)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                ushort value = BitConverterX.Swap(pixels[i].PackedValue);
                MemoryMarshal.Write(data.Slice(i * 2, 2), ref value);
            }
        }
    }
}
