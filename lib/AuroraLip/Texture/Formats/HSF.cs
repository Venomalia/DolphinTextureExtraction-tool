using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    //base https://github.com/Ploaj/Metanoia/blob/master/Metanoia/Formats/GameCube/HSF.cs
    public class HSF : JUTTexture, IFileAccess, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "HSFV037";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(Magic);

        protected override void Read(Stream stream)
        {
            stream.Position = 8;
            var header = stream.Read<Header>(Endian.Big);

            stream.Seek(header.Textures.Offset, SeekOrigin.Begin);
            TextureInfo[] texInfo = stream.For(header.Textures.Count, S => S.Read<TextureInfo>(Endian.Big));
            var startOffset = stream.Position;

            stream.Seek(header.Palettes.Offset, SeekOrigin.Begin);
            PaletteInfo[] palInfo = stream.For(header.Palettes.Count, S => S.Read<PaletteInfo>(Endian.Big));
            var palSectionOffset = stream.Position;

            for (int i = 0; i < texInfo.Length; i++)
            {
                var textureName = stream.At(header.StringTable.Offset + texInfo[i].NameOffset, S => S.ReadString());

                texInfo[i].GetGXFormat(out GXImageFormat Format, out GXPaletteFormat PaletteFormat);

                // Get palett Info
                PaletteInfo? palette = null;
                if (texInfo[i].PaletteIndex == -1)
                {
                    try
                    {
                        palette = palInfo.First(p => p.NameOffset == texInfo[i].NameOffset);
                    }
                    catch (Exception) { }
                }
                else
                {
                    palette = palInfo[texInfo[i].PaletteIndex];
                }

                //unfortunately i do not know where the mip flag is, so => Get Mipmaps From Size
                var Images = 0;
                if (i == texInfo.Length - 1)
                {
                    Images = (int)(header.Palettes.Offset - startOffset - texInfo[i].DataOffset);
                }
                else
                {
                    Images = (int)(texInfo[i + 1].DataOffset - texInfo[i].DataOffset);
                }
                Images = Format.GetMipmapsFromSize(Images, texInfo[i].Width, texInfo[i].Height);

                // get Palett Data
                var PaletteCount = 0;
                byte[] PaletteData = null;
                if (Format.IsPaletteFormat())
                {
                    if (palette != null)
                    {
                        PaletteCount = palette.Value.Count;
                        var palName = stream.At(header.StringTable.Offset + palette.Value.NameOffset, S => S.ReadString());
                        PaletteData = stream.At(palSectionOffset + palette.Value.DataOffset, S => S.Read(2 * PaletteCount));
                    }
                }

                stream.Seek(startOffset + texInfo[i].DataOffset, SeekOrigin.Begin);

                TexEntry current = new TexEntry(stream, PaletteData, Format, PaletteFormat, PaletteCount, texInfo[i].Width, texInfo[i].Height, Images)
                {
                    LODBias = 0,
                    MagnificationFilter = GXFilterMode.Nearest,
                    MinificationFilter = GXFilterMode.Nearest,
                    WrapS = GXWrapMode.CLAMP,
                    WrapT = GXWrapMode.CLAMP,
                    MinLOD = 0,
                    MaxLOD = 0
                };
                Add(current);

            }

        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        private struct Position
        {
            public uint Offset;
            public int Count;
        }

        private struct Header
        {
            public uint Unk1;
            public uint Unk2;
            public uint Unk3;
            public uint Flag;
            public Position Material1Table;
            public Position MaterialTable;
            public Position Positions;
            public Position Normals;
            public Position UV;
            public Position Primitives;
            public Position Bones;
            public Position Textures;
            public Position Palettes;
            public Position Unkown1;
            public Position Rig;
            public Position Unkown2;
            public Position Unkown3;
            public Position Unkown4;
            public Position Unkown5;
            public Position Unkown6;
            public Position Unkown7;
            public Position Unkown8;
            public Position StringTable;
        }

        private struct TextureInfo
        {
            public uint NameOffset;
            public uint Padding;
            public HSFImageFormat Format;
            public byte Type;
            public ushort Width;
            public ushort Height;
            public ushort Depth;
            public uint Padding1;
            public int PaletteIndex; // -1 usually excet for paletted?
            public uint Padding2;
            public uint DataOffset;

            public void GetGXFormat(out GXImageFormat imageFormat, out GXPaletteFormat paletteFormat)
            {
                switch (Format)
                {
                    case HSFImageFormat.Palette1:
                    case HSFImageFormat.Palette2:
                    case HSFImageFormat.Palette3:
                        paletteFormat = Format == HSFImageFormat.Palette2 ? GXPaletteFormat.RGB5A3 : GXPaletteFormat.RGB565;
                        if (Type == 4)
                            imageFormat = GXImageFormat.C4;
                        else
                            imageFormat = GXImageFormat.C8;
                        return;
                    default:
                        imageFormat = (GXImageFormat)Enum.Parse(typeof(GXImageFormat), Format.ToString());
                        paletteFormat = GXPaletteFormat.IA8;
                        return;
                }
            }
        }

        private struct PaletteInfo
        {
            public uint NameOffset;
            public int Format;
            public int Count;
            public uint DataOffset;
        }

        private enum HSFImageFormat : byte
        {
            I4 = 0x00,
            I8 = 0x01,
            IA4 = 0x02,
            IA8 = 0x03,
            RGB565 = 0x04,
            RGB5A3 = 0x05,
            RGBA32 = 0x06,
            CMPR = 0x07,
            Palette1 = 0x09,
            Palette2 = 0x0A,
            Palette3 = 0x0B,
        }

    }
}
