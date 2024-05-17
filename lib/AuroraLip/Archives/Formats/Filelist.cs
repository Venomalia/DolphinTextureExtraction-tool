using AuroraLib.Common;
using AuroraLib.Common.Node;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Eurocom Archive
    /// </summary>
    public sealed class Filelist : ArchiveNode
    {
        public override bool CanWrite => false;


        public Filelist()
        {
        }

        public Filelist(string name) : base(name)
        {
        }

        public Filelist(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.SequenceEqual(".000");

        protected override void Deserialize(Stream source)
        {
            //try to request an external file.
            string datname = Path.GetFileNameWithoutExtension(Name);
            if (TryGetRefFile(datname + ".txt", out FileNode refFile))
            {
                Stream reference_txt = refFile.Data;
                List<TXTEntry> entryslist = ReadTXTEntrys(reference_txt);
                foreach (var entry in entryslist)
                {
                    Add(new FileNode(entry.Paht, new SubStream(source, entry.Len, entry.Loc)));
                }
            }
            if (TryGetRefFile(datname + ".bin", out refFile))
            {
                Stream reference_bin = refFile.Data;
                Endian endian = reference_bin.DetectByteOrder<uint>();

                BinHeader header = reference_bin.Read<BinHeader>(endian);
                BinEntry[] entrys = reference_bin.For((int)header.Files, s => new BinEntry(s, endian));
                for (int i = 0; i < entrys.Length; i++)
                {
                    if (!Contains(entrys[i].Hash.ToString()))
                    {
                        FileNode file = new(entrys[i].Hash.ToString(), new SubStream(source, entrys[i].Size, entrys[i].Offsets[0].LocOffset));
                        Add(file);
                    }
                }
            }
            else
            {
                throw new Exception($"{nameof(DICTPO)}: could not request the file {datname}.txt or {datname}.bin.");
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private List<TXTEntry> ReadTXTEntrys(Stream streamTXT)
        {
            List<TXTEntry> entries = new();
            try
            {
                using (StreamReader reader = new StreamReader(streamTXT))
                {
                    Regex regex = new(@"^(.*?):\s+Len\s+(\d+)\s+:\s+Ver\s+(\d+)\s+:\s+Hash\s+(0x[0-9a-fA-F]+)\s+:\s+Ts\s+(0x[0-9a-fA-F]+)\s+:\s+Loc\s+([\da-fA-F]+):([\da-fA-F]+)$");
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Match match = regex.Match(line);

                        if (match.Success)
                        {
                            entries.Add(new TXTEntry
                            {
                                Paht = Path.GetRelativePath(Path.GetPathRoot(match.Groups[1].Value), match.Groups[1].Value).TrimStart('\\'),
                                Len = uint.Parse(match.Groups[2].Value),
                                Hash = uint.Parse(match.Groups[4].Value.Substring(2), System.Globalization.NumberStyles.HexNumber),
                                Ts = uint.Parse(match.Groups[5].Value.Substring(2), System.Globalization.NumberStyles.HexNumber),
                                Loc = uint.Parse(match.Groups[6].Value, System.Globalization.NumberStyles.HexNumber)
                            }); ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Events.NotificationEvent.Invoke(NotificationType.Warning, $"Error when reading Filelist.txt : {ex.Message}");
            }
            return entries;
        }

        private struct BinHeader
        {
            public uint Version;
            public uint BinSize;
            public uint Files;
            public uint Unk; // 65536
            public uint Offset;

            public uint EntryTabelSize => Offset - 4;
            public uint NamePointerOffset => Offset + 16;
            public uint NameTabelOffset => NamePointerOffset + Files * 4;
        }


        struct TXTEntry
        {
            public string Paht;
            public uint Len;
            public uint Hash;
            public uint Ts;
            public uint Loc;

        }

        struct BinEntry
        {
            public uint Size;
            public uint Hash;
            public uint Ver;
            public uint UnkFlags;
            public Offset[] Offsets;

            public BinEntry(Stream stream, Endian endian)
            {
                Size = stream.ReadUInt32(endian);
                Hash = stream.ReadUInt32(endian);
                Ver = stream.ReadUInt32(endian);
                UnkFlags = stream.ReadUInt32(endian);
                int OffsetCount = stream.ReadInt32(endian);
                Offsets = stream.For(OffsetCount, s => s.Read<Offset>(endian));
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Offset
            {
                public uint LocOffset;
                public uint LocFile;
            }
        }
    }
}
