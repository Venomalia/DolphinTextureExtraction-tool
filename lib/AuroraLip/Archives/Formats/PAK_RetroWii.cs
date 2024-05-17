using AuroraLib.Common;
using AuroraLib.Common.Node;

namespace AuroraLib.Archives.Formats.Retro
{
    /// <summary>
    /// The .pak format used in Metroid Prime 3 and Donkey Kong Country Returns
    /// </summary>
    // base https://www.metroid2002.com/retromodding/wiki/PAK_(Metroid_Prime_3)#Header
    public sealed class PAK_RetroWii : PAK_Retro
    {
        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public new static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(".pak", StringComparison.InvariantCultureIgnoreCase) && stream.ReadUInt32(Endian.Big) == 2 && stream.ReadUInt32(Endian.Big) == 64 && stream.ReadUInt64(Endian.Big) != 0 && stream.ReadUInt64(Endian.Big) != 0;

        protected override void Deserialize(Stream source)
        {
            //Header
            uint Version = source.ReadUInt32(Endian.Big);
            uint HeaderSize = source.ReadUInt32(Endian.Big);
            byte[] MD5hash = source.Read(16);

            source.Seek(HeaderSize, SeekOrigin.Begin);

            //Table of Contents
            uint Sections = source.ReadUInt32(Endian.Big);

            uint STRG_SectionSize = 0, RSHD_SectionSize = 0, DATA_SectionSize = 0;
            for (int i = 0; i < Sections; i++)
            {
                switch (source.ReadString(4))
                {
                    case "STRG":
                        STRG_SectionSize = source.ReadUInt32(Endian.Big);
                        break;

                    case "RSHD":
                        RSHD_SectionSize = source.ReadUInt32(Endian.Big);
                        break;

                    case "DATA":
                        DATA_SectionSize = source.ReadUInt32(Endian.Big);
                        break;

                    default:
                        throw new Exception("unknown section");
                }
            }

            //NameTabel
            source.Seek(128, SeekOrigin.Begin);

            Sections = source.ReadUInt32(Endian.Big);
            Dictionary<ulong, NameEntry> NameTable = new();
            for (int i = 0; i < Sections; i++)
            {
                NameEntry entry = new(source);
                if (!NameTable.ContainsKey(entry.ID))
                    NameTable.Add(entry.ID, entry);
                else
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(PAK_RetroWii)},{entry.ID} is already in name table. string:{entry.Name}.");
                }
            }

            //ResourceTable
            source.Seek(128 + STRG_SectionSize, SeekOrigin.Begin);

            Sections = source.ReadUInt32(Endian.Big);
            AssetEntry[] AssetTable = source.For((int)Sections, s => new AssetEntry(source));

            //DataTable
            long DATAStart = 128 + STRG_SectionSize + RSHD_SectionSize;
            source.Seek(DATAStart, SeekOrigin.Begin);

            foreach (AssetEntry entry in AssetTable)
            {
                string name = NameTable.TryGetValue(entry.ID, out NameEntry nameEntry)
                    ? $"{entry.ID}_{nameEntry.Name}.{nameEntry.Type}"
                    : $"{entry.ID}.{entry.Type}";

                if (Contains(name))
                    continue;

                if (entry.Compressed)
                {
                    source.Seek(entry.Offset + DATAStart, SeekOrigin.Begin);
                    string Type = source.ReadString(4);
                    if (Type != "CMPD")
                        Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(PAK_Retro)},{name} type:{Type} is not known.");
                    uint blocks = source.ReadUInt32(Endian.Big);
                    CMPDEntry[] CMPD = source.For((int)blocks, s => new CMPDEntry(source));

                    //DKCR = Zlip & prime 3 = LZO1X-999
                    MemoryPoolStream MS = new();
                    for (int i = 0; i < blocks; i++)
                    {
                        //Copy block if not compressed
                        if (CMPD[i].DeSize == (int)CMPD[i].CoSize)
                        {
                            MS.Write(source.Read((int)CMPD[i].DeSize), 0, (int)CMPD[i].DeSize);
                            continue;
                        }
                        // Decompress the data for this block
                        using Stream temStream = Decompress(new SubStream(source, (int)CMPD[i].CoSize), CMPD[i].DeSize);
                        MS.Write(temStream.ToArray().AsSpan());
                    }
                    FileNode file = new(name, MS);
                    Add(file);
                }
                else
                {
                    FileNode file = new(name, new SubStream(source, entry.Size, entry.Offset + DATAStart));
                    Add(file);
                }
            }
        }

        private struct CMPDEntry
        {
            public byte Flag;

            public UInt24 CoSize;

            public uint DeSize;

            public CMPDEntry(Stream stream)
            {
                Flag = (byte)stream.ReadByte();
                CoSize = stream.ReadUInt24(Endian.Big);
                DeSize = stream.ReadUInt32(Endian.Big);
            }
        }

        private new struct NameEntry
        {
            public string Name;

            public string Type;

            public ulong ID;

            public NameEntry(Stream stream)
            {
                Name = stream.ReadString();
                Type = stream.ReadString(4);
                ID = stream.ReadUInt64(Endian.Big);
            }
        }

        private new class AssetEntry
        {
            public string Type;
            public ulong ID;
            public bool Compressed;
            public uint Size;
            public uint Offset;

            public AssetEntry(Stream stream)
            {
                Compressed = stream.ReadUInt32(Endian.Big) == 1;
                Type = stream.ReadString(4);
                ID = stream.ReadUInt64(Endian.Big);
                Size = stream.ReadUInt32(Endian.Big);
                Offset = stream.ReadUInt32(Endian.Big);
            }
        }
    }
}
