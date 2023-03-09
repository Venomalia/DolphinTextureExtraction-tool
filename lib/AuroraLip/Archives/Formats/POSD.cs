using AuroraLip.Common;

namespace AuroraLip.Archives.Formats
{
    public class POSD : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "POSD";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(Magic);

        protected override void Read(Stream stream)
        {
            if (!IsMatch(stream))
                throw new InvalidIdentifierException(Magic);

            uint dir_count = stream.ReadUInt32(Endian.Big);

            FolderEntry[] folders = new FolderEntry[dir_count];
            for (int i = 0; i < dir_count; i++)
            {
                folders[i] = new FolderEntry(stream);
            }
            byte[] data = stream.Read(16);

            Root = new ArchiveDirectory() { OwnerArchive = this };

            //try to request an external file.
            string datname = Path.ChangeExtension(Path.GetFileNameWithoutExtension(FullPath), ".dat");
            try
            {
                reference_stream = FileRequest.Invoke(datname);
            }
            catch (Exception)
            {
                throw new Exception($"{nameof(POSD)}: could not request the file {datname}.");
            }
            //reference_stream = new FileStream(Path.ChangeExtension(this.FullPath, ".dat"), FileMode.Open, FileAccess.Read, FileShare.Read);

            for (int f = 0; f < dir_count; f++)
            {
                ArchiveDirectory directory = new ArchiveDirectory(this, Root) { Name = $"dir_{f}" };
                Root.Items.Add(directory.Name, directory);

                for (int i = 0; i < folders[f].FileCount; i++)
                {
                    uint file_offset = stream.ReadUInt32(Endian.Big) * 2048;
                    uint file_size = stream.ReadUInt32(Endian.Big);

                    directory.AddArchiveFile(reference_stream, file_size, file_offset, $"file_{i}");
                }

            }

        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private struct FolderEntry
        {
            public int Unk;
            public uint FileCount;

            public FolderEntry(Stream stream)
            {
                Unk = stream.ReadInt32(Endian.Big);
                FileCount = stream.ReadUInt32(Endian.Big);
            }
        }

        private Stream reference_stream;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (reference_stream != null)
                {
                    reference_stream.Dispose();
                }
            }
        }
    }
}
