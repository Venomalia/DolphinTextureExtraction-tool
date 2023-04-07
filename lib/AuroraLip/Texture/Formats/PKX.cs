using AuroraLib.Archives;
using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    // Genius Senority (Pokémon XD Gale of Darkness) PKX file (pokémon and some models?)
    public class PKX : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public PKX() { }

        public PKX(string filename) : base(filename) { }

        public PKX(Stream stream, string filename = null) : base(stream, filename) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => extension.ToLower() == ".PKX";

        protected override void Read(Stream stream)
        {
            Header header = stream.Read<Header>(Endian.Big);

            if (header.Unknown1A == 0x0C)
            {
                uint archive_begin = 0x84 + header.NumEntries * 208;
                archive_begin = (archive_begin + 31) & ~(uint)31; // Round to next 32-byte boundary
                archive_begin = (archive_begin + header.Unknown08 + 31) & ~(uint)31;

                Root = new ArchiveDirectory() { OwnerArchive = this };
                ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = "thing.GSscene" };
                stream.Seek(archive_begin, SeekOrigin.Begin);
                Sub.FileData = new SubStream(stream, header.ArchiveSize);
                Root.Items.Add(Sub.Name, Sub);
            }
            else
            {
                throw new NotImplementedException($"Unknown header value {header.Unknown1A}");
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        public struct Header
        {
            public uint ArchiveSize;
            public uint Unknown04;
            public uint Unknown08;
            public uint Unknown0C;
            public uint NumEntries;
            public uint Unknown14;
            public ushort Unknown18;
            public ushort Unknown1A;

        }

    }
}
