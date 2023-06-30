using AuroraLib.Archives.Formats;
using AuroraLib.Common;
using AuroraLib.Common.Struct;

namespace AuroraLib.Texture.Formats
{
    // https://wiki.tockdom.com/wiki/BREFF_and_BREFT_(File_Format)
    public class REFT : JUTTexture, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("REFT");

        public Endian ByteOrder { get; set; }

        public ushort FormatVersion { get; set; }

        public REFT()
        { }

        public REFT(Stream stream) : base(stream)
        {
        }

        public REFT(string filepath) : base(filepath)
        {
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            Bres.Header header = new(stream);
            stream.MatchThrow(_identifier);
            ByteOrder = header.BOM;
            FormatVersion = header.Version;
            if (header.Sections > 1)
            {
                Events.NotificationEvent?.Invoke(NotificationType.Warning, "REFT with more than one sections are not fully supported.");
            }
            stream.Position = 0x10;
            //root sections
            stream.MatchThrow(_identifier);
            uint RootSize = stream.ReadUInt32(ByteOrder);
            long SubfilePosition = stream.ReadUInt32(ByteOrder) + stream.Position - 4;
            uint Unknown = stream.ReadUInt32(ByteOrder);
            uint Unknown2 = stream.ReadUInt32(ByteOrder);
            ushort NameLength = stream.ReadUInt16(ByteOrder);
            ushort Unknown3 = stream.ReadUInt16(ByteOrder);
            string Name = stream.ReadString();
            stream.Position = SubfilePosition;
#if DEBUG
            if (Unknown != 0 || Unknown2 != 0 || Unknown3 != 0)
            {
                Console.WriteLine($"Warning, {Unknown}-{Unknown2}-{Unknown3}");
            }
#endif
            //Subfile List
            ReadSubfile(stream);
        }

        private void ReadSubfile(Stream stream)
        {
            long StartOfIndex = stream.Position;
            uint SubfileSize = stream.ReadUInt32(ByteOrder);
            ushort subfiles = stream.ReadUInt16(ByteOrder);
            ushort Unknown = stream.ReadUInt16(ByteOrder);

            for (int i = 0; i < subfiles; i++)
            {
                SubfileItem Item = new(stream, ByteOrder);

                stream.At(StartOfIndex + Item.DataOffset, s => ReadREFTfile(s));
            }
        }

        private void ReadREFTfile(Stream stream)
        {
            ImageHeader Header = stream.Read<ImageHeader>(ByteOrder);

            if (FormatVersion >= 11)
            {
                byte[] Unknown4 = stream.Read(32);
#if DEBUG
                Events.NotificationEvent.Invoke(NotificationType.Info, $"{nameof(REFT)},{nameof(Unknown4)}: " + BitConverter.ToString(Unknown4));
#endif
            }

            ReadOnlySpan<byte> PaletteData = null;
            if (Header.Format.IsPaletteFormat())
            {
                PaletteData = stream.At(Header.ImageDataSize, SeekOrigin.Current, s => s.Read((int)Header.PaletteSize));
            }

            TexEntry current = new(stream, PaletteData, Header.Format, Header.PaletteFormat, Header.Palettes, Header.Width, Header.Height, Header.Mipmaps)
            {
                LODBias = Header.LODBias,
                MagnificationFilter = Header.MaxFilter,
                MinificationFilter = Header.MinFilter,
                MinLOD = 0,
                MaxLOD = Header.Images
            };
            Add(current);
        }

        protected override void Write(Stream stream)
            => throw new NotImplementedException();

        private struct SubfileItem
        {
            public string Name;
            public uint DataOffset;
            public uint DataSize;

            public SubfileItem(Stream stream, Endian ByteOrder)
            {
                ushort NameLength = stream.ReadUInt16(ByteOrder);
                Name = stream.ReadString(NameLength); // NULL terminated
                DataOffset = stream.ReadUInt32(ByteOrder);
                DataSize = stream.ReadUInt32(ByteOrder);
            }
        }

        private struct ImageHeader
        {
            public uint Unknown;
            public ushort Width;
            public ushort Height;
            public uint ImageDataSize;
            public GXImageFormat Format;
            public GXPaletteFormat PaletteFormat;
            public ushort Palettes;
            public uint PaletteSize;
            public byte Images;
            public GXFilterMode MinFilter;
            public GXFilterMode MaxFilter;
            public byte Unknown2;
            public float LODBias;
            public uint Unknown3;

            public int Mipmaps => Images <= 0 ? 0 : Images - 1;
        }
    }
}
