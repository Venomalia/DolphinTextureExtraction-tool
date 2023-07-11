using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class CMN : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".cmn";

        public bool IsMatch(Stream stream, in string extension = "")
            => extension.ToLower() == Extension && stream.ReadUInt32() < 200 && stream.ReadUInt32() != 0 && stream.ReadUInt32() == 1;

        protected override void Read(Stream stream)
        {
            uint files = stream.ReadUInt32();
            Root = new ArchiveDirectory() { OwnerArchive = this };

            for (int i = 0; i < files; i++)
            {
                FileEntrie entrie = stream.Read<FileEntrie>();
                Root.AddArchiveFile(stream, entrie.Size, entrie.Offset, $"File_{i}_{entrie.hash}");
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct FileEntrie
        {
            public uint hash; // ?
            public uint Unknown; // 0x1
            public uint Offset;
            public uint Size;
        }
    }
}
