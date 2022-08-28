using AuroraLip.Archives;
using AuroraLip.Archives.Formats;
using AuroraLip.Common;
using AuroraLip.Palette.Formats;
using AuroraLip.Texture.J3D;
using System;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

namespace AuroraLip.Texture.Formats
{
    public class TEX0 : JUTTexture, IMagicIdentify, IFileAccess
    {

        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        private const string magic = "TEX0";

        public TEX0() { }

        public TEX0(Stream stream) : base(stream) { }

        public TEX0(string filepath) : base(filepath) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream) => Read(stream, null, GXPaletteFormat.IA8, 0);

        protected void Read(Stream stream, byte[] PaletteData, GXPaletteFormat PaletteFormat, int PaletteCount)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);
            uint TotalSize = stream.ReadUInt32(Endian.Big);
            uint FormatVersion = stream.ReadUInt32(Endian.Big);
            uint Offset = stream.ReadUInt32(Endian.Big);
            long SectionOffsets;
            if (FormatVersion == 2)
            {
                SectionOffsets = (long)stream.ReadUInt64(Endian.Big);
            }
            else
            {
                SectionOffsets = stream.ReadUInt32(Endian.Big);
            }
            uint StringOffset = stream.ReadUInt32(Endian.Big);
            //TEX0 Header
            uint HasPalette = stream.ReadUInt32(Endian.Big);
            int ImageWidth = stream.ReadUInt16(Endian.Big);
            int ImageHeight = stream.ReadUInt16(Endian.Big);
            GXImageFormat Format = (GXImageFormat)stream.ReadUInt32(Endian.Big);
            int TotalImageCount = stream.ReadInt32(Endian.Big);
            float MinLOD = stream.ReadSingle(Endian.Big);
            float MaxLOD = stream.ReadSingle(Endian.Big);
            uint Unknown1 = stream.ReadUInt32(Endian.Big);
            uint Unknown2 = stream.ReadUInt32(Endian.Big);


            if (PaletteData == null && JUtility.IsPaletteFormat(Format))
            {
                if (stream is bres.DataStream substream)
                {
                    string name;
                    lock (substream.BaseStream)
                    {
                        substream.BaseStream.Seek(substream.Offset + StringOffset, SeekOrigin.Begin);
                        name = substream.BaseStream.ReadString();
                    }
                    if (substream.Parent.ItemExists("root/Palettes(NW4R)"))
                    {
                        var Pallets = ((ArchiveDirectory)substream.Parent["root/Palettes(NW4R)"]).FindItems(name + "*");
                        if (Pallets.Count == 1)
                        {
                            ArchiveFile PFile = (ArchiveFile)substream.Parent[Pallets[0]];
                            var Pallet = new PLT0(PFile.FileData);
                            PaletteFormat = Pallet.PaletteFormat;
                            PaletteData = Pallet.PaletteData;
                            PaletteCount = Pallet.PaletteData.Length / 2;
                        }
                        else if (Pallets.Count > 1)
                        {
#if DEBUG
                            return;
#else
                            throw new PaletteException($"multiple palettes ({Pallets.Count}) are not supported");
#endif
                        }
                    }
                }
                if (PaletteData == null)
                {
#if DEBUG
                    return;
#else
                    throw new PaletteException("No palette data could be found");
#endif
                }
            }
            stream.Position = SectionOffsets;
            Add(new TexEntry(stream, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight, TotalImageCount - 1)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = MinLOD,
                MaxLOD = MaxLOD
            });
        }


        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
