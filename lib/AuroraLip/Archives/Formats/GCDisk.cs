using AuroraLip.Archives.DiscImage;
using AuroraLip.Common;
using System;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    /// <summary>
    /// Nintendo Gamecube Mini Disc Image.
    /// </summary>
    public class GCDisk : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        private const uint MagicWord = 3258163005;

        public BootBin Boot;

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (extension.ToLower().Equals(".iso") && stream.Length > 9280)
            {
                GameID gameID = (GameID)stream.Read(6);
                return Enum.IsDefined(typeof(SystemCode), gameID.SystemCode) && Enum.IsDefined(typeof(RegionCode), gameID.RegionCode) && stream.At(28, s => s.ReadUInt32(Endian.Big)) == MagicWord;
            }
            return false;
        }

        protected override void Read(Stream stream)
        {
            Boot = (BootBin)stream.Read(1088);

            // get appLoaderDate
            stream.Seek(9280, SeekOrigin.Begin);
            ReadOnlySpan<char> appLoaderDate = stream.ReadString(10).AsSpan();
            LastWriteTimeUtc = new DateTime(int.Parse(appLoaderDate.Slice(0, 4).ToString()), int.Parse(appLoaderDate.Slice(5, 2).ToString()), int.Parse(appLoaderDate.Slice(8, 2).ToString()));

            Root = new ArchiveDirectory() { Name = $"{Boot.GameName} [{Boot.GameID}]", OwnerArchive = this, LastWriteTimeUtc = LastWriteTimeUtc };
            ArchiveDirectory SysDirectory = new ArchiveDirectory(this, Root) { Name = "sys", LastWriteTimeUtc = LastWriteTimeUtc };
            Root.Items.Add(SysDirectory.Name, SysDirectory);
            SysDirectory.AddArchiveFile(stream, 1088, 0, "boot.bin");
            SysDirectory.AddArchiveFile(stream, 8192, 1088, "bi2.bin");
            SysDirectory.AddArchiveFile(stream, Boot.DolOffset - 9280, 9280, "apploader.img");
            SysDirectory.AddArchiveFile(stream, Boot.FSTableOffset - Boot.DolOffset, Boot.DolOffset, "main.dol");
            SysDirectory.AddArchiveFile(stream, Boot.FSTableSize, Boot.FSTableOffset, "fst.bin");

            //FileSystemTable (fst.bin)

            ArchiveDirectory filedir = new ArchiveDirectory(this, Root) { Name = "files", LastWriteTimeUtc = LastWriteTimeUtc };
            Root.Items.Add(filedir.Name, filedir);
            stream.Seek(Boot.FSTableOffset, SeekOrigin.Begin);
            var FST = new FSTBin(stream);
            FST.ProcessEntres(stream, filedir);
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        public class Bi2Bin
        {
            public byte[] Data
            {
                get
                {
                    MemoryStream stream = new MemoryStream();
                    stream.Write(DebugMonitorSize, Endian.Big);
                    stream.Write(SimulatedMemorySize, Endian.Big);
                    stream.Write(ArgumentOffset, Endian.Big);
                    stream.Write(DebugFlag, Endian.Big);
                    stream.Write(TrackLocation, Endian.Big);
                    stream.Write(TrackSize, Endian.Big);
                    stream.Write(CountryCode, Endian.Big);
                    stream.Write(Unknown_1, Endian.Big);
                    stream.Write(Unknown_2, Endian.Big);
                    stream.AddPadding(7068);
                    return stream.ToArray();
                }
            }

            public uint DebugMonitorSize; //0
            public uint SimulatedMemorySize; // 25165824
            public uint ArgumentOffset; //0
            public uint DebugFlag; //0
            public uint TrackLocation; //0
            public uint TrackSize; //0
            public uint CountryCode; //2
            public uint Unknown_1; //1
            public uint Unknown_2; //1

            public Bi2Bin(byte[] Value)
            {
                if (Value.Length != 8192)
                    throw new ArgumentException($"A {nameof(Bi2Bin)} must consist of 8192 bytes");
                MemoryStream stream = new MemoryStream(Value);

                DebugMonitorSize = stream.ReadUInt32(Endian.Big);
                SimulatedMemorySize = stream.ReadUInt32(Endian.Big);
                ArgumentOffset = stream.ReadUInt32(Endian.Big);
                DebugFlag = stream.ReadUInt32(Endian.Big);
                TrackLocation = stream.ReadUInt32(Endian.Big);
                TrackSize = stream.ReadUInt32(Endian.Big);
                CountryCode = stream.ReadUInt32(Endian.Big);
                Unknown_1 = stream.ReadUInt32(Endian.Big);
                Unknown_2 = stream.ReadUInt32(Endian.Big);
            }

            public static explicit operator Bi2Bin(byte[] x) => new Bi2Bin(x);
            public static explicit operator byte[](Bi2Bin x) => x.Data;
        }

        public class BootBin
        {
            public byte[] Data
            {
                get
                {
                    MemoryStream stream = new MemoryStream();
                    stream.WriteString((string)GameID);
                    stream.WriteByte(DiskID);
                    stream.WriteByte(Version);
                    stream.WriteByte(streaming);
                    stream.WriteByte(StreamBufSize);
                    stream.AddPadding(14);
                    stream.Write(WiiMagicWord, Endian.Big);
                    stream.Write(GCMagicWord, Endian.Big);
                    stream.WriteString(GameName);
                    stream.AddPadding(0x03e0 - GameName.Length);
                    stream.Write(debugMonitorOffset, Endian.Big);
                    stream.Write(debugMonitorAddress, Endian.Big);
                    stream.AddPadding(24);
                    stream.Write(DolOffset, Endian.Big);
                    stream.Write(FSTableOffset, Endian.Big);
                    stream.Write(FSTableSize, Endian.Big);
                    stream.Write(FSTableSizeMax, Endian.Big);
                    stream.Write(userPosition, Endian.Big);
                    stream.Write(userLength, Endian.Big);
                    stream.Write(unknown, Endian.Big);
                    stream.AddPadding(4);
                    return stream.ToArray();
                }
            }
            public GameID GameID;
            public byte DiskID;
            public byte Version;
            public bool Streaming { get => streaming == 1; set => streaming = (byte)(value ? 1 : 0); }
            private byte streaming; // 1 on , 0 off
            public byte StreamBufSize; //0 = default BufSize

            public uint WiiMagicWord; // GC 0 Wii 1562156707
            public uint GCMagicWord; // GC 3258163005 Wii 0
            public string GameName;
            public uint debugMonitorOffset; //offset of debug monitor (dh.bin)?
            public uint debugMonitorAddress; //addr(?) to load debug monitor?

            public uint DolOffset; //offset of main executable DOL (bootfile)
            public uint FSTableOffset; //offset of the FST ("fst.bin")
            public uint FSTableSize;
            public uint FSTableSizeMax; //maximum size of FST usually same as FSTSize
            public uint userPosition;
            public uint userLength;
            public uint unknown;


            public BootBin(in byte[] Value)
            {
                if (Value.Length != 1088)
                    throw new ArgumentException($"A {nameof(BootBin)} must consist of 1088 bytes");
                MemoryStream stream = new MemoryStream(Value);
                GameID = (GameID)stream.ReadString(6);
                DiskID = stream.ReadUInt8();
                Version = stream.ReadUInt8();
                streaming = stream.ReadUInt8();
                StreamBufSize = stream.ReadUInt8();
                stream.Read(14);
                WiiMagicWord = stream.ReadUInt32(Endian.Big);
                GCMagicWord = stream.ReadUInt32(Endian.Big);
                GameName = stream.ReadString(992);
                debugMonitorOffset = stream.ReadUInt32(Endian.Big);
                debugMonitorAddress = stream.ReadUInt32(Endian.Big);
                stream.Read(24);
                DolOffset = stream.ReadUInt32(Endian.Big); //offset of main executable DOL (bootfile)
                FSTableOffset = stream.ReadUInt32(Endian.Big); //offset of the FST ("fst.bin")
                FSTableSize = stream.ReadUInt32(Endian.Big);
                FSTableSizeMax = stream.ReadUInt32(Endian.Big); //maximum size of FST (usually same as FSTSize)*
                userPosition = stream.ReadUInt32(Endian.Big);
                userLength = stream.ReadUInt32(Endian.Big);
                unknown = stream.ReadUInt32(Endian.Big);
            }

            public static explicit operator BootBin(byte[] x) => new BootBin(x);
            public static explicit operator byte[](BootBin x) => x.Data;
        }

        public class FSTBin
        {
            public FSTEntry[] Entires;
            public long stringTableOffset;

            public FSTBin(Stream stream)
            {
                var root = stream.Read<FSTEntry>(Endian.Big);
                Entires = stream.For((int)root.Data - 1, S => S.Read<FSTEntry>(Endian.Big));
                stringTableOffset = stream.Position;
            }

            public void ProcessEntres(Stream stream, ArchiveDirectory directory)
                => ProcessEntres(stream, directory, 0, Entires.Length);

            private int ProcessEntres(Stream stream, ArchiveDirectory directory, int i, int l)
            {
                while (i < l)
                {
                    stream.Seek(stringTableOffset + (int)Entires[i].NameOffset, SeekOrigin.Begin);
                    string name = stream.ReadString();
                    if (Entires[i].IsDirectory)
                    {
                        ArchiveDirectory subdir = new ArchiveDirectory(directory.OwnerArchive, directory) { Name = name };
                        directory.Items.Add(name, subdir);
                        i = ProcessEntres(stream, subdir, i + 1, (int)Entires[i].Data - 1);
                    }
                    else
                    {
                        directory.AddArchiveFile(stream, Entires[i].Data, Entires[i].Offset, name);
                        i++;
                    }
                }
                return l;
            }

            public struct FSTEntry
            {
                public byte Flag { get; set; }
                public UInt24 NameOffset { get; set; }
                public uint Offset { get; set; } // file or parent Offset
                public uint Data { get; set; } // fileSize or numberOfFiles Offset

                public bool IsDirectory
                {
                    get => Flag != 0;
                    set => Flag = (byte)(value ? 1 : 0);
                }
            }
        }
    }

}
