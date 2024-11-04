using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Junction Point Studios Archive
    /// </summary>
    public sealed class PAK_JPS : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(0x20, (byte)'K', (byte)'A', (byte)'P');

        public PAK_JPS()
        {
        }

        public PAK_JPS(string name) : base(name)
        {
        }

        public PAK_JPS(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);
            int version = source.ReadInt32(Endian.Big);
            int zero = source.ReadInt32(Endian.Big);
            int size = source.ReadInt32(Endian.Big);
            int data_pos = source.ReadInt32(Endian.Big);
            data_pos += size;
            source.Seek(size, SeekOrigin.Begin);
            int file_count = source.ReadInt32(Endian.Big);
            long string_table_position = file_count * 24 + source.Position;
            long current_file_data_position = data_pos;

            ZLib zlib = new();
            for (int i = 0; i < file_count; i++)
            {
                int uncompressed_size = source.ReadInt32(Endian.Big);
                int compressed_size = source.ReadInt32(Endian.Big);
                int aligned_size = source.ReadInt32(Endian.Big);
                int folder_name_offset = source.ReadInt32(Endian.Big);
                string file_type = source.ReadString(4);
                int file_name_offset = source.ReadInt32(Endian.Big);

                long pos = source.Position;
                source.Seek(string_table_position + file_name_offset, SeekOrigin.Begin);
                string file_name = source.ReadCString();

                if (!Contains(file_name))
                {
                    if (compressed_size == uncompressed_size)
                    {
                        FileNode file = new(file_name, new SubStream(source, uncompressed_size, current_file_data_position));
                        Add(file);
                    }
                    else
                    {
                        source.Seek(current_file_data_position, SeekOrigin.Begin);
                        Stream decompressed_stream = new MemoryPoolStream();
                        zlib.Decompress(source, decompressed_stream, compressed_size);
                        FileNode file = new(file_name, decompressed_stream) { Properties = "ZLib"};
                        Add(file);
                    }
                }
                current_file_data_position += aligned_size;
                source.Seek(pos, SeekOrigin.Begin);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}
