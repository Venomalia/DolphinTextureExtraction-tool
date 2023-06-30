using AuroraLib.Common;
using AuroraLib.Common.Struct;

namespace AuroraLib.Texture.Formats
{
    // base on https://github.com/KillzXGaming/Switch-Toolbox/blob/12dfbaadafb1ebcd2e07d239361039a8d05df3f7/File_Format_Library/FileFormats/NLG/MarioStrikers/StrikersRLT.cs
    public class PTLG : JUTTexture, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("PTLG");

        public PTLG()
        { }

        public PTLG(Stream stream) : base(stream)
        {
        }

        public PTLG(string filepath) : base(filepath)
        {
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier) || (extension == string.Empty && stream.At(0x10, s => s.Match(_identifier)));

        protected override void Read(Stream stream)
        {
            if (!stream.Match(_identifier))
            {
                stream.Seek(0x10, SeekOrigin.Begin);
                stream.MatchThrow(_identifier);
            }

            uint numTextures = stream.ReadUInt32(Endian.Big);
            uint hash = stream.ReadUInt32(Endian.Big); // GC 0 Wii != 0
            uint padding = stream.ReadUInt32(Endian.Big);

            uint Off = stream.ReadUInt32(Endian.Big);

            PTLGType type = Off == 0 ? PTLGType.GC : hash == 0 ? PTLGType.GCCompact : PTLGType.Wii;
            if (Off == 0)
                stream.Seek(12, SeekOrigin.Current);
            else
                stream.Seek(-4, SeekOrigin.Current);

            Entry[] Entrys = stream.For((int)numTextures, s => s.Read<Entry>(Endian.Big));
            long startPos = stream.Position;

            for (int i = 0; i < numTextures; i++)
            {
                stream.Seek(startPos + Entrys[i].ImageOffset, SeekOrigin.Begin);
                if (ReadTexture(stream, Entrys[i].SectionSize, out TexEntry current, Entrys[i].Flag))
                {
                    Add(current);
                }
            }
        }

        public static bool ReadTexture(Stream stream, uint size, out TexEntry texture, uint flag = 0)
        {
            long endPos = stream.Position + size;

            uint Images = stream.ReadUInt32(Endian.Big);

            if (Images > 0x10)
            {
                //1313621792 "NLG " Font Description file
                //1600939625 "_lfi" maybe a pallete or other game data?
                //2142000 TPL

                if (Images == 2142000)
                {
                    List<TexEntry> entries = new();
                    TPL.ProcessStream(stream, stream.Position - 4, entries);

                    texture = entries.First();
                    return true;
                }

                texture = null;
                return false;
            }

            uint Format0 = stream.ReadUInt32(Endian.Big); //RGB5A3 1 CMPR 2 RGBA32 3 C8 8
            byte Format1 = (byte)stream.ReadByte(); //5 RGBA32 8
            PTLGImageFormat PTLGFormat = (PTLGImageFormat)stream.ReadByte();
            byte Format3 = (byte)stream.ReadByte(); //5 RGBA32 8
            byte Format4 = (byte)stream.ReadByte(); //CMPR 0 RGB5A3 3 RGBA32 8 C8 0 || 1 || 4 

            GXImageFormat Format = (GXImageFormat)Enum.Parse(typeof(GXImageFormat), PTLGFormat.ToString());
            ReadOnlySpan<byte> Palette = ReadOnlySpan<byte>.Empty;
            ushort ImageWidth, ImageHeight;
            uint Collors = 0;

            ImageWidth = stream.ReadUInt16(Endian.Big);
            if (ImageWidth == 0)
            {
                ImageWidth = stream.ReadUInt16(Endian.Big);
                ImageHeight = stream.ReadUInt16(Endian.Big);

                ushort pad1 = stream.ReadUInt16(Endian.Big);
                Collors = stream.ReadUInt32(Endian.Big);
            }
            else
            {
                ImageHeight = stream.ReadUInt16(Endian.Big);
            }

            if (Collors != 0)
            {
                Format = GXImageFormat.C8;
                Palette = stream.At(endPos - Collors * 2, SeekOrigin.Begin, s => s.Read((int)Collors * 2));
            }

            //The image files are aligned from end.
            int imageSize = Format.GetCalculatedTotalDataSize(ImageWidth, ImageHeight, (int)Images - 1);
            stream.Seek(endPos - Collors * 2 - imageSize, SeekOrigin.Begin);

            texture = new(stream, Palette, Format, GXPaletteFormat.RGB5A3, (int)Collors, ImageWidth, ImageHeight, (int)Images - 1)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = Images - 1
            };

            return true;
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
            RGB5A3 = 0x05, // or C8
            CMPR = 0x06,
            RGB565 = 0x07,
            RGBA32 = 0x08
        }

        public struct Entry
        {
            public uint Hash;
            public uint ImageOffset;
            public uint SectionSize;
            public uint Flag; //0 or 12
        }

        public enum PTLGType
        {
            Wii,
            GC,
            GCCompact
        }
    }
}
