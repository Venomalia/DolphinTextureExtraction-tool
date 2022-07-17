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
            uint TotalSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            uint FormatVersion = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            uint Offset = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            long SectionOffsets;
            if (FormatVersion == 2)
            {
                SectionOffsets = (long)BitConverter.ToUInt64(stream.ReadBigEndian(8), 0);
            }
            else
            {
                SectionOffsets = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            }
            uint StringOffset = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            //TEX0 Header
            uint Unknown = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            int ImageWidth = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            int ImageHeight = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            GXImageFormat Format = (GXImageFormat)BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            int TotalImageCount = BitConverter.ToUInt16(stream.ReadBigEndian(4), 0);
            uint Unknown2 = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            uint Mipmaps = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            uint Unknown3 = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
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
