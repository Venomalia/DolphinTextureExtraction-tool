using AuroraLib.Common.Node;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// H.a.n.d. Fables Chocobo archive.
    /// </summary>
    public sealed class FBC : ArchiveNode
    {
        public override bool CanWrite => false;

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.SequenceEqual("FBC");

        private const string Bres = "bresþÿ";

        public FBC()
        {
        }

        public FBC(string name) : base(name)
        {
        }

        public FBC(FileNode source) : base(source)
        {
        }

        protected override void Deserialize(Stream source)
        {
            //we do not know the header, so we skip it
            source.Seek(150, SeekOrigin.Begin);

            //FBC seem to contain only bres files
            while (source.Search(Bres))
            {
                long entrystart = source.Position;
                if (!source.Match(Bres))
                    continue;
                ushort Version = source.ReadUInt16(Endian.Big);
                uint TotalSize = source.ReadUInt32(Endian.Big);

                if (TotalSize > source.Length - entrystart)
                {
                    source.Search(Bres);
                    TotalSize = (uint)(source.Position - entrystart);
                }

                FileNode Sub = new($"entry_{Count + 1}.bres", new SubStream(source, TotalSize, entrystart));
                Add(Sub);

                source.Position = entrystart + TotalSize;
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}
