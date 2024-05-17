using AuroraLib.Common.Node;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Capcom MT Framework Archive.
    /// </summary>
    // http://svn.gib.me/public/mt/trunk/Gibbed.MT.FileFormats/ArchiveFile.cs
    // https://forum.xentax.com/viewtopic.php?t=8972
    public sealed class ARC_MTF : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(0x0, (byte)'C', (byte)'R', (byte)'A');

        public ARC_MTF()
        {
        }

        public ARC_MTF(string name) : base(name)
        {
        }

        public ARC_MTF(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);
            ushort version = source.ReadUInt16(Endian.Big);
            ushort file_count = source.ReadUInt16(Endian.Big);

            ZLib zlib = new();
            for (int i = 0; i < file_count; i++)
            {
                string name = source.ReadString(64);
                uint hash = source.ReadUInt32(Endian.Big);
                uint compressed_size = source.ReadUInt32(Endian.Big);
                uint flags = source.ReadUInt32(Endian.Big);
                uint offset = source.ReadUInt32(Endian.Big);

                uint uncompressed_size = (flags & 0x1FFFFFFF) >> 0;

                if (compressed_size == uncompressed_size)
                {
                    Add(new FileNode(name, new SubStream(source, uncompressed_size, offset)) { ID = hash});
                }
                else
                {
                    long pos = source.Position;
                    source.Seek(offset, SeekOrigin.Begin);
                    Stream decompressed_stream = new MemoryPoolStream((int)compressed_size);
                    zlib.Decompress(source, decompressed_stream, (int)compressed_size);
                    long decompressed_stream_pos = decompressed_stream.Position;
                    decompressed_stream.Seek(0, SeekOrigin.Begin);

                    // The inner magic determines what the extension is
                    decompressed_stream.ReadByte();
                    string inner_file_magic = decompressed_stream.ReadString(3);
                    uint data_skip = 0;
                    if (inner_file_magic == "XET")
                    {
                        name += ".brtex";
                        data_skip = 0x20;
                    }
                    else if (inner_file_magic == "TLP")
                    {
                        name += ".brplt";
                        data_skip = 0x20;
                    }
                    else
                    {
                        // Unknown...
                        name += ".unk" + hash.ToString();
                    }

                    decompressed_stream.Seek(decompressed_stream_pos, SeekOrigin.Begin);

                    Add(new FileNode(name, new SubStream(decompressed_stream, decompressed_stream.Length - data_skip, data_skip)));
                    source.Seek(pos, SeekOrigin.Begin);
                }
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}
