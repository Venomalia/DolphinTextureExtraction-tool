using AuroraLip.Common;
using AuroraLip.Compression;
using AuroraLip.Compression.Formats;

namespace AuroraLip.Archives.Formats
{
    /// <summary>
    /// The .pak format used in Metroid Prime 3 and Donkey Kong Country Returns
    /// </summary>
    // base https://www.metroid2002.com/retromodding/wiki/PAK_(Metroid_Prime_3)#Header
    public class PAK_RetroWii : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".pak";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension.ToLower().Equals(Extension) && stream.ReadUInt32(Endian.Big) == 2;


        protected override void Read(Stream stream)
        {
            //Header
            uint Version = stream.ReadUInt32(Endian.Big);
            uint HeaderSize = stream.ReadUInt32(Endian.Big);
            byte[] MD5hash = stream.Read(16);

            stream.Seek(HeaderSize, SeekOrigin.Begin);


            //Table of Contents
            uint Sections = stream.ReadUInt32(Endian.Big);

            uint STRG_SectionSize = 0, RSHD_SectionSize = 0, DATA_SectionSize = 0;
            for (int i = 0; i < Sections; i++)
            {
                switch (stream.ReadString(4))
                {
                    case "STRG":
                        STRG_SectionSize = stream.ReadUInt32(Endian.Big);
                        break;
                    case "RSHD":
                        RSHD_SectionSize = stream.ReadUInt32(Endian.Big);
                        break;
                    case "DATA":
                        DATA_SectionSize = stream.ReadUInt32(Endian.Big);
                        break;
                    default:
                        throw new Exception("unknown section");
                }
            }

            //NameTabel
            stream.Seek(128, SeekOrigin.Begin);

            Sections = stream.ReadUInt32(Endian.Big);
            Dictionary<ulong, NameEntry> NameTable = new Dictionary<ulong, NameEntry>();
            for (int i = 0; i < Sections; i++)
            {
                NameEntry entry = new NameEntry(stream);
                if (!NameTable.ContainsKey(entry.ID))
                {
                    NameTable.Add(entry.ID, entry);
                }
                else
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(PAK_RetroWii)},{entry.ID} is already in name table. string:{entry.Name}.");
                }
            }

            //ResourceTable
            stream.Seek(128 + STRG_SectionSize, SeekOrigin.Begin);

            Sections = stream.ReadUInt32(Endian.Big);
            List<AssetEntry> AssetTable = new List<AssetEntry>();
            for (int i = 0; i < Sections; i++)
            {
                AssetEntry entry = new AssetEntry(stream);
                AssetTable.Add(entry);
            }

            //DataTable
            long DATAStart = 128 + STRG_SectionSize + RSHD_SectionSize;
            stream.Seek(DATAStart, SeekOrigin.Begin);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            foreach (AssetEntry entry in AssetTable)
            {
                string name;
                if (NameTable.TryGetValue(entry.ID, out NameEntry nameEntry))
                {
                    name = $"{entry.ID}_{nameEntry.Name}.{nameEntry.Type}";
                }
                else
                {
                    name = $"{entry.ID}.{entry.Type}";
                }
                if (Root.Items.ContainsKey(name)) continue;

                if (entry.Compressed)
                {
                    stream.Seek(entry.Offset + DATAStart, SeekOrigin.Begin);
                    string Type = stream.ReadString(4);
                    if (Type != "CMPD")
                    {
                        Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(PAK_Retro)},{name} type:{Type} is not known.");
                    }
                    uint blocks = stream.ReadUInt32(Endian.Big);

                    CMPDEntry[] CMPD = new CMPDEntry[blocks];
                    for (int i = 0; i < blocks; i++)
                    {
                        CMPD[i] = new CMPDEntry(stream);
                    }

                    //DKCR = Zlip & prime 3 = LZO1X-999 
                    Stream MS = new MemoryStream();
                    for (int i = 0; i < CMPD.Length; i++)
                    {
                        if (CMPD[i].DeSize == (int)CMPD[i].CoSize)
                        {
                            MS.Write(stream.Read(CMPD[i].DeSize), 0, (int)CMPD[i].DeSize);
                            continue;
                        }
                        Stream es = new SubStream(stream, (int)CMPD[i].CoSize);
                        if (Compression<ZLib>.IsMatch(es))
                        {
                            es.Seek(0, SeekOrigin.Begin);
                            ZLib zLib = new ZLib();
                            MS.Write(zLib.Decompress(es.Read((int)es.Length), (int)CMPD[i].DeSize).ToArray(), 0, (int)CMPD[i].DeSize);
                        }
                        else
                        {
                            es.Seek(0, SeekOrigin.Begin);
                            Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(PAK_Retro)},{entry.ID} LZO is not supported.");
                            name += ".LZO";
                            MS = es;

                        }
                    }
                    ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = name, FileData = MS };
                    Root.Items.Add(Sub.Name, Sub);
                }
                else
                {
                    Root.AddArchiveFile(stream, entry.Size, entry.Offset + DATAStart, name);
                }
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private class CMPDEntry
        {
            public byte Flag;

            public UInt24 CoSize;

            public int DeSize;

            public CMPDEntry(Stream stream)
            {
                Flag = (byte)stream.ReadByte();
                CoSize = stream.ReadUInt24(Endian.Big);
                DeSize = stream.ReadInt32(Endian.Big);
            }
        }

        private class NameEntry
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

        private class AssetEntry
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
