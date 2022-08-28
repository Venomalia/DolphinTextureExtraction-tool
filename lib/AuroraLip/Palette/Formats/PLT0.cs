using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Palette.Formats
{
    public class PLT0: IMagicIdentify
    {
        public string Magic => magic;

        private const string magic = "PLT0";

        public GXPaletteFormat PaletteFormat { get; set; }

        public byte[] PaletteData { get; set; }

        public PLT0() {}

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
            PaletteFormat = (GXPaletteFormat)stream.ReadUInt32(Endian.Big);
            short colors = stream.ReadInt16(Endian.Big);
            ushort pad = stream.ReadUInt16(Endian.Big);
            uint PathOffset = stream.ReadUInt32(Endian.Big);
            uint DataOffset = stream.ReadUInt32(Endian.Big);
            stream.Seek(SectionOffsets,SeekOrigin.Begin);
            PaletteData = stream.Read(colors*2);
        }
    }
}
