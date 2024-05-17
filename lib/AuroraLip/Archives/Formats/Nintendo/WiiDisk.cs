using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;
using AuroraLib.DiscImage.Dolphin;
using AuroraLib.DiscImage.Revolution;

namespace AuroraLib.Archives.Formats.Nintendo
{
    /// <summary>
    /// Wii Optical Disc (RVL-006) ISO Image
    /// </summary>
    // ref https://wiibrew.org/wiki/Wii_disc
    public class WiiDisk : GCDisk
    {
        private readonly List<Stream> WiiDiskEncrypter = new();

        public new HeaderBin Header { get => (HeaderBin)base.Header; set => header = value; }

        public WiiDisk()
        {
        }

        public WiiDisk(string name) : base(name)
        {
        }

        public WiiDisk(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 9280 && extension.Contains(".iso", StringComparison.InvariantCultureIgnoreCase) && new BootBin(stream).DiskType == DiskTypes.Wii;

        protected override void Deserialize(Stream source)
        {
            base.Header = new HeaderBin(source);
            ProcessPartitionData(source);
        }

        internal void ProcessPartitionData(Stream source)
        {
            source.Seek(0x40000, SeekOrigin.Begin);
            PartitionInfo partitions = new(source);
            Name = $"{Header.GameName} [{Header.GameID}]";

            foreach (var partition in partitions)
            {
                DirectoryNode parDirectory = new(partition.Type.ToString());
                parDirectory.Add(new FileNode("ticket.bin", new SubStream(source, Partition.TICKETsize, partition.TICKEToffse)));
                parDirectory.Add(new FileNode("tmd.bin", new SubStream(source, partition.TMDsize, partition.TMDoffse)));
                parDirectory.Add(new FileNode("cert.bin", new SubStream(source, partition.CERTsize, partition.CERToffse)));
                parDirectory.Add(new FileNode("h3.bin", new SubStream(source, Partition.H3size, partition.H3offset)));
                Add(parDirectory);

                //Shared files of each partition
                DirectoryNode DiscDirectory = new("disc");
                parDirectory.Add(new FileNode("header.bin", new SubStream(source, 512, 0)));
                parDirectory.Add(new FileNode("region.bin", new SubStream(source, 32, 319488)));
                parDirectory.Add(DiscDirectory);

                Stream wiiStream;
                if (Header.UseEncryption)
                {
                    wiiStream = new WiiDiskStream(source, source.Length - partition.DATAoffset, partition.DATAoffset, partition.Ticket.GetPartitionKey());
                    WiiDiskEncrypter.Add(wiiStream);
                }
                else
                {
                    wiiStream = new SubStream(source, source.Length - partition.DATAoffset, partition.DATAoffset);
                }

                DirectoryNode mainDirectory = new("null");
                ProcessData(wiiStream, parDirectory);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (Stream stream in WiiDiskEncrypter)
                {
                    stream.Dispose();
                }
            }
        }

        private class Partition
        {
            public const uint TICKETsize = 676;
            public const uint H3size = 98304;

            public V0Ticket Ticket;
            public TMD TMD;
            public long TICKEToffse;
            public uint TMDsize;
            public long TMDoffse { get => (tmdffse << 2) + TICKEToffse; set => tmdffse = (uint)((value - TICKEToffse) >> 2); }
            private uint tmdffse;
            public uint CERTsize;
            public long CERToffse { get => (certoffse << 2) + TICKEToffse; set => certoffse = (uint)((value - TICKEToffse) >> 2); }
            private uint certoffse;
            public long H3offset { get => (h3offse << 2) + TICKEToffse; set => h3offse = (uint)(value >> 2); }
            private uint h3offse;
            public long DATAsize { get => datasize << 2; set => datasize = (uint)(value >> 2); }
            private uint datasize;
            public long DATAoffset { get => (dataoffse << 2) + TICKEToffse; set => dataoffse = (uint)((value - TICKEToffse) >> 2); }
            private uint dataoffse;
            public PartitionTyp Type;

            public Partition(Stream stream, PartitionTyp type)
            {
                Type = type;
                TICKEToffse = stream.Position;
                Ticket = new V0Ticket(stream);
                TMDsize = stream.ReadUInt32(Endian.Big);
                tmdffse = stream.ReadUInt32(Endian.Big);
                CERTsize = stream.ReadUInt32(Endian.Big);
                certoffse = stream.ReadUInt32(Endian.Big);
                h3offse = stream.ReadUInt32(Endian.Big);
                dataoffse = stream.ReadUInt32(Endian.Big);
                datasize = stream.ReadUInt32(Endian.Big);
                TMD = new TMD(stream);
            }
        }

        public enum PartitionTyp : uint
        {
            //Game Partition
            DATA = 0,

            //Update Partition
            UPDATE = 1,

            //Channel installer
            CHANNEL = 2,

            //ELSE = default
        }

        private class PartitionInfo : List<Partition>
        {
            public PartitionInfo(Stream stream)
            {
                Span<EntryInfo> entrys = stackalloc EntryInfo[4];
                stream.Read(entrys, Endian.Big);
                foreach (EntryInfo item in entrys)
                {
                    stream.Seek(item.Offset, SeekOrigin.Begin);
                    using SpanBuffer<PartitionTableInfo> PTIs = new(item.Partition);
                    stream.Read<PartitionTableInfo>(PTIs, Endian.Big);
                    foreach (PartitionTableInfo PTI in PTIs)
                    {
                        stream.Seek(PTI.Offset, SeekOrigin.Begin);
                        //Partition Header
                        Add(new Partition(stream, PTI.Type));
                    }
                }
            }

            private struct EntryInfo
            {
                public uint Partition;
                public uint Offset { readonly get => offset << 2; set => offset = value >> 2; }
                private uint offset;
            }

            private struct PartitionTableInfo
            {
                public uint Offset { readonly get => offset << 2; set => offset = value >> 2; }
                private uint offset;
                public PartitionTyp Type;
            }

        }
    }
}
