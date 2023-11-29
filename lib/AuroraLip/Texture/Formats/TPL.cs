using AuroraLib.Common;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;
using SevenZipExtractor;
using System.Runtime.CompilerServices;

namespace AuroraLib.Texture.Formats
{
    public class TPL : JUTTexture, IFileAccess, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public virtual IIdentifier Identifier => Magic;

        public static readonly Identifier32 Magic = new(0x00, 0x20, 0xAF, 0x30);

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 12 && stream.Match(Magic);

        public static int GetSize(Stream stream)
        {
            long HeaderStart = stream.Position;
            if (!stream.Match(Magic))
            {
                return -1;
            }

            int TotalImageCount = stream.ReadInt32(Endian.Big);
            int ImageOffsetTableOffset = stream.ReadInt32(Endian.Big);
            stream.Seek(HeaderStart + ImageOffsetTableOffset + 8 * (TotalImageCount - 1), SeekOrigin.Begin);
            ImageOffsetEntry lastImageOffset = stream.Read<ImageOffsetEntry>(Endian.Big);
            stream.Seek(HeaderStart + lastImageOffset.ImageHeaderOffset, SeekOrigin.Begin);
            ImageHeader imageHeader = stream.Read<ImageHeader>(Endian.Big);

            return (int)imageHeader.ImageDataAddress + imageHeader.Format.GetCalculatedTotalDataSize(imageHeader.Width, imageHeader.Height, imageHeader.MaxLOD);
        }

        public TPL() : base()
        {
        }

        public TPL(string filepath) : base(filepath)
        {
        }

        public TPL(Stream stream) => Read(stream);

        // Helper function to read a TPL stream
        // exposed so other classes that wish to read
        // a TPL can do so
        public static void ProcessStream(Stream stream, long HeaderStart, List<TexEntry> textures)
        {
            int TotalImageCount = stream.ReadInt32(Endian.Big);
            int ImageOffsetTableOffset = stream.ReadInt32(Endian.Big);

            stream.Seek(HeaderStart + ImageOffsetTableOffset, SeekOrigin.Begin);

            using SpanBuffer<ImageOffsetEntry> ImageOffsetTable = new(TotalImageCount);
            stream.Read(ImageOffsetTable.Span, Endian.Big);

            for (int i = 0; i < ImageOffsetTable.Length; i++)
            {
                byte[] PaletteData = null;
                PaletteHeader paletteHeader = default;
                if (ImageOffsetTable[i].HasPaletteData)
                {
                    stream.Seek(HeaderStart + ImageOffsetTable[i].PaletteHeaderOffset, SeekOrigin.Begin);
                    paletteHeader = stream.Read<PaletteHeader>(Endian.Big);

                    int paletteSize = paletteHeader.Count * 2;
                    if ((byte)paletteHeader.Format == byte.MaxValue) // This is a modification from TPX format which describes a RGBA pallete with the value 255
                    {
                        paletteSize *= 2;
                        paletteHeader.Format = GXPaletteFormat.IA8;
                    }
                    stream.Seek(HeaderStart + paletteHeader.PaletteDataAddress, SeekOrigin.Begin);
                    PaletteData = stream.Read(paletteSize);
                }

                stream.Seek(HeaderStart + ImageOffsetTable[i].ImageHeaderOffset, SeekOrigin.Begin);

                ImageHeader imageHeader = stream.Read<ImageHeader>(Endian.Big);

                stream.Seek(HeaderStart + imageHeader.ImageDataAddress, SeekOrigin.Begin);
                TexEntry current = new(stream, PaletteData, imageHeader.Format, paletteHeader.Format, paletteHeader.Count, imageHeader.Width, imageHeader.Height, imageHeader.MaxLOD)
                {
                    LODBias = imageHeader.LODBias,
                    MagnificationFilter = imageHeader.MagFilter,
                    MinificationFilter = imageHeader.MinFilter,
                    WrapS = imageHeader.WrapS,
                    WrapT = imageHeader.WrapT,
                    EnableEdgeLOD = imageHeader.EnableEdgeLOD,
                    MinLOD = imageHeader.MinLOD,
                    MaxLOD = imageHeader.MaxLOD
                };
                textures.Add(current);
            }
        }

        protected override void Read(Stream stream)
        {
            long HeaderStart = stream.Position;

            stream.MatchThrow(Magic);

            ProcessStream(stream, HeaderStart, this);
        }

        protected override void Write(Stream stream)
        {
            long HeaderStart = stream.Position;
            stream.Write(Magic);

            stream.Write(Count, Endian.Big); //TotalImageCount
            stream.Write(stream.Position + 4 - HeaderStart, Endian.Big); //ImageOffsetTableOffset

            long OffsetLocation = stream.Position;
            using SpanBuffer<ImageOffsetEntry> ImageOffsetTable = new(Count);
            stream.Write(new byte[Count * 8]); //ImageOffsetTable Placeholders

            for (int i = 0; i < Count; i++)
            {
                stream.WriteAlign(32);
                bool IsPalette = this[i].Format.IsPaletteFormat();
                uint PaletteHeader = !IsPalette ? 0 : (uint)(stream.Position - HeaderStart);

                if (IsPalette)
                {
                    PaletteHeader paletteHeader = new()
                    {
                        Count = (ushort)this[i].Palettes.Sum(p => p.Length / 2),
                        Unpacked = 0x00,
                        Pad = 0x00,
                        Format = this[i].PaletteFormat,
                        PaletteDataAddress = (uint)(StreamEx.AlignPosition(stream.Position + Unsafe.SizeOf<PaletteHeader>(), 32) - HeaderStart)
                    };
                    stream.Write(paletteHeader, Endian.Big);
                    stream.WriteAlign(32);
                    foreach (byte[] Palette in this[i].Palettes)
                        stream.Write(Palette);
                    stream.WriteAlign(32);
                }

                uint ImageHeader = ImageHeader = (uint)(stream.Position - HeaderStart);

                ImageHeader imageHeader = new()
                {
                    Height = (ushort)this[i].Height,
                    Width = (ushort)this[i].Width,
                    Format = this[i].Format,
                    ImageDataAddress = (uint)(StreamEx.AlignPosition(stream.Position + Unsafe.SizeOf<PaletteHeader>(), 32) - HeaderStart),
                    WrapS = this[i].WrapS,
                    WrapT = this[i].WrapT,
                    MagFilter = this[i].MagnificationFilter,
                    MinFilter = this[i].MinificationFilter,
                    LODBias = this[i].LODBias,
                    EnableEdgeLOD = this[i].EnableEdgeLOD,
                    MinLOD = (byte)this[i].MinLOD,
                    MaxLOD = (byte)this[i].Count,
                    Unpacked = 0x00,
                };
                stream.Write(imageHeader, Endian.Big);
                stream.WriteAlign(32);

                foreach (byte[] bytes in this[i].RawImages)
                {
                    stream.Write(bytes);
                }

                ImageOffsetTable[i] = new ImageOffsetEntry(ImageHeader, PaletteHeader);
            }

            //Write ImageOffsetTable
            stream.At(OffsetLocation, s => s.Write<ImageOffsetEntry>(ImageOffsetTable, Endian.Big));
        }

        public readonly struct ImageOffsetEntry
        {
            public readonly uint ImageHeaderOffset;
            public readonly uint PaletteHeaderOffset;
            public readonly bool HasPaletteData => PaletteHeaderOffset != 0;

            public ImageOffsetEntry(uint imageHeaderOffset, uint paletteHeaderOffset)
            {
                ImageHeaderOffset = imageHeaderOffset;
                PaletteHeaderOffset = paletteHeaderOffset;
            }
        }

        public struct ImageHeader
        {
            public ushort Height;
            public ushort Width;
            private uint format;
            public uint ImageDataAddress;
            private uint wrapS;
            private uint wrapT;
            private uint minFilter;
            private uint maxFilter;
            public float LODBias;
            public byte EdgeLOD;
            public byte MinLOD;
            public byte MaxLOD;
            public byte Unpacked;

            public GXImageFormat Format
            {
                readonly get => (GXImageFormat)format;
                set => format = (uint)value;
            }

            public GXWrapMode WrapS
            {
                readonly get => (GXWrapMode)wrapS;
                set => wrapS = (uint)value;
            }

            public GXWrapMode WrapT
            {
                readonly get => (GXWrapMode)wrapT;
                set => wrapT = (uint)value;
            }

            public GXFilterMode MinFilter
            {
                readonly get => (GXFilterMode)minFilter;
                set => minFilter = (uint)value;
            }

            public GXFilterMode MagFilter
            {
                readonly get => (GXFilterMode)maxFilter;
                set => maxFilter = (uint)value;
            }

            public bool EnableEdgeLOD
            {
                readonly get => EdgeLOD > 0;
                set => EdgeLOD = (byte)(value ? 1 : 0);
            }
        }

        public struct PaletteHeader
        {
            public ushort Count;
            public byte Unpacked;
            public byte Pad;
            private uint format;
            public uint PaletteDataAddress;

            public GXPaletteFormat Format
            {
                readonly get => (GXPaletteFormat)format;
                set => format = (uint)value;
            }
        }
    }
}
