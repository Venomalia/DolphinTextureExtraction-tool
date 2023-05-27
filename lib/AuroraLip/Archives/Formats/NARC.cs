using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class NARC : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "NARC";

        public NARC()
        { }

        public NARC(string filename) : base(filename)
        {
        }

        public NARC(Stream RARCFile, string filename = null) : base(RARCFile, filename)
        {
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);
            uint NrEntries = stream.ReadUInt32(Endian.Big);
            uint StringTableSize = stream.ReadUInt32(Endian.Big);
            uint DataTablePosition = stream.ReadUInt32(Endian.Big);

            Root = new ArchiveDirectory() { OwnerArchive = this };

            List<NARCEntry> Entries = new List<NARCEntry>();
            for (int i = 0; i < NrEntries; i++)
            {
                Entries.Add(new NARCEntry(stream));
            }
            long position = stream.Position;

            foreach (NARCEntry entry in Entries)
            {
                ArchiveFile Sub = new ArchiveFile() { Parent = Root };
                stream.Position = position + entry.NameOffset;
                Sub.Name = stream.ReadString();
                stream.Position = DataTablePosition + entry.DataOffset;
                Sub.FileData = new SubStream(stream, entry.DataSize);
                Root.Items.Add(Sub.Name, Sub);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private class NARCEntry
        {
            public uint Unknown;
            public uint NameOffset;
            public uint DataOffset;
            public uint DataSize;

            public NARCEntry(Stream stream)
            {
                Unknown = stream.ReadUInt32(Endian.Big);
                NameOffset = stream.ReadUInt32(Endian.Big);
                DataOffset = stream.ReadUInt32(Endian.Big);
                DataSize = stream.ReadUInt32(Endian.Big);
            }
        }
    }
}
