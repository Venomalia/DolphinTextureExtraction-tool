namespace AuroraLib.DiscImage.Dolphin
{
    public abstract class GameHeader
    {
        public const uint WiiMagicWord = 1562156707;
        public const uint GCMagicWord = 3258163005;

        public GameID GameID;
        public byte DiskID;
        public byte Version;
        public bool Streaming;
        public byte StreamBufSize; // 0 = default BufSize

        public string GameName { get; set; }
        public bool IsValid;

        protected GameHeader()
        {
        }

        public GameHeader(Stream source)
        {
            GameID = source.Read<GameID>();
            DiskID = source.ReadUInt8();
            Version = source.ReadUInt8();
            Streaming = source.ReadUInt8() == 1;
            StreamBufSize = source.ReadUInt8();
            source.Position += 14;
            uint magicWordWii = source.ReadUInt32(Endian.Big); // GC 0 Wii 1562156707
            uint magicWordGC = source.ReadUInt32(Endian.Big); // GC 3258163005 Wii 0
            IsValid = Enum.IsDefined(typeof(SystemCode), GameID.SystemCode) && ((magicWordWii == WiiMagicWord && magicWordGC == 0) || (magicWordWii == 0 && magicWordGC == 0));
        }

        public void Write(Stream dest)
        {
            dest.Write(GameID);
            dest.WriteByte(DiskID);
            dest.WriteByte(Version);
            dest.WriteByte((byte)(Streaming ? 1 : 0));
            dest.WriteByte(StreamBufSize);
            dest.Write(stackalloc byte[14]);
            if (GameID.SystemCode == SystemCode.Revolution || GameID.SystemCode == SystemCode.Wii_Newer)
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

        protected abstract void WriteData(Stream dest);
    }
}
