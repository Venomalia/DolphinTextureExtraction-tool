using AuroraLip.Common;
using System;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    // https://pikmintkb.com/wiki/TXE_file
    public class TXE : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Extension => ".txe";

        public TXE() { }

        public TXE(Stream stream) : base(stream) { }

        public TXE(string filepath) : base(filepath) { }

        public static bool Matcher(Stream stream, in string extension = "")
        {
            ushort ImageWidth = stream.ReadUInt16(Endian.Big);
            ushort ImageHeight = stream.ReadUInt16(Endian.Big);
            short Unknown = stream.ReadInt16(Endian.Big);
            ushort Tex_Format = stream.ReadUInt16(Endian.Big);
            return extension.ToLower().StartsWith(".txe") && ImageWidth > 1 && ImageWidth <= 1024 && ImageHeight >= 1 && ImageHeight <= 1024 && Tex_Format <= 7 && GetCalculatedDataSize(TEX_ImageFormat[Tex_Format], ImageWidth, ImageHeight) < stream.Length;
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, in extension);

        protected override void Read(Stream stream)
        {
            ushort ImageWidth = stream.ReadUInt16(Endian.Big);
            ushort ImageHeight = stream.ReadUInt16(Endian.Big);
            short Unknown = stream.ReadInt16(Endian.Big);
            ushort Tex_Format = stream.ReadUInt16(Endian.Big);
            GXImageFormat Format = TEX_ImageFormat[Tex_Format];
            //We use our own calculation
            int DataSize = stream.ReadInt32(Endian.Big);

            stream.Position = 32;
            TexEntry current = new TexEntry(stream, null, Format, GXPaletteFormat.IA8, 0, ImageWidth, ImageHeight)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = 0
            };
            Add(current);
        }

        static readonly GXImageFormat[] TEX_ImageFormat = new GXImageFormat[]
        {
            GXImageFormat.RGB5A3,
            GXImageFormat.CMPR,
            GXImageFormat.RGB565,
            GXImageFormat.I4,
            GXImageFormat.I8,
            GXImageFormat.IA4,
            GXImageFormat.IA8,
            GXImageFormat.RGBA32,
        };

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
