using AuroraLib.Common;
using System.Runtime.CompilerServices;

namespace AuroraLib.Texture.Formats
{

    public class TPL : JUTTexture, IFileAccess
    {
        public static readonly byte[] Magic = new byte[4] { 0x00, 0x20, 0xAF, 0x30 };

        public bool CanRead => true;

        public bool CanWrite => true;

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => stream.Length > 12 && stream.ReadByte() == Magic[0] && stream.ReadByte() == Magic[1] && stream.ReadByte() == Magic[2] && stream.ReadByte() == Magic[3];

        public TPL() : base() { }
        public TPL(string filepath) : base(filepath) { }
        public TPL(Stream stream) => Read(stream);

        // Helper function to read a TPL stream
        // exposed so other classes that wish to read
        // a TPL can do so
        public static void ProcessStream(Stream stream, long HeaderStart, List<TexEntry> textures)
        {
            int TotalImageCount = stream.ReadInt32(Endian.Big);
            int ImageOffsetTableOffset = stream.ReadInt32(Endian.Big);

            stream.Seek(HeaderStart + ImageOffsetTableOffset, SeekOrigin.Begin);
            ImageOffsetEntry[] ImageOffsetTable = stream.For(TotalImageCount, s => s.Read<ImageOffsetEntry>(Endian.Big));

            for (int i = 0; i < ImageOffsetTable.Length; i++)
            {
                byte[] PaletteData = null;
                PaletteHeader paletteHeader;
                if (ImageOffsetTable[i].HasPaletteData)
                {
                    stream.Seek(HeaderStart + ImageOffsetTable[i].PaletteHeaderOffset, SeekOrigin.Begin);
                    paletteHeader = stream.Read<PaletteHeader>(Endian.Big);

                    stream.Seek(HeaderStart + paletteHeader.PaletteDataAddress, SeekOrigin.Begin);
                    PaletteData = stream.Read(paletteHeader.Count * 2);
                }
                else
                {
                    paletteHeader = default;
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

            if (!IsMatch(stream))
                throw new InvalidIdentifierException("0x0020AF30");

            ProcessStream(stream, HeaderStart, this);
        }

        protected override void Write(Stream stream)
        {
            long HeaderStart = stream.Position;
            stream.Write(Magic, 0, Magic.Length);

            stream.Write(Count, Endian.Big); //TotalImageCount
            stream.Write(stream.Position + 4 - HeaderStart, Endian.Big); //ImageOffsetTableOffset

            long OffsetLocation = stream.Position;
            ImageOffsetEntry[] ImageOffsetTable = new ImageOffsetEntry[Count];
            stream.Write(new byte[Count * 8]); //ImageOffsetTable Placeholders

            for (int i = 0; i < Count; i++)
            {
                stream.WritePadding(32);
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
                        PaletteDataAddress = (uint)(StreamEx.CalculatePadding(stream.Position + Unsafe.SizeOf<PaletteHeader>(), 32) - HeaderStart)
                    };
                    stream.WriteObjekt(paletteHeader, Endian.Big);
                    stream.WritePadding(32);
                    foreach (var Palette in this[i].Palettes)
                        stream.Write(Palette);
                    stream.WritePadding(32);
                }

                uint ImageHeader = ImageHeader = (uint)(stream.Position - HeaderStart);

                ImageHeader imageHeader = new()
                {
                    Height = (ushort)this[i].Height,
                    Width = (ushort)this[i].Width,
                    Format = this[i].Format,
                    ImageDataAddress = (uint)(StreamEx.CalculatePadding(stream.Position + Unsafe.SizeOf<PaletteHeader>(), 32) - HeaderStart),
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
                stream.WriteObjekt(imageHeader, Endian.Big);
                stream.WritePadding(32);

                foreach (byte[] bytes in this[i].RawImages)
                {
                    stream.Write(bytes);
                }

                ImageOffsetTable[i] = new ImageOffsetEntry() { ImageHeaderOffset = ImageHeader, PaletteHeaderOffset = PaletteHeader };
            }

            //Write ImageOffsetTable
            stream.At(OffsetLocation, s => s.WriteObjekt(ImageOffsetTable, Endian.Big));
        }

        public struct ImageOffsetEntry
        {
            public uint ImageHeaderOffset;
            public uint PaletteHeaderOffset;

            public bool HasPaletteData => PaletteHeaderOffset != 0;
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
                get => (GXImageFormat)format;
                set => format = (uint)value;
            }
            public GXWrapMode WrapS
            {
                get => (GXWrapMode)wrapS;
                set => wrapS = (uint)value;
            }
            public GXWrapMode WrapT
            {
                get => (GXWrapMode)wrapT;
                set => wrapT = (uint)value;
            }
            public GXFilterMode MinFilter
            {
                get => (GXFilterMode)minFilter;
                set => minFilter = (uint)value;
            }
            public GXFilterMode MagFilter
            {
                get => (GXFilterMode)maxFilter;
                set => maxFilter = (uint)value;
            }
            public bool EnableEdgeLOD
            {
                get => EdgeLOD > 0;
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
                get => (GXPaletteFormat)format;
                set => format = (uint)value;
            }
        }
    }
}
