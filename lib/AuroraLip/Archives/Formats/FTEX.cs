using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class FTEX : Archive, IHasIdentifier, IFileAccess
    {
        public virtual bool CanRead => true;

        public virtual bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("FTEX");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            Header header = stream.Read<Header>();
            stream.Seek(0x10, SeekOrigin.Current);
            NameEntry[] names = stream.For((int)header.Entrys, s => new NameEntry(s));

            stream.Seek(header.Offset, SeekOrigin.Begin);
            Root = new ArchiveDirectory() { Name = "root", OwnerArchive = this };
            for (int i = 0; i < header.Entrys; i++)
            {
                FTX0 Entry = stream.Read<FTX0>();
                stream.Seek(Entry.Offset - 0x10, SeekOrigin.Current);
                Root.AddArchiveFile(stream, Entry.Size, names[i].Name);
                stream.Seek(Entry.Size, SeekOrigin.Current);
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct Header
        {
            public uint Magic; // FTEX 1480938566
            public uint Size;
            public uint Offset;
            public uint Entrys;

            public uint FullSize => Size + Offset + 0x10;
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

            public uint FullSize => Size + Offset;
        }
    }
}
