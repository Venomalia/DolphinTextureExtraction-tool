using AuroraLib.Common;
using AuroraLib.Common.Node;
using System;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Treyarch CMN Archive.
    /// </summary>
    public sealed class CMN : ArchiveNode
    {
        public override bool CanWrite => false;

        public CMN()
        {
        }

        public CMN(string name) : base(name)
        {
        }

        public CMN(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(".cmn", StringComparison.InvariantCultureIgnoreCase) && stream.ReadUInt32() < 2048 && stream.ReadUInt32() != 0;

        protected override void Deserialize(Stream source)
        {
            uint files = source.ReadUInt32();
            for (int i = 0; i < files; i++)
            {
                FileEntrie entrie = source.Read<FileEntrie>();
                Add(new FileNode($"File_{i}_{entrie.Type}_{entrie.Hash}", new SubStream(source, entrie.Size, entrie.Offset)));
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private struct FileEntrie
        {
            public uint Hash; // ?
            public uint Type; //0x0 - 0x1
            public uint Offset;
            public uint Size;
        }
    }
}
