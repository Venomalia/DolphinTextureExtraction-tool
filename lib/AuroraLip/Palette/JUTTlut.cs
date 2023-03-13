using AuroraLib.Texture;
using System.Drawing;
using static AuroraLib.Texture.J3DTextureConverter;

namespace AuroraLib.Palette
{
    public class JUTPalette : List<Color>
    {
        /// <summary>
        /// specifies how the data within the palette is stored.
        /// </summary>
        public GXPaletteFormat Format { get; set; }

        /// <summary>
        /// The size in bits.
        /// </summary>
        public int Size => this.Count * 2;

        public JUTPalette(GXPaletteFormat format = GXPaletteFormat.IA8) : base()
            => Format = format;

        public JUTPalette(GXPaletteFormat format, IEnumerable<Color> collection) : base(collection)
            => Format = format;

        public JUTPalette(GXPaletteFormat format, ReadOnlySpan<byte> PaletteData, int colors) : base(DecodePalette(PaletteData, format, colors))
            => Format = format;

        public byte[] GetBytes() => EncodePalette(this, this.Format);

        public static explicit operator byte[](JUTPalette x) => EncodePalette(x, x.Format);

        public override bool Equals(object obj)
        {
            return obj is JUTPalette entry && Format == entry.Format && base.Equals(entry);
        }

        public override int GetHashCode()
        {
            int hashCode = -1146949837;
            hashCode = hashCode * -1521134295 + Format.GetHashCode();
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            return hashCode;
        }
    }
}
