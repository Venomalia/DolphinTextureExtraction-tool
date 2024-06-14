using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Texture.Formats
{
    //base https://github.com/Ploaj/Metanoia/blob/master/Metanoia/Formats/GameCube/HSF.cs
    public class HSF : JUTTexture, IFileAccess, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier64 _identifier = new(15537406417982280);

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
            Header header = stream.Read<Header>(Endian.Big);
            stream.Seek(header.Textures.Offset, SeekOrigin.Begin);
            Span<TextureInfo> texInfos = stackalloc TextureInfo[header.Textures.Count];
            stream.Read(texInfos, Endian.Big);
            long startOffset = stream.Position;

            stream.Seek(header.Palettes.Offset, SeekOrigin.Begin);
            Span<PaletteInfo> palInfo = stackalloc PaletteInfo[header.Palettes.Count];
            stream.Read(palInfo, Endian.Big);
            long palSectionOffset = stream.Position;

            for (int i = 0; i < texInfos.Length; i++)
            {
                string textureName = stream.At(header.StringTable.Offset + texInfos[i].NameOffset, S => S.ReadString());
                GXImageFormat format = texInfos[i].GetGXImageFormat();
                GXPaletteFormat paletteFormat = texInfos[i].GetGXPaletteFormat();

                //unfortunately i do not know where the mip flag is, so => Get Mipmaps From Size
                int Images = 0;
                if (i == texInfos.Length - 1)
                {
                    Images = (int)(header.Palettes.Offset - startOffset - texInfos[i].DataOffset);
                }
                else
                {
                    Images = (int)(texInfos[i + 1].DataOffset - texInfos[i].DataOffset);
                }
                Images = format.GetMipmapsFromSize(Images, texInfos[i].Width, texInfos[i].Height);

                // get Palette Data
                PaletteInfo palette = default;
                Span<byte> PaletteData = Span<byte>.Empty;
                if (format.IsPaletteFormat())
                {
                    if (texInfos[i].PaletteIndex == -1)
                    {
                        foreach (PaletteInfo pInfo in palInfo)
                        {
                            if (pInfo.NameOffset == texInfos[i].NameOffset)
                            {
                                palette = pInfo;
                                break;
                            }
                        }
                    }
                    else
                    {
                        palette = palInfo[texInfos[i].PaletteIndex];
                    }
                    if (palette.Count != 0)
                    {
                        stream.Seek(palSectionOffset + palette.DataOffset, SeekOrigin.Begin);
                        PaletteData = new byte[palette.Count *2];
                        stream.Read(PaletteData);
                    }
                }

                stream.Seek(startOffset + texInfos[i].DataOffset, SeekOrigin.Begin);
                TexEntry current = new(stream, PaletteData, format, paletteFormat, palette.Count, texInfos[i].Width, texInfos[i].Height, Images)
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

        private readonly struct Position
        {
            public readonly uint Offset;
            public readonly int Count;
        }

        private readonly struct Header
        {
            public readonly uint Unk1;
            public readonly uint Unk2;
            public readonly uint Unk3;
            public readonly uint Flag;
            public readonly Position Material1Table;
            public readonly Position MaterialTable;
            public readonly Position Positions;
            public readonly Position Normals;
            public readonly Position UV;
            public readonly Position Primitives;
            public readonly Position Bones;
            public readonly Position Textures;
            public readonly Position Palettes;
            public readonly Position Unkown1;
            public readonly Position Rig;
            public readonly Position Unkown2;
            public readonly Position Unkown3;
            public readonly Position Unkown4;
            public readonly Position Unkown5;
            public readonly Position Unkown6;
            public readonly Position Unkown7;
            public readonly Position Unkown8;
            public readonly Position StringTable;
        }

        private readonly struct TextureInfo
        {
            public readonly int NameOffset;
            public readonly uint Padding;
            public readonly HSFImageFormat Format;
            public readonly byte Type;
            public readonly ushort Width;
            public readonly ushort Height;
            public readonly ushort Depth;
            public readonly uint Padding1;
            public readonly int PaletteIndex; // -1 usually excet for paletted?
            public readonly uint Padding2;
            public readonly uint DataOffset;

            public readonly GXImageFormat GetGXImageFormat()
            {
                switch (Format)
                {
                    case HSFImageFormat.Palette1:
                    case HSFImageFormat.Palette2:
                    case HSFImageFormat.Palette3:
                        return Type == 4 ? GXImageFormat.C4 : GXImageFormat.C8;
                    default:
                        return (GXImageFormat)Enum.Parse(typeof(GXImageFormat), Format.ToString());
                }
            }

            public readonly GXPaletteFormat GetGXPaletteFormat() => Format switch
            {
                HSFImageFormat.Palette1 => GXPaletteFormat.RGB565,
                HSFImageFormat.Palette2 => GXPaletteFormat.RGB5A3,
                _ => GXPaletteFormat.IA8,
            };
        }

        private readonly struct PaletteInfo
        {
            public readonly uint NameOffset;
            public readonly int Format;
            public readonly int Count;
            public readonly uint DataOffset;
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
