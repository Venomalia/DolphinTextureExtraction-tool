using AuroraLib.Common;
using AuroraLib.Texture;

namespace AuroraLib.Palette.Formats
{
    public class PLT0 : IMagicIdentify, IJUTPalette
    {
        public string Magic => magic;

        public GXPaletteFormat Format { get; set; }

        public byte[] Data { get; set; }

        private const string magic = "PLT0";

        public PLT0(Stream stream) => Read(stream);

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
            Data = stream.Read(colors * 2);
        }
    }
}
