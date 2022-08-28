using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    // base on https://github.com/KillzXGaming/Switch-Toolbox/blob/12dfbaadafb1ebcd2e07d239361039a8d05df3f7/File_Format_Library/FileFormats/NLG/MarioStrikers/StrikersRLT.cs
    public class PTLG : JUTTexture, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "PTLG";

        public PTLG() { }

        public PTLG(Stream stream) : base(stream) { }

        public PTLG(string filepath) : base(filepath) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);

            uint numTextures = stream.ReadUInt32(Endian.Big);
            uint unk = stream.ReadUInt32(Endian.Big);
            uint padding = stream.ReadUInt32(Endian.Big);

            uint Off = stream.ReadUInt32(Endian.Big);
            if (Off == 0)
                stream.Seek(12, SeekOrigin.Current); //GC
            else
                stream.Seek(-4, SeekOrigin.Current); //wii

            List<PTLGEntry> Entries = new List<PTLGEntry>();
            for (int i = 0; i < numTextures; i++)
            {
                Entries.Add(new PTLGEntry(stream));
            }

            long startPos = stream.Position; //1008

            for (int i = 0; i < numTextures; i++)
            {
                stream.Seek(startPos + Entries[i].ImageOffset, SeekOrigin.Begin);

                long pos = stream.Position;

                uint Images = stream.ReadUInt32(Endian.Big);
                uint unknown2 = stream.ReadUInt32(Endian.Big); //1 3 2 8 ||Unknown ==12  2439336753|1267784150|3988437873
                byte unknown4 = (byte)stream.ReadByte(); //5 0 8
                PTLGImageFormat PTLGFormat = (PTLGImageFormat)stream.ReadByte();
                GXImageFormat Format = (GXImageFormat)Enum.Parse(typeof(GXImageFormat), PTLGFormat.ToString());
                byte unknown5 = (byte)stream.ReadByte(); //5 0 8
                byte unknown6 = (byte)stream.ReadByte(); //3 4 0 2 6 8
                ushort ImageWidth = stream.ReadUInt16(Endian.Big);//GC

                if (ImageWidth == 0)
                    ImageWidth = stream.ReadUInt16(Endian.Big); //wii
                ushort ImageHeight = stream.ReadUInt16(Endian.Big);

                ushort unknown7 = stream.ReadUInt16(Endian.Big);//0
                uint unknown8 = stream.ReadUInt32(Endian.Big); //256
                uint unknown9 = stream.ReadUInt32(Endian.Big);//is 65535 Unknown ==12
                uint unknown10 = stream.ReadUInt32(Endian.Big);//0

                //No a texture?
                if (Entries[i].Unknown != 0) continue;

                TexEntry current = new TexEntry(stream, null, Format, GXPaletteFormat.IA8, 0, ImageWidth, ImageHeight, (int)Images - 1)
                {
                    LODBias = 0,
                    MagnificationFilter = GXFilterMode.Nearest,
                    MinificationFilter = GXFilterMode.Nearest,
                    WrapS = GXWrapMode.CLAMP,
                    WrapT = GXWrapMode.CLAMP,
                    EnableEdgeLOD = false,
                    MinLOD = 0,
                    MaxLOD = Images-1
                };
                Add(current);

                stream.Seek(pos + Entries[i].SectionSize, SeekOrigin.Begin);


            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        public enum PTLGImageFormat : byte
        {
            I4 = 0x02,
            I8 = 0x03,
            IA4 = 0x04,
            RGB5A3 = 0x05,
            CMPR = 0x06,
            RGB565 = 0x07,
            RGBA32 = 0x08
        }

        public class PTLGEntry
        {
            public uint Hash;
            public uint ImageOffset;
            public uint SectionSize;
            public uint Unknown; //0 or 12

            public PTLGEntry(Stream stream)
            {
                Hash = stream.ReadUInt32(Endian.Big);
                ImageOffset = stream.ReadUInt32(Endian.Big);
                SectionSize = stream.ReadUInt32(Endian.Big);
                Unknown = stream.ReadUInt32(Endian.Big);
            }
        }
    }
}
