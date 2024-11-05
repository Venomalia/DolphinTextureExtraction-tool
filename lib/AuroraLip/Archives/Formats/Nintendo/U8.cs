using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;
using System.IO;

namespace AuroraLib.Archives.Formats.Nintendo
{
    /// <summary>
    /// Nintendo U8 Archive
    /// </summary>
    // ref https://wiki.tockdom.com/wiki/U8_(File_Format) an https://github.com/SuperHackio/Hack.io
    public sealed class U8 : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => true;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("Uª8-");

        public U8()
        { }

        public U8(string name) : base(name)
        { }

        public U8(FileNode source) : base(source)
        { }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x30 && stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);
            uint nodeTableOffset = source.ReadUInt32(Endian.Big); //usually 0x20
            uint nodeSectionSize = source.ReadUInt32(Endian.Big);
            uint dataSectionOffset = source.ReadUInt32(Endian.Big);
            source.Position += 0x10; //Skip 16 reserved bytes

            //Read Nods
            source.Seek(nodeTableOffset, SeekOrigin.Begin);
            U8Node rootNode = source.Read<U8Node>(Endian.Big);
            using SpanBuffer<U8Node> nodes = new(rootNode.Size);
            source.Read(nodes.Span[1..], Endian.Big);

            long StringTableOffset = nodeTableOffset + (rootNode.Size * 0x0C);
            Dictionary<int, DirectoryNode> directorys = new() { { 0, this } };
            Stack<ValueTuple<DirectoryNode, int>> stack = new();
            stack.Push(new(this, nodes.Length));
            for (int i = 1; i < nodes.Length; i++)
            {
                U8Node node = nodes[i];
                source.Seek(StringTableOffset + node.NameOffset, SeekOrigin.Begin);
                string name = source.ReadCString();

                //if there is already an ellement with the names.
                if (stack.Peek().Item1.Contains(name))
                {
                    name = $"{Path.GetFileNameWithoutExtension(name.AsSpan())}_{i}{Path.GetExtension(name.AsSpan())}";
                }
                if (node.IsDirectory)
                {
                    DirectoryNode dir = new(name);
                    directorys[node.Offset].Add(dir);
                    directorys.Add(i, dir);
                    stack.Push(new(dir, node.Size));
                }
                else
                {
                    if (name.Contains('\\') || name.Contains('/'))
                        stack.Peek().Item1.AddPath(name, new FileNode(Path.GetFileName(name), new SubStream(source, node.Size, node.Offset)));
                    else
                        stack.Peek().Item1.Add(new FileNode(name, new SubStream(source, node.Size, node.Offset)));
                }

                if (i == stack.Peek().Item2 - 1)
                {
                    stack.Pop();
                }
            }
        }

        protected override void Serialize(Stream dest)
        {
            // Create Note Tabel
            U8Node rootNode = new(true, 0, 0, TotalItemsCountOf<FileNode>() + 1);
            using SpanBuffer<U8Node> nodeTabel = new(rootNode.Size);
            nodeTabel[0] = rootNode;

            // Build String Tabel
            using MemoryPoolStream stringTabel = new();
            Dictionary<string, UInt24> stringOffsets = new(nodeTabel.Length);
            stringTabel.WriteByte(0); // Root byte
            BuildStringTabel(this);

            //Write Header
            uint nodeSectionSize = (uint)(nodeTabel.Length * 0x0C + stringTabel.Length);
            uint dataSectionOffset = nodeSectionSize + 0x20;
            dest.Write(_identifier);
            dest.Write(0x20, Endian.Big); // Node Table Offset
            dest.Write(nodeSectionSize, Endian.Big); // Node Section Size
            dest.Write(dataSectionOffset, Endian.Big); // File Data Offset
            dest.Write(0, 4); // reserved bytes

            //Write Node Section
            long nodeSectionPosition = dest.Position;
            dest.Write<U8Node>(nodeTabel); // Node Tabel Placeholder
            stringTabel.WriteTo(dest); // String Tabel

            //Write Data Section
            long DataSectionPosition = dest.Position;
            List<DirectoryNode> directorys = new() { this };
            WriteDataSection(this, 1);

            //Write Real Nodes
            dest.At(nodeSectionPosition, s => s.Write<U8Node>(nodeTabel, Endian.Big));

            void BuildStringTabel(DirectoryNode currentNode)
            {
                foreach (ObjectNode item in currentNode.Values)
                {
                    if (stringOffsets.TryAdd(item.Name, (UInt24)stringTabel.Position))
                        stringTabel.WriteString(item.Name, (byte)0);

                    if (item is DirectoryNode subDir)
                        BuildStringTabel(subDir);
                }
            }

            void WriteDataSection(DirectoryNode currentNode, int index)
            {
                // Process the files first then the directories.
                foreach (ObjectNode item in currentNode.Values)
                {
                    if (item is FileNode file)
                    {
                        UInt24 nameOffset = stringOffsets.GetValueOrDefault(file.Name);
                        int dataOffset = (int)(dest.Position - DataSectionPosition + dataSectionOffset);
                        nodeTabel[index++] = new(false, nameOffset, dataOffset, (int)file.Size);
                        file.Data.Position = 0;
                        file.Data.CopyTo(dest);
                    }
                }
                foreach (ObjectNode item in currentNode.Values)
                {
                    if (item is DirectoryNode directory)
                    {
                        UInt24 nameOffset = stringOffsets.GetValueOrDefault(directory.Name);
                        int parentIndex = directorys.IndexOf(directory.Parent);
                        int itemEndOffset = directory.TotalItemsCountOf<FileNode>() + index + 1;
                        nodeTabel[index++] = new(true, nameOffset, parentIndex, itemEndOffset);
                        directorys.Add(directory);
                        WriteDataSection(directory, index);
                    }
                }
            }
        }

        internal readonly struct U8Node
        {
            private readonly byte isDirectoryByte;
            public readonly UInt24 NameOffset;
            public readonly int Offset;
            public readonly int Size;

            public bool IsDirectory => isDirectoryByte == 1;

            public U8Node(bool isDirectory, UInt24 nameOffset, int dataOffset, int size)
            {
                isDirectoryByte = isDirectory ? (byte)1 : (byte)0;
                NameOffset = nameOffset;
                Offset = dataOffset;
                Size = size;
            }
        }
    }
}
