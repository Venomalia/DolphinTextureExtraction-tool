using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class FPK : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".fpk";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension.ToLower() == Extension;

        protected override void Read(Stream stream)
        {
            Endian order = stream.DetectByteOrder(typeof(uint), typeof(uint), typeof(uint), typeof(uint));
            int nameLength = stream.At(0x2f, s => s.ReadUInt8()) == 0 ? 32 : 16; // GC 16 Wii 32

            Header header = stream.Read<Header>(order);
            Entry[] entries = stream.For((int)header.Entrys, s => new Entry(s, order, nameLength));

            Root = new ArchiveDirectory() { Name = "root", OwnerArchive = this };
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].CompressedSize == entries[i].UncompressedSize)
                {
                    Root.AddArchiveFile(stream, entries[i].CompressedSize, entries[i].Offset, entries[i].Name);
                }
                else
                {
                    //Use PRS BigEndian
                    Root.AddArchiveFile(stream, entries[i].CompressedSize, entries[i].Offset, entries[i].Name + ".prs");
                }
            }
        }

        protected override void Write(Stream ArchiveFile) => throw new NotImplementedException();

        private struct Header
        {
            public uint Hash;
            public uint Entrys;
            public uint Offset;
            public uint Size;
        }

        private struct Entry
        {
            public string Name;
            public uint Unk;
            public uint Offset;
            public uint CompressedSize;
            public uint UncompressedSize;

            public Entry(Stream stream, Endian order, int nameLength)
            {
                Name = stream.ReadString(nameLength);
                Unk = stream.ReadUInt32(order);
                Offset = stream.ReadUInt32(order);
                CompressedSize = stream.ReadUInt32(order);
                UncompressedSize = stream.ReadUInt32(order);
            }
        }
    }
}
