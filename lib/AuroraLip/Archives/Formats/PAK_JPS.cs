using AuroraLib.Common;
using AuroraLib.Compression.Formats;
using AuroraLib.Compression;
using AuroraLib.Common.Struct;

namespace AuroraLib.Archives.Formats
{
    public class PAK_JPS : Archive, IHasIdentifier, IFileAccess
    {
        private static readonly Identifier32 _identifier = new(0x20, (byte)'K', (byte)'A', (byte)'P');

        public virtual IIdentifier Identifier => _identifier;

        public bool CanRead => true;

        public bool CanWrite => false;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
            int version = stream.ReadInt32(Endian.Big);
            int zero = stream.ReadInt32(Endian.Big);
            int size = stream.ReadInt32(Endian.Big);
            int data_pos = stream.ReadInt32(Endian.Big);
            data_pos += size;
            stream.Seek(size, SeekOrigin.Begin);
            int file_count = stream.ReadInt32(Endian.Big);
            long string_table_position = file_count * 24 + stream.Position;
            long current_file_data_position = data_pos;

            var zlib = new ZLib();
            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < file_count; i++)
            {
                int uncompressed_size = stream.ReadInt32(Endian.Big);
                int compressed_size = stream.ReadInt32(Endian.Big);
                int aligned_size = stream.ReadInt32(Endian.Big);
                int folder_name_offset = stream.ReadInt32(Endian.Big);
                string file_type = stream.ReadString(4);
                int file_name_offset = stream.ReadInt32(Endian.Big);

                long pos = stream.Position;
                stream.Seek(string_table_position + file_name_offset, SeekOrigin.Begin);
                string file_name = stream.ReadString();

                if (compressed_size == uncompressed_size)
                {
                    Root.AddArchiveFile(stream, uncompressed_size, current_file_data_position, file_name);
                }
                else
                {
                    stream.Seek(current_file_data_position, SeekOrigin.Begin);
                    byte[] bytes = stream.Read((int)compressed_size).ToArray();
                    var decompressed_stream = zlib.Decompress(bytes, 4096, false);
                    Root.AddArchiveFile(decompressed_stream, file_name);
                }
                current_file_data_position += aligned_size;
                stream.Seek(pos, SeekOrigin.Begin);
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();
    }
}
