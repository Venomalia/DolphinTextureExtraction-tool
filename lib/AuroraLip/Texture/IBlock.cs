using System.Runtime.CompilerServices;

namespace AuroraLib.Texture
{
    /// <summary>
    /// Represents a image block format.
    /// </summary>
    public interface IBlock
    {
        /// <summary>
        /// Gets the width of the block.
        /// </summary>
        int BlockWidth { get; }

        /// <summary>
        /// Gets the height of the block.
        /// </summary>
        int BlockHeight { get; }

        /// <summary>
        /// Gets the number of bits per pixel in the block.
        /// </summary>
        int BitsPerPixel { get; }

        /// <summary>
        /// Gets the number of bytes per block.
        /// </summary>
        int BytePerBlock => BitsPerPixel * BlockWidth * BlockHeight / 8;

        /// <summary>
        /// Gets the number of pixels per block.
        /// </summary>
        int PixelPerBlock => BlockWidth * BlockHeight;

        /// <summary>
        /// Calculates the number of blocks required based on the specified <paramref name="width"/> and <paramref name="height"/>.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>The number of blocks required.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculateBlockCount(in int width, in int height)
            => (int)Math.Ceiling((double)width / BlockWidth) * (int)Math.Ceiling((double)height / BlockHeight);

        /// <summary>
        /// Calculates the size of the data based on the specified <paramref name="width"/> and <paramref name="height"/>.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>The calculated data size.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculatedDataSize(in int width, in int height)
            => CalculateBlockCount(width, height) * BytePerBlock;

        /// <summary>
        /// Calculates the size of the data based on the specified <paramref name="width"/>, <paramref name="height"/>, and <paramref name="mipmap"/> level.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="mipmap">The mipmap level of the texture.</param>
        /// <returns>The calculated data size.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculatedDataSize(in int width, in int height, in int mipmap)
            => CalculatedDataSize(Math.Max(1, width >> mipmap), Math.Max(1, height >> mipmap));

        /// <summary>
        /// Calculates the total data size in bytes for a texture with the specified <paramref name="width"/>, <paramref name="height"/>, and <paramref name="mipmap"/> levels.
        /// </summary>
        /// <param name="Width">The width of the texture.</param>
        /// <param name="Height">The height of the texture.</param>
        /// <param name="Mipmap">The number of mipmap levels.</param>
        /// <returns>The total data size in bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculatedTotalDataSize(int width, int height, in int mipmap)
            => Enumerable.Range(0, mipmap + 1).Sum(i => CalculatedDataSize(width, height, i));

        /// <summary>
        /// Calculates the number of mipmap levels based on the total data <paramref name="size"/>, <paramref name="width"/>, and <paramref name="height"/> of the texture.
        /// </summary>
        /// <param name="size">The total data size in bytes.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>The number of mipmap levels.</returns>
        public int CalculatedMipmapsFromSize(int size, int width, int height)
        {
            int image = 0;
            while (size > 0 && width != 0 && height != 0)
            {
                size -= CalculatedDataSize(width, height);
                width >>= 1;
                height >>= 1;
                image++;
            }
            return image - 1;
        }
    }
}
