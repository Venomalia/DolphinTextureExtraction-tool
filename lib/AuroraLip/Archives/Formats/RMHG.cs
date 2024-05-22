using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Grasshopper Manufacture RMHG Archive
    /// </summary>
    // base https://github.com/Zheneq/Noesis-Plugins/blob/b47579012af3b43c1e10e06639325d16ece81f71/fmt_fatalframe_rsl.py
    public sealed class RMHG : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("RMHG");

        public RMHG()
        {
        }

        public RMHG(string name) : base(name)
        {
        }

        public RMHG(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);

            uint count = source.ReadUInt32();
            uint DataOffset = source.ReadUInt32();
            uint unknown2 = source.ReadUInt32();
            uint dataSize = source.ReadUInt32();

            source.Seek(DataOffset, SeekOrigin.Begin);

            for (int i = 0; i < count; i++)
            {
                uint offset = source.ReadUInt32();
                uint size = source.ReadUInt32();
                uint[] unknown = new uint[6];// 0-2 unknown | 3-5 padding ?
                for (int r = 0; r < 6; r++)
                {
                    unknown[r] = source.ReadUInt32();
                }
                if (size != 0)
                    Add(new FileNode("Entry" + i, new SubStream(source, size, offset)));
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}
