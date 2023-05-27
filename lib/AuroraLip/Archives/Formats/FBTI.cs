using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    // Rune Factory (Frontier) archive format
    public class FBTI : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "FBTI";

        public FBTI()
        { }

        public FBTI(string filename) : base(filename)
        {
        }

        public FBTI(Stream stream, string filename = null) : base(stream, filename)
        {
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);
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
