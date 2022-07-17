using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    // https://wiki.tockdom.com/wiki/BREFF_and_BREFT_(File_Format)
    public class REFT : JUTTexture, IMagicIdentify, IFileAccess
    {

        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "REFT";

        public ushort ByteOrder { get; set; }

        public REFT() {}

        public REFT(Stream stream) : base(stream) {}

        public REFT(string filepath) : base(filepath) {}

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");
            ByteOrder = BitConverter.ToUInt16(stream.Read(2), 0); //65534 BigEndian
            if (ByteOrder != 65534)
            {
                throw new Exception($"ByteOrder: \"{ByteOrder}\" Not Implemented");
            }
            ushort FormatVersion = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            uint TotalSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            ushort Offset = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            ushort sections = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            if (sections > 1)
            {
                Console.WriteLine("Warning, REFT with more than one sections are not fully supported.");
            }
            stream.Position = 0x10;
            //root sections
            if (!stream.MatchString(magic))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");
            uint RootSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            long SubfilePosition = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0) + stream.Position-4;
            uint Unknown = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            uint Unknown2 = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            ushort NameLength = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            ushort Unknown3 = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
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
            uint SubfileSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            ushort subfiles = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            ushort Unknown = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            for (int i = 0; i < subfiles; i++)
            {
                ushort NameLength = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                long EndNamePosition = stream.Position + NameLength;
                string Name = stream.ReadString();
                stream.Position = EndNamePosition;
                uint DataOffset = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                uint DataSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                long EndOfGroup = stream.Position;
                stream.Position = StartOfIndex + DataOffset;
                ReadREFTfile(stream);
                stream.Position = EndOfGroup;
            }
        }

        private void ReadREFTfile(Stream stream)
        {
            uint Unknown = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            int ImageWidth = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            int ImageHeight = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            uint totalSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            GXImageFormat Format = (GXImageFormat)stream.ReadByte();
            GXPaletteFormat PaletteFormat = (GXPaletteFormat)stream.ReadByte();
            int Palettes = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            uint PaletteSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            byte images = (byte)stream.ReadByte();
            if (images > 0) --images;
            byte MinFilter = (byte)stream.ReadByte();
            byte MaxFilter = (byte)stream.ReadByte();
            byte Unknown2 = (byte)stream.ReadByte();
            float LODBias = BitConverter.ToSingle(stream.ReadBigEndian(4), 0);
            uint Unknown3 = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            long ImageAddress = stream.Position;

            byte[] PaletteData = null;
            if (PaletteSize > 0)
            {
                stream.Position += totalSize;
                PaletteData = stream.Read((int)PaletteSize);
            }
            stream.Position = ImageAddress;
            TexEntry current = new TexEntry(stream, PaletteData, Format, PaletteFormat, Palettes, ImageWidth, ImageHeight, images)
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
