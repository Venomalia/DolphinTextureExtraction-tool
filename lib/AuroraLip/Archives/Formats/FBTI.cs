using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Rune Factory (Frontier) archive format
    /// </summary>
    public sealed class FBTI : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("FBTI");

        public FBTI()
        {
        }

        public FBTI(string name) : base(name)
        {
        }

        public FBTI(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);
            int version = int.Parse(source.ReadString(4));
            uint file_count = source.ReadUInt32(Endian.Big);
            uint start_offset = source.ReadUInt32(Endian.Big); // always 0x10

            Span<FileEntry> entries = stackalloc FileEntry[(int)file_count];
            source.Read(entries, Endian.Big);

            for (int i = 0; i < file_count; i++)
            {
                string name = NLCM.GetName(source, entries[i].Offset, entries[i].Size, i);
                Add(new FileNode(name, new SubStream(source, entries[i].Size, entries[i].Offset)));
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private readonly struct FileEntry
        {
            public readonly uint Offset;
            public readonly uint Size;

            public FileEntry(uint offset, uint size)
            {
                Offset = offset;
                Size = size;
            }
        }
    }
}
