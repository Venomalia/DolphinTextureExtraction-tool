using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Treasure Sin and Punishment archive
    /// </summary>
    public sealed class NARC : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("NARC");

        public NARC()
        {
        }

        public NARC(string name) : base(name)
        {
        }

        public NARC(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x20 && stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);
            uint NrEntries = source.ReadUInt32(Endian.Big);
            uint StringTableSize = source.ReadUInt32(Endian.Big);
            uint dataTableOffset = source.ReadUInt32(Endian.Big);

            using SpanBuffer<NARCEntry> entries = new(NrEntries);
            source.Read<NARCEntry>(entries,Endian.Big);
            long nameTableOffset = source.Position;

            foreach (NARCEntry entry in entries)
            {
                source.Position = nameTableOffset + entry.NameOffset;
                FileNode Sub = new(source.ReadCString(), new SubStream(source, entry.DataSize, dataTableOffset + entry.DataOffset));
                Add(Sub);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private readonly struct NARCEntry
        {
            public readonly uint Unknown;
            public readonly uint NameOffset;
            public readonly uint DataOffset;
            public readonly uint DataSize;
        }
    }
}
