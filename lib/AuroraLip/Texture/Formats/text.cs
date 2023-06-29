using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class text_AQ : JUTTexture, IMagicIdentify, IFileAccess
    {
        public virtual bool CanRead => true;

        public virtual bool CanWrite => false;

        public virtual string Magic => magic;

        private const string magic = "chnkdata";
        private const string Platform = "wii ";
        private const string Type = "text";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension.ToLower() != ".pk" && stream.Length > 0x40 && stream.MatchString(magic) && stream.ReadInt32() == 0 && stream.MatchString(Platform) && stream.MatchString(Type);

        protected override void Read(Stream stream)
        {
            stream.Seek(0x10, SeekOrigin.Current);
            Header header = stream.Read<Header>(Endian.Big);

            Add(new TexEntry(stream, null, header.Format, GXPaletteFormat.IA8, 0, (int)header.Width, (int)header.Height, (int)header.Images - 1)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = header.Images - 1
            });
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct Header
        {
            public uint Magic; // "text" 1952807028
            public uint pad; // 0x0
            public uint HeaderPos; // 0x10
            public uint Size;

            private uint format;
            public uint Width;
            public uint Height;
            public uint Images;

            public uint Unk3; // 56
            public uint HeadeSize; // 64
            public uint Unk5; // 13421772
            public uint Unk6; // 3435973836

            public GXImageFormat Format
            {
                get => (GXImageFormat)format;
                set => format = (uint)value;
            }
        }
    }
}
