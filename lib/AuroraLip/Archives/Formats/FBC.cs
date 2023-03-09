using AuroraLip.Common;

namespace AuroraLip.Archives.Formats
{
    public class FBC : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        private const string Extension = "FBC";

        public FBC() { }

        public FBC(string filename) : base(filename) { }

        public FBC(Stream stream, string filename = null) : base(stream, filename) { }

        public bool IsMatch(Stream stream, in string extension = "")
        {
            return extension == Extension;
        }

        private const string Bres = "bresþÿ";

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };

            //we do not know the header, so we skip it
            stream.Seek(150, SeekOrigin.Begin);

            //FBC seem to contain only bres files
            while (stream.Search(Bres))
            {
                long entrystart = stream.Position;
                if (!stream.MatchString(Bres))
                    continue;
                ushort Version = stream.ReadUInt16(Endian.Big);
                uint TotalSize = stream.ReadUInt32(Endian.Big);

                if (TotalSize > stream.Length - entrystart)
                {
                    stream.Search(Bres);
                    TotalSize = (uint)(stream.Position - entrystart);
                }

                ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = $"entry_{TotalFileCount + 1}.bres" };
                Sub.FileData = new SubStream(stream, TotalSize, entrystart);
                Root.Items.Add(Sub.Name, Sub);

                stream.Position = entrystart + TotalSize;
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
