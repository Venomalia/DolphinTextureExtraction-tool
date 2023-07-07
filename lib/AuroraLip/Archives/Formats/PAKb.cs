using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class PAKb : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("PAKb");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            uint file_count = stream.ReadUInt32(Endian.Big);

            FileData[] file_data = new FileData[file_count];
            for (uint i = 0; i < file_count; i++)
            {
                file_data[i] = new FileData(stream);
            }

            uint names_start = file_data[file_count - 1].offset;

            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (uint i = 0; i < file_count; i++)
            {
                stream.Seek(names_start, SeekOrigin.Begin);

                uint expected_crc = file_data[i].crc;
                uint crc = 0;
                string name = "";
                do
                {
                    crc = stream.ReadUInt32(Endian.Big);
                    uint name_size = stream.ReadUInt32(Endian.Big);
                    if (name_size == 0)
                    {
                        crc = expected_crc;
                    }
                    name = stream.ReadString((int)name_size);
                } while (expected_crc != crc);
                if (Root.Items.ContainsKey(name))
                {
                    name += "_" + i.ToString();
                }
                Root.AddArchiveFile(stream, file_data[i].size, file_data[i].offset, name);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private class FileData
        {
            public uint crc;
            public uint offset;
            public uint size;

            public FileData(Stream stream)
            {
                crc = stream.ReadUInt32(Endian.Big);
                offset = stream.ReadUInt32(Endian.Big);
                size = stream.ReadUInt32(Endian.Big);
            }
        }
    }
}
