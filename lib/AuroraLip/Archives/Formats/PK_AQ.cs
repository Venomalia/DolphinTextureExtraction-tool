using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;
using AuroraLib.Cryptography.Hash;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// AQ Interactive Archive
    /// </summary>
    //base on https://forum.xentax.com/viewtopic.php?f=10&t=5938
    public sealed class PK_AQ : ArchiveNode, IFileAccess
    {
        public override bool CanWrite => false;

        private const string FSExtension = ".pfs";

        private const string HeaderExtension = ".pkh";

        private const string DataExtension = ".pk";

        public PK_AQ()
        {
        }

        public PK_AQ(string name) : base(name)
        {
        }

        public PK_AQ(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (extension.Contains(HeaderExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                uint Entrys = stream.ReadUInt32(Endian.Big);
                return Entrys * 0x10 + 4 == stream.Length;
            }
            return false;
        }

        protected override void Deserialize(Stream source)
        {
            //try to request an external file.
            string datname = Path.GetFileNameWithoutExtension(Name);
            if (TryGetRefFile(datname + DataExtension, out FileNode refFile))
            {
                pkstream = refFile.Data;
            }
            else
            {
                throw new Exception($"{nameof(PK_AQ)}: could not request the file {datname}.");
            }

            //Read PKH
            uint Entrys = source.ReadUInt32(Endian.Big);
            using SpanBuffer<HEntry> hEntries = new((int)Entrys);
            source.Read(hEntries.Span, Endian.Big);
            if (TryGetRefFile(datname + FSExtension, out refFile))
            {
                using Stream fsStream = refFile.Data;
                refFile.Data = new MemoryStream();

                //Read PFS
                FSHeader header = fsStream.Read<FSHeader>(Endian.Big);

                using SpanBuffer<DirectoryEntry> directories = new((int)header.Directorys);
                fsStream.Read(directories.Span, Endian.Big);
                using SpanBuffer<uint> directoryNameOffsets = new((int)header.Directorys);
                fsStream.Read(directoryNameOffsets.Span, Endian.Big);
                using SpanBuffer<uint> fileNameOffsets = new((int)header.Files);
                fsStream.Read(fileNameOffsets.Span, Endian.Big);
                long nameTabelPos = fsStream.Position;

                //Process
                Crc32 crc32 = new(Crc32Algorithm.BZIP2);
                Dictionary<int, DirectoryNode> directoryPairs = new();
                string rootPath = GetFullPath();
                for (int i = 0; i < directories.Length; i++)
                {
                    DirectoryEntry dirEntry = directories[i];
                    DirectoryNode dir = this;

                    if (directories[i].ParentIndex != -1)
                    {
                        fsStream.Seek(nameTabelPos + directoryNameOffsets[i], SeekOrigin.Begin);
                        string name = fsStream.ReadString();
                        dir = new(name);
                        directoryPairs[dirEntry.ParentIndex].Add(dir);
                    }
                    directoryPairs.Add(directories[i].Index, dir);

                    //Process FileEntrys
                    for (int fi = 0; fi < dirEntry.FileEntrys; fi++)
                    {
                        int FileIndex = dirEntry.FileStartChild + fi;
                        fsStream.Seek(nameTabelPos + fileNameOffsets[FileIndex], SeekOrigin.Begin);
                        string filename = fsStream.ReadString();
                        string filePath = Path.Combine(Path.GetRelativePath(rootPath, dir.GetFullPath()), filename).Replace('\\', '/').ToLower().Replace('?', 'L');
                        //Get CRC32
                        crc32.Reset();
                        crc32.Compute(filePath);
                        uint pathCRC32 = crc32.Value;

                        foreach (HEntry item in hEntries)
                        {
                            if (item.CRC32 == pathCRC32)
                            {
                                FileNode file = item.IsCompressed
                                    ? new($"{filename}.lz", new SubStream(pkstream, item.ComprSize, item.Offset))
                                    : new(filename, new SubStream(pkstream, item.DecomSize, item.Offset));
                                dir.Add(file);
                                break;
                            }
                        }
                    }
                    if (dir.Count != dirEntry.FileEntrys)
                    {

                    }
                }

                // Move Root items
                foreach (var item in directoryPairs[0])
                {
                    item.Value.MoveTo(this);
                }
            }
            else
            {
                //Process without PFS
                for (int i = 0; i < Entrys; i++)
                {
                    FileNode file = hEntries[i].IsCompressed
                        ? new($"{i}_{hEntries[i].CRC32}.lz", new SubStream(pkstream, hEntries[i].ComprSize, hEntries[i].Offset))
                        : new($"{i}_{hEntries[i].CRC32}.bin", new SubStream(pkstream, hEntries[i].DecomSize, hEntries[i].Offset));
                    Add(file);
                }
            }
        }


        public readonly struct FSHeader
        {
            public readonly uint Pad1;
            public readonly uint Pad2;
            public readonly uint Directorys;
            public readonly uint Files;
        }

        public readonly struct DirectoryEntry
        {
            public readonly int Index;
            public readonly int ParentIndex;
            public readonly int DirectoryStartChild;
            public readonly uint DirectoryEntrys;
            public readonly int FileStartChild;
            public readonly uint FileEntrys;
        }

        public readonly struct HEntry
        {
            public readonly uint CRC32;
            public readonly uint Offset;
            public readonly uint DecomSize;
            public readonly uint ComprSize;

            public readonly bool IsCompressed => ComprSize != 0;
        }

        private Stream pkstream;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (pkstream != null)
                {
                    pkstream.Dispose();
                }
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}
