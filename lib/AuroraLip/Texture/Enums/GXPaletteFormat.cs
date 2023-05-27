namespace AuroraLib.Texture
{
    /// <summary>
    /// PaletteFormat specifies how the data within the palette is stored.
    /// Only C4, C8, and C14X2 use palettes. For all other formats the type is zero.
    /// </summary>
    public enum GXPaletteFormat : byte
    {
        /// <summary>
        /// The IA8 format is used for storing 8 bit intensity values, along with a separate alpha channel.
        /// Greyscale + Alpha - 16 bits/pixel (bpp) | Block Width: 4 | Block height: 4 | Block size: 32 bytes
        /// </summary>
        IA8 = 0x00,

        /// <summary>
        /// 16 bit color values without alpha. alpha use 0xff.
        /// Colour - 16 bits/pixel (bpp) | Block Width: 4 | Block height: 4 | Block size: 32 bytes
        /// </summary>
        RGB565 = 0x01,

        /// <summary>
        /// It is used for storing either 15 bit color values without alpha, or 12 bit color values with a 3 bit alpha channel.
        /// Colour + Alpha - 16 bits/pixel (bpp) | Block Width: 4 | Block height: 4 | Block size: 32 bytes
        /// </summary>
        RGB5A3 = 0x02
    }
}
