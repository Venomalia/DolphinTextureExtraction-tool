using AuroraLib.Common;
using AuroraLib.Palette;
using AuroraLib.Texture;
using AuroraLib.Texture.Interfaces;
using System.Drawing;
using Image = SixLabors.ImageSharp.Image;

namespace AuroraLib.Texture
{

    public abstract partial class JUTTexture
    {
        /// <summary>
        /// A JUTTexture Entry. ccan contains Mipmaps ant Palettes
        /// </summary>
        public class TexEntry : IDisposable, IGXTexture
        {
            /// <inheritdoc/>
            public int Width { get; set; }

            /// <inheritdoc/>
            public int Height { get; set; }

            /// <inheritdoc/>
            public GXImageFormat Format { get; set; } = GXImageFormat.CMPR;

            /// <inheritdoc/>
            public GXPaletteFormat PaletteFormat { get; set; } = GXPaletteFormat.IA8;

            /// <inheritdoc/>
            public GXWrapMode WrapS { get; set; } = GXWrapMode.CLAMP;

            /// <inheritdoc/>
            public GXWrapMode WrapT { get; set; } = GXWrapMode.CLAMP;

            /// <inheritdoc/>
            public GXFilterMode MagnificationFilter { get; set; } = GXFilterMode.Nearest;

            /// <inheritdoc/>
            public GXFilterMode MinificationFilter { get; set; } = GXFilterMode.Nearest;

            /// <inheritdoc/>
            public float MinLOD { get; set; } = 0;

            /// <inheritdoc/>
            public float MaxLOD { get; set; } = 0;

            /// <inheritdoc/>
            public float LODBias { get; set; } = 0;

            /// <inheritdoc/>
            public bool EnableEdgeLOD { get; set; } = true;

            /// <inheritdoc/>
            public int MipMaps => RawImages.Count - 1;

            /// <summary>
            /// the calculated 64-bit xxHash of the base texture.
            /// </summary>
            public ulong Hash { get; internal set; } = 0;

            /// <summary>
            /// Pallets raw data
            /// </summary>
            public List<byte[]> Palettes { get; set; } = new();

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
                this.Height = ImageHeight;
                this.Width = ImageWidth;

                //reads all row image data.
                for (int i = 0; i <= Mipmap; i++)
                {
                    RawImages.Add(Stream.Read(Format.CalculatedDataSize(ImageWidth, ImageHeight, i)));
                }
                Hash = HashDepot.XXHash.Hash64(RawImages[0]);
            }

            /// <summary>
            /// Creates an TexEntry with Pallets data from a Stream
            /// </summary>
            public TexEntry(Stream Stream, ReadOnlySpan<byte> PaletteData, GXImageFormat Format, GXPaletteFormat PaletteFormat, int PaletteCount, int ImageWidth, int ImageHeight, int Mipmap = 0) : this(Stream, Format, ImageWidth, ImageHeight, Mipmap)
            {
                this.PaletteFormat = PaletteFormat;
                //Splits the pallete data if there are more than one
                if (Format.IsPaletteFormat() && PaletteCount > 0)
                {
                    int PalettesNumber = PaletteData.Length / (PaletteCount * 2);
                    if (PalettesNumber <= 1)
                        Palettes.Add(PaletteData[..(PaletteCount * 2)].ToArray());
                    else
                    {
                        int PaletteSize = PaletteData.Length / PalettesNumber;

                        for (int i = 0; i < PalettesNumber; i++)
                            Palettes.Add(PaletteData.Slice(i * PaletteSize, PaletteSize).ToArray());
                    }
                }
            }

            #endregion

            public Image GetImage(int Mipmap = 0, int Palette = 0)
                => GetImage(Mipmap, Palettes.Count == 0 ? ReadOnlySpan<byte>.Empty : Palettes[Palette]);

            public Image GetImage(int Mipmap, ReadOnlySpan<byte> Palette)
                => Format.DecodeImage(RawImages[Mipmap], Width >> Mipmap, Height >> Mipmap, Palette, PaletteFormat);

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
            public ulong GetTlutHash(ReadOnlySpan<byte> Palette)
            {
                if (!Format.IsPaletteFormat() || Palette == null) return 0;

                (int start, int length) = Format.GetTlutRange(RawImages[0].AsSpan());
                if (Palette.Length < length)
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Tlut out of range({start}-{length})Tlut_Length:{Palette.Length}");
                    return 0;
                }
                return HashDepot.XXHash.Hash64(Palette.Slice(start, length));
            }

            public string GetDolphinTextureHash(int mipmap = 0, ulong TlutHash = 0, bool DolphinMipDetection = true, bool IsArbitraryMipmap = false)
            {
                bool HasMips = this.Count != 1;
                //dolphin seems to use the MaxLOD value to decide if it is a mipmap Texture.
                //https://github.com/dolphin-emu/dolphin/blob/master/Source/Core/VideoCommon/TextureInfo.cpp#L80
                if (!HasMips && DolphinMipDetection)
                    HasMips = MaxLOD != 0;

                return DolphinTextureHashInfo.Build(this.Width, this.Height, Hash, Format, TlutHash, mipmap, HasMips, IsArbitraryMipmap);
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
