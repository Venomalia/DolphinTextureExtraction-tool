using AuroraLib.Common;
using System;

namespace AuroraLib.Archives.Formats
{
    public class CMN : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".cmn";

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase) && stream.ReadUInt32() < 2048 && stream.ReadUInt32() != 0;

        protected override void Read(Stream stream)
        {
            uint files = stream.ReadUInt32();
            Root = new ArchiveDirectory() { OwnerArchive = this };

            for (int i = 0; i < files; i++)
            {
                FileEntrie entrie = stream.Read<FileEntrie>();
                Root.AddArchiveFile(stream, entrie.Size, entrie.Offset, $"File_{i}_{entrie.Type}_{entrie.Hash}");
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct FileEntrie
        {
            public uint Hash; // ?
            public uint Type; //0x0 - 0x1
            public uint Offset;
            public uint Size;
        }
    }
}
