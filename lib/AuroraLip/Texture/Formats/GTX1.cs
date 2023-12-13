using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Texture.Formats
{
    public class GTX1 : JUTTexture, IHasIdentifier, IFileAccess
    {
        public virtual bool CanRead => true;

        public virtual bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("GTX1");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x10 && stream.At(4, s => s.Match(_identifier));

        protected override void Read(Stream stream)
        {
            Header header = stream.Read<Header>(Endian.Big);

            //hack to get mips i do not trust the header.mip value
            int mips = header.Format.GetMipmapsFromSize((int)header.Size, header.Width, header.Height) - 1;

            Add(new(stream, Span<byte>.Empty, header.Format, GXPaletteFormat.IA8, 0, header.Width, header.Height, mips)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = mips
            });
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct Header
        {
            public uint Size;
            public Identifier32 Identifier; //GTX1
            public byte PahtID;
            public byte ImageID;
            public GXImageFormat Format;
            public byte Unk1;//255
            public ushort Width;
            public ushort Height;

            public uint Unk2;//0
            public byte Unk3;//0 80
            public byte data;
            public ushort Unk5; // 0 256
            public Identifier64 Name; //loobFrab

            public int Unk4 //1 2 3 11
            {
                get => (data >> 4);
                set => data = (byte)((data & 0x0F) | (value << 4));
            }
            public int Mips
            {
                get => (data & 0xF);
                set => data = (byte)((data & 0xF0) | value);
            }
        }
    }
}
