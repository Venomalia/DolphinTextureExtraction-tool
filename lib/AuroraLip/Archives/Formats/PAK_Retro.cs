using AuroraLip.Common;
using AuroraLip.Compression;
using AuroraLip.Compression.Formats;
using System;
using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    /// <summary>
    /// The .pak format used in Metroid Prime and Metroid Prime 2
    /// </summary>
    // base https://www.metroid2002.com/retromodding/wiki/PAK_(Metroid_Prime)#Header
    public class PAK_Retro : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".pak";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension.ToLower().Equals(Extension) && stream.ReadUInt16(Endian.Big) == 3 && stream.ReadUInt16(Endian.Big) == 5 && stream.ReadUInt32(Endian.Big) == 0;

        protected override void Read(Stream stream)
        {
            //Header
            ushort VersionMajor = stream.ReadUInt16(Endian.Big);
            ushort VersionMinor = stream.ReadUInt16(Endian.Big);
            uint Padding = stream.ReadUInt32(Endian.Big);
            if (VersionMajor != 3 && VersionMinor != 5 && Padding != 0)
                throw new InvalidIdentifierException($"{VersionMajor},{VersionMinor},{Padding}");

            //NameTabel
            uint Sections = stream.ReadUInt32(Endian.Big);
            Dictionary<uint, NameEntry> NameTable = new Dictionary<uint, NameEntry>();
            for (int i = 0; i < Sections; i++)
            {
                NameEntry entry = new NameEntry(stream);
                if (!NameTable.ContainsKey(entry.ID))
                {
                    NameTable.Add(entry.ID, entry);
                }
                else
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(PAK_Retro)},{entry.ID} is already in name table. string:{entry.Name}.");
                }
            }

            //ResourceTable
            Sections = stream.ReadUInt32(Endian.Big);
            List<AssetEntry> AssetTable = new List<AssetEntry>();
            for (int i = 0; i < Sections; i++)
            {
                AssetEntry entry = new AssetEntry(stream);
                AssetTable.Add(entry);
            }

            //DataTable
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
                    stream.Seek(entry.Offset, SeekOrigin.Begin);
                    uint DeSize = stream.ReadUInt32(Endian.Big);
                    Stream es = new SubStream(stream, entry.Size - 4);
                    //prime 1 = Zlip & prime 2 = LZO1X-999 
                    if (Compression<ZLib>.IsMatch(es))
                    {
                        es.Seek(0, SeekOrigin.Begin);
                        ZLib zLib = new ZLib();
                        es = zLib.Decompress(es.Read((int)es.Length), (int)DeSize);
                    }
                    else
                    {
                        es.Seek(0, SeekOrigin.Begin);
                        Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(PAK_Retro)},{entry.ID} LZO is not supported.");
                        name += ".LZO";

                    }
                    ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = name, FileData = es };
                    Root.Items.Add(Sub.Name, Sub);
                }
                else
                {
                    Root.AddArchiveFile(stream, entry.Size, entry.Offset, name);
                }
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private class NameEntry
        {
            public string Type;

            public uint ID;

            public string Name;

            public NameEntry(Stream stream)
            {
                Type = stream.ReadString(4);
                ID = stream.ReadUInt32(Endian.Big);
                int Length = (int)stream.ReadUInt32(Endian.Big);
                Name = stream.ReadString(Length);
            }
        }

        private class AssetEntry
        {
            public string Type;

            public uint ID;

            public bool Compressed;

            public uint Size;

            public uint Offset;

            public AssetEntry(Stream stream)
            {
                Compressed = stream.ReadUInt32(Endian.Big) == 1;
                Type = stream.ReadString(4);
                ID = stream.ReadUInt32(Endian.Big);
                Size = stream.ReadUInt32(Endian.Big);
                Offset = stream.ReadUInt32(Endian.Big);
            }
        }
    }
}
