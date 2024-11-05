using AuroraLib.Common.Node;
using AuroraLib.Compression;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;
using System.Runtime.CompilerServices;

namespace AuroraLib.Archives.Formats.Nintendo
{
    /// <summary>
    /// Nintendo RARC Archive
    /// </summary>
    // ref https://www.lumasworkshop.com/wiki/RARC_(File_Format) an https://github.com/SuperHackio/Hack.io
    public sealed class RARC : ArchiveNode, IHasIdentifier
    {
        private const string SelfKey = ".";

        private const string ParentKey = "..";

        public override bool CanWrite => true;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("RARC");

        public bool KeepFileIDsSynced { get; set; } = true;

        public RARC()
        { }

        public RARC(string name) : base(name)
        { }

        public RARC(FileNode source) : base(source)
        { }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x64 && stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            // Header
            source.MatchThrow(_identifier);
            uint FileSize = source.ReadUInt32(Endian.Big);
            uint DataHeaderOffset = source.ReadUInt32(Endian.Big);
            uint DataOffset = source.ReadUInt32(Endian.Big) + DataHeaderOffset;
            uint DataLength = source.ReadUInt32(Endian.Big);
            uint MRAMSize = source.ReadUInt32(Endian.Big);
            uint ARAMSize = source.ReadUInt32(Endian.Big);
            uint DVDSize = source.ReadUInt32(Endian.Big);
            // Data Header
            source.Seek(DataHeaderOffset, SeekOrigin.Begin);
            uint directoryCount = source.ReadUInt32(Endian.Big);
            uint directoryTableOffset = source.ReadUInt32(Endian.Big) + DataHeaderOffset;
            uint entryCount = source.ReadUInt32(Endian.Big);
            uint entryTableOffset = source.ReadUInt32(Endian.Big) + DataHeaderOffset;
            uint stringTableSize = source.ReadUInt32(Endian.Big);
            uint stringTableOffset = source.ReadUInt32(Endian.Big) + DataHeaderOffset;
            ushort nextFreeFileID = source.ReadUInt16(Endian.Big);
            KeepFileIDsSynced = source.ReadByte() != 0x00;
            // Directory Table
            source.Seek(directoryTableOffset, SeekOrigin.Begin);
            using SpanBuffer<DirectoryT> directories = new(directoryCount);
            source.Read<DirectoryT>(directories, Endian.Big);
            // Entrie Table
            source.Seek(entryTableOffset, SeekOrigin.Begin);
            using SpanBuffer<EntrieT> files = new(entryCount);
            source.Read<EntrieT>(files, Endian.Big);

            //Processing
            Dictionary<uint, DirectoryNode> reference = new();
            foreach (DirectoryT dir in directories)
            {
                source.Seek(stringTableOffset + dir.NameOffset, SeekOrigin.Begin);
                string name = source.ReadCString();

                DirectoryNode currentNode;
                if (dir.Type == 1414483794) // is ROOT
                {
                    currentNode = this;
                    Name = name + ".arc";
                }
                else
                {
                    currentNode = new(name);
                }

                for (int n = (int)dir.FirstFileOffset; n < dir.FileCount + dir.FirstFileOffset; n++)
                {
                    source.Seek(stringTableOffset + files[n].NameOffset, SeekOrigin.Begin);
                    name = source.ReadCString();

                    if (files[n].Attribute == FileAttribute.DIRECTORY) //IsDirectory
                    {
                        if (name == SelfKey) // current node index
                            reference.Add(files[n].Offset, currentNode);
                        else if (name == ParentKey && (int)files[n].Offset != -1) // current node parent index
                            reference[files[n].Offset].Add(currentNode);
                    }
                    else //IsFile
                    {
                        string attribute = string.Empty;
                        if (files[n].Attribute.HasFlag(FileAttribute.LOAD_FROM_DVD))
                            attribute = "DVD";
                        else if (files[n].Attribute.HasFlag(FileAttribute.PRELOAD_TO_ARAM))
                            attribute = "ARAM";

                        if (files[n].Attribute.HasFlag(FileAttribute.YAZ0_COMPRESSED))
                            attribute += " YAZ0";

                        currentNode.Add(new FileNode(name, new SubStream(source, files[n].Size, DataOffset + files[n].Offset))
                        {
                            ID = (ushort)files[n].Index,
                            Properties = attribute,
                        });
                    }
                }
            }
        }

        protected override void Serialize(Stream dest)
        {
            // Build Data Tabel
            Dictionary<FileNode, uint> dataOffsets = new();
            using MemoryPoolStream dataTabel = new();
            BuildDataTabel(dataTabel, dataOffsets, out uint MRAMSize, out uint ARAMSize, out uint DVDSize);
            dataTabel.Seek(0, SeekOrigin.Begin);

            // Build String Tabel
            Dictionary<string, ushort> nameOffsets = new();
            using MemoryPoolStream nameTabel = new();
            nameOffsets.Add(SelfKey, (ushort)nameTabel.Position);
            nameTabel.WriteString(".", 0x0);
            nameOffsets.Add(ParentKey, (ushort)nameTabel.Position);
            nameTabel.WriteString("..", 0x0);
            BuildStringTabel(nameTabel, nameOffsets, this);
            nameOffsets.Add(Name, (ushort)nameTabel.Position);
            nameTabel.WriteString(Name, 0x0);
            nameTabel.Seek(0, SeekOrigin.Begin);

            // Build Entrie & Directory Table
            List<DirectoryT> directorieTable = new()
            {
                new(new("ROOT"), nameOffsets[Name], NameToHash(Name), (ushort)(Count + 2), 0)
            };
            List<EntrieT> entrieTable = new();
            BuildDirEntrieTabel(directorieTable, entrieTable, nameOffsets, dataOffsets, this, 0, -1);

            uint entryTableOffset = (uint)StreamEx.AlignPosition(directorieTable.Count * Unsafe.SizeOf<DirectoryT>(), 0x20) + 0x20;
            uint DataTableOffset = (uint)StreamEx.AlignPosition(entrieTable.Count * Unsafe.SizeOf<EntrieT>(), 0x20) + entryTableOffset;
            uint stringTableOffset = (uint)dataTabel.Length + DataTableOffset;
            // Write Header
            dest.Write(_identifier);
            dest.Write(0x20 + stringTableOffset + (uint)nameTabel.Length, Endian.Big); // Size
            dest.Write(0x20, Endian.Big); // DataHeaderOffset
            dest.Write(DataTableOffset, Endian.Big); // DataTableOffset
            dest.Write(MRAMSize, Endian.Big);
            dest.Write(ARAMSize, Endian.Big);
            dest.Write(DVDSize, Endian.Big);
            // Write Data Header
            dest.Write(directorieTable.Count, Endian.Big); // directoryCount
            dest.Write(0x20, Endian.Big); // directoryTableOffset
            dest.Write(entrieTable.Count, Endian.Big); // entryCount
            dest.Write(entryTableOffset, Endian.Big); // entryTableOffset
            dest.Write((uint)nameTabel.Length, Endian.Big); // stringTableSize
            dest.Write(stringTableOffset, Endian.Big); // stringTableOffset
            dest.Write((ushort)(directorieTable.Count + 1), Endian.Big); // nextFreeFileID
            dest.WriteByte(KeepFileIDsSynced ? (byte)1 : (byte)0); // KeepFileIDsSynced
            dest.WriteAlign(0x20);
            // Write Directorie Tables
            dest.Write(directorieTable, Endian.Big);
            dest.WriteAlign(0x20);
            // Write Entrie Tables
            dest.Write(entrieTable, Endian.Big);
            dest.WriteAlign(0x20);
            // Write Data + Name Tables
            dataTabel.WriteTo(dest);
            nameTabel.WriteTo(dest);
        }

        private static void BuildDirEntrieTabel(List<DirectoryT> directorieTable, List<EntrieT> entrieTable, Dictionary<string, ushort> nameOffsets, Dictionary<FileNode, uint> dataOffsets, DirectoryNode currentNode, int currentIndex, int parentIndex)
        {
            List<(int index, DirectoryNode directory)> subDirs = new();
            foreach (ObjectNode item in currentNode.Values)
            {
                if (item is FileNode file)
                {
                    FileAttribute attribute = FileAttribute.FILE;
                    if (file.Properties.Contains("ARAM"))
                        attribute |= FileAttribute.PRELOAD_TO_ARAM;
                    else if (file.Properties.Contains("DVD"))
                        attribute |= FileAttribute.LOAD_FROM_DVD;
                    else
                        attribute |= FileAttribute.PRELOAD_TO_MRAM;

                    if (file.Data.At(0, s => s.Match("Yaz0")))
                        attribute |= FileAttribute.YAZ0_COMPRESSED | FileAttribute.COMPRESSED;

                    entrieTable.Add(new((short)entrieTable.Count, NameToHash(file.Name), nameOffsets[file.Name], dataOffsets[file], (uint)file.Size, attribute));
                }
            }
            foreach (ObjectNode item in currentNode.Values)
            {
                if (item is DirectoryNode dir)
                {
                    Identifier32 type = new(dir.Name.ToUpper().PadRight(4).AsSpan()[..4]);
                    directorieTable.Add(new(type, nameOffsets[dir.Name], NameToHash(dir.Name), (ushort)(dir.Count + 2), (uint)directorieTable.Sum(s => s.FileCount)));
                    entrieTable.Add(new(NameToHash(dir.Name), nameOffsets[dir.Name], (uint)directorieTable.Count));
                    subDirs.Add(new(directorieTable.Count, dir));
                }
            }
            entrieTable.Add(new(NameToHash(SelfKey), nameOffsets[SelfKey], (uint)currentIndex));
            entrieTable.Add(new(NameToHash(ParentKey), nameOffsets[ParentKey], (uint)parentIndex));
            foreach (var (index, dir) in subDirs)
            {
                BuildDirEntrieTabel(directorieTable, entrieTable, nameOffsets, dataOffsets, dir, index, currentIndex);
            }
        }

        private void BuildDataTabel(Stream dataTabel, Dictionary<FileNode, uint> dataOffsets, out uint MRAMSize, out uint ARAMSize, out uint DVDSize)
        {
            //Files in DataTabel must be sorted by load type, first MRAM, then ARAM and finally DVD.
            List<FileNode> fileARAM = new();
            List<FileNode> fileDVD = new();

            // MRAM + sorting
            Yaz0 yaz = new() { LookAhead = false, FormatByteOrder = Endian.Big };
            foreach (FileNode fileNode in GetAllValuesOf<FileNode>())
            {
                if (fileNode.Properties.Contains("YAZ0") && !fileNode.Data.At(0, s => s.Match("Yaz0")))
                {
                    MemoryPoolStream yaz0Stream = new();
                    yaz.Compress(fileNode.Data, yaz0Stream);
                    fileNode.Data.Dispose();
                    fileNode.Data = yaz0Stream;
                }

                if (fileNode.Properties.Contains("ARAM"))
                    fileARAM.Add(fileNode);
                else if (fileNode.Properties.Contains("DVD"))
                    fileDVD.Add(fileNode);
                else
                    WriteToDataTabel(fileNode);
            }
            MRAMSize = (uint)dataTabel.Position;

            // ARAM
            foreach (FileNode fileNode in fileARAM)
                WriteToDataTabel(fileNode);
            ARAMSize = (uint)dataTabel.Position - MRAMSize;

            // DVD
            foreach (FileNode fileNode in fileDVD)
                WriteToDataTabel(fileNode);
            DVDSize = (uint)dataTabel.Position - MRAMSize - ARAMSize;

            void WriteToDataTabel(FileNode file)
            {
                dataOffsets.Add(file, (uint)dataTabel.Position);
                file.Data.Seek(0, SeekOrigin.Begin);
                file.Data.CopyTo(dataTabel);
                file.Data.Seek(0, SeekOrigin.Begin);
                dataTabel.WriteAlign(32);
            }
        }

        private static void BuildStringTabel(Stream nameTabel, Dictionary<string, ushort> nameOffsets, DirectoryNode currentNode)
        {
            foreach (ObjectNode item in currentNode.Values)
            {
                if (nameOffsets.TryAdd(item.Name, (ushort)nameTabel.Position))
                    nameTabel.WriteString(item.Name, (byte)0);

                if (item is DirectoryNode dir)
                    BuildStringTabel(nameTabel, nameOffsets, dir);
            }
        }

        private static ushort NameToHash(string Input)
        {
            int Hash = 0;
            for (int i = 0; i < Input.Length; i++)
            {
                Hash *= 3;
                Hash += Input[i];
                Hash = 0xFFFF & Hash; //cast to short
            }

            return (ushort)Hash;
        }

        private readonly struct DirectoryT
        {
            public readonly Identifier32 Type;
            public readonly uint NameOffset;
            public readonly ushort NameHash;
            public readonly ushort FileCount;
            public readonly uint FirstFileOffset;

            public DirectoryT(Identifier32 type, uint nameOffset, ushort nameHash, ushort fileCount, uint firstFileOffset)
            {
                Type = type;
                NameOffset = nameOffset;
                NameHash = nameHash;
                FileCount = fileCount;
                FirstFileOffset = firstFileOffset;
            }
        }

        private readonly struct EntrieT
        {
            public readonly short Index;
            public readonly ushort NameHash;
            public readonly FileAttribute Attribute;
            public readonly byte Padding;
            public readonly ushort NameOffset;
            public readonly uint Offset;
            public readonly uint Size;
            public readonly uint Padding2;

            public EntrieT(ushort nameHash, ushort nameOffset, uint offset) : this()
            {
                Index = -1;
                NameHash = nameHash;
                NameOffset = nameOffset;
                Offset = offset;
                Size = 0x10;
                Attribute = FileAttribute.DIRECTORY;
            }

            public EntrieT(short iD, ushort nameHash, ushort nameOffset, uint offset, uint size, FileAttribute attribute) : this()
            {
                Index = iD;
                NameHash = nameHash;
                NameOffset = nameOffset;
                Offset = offset;
                Size = size;
                Attribute = attribute;
            }
        }

        /// <summary>
        /// File Attibutes
        /// </summary>
        [Flags]
        public enum FileAttribute : byte
        {
            /// <summary>
            /// Indicates this is a File
            /// </summary>
            FILE = 0x01,

            /// <summary>
            /// Directory.
            /// </summary>
            DIRECTORY = 0x02,

            /// <summary>
            /// Indicates that this file is compressed
            /// </summary>
            COMPRESSED = 0x04,

            /// <summary>
            /// Indicates that this file gets Pre-loaded into Main RAM
            /// </summary>
            PRELOAD_TO_MRAM = 0x10,

            /// <summary>
            /// Indicates that this file gets Pre-loaded into Auxiliary RAM (GameCube only) 
            /// </summary>
            PRELOAD_TO_ARAM = 0x20,

            /// <summary>
            /// Indicates that this file does not get pre-loaded, but rather read from the DVD
            /// </summary>
            LOAD_FROM_DVD = 0x40,

            /// <summary>
            /// Indicates that this file is YAZ0 Compressed
            /// </summary>
            YAZ0_COMPRESSED = 0x80
        }
    }
}
