using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class PAK_TM2 : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".pak";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if ((extension == Extension || extension == string.Empty || extension == ".cns") && stream.Length > 0x20)
            {
                uint entryCount = stream.ReadUInt32(Endian.Big);
                if (entryCount != 0 && entryCount < 1024 && stream.Position + entryCount * 8 < stream.Length)
                {
                    Entry[] entrys = stream.For((int)entryCount, s => s.Read<Entry>(Endian.Big));

                    for (int i = 0; i < entryCount - 1; i++)
                    {
                        if (entrys[i].Offset + entrys[i].Size > entrys[i + 1].Offset)
                        {
                            return false;
                        }
                    }
                    return entrys.First().Offset >= stream.Position && entrys.Last().Offset + entrys.Last().Size == stream.Length;
                }
            }
            return false;
        }

        protected override void Read(Stream stream)
        {
            uint entryCount = stream.ReadUInt32(Endian.Big);
            Entry[] entrys = stream.For((int)entryCount, s => s.Read<Entry>(Endian.Big));


            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < entrys.Length; i++)
            {
                Root.AddArchiveFile(stream, entrys[i].Size, entrys[i].Offset, $"Entry_{i}");
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct Entry
        {
            public uint Offset;
            public uint Size;
        }
    }
}
