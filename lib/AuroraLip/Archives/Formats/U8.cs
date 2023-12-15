using AuroraLib.Common;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{

    // Based on https://wiki.tockdom.com/wiki/U8_(File_Format) an https://github.com/SuperHackio/Hack.io
    /// <summary>
    /// Nintendo U8 Archive
    /// </summary>
    public class U8 : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("Uª8-");

        public U8()
        { }

        public U8(string filename) : base(filename)
        {
        }

        public U8(Stream stream, string filename = null) : base(stream, filename)
        {
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x30 && stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
            uint nodeTableOffset = stream.ReadUInt32(Endian.Big); //usually 0x20
            uint nodeSectionSize = stream.ReadUInt32(Endian.Big);
            uint dataSectionOffset = stream.ReadUInt32(Endian.Big);
            stream.Position += 0x10; //Skip 16 reserved bytes

            //Read Nods
            stream.Seek(nodeTableOffset, SeekOrigin.Begin);
            U8Node rootNode = stream.Read<U8Node>(Endian.Big);
            using SpanBuffer<U8Node> nodes = new(rootNode.Size);
            stream.Read(nodes.Span[1..], Endian.Big);

            long StringTableOffset = nodeTableOffset + (rootNode.Size * 0x0C);
            Root = new ArchiveDirectory() { Name = "root", OwnerArchive = this };
            Dictionary<int, ArchiveDirectory> directorys = new() { { 0, Root } };
            Stack<Tuple<ArchiveDirectory, int>> stack = new();
            stack.Push(new(Root, nodes.Length));
            for (int i = 1; i < nodes.Length; i++)
            {
                U8Node node = nodes[i];
                stream.Seek(StringTableOffset + node.NameOffset, SeekOrigin.Begin);
                string name = stream.ReadString();

                //if there is already an ellement with the names.
                if (stack.Peek().Item1.Items.ContainsKey(name))
                {
                    name = $"{Path.GetFileNameWithoutExtension(name.AsSpan())}_{i}{Path.GetExtension(name.AsSpan())}";
                }
                if (node.IsDirectory)
                {
                    ArchiveDirectory dir = new() { OwnerArchive = this, Parent = directorys[node.Offset], Name = name };
                    directorys[node.Offset].Items.Add(name, dir);
                    directorys.Add(i, dir);
                    stack.Push(new(dir, node.Size));
                }
                else
                {
                    stack.Peek().Item1.AddArchiveFile(stream, node.Size, node.Offset, name);
                }

                if (i == stack.Peek().Item2 - 1)
                {
                    stack.Pop();
                }
            }
        }

        protected override void Write(Stream stream)
        {
            // Create Note Tabel
            U8Node rootNode = new(true, 0, 0, TotalFileCount + 1);
            using SpanBuffer<U8Node> nodeTabel = new(rootNode.Size);
            nodeTabel[0] = rootNode;

            // Build String Tabel
            using MemoryPoolStream stringTabel = new();
            Dictionary<string, UInt24> stringOffsets = new(nodeTabel.Length);
            stringTabel.WriteByte(0); // Root byte
            BuildStringTabel(Root);

            //Write Header
            uint nodeSectionSize = (uint)(nodeTabel.Length * 0x0C + stringTabel.Length);
            uint dataSectionOffset = nodeSectionSize + 0x20;
            stream.Write(_identifier);
            stream.Write(0x20, Endian.Big); // Node Table Offset
            stream.Write(nodeSectionSize, Endian.Big); // Node Section Size
            stream.Write(dataSectionOffset, Endian.Big); // File Data Offset
            stream.Write(0, 4); // reserved bytes

            //Write Node Section
            long nodeSectionPosition = stream.Position;
            stream.Write<U8Node>(nodeTabel); // Node Tabel Placeholder
            stringTabel.WriteTo(stream); // String Tabel

            //Write Data Section
            long DataSectionPosition = stream.Position;
            List<ArchiveDirectory> directorys = new() { Root };
            WriteDataSection(Root, 1);

            //Write Real Nodes
            stream.At(nodeSectionPosition, s => s.Write<U8Node>(nodeTabel, Endian.Big));

            void BuildStringTabel(ArchiveDirectory parent)
            {
                foreach (ArchiveObject item in parent.Items.Values)
                {
                    if (!stringOffsets.ContainsKey(item.Name))
                    {
                        stringOffsets.Add(item.Name, (UInt24)stringTabel.Position);
                        stringTabel.WriteString(item.Name, (byte)0);
                    }
                    if (item is ArchiveDirectory directory)
                    {
                        BuildStringTabel(directory);
                    }
                }
            }

            void WriteDataSection(ArchiveDirectory parent, int index)
            {
                // Process the files first then the directories.
                foreach (ArchiveObject item in parent.Items.Values)
                {
                    if (item is ArchiveFile file)
                    {
                        UInt24 nameOffset = stringOffsets.GetValueOrDefault(file.Name);
                        int dataOffset = (int)(stream.Position - DataSectionPosition + dataSectionOffset);
                        nodeTabel[index++] = new(false, nameOffset, dataOffset, (int)file.Size);
                        file.FileData.Position = 0;
                        file.FileData.CopyTo(stream);
                    }
                }
                foreach (ArchiveObject item in parent.Items.Values)
                {
                    if (item is ArchiveDirectory directory)
                    {
                        UInt24 nameOffset = stringOffsets.GetValueOrDefault(directory.Name);
                        int parentIndex = directorys.IndexOf(directory.Parent);
                        int itemEndOffset = directory.GetCountAndChildren() + index + 1;
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
