namespace AuroraLip.Texture.J3D
{
    public static partial class JUtility
    {
        /// <summary>
        /// PaletteFormat specifies how the data within the palette is stored.
        /// Only C4, C8, and C14X2 use palettes. For all other formats the type is zero.
        /// </summary>
        public enum GXPaletteFormat : byte
        {
            /// <summary>
            /// Greyscale + Alpha - 16 bits/pixel (bpp) | Block Width: 4 | Block height: 4 | Block size: 32 bytes
            /// </summary>
            IA8 = 0x00,
            /// <summary>
            /// Colour - 16 bits/pixel (bpp) | Block Width: 4 | Block height: 4 | Block size: 32 bytes
            /// </summary>
            RGB565 = 0x01,
            /// <summary>
            /// Colour + Alpha - 16 bits/pixel (bpp) | Block Width: 4 | Block height: 4 | Block size: 32 bytes
            /// </summary>
            RGB5A3 = 0x02
        }
    }
}
