using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class RSC : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".RSC";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension == Extension;

        protected override void Read(Stream stream)
        {
            _ = stream.Read(32);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            do
            {
                Entry entry = stream.Read<Entry>(Endian.Big);
                Root.AddArchiveFile(stream, (int)entry.Size, $"{entry.Flag}_entry{Root.Count}");
                if (entry.NextOffset == 0)
                {
                    break;
                }
                stream.Seek(entry.NextOffset, SeekOrigin.Begin);
            }
            while (true);
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct Entry
        {
            public uint Flag; //0-14, 1 = TPL
            public uint Size;
            public uint NextOffset;
            public uint Pad12;

            public uint Pad16;
            public uint Pad20;
            public uint Pad24;
            public uint Pad26;
        }
    }
}
