using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class text_AQ : JUTTexture, IHasIdentifier, IFileAccess
    {
        public virtual bool CanRead => true;

        public virtual bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly byte[] _bytes = new byte[] { 0x63, 0x68, 0x6E, 0x6B, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00, 0x00, 0x77, 0x69, 0x69, 0x20, 0x74, 0x65, 0x78, 0x74 };

        private static readonly Identifier _identifier = new(_bytes);

        //public static readonly Identifier32 Platform = new(_identifierbyte.AsSpan().Slice(12, 4)); // Wii
        //public static readonly Identifier32 Type = new(_identifierbyte.AsSpan().Slice(16, 4)); // text

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension.ToLower() != ".pk" && stream.Length > 0x40 && stream.Match(_identifier);

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
