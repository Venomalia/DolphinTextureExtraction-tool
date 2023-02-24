using AuroraLip.Common;
using AuroraLip.Texture.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuroraLip.Archives.DiscImage.V0Ticket;

namespace AuroraLip.Archives.DiscImage
{
    public struct TMD : ISignedBlobHeader
    {
        //Signed blob header
        public SigTyp SignatureType { get; } //Signature type (always 65537 for RSA-2048)
        public byte[] Certificate { get; }
        public byte[] SigPad { get; }
        //TMD Main header
        public byte[] Issuer;
        public byte FormatVersion;
        public byte CA_CRL_Version;
        public byte Signer_CRL_Version;
        public bool IsVWii{ get => vWii != 0; set => vWii = (byte)(value ? 1 : 0); }
        private byte vWii;
        public ulong IOS;
        public byte[] TitleID;
        public TitleTypes TitleType;
        public ushort GroupID;
        private ushort pad1;
        public RegionTyps Region;
        public byte[] Ratings;
        public byte[] IPCMask;
        public uint AccessRights;
        public ushort TitleVersion;
        public ushort Content;
        public ushort bootIndex;
        public ushort MinorVersion;
        public List<CMD> CMDs;

        public TitleFlags TitleFlag => (TitleFlags)BitConverter.ToUInt32(TitleID, 0).Swap();
        public string TitleString => TitleID.ToValidString(4,4);

        public TMD(Stream stream)
        {
            SignatureType = (SigTyp)stream.ReadUInt32(Endian.Big);
            Certificate = stream.Read(256);
            SigPad = stream.Read(60);
            Issuer = stream.Read(64);
            FormatVersion = stream.ReadUInt8();
            CA_CRL_Version = stream.ReadUInt8();
            Signer_CRL_Version = stream.ReadUInt8();
            vWii = stream.ReadUInt8();
            IOS = stream.ReadUInt64(Endian.Big);
            TitleID = stream.Read(8);
            TitleType = (TitleTypes)stream.ReadUInt32(Endian.Big);
            GroupID = stream.ReadUInt16(Endian.Big);
            pad1 = stream.ReadUInt16();
            Region = (RegionTyps)stream.ReadUInt16(Endian.Big);
            Ratings = stream.Read(28);
            IPCMask = stream.Read(30);
            AccessRights = stream.ReadUInt32(Endian.Big);
            TitleVersion = stream.ReadUInt16(Endian.Big);
            Content = stream.ReadUInt16(Endian.Big);
            bootIndex = stream.ReadUInt16(Endian.Big);
            MinorVersion = stream.ReadUInt16(Endian.Big);

            CMDs = new List<CMD>();
            for (int i = 0; i < Content; i++)
            {
                CMDs.Add(new CMD(stream));
            }

        }

        public enum RegionTyps: short
        {
            Japan = 0,
            USA = 1,
            Europe = 2,
            Free = 3,
            Korea = 4,
        }

        public enum TitleTypes
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

        public struct CMD
        {
            public uint ContentId;
            public ushort Index;
            public ContentType Type;
            public ulong Size;
            public byte[] Hash;

            public CMD(Stream stream)
            {
                ContentId = stream.ReadUInt32(Endian.Big);
                Index = stream.ReadUInt16(Endian.Big);
                Type = (ContentType)stream.ReadUInt16(Endian.Big);
                Size = stream.ReadUInt64(Endian.Big);
                Hash = stream.Read(20);
            }

            public enum ContentType : ushort
            {
                Normal = 0x0001,
                DLC = 0x4001,
                Shared = 0x8001,
            }

            public byte[] GetContentIV()
            {
                byte[] iv_bits = BitConverter.GetBytes(Index);
                byte[] iv = new byte[16];
                iv[0] = iv_bits[1];
                iv[1] = iv_bits[0];
                return iv;
            }
        }
    }
}
