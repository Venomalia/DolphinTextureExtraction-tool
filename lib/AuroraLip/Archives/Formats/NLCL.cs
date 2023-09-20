using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    // Rune Factory (Tides) archive format
    public class NLCL : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("NLCL");

        public NLCL()
        { }

        public NLCL(string filename) : base(filename)
        {
        }

        public NLCL(Stream stream, string filename = null) : base(stream, filename)
        {
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            Root = new ArchiveDirectory() { OwnerArchive = this };

            // This archive can have other things in it.  But it
            // isn't clear to me how the each file is sourced
            // there is a count but no offset...
            while (stream.Search("HXTB"))
            {
                long entrystart = stream.Position;
                if (!stream.Match("HXTB"))
                    continue;
                stream.Seek(0x14, SeekOrigin.Current);
                uint total_size = stream.ReadUInt32(Endian.Big);

                if (total_size > stream.Length - entrystart)
                {
                    stream.Search("HXTB");
                    total_size = (uint)(stream.Position - entrystart);
                }

                ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = $"entry_{TotalFileCount + 1}.hxtb" };
                Sub.FileData = new SubStream(stream, total_size, entrystart);
                Root.Items.Add(Sub.Name, Sub);

                stream.Position = entrystart + total_size;
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}
