using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Red Entertainment Tengai Makyō II Archive
    /// </summary>
    public sealed class PAK_TM2 : ArchiveNode
    {
        public override bool CanWrite => false;

        public PAK_TM2()
        {
        }

        public PAK_TM2(string name) : base(name)
        {
        }

        public PAK_TM2(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if ((extension.SequenceEqual(".pak") || extension.Length == 0 || extension.SequenceEqual(".cns")) && stream.Length > 0x20)
            {
                uint entryCount = stream.ReadUInt32(Endian.Big);
                if (entryCount != 0 && entryCount < 1024 && stream.Position + entryCount * 8 < stream.Length)
                {
                    Entry[] entrys = stream.For((int)entryCount, s => s.Read<Entry>(Endian.Big));

                    for (int i = 0; i < entryCount - 1; i++)
                    {
                        if (entrys[i].Offset + entrys[i].Size > entrys[i + 1].Offset)
                        {
                            return false;
                        }
                    }
                    return entrys.First().Offset >= stream.Position && entrys.Last().Offset + entrys.Last().Size == stream.Length;
                }
            }
            return false;
        }

        protected override void Deserialize(Stream source)
        {
            uint entryCount = source.ReadUInt32(Endian.Big);
            using SpanBuffer<Entry> entries = new SpanBuffer<Entry>(entryCount);
            source.Read<Entry>(entries, Endian.Big);

            for (int i = 0; i < entries.Length; i++)
            {
                FileNode file = new($"Entry_{i}", new SubStream(source, entries[i].Size, entries[i].Offset));
                Add(file);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private readonly struct Entry
        {
            public readonly uint Offset;
            public readonly uint Size;
        }
    }
}
