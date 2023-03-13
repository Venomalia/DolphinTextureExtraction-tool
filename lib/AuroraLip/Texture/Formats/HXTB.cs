using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    // Rune Factory (Frontier) texture format
    public class HXTB : JUTTexture, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "HXTB";

        public HXTB() { }

        public HXTB(Stream stream) : base(stream) { }

        public HXTB(string filepath) : base(filepath) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);

            string version = stream.ReadString(4);
            uint name_table_offset = stream.ReadUInt32(Endian.Big);
            uint file_count = stream.ReadUInt32(Endian.Big);
            // File size and other data after this
            for (uint i = 0; i < file_count; i++)
            {
                stream.Seek(name_table_offset + i * 0x20, SeekOrigin.Begin);
                string name = stream.ReadString(16);
                uint unknown = stream.ReadUInt32(Endian.Big);
                uint header_offset = stream.ReadUInt32(Endian.Big);
                uint palette_header_offset = stream.ReadUInt32(Endian.Big);
                stream.Seek(header_offset, SeekOrigin.Begin);
                uint data_offset = stream.ReadUInt32(Endian.Big);
                GXImageFormat format = (GXImageFormat)stream.ReadByte();
                byte image_count = (byte)stream.ReadByte(); // max_LOD?
                ushort unknown3 = stream.ReadUInt16(Endian.Big);
                ushort width = stream.ReadUInt16(Endian.Big);
                ushort height = stream.ReadUInt16(Endian.Big);
                uint size = stream.ReadUInt32(Endian.Big);

                //int mipmaps = image_count - 1;

                //some images have a too large or too small image_count!
                //We keep calculating the mipmaps until we know why this is so.
                int mipmaps = format.GetMipmapsFromSize((int)size, width, height);

                byte[] palette_data = null;
                GXPaletteFormat palette_format = GXPaletteFormat.IA8;
                uint palette_count = 0;
                if (palette_header_offset > 0)
                {
                    stream.Seek(palette_header_offset, SeekOrigin.Begin);
                    uint palette_data_offset = stream.ReadUInt32(Endian.Big);
                    palette_format = (GXPaletteFormat)stream.ReadByte();
                    ushort palette_unknown = stream.ReadUInt16(Endian.Big);
                    ushort palette_unknown2 = stream.ReadUInt16(Endian.Big);
                    uint palette_unknown3 = stream.ReadUInt32(Endian.Big);
                    uint palette_size = stream.ReadUInt32(Endian.Big);
                    palette_count = palette_size / 2;
                    stream.Seek(palette_data_offset, SeekOrigin.Begin);
                    palette_data = stream.Read((int)palette_size, Endian.Little);
                }

                stream.Seek(data_offset, SeekOrigin.Begin);
                TexEntry current = new TexEntry(stream, palette_data, format, palette_format, (int)palette_count, width, height, mipmaps)
                {
                    LODBias = 0,
                    MagnificationFilter = GXFilterMode.Nearest,
                    MinificationFilter = GXFilterMode.Nearest,
                    WrapS = GXWrapMode.CLAMP,
                    WrapT = GXWrapMode.CLAMP,
                    EnableEdgeLOD = false,
                    MinLOD = 0,
                    MaxLOD = image_count
                };
                Add(current);
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
