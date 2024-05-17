using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Keen Games Dawn of Discovery Archive
    /// </summary>
    public sealed class PAKb : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("PAKb");

        public PAKb()
        {
        }

        public PAKb(string name) : base(name)
        {
        }

        public PAKb(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);

            uint file_count = source.ReadUInt32(Endian.Big);

            using SpanBuffer<FileData> file_data = new(file_count);
            source.Read<FileData>(file_data, Endian.Big);

            uint names_start = file_data[(int)file_count - 1].Offset;

            for (int i = 0; i < file_count; i++)
            {
                source.Seek(names_start, SeekOrigin.Begin);

                uint expected_crc = file_data[i].Crc;
                uint crc = 0;
                string name = "";
                do
                {
                    crc = source.ReadUInt32(Endian.Big);
                    uint name_size = source.ReadUInt32(Endian.Big);
                    if (name_size == 0)
                    {
                        crc = expected_crc;
                    }
                    name = source.ReadString((int)name_size);
                } while (expected_crc != crc);

                FileNode file = new(name, new SubStream(source, file_data[i].Size, file_data[i].Offset));
                Add(file);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private readonly struct FileData
        {
            public readonly uint Crc;
            public readonly uint Offset;
            public readonly uint Size;
        }
    }
}
