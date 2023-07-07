using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    // Super Swing Golf S2 texture format
    public class GSR : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public GSR()
        { }

        public GSR(Stream stream) : base(stream)
        {
        }

        public GSR(string filepath) : base(filepath)
        {
        }

        private static readonly Identifier32 _le_identifier = new(0x20, 0, 0, 0);

        public bool IsMatch(Stream stream, in string extension = "")
            => extension.ToLower() == ".gsr" && !stream.Match(_le_identifier);

        protected override void Read(Stream stream)
        {
            stream.ReadString(4);
            stream.ReadString(8);
            uint file_size = stream.ReadUInt32(Endian.Big);
            uint unknown1 = stream.ReadUInt32(Endian.Big);
            uint identifier_count = stream.ReadUInt32(Endian.Big);
            uint unknown3 = stream.ReadUInt32(Endian.Big);
            uint unknown4 = stream.ReadUInt32(Endian.Big);

            long texture_data_offset = 0;
            long texture_details_offset = 0;
            long palette_data_offset = 0;
            long palette_details_offset = 0;

            uint palette_size = 0;

            Identifier32 texture_detail_identifer = new((byte)'d', (byte)'x', (byte)'e', (byte)'t');
            Identifier32 texture_identifer = new(0, (byte)'x', (byte)'e', (byte)'t');
            Identifier32 palette_detail_identifer = new((byte)'d', (byte)'l', (byte)'a', (byte)'p');
            Identifier32 palette_identifer = new(0, (byte)'l', (byte)'a', (byte)'p');
            for (int i = 0; i < identifier_count; i++)
            {
                var identifier = stream.Read<Identifier32>(Endian.Big);
                if (identifier == texture_detail_identifer)
                {
                    uint unknown = stream.ReadUInt32(Endian.Big);
                    uint size = stream.ReadUInt32(Endian.Big);
                    texture_data_offset = stream.ReadUInt32(Endian.Big);
                }
                else if (identifier == texture_identifer)
                {
                    uint unknown = stream.ReadUInt32(Endian.Big);
                    uint size = stream.ReadUInt32(Endian.Big);
                    texture_details_offset = (long)stream.ReadUInt32(Endian.Big);
                }
                else if (identifier == palette_detail_identifer)
                {
                    palette_size = stream.ReadUInt32(Endian.Big);
                    uint unknown = stream.ReadUInt32(Endian.Big);
                    palette_data_offset = stream.ReadUInt32(Endian.Big);
                }
                else if (identifier == palette_identifer)
                {
                    uint unknown = stream.ReadUInt32(Endian.Big);
                    uint size = stream.ReadUInt32(Endian.Big);
                    palette_details_offset = (long)stream.ReadUInt32(Endian.Big);
                }
            }

            byte[] palette_data = null;
            GXPaletteFormat palette_format = GXPaletteFormat.IA8;
            int palette_count = 0;
            if (palette_details_offset != 0)
            {
                stream.Seek(palette_details_offset, SeekOrigin.Begin);
                int palette_unknown = (int)stream.ReadUInt32(Endian.Big);
                palette_count = (int)palette_size / 4;
                palette_format = (GXPaletteFormat)stream.ReadUInt32(Endian.Big);
                if (palette_data_offset != 0)
                {
                    stream.Seek(palette_data_offset, SeekOrigin.Begin);
                    byte[] all_palette_data = stream.Read(palette_size);
                    palette_data = new byte[all_palette_data.Length];

                    // Creators broke up the image into two palettes, interweaving
                    // the bytes of the two palettes
                    // RG (first), RG (second), BA (first), BA (second), etc...
                    // For the texture extractor to do its thing, the palettes need to be
                    // sequential.  So we will resequence them to have the first one be
                    // in the first half of the palette memory, and the second one
                    // be in the latter half of the memory

                    for (int i = 0; i < all_palette_data.Length; i += 4)
                    {
                        // First pair goes into the first half of the outgoing palette memory
                        palette_data[i / 2] = all_palette_data[i];
                        palette_data[i / 2 + 1] = all_palette_data[i + 1];

                        // Second pair goes into the later half of hte outgoing palette memory
                        palette_data[i / 2 + all_palette_data.Length / 2] = all_palette_data[i + 2];
                        palette_data[i / 2 + 1 + all_palette_data.Length / 2] = all_palette_data[i + 3];
                    }
                }
            }

            if (texture_details_offset != 0)
            {
                stream.Seek(texture_details_offset, SeekOrigin.Begin);
                ushort another_unknown = stream.ReadUInt16(Endian.Big);
                ushort format = stream.ReadUInt16(Endian.Big);
                uint width = stream.ReadUInt32(Endian.Big);
                uint height = stream.ReadUInt32(Endian.Big);
                uint mipmap = stream.ReadUInt32(Endian.Big);
                uint texture_bytes = stream.ReadUInt32(Endian.Big);

                if (texture_data_offset != 0)
                {
                    stream.Seek(texture_data_offset, SeekOrigin.Begin);

                    TexEntry current = new TexEntry(stream, palette_data, (GXImageFormat)format, palette_format, palette_count, (int)width, (int)height, (int)mipmap)
                    {
                        LODBias = 0,
                        MagnificationFilter = GXFilterMode.Nearest,
                        MinificationFilter = GXFilterMode.Nearest,
                        WrapS = GXWrapMode.CLAMP,
                        WrapT = GXWrapMode.CLAMP,
                        EnableEdgeLOD = false,
                        MinLOD = 0,
                        MaxLOD = 0
                    };
                    Add(current);
                }
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
