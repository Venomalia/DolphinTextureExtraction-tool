using AuroraLib.Common;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Texture.Formats
{
    public class TDL0 : JUTTexture, IFileAccess, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => Magic;

        public static readonly Identifier32 Magic = new("TDL0");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x40 && stream.Match(Magic);

        protected override void Read(Stream stream)
        {
            Header header = stream.Read<Header>(Endian.Big);
            using SpanBuffer<byte> palette = new(header.PaletteSize * header.TextureCount);
            if (header.Format.IsPaletteFormat())
                stream.At(header.PaletteStart, s => s.Read(palette));

            for (int i = 0; i < header.TextureCount; i++)
            {
                TextureHeader texture = stream.Read<TextureHeader>(Endian.Big);

                stream.At(header.DataStart, s =>
                {
                    TexEntry current = new(stream, palette.Slice(header.PaletteSize * i, header.PaletteSize), header.Format, GXPaletteFormat.RGB5A3, header.PaletteSize / 2, header.TotalWidth, header.TotalHeight, header.MipmapCount)
                    {
                        MinLOD = 0,
                        MaxLOD = header.MipmapCount
                    };
                    Add(current);
                });
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct Header
        {
            public readonly Identifier32 Magic;
            public readonly int unk;
            public readonly ushort TotalWidth;
            public readonly ushort TotalHeight;
            public readonly ushort TextureCount;
            public readonly ushort MipmapCount;
            private readonly byte format;
            public readonly byte unk02;
            public readonly ushort PaletteSize;
            public readonly uint DataStart;
            public readonly uint PaletteStart;

            public readonly GXImageFormat Format => format switch
            {
                4 => GXImageFormat.C4,
                5 => GXImageFormat.C8,
                8 => GXImageFormat.RGB5A3,
                10 => GXImageFormat.CMPR,
                _ => throw new NotImplementedException(),
            };
        }

        private struct TextureHeader
        {
            public readonly int ID;
            public readonly ushort Width;
            public readonly ushort Height;
            public readonly ushort OffsetWidth;
            public readonly ushort OffsetHeight;
        }
    }
}
