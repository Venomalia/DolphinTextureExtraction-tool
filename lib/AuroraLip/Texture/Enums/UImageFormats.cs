namespace AuroraLib.Texture
{
    public enum AImageFormats : uint
    {
        /// <inheritdoc cref="GXImageFormat.I4"/>
        GXI4 = 0x00,
        /// <inheritdoc cref="GXImageFormat.I8"/>
        GXI8 = 0x01,
        /// <inheritdoc cref="GXImageFormat.IA4"/>
        GXIA4 = 0x02,
        /// <inheritdoc cref="GXImageFormat.IA8"/>
        GXIA8 = 0x03,
        /// <inheritdoc cref="GXImageFormat.RGB565"/>
        GXRGB565 = 0x04,
        /// <inheritdoc cref="GXImageFormat.RGB5A3"/>
        GXRGB5A3 = 0x05,
        /// <inheritdoc cref="GXImageFormat.RGBA32"/>
        GXRGBA32 = 0x06,
        /// <inheritdoc cref="GXImageFormat.C4"/>
        GXC4 = 0x08,
        /// <inheritdoc cref="GXImageFormat.C8"/>
        GXC8 = 0x09,
        /// <inheritdoc cref="GXImageFormat.C14X2"/>
        GXC14X2 = 0x0A,
        /// <inheritdoc cref="GXImageFormat.CMPR"/>
        CMPR = 0x0E,

        /// <summary>
        /// 4 bit intensity values.
        /// </summary>
        I4 = 0xC0000000,
        /// <summary>
        /// 8 bit intensity values.
        /// </summary>
        I8 = 0xC0000001,
        /// <summary>
        /// 4 bit intensity values, along with a 8 bit alpha channel
        /// </summary>
        IA4 = 0xC0000002,
        /// <summary>
        /// 8 bit intensity values, along with a 8 bit alpha channel.
        /// </summary>
        IA8 = 0xC0000003,
        /// <summary>
        /// 16 bit color values without alpha. The alpha component is set to 0xff.
        /// </summary>
        RGB565 = 0xC0000004,
        /// <summary>
        /// Either 15 bit color values without alpha, or 12 bit color values with a 3 bit alpha channel.
        /// </summary>
        RGB5A3 = 0xC0000005,
        /// <summary>
        /// 24 bit depth true color, with an 8 bit alpha channel.
        /// </summary>
        RGBA32 = 0xC0000006,
        /// <summary>
        /// Is a 4 bit palette format. Supports up to 16 different colors.
        /// </summary>
        C4 = 0xC0000008,
        /// <summary>
        /// Is an 8 bit palette format. Supports up to 256 different colors.
        /// </summary>
        C8 = 0xC0000009,
        /// <summary>
        /// Is a 14 bit palette format. Supports up to 16384 different colors.
        /// </summary>
        C14X2 = 0xC000000A,
        /// <summary>
        /// Compressed image format.
        /// </summary>
        DXT1 = 0xC000000E,
    }
}
