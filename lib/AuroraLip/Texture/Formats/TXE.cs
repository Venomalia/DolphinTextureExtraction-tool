using AuroraLip.Common;
using System;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    // https://pikmintkb.com/wiki/TXE_file
    public class TXE : JUTTexture
    {
        public TXE() { }

        public TXE(Stream stream) : base(stream) { }

        public TXE(string filepath) : base(filepath) { }

        protected override void Read(Stream stream)
        {
            ushort ImageWidth = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            ushort ImageHeight = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            short Unknown = BitConverter.ToInt16(stream.ReadBigEndian(2), 0);
            ushort Tex_Format = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            GXImageFormat Format = TEX_ImageFormat[Tex_Format];
            //We use our own calculation
            int DataSize = BitConverter.ToInt32(stream.ReadBigEndian(4), 0);

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
