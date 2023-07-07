using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class POSD : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("POSD");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

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

            stream.MatchThrow(_identifier);

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
