using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class GCNT : JUTTexture, IHasIdentifier, IFileAccess
    {
        public virtual bool CanRead => true;

        public virtual bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("GCNT");

        private const string alt = "SIZE";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier) || stream.At(8, s => s.Match(_identifier));

        protected override void Read(Stream stream)
        {
            long HeaderStart = stream.Position;

            if (!stream.Match(_identifier))
            {
                HeaderStart = stream.Position += 4;
                stream.MatchThrow(_identifier);
            }

            ImageHeader ImageHeader = stream.Read<ImageHeader>(Endian.Big);

            ReadOnlySpan<byte> PaletteData = null;
            if (ImageHeader.Format.IsPaletteFormat())
            {
                stream.Seek(HeaderStart + ImageHeader.Offset + ImageHeader.Size, SeekOrigin.Begin);
                PaletteData = stream.Read(ImageHeader.Format.GetMaxPaletteSize());
            }
            stream.Seek(HeaderStart + ImageHeader.Offset, SeekOrigin.Begin);

            TexEntry current = new(stream, PaletteData, ImageHeader.Format, ImageHeader.PaletteFormat, ImageHeader.Format.GetMaxPaletteColours(), ImageHeader.Width, ImageHeader.Height, ImageHeader.Mipmaps)
            {
                LODBias = 0,
                MagnificationFilter = default,
                MinificationFilter = default,
                WrapS = default,
                WrapT = default,
                EnableEdgeLOD = default,
                MinLOD = 0,
                MaxLOD = ImageHeader.Mipmaps
            };
            Add(current);
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct ImageHeader
        {
            public uint Unknown; //0x3
            public ushort Offset;
            public ushort Unknown10;
            public uint Size;
            public ushort Width;
            public ushort Height;
            public GXImageFormat Format;
            public GXPaletteFormat PaletteFormat;
            public byte Mipmaps;
            public byte Padding23;
            public uint Padding24;
            public uint Padding28;
        }
    }
}
