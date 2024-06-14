using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class TXTR : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".txtr";

        public TXTR()
        { }

        public TXTR(Stream stream) : base(stream)
        {
        }

        public TXTR(string filepath) : base(filepath)
        {
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase);

        protected override void Read(Stream stream)
        {
            TXTRImageFormat TXTRFormat = (TXTRImageFormat)stream.ReadUInt32(Endian.Big);
            GXImageFormat Format = (GXImageFormat)Enum.Parse(typeof(GXImageFormat), TXTRFormat.ToString());
            int ImageWidth = stream.ReadUInt16(Endian.Big);
            int ImageHeight = stream.ReadUInt16(Endian.Big);
            uint Images = stream.ReadUInt32(Endian.Big);

            Span<byte> palettedata = Span<byte>.Empty;
            int ColorsCount = 0;
            GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8;
            if (Format.IsPaletteFormat())
            {
                PaletteFormat = (GXPaletteFormat)stream.ReadUInt32(Endian.Big);
                int CWidth = stream.ReadUInt16(Endian.Big);
                int CHeight = stream.ReadUInt16(Endian.Big);
                ColorsCount = CHeight * CWidth;
                palettedata = new byte[ColorsCount * 2];
                stream.Read(palettedata);
            }

            TexEntry current = new TexEntry(stream, palettedata, Format, PaletteFormat, ColorsCount, ImageWidth, ImageHeight, (int)Images - 1)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = Images - 1
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
