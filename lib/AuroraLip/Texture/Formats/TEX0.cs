using AuroraLib.Archives.Formats.Nintendo;
using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;
using AuroraLib.Palette.Formats;

namespace AuroraLib.Texture.Formats
{
    public class TEX0 : JUTTexture, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public virtual IIdentifier Identifier => _identifier;

        private const string PalettesPath = "Palettes(NW4R)";
        private static readonly Identifier32 _identifier = new("TEX0");

        public TEX0()
        { }

        public TEX0(Stream stream) : base(stream)
        {
        }

        public TEX0(string filepath) : base(filepath)
        {
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream) => Read(stream, null, GXPaletteFormat.IA8, 0);

        protected void Read(Stream stream, Span<byte> PaletteData, GXPaletteFormat PaletteFormat, int PaletteCount)
        {
            stream.MatchThrow(_identifier);

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
                //only temporary
                if (stream is Bres.SubStream substream)
                {
                    string name = substream.Name;
                    IEnumerable<FileNode> PalletNames = new List<FileNode>();
                    DirectoryNode ParentBres = substream.Root;
                    if (ParentBres.Contains(PalettesPath))
                        PalletNames = ((DirectoryNode)ParentBres[PalettesPath]).Search<FileNode>(name + "*");

                    if (!PalletNames.Any())
                    {
                        throw new PaletteException("No linked pallete palette data could be found");
                    }


                    stream.Position = SectionOffsets;
                    TexEntry tex = new(stream, Format, ImageWidth, ImageHeight, TotalImageCount - 1)
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

                    foreach (FileNode PFile in PalletNames)
                    {
                        lock (PFile.Data)
                        {
                            PFile.Data.Seek(0, SeekOrigin.Begin);

                            PLT0 pallet = new(PFile.Data);
                            tex.PaletteFormat = pallet.Format;
                            tex.Palettes.Add(pallet.Data);
                        }
                    }
                    Add(tex);
                    return;
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
