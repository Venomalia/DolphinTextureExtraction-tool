using AuroraLib.Archives.DiscImage;
using AuroraLib.Common;
using System.Security.Cryptography;
using static AuroraLib.Archives.Formats.GCDisk;
using static AuroraLib.Archives.Formats.WiiDisk.PartitionInfo;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Wii Disc Image
    /// </summary>
    // ref https://wiibrew.org/wiki/Wii_disc
    public partial class WiiDisk : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        private const uint MagicWord = 1562156707;

        public HeaderBin Header;

        public WiiDisk()
        { }

        public WiiDisk(string filename) : base(filename)
        {
        }

        public WiiDisk(Stream stream, string fullpath = null) : base(stream, fullpath)
        {
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (extension.Contains(".iso", StringComparison.InvariantCultureIgnoreCase) && stream.Length > 9280)
            {
                GameID gameID = (GameID)stream.Read(6);
                return Enum.IsDefined(typeof(SystemCode), gameID.SystemCode) && Enum.IsDefined(typeof(RegionCode), gameID.RegionCode) && stream.At(24, s => s.ReadUInt32(Endian.Big)) == MagicWord;
            }
            return false;
        }

        protected override void Read(Stream stream)
        {
            Header = new HeaderBin(stream);
            stream.Seek(262144, SeekOrigin.Begin);

            var Partitions = new PartitionInfo(stream);

            Root = new ArchiveDirectory() { Name = $"{Header.GameName} [{Header.GameID}]", OwnerArchive = this };

            foreach (var partition in Partitions)
            {
                ArchiveDirectory parDirectory = new ArchiveDirectory(this, Root) { Name = partition.Type.ToString() };
                parDirectory.AddArchiveFile(stream, partition.TICKETsize, partition.TICKEToffse, "ticket.bin");
                parDirectory.AddArchiveFile(stream, partition.TMDsize, partition.TMDoffse, "tmd.bin");
                parDirectory.AddArchiveFile(stream, partition.CERTsize, partition.CERToffse, "cert.bin");
                parDirectory.AddArchiveFile(stream, partition.H3size, partition.H3offset, "h3.bin");
                Root.Items.Add(parDirectory.Name, parDirectory);
                //parDirectory.AddArchiveFile(stream, partition.DATAsize, partition.DATAoffset, "data.bin");

                //Shared files of each partition
                ArchiveDirectory DiscDirectory = new ArchiveDirectory(this, parDirectory) { Name = "disc" };
                DiscDirectory.AddArchiveFile(stream, 512, 0, "header.bin");
                DiscDirectory.AddArchiveFile(stream, 32, 319488, "region.bin");
                parDirectory.Items.Add(DiscDirectory.Name, DiscDirectory);

                //WiiStream wiiStream = new WiiStream(stream, partition.DATAsize, partition.DATAoffset, partition.Ticket.GetPartitionKey());
                WiiStream wiiStream = new WiiStream(stream, stream.Length - partition.DATAoffset, partition.DATAoffset, partition.Ticket.GetPartitionKey());

                var Boot = (GCDisk.BootBin)stream.Read(1088);

                wiiStream.Seek(1056, SeekOrigin.Begin);
                uint DolOffset = wiiStream.ReadUInt32(Endian.Big) << 2;
                uint FSTableOffset = wiiStream.ReadUInt32(Endian.Big) << 2; //224512
                uint FSTableSize = wiiStream.ReadUInt32(Endian.Big);
                uint FSTableSizeMax = wiiStream.ReadUInt32(Endian.Big);
                uint apploaderOffset = wiiStream.ReadUInt32(Endian.Big);

                // get appLoaderDate
                wiiStream.Seek(9280, SeekOrigin.Begin);
                ReadOnlySpan<char> appLoaderDate = wiiStream.ReadString(10).AsSpan();
                LastWriteTimeUtc = new DateTime(int.Parse(appLoaderDate.Slice(0, 4).ToString()), int.Parse(appLoaderDate.Slice(5, 2).ToString()), int.Parse(appLoaderDate.Slice(8, 2).ToString()));

                ArchiveDirectory sysDirectory = new ArchiveDirectory(this, parDirectory) { Name = "sys" };
                sysDirectory.AddArchiveFile(wiiStream, 1088, 0, "boot.bin");
                sysDirectory.AddArchiveFile(wiiStream, 8192, 1088, "bi2.bin");
                sysDirectory.AddArchiveFile(wiiStream, DolOffset - 9280, 9280, "apploader.img");
                sysDirectory.AddArchiveFile(wiiStream, FSTableOffset - DolOffset, DolOffset, "main.dol");
                sysDirectory.AddArchiveFile(wiiStream, FSTableSize, FSTableOffset, "fst.bin");
                parDirectory.Items.Add(sysDirectory.Name, sysDirectory);

                //FileSystemTable (fst.bin)
                ArchiveDirectory filedir = new ArchiveDirectory(this, parDirectory) { Name = "files", LastWriteTimeUtc = LastWriteTimeUtc };
                wiiStream.Seek(FSTableOffset, SeekOrigin.Begin);
                var FST = new FSTBin(wiiStream, false);
                FST.ProcessEntres(wiiStream, filedir);
                parDirectory.Items.Add(filedir.Name, filedir);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Nintendo Wii AES Encrypted Disc Stream
        /// </summary>
        public class WiiStream : SubStream
        {
            private byte[] BlockBuffer = new byte[_DataSize];

            private int BufferedBlock = -1;

            private readonly Aes AES;

            public override bool CanWrite => false;

            private const int _ClustersSize = 32768;
            private const int _DataSize = 31744;

            public int BlockNumber { get; private set; }
            public int BlockOffset { get; private set; }

            public override long Position
            {
                get => position;
                set
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value));
                    if (value > Length)
                        value = Length;
                    BlockNumber = (int)(value / _DataSize);
                    BlockOffset = (int)(value % _DataSize);
                    position = value;
                }
            }

            private new long position = 0;

            public WiiStream(Stream stream, long length, long offset, byte[] key) : base(stream, length, offset, true)
            {
                AES = Aes.Create();
                AES.KeySize = key.Length * 8;
                AES.Mode = CipherMode.CBC;
                AES.Padding = PaddingMode.Zeros;
                AES.Key = key;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                /*
                 Partition data is encrypted using a key, which can be obtained from the partition header and the master key.
                 The actual partition data starts at an offset into the partition, and it is formatted in "clusters" of size 0x8000 (32768).
                 Each one of these blocks consists of 0x400(1024) bytes of encrypted SHA-1 hash data, followed by 0x7C00(31744) bytes of encrypted user data.
                 The 0x400 bytes SHA-1 data is encrypted using AES-128-CBC, with the partition key and a null (all zeroes) IV. Clusters are aggregated into subgroups of 8
                 */

                if (BufferedBlock != BlockNumber)
                {
                    BufferedBlock = BlockNumber;

                    //Get this Block IV
                    base._position = (long)BlockNumber * _ClustersSize + 976;
                    byte[] IV = new byte[16];
                    base.Read(IV, 0, IV.Length);
                    AES.IV = IV;

                    //Read Encrypted Block
                    base._position = (long)BlockNumber * _ClustersSize + 1024;
                    base.Read(BlockBuffer, 0, _DataSize);

                    //Decrypt Block
                    BlockBuffer = AES.CreateDecryptor().Decrypt(BlockBuffer);
                }

                int CopyCount = _DataSize - BlockOffset;
                CopyCount = (count > CopyCount) ? CopyCount : count;

                Array.Copy(BlockBuffer, BlockOffset, buffer, offset, CopyCount);
                Seek(CopyCount, SeekOrigin.Current);
                if (count > CopyCount)
                    return Read(buffer, CopyCount, count - CopyCount);
                else
                    return CopyCount;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }

        public class HeaderBin
        {
            public GameID GameID;
            public byte DiskID;
            public byte Version;
            public bool Streaming { get => streaming == 1; set => streaming = (byte)(value ? 1 : 0); }
            private byte streaming; // 1 on , 0 off
            public byte StreamBufSize; //0

            public uint WiiMagicWord; // GC 0 Wii 1562156707
            public uint GCMagicWord; // GC 3258163005 Wii 0
            public string GameName;

            /// <summary>
            /// 'False' don't work on retail consoles
            /// </summary>
            public bool Verification { get => verification == 0; set => verification = (byte)(value ? 0 : 1); }

            private byte verification; // 0 on , 1 off

            /// <summary>
            /// 'False' don't work on retail consoles
            /// </summary>
            public bool Encryption { get => encryption == 0; set => encryption = (byte)(value ? 0 : 1); }

            private byte encryption; // 0 on , 1 off

            public HeaderBin(Stream stream)
            {
                GameID = (GameID)stream.ReadString(6);
                DiskID = stream.ReadUInt8();
                Version = stream.ReadUInt8();
                streaming = stream.ReadUInt8();
                StreamBufSize = stream.ReadUInt8();
                _ = stream.Read(14);
                WiiMagicWord = stream.ReadUInt32(Endian.Big);
                GCMagicWord = stream.ReadUInt32(Endian.Big);
                GameName = stream.ReadString(64);
                verification = stream.ReadUInt8();
                encryption = stream.ReadUInt8();
            }
        }

        public partial class PartitionInfo : List<Partition>
        {
            public PartitionInfo(Stream stream)
            {
                var entrys = stream.For(4, S => S.Read<EntryInfo>(Endian.Big));
                foreach (var item in entrys)
                {
                    stream.Seek(item.Offset, SeekOrigin.Begin);
                    var PTIs = stream.For((int)item.Partition, S => S.Read<PartitionTableInfo>(Endian.Big));
                    foreach (var PTI in PTIs)
                    {
                        stream.Seek(PTI.Offset, SeekOrigin.Begin);
                        //Partition Header
                        Add(new Partition(stream, PTI.Type));
                    }
                }
            }

            public struct Partition
            {
                public uint TICKETsize { get => _TICKETsize; }
                public const uint _TICKETsize = 676;
                public V0Ticket Ticket;
                public TMD TMD;
                public long TICKEToffse;
                public uint TMDsize;
                public long TMDoffse { get => (tmdffse << 2) + TICKEToffse; set => tmdffse = (uint)((value - TICKEToffse) >> 2); }
                private uint tmdffse;
                public uint CERTsize;
                public long CERToffse { get => (certoffse << 2) + TICKEToffse; set => certoffse = (uint)((value - TICKEToffse) >> 2); }
                private uint certoffse;
                public uint H3size { get => _H3size; }
                public const uint _H3size = 98304;
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
                public uint Offset { get => offset << 2; set => offset = value >> 2; }
                private uint offset;
            }

            private struct PartitionTableInfo
            {
                public uint Offset { get => offset << 2; set => offset = value >> 2; }
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
