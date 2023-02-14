using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraLip.Texture
{
    public static class GXImageFormat_Info
    {
        private static readonly int[] BlockWidth = { 8, 8, 8, 4, 4, 4, 4, 0, 8, 8, 4, 0, 0, 0, 8 };
        private static readonly int[] BlockHeight = { 8, 4, 4, 4, 4, 4, 4, 0, 8, 4, 4, 0, 0, 0, 8 };
        private static readonly int[] Bpp = { 4, 8, 8, 16, 16, 16, 32, 0, 4, 8, 16, 0, 0, 0, 4 };
        private static readonly int[] MaxTlutColours = { 16, 256, 16384};

        /// <summary>
        /// Get block width and height
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static (int blockWidth, int blockHeight) GetBlockSize(this GXImageFormat format) => (BlockWidth[(int)format], BlockHeight[(int)format]);
        /// <summary>
        /// Get block width
        /// </summary>
        /// <param name="format"></param>
        /// <returns>Block Width</returns>
        public static int GetBlockWidth(this GXImageFormat format) => BlockWidth[(int)format];
        /// <summary>
        /// Get block height
        /// </summary>
        /// <param name="format"></param>
        /// <returns>Block Height</returns>
        public static int GetBlockHeight(this GXImageFormat format) => BlockHeight[(int)format];
        /// <summary>
        /// Get bits per pixel
        /// </summary>
        /// <param name="Format"></param>
        /// <returns>bits per pixel</returns>
        public static int GetBpp(this GXImageFormat Format) { return Bpp[(int)Format]; }

        public static int GetBlockDataSize(this GXImageFormat Format) => Format == GXImageFormat.RGBA32 ? 64 : 32;
        /// <summary>
        /// Indicates whether it is a format with palette.
        /// </summary>
        /// <param name="Format"></param>
        /// <returns></returns>
        public static bool IsPaletteFormat(this GXImageFormat Format) => Format == GXImageFormat.C4 || Format == GXImageFormat.C8 || Format == GXImageFormat.C14X2;
        /// <summary>
        /// Calculates the size of the image plus its mipmaps in bytes
        /// </summary>
        /// <param name="Format"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Mipmap"></param>
        /// <returns></returns>
        public static int GetCalculatedTotalDataSize(this GXImageFormat Format, int Width, int Height, int Mipmap)
        {
            int TotalSize = 0;
            for (int i = 0; i <= Mipmap; i++)
            {
                TotalSize += GetCalculatedDataSize(Format, Width, Height, i);
            }
            return TotalSize;
        }
        /// <summary>
        /// Calculates the size of the mipmap in byts
        /// </summary>
        /// <param name="Format"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Mipmap"></param>
        /// <returns></returns>
        public static int GetCalculatedDataSize(this GXImageFormat Format, int Width, int Height, int Mipmap)
        {
            Width = Math.Max(1, Width >> Mipmap);
            Height = Math.Max(1, Height >> Mipmap);
            return GetCalculatedDataSize(Format, Width, Height);
        }

        /// <summary>
        /// Calculates the size of the image in bytes
        /// </summary>
        /// <param name="Format"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        public static int GetCalculatedDataSize(this GXImageFormat Format, int Width, int Height)
        {
            while ((Width % BlockWidth[(int)Format]) != 0) Width++;
            while ((Height % BlockHeight[(int)Format]) != 0) Height++;
            return Width * Height * GetBpp(Format) / 8;
        }
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
                Size -= GetCalculatedDataSize(Format, Width, Height);
                Width /= 2;
                Height /= 2;
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
            if (Format.IsPaletteFormat())
                throw new FormatException($"{Format} Is not a Palette format!");
            return MaxTlutColours[(int)Format-8];
        }

        /// <summary>
        /// get the maximum pallet size in bit.
        /// </summary>
        /// <param name="Format"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static int GetMaxPaletteSize(this GXImageFormat Format)
            => Format.GetMaxPaletteSize() * 2;

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
