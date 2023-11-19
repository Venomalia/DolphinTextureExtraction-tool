using AuroraLib.DiscImage.Dolphin;
using AuroraLib.DiscImage.Revolution;
using static AuroraLib.Archives.Formats.WiiDisk.PartitionInfo;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Wii Disc Image
    /// </summary>
    // ref https://wiibrew.org/wiki/Wii_disc
    public partial class WiiDisk : GCDisk
    {
        private readonly List<WiiDiskStream> WiiDiskEncrypter = new();

        public WiiDisk()
        { }

        public WiiDisk(string filename) : base(filename)
        {
        }

        public WiiDisk(Stream stream, string fullpath = null) : base(stream, fullpath)
        {
        }

        protected override void Read(Stream stream)
        {
            Header = new HeaderBin(stream);
            stream.Seek(262144, SeekOrigin.Begin);
            PartitionInfo Partitions = new(stream);

            Root = new() { Name = $"{Header.GameName} [{Header.GameID}]", OwnerArchive = this };

            foreach (var partition in Partitions)
            {
                ArchiveDirectory parDirectory = new(this, Root) { Name = partition.Type.ToString() };
                parDirectory.AddArchiveFile(stream, Partition.TICKETsize, partition.TICKEToffse, "ticket.bin");
                parDirectory.AddArchiveFile(stream, partition.TMDsize, partition.TMDoffse, "tmd.bin");
                parDirectory.AddArchiveFile(stream, partition.CERTsize, partition.CERToffse, "cert.bin");
                parDirectory.AddArchiveFile(stream, Partition.H3size, partition.H3offset, "h3.bin");
                Root.Items.Add(parDirectory.Name, parDirectory);

                //Shared files of each partition
                ArchiveDirectory DiscDirectory = new(this, parDirectory) { Name = "disc" };
                DiscDirectory.AddArchiveFile(stream, 512, 0, "header.bin");
                DiscDirectory.AddArchiveFile(stream, 32, 319488, "region.bin");
                parDirectory.Items.Add(DiscDirectory.Name, DiscDirectory);

                WiiDiskStream wiiStream = new(stream, stream.Length - partition.DATAoffset, partition.DATAoffset, partition.Ticket.GetPartitionKey());
                WiiDiskEncrypter.Add(wiiStream);

                ArchiveDirectory mainDirectory = ProcessData(wiiStream, false);
                foreach (var item in mainDirectory.Items)
                {
                    item.Value.Parent = parDirectory;
                    parDirectory.Items.Add(item.Key,item.Value);
                }
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (WiiDiskStream stream in WiiDiskEncrypter)
                {
                    stream.Dispose();
                }
            }
        }

        public partial class PartitionInfo : List<Partition>
        {
#pragma warning disable CA2014
            public PartitionInfo(Stream stream)
            {
                Span<EntryInfo> entrys = stackalloc EntryInfo[4];
                stream.Read(entrys, Endian.Big);
                foreach (EntryInfo item in entrys)
                {
                    stream.Seek(item.Offset, SeekOrigin.Begin);
                    Span<PartitionTableInfo> PTIs = stackalloc PartitionTableInfo[(int)item.Partition];
                    stream.Read(PTIs, Endian.Big);
                    foreach (PartitionTableInfo PTI in PTIs)
                    {
                        stream.Seek(PTI.Offset, SeekOrigin.Begin);
                        //Partition Header
                        Add(new Partition(stream, PTI.Type));
                    }
                }
            }
#pragma warning restore CA2014

            public class Partition
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
        }
    }
}
