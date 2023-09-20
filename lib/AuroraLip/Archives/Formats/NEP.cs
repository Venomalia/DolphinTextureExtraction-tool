using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public sealed class NEP : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default) => extension == ".NEP";

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };
            while (stream.Position + 0x20 < stream.Length)
            {
                long pos = stream.Position;
                Entry entry = stream.Read<Entry>(Endian.Big);
                string name = stream.ReadString();
                name = name.Replace("..\\", string.Empty);
                if (!Root.Items.ContainsKey(name))
                {
                    Root.AddArchiveFile(stream, entry.Size, pos + entry.Offset, name);
                }
                stream.Seek(pos + entry.TotalSize, SeekOrigin.Begin);
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct Entry
        {
            public Types Type;
            public uint Offset;
            public uint Size;
            public uint TotalSize;

            public enum Types : uint
            {
                GVR = 0, //texture
                GNM = 1, //model
                PEF = 2,
            }
        }
    }
}
