namespace AuroraLib.Texture
{
    public enum AImageFormats : uint
    {
        /// <inheritdoc cref="GXImageFormat"/>
        I4 = 0x00,
        I8 = 0x01,
        IA4 = 0x02,
        IA8 = 0x03,
        RGB565 = 0x04,
        RGB5A3 = 0x05,
        RGBA32 = 0x06,
        C4 = 0x08,
        C8 = 0x09,
        C14X2 = 0x0A,
        CMPR = 0x0E,

        //DDS 
        DXT1 = 0xC000000E,
    }
}
