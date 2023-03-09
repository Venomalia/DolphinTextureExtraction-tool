using AuroraLip.Common;

namespace AuroraLip.Archives.Formats
{
    // ref https://github.com/nickworonekin/puyotools/blob/master/src/PuyoTools.Core/Archives/Formats/GvmArchive.cs
    public class GVMH : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "GVMH";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(Magic);

        protected override void Read(Stream stream)
        {
            if (!IsMatch(stream))
                throw new InvalidIdentifierException(Magic);

            uint entryOffset = stream.ReadUInt32() + 8;
            stream.Position++;
            byte properties = stream.ReadUInt8();

            ushort numEntries = stream.ReadUInt16(Endian.Big);
            Entry[] entries = new Entry[numEntries];
            for (int i = 0; i < numEntries; i++)
            {
                entries[i] = new Entry(stream, properties);
            }

            stream.Seek(entryOffset, SeekOrigin.Begin);
            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < numEntries; i++)
            {
                long offset = stream.Position;
                string Magic = stream.ReadString(4);
                int length = stream.ReadInt32();

                // Some Billy Hatcher textures have an oddity where the last texture length is 16 more than what it
                // actually should be.
                if (i == numEntries - 1 && stream.Position + length != stream.Length)
                    length += 16;

                if (Root.ItemKeyExists(entries[i].Name))
                {
                    Root.AddArchiveFile(stream, length + 8, offset, entries[i].Name + i);
                }
                else
                {
                    Root.AddArchiveFile(stream, length + 8, offset, entries[i].Name);
                }

                stream.Seek(length, SeekOrigin.Current);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private class Entry
        {
            public ushort Index { get; set; }
            public string Name { get; set; } = string.Empty;
            public ushort Format { get; set; }
            public ushort Dimension { get; set; }
            public int GlobalIndex { get; set; }

            public Entry() { }

            public Entry(Stream stream, byte properties)
            {
                bool hasFilenames = (properties & (1 << 3)) > 0;
                bool hasFormats = (properties & (1 << 2)) > 0;
                bool hasDimensions = (properties & (1 << 1)) > 0;
                bool hasGlobalIndexes = (properties & (1 << 0)) > 0;

                Index = stream.ReadUInt16(Endian.Big);
                if (hasFilenames) Name = stream.ReadString(28);
                if (hasFormats) Format = stream.ReadUInt16();
                if (hasDimensions) Dimension = stream.ReadUInt16();
                if (hasGlobalIndexes) GlobalIndex = stream.ReadInt32();
            }
        }
    }
}
