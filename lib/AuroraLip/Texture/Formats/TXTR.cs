using AuroraLip.Common;
using AuroraLip.Texture.J3D;
using System;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    public class TXTR : JUTTexture
    {
        public TXTR() { }

        public TXTR(Stream stream) : base(stream) { }

        public TXTR(string filepath) : base(filepath) { }

        protected override void Read(Stream stream)
        {
            TXTRImageFormat TXTRFormat = (TXTRImageFormat)BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            GXImageFormat Format = (GXImageFormat)Enum.Parse(typeof(GXImageFormat), TXTRFormat.ToString());
            int ImageWidth = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            int ImageHeight = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            uint Images = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);

            byte[] palettedata = null;
            int ColorsCount = 0;
            GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8;
            if (JUtility.IsPaletteFormat(Format))
            {
                PaletteFormat = (GXPaletteFormat)BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                int CWidth = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                int CHeight = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                ColorsCount = CHeight* CWidth;
                palettedata = stream.Read(ColorsCount*2);
            }

            TexEntry current = new TexEntry(stream, palettedata, Format, PaletteFormat, ColorsCount, ImageWidth, ImageHeight, (int)Images-1)
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

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        public enum TXTRImageFormat : uint
        {
            I4 = 0x00,
            I8 = 0x01,
            IA4 = 0x02,
            IA8 = 0x03,
            C4 = 0x04,
            C8 = 0x05,
            C14X2 = 0x06,
            RGB565 = 0x07,
            RGB5A3 = 0x08,
            RGBA32 = 0x09, //RGBA8?
            CMPR = 0x0A,
        }

    }
}
