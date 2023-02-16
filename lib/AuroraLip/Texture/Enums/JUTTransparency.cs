namespace AuroraLip.Texture
{
    public enum JUTTransparency : byte
    {
        /// <summary>
        /// No Transperancy
        /// </summary>
        OPAQUE = 0x00,
        /// <summary>
        /// Only allows fully Transperant pixels to be see through
        /// </summary>
        CUTOUT = 0x01,
        /// <summary>
        /// Allows Partial Transperancy. Also known as XLUCENT
        /// </summary>
        TRANSLUCENT = 0x02,
        /// <summary>
        /// Unknown
        /// </summary>
        SPECIAL = 0xCC
    }
}
