namespace AuroraLip.Texture
{
    /// <summary>
    /// Defines how textures handle going out of [0..1] range for texcoords.
    /// </summary>
    public enum GXWrapMode : short
    {
        /// <summary>
        /// Clamps the texture to the last pixel at the edge.
        /// </summary>
        CLAMP = 0x00,
        /// <summary>
        /// Tiles the texture, creating a repeating pattern.
        /// </summary>
        REPEAT = 0x01,
        /// <summary>
        /// Tiles the texture, creating a repeating pattern by mirroring it at every integer boundary.
        /// </summary>
        MIRRORREAPEAT = 0x02
    }
}
