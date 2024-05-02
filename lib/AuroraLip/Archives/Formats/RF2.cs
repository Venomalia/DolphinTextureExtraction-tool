using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    public class RF2 : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("RF2M");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream source)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this, Name = "Root" };
            long endpos = source.Position;

            while (source.Search(_identifier.AsSpan()))
            {
                if (endpos != source.Position)
                    Root.AddArchiveFile(source, source.Position - endpos, endpos, "data" + Root.Items.Count);

                ArchiveDirectory folder = new() { OwnerArchive = this, Name = "data" + Root.Items.Count, Parent = Root };
                Root.Items.Add(folder.Name, folder);
                ProcessData(source, folder);
                source.Align(0x800);
                endpos = source.Position;
            }
        }

        private static void ProcessData(Stream source, ArchiveDirectory directory)
        {
            long headerPos = source.Position;
            Header header = source.Read<Header>();
            long endpos = headerPos + header.Size;
            directory.AddArchiveFile(source, header.DataSize, header.HeaderSize + headerPos, directory.Items.Count + ".data");
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
                            ArchiveDirectory folder = new() { OwnerArchive = directory.OwnerArchive, Name = name, Parent = directory };
                            directory.Items.Add(name, folder);
                            ProcessData(source, folder);
                            if (endpos < source.Position)
                                endpos = source.Position;
                        }
                        else
                        {
                            if (directory.Items.ContainsKey(name))
                                name += directory.Items.Count;
                            directory.AddArchiveFile(source, size, offset, name);
                            if (endpos < offset + size)
                                endpos = offset + size;
                        }
                    });
                }
            }
            source.Seek(endpos, SeekOrigin.Begin);
        }

        protected override void Write(Stream ArchiveFile) => throw new NotImplementedException();

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
