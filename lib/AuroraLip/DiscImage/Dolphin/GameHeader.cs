using AuroraLib.Core.Interfaces;

namespace AuroraLib.DiscImage.Dolphin
{
    public abstract class GameHeader : IBinaryObject
    {
        public const uint WiiMagicWord = 1562156707;
        public const uint GCMagicWord = 3258163005;

        public GameID GameID;
        public byte DiskID;
        public byte Version;
        public bool Streaming;
        public byte StreamBufSize; // 0 = default BufSize

        public string GameName { get; set; }
        public DiskTypes DiskType;

        protected GameHeader()
        { }

        public GameHeader(Stream source)
            => BinaryDeserialize(source);

        public virtual void BinaryDeserialize(Stream source)
        {
            GameID = source.Read<GameID>();
            DiskID = source.ReadUInt8();
            Version = source.ReadUInt8();
            Streaming = source.ReadUInt8() == 1;
            StreamBufSize = source.ReadUInt8();
            source.Position += 14;
            uint magicWordWii = source.ReadUInt32(Endian.Big); // GC 0 Wii 1562156707
            uint magicWordGC = source.ReadUInt32(Endian.Big); // GC 3258163005 Wii 0
            if (magicWordWii == 0 && magicWordGC == GCMagicWord) DiskType = DiskTypes.GC;
            else if (magicWordWii == WiiMagicWord && magicWordGC == 0) DiskType = DiskTypes.Wii;
            else DiskType = DiskTypes.Invalid;
            ReadData(source);
        }

        public virtual void BinarySerialize(Stream dest)
        {
            dest.Write(GameID);
            dest.WriteByte(DiskID);
            dest.WriteByte(Version);
            dest.WriteByte((byte)(Streaming ? 1 : 0));
            dest.WriteByte(StreamBufSize);
            dest.Write(stackalloc byte[14]);
            if (DiskType == DiskTypes.Wii)
            {
                dest.Write(WiiMagicWord, Endian.Big);
                dest.Write(0);
            }
            else
            {
                dest.Write(0);
                dest.Write(GCMagicWord, Endian.Big);
            }
            WriteData(dest);
        }

        protected abstract void ReadData(Stream source);
        protected abstract void WriteData(Stream dest);
    }

    public enum DiskTypes
    {
        Invalid = 0,
        GC = 1,
        Wii = 2,
    }
}
