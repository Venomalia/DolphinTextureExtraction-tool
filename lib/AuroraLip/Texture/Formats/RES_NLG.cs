using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class RES_NLG : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".res";
        public const string Extension2 = ".dmn";

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => (extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase) || extension.Contains(Extension2, StringComparison.InvariantCultureIgnoreCase) && stream.ReadInt32(Endian.Big) == 0x20 && stream.ReadInt32(Endian.Big) != 0 && stream.ReadInt32(Endian.Big) == 1);

        protected override void Read(Stream stream)
        {
            Header header = stream.Read<Header>(Endian.Big);
            stream.Seek(header.Offset, SeekOrigin.Begin);
            Entry[] entries = stream.For((int)header.Entrys, s => s.Read<Entry>(Endian.Big));
            for (int i = 0; i < entries.Length; i++)
            {
                stream.Seek(entries[i].Offset, SeekOrigin.Begin);

                if (PTLG.ReadTexture(stream, entries[i].Size, out TexEntry current))
                {
                    Add(current);
                }

            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct Header
        {
            public uint Offset; // 0x20
            public uint Entrys;
            public uint Version; //1
            private uint firstEntry;

            public long FirstEntryOffset
            {
                get => firstEntry << 5;
                set => firstEntry = (uint)(value >> 5);
            }
        }

        private struct Entry
        {
            public uint Hash;
            private uint offset;
            public uint Size;

            public long Offset
            {
                get => offset << 5;
                set => offset = (uint)(value >> 5);
            }
        }
    }
}
