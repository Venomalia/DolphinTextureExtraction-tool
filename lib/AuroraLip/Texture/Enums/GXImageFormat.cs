namespace AuroraLib.Texture
{
    /// <summary>
    /// ImageFormat specifies how the data within the image is encoded.
    /// Included is a chart of how many bits per pixel there are,
    /// the width/height of each block, how many bytes long the
    /// actual block is, and a description of the type of data stored.
    /// </summary>
    public enum GXImageFormat : byte
    {
        /// <summary>
        /// 4 bit intensity values. The alpha component is set to 0xff.
        /// Greyscale - 4 bits/pixel (bpp) | Block Width: 8 | Block height: 8 | Block size: 32 bytes
        /// </summary>
        I4 = 0x00,

        /// <summary>
        /// 8 bit intensity values. The alpha component is set to 0xff.
        /// Greyscale - 8 bits/pixel (bpp) | Block Width: 8 | Block height: 4 | Block size: 32 bytes
        /// </summary>
        I8 = 0x01,

        /// <summary>
        /// 4 bit intensity values, along with a separate alpha channel
        /// Greyscale + Alpha - 8 bits/pixel (bpp) | Block Width: 8 | Block height: 4 | Block size: 32 bytes
        /// </summary>
        IA4 = 0x02,

        /// <summary>
        /// 8 bit intensity values, along with a separate alpha channel.
        /// Greyscale + Alpha - 16 bits/pixel (bpp) | Block Width: 4 | Block height: 4 | Block size: 32 bytes
        /// </summary>
        IA8 = 0x03,

        /// <summary>
        /// 16 bit color values without alpha. The alpha component is set to 0xff.
        /// Colour - 16 bits/pixel (bpp) | Block Width: 4 | Block height: 4 | Block size: 32 bytes
        /// </summary>
        RGB565 = 0x04,

        /// <summary>
        /// either 15 bit color values without alpha, or 12 bit color values with a 3 bit alpha channel.
        /// Colour + Alpha - 16 bits/pixel (bpp) | Block Width: 4 | Block height: 4 | Block size: 32 bytes
        /// </summary>
        RGB5A3 = 0x05,

        /// <summary>
        /// 24 bit depth true color, with an 8 bit alpha channel.
        /// Colour + Alpha - 32 bits/pixel (bpp) | Block Width: 4 | Block height: 4 | Block size: 64 bytes
        /// </summary>
        RGBA32 = 0x06,

        /// <summary>
        /// Is a 4 bit palette format. Supports up to 16 different colors.
        /// Palette - 4 bits/pixel (bpp) | Block Width: 8 | Block Height: 8 | Block size: 32 bytes
        /// </summary>
        C4 = 0x08,

        /// <summary>
        /// Is an 8 bit palette format. Supports up to 256 different colors.
        /// Palette - 8 bits/pixel (bpp) | Block Width: 8 | Block Height: 4 | Block size: 32 bytes
        /// </summary>
        C8 = 0x09,

        /// <summary>
        /// Is a 14 bit palette format. Supports up to 16384 different colors.
        /// Palette - 14 bits/pixel (bpp) | Block Width: 4 | Block Height: 4 | Block size: 32 bytes
        /// </summary>
        C14X2 = 0x0A,

        /// <summary>
        /// Compressed image format that uses a DXT1 algorithm.
        /// Colour + Alpha (1 bit) - 4 bits/pixel (bpp) | Block Width: 8 | Block height: 8 | Block size: 32 bytes
        /// </summary>
        CMPR = 0x0E
    }
}
