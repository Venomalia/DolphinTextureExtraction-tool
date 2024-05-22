using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// SEGA Texture archive
    /// </summary>
    // ref https://github.com/nickworonekin/puyotools/blob/master/src/PuyoTools.Core/Archives/Formats/GvmArchive.cs
    public sealed class GVMH : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("GVMH");

        public GVMH()
        {
        }

        public GVMH(string name) : base(name)
        {
        }

        public GVMH(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);

            uint entryOffset = source.ReadUInt32() + 8;
            source.Position++;
            byte properties = source.ReadUInt8();

            ushort numEntries = source.ReadUInt16(Endian.Big);
            Entry[] entries = new Entry[numEntries];
            for (int i = 0; i < numEntries; i++)
            {
                entries[i] = new Entry(source, properties);
            }

            source.Seek(entryOffset, SeekOrigin.Begin);
            for (int i = 0; i < numEntries; i++)
            {
                long offset = source.Position;
                string Magic = source.ReadString(4);
                int length = source.ReadInt32();

                // Some Billy Hatcher textures have an oddity where the last texture length is 16 more than what it
                // actually should be.
                if (i == numEntries - 1 && source.Position + length != source.Length)
                    length += 16;

                FileNode file = new(entries[i].Name, new SubStream(source, length + 8, offset));
                Add(file);

                source.Seek(length, SeekOrigin.Current);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private class Entry
        {
            public ushort Index { get; set; }
            public string Name { get; set; } = string.Empty;
            public ushort Format { get; set; }
            public ushort Dimension { get; set; }
            public int GlobalIndex { get; set; }

            public Entry()
            { }

            public Entry(Stream stream, byte properties)
            {
                bool hasFilenames = (properties & (1 << 3)) > 0;
                bool hasFormats = (properties & (1 << 2)) > 0;
                bool hasDimensions = (properties & (1 << 1)) > 0;
                bool hasGlobalIndexes = (properties & (1 << 0)) > 0;

                Index = stream.ReadUInt16(Endian.Big);
                if (hasFilenames) Name = stream.ReadString(28);
                if (hasFormats) Format = stream.ReadUInt16();
                if (hasDimensions) Dimension = stream.ReadUInt16();
                if (hasGlobalIndexes) GlobalIndex = stream.ReadInt32();
            }
        }
    }
}
