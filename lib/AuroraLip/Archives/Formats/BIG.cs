using AuroraLib.Common;
using System.Runtime.InteropServices;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    public class BIG : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new((byte)'B', (byte)'I', (byte)'G', 0);

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier) && stream.ReadUInt8() == 0;

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            Header header = stream.Read<Header>();

            stream.Seek(header.TableOffset, SeekOrigin.Begin);
            InfoEntrie[] infos = stream.For((int)header.Files, s => s.Read<InfoEntrie>());

            stream.Seek(header.DirectoryTableOffset, SeekOrigin.Begin);
            DirectoryEntrie[] directorys = stream.For((int)header.Directorys, s => new DirectoryEntrie(s));

            stream.Seek(header.FileTableOffset, SeekOrigin.Begin);
            FileEntrie[] files = stream.For((int)header.Files, s => new FileEntrie(s));

            //Process Directorys
            Dictionary<int, ArchiveDirectory> directoryPairs = new();
            for (int i = 0; i < directorys.Length; i++)
            {
                ArchiveDirectory dir = new()
                {
                    OwnerArchive = this,
                    Name = directorys[i].Name,
                };
                directoryPairs.Add(i, dir);
                if (directorys[i].ParentIndex != -1)
                {
                    directoryPairs[directorys[i].ParentIndex].Items.Add(dir.Name, dir);
                    dir.Parent = directoryPairs[directorys[i].ParentIndex];
                }
            }
            Root = directoryPairs[0];

            //Process Files
            for (int i = 0; i < files.Length; i++)
            {
                directoryPairs[(int)files[i].DirectoryIndex].AddArchiveFile(stream, files[i].Size, infos[i].Offset, files[i].Name);
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        public struct Header
        {
            public uint Version;
            public uint Files;
            public uint Directorys;

            public int DUMMY16; // 0x0
            public int DUMMY20; // 0x0
            public int DUMMY24; // 0xFFFFFFFF
            public int DUMMY28; // 0xFFFFFFFF

            public uint TableSize;
            public int DUMMY36; // 0x1
            public uint Unknown;
            public uint Files_2;

            public uint Directorys_2;
            public uint TableOffset;
            public int DUMMY56; // 0xFFFFFFFF
            public int DUMMY60; // 0x0

            public uint TableSize2;

            public uint FileTableOffset => TableOffset + (TableSize << 3);
            public uint DirectoryTableOffset => FileTableOffset + TableSize * 0x54;

        }

        public unsafe struct InfoEntrie
        {
            public uint Offset;
            public uint Unknown;
        }

        public unsafe struct DirectoryEntrie
        {
            public uint FileOffset;
            public int SubDirectorys;
            public int NextIndex;
            public int PreviousIndex;
            public int ParentIndex;
            public string Name;

            public DirectoryEntrie(Stream stream)
            {
                FileOffset = stream.ReadUInt32();
                SubDirectorys = stream.ReadInt32();
                NextIndex = stream.ReadInt32();
                PreviousIndex = stream.ReadInt32();
                ParentIndex = stream.ReadInt32();
                Name = stream.ReadString(64);
            }
        }

        public unsafe struct FileEntrie
        {
            public uint Size;
            public int NextIndex;
            public int PreviousIndex;
            public uint DirectoryIndex;
            private uint timestamp;
            public string Name;

            public DateTimeOffset Timestamp
            {
                get => DateTimeOffset.FromUnixTimeSeconds(timestamp);
                set => timestamp = (uint)value.ToUnixTimeSeconds();
            }

            public FileEntrie(Stream stream)
            {
                Size = stream.ReadUInt32();
                NextIndex = stream.ReadInt32();
                PreviousIndex = stream.ReadInt32();
                DirectoryIndex = stream.ReadUInt32();
                timestamp = stream.ReadUInt32();
                Name = stream.ReadString(64);
            }
        }
    }
}
