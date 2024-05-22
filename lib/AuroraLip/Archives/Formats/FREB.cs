using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Square Enix Crystal Bearers Archive
    /// </summary>
    public sealed class FREB : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("FREB");

        public FREB()
        {
        }

        public FREB(string name) : base(name)
        {
        }

        public FREB(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier) && stream.Length < 67108864;

        protected override void Deserialize(Stream source)
        {
            source.Seek(0x20, SeekOrigin.Begin);
            FolderEntye A = new(source, Endian.Big);
            FolderEntye B = new(source, Endian.Big);

            FileEntye[] entyes = source.For(B.Files, s => new FileEntye(s, Endian.Big));


            for (int i = 0; i < entyes.Length; i++)
            {
                string name;
                if (entyes[i].Name.Length != 0)
                {
                    name = entyes[i].Name;
                }
                else
                {
                    long nameOffset = entyes[i].GetNameOffset();
                    if (nameOffset != -1)
                    {
                        source.Seek(nameOffset, SeekOrigin.Begin);
                        name = source.ReadString(8).TrimEnd('ÿ');
                    }
                    else
                    {
                        name = $"Entry";
                    }
                }
                name = $"{name}_{i}.{entyes[i].Type}";
                FileNode file = new(name, new SubStream(source, entyes[i].Size, entyes[i].Offset));
                Add(file);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private unsafe struct FolderEntye
        {
            public string Name;
            public ushort Folder;
            public ushort Files;
            public uint Offset;

            public FolderEntye(Stream stream, Endian endian)
            {
                Name = stream.ReadString(8).TrimEnd('0');
                Folder = stream.ReadUInt16(endian);
                Files = stream.ReadUInt16(endian);
                Offset = stream.ReadUInt32(endian);
            }
        }

        private unsafe struct FileEntye
        {
            public FileType Type;
            public uint Size;
            public uint Offset;
            public uint unk2;
            public string Name;
            public uint unk3;
            public uint unk4;

            public readonly long GetNameOffset() => Type switch
            {
                FileType.FMOT => Offset + 16,
                FileType.FSKL => Offset + 8,
                FileType.EEVB => Offset + 16,
                FileType.FMCD => Offset + 8,
                FileType.DYNB => Offset + 8,
                _ => -1,
            };

            public FileEntye(Stream stream, Endian endian)
            {
                Type = (FileType)stream.ReadUInt32(endian);
                Size = stream.ReadUInt32(endian);
                Offset = stream.ReadUInt32(endian);
                unk2 = stream.ReadUInt32(endian);
                Name = stream.ReadString(0x10);
                unk3 = stream.ReadUInt32(endian);
                unk4 = stream.ReadUInt32(endian);
            }

            public enum FileType : uint
            {
                BIN = 0,
                FFEF = 983040,
                FMOT = 65536,
                FSKL = 131072,
                EEVB = 393216,
                FSCE = 458752,
                FMCD = 196608,
                DYNB = 1310720,
                FCAM = 262144,
                SEDBSSCF = 1245184,
                FEFF = 786432, // It seems to have several model related data
                Bin3 = 327680,
                Bin4 = 524288,
            }
        }
    }
}
