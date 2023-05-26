using System.Runtime.CompilerServices;

namespace AuroraLib.Texture
{
    public static class GXImageFormat_Info
    {
        private static readonly int[] BlockWidth = { 8, 8, 8, 4, 4, 4, 4, 0, 8, 8, 4, 0, 0, 0, 8 };
        private static readonly int[] BlockHeight = { 8, 4, 4, 4, 4, 4, 4, 0, 8, 4, 4, 0, 0, 0, 8 };
        private static readonly int[] Bpp = { 4, 8, 8, 16, 16, 16, 32, 0, 4, 8, 16, 0, 0, 0, 4 };
        private static readonly int[] MaxTlutColours = { 16, 256, 16384 };


        /// <summary>
        /// Get block width
        /// </summary>
        /// <param name="format"></param>
        /// <returns>Block Width</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBlockWidth(this GXImageFormat format) => BlockWidth[(int)format];
        /// <summary>
        /// Get block height
        /// </summary>
        /// <param name="format"></param>
        /// <returns>Block Height</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBlockHeight(this GXImageFormat format) => BlockHeight[(int)format];
        /// <summary>
        /// Get bits per pixel
        /// </summary>
        /// <param name="Format"></param>
        /// <returns>bits per pixel</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBpp(this GXImageFormat Format) => Bpp[(int)Format];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBytePerBlock(this GXImageFormat Format) => Format == GXImageFormat.RGBA32 ? 64 : 32;

        /// <summary>
        /// Indicates whether it is a format with palette.
        /// </summary>
        /// <param name="Format"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPaletteFormat(this GXImageFormat Format) => Format == GXImageFormat.C4 || Format == GXImageFormat.C8 || Format == GXImageFormat.C14X2;
        /// <summary>
        /// Calculates the size of the image plus its mipmaps in bytes
        /// </summary>
        /// <param name="Format"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="Mipmap"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCalculatedTotalDataSize(this GXImageFormat Format, int width, int height, in int Mipmap)
            => Enumerable.Range(0, Mipmap + 1).Sum(i => Format.CalculatedDataSize(width, height, i));

        /// <summary>
        /// Calculates the size of the mipmap in byts
        /// </summary>
        /// <param name="Format"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipmap"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculatedDataSize(this GXImageFormat Format, in int width, in int height, in int mipmap)
            => Format.CalculatedDataSize(Math.Max(1, width >> mipmap), Math.Max(1, height >> mipmap));

        /// <summary>
        /// Calculates the size of the image in bytes
        /// </summary>
        /// <param name="Format"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculatedDataSize(this GXImageFormat Format, in int width, in int height)
            => Format.CalculateBlockCount(width, height) * Format.GetBytePerBlock();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateBlockCount(this GXImageFormat Format, in int width, in int height)
            => (int)Math.Ceiling((double)width / BlockWidth[(int)Format]) * (int)Math.Ceiling((double)height / BlockHeight[(int)Format]);


        /// <summary>
        /// Calculates the possible number of mipmaps based on size.
        /// </summary>
        /// <param name="Format"></param>
        /// <param name="Size"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        public static int GetMipmapsFromSize(this GXImageFormat Format, int Size, int Width, int Height)
        {
            int mips = 0;
            while (Size > 0 && Width != 0 && Height != 0)
            {
                Size -= CalculatedDataSize(Format, Width, Height);
                Width >>= 1;
                Height >>= 1;
                mips++;
            }
            return mips - 1;
        }
        /// <summary>
        /// get the maximum number of color pallets
        /// </summary>
        /// <param name="Format"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static int GetMaxPaletteColours(this GXImageFormat Format)
        {
            if (!Format.IsPaletteFormat())
                throw new FormatException($"{Format} Is not a Palette format!");
            return MaxTlutColours[(int)Format - 8];
        }

        /// <summary>
        /// get the maximum pallet size in bit.
        /// </summary>
        /// <param name="Format"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxPaletteSize(this GXImageFormat Format)
            => Format.GetMaxPaletteColours() * 2;

        //This function is from here: https://github.com/dolphin-emu/dolphin/blob/94faad0d3727876f507577655d771d1f978b2f4a/Source/Core/VideoCommon/TextureInfo.cpp#L106
        /// <summary>
        /// Calculates the index range of the pallets used by the texture.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static (int start, int length) GetTlutRange(this GXImageFormat format, ReadOnlySpan<byte> bytes)
        {
            int start = 0xffff, length = 0;

            switch (format)
            {
                case GXImageFormat.C4:
                    foreach (byte b in bytes)
                    {
                        int low_nibble = b & 0xf;
                        int high_nibble = b >> 4;

                        start = Math.Min(start, Math.Min(low_nibble, high_nibble));
                        length = Math.Max(length, Math.Max(low_nibble, high_nibble));
                    }
                    break;
                case GXImageFormat.C8:
                    foreach (byte b in bytes)
                    {
                        int texture_byte = b;
                        start = Math.Min(start, texture_byte);
                        length = Math.Max(length, texture_byte);
                    }
                    break;
                case GXImageFormat.C14X2:
                    for (int i = 0; i < bytes.Length; i += 2)
                    {
                        int texture_byte = (short)(bytes[i + 1] | (bytes[i] << 8)) & 0x3fff;
                        start = Math.Min(start, texture_byte);
                        length = Math.Max(length, texture_byte);
                    }
                    break;
                default:
                    break;
            }
            return (start * 2, (length + 1 - start) * 2);
        }
    }
}
