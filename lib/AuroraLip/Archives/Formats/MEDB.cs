using AuroraLib.Common;
using AuroraLib.Common.Struct;

namespace AuroraLib.Archives.Formats
{
    // Rune Factory (Tides) archive format
    public class MEDB : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("MEDB");

        public MEDB()
        { }

        public MEDB(string filename) : base(filename)
        {
        }

        public MEDB(Stream stream, string filename = null) : base(stream, filename)
        {
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            Root = new ArchiveDirectory() { OwnerArchive = this };

            // We know there are textures here, just search for them
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
