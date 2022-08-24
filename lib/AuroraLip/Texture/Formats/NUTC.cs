using AuroraLip.Common;
using System;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    //https://github.com/marco-calautti/Rainbow/wiki/NUT-File-Format
    public class NUTC : JUTTexture, IMagicIdentify, IFileAccess
    {
        public ushort FormatVersion { get; set; }

        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "NUTC";

        public NUTC() { }

        public NUTC(Stream stream) : base(stream) { }

        public NUTC(string filepath) : base(filepath) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

            FormatVersion = stream.ReadUInt16(Endian.Big); //is 32770
            ushort texturesCount = stream.ReadUInt16(Endian.Big);
            stream.Position = 0x20;

            if (FormatVersion != 32770)
                Console.WriteLine("NUTC Version: " + FormatVersion);

            for (int i = 0; i < texturesCount; i++)
            {

                long ImageAddress = stream.Position;
                uint totalSize = stream.ReadUInt32(Endian.Big);
                uint paletteSize = stream.ReadUInt32(Endian.Big);
                uint imageSize = stream.ReadUInt32(Endian.Big);
                ushort headerSize = stream.ReadUInt16(Endian.Big);
                ushort ColorsCount = stream.ReadUInt16(Endian.Big);
                byte imageformat = (byte)stream.ReadByte();

                if (imageformat != 0)
                    throw new Exception("Unsupported image format");

                byte TotalImageCount = (byte)stream.ReadByte();
                NUTCPaletteFormat PaletteFormat = (NUTCPaletteFormat)stream.ReadByte();
                NUTCImageFormat Format = (NUTCImageFormat)stream.ReadByte();

                int ImageWidth = stream.ReadUInt16(Endian.Big);
                int ImageHeight = stream.ReadUInt16(Endian.Big);
                byte[] hardwaredata = stream.Read(24, Endian.Big);
                int userDataSize = headerSize - 0x30;

                byte[] userdata = null;
                if (userDataSize > 0) userdata = stream.Read(userDataSize, Endian.Big);
                long ImageDataAddress = stream.Position;
                stream.Position += imageSize;
                byte[] palettedata = stream.Read((int)paletteSize);
                stream.Position = ImageDataAddress;

                //When the data size is zero these images hold additional data like Palettedata
                if (imageSize != 0)
                {
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
                        MaxLOD = TotalImageCount-1
                    };
                    Add(current);
                }
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


