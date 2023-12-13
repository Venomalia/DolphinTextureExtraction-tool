using SixLabors.ImageSharp.PixelFormats;

namespace AuroraLib.Texture.BlockFormats
{
    /// <summary>
    /// Represents a block of RGBA32 pixels. Each block has a size of 4x4 pixels.
    /// The 64-byte block is organized in a zigzag pattern.
    /// </summary>
    public readonly struct RGBA32Block : IBlock<Rgba32>
    {
        /// <inheritdoc />
        public int BlockWidth => 4;

        /// <inheritdoc />
        public int BlockHeight => 4;

        /// <inheritdoc />
        public int BitsPerPixel => 32;

        /*
         * The pixel data is separated into two groups:
         * A and R are encoded in the first group, and G and B are encoded in the second group.
         * The data is organized as follows:
         * ARARARARARARARAR
         * ARARARARARARARAR
         * GBGBGBGBGBGBGBGB
         * GBGBGBGBGBGBGBGB
        */

        /// <inheritdoc />
        public void DecodeBlock(ReadOnlySpan<byte> data, Span<Rgba32> pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Rgba32(data[(i * 2) + 1], data[(i * 2) + 32], data[(i * 2) + 33], data[(i * 2)]);
        }

        /// <inheritdoc />
        public void EncodeBlock(Span<Rgba32> pixels, Span<byte> data)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                data[i * 2] = pixels[i].A;
                data[(i * 2) + 01] = pixels[i].R;
                data[(i * 2) + 32] = pixels[i].G;
                data[(i * 2) + 33] = pixels[i].B;
            }
        }
    }
}
