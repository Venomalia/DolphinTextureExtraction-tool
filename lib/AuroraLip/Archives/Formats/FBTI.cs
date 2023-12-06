using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    // Rune Factory (Frontier) archive format
    public class FBTI : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("FBTI");

        public FBTI()
        { }

        public FBTI(string filename) : base(filename)
        {
        }

        public FBTI(Stream stream, string filename = null) : base(stream, filename)
        {
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
            int version = int.Parse(stream.ReadString(4));
            uint file_count = stream.ReadUInt32(Endian.Big);
            uint start_offset = stream.ReadUInt32(Endian.Big); // always 0x10

            Span<FileEntry> entries = stackalloc FileEntry[(int)file_count];
            stream.Read(entries, Endian.Big);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < file_count; i++)
            {
                string name = NLCM.GetName(stream, entries[i].Offset, entries[i].Size, i);
                Root.AddArchiveFile(stream, entries[i].Size, entries[i].Offset, name);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

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
