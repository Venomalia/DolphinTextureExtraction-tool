using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// UbiSoft BIG Archive
    /// </summary>
    public class BIG : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new((byte)'B', (byte)'I', (byte)'G', 0);

        public BIG()
        {
        }

        public BIG(string name) : base(name)
        {
        }

        public BIG(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(Identifier);


        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(Identifier);
            ReadData(source);
        }

        protected void ReadData(Stream source)
        {
            Header header = source.Read<Header>();

            source.Seek(header.TableOffset, SeekOrigin.Begin);
            using SpanBuffer<InfoEntrie> infos = new(header.Files);
            source.Read<InfoEntrie>(infos);

            source.Seek(header.FileTableOffset, SeekOrigin.Begin);
            FileEntrie[] files = source.For((int)header.Files, s => new FileEntrie(s, header.Version));

            source.Seek(header.DirectoryTableOffset, SeekOrigin.Begin);
            DirectoryEntrie[] directorys = source.For((int)header.Directorys, s => new DirectoryEntrie(s));


            //Process Directorys
            Dictionary<int, DirectoryNode> directoryPairs = new()
            {
                { 0, this }
            };
            Name = directorys[0].Name;

            for (int i = 1; i < directorys.Length; i++)
            {
                DirectoryNode dir = new(directorys[i].Name);
                directoryPairs.Add(i, dir);
                if (directorys[i].ParentIndex != -1)
                {
                    directoryPairs[directorys[i].ParentIndex].Add(dir);
                }
            }

            //Process Files
            for (int i = 0; i < files.Length; i++)
            {
                directoryPairs[(int)files[i].DirectoryIndex].Add(new FileNode(files[i].Name, new SubStream(source, files[i].Size, infos[i].Offset)));
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

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

            public readonly uint FileTableOffset => TableOffset + (TableSize << 3);
            public readonly uint DirectoryTableOffset => FileTableOffset + TableSize * (Version == 0x24 ? (byte)0x58 : (byte)0x54);

        }

        public struct InfoEntrie
        {
            public uint Offset;
            public uint Unknown;
        }

        public class DirectoryEntrie
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

        public class FileEntrie
        {
            public uint Size;
            public int NextIndex;
            public int PreviousIndex;
            public uint DirectoryIndex;
            private uint timestamp;
            public string Name;
            public uint Unk1;
            public byte[] Hash;
            public uint Unk2;

            public DateTimeOffset Timestamp
            {
                get => DateTimeOffset.FromUnixTimeSeconds(timestamp);
                set => timestamp = (uint)value.ToUnixTimeSeconds();
            }

            public FileEntrie(Stream stream, uint version)
            {
                Size = stream.ReadUInt32();
                NextIndex = stream.ReadInt32();
                PreviousIndex = stream.ReadInt32();
                DirectoryIndex = stream.ReadUInt32();
                timestamp = stream.ReadUInt32();
                Name = stream.ReadString(64);
                if (version >= 36)
                    Unk1 = stream.ReadUInt32();
                if (version >= 44)
                {
                    Hash = stream.Read(32);
                    Unk2 = stream.ReadUInt32();
                }
            }
        }
    }
}
