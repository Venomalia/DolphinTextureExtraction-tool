using AuroraLib.Common;
using AuroraLib.Compression.Formats;

namespace AuroraLib.Archives.Formats
{
    // http://svn.gib.me/public/mt/trunk/Gibbed.MT.FileFormats/ArchiveFile.cs
    // https://forum.xentax.com/viewtopic.php?t=8972
    public class ARC_MTF : Archive, IHasIdentifier, IFileAccess
    {
        private static readonly Identifier32 _identifier = new(0x0, (byte)'C', (byte)'R', (byte)'A');

        public virtual IIdentifier Identifier => _identifier;

        public bool CanRead => true;

        public bool CanWrite => false;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
            ushort version = stream.ReadUInt16(Endian.Big);
            ushort file_count = stream.ReadUInt16(Endian.Big);

            var zlib = new ZLib();
            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < file_count; i++)
            {
                string name = stream.ReadString(64);
                uint hash = stream.ReadUInt32(Endian.Big);
                uint compressed_size = stream.ReadUInt32(Endian.Big);
                uint flags = stream.ReadUInt32(Endian.Big);
                uint offset = stream.ReadUInt32(Endian.Big);

                var uncompressed_size = (flags & 0x1FFFFFFF) >> 0;

                if (compressed_size == uncompressed_size)
                {
                    Root.AddArchiveFile(stream, uncompressed_size, offset, name);
                }
                else
                {
                    var pos = stream.Position;
                    stream.Seek(offset, SeekOrigin.Begin);
                    byte[] bytes = stream.Read((int)compressed_size).ToArray();
                    var decompressed_stream = zlib.Decompress(bytes, 4096, false);
                    var decompressed_stream_pos = decompressed_stream.Position;
                    decompressed_stream.Seek(0, SeekOrigin.Begin);

                    // The inner magic determines what the extension is
                    decompressed_stream.ReadByte();
                    var inner_file_magic = decompressed_stream.ReadString(3);
                    uint data_skip = 0;
                    if (inner_file_magic == "XET")
                    {
                        name = name + ".brtex";
                        data_skip = 0x20;
                    }
                    else if (inner_file_magic == "TLP")
                    {
                        name = name + ".brplt";
                        data_skip = 0x20;
                    }
                    else
                    {
                        // Unknown...
                        name = name + ".unk" + hash.ToString();
                    }

                    decompressed_stream.Seek(decompressed_stream_pos, SeekOrigin.Begin);

                    Root.AddArchiveFile(decompressed_stream, decompressed_stream.Length - data_skip, data_skip, name);
                    stream.Seek(pos, SeekOrigin.Begin);
                }
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();
    }
}
