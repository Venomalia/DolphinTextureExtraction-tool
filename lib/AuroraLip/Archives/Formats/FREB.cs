using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class FREB : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("FREB");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier) && stream.Length < 67108864;

        protected override void Read(Stream stream)
        {
            stream.Seek(0x20, SeekOrigin.Begin);
            FolderEntye A = new(stream, Endian.Big);
            FolderEntye B = new(stream, Endian.Big);

            FileEntye[] entyes = stream.For(B.Files, s => new FileEntye(s, Endian.Big));

            Root = new ArchiveDirectory() { OwnerArchive = this };

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
                        stream.Seek(nameOffset, SeekOrigin.Begin);
                        name = stream.ReadString(8).TrimEnd('ÿ');
                    }
                    else
                    {
                        name = $"Entry";
                    }
                }
                name = $"{name}_{i}.{entyes[i].Type}";
                Root.AddArchiveFile(stream, entyes[i].Size, entyes[i].Offset, name);
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

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

            public long GetNameOffset() => Type switch
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
                MODEL = 786432, // It seems to have several model related data
                Bin3 = 327680,
                Bin4 = 524288,
            }
        }
    }
}
