using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Texture.Formats
{
    // From: https://forum.xentax.com/viewtopic.php?f=18&t=17220
    // Thanks to Acewell
    public class TEX_RFS : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Extension => ".tex";

        public TEX_RFS() { }

        public TEX_RFS(Stream stream) : base(stream) { }

        public TEX_RFS(string filepath) : base(filepath) { }

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (!extension.ToLower().StartsWith(".tex"))
                return false;

            stream.Seek(0x18, SeekOrigin.Begin);
            uint Tex_Format = stream.ReadUInt32(Endian.Little);

            if (!TEX_ImageFormat.ContainsKey(Tex_Format))
                return false;

            uint ImageWidth = stream.ReadUInt32(Endian.Little);
            uint ImageHeight = stream.ReadUInt32(Endian.Little);
            stream.Seek(0x34, SeekOrigin.Begin);
            return ImageWidth > 1 && ImageWidth <= 1024 && ImageHeight >= 1 && ImageHeight <= 1024 && TEX_ImageFormat[Tex_Format].GetCalculatedDataSize( (int)ImageWidth, (int)ImageHeight) < stream.Length;
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, in extension);

        protected override void Read(Stream stream)
        {
            stream.Seek(0x18, SeekOrigin.Begin);
            uint Tex_Format = stream.ReadUInt32(Endian.Little);

            GXImageFormat Format = TEX_ImageFormat[Tex_Format];
            uint ImageWidth = stream.ReadUInt32(Endian.Little);
            uint ImageHeight = stream.ReadUInt32(Endian.Little);

            stream.Seek(0x34, SeekOrigin.Begin);


            TexEntry current = new TexEntry(stream, null, Format, GXPaletteFormat.IA8, 0, (int)ImageWidth, (int)ImageHeight)
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

        static Dictionary<uint, GXImageFormat> TEX_ImageFormat = new Dictionary<uint, GXImageFormat>
        {
            { 0x11, GXImageFormat.RGBA32 },
            { 0x13, GXImageFormat.I8 },
            { 0x35, GXImageFormat.I4 },
            { 0x0b, GXImageFormat.CMPR }
        };

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
