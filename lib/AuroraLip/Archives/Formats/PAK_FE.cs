using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    public class PAK_FE : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("pack");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            ushort NrEntries = stream.ReadUInt16(Endian.Big);
            ushort Unk = stream.ReadUInt16(Endian.Big);

            Entrie[] entries = new Entrie[NrEntries];
            for (int i = 0; i < NrEntries; i++)
            {
                entries[i] = new Entrie(stream);
            }

            Root = new ArchiveDirectory() { OwnerArchive = this };

            for (int i = 0; i < NrEntries; i++)
            {
                stream.Seek(entries[i].name, SeekOrigin.Begin);
                string name = stream.ReadString();

                Root.AddArchiveFile(stream, entries[i].size, entries[i].data, name);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private class Entrie
        {
            public uint Unk;
            public uint name;
            public uint data;
            public uint size;

            public Entrie(Stream stream)
            {
                Unk = stream.ReadUInt32(Endian.Big);
                name = stream.ReadUInt32(Endian.Big);
                data = stream.ReadUInt32(Endian.Big);
                size = stream.ReadUInt32(Endian.Big);
            }
        }
    }
}
