using AuroraLib.Common;

namespace AuroraLib.Archives.DiscImage
{
    public struct V0Ticket : ISignedBlobHeader
    {
        //Signed blob header
        public SigTyp SignatureType { get; } //Signature type (always 65537 for RSA-2048)

        public byte[] Certificate { get; }
        public byte[] SigPad { get; }

        //v0 ticket
        public byte[] Issuer;

        public byte[] ECDH;
        public byte FormatVersion;
        public ushort Reserved;
        public byte[] TitleKey;
        public byte Unknown;
        public byte[] TicketId;
        public uint ConsoleID;
        public byte[] TitleID;
        public ushort Unknown2;//mostly 0xFFFF
        public ushort TitleVersion;
        public uint TitlesMask;
        public uint PermitMask;
        public bool UsePRNG { get => use_PRNG == 1; set => use_PRNG = (byte)(value ? 1 : 0); }
        private byte use_PRNG;
        public KeyID CommonKeyID;
        public byte[] Unknown3;//Is all 0 for non-VC, for VC, all 0 except last byte is 1.
        public byte[] ContentAccessPermissions;
        public ushort Padding;
        public uint LimitType;
        public uint MaximumUsage;
        public byte[] ccLimitStructs;

        public TMD.TitleFlags TitleFlag => (TMD.TitleFlags)BitConverter.ToUInt32(TitleID, 0).Swap();
        public string TitleString => TitleID.ToValidString(4, 4);

        public V0Ticket(Stream stream)
        {
            SignatureType = (SigTyp)stream.ReadUInt32(Endian.Big);
            Certificate = stream.Read(256);
            SigPad = stream.Read(60);
            Issuer = stream.Read(64);
            ECDH = stream.Read(60);
            FormatVersion = stream.ReadUInt8();
            Reserved = stream.ReadUInt16();
            TitleKey = stream.Read(16);
            Unknown = stream.ReadUInt8();
            TicketId = stream.Read(8);
            ConsoleID = stream.ReadUInt32(Endian.Big);
            TitleID = stream.Read(8);
            Unknown2 = stream.ReadUInt16();
            TitleVersion = stream.ReadUInt16(Endian.Big);
            TitlesMask = stream.ReadUInt32(Endian.Big);
            PermitMask = stream.ReadUInt32(Endian.Big);
            use_PRNG = stream.ReadUInt8();
            CommonKeyID = (KeyID)stream.ReadUInt8();
            Unknown3 = stream.Read(48);
            ContentAccessPermissions = stream.Read(64);
            Padding = stream.ReadUInt16();
            LimitType = stream.ReadUInt32(Endian.Big);
            MaximumUsage = stream.ReadUInt32(Endian.Big);
            ccLimitStructs = stream.Read(56);
        }

        public enum KeyID : byte
        {
            CommonKey = 0,
            KoreanKey = 1,
            vWiiKey = 2,
        }

        public byte[] GetTitleIV()
        {
            var iv = new byte[0x10];
            Array.Copy(TitleID, 0, iv, 0, 8);
            return iv;
        }

        public byte[] GetPartitionKey()
        {
            byte[] Key = null;
            switch (CommonKeyID)
            {
                case KeyID.CommonKey:
                    Key = WiiKey.CKey;
                    break;

                case KeyID.KoreanKey:
                    Key = WiiKey.KKey;
                    break;

                case KeyID.vWiiKey:
                    Key = WiiKey.VKey;
                    break;

                default:
                    Events.NotificationEvent.Invoke(NotificationType.Warning, $"{nameof(V0Ticket)} Common Key ID'{CommonKeyID}' does not exist!");
                    Key = WiiKey.CKey;
                    break;
            }
            return MiscEX.AESDecrypt(TitleKey, Key, GetTitleIV());
        }
    }
}
