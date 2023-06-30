using AuroraLib.Common;
using AuroraLib.Common.Struct;

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

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
            string version = stream.ReadString(4);
            uint file_count = stream.ReadUInt32(Endian.Big);
            uint unknown = stream.ReadUInt32(Endian.Big);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (uint i = 0; i < file_count; i++)
            {
                uint file_offset = stream.ReadUInt32(Endian.Big);
                uint size = stream.ReadUInt32(Endian.Big);
                long saved_position = stream.Position;

                ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = i.ToString() };
                stream.Seek(file_offset, SeekOrigin.Begin);
                Sub.FileData = new SubStream(stream, size);
                Root.Items.Add(Sub.Name, Sub);

                // Read the file, move on to the next one
                stream.Seek(saved_position, SeekOrigin.Begin);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}
