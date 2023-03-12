using AuroraLib.Archives;
using AuroraLib.Common;
using AuroraLib.Palette.Formats;

namespace AuroraLib.Texture.Formats
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


            if (PaletteData == null && Format.IsPaletteFormat())
            {
                if (stream is ArchiveFile.ArchiveFileStream substream)
                {
                    string name;
                    lock (substream.BaseStream)
                    {
                        substream.BaseStream.Seek(substream.Offset + StringOffset, SeekOrigin.Begin);
                        name = substream.BaseStream.ReadString();
                    }
                    Archive ParentBres = substream.Parent.OwnerArchive;
                    if (ParentBres.ItemExists("Palettes(NW4R)"))
                    {
                        var PalletNames = ((ArchiveDirectory)ParentBres["Palettes(NW4R)"]).FindItems(name + "*");

                        if (PalletNames.Count == 0)
                        {
                            throw new PaletteException("No palette data could be found");
                        }

                        stream.Position = SectionOffsets;
                        var tex = new TexEntry(stream, Format, ImageWidth, ImageHeight, TotalImageCount - 1)
                        {
                            LODBias = 0,
                            MagnificationFilter = GXFilterMode.Nearest,
                            MinificationFilter = GXFilterMode.Nearest,
                            WrapS = GXWrapMode.CLAMP,
                            WrapT = GXWrapMode.CLAMP,
                            EnableEdgeLOD = false,
                            MinLOD = MinLOD,
                            MaxLOD = MaxLOD
                        };

                        foreach (var PalletName in PalletNames)
                        {
                            ArchiveFile PFile = (ArchiveFile)ParentBres[PalletName];
                            lock (PFile.FileData)
                            {
                                PFile.FileData.Seek(0, SeekOrigin.Begin);
                                tex.Palettes.Add(new PLT0(PFile.FileData));
                            }
                        }
                        Add(tex);
                        return;
                    }
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
