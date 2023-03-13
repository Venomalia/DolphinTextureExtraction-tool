using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    // https://wiki.tockdom.com/wiki/BREFF_and_BREFT_(File_Format)
    public class REFT : JUTTexture, IMagicIdentify, IFileAccess
    {

        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "REFT";

        public ushort ByteOrder { get; set; }

        public ushort FormatVersion { get; set; }


        public REFT() { }

        public REFT(Stream stream) : base(stream) { }

        public REFT(string filepath) : base(filepath) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);
            ByteOrder = BitConverter.ToUInt16(stream.Read(2), 0); //65534 BigEndian
            if (ByteOrder != 65534)
            {
                throw new NotImplementedException($"ByteOrder: \"{ByteOrder}\"");
            }
            FormatVersion = stream.ReadUInt16(Endian.Big); // SSBB 7 MKart 9  NSMB 11
            uint TotalSize = stream.ReadUInt32(Endian.Big);
            ushort Offset = stream.ReadUInt16(Endian.Big);
            ushort sections = stream.ReadUInt16(Endian.Big);
            if (sections > 1)
            {
                Events.NotificationEvent?.Invoke(NotificationType.Warning, "REFT with more than one sections are not fully supported.");
            }
            stream.Position = 0x10;
            //root sections
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);
            uint RootSize = stream.ReadUInt32(Endian.Big);
            long SubfilePosition = stream.ReadUInt32(Endian.Big) + stream.Position - 4;
            uint Unknown = stream.ReadUInt32(Endian.Big);
            uint Unknown2 = stream.ReadUInt32(Endian.Big);
            ushort NameLength = stream.ReadUInt16(Endian.Big);
            ushort Unknown3 = stream.ReadUInt16(Endian.Big);
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
            uint SubfileSize = stream.ReadUInt32(Endian.Big);
            ushort subfiles = stream.ReadUInt16(Endian.Big);
            ushort Unknown = stream.ReadUInt16(Endian.Big);
            for (int i = 0; i < subfiles; i++)
            {
                ushort NameLength = stream.ReadUInt16(Endian.Big);
                long EndNamePosition = stream.Position + NameLength;
                string Name = stream.ReadString();
                stream.Position = EndNamePosition;
                uint DataOffset = stream.ReadUInt32(Endian.Big);
                uint DataSize = stream.ReadUInt32(Endian.Big);
                long EndOfGroup = stream.Position;
                stream.Position = StartOfIndex + DataOffset;
                ReadREFTfile(stream);
                stream.Position = EndOfGroup;
            }
        }

        private void ReadREFTfile(Stream stream)
        {
            uint Unknown = stream.ReadUInt32(Endian.Big);
            int ImageWidth = stream.ReadUInt16(Endian.Big);
            int ImageHeight = stream.ReadUInt16(Endian.Big);
            uint totalSize = stream.ReadUInt32(Endian.Big);
            GXImageFormat Format = (GXImageFormat)stream.ReadByte();
            GXPaletteFormat PaletteFormat = (GXPaletteFormat)stream.ReadByte();
            int Palettes = stream.ReadUInt16(Endian.Big);
            uint PaletteSize = stream.ReadUInt32(Endian.Big);
            byte images = (byte)stream.ReadByte();
            byte MinFilter = (byte)stream.ReadByte();
            byte MaxFilter = (byte)stream.ReadByte();
            byte Unknown2 = (byte)stream.ReadByte();
            float LODBias = stream.ReadSingle(Endian.Big);
            uint Unknown3 = stream.ReadUInt32(Endian.Big);


            if (FormatVersion >= 11)
            {
                byte[] Unknown4 = stream.Read(32);
#if DEBUG
                foreach (byte bit in Unknown4)
                {
                    if (bit != 0)
                    {
                        Console.WriteLine($"Indos, {Unknown4}--{bit}");
                    }
                }
#endif
            }

            long ImageAddress = stream.Position;

            byte[] PaletteData = null;
            if (PaletteSize > 0)
            {
                stream.Position += totalSize;
                PaletteData = stream.Read((int)PaletteSize);
            }
            stream.Position = ImageAddress;

            int mips = images <= 0 ? 0 : images - 1;
            TexEntry current = new TexEntry(stream, PaletteData, Format, PaletteFormat, Palettes, ImageWidth, ImageHeight, mips)
            {
                LODBias = LODBias,
                MagnificationFilter = (GXFilterMode)MaxFilter,
                MinificationFilter = (GXFilterMode)MinFilter,
                MinLOD = 0,
                MaxLOD = images
            };
            Add(current);
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
