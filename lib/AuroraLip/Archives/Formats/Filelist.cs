using AuroraLib.Common;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AuroraLib.Archives.Formats
{
    public class Filelist : Archive, IFileAccess
    {

        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".000";

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => extension == Extension;

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };
            //try to request an external file.
            string datname = Path.ChangeExtension(Path.GetFileNameWithoutExtension(FullPath), ".txt");
            try
            {
                reference_steam = FileRequest.Invoke(datname);
                List<TXTEntry> entryslist = ReadTXTEntrys(reference_steam);

                foreach (var entry in entryslist)
                {
                    Root.AddArchiveFile(stream, entry.Len, entry.Loc, entry.Paht);
                }

            }
            catch (Exception)
            {
                datname = Path.ChangeExtension(datname, ".bin");
                try
                {
                    reference_bin = FileRequest.Invoke(datname);

                    Endian endian = reference_bin.DetectByteOrder<uint>();

                    BinHeader header = reference_bin.Read<BinHeader>(endian);
                    BinEntry[] entrys = reference_bin.For((int)header.Files, s => new BinEntry(s, endian));


                    for (int i = 0; i < entrys.Length; i++)
                    {
                        if (!Root.Items.ContainsKey(entrys[i].Hash.ToString()))
                        {
                            Root.AddArchiveFile(stream, entrys[i].Size, entrys[i].Offsets[0].LocOffset, entrys[i].Hash.ToString());
                        }
                    }
                }
                catch (Exception)
                {
                    throw new Exception($"{nameof(Filelist)}: could not request the file {datname}.");
                }
            }
        }


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

        protected override void Write(Stream stream) => throw new NotImplementedException();

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

        private Stream reference_bin;
        private Stream reference_steam;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                reference_bin?.Dispose();
                reference_steam?.Dispose();
            }
        }

    }
}
