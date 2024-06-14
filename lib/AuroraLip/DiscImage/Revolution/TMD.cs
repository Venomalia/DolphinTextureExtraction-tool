using AuroraLib.Core.Text;

namespace AuroraLib.DiscImage.Revolution
{
    /// <summary>
    /// Wii Title Metadata
    /// </summary>
    public class TMD : SignedBlobHeader
    {
        public readonly byte[] Issuer = new byte[64];
        public byte FormatVersion;
        public byte CA_CRL_Version;
        public byte Signer_CRL_Version;
        public bool IsVWii;
        public ulong IOS;
        public readonly byte[] TitleID = new byte[8];
        public TitleTypes TitleType;
        public ushort GroupID;
        private readonly ushort pad1;
        public RegionTyps Region;
        public Ratings Ratings;
        public readonly byte[] IPCMask = new byte[30];
        public uint AccessRights;
        public ushort TitleVersion;
        public ushort bootIndex;
        public ushort MinorVersion;
        public List<CMD> CMDs;

        public TitleFlags TitleFlag => (TitleFlags)BitConverterX.Swap(BitConverter.ToUInt32(TitleID, 0));
        public string TitleString => EncodingX.GetDisplayableString(TitleID.AsSpan()[..4]);

        public TMD(Stream source) : base(source)
        {
            source.Read(Issuer);
            FormatVersion = source.ReadUInt8();
            CA_CRL_Version = source.ReadUInt8();
            Signer_CRL_Version = source.ReadUInt8();
            IsVWii = source.ReadUInt8() != 0;
            IOS = source.ReadUInt64(Endian.Big);
            source.Read(TitleID);
            TitleType = source.Read<TitleTypes>(Endian.Big);
            GroupID = source.ReadUInt16(Endian.Big);
            pad1 = source.ReadUInt16();
            Region = source.Read<RegionTyps>(Endian.Big);
            Ratings = source.Read<Ratings>();
            source.Position += 12;
            source.Read(IPCMask);
            AccessRights = source.ReadUInt32(Endian.Big);
            TitleVersion = source.ReadUInt16(Endian.Big);
            ushort content = source.ReadUInt16(Endian.Big);
            bootIndex = source.ReadUInt16(Endian.Big);
            MinorVersion = source.ReadUInt16(Endian.Big);

            CMDs = new List<CMD>(content);
            for (int i = 0; i < content; i++)
            {
                CMDs.Add(new CMD(source));
            }
        }

        protected override void WriteData(Stream dest)
        {
            dest.Write(Issuer);
            dest.WriteByte(FormatVersion);
            dest.WriteByte(FormatVersion);
            dest.WriteByte(Signer_CRL_Version);
            dest.WriteByte((byte)(IsVWii ? 1 : 0));
            dest.Write(IOS, Endian.Big);
            dest.Write(TitleID);
            dest.Write(TitleType, Endian.Big);
            dest.Write(GroupID, Endian.Big);
            dest.Write(pad1);
            dest.Write(Region, Endian.Big);
            dest.Write(Ratings);
            dest.Write(stackalloc byte[12]);
            dest.Write(IPCMask);
            dest.Write(AccessRights, Endian.Big);
            dest.Write(TitleVersion, Endian.Big);
            dest.Write((ushort)CMDs.Count, Endian.Big);
            dest.Write(bootIndex, Endian.Big);
            dest.Write(MinorVersion, Endian.Big);
            foreach (CMD item in CMDs)
            {
                item.Write(dest);
            }
        }
        public enum RegionTyps : short
        {
            Japan = 0,
            USA = 1,
            Europe = 2,
            Free = 3,
            Korea = 4,
        }

        public enum TitleTypes : uint
        {
            // All official titles have this flag set.
            Default = 0x1,

            Unknown_0x4 = 0x4,

            // Used for DLC titles.
            Data = 0x8,

            Unknown_0x10 = 0x10,

            // Seems to be used for WFS titles.
            Maybe_WFS = 0x20,

            Unknown_CT = 0x40,
        }

        public enum TitleFlags : uint
        {
            SystemTitles = 0x00000001,
            Game = 0x00010000,
            Channel = 0x00010001,
            SystemChannels = 0x00010002,
            GameChannel = 0x00010004,
            DLC = 0x00010005,
            HiddenChannels = 0x00010008,
        }
    }
}
