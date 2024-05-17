using AuroraLib.Common;
using AuroraLib.Common.Node;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// SEGA NEP Archive
    /// </summary>
    public sealed class NEP : ArchiveNode
    {
        public override bool CanWrite => false;

        public NEP()
        {
        }

        public NEP(string name) : base(name)
        {
        }

        public NEP(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default) => extension == ".NEP";

        protected override void Deserialize(Stream source)
        {
            while (source.Position + 0x20 < source.Length)
            {
                long pos = source.Position;
                Entry entry = source.Read<Entry>(Endian.Big);
                string name = source.ReadString();
                name = name.Replace("..\\", string.Empty);
                if (!Contains(name))
                {
                    FileNode Sub = new(name, new SubStream(source, entry.Size, pos + entry.Offset));
                    Add(Sub);
                }
                source.Seek(pos + entry.TotalSize, SeekOrigin.Begin);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

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
