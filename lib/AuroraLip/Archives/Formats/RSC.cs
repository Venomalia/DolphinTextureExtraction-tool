using AuroraLib.Common;
using AuroraLib.Common.Node;
using System.Xml.Linq;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Treasure Wario World archive
    /// </summary>
    public sealed class RSC : ArchiveNode
    {
        public override bool CanWrite => false;

        public RSC()
        {
        }

        public RSC(string name) : base(name)
        {
        }

        public RSC(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.SequenceEqual(".RSC");

        protected override void Deserialize(Stream source)
        {
            Span<byte> unk = stackalloc byte[32];
            source.Read(unk);

            do
            {
                Entry entry = source.Read<Entry>(Endian.Big);
                FileNode file = new($"{entry.Flag}_entry{Count}", new SubStream(source, (int)entry.Size));
                Add(file);
                if (entry.NextOffset == 0)
                {
                    break;
                }
                source.Seek(entry.NextOffset, SeekOrigin.Begin);
            }
            while (true);
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

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
