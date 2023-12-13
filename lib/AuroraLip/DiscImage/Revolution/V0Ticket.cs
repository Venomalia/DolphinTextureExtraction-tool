using AuroraLib.Common;
using AuroraLib.Core.Text;

namespace AuroraLib.DiscImage.Revolution
{
    public class V0Ticket : SignedBlobHeader
    {
        //v0 ticket
        public readonly byte[] Issuer;
        public readonly byte[] ECDH;
        public byte FormatVersion;
        public ushort Reserved;
        public readonly byte[] TitleKey;
        public byte Unknown;
        public readonly byte[] TicketId;
        public uint ConsoleID;
        public readonly byte[] TitleID;
        public ushort Unknown2;//mostly 0xFFFF
        public ushort TitleVersion;
        public uint TitlesMask;
        public uint PermitMask;
        public bool UsePRNG;
        public KeyID CommonKeyID;
        public readonly byte[] Unknown3;//Is all 0 for non-VC, for VC, all 0 except last byte is 1.
        public readonly byte[] ContentAccessPermissions;
        private readonly ushort Padding;
        public readonly Limit[] Limits;

        public TMD.TitleFlags TitleFlag => (TMD.TitleFlags)BitConverterX.Swap(BitConverter.ToUInt32(TitleID, 0));
        public string TitleString => EncodingX.GetValidString(TitleID.AsSpan()[..4]);

        public V0Ticket(Stream source) : base(source)
        {
            Issuer = source.Read(64);
            ECDH = source.Read(60);
            FormatVersion = source.ReadUInt8();
            Reserved = source.ReadUInt16();
            TitleKey = source.Read(16);
            Unknown = source.ReadUInt8();
            TicketId = source.Read(8);
            ConsoleID = source.ReadUInt32(Endian.Big);
            TitleID = source.Read(8);
            Unknown2 = source.ReadUInt16();
            TitleVersion = source.ReadUInt16(Endian.Big);
            TitlesMask = source.ReadUInt32(Endian.Big);
            PermitMask = source.ReadUInt32(Endian.Big);
            UsePRNG = source.ReadUInt8() == 1;
            CommonKeyID = source.Read<KeyID>();
            Unknown3 = source.Read(48);
            ContentAccessPermissions = source.Read(64);
            Padding = source.ReadUInt16();
            Limits = source.Read<Limit>(8, Endian.Big);
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
