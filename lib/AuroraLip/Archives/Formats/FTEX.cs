using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Vanillaware Muramasa Texture Archive
    /// </summary>
    public sealed class FTEX : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("FTEX");

        public FTEX()
        {
        }

        public FTEX(string name) : base(name)
        {
        }

        public FTEX(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            Header header = source.Read<Header>();
            source.Seek(0x10, SeekOrigin.Current);
            NameEntry[] names = source.For((int)header.Entrys, s => new NameEntry(s));

            source.Seek(header.Offset, SeekOrigin.Begin);
            for (int i = 0; i < header.Entrys; i++)
            {
                FTX0 Entry = source.Read<FTX0>();
                source.Seek(Entry.Offset - 0x10, SeekOrigin.Current);
                FileNode file = new(names[i].Name, new SubStream(source, Entry.Size));
                Add(file);
                source.Seek(Entry.Size, SeekOrigin.Current);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private struct Header
        {
            public uint Magic; // FTEX 1480938566
            public uint Size;
            public uint Offset;
            public uint Entrys;

            public readonly uint FullSize => Size + Offset + 0x10;
        }

        private struct NameEntry
        {
            public string Name;
            public Propertie Properties;

            public NameEntry(Stream stream)
            {
                Name = stream.ReadString(0x20);
                Properties = stream.Read<Propertie>();
            }

            public struct Propertie
            {
                public uint Padding;
                public uint unk1;
                public uint unk2;
                public uint unk3;
            }
        }

        private struct FTX0
        {
            public uint Magic; // FTX0 811095110
            public uint Size;
            public uint Offset;
            public uint Padding;
        }
    }
}
