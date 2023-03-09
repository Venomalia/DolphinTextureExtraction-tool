using AuroraLip.Common;
using AuroraLip.Texture;
using System.Drawing;
using static AuroraLip.Texture.J3DTextureConverter;

namespace AuroraLip.Palette.Formats
{
    public class PLT0 : JUTPalette, IMagicIdentify
    {
        public string Magic => magic;

        private const string magic = "PLT0";

        public PLT0(Stream stream) => Read(stream);

        public PLT0(GXPaletteFormat format = GXPaletteFormat.IA8) : base(format) { }

        public PLT0(GXPaletteFormat format, IEnumerable<Color> collection) : base(format, collection) { }

        public PLT0(GXPaletteFormat format, ReadOnlySpan<byte> PaletteData, int colors) : base(format, PaletteData, colors) { }

        protected void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);
            uint TotalSize = stream.ReadUInt32(Endian.Big);
            uint FormatVersion = stream.ReadUInt32(Endian.Big);
            uint Offset = stream.ReadUInt32(Endian.Big);

            uint SectionOffsets = stream.ReadUInt32(Endian.Big);
            uint StringOffset = stream.ReadUInt32(Endian.Big);
            Format = (GXPaletteFormat)stream.ReadUInt32(Endian.Big);
            short colors = stream.ReadInt16(Endian.Big);
            ushort pad = stream.ReadUInt16(Endian.Big);
            uint PathOffset = stream.ReadUInt32(Endian.Big);
            uint DataOffset = stream.ReadUInt32(Endian.Big);
            stream.Seek(SectionOffsets, SeekOrigin.Begin);
            byte[] PaletteData = stream.Read(colors * 2);
            this.AddRange(DecodePalette(PaletteData, Format, colors));
        }
    }
}
