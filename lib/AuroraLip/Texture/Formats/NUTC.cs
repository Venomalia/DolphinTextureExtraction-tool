using AuroraLip.Common;
using System;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    //https://github.com/marco-calautti/Rainbow/wiki/NUT-File-Format
    public class NUTC : JUTTexture, IMagicIdentify
    {
        public ushort FormatVersion { get; set; }

        public string Magic => magic;

        private const string magic = "NUTC";

        public NUTC() { }

        public NUTC(Stream stream) : base(stream) { }

        public NUTC(string filepath) : base(filepath) { }

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

            FormatVersion = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0); //is 32770
            ushort texturesCount = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            stream.Position = 0x20;

            for (int i = 0; i < texturesCount; i++)
            {

                long ImageAddress = stream.Position;
                uint totalSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                uint paletteSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                uint imageSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                ushort headerSize = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                ushort ColorsCount = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                byte imageformat = (byte)stream.ReadByte();

                if (imageformat != 0)
                    throw new Exception("Unsupported image format");

                byte TotalImageCount = (byte)stream.ReadByte();
                NUTCPaletteFormat PaletteFormat = (NUTCPaletteFormat)stream.ReadByte();
                NUTCImageFormat Format = (NUTCImageFormat)stream.ReadByte();

                int ImageWidth = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                int ImageHeight = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                byte[] hardwaredata = stream.ReadBigEndian(24);

                int userDataSize = headerSize - 0x30;

                byte[] userdata = null;
                if (userDataSize > 0) userdata = stream.ReadBigEndian(userDataSize);
                long ImageDataAddress = stream.Position;
                stream.Position += imageSize;
                byte[] palettedata = stream.Read((int)paletteSize);
                stream.Position = ImageDataAddress;

                GXPaletteFormat GXPaletteFormat = GXPaletteFormat.IA8;
                if (PaletteFormat != NUTCPaletteFormat.None)
                {
                    if ((int)PaletteFormat > 3) PaletteFormat = NUTCPaletteFormat.IA8;
                    GXPaletteFormat = (GXPaletteFormat)Enum.Parse(typeof(GXPaletteFormat), PaletteFormat.ToString());
                }
                TexEntry current = new TexEntry(stream, palettedata, (GXImageFormat)Enum.Parse(typeof(GXImageFormat), Format.ToString()), GXPaletteFormat, ColorsCount, ImageWidth, ImageHeight, TotalImageCount - 1)
                {
                    LODBias = 0,
                    MagnificationFilter = GXFilterMode.Nearest,
                    MinificationFilter = GXFilterMode.Nearest,
                    WrapS = GXWrapMode.CLAMP,
                    WrapT = GXWrapMode.CLAMP,
                    EnableEdgeLOD = false,
                    MinLOD = 0,
                    MaxLOD = 0
                };
                Add(current);
                stream.Position = ImageAddress + totalSize;
            }
        }

        public enum NUTCImageFormat : byte
        {
            RGBA32 = 3,
            CMPR,
            C4,
            C8,
            I8 = 10,
            IA8 = 11,
        }

        public enum NUTCPaletteFormat : byte
        {
            None,
            RGB565,
            RGB5A3,
            IA8,
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}


