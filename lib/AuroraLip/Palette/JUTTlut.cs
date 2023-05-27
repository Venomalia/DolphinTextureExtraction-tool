using AuroraLib.Texture;
using SixLabors.ImageSharp.PixelFormats;

namespace AuroraLib.Palette
{
    public interface IJUTPalette
    {
        /// <summary>
        /// specifies how the data within the palette is stored.
        /// </summary>
        GXPaletteFormat Format { get; }

        /// <summary>
        /// Palette data
        /// </summary>
        byte[] Data { get; }
    }
}
