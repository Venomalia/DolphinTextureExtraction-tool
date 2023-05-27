using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class ARC_Pit : Archive, IFileAccess
    {
        public const string Extension = ".arc";

        public bool CanRead => true;

        public bool CanWrite => false;

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (extension.ToLower() == Extension)
            {
                try
                {
                    int fsOffset = stream.ReadInt32();
                    stream.Seek(fsOffset, SeekOrigin.Begin);
                    Endian endian = stream.DetectByteOrder(typeof(int), typeof(int), typeof(int));
                    stream.Seek(fsOffset + 8, SeekOrigin.Begin);
                    int Offset = stream.ReadInt32(endian);
                    return Offset == 4;
                }
                catch (Exception) { }
            }
            return false;
        }

        protected override void Read(Stream stream)
        {
            int fsOffset = stream.ReadInt32();
            stream.Seek(fsOffset, SeekOrigin.Begin);
            Endian endian = stream.DetectByteOrder(typeof(int), typeof(int), typeof(int));
            int filesCount = stream.ReadInt32(endian);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < filesCount; i++)
            {
                uint CRC = stream.ReadUInt32(endian);
                uint Offset = stream.ReadUInt32(endian);
                uint Size = stream.ReadUInt32(endian);
                string Name = stream.ReadString();
                long Timestamp = stream.ReadInt64(endian);
                Root.AddArchiveFile(stream, Size, Offset, Name);
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        /// <summary>
        /// currently not needed. to read index.ind
        /// </summary>
        public class IND
        {
            public List<ArchiveInfo> ArchiveInfos = new();

            protected void Read(Stream stream)
            {
                Endian endian = stream.DetectByteOrder(typeof(int), typeof(int), typeof(int));

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
