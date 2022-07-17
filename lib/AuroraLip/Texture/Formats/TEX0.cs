using AuroraLip.Common;
using AuroraLip.Texture.J3D;
using System;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    public class TEX0 : JUTTexture, IMagicIdentify, IFileAccess
    {

        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        private const string magic = "TEX0";

        public TEX0() { }

        public TEX0(Stream stream) : base(stream) { }

        public TEX0(string filepath) : base(filepath) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream) => Read(stream, null, GXPaletteFormat.IA8, 0);

        protected void Read(Stream stream, byte[] PaletteData, GXPaletteFormat PaletteFormat, int PaletteCount)
        {
            if (!stream.MatchString(magic))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");
            uint TotalSize = stream.ReadUInt32(Endian.Big);
            uint FormatVersion = stream.ReadUInt32(Endian.Big);
            uint Offset = stream.ReadUInt32(Endian.Big);
            long SectionOffsets;
            if (FormatVersion == 2)
            {
                SectionOffsets = (long)stream.ReadUInt64(Endian.Big);
            }
            else
            {
                SectionOffsets = stream.ReadUInt32(Endian.Big);
            }
            uint StringOffset = stream.ReadUInt32(Endian.Big);
            //TEX0 Header
            uint Unknown = stream.ReadUInt32(Endian.Big);
            int ImageWidth = stream.ReadUInt16(Endian.Big);
            int ImageHeight = stream.ReadUInt16(Endian.Big);
            GXImageFormat Format = (GXImageFormat)stream.ReadUInt32(Endian.Big);
            int TotalImageCount = stream.ReadUInt16(Endian.Big);
            uint Unknown2 = stream.ReadUInt32(Endian.Big);
            uint Mipmaps = stream.ReadUInt32(Endian.Big);
            uint Unknown3 = stream.ReadUInt32(Endian.Big);
            stream.Position = SectionOffsets;

            if (PaletteData == null && JUtility.IsPaletteFormat(Format))
            {
#if DEBUG
                return;
#else
                throw new Exception("TEX0 palette formats are not yet supported");
#endif
            }
            Add(new TexEntry(stream, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight, TotalImageCount - 1)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = 0
            });
        }


        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
