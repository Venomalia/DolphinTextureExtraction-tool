using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    internal class ALIG : JUTTexture, IMagicIdentify, IFileAccess
    {
        public virtual bool CanRead => true;

        public virtual bool CanWrite => false;

        public virtual string Magic => magic;

        private const string magic = "ALIG";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(Magic);

        protected override void Read(Stream stream)
        {
            long start = stream.Position;

            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(magic);

            Header header = stream.Read<Header>();
            stream.Seek(start + header.Offset, SeekOrigin.Begin);

            GXImageFormat format = AsGXFormat(header.Format);

            Add(new TexEntry(stream, null, format, GXPaletteFormat.IA8, 0, header.Width, header.Height, header.Mips)
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

            if (header.Format == ImageFormat.GACC)
            {
                Add(new TexEntry(stream, null, format, GXPaletteFormat.IA8, 0, header.Width, header.Height, header.Mips)
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
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct Header
        {
            public int Unk;
            public ImageFormat Format;
            public int Pad;

            public ushort Width;
            public ushort Height;
            public int Images;
            public int Unk2;// 0x10
            public int Offset; // 0x40

            private uint size;
            public int Unk3;
            public int Unk4;
            public ushort Un5;
            public ushort Mips;

            private uint Size
            {
                get => size.Swap();
                set => size = value.Swap();
            }
        }

        private enum ImageFormat
        {
            GCI4 = 877216583,//I4
            GCI8 = 944325447,//I8
            GIA4 = 876693831,//IA4
            GIA8 = 943802695,//IA8
            G565 = 892745031,//RGB565
            GACC = 1128481095,//2xCMPR
            GCCP = 1346585415,//CMPR
            G5A3 = 859911495, //RGB5A3
        }

        private static GXImageFormat AsGXFormat(ImageFormat mode) => mode switch
        {
            ImageFormat.GCI4 => GXImageFormat.I4,
            ImageFormat.GCI8 => GXImageFormat.I8,
            ImageFormat.GIA4 => GXImageFormat.IA4,
            ImageFormat.GIA8 => GXImageFormat.IA8,
            ImageFormat.GACC => GXImageFormat.CMPR,
            ImageFormat.GCCP => GXImageFormat.CMPR,
            ImageFormat.G5A3 => GXImageFormat.RGB5A3,
            ImageFormat.G565 => GXImageFormat.RGB565,
            _ => throw new NotImplementedException(),
        };
    }
}
