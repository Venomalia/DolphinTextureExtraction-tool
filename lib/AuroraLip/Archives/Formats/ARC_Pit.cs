using AuroraLib.Common.Node;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Edge of Reality Pitfall Archive.
    /// </summary>
    public sealed class ARC_Pit : ArchiveNode
    {
        private const string ExtensionARC = ".arc";

        public override bool CanWrite => false;

        public ARC_Pit()
        {
        }

        public ARC_Pit(string name) : base(name)
        {
        }

        public ARC_Pit(FileNode source) : base(source)
        {
        }


        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (extension.Contains(ExtensionARC, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    int fsOffset = stream.ReadInt32();
                    stream.Seek(fsOffset, SeekOrigin.Begin);
                    Endian endian = stream.DetectByteOrder<uint>(3);
                    stream.Seek(fsOffset + 8, SeekOrigin.Begin);
                    int Offset = stream.ReadInt32(endian);
                    return Offset == 4;
                }
                catch (Exception) { }
            }
            return false;
        }

        protected override void Deserialize(Stream source)
        {
            int fsOffset = source.ReadInt32();
            source.Seek(fsOffset, SeekOrigin.Begin);
            Endian endian = source.DetectByteOrder<uint>(3);
            int filesCount = source.ReadInt32(endian);

            for (int i = 0; i < filesCount; i++)
            {
                uint CRC = source.ReadUInt32(endian);
                uint Offset = source.ReadUInt32(endian);
                uint Size = source.ReadUInt32(endian);
                string Name = source.ReadString();
                long Timestamp = source.ReadInt64(endian);
                Add(new FileNode(Name, new SubStream(source, Size, Offset)));
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        /// <summary>
        /// currently not needed. to read index.ind
        /// </summary>
        public class IND
        {
            public List<ArchiveInfo> ArchiveInfos = new();

            protected void Read(Stream stream)
            {
                Endian endian = stream.DetectByteOrder<uint>(4);

                int archives = stream.ReadInt32(endian) >> 1;
                Offset[] OffsetTable = stream.For(archives, s => s.Read<Offset>(endian));

                for (int i = 0; i < archives; i++)
                {
                    ArchiveInfos.Add(new ArchiveInfo(stream, OffsetTable[i], endian));
                }
            }

            public class ArchiveInfo
            {
                public string Name;
                public List<uint> FileCRCs = new();
                public List<FileInfo> FileInfos = new();

                public ArchiveInfo()
                { }

                internal ArchiveInfo(Stream stream, Offset offset, Endian endian)
                {
                    stream.Seek(offset.NameOffsets, SeekOrigin.Begin);
                    string Name = stream.ReadString();
                    stream.Seek(offset.InfoOffsets, SeekOrigin.Begin);
                    uint filesCount = stream.ReadUInt32(endian);
                    for (int i = 0; i < filesCount; i++)
                    {
                        FileCRCs.Add(stream.ReadUInt32(endian));
                    }
                    for (int i = 0; i < filesCount; i++)
                    {
                        FileInfos.Add(stream.Read<FileInfo>(endian));
                    }
                }

                public struct FileInfo
                {
                    public uint Offset;
                    public uint Size;
                }
            }

            internal struct Offset
            {
                public uint NameOffsets;
                public uint InfoOffsets;
            }
        }
    }
}
