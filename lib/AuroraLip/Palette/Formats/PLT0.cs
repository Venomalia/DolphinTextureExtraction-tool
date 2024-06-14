using AuroraLib.Common;
using AuroraLib.Core.Interfaces;
using AuroraLib.Texture;

namespace AuroraLib.Palette.Formats
{
    public class PLT0 : IHasIdentifier, IJUTPalette
    {
        public GXPaletteFormat Format { get; set; }

        public byte[] Data { get; set; }

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("PLT0");

        public PLT0(Stream stream) => Read(stream);

        protected void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
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
            Data = new byte[colors * 2];
            stream.Read(Data);
        }
    }
}
