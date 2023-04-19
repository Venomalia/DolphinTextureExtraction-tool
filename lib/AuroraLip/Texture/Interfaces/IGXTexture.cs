using AuroraLib.Texture;

namespace AuroraLip.Texture.Interfaces
{
    /// <summary>
    /// Defines the properties of a texture used by the GX API in the GameCube and Wii consoles.
    /// </summary>
    public interface IGXTexture : IImage
    {
        /// <summary>
        /// The image format of the texture.
        /// specifies how the data within the image is encoded.
        /// </summary>
        GXImageFormat Format { get; }

        /// <summary>
        /// The palette format of the texture.
        /// Only C4, C8, and C14X2 use palettes.
        /// </summary>
        GXPaletteFormat PaletteFormat { get; }

        /// <summary>
        /// The wrap mode for the S coordinate.
        /// Specifies how textures outside the vertical range [0..1] are treated for text coordinates.
        /// </summary>
        GXWrapMode WrapS { get; set; }

        /// <summary>
        /// The wrap mode for the T coordinate.
        /// Specifies how textures outside the horizontal range [0..1] are treated for text coordinates.
        /// </summary>
        GXWrapMode WrapT { get; set; }

        /// <summary>
        /// The magnification filter mode for the texture.
        /// specifies what type of filtering the file should use as magnification filter.
        /// </summary>
        GXFilterMode MagnificationFilter { get; set; }

        /// <summary>
        /// The minification filter mode for the texture.
        /// specifies what type of filtering the file should use as minification filter.
        /// </summary>
        GXFilterMode MinificationFilter { get; set; }

        /// <summary>
        /// The minimum level-of-detail for the texture.
        /// Exclude textures below a certain LOD level from being used.
        /// </summary>
        float MinLOD { get; set; }

        /// <summary>
        /// The maximum level-of-detail for the texture.
        /// Exclude textures above a certain LOD level from being used.
        /// A value larger than the actual textures should lead to culling.
        /// </summary>
        float MaxLOD { get; set; }

        /// <summary>
        /// The level-of-detail bias for the texture.
        /// A larger value leads to a larger camera distance before a lower LOD resolution is selected.
        /// </summary>
        float LODBias { get; set; }

        /// <summary>
        /// Indicating whether to enable edge level of detail (LOD) on the texture.
        /// When enabled, the LOD level is adjusted on the viewer distance to the texture's edges, resulting in smoother transitions.
        /// </summary>
        bool EnableEdgeLOD { get; set; }

        /// <summary>
        /// The number of mipmaps for the texture.
        /// </summary>
        int MipMaps { get; }

    }

}
