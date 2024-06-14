using AuroraLib.Common;
using AuroraLib.Core.Text;

namespace AuroraLib.DiscImage.Revolution
{
    public class V0Ticket : SignedBlobHeader
    {
        //v0 ticket
        public readonly byte[] Issuer = new byte[64];
        public readonly byte[] ECDH = new byte[60];
        public byte FormatVersion;
        public ushort Reserved;
        public readonly byte[] TitleKey = new byte[16];
        public byte Unknown;
        public readonly byte[] TicketId = new byte[8];
        public uint ConsoleID;
        public readonly byte[] TitleID = new byte[8];
        public ushort Unknown2;//mostly 0xFFFF
        public ushort TitleVersion;
        public uint TitlesMask;
        public uint PermitMask;
        public bool UsePRNG;
        public KeyID CommonKeyID;
        public readonly byte[] Unknown3 = new byte[48];//Is all 0 for non-VC, for VC, all 0 except last byte is 1.
        public readonly byte[] ContentAccessPermissions = new byte[64];
        private readonly ushort Padding;
        public readonly Limit[] Limits = new Limit[8];

        public TMD.TitleFlags TitleFlag => (TMD.TitleFlags)BitConverterX.Swap(BitConverter.ToUInt32(TitleID, 0));
        public string TitleString => EncodingX.GetDisplayableString(TitleID.AsSpan()[..4]);

        public V0Ticket(Stream source) : base(source)
        {
            source.Read(Issuer);
            source.Read(ECDH);
            FormatVersion = source.ReadUInt8();
            Reserved = source.ReadUInt16();
            source.Read(TitleKey);
            Unknown = source.ReadUInt8();
            source.Read(TicketId);
            ConsoleID = source.ReadUInt32(Endian.Big);
            source.Read(TitleID);
            Unknown2 = source.ReadUInt16();
            TitleVersion = source.ReadUInt16(Endian.Big);
            TitlesMask = source.ReadUInt32(Endian.Big);
            PermitMask = source.ReadUInt32(Endian.Big);
            UsePRNG = source.ReadUInt8() == 1;
            CommonKeyID = source.Read<KeyID>();
            source.Read(Unknown3);
            source.Read(ContentAccessPermissions);
            Padding = source.ReadUInt16();
            source.Read<Limit>(Limits, Endian.Big);
        }

        protected override void WriteData(Stream dest)
        {
            dest.Write(Issuer);
            dest.Write(ECDH);
            dest.Write(FormatVersion);
            dest.Write(Reserved);
            dest.Write(TitleKey);
            dest.Write(Unknown);
            dest.Write(TicketId);
            dest.Write(ConsoleID, Endian.Big);
            dest.Write(TitleID);
            dest.Write(Unknown2);
            dest.Write(TitleVersion, Endian.Big);
            dest.Write(TitlesMask, Endian.Big);
            dest.Write(PermitMask, Endian.Big);
            dest.Write(Unknown2);
            dest.WriteByte((byte)(UsePRNG ? 1 : 0));
            dest.Write(CommonKeyID);
            dest.Write(Unknown3);
            dest.Write(ContentAccessPermissions);
            dest.Write(Padding);
            dest.Write<Limit>(Limits);
        }

        public struct Limit
        {
            public LimitType Type;
            public uint MaximumUsage;
        }

        public enum LimitType : uint
        {
            Unrestricted = 0,
            Minutes = 1,
            Disable = 2,
            StartCount = 3,
        }

        public enum KeyID : byte
        {
            CommonKey = 0,
            KoreanKey = 1,
            vWiiKey = 2,
        }

        public byte[] GetTitleIV()
        {
            byte[] iv = new byte[0x10];
            Array.Copy(TitleID, 0, iv, 0, 8);
            return iv;
        }

        public byte[] GetPartitionKey()
        {
            byte[] Key = CommonKeyID switch
            {
                KeyID.CommonKey => WiiKey.CKey,
                KeyID.KoreanKey => WiiKey.KKey,
                KeyID.vWiiKey => WiiKey.VKey,
                _ => WiiKey.CKey, // throw new NotSupportedException($"{nameof(V0Ticket)} Common key ID'{CommonKeyID}' not supported!"),
            };

            byte[] partitionKey = TitleKey.ToArray();
            MiscEX.AESDecrypt(partitionKey, Key, GetTitleIV());
            return partitionKey;
        }
    }
}
