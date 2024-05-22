using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Arika Endless Ocean Archive
    /// </summary>
    public sealed class RF2 : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("RF2M");

        public RF2()
        {
        }

        public RF2(string name) : base(name)
        {
        }

        public RF2(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            long lastEndPosition = source.Position;

            while (source.Search(_identifier.AsSpan()))
            {
                if (lastEndPosition != source.Position)
                    Add(new FileNode(Count + ".data", new SubStream(source, source.Position - lastEndPosition, lastEndPosition)));

                DirectoryNode folder = new("RF2M_" + Count);
                Add(folder);
                ProcessData(source, folder);
                source.Align(0x800);
                lastEndPosition = source.Position;
            }
        }

        private static void ProcessData(Stream source, DirectoryNode directory)
        {
            long headerPos = source.Position;
            Header header = source.Read<Header>();
            long lastEndPosition = headerPos + header.Size;
            directory.Add(new FileNode(directory.Count + ".data", new SubStream(source, header.DataSize, header.HeaderSize + headerPos)));

            for (int i = 0; i < header.Files; i++)
            {
                string name = source.ReadString(0x14);
                int size = source.ReadInt32();
                long offset = source.ReadInt32() + headerPos;
                int flag = source.ReadInt32();
                if (size != 0)
                {
                    source.At(offset, s =>
                    {
                        if (_identifier == source.Peek<Identifier32>()) // if folder
                        {
                            DirectoryNode folder = new(name);
                            directory.Add(folder);
                            ProcessData(source, folder);
                            if (lastEndPosition < source.Position)
                                lastEndPosition = source.Position;
                        }
                        else
                        {
                            if (directory.Contains(name))
                                name += directory.Count;

                            directory.Add(new FileNode(name, new SubStream(source, size, offset)));
                            if (lastEndPosition < offset + size)
                                lastEndPosition = offset + size;
                        }
                    });
                }
            }
            source.Seek(lastEndPosition, SeekOrigin.Begin);
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private readonly struct Header
        {
            public readonly UInt24 Identifier;
            public readonly UInt24 Type;
            public readonly short Files;
            public readonly UInt24 HeaderSize;
            public readonly byte unk; // 1
            public readonly int Size;
            public readonly int DataSize => Size - HeaderSize;
        }
    }
}
