using AuroraLip.Common;
using AuroraLip.Texture.J3D;
using System;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    //bas on https://forum.xentax.com/viewtopic.php?t=9256
    public class WTMD : JUTTexture, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "WTMD";

        public WTMD() { }

        public WTMD(Stream stream) : base(stream) { }

        public WTMD(string filepath) : base(filepath) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

            uint none = stream.ReadUInt16(Endian.Big);
            uint PalettePosition = stream.ReadUInt16(Endian.Big);
            int ImageWidth = stream.ReadUInt16(Endian.Big);
            int ImageHeight = stream.ReadUInt16(Endian.Big);
            GXImageFormat Format = (GXImageFormat)stream.ReadByte();
            byte unknown = (byte)stream.ReadByte(); //2 0 3
            byte unknown1 = (byte)stream.ReadByte(); //2 0 3
            byte unknown2 = (byte)stream.ReadByte(); //1 2
            uint unknown3 = stream.ReadUInt16(Endian.Big);
            uint ImagePosition = stream.ReadUInt16(Endian.Big);
            uint unknown4 = stream.ReadUInt16(Endian.Big); //2 1 0
            uint unknown5 = stream.ReadUInt16(Endian.Big);
            uint unknown6 = stream.ReadUInt16(Endian.Big);
            uint unknown8 = stream.ReadUInt16(Endian.Big);
            uint unknown9 = stream.ReadUInt16(Endian.Big);
            uint unknown10 = stream.ReadUInt16(Endian.Big);

            byte[] PaletteData = null;
            int PaletteCount = 0;
            GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8;
            if (JUtility.IsPaletteFormat(Format))
            {
                stream.Position = PalettePosition;
                int PaletteSize = (int)ImagePosition - (int)PalettePosition;
                PaletteCount = PaletteSize / 2;
                PaletteData = stream.Read(PaletteSize);

            }
            stream.Position = ImagePosition;
            TexEntry current = new TexEntry(stream, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight, 0)
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
            while (stream.Position != stream.Length)
            {
                int i = (int)Math.Pow(2,current.Count);
                if (ImageWidth / i < 1 || ImageHeight / i < 1) break;
                 current.Add(DecodeImage(stream, PaletteData, Format, GXPaletteFormat.IA8, PaletteCount, ImageWidth/ i, ImageHeight / i));
            }
            Add(current);

        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
