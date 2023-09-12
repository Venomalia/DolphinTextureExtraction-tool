using AuroraLib.Texture.PixelFormats;

namespace AuroraLib.Texture.BlockFormats
{
    /// <summary>
    /// Represents a block of I4 (Intensity 4-bit) pixels. Each block has a size of 8x8 pixels.
    /// </summary>
    public readonly struct I4Block : IBlock<I8>
    {
        /// <inheritdoc />
        public int BlockWidth => 8;

        /// <inheritdoc />
        public int BlockHeight => 8;

        /// <inheritdoc />
        public int BitsPerPixel => 4;

        /// <inheritdoc />
        public void DecodeBlock(ReadOnlySpan<byte> data, Span<I8> pixels)
        {
            for (int i = 0; i < data.Length; i++)
            {
                pixels[i * 2].PackedValue = (byte)((data[i] & 0xF0) | (data[i] >> 4));
                pixels[i * 2 + 1].PackedValue = (byte)((data[i] << 4) | (data[i] & 0x0F));
            }
        }

        /// <inheritdoc />
        public void EncodeBlock(Span<I8> pixels, Span<byte> data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                byte highNibble = (byte)(pixels[i * 2].PackedValue & 0x0F);
                byte lowNibble = (byte)(pixels[i * 2 + 1].PackedValue & 0x0F);
                data[i] = (byte)((highNibble << 4) | lowNibble);
            }
        }
    }
}
