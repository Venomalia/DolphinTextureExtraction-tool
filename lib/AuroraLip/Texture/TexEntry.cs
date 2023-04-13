using AuroraLib.Common;
using AuroraLib.Palette;
using AuroraLip.Texture;
using System.Drawing;
using static AuroraLib.Texture.J3DTextureConverter;

namespace AuroraLib.Texture
{

    /*
    * Base on https://github.com/SuperHackio/Hack.io
    */

    public abstract partial class JUTTexture
    {
        /// <summary>
        /// A JUTTexture Entry. ccan contains Mipmaps ant Palettes
        /// </summary>
        public class TexEntry : IDisposable
        {
            /// <summary>
            /// specifies how the data within the image is encoded.
            /// </summary>
            public GXImageFormat Format { get; set; } = GXImageFormat.CMPR;

            /// <summary>
            /// specifies how the data within the palette is stored.
            /// Only C4, C8, and C14X2 use palettes. For all other formats the type is zero.
            /// </summary>
            public GXPaletteFormat PaletteFormat => Palettes.Count == 0 ? GXPaletteFormat.IA8 : Palettes[0].Format;

            /// <summary>
            /// Specifies how textures outside the vertical range [0..1] are treated for text coordinates.
            /// </summary>
            public GXWrapMode WrapS { get; set; } = 0;

            /// <summary>
            /// Specifies how textures outside the horizontal range [0..1] are treated for text coordinates.
            /// </summary>
            public GXWrapMode WrapT { get; set; } = 0;

            /// <summary>
            /// specifies what type of filtering the file should use as magnification filter.
            /// </summary>
            public GXFilterMode MagnificationFilter { get; set; } = 0;

            /// <summary>
            /// specifies what type of filtering the file should use as minification filter.
            /// </summary>
            public GXFilterMode MinificationFilter { get; set; } = 0;

            /// <summary>
            /// the calculated 64-bit xxHash of the base texture.
            /// </summary>
            public ulong Hash { get; internal set; } = 0;

            /// <summary>
            /// "Min Level of Detail" 
            /// Exclude textures below a certain LOD level from being used.
            /// </summary>
            public float MinLOD { get; set; } = 0;

            /// <summary>
            /// "Max Level of Detail" 
            /// Exclude textures above a certain LOD level from being used.
            /// A value larger than the actual textures should lead to culling.
            /// </summary>
            public float MaxLOD { get; set; }

            /// <summary>
            /// "Level of Detail Bias" 
            /// A larger value leads to a larger camera distance before a lower LOD resolution is selected.
            /// </summary>
            public float LODBias { get; set; }

            /// <summary>
            /// ?
            /// </summary>
            public bool EnableEdgeLOD { get; set; }

            /// <summary>
            /// Pallets raw data
            /// </summary>
            public List<JUTPalette> Palettes { get; set; } = new();

            /// <summary>
            /// Gets the image width, in pixels.
            /// </summary>
            public int ImageWidth;

            /// <summary>
            /// Gets the image height, in pixels.
            /// </summary>
            public int ImageHeight;

            /// <summary>
            /// Number of images
            /// </summary>
            public int Count => RawImages.Count;

            /// <summary>
            /// Raw image and mips
            /// </summary>
            public readonly List<byte[]> RawImages = new();

            #region Constructor

            /// <summary>
            /// Creates an empty TexEntry
            /// </summary>
            public TexEntry() { }

            /// <summary>
            /// Creates an TexEntry from a Stream
            /// </summary>
            public TexEntry(Stream Stream, GXImageFormat Format, int ImageWidth, int ImageHeight, int Mipmap = 0)
            {
                this.Format = Format;
                this.ImageHeight = ImageHeight;
                this.ImageWidth = ImageWidth;

                //reads all row image data.
                for (int i = 0; i <= Mipmap; i++)
                {
                    RawImages.Add(Stream.Read(Format.GetCalculatedDataSize(ImageWidth, ImageHeight, i)));
                }
                Hash = HashDepot.XXHash.Hash64(RawImages[0]);
            }

            /// <summary>
            /// Creates an TexEntry with Pallets data from a Stream
            /// </summary>
            public TexEntry(Stream Stream, ReadOnlySpan<byte> PaletteData, GXImageFormat Format, GXPaletteFormat PaletteFormat, int PaletteCount, int ImageWidth, int ImageHeight, int Mipmap = 0) : this(Stream, Format, ImageWidth, ImageHeight, Mipmap)
            {
                //Splits the pallete data if there are more than one
                if (Format.IsPaletteFormat() && PaletteCount > 0)
                {
                    int PalettesNumber = PaletteData.Length / (PaletteCount * 2);
                    if (PalettesNumber <= 1)
                        Palettes.Add(new JUTPalette(PaletteFormat, PaletteData, PaletteCount));
                    else
                    {
                        int PaletteSize = PaletteData.Length / PalettesNumber;

                        for (int i = 0; i < PalettesNumber; i++)
                            Palettes.Add(new JUTPalette(PaletteFormat, PaletteData.Slice(i * PaletteSize, PaletteSize), PaletteSize / 2));
                    }
                }
            }

            /// <summary>
            /// Creates an TexEntry from a Bitmap
            /// </summary>
            public TexEntry(Bitmap Image, GXImageFormat ImageFormat = GXImageFormat.CMPR, GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8)
            {
                this.Format = Format;
                this.ImageHeight = Image.Height;
                this.ImageWidth = Image.Width;

                var ImageData = GetImageAndPaletteData(out byte[] PaletteData, Image, ImageFormat, PaletteFormat);
                if (ImageFormat.IsPaletteFormat())
                    Palettes.Add(new JUTPalette(PaletteFormat, PaletteData, PaletteData.Length / 2));
                this.RawImages.Add(ImageData);
            }

            /// <summary>
            /// Creates an TexEntry with mips from a Bitmap
            /// </summary>
            public TexEntry(List<Bitmap> Image, GXImageFormat ImageFormat = GXImageFormat.CMPR, GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8)
            {
                this.Format = Format;
                this.ImageHeight = Image[0].Height;
                this.ImageWidth = Image[0].Width;

                var ImageData = GetImageAndPaletteData(out byte[] PaletteData, Image, ImageFormat, PaletteFormat);
                if (ImageFormat.IsPaletteFormat())
                    Palettes.Add(new JUTPalette(PaletteFormat, PaletteData, PaletteData.Length / 2));

                foreach (var raw in ImageData)
                    this.RawImages.Add(raw);
            }

            #endregion

            public Bitmap AsBitmap(int Mipmap = 0, int Palette = 0)
                => AsBitmap(Mipmap, Palettes.Count == 0 ? null : Palettes[Palette]);

            public Bitmap AsBitmap(int Mipmap, JUTPalette Palette)
                => DecodeImage(RawImages[Mipmap], Palette?.ToArray(), Format, ImageWidth >> Mipmap, ImageHeight >> Mipmap);

            /// <summary>
            /// calculated the 64-bit xxHash of the Tlut
            /// </summary>
            /// <param name="Palette"></param>
            /// <returns></returns>
            public ulong GetTlutHash(int Palette = 0)
                => GetTlutHash(Palettes.Count == 0 ? null : Palettes[Palette]);

            /// <summary>
            /// calculated the 64-bit xxHash of the Tlut
            /// </summary>
            /// <param name="Palette"></param>
            /// <returns></returns>
            public ulong GetTlutHash(JUTPalette Palette)
            {
                if (!Format.IsPaletteFormat() || Palette == null) return 0;

                (int start, int length) = Format.GetTlutRange(RawImages[0].AsSpan());
                if (Palette.Size < length)
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Tlut out of range({start}-{length})Tlut_Length:{Palette.Size}");
                    return 0;
                }
                return HashDepot.XXHash.Hash64(Palette.GetBytes().AsSpan().Slice(start, length));
            }

            public string GetDolphinTextureHash(int mipmap = 0, ulong TlutHash = 0, bool DolphinMipDetection = true, bool IsArbitraryMipmap = false)
            {
                bool HasMips = this.Count != 1;
                //dolphin seems to use the MaxLOD value to decide if it is a mipmap Texture.
                //https://github.com/dolphin-emu/dolphin/blob/master/Source/Core/VideoCommon/TextureInfo.cpp#L80
                if (!HasMips && DolphinMipDetection)
                    HasMips = MaxLOD != 0;

                return DolphinTextureHashInfo.Build(this.ImageWidth, this.ImageHeight, Hash, Format, TlutHash, mipmap, HasMips, IsArbitraryMipmap);
            }

            public override bool Equals(object obj)
            {
                return obj is TexEntry entry &&
                       Format == entry.Format &&
                       PaletteFormat == entry.PaletteFormat &&
                       WrapS == entry.WrapS &&
                       WrapT == entry.WrapT &&
                       MagnificationFilter == entry.MagnificationFilter &&
                       MinificationFilter == entry.MinificationFilter &&
                       MinLOD == entry.MinLOD &&
                       MaxLOD == entry.MaxLOD &&
                       LODBias == entry.LODBias &&
                       EnableEdgeLOD == entry.EnableEdgeLOD;
            }

            public static bool operator ==(TexEntry entry1, TexEntry entry2) => entry1.Equals(entry2);

            public static bool operator !=(TexEntry entry1, TexEntry entry2) => !(entry1 == entry2);

            public override int GetHashCode()
            {
                HashCode hash = new();
                hash.Add(Format);
                hash.Add(Palettes);
                hash.Add(WrapS);
                hash.Add(WrapT);
                hash.Add(MagnificationFilter);
                hash.Add(MinificationFilter);
                hash.Add(MinLOD);
                hash.Add(MaxLOD);
                hash.Add(LODBias);
                hash.Add(EnableEdgeLOD);
                return hash.ToHashCode();
            }

            public override string ToString()
                => GetDolphinTextureHash();

            #region Dispose
            private bool disposedValue;
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        //foreach (var item in this) item.Dispose();
                    }
                    disposedValue = true;
                }
            }
            ~TexEntry()
            {
                Dispose(disposing: false);
            }
            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            #endregion
        }
    }
}
