using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Texture.Formats
{
    public class ALIG : JUTTexture, IHasIdentifier, IFileAccess
    {
        public virtual bool CanRead => true;

        public virtual bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("ALIG");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(Identifier);

        protected override void Read(Stream stream)
        {
            long start = stream.Position;
            stream.MatchThrow(_identifier);

            TypeHeader typeH = stream.Read<TypeHeader>();
            GXImageFormat format = AsGXFormat(typeH.Format);
            switch (typeH.Palette)
            {
                case PaletteFormat.None:
                    BaseHeader header = stream.Read<BaseHeader>();
                    stream.Seek(start + header.ImageOffset, SeekOrigin.Begin);


                    Add(new TexEntry(stream, format, header.Width, header.Height, header.Mips)
                    {
                        LODBias = 0,
                        MagnificationFilter = GXFilterMode.Nearest,
                        MinificationFilter = GXFilterMode.Nearest,
                        WrapS = GXWrapMode.CLAMP,
                        WrapT = GXWrapMode.CLAMP,
                        EnableEdgeLOD = false,
                        MinLOD = 0,
                        MaxLOD = header.Mips
                    });

                    if (typeH.Format == ImageFormat.GACC)
                    {
                        Add(new TexEntry(stream, format, header.Width, header.Height, header.Mips)
                        {
                            LODBias = 0,
                            MagnificationFilter = GXFilterMode.Nearest,
                            MinificationFilter = GXFilterMode.Nearest,
                            WrapS = GXWrapMode.CLAMP,
                            WrapT = GXWrapMode.CLAMP,
                            EnableEdgeLOD = false,
                            MinLOD = 0,
                            MaxLOD = header.Mips
                        });
                    }
                    break;
                case PaletteFormat.RGBA:
                default:
                    throw new NotSupportedException();
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct TypeHeader
        {
            public MipmapType Mipmap;
            public ImageFlags Flags;
            public ushort PaletteColors;
            public ImageFormat Format;
            public PaletteFormat Palette;
        }

        private struct BaseHeader
        {
            public ushort Width;
            public ushort Height;
            public int Images;
            public int PaletteOffset;// 0x10
            public int ImageOffset; // 0x40

            private uint size;
            public int Unk3;
            public int Unk4;
            public ushort Un5;
            public ushort Mips;

            public uint Size
            {
                readonly get => BitConverterX.Swap(size);
                set => size = BitConverterX.Swap(value);
            }
        }

        private struct PaletteHeader
        {
            public int Unk;
            public int Unk2;
            public ushort Width;
            public ushort Height;
            public int Unk3;
        }

        private enum ImageFormat : uint
        {
            GCI4 = 877216583,//GameCube I4
            GCI8 = 944325447,//GameCube I8
            GIA4 = 876693831,//GameCube IA4
            GIA8 = 943802695,//GameCube IA8
            G565 = 892745031,//GameCube RGB565
            GACC = 1128481095,//GameCube 2xCMPR
            GCCP = 1346585415,//GameCube CMPR
            G5A3 = 859911495, //GameCube RGB5A3
            GC32 = 842220359,//GameCube RGBA32
            PAL4 = 877412688,//C4
            PAL8 = 944521552,//C8
            BGRA = 1095911234,//BGRA32
            ETC1 = 826496069,//Ericsson Texture Compression
            EC1A = 1093747525,//Ericsson Texture Compression
        }

        private enum PaletteFormat : uint
        {
            None = 0,
            RGBA = 1094862674,
        }

        public enum MipmapType : byte
        {
            NoMipmap = 0,
            HasMipmap = 1,
            Platform = 2
        }

        [Flags]
        public enum ImageFlags : byte
        {
            None = 0,
            HasAlpha = 1,
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
            ImageFormat.GC32 => GXImageFormat.RGBA32,
            ImageFormat.PAL4 => GXImageFormat.I4,
            ImageFormat.PAL8 => throw new NotImplementedException($"PAL8"),
            _ => throw new NotImplementedException(mode.ToString()),
        };
    }
}
