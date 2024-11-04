using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Intelligent Systems Fire Emblem Archive
    /// </summary>
    public sealed class PAK_FE : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("pack");

        public PAK_FE()
        {
        }

        public PAK_FE(string name) : base(name)
        {
        }

        public PAK_FE(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);

            ushort NrEntries = source.ReadUInt16(Endian.Big);
            ushort Unk = source.ReadUInt16(Endian.Big);

            using SpanBuffer<Entrie> entries = new (NrEntries);
            source.Read<Entrie>(entries, Endian.Big);

            for (int i = 0; i < NrEntries; i++)
            {
                source.Seek(entries[i].name, SeekOrigin.Begin);
                string name = source.ReadCString();

                FileNode file = new(name, new SubStream(source, entries[i].size, entries[i].data));
                Add(file);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private readonly struct Entrie
        {
            public readonly uint Unk;
            public readonly uint name;
            public readonly uint data;
            public readonly uint size;
        }
    }
}
