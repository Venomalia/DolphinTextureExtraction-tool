namespace AuroraLib.DiscImage.Dolphin
{
    public class BootBin : GameHeader
    {
        public uint DebugMonitorOffset; //offset of debug monitor (dh.bin)?
        public uint DebugMonitorAddress; //addr(?) to load debug monitor?

        public uint DolOffset; //offset of main executable DOL (bootfile)
        public uint FSTableOffset; //offset of the FST ("fst.bin")
        public uint FSTableSize;
        public uint FSTableSizeMax; //maximum size of FST usually same as FSTSize
        public uint UserPosition;
        public uint UserLength;
        public uint Unknown;

        public BootBin(Stream source) : base(source)
        { }

        protected override void ReadData(Stream source)
        {
            GameName = source.ReadString(992);
            DebugMonitorOffset = source.ReadUInt32(Endian.Big);
            DebugMonitorAddress = source.ReadUInt32(Endian.Big);
            source.Read(24);
            DolOffset = source.ReadUInt32(Endian.Big); //offset of main executable DOL (bootfile)
            FSTableOffset = source.ReadUInt32(Endian.Big); //offset of the FST ("fst.bin")
            FSTableSize = source.ReadUInt32(Endian.Big);
            FSTableSizeMax = source.ReadUInt32(Endian.Big); //maximum size of FST (usually same as FSTSize)*
            UserPosition = source.ReadUInt32(Endian.Big);
            UserLength = source.ReadUInt32(Endian.Big);
            Unknown = source.ReadUInt32(Endian.Big);
            source.Position += 4;
            if (DiskType == DiskTypes.Wii)
            {
                DolOffset <<= 2;
                FSTableOffset <<= 2;
            }
        }

        protected override void WriteData(Stream dest)
        {
            dest.WriteString(GameName, 992, 0);
            dest.Write(DebugMonitorOffset, Endian.Big);
            dest.Write(DebugMonitorAddress, Endian.Big);
            dest.Write(stackalloc byte[24]);
            dest.Write(DiskType == DiskTypes.GC ? DolOffset : DolOffset >> 2, Endian.Big);
            dest.Write(DiskType == DiskTypes.GC ? FSTableOffset : FSTableOffset >> 2, Endian.Big);
            dest.Write(FSTableSize, Endian.Big);
            dest.Write(FSTableSizeMax, Endian.Big);
            dest.Write(UserPosition, Endian.Big);
            dest.Write(UserLength, Endian.Big);
            dest.Write(Unknown, Endian.Big);
            dest.Write(0);
        }
    }
}
