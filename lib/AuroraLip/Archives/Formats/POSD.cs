using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
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

            if (!IsMatch(stream))
                throw new InvalidIdentifierException(Magic);

            uint dir_count = stream.ReadUInt32(Endian.Big);

            FolderEntry[] folders = stream.For((int)dir_count, s => s.Read<FolderEntry>(Endian.Big));
            byte[] pedding = stream.Read(8);

            Root = new ArchiveDirectory() { OwnerArchive = this };

            for (int f = 0; f < dir_count; f++)
            {
                ArchiveDirectory directory = new(this, Root) { Name = $"dir_{f}" };
                Root.Items.Add(directory.Name, directory);

                for (int i = 0; i < folders[f].FileCount; i++)
                {
                    FieleEntry file = stream.Read<FieleEntry>(Endian.Big);
                    directory.AddArchiveFile(reference_stream, file.Size, file.Offset, $"file_{i}");
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
        }

        private struct FieleEntry
        {
            private uint offset;
            public uint Size;

            public long Offset => offset << 11;
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
