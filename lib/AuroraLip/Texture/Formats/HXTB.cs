using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Texture.Formats
{
    // Rune Factory (Frontier) texture format
    public class HXTB : JUTTexture, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("HXTB");

        public HXTB()
        { }

        public HXTB(Stream stream) : base(stream)
        {
        }

        public HXTB(string filepath) : base(filepath)
        {
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            int version = int.Parse(stream.ReadString(4));
            uint name_table_offset = stream.ReadUInt32(Endian.Big);
            uint file_count = stream.ReadUInt32(Endian.Big);
            //stream.Position += 8;
            //uint data_Size = stream.ReadUInt32(Endian.Big);
            //uint file_Size = stream.ReadUInt32(Endian.Big);

            // File size and other data after this
            for (uint i = 0; i < file_count; i++)
            {
                stream.Seek(name_table_offset + i * 0x20, SeekOrigin.Begin);
                string name = stream.ReadString(16);
                uint hash = stream.ReadUInt32(Endian.Big);
                uint header_offset = stream.ReadUInt32(Endian.Big);
                uint palette_header_offset = stream.ReadUInt32(Endian.Big);
                stream.Seek(header_offset, SeekOrigin.Begin);
                uint data_offset = stream.ReadUInt32(Endian.Big);
                GXImageFormat format = stream.Read<GXImageFormat>();
                byte max_LOD = stream.ReadUInt8();
                ushort padding = stream.ReadUInt16(Endian.Big); //always 0
                ushort width = stream.ReadUInt16(Endian.Big);
                ushort height = stream.ReadUInt16(Endian.Big);
                uint size = stream.ReadUInt32(Endian.Big);

                int mipmaps = max_LOD == 0 ? 0 : max_LOD - 1;

                Span<byte> palette_data = Span<byte>.Empty;
                GXPaletteFormat palette_format = GXPaletteFormat.IA8;
                uint palette_count = 0;
                if (format.IsPaletteFormat())
                {
                    stream.Seek(palette_header_offset, SeekOrigin.Begin);
                    uint palette_data_offset = stream.ReadUInt32(Endian.Big);
                    palette_format = stream.Read<GXPaletteFormat>();
                    byte palette_unknown = stream.ReadUInt8();
                    ushort palette_unknown2 = stream.ReadUInt16(Endian.Big);
                    palette_count = stream.ReadUInt16(Endian.Big);
                    uint palette_unknown3 = stream.ReadUInt16(Endian.Big); //always 1
                    uint palette_size = stream.ReadUInt32(Endian.Big);
                    stream.Seek(palette_data_offset, SeekOrigin.Begin);
                    palette_data = stream.Read(palette_size);
                }

                stream.Seek(data_offset, SeekOrigin.Begin);
                TexEntry current = new (stream, palette_data, format, palette_format, (int)palette_count, width, height, mipmaps)
                {
                    LODBias = 0,
                    MagnificationFilter = GXFilterMode.Nearest,
                    MinificationFilter = GXFilterMode.Nearest,
                    WrapS = GXWrapMode.CLAMP,
                    WrapT = GXWrapMode.CLAMP,
                    EnableEdgeLOD = false,
                    MinLOD = 0,
                    MaxLOD = max_LOD
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
