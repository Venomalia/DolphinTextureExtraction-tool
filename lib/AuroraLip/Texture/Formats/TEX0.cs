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
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");
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
            uint Unknown = stream.ReadUInt32(Endian.Big); //1
            int ImageWidth = stream.ReadUInt16(Endian.Big);
            int ImageHeight = stream.ReadUInt16(Endian.Big);
            GXImageFormat Format = (GXImageFormat)stream.ReadUInt32(Endian.Big);
            int TotalImageCount = stream.ReadUInt16(Endian.Big);
            uint Unknown2 = stream.ReadUInt32(Endian.Big); // 65536
            uint Mipmaps = stream.ReadUInt32(Endian.Big);
            uint Unknown3 = stream.ReadUInt32(Endian.Big);


            if (PaletteData == null && JUtility.IsPaletteFormat(Format))
            {
                if (stream is SubStream substream)
                {
                    lock (substream.BaseStream)
                    {

                        substream.BaseStream.Seek(substream.Offset + StringOffset, SeekOrigin.Begin);
                        string name = substream.BaseStream.ReadString();
                        substream.BaseStream.Seek(0, SeekOrigin.Begin);
                        using (bres parent = new bres(substream.BaseStream))
                        {
                            if (parent.ItemExists("root/Palettes(NW4R)"))
                            {
                                var Pallets = ((ArchiveDirectory)parent["root/Palettes(NW4R)"]).FindItems(name + "*");
                                if (Pallets.Count == 1)
                                {
                                    var Pallet = new PLT0(((ArchiveFile)parent[Pallets[0]]).FileData);
                                    PaletteFormat = Pallet.PaletteFormat;
                                    PaletteData = Pallet.PaletteData;
                                    PaletteCount = Pallet.PaletteData.Length / 2;
                                }
                                else if (Pallets.Count > 1)
                                {
#if DEBUG
                                    return;
#else
                                    throw new Exception($"multiple palettes ({Pallets.Count}) are not supported");
#endif
                                }
                            }
                        }
                    }
                }
                if (PaletteData == null)
                {
#if DEBUG
                    return;
#else
                    throw new Exception("No palette data could be found");
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
                MinLOD = 0,
                MaxLOD = 0
            });
        }


        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
