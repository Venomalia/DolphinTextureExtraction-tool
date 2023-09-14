using AuroraLib.Common;
using AuroraLib.Core.Buffers;
using AuroraLib.Texture.BlockFormats;
using AuroraLib.Texture.Interfaces;
using AuroraLib.Texture.PixelFormats;
using System.Runtime.InteropServices;

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
            /// Creates an empty <see cref="TexEntry"/>.
            /// </summary>
            public TexEntry()
            { }


            public TexEntry(Image image)
            {
                Height = image.Height;
                Width = image.Width;
                switch (image)
                {
                    case Image<I8>:
                        Format = GXImageFormat.I8;
                        RawImages.Add(((IBlock<I8>)new I8Block()).EncodeImage((Image<I8>)image));
                        break;
                    case Image<IA4>:
                        Format = GXImageFormat.RGBA32;
                        RawImages.Add(((IBlock<IA4>)new IA4Block()).EncodeImage((Image<IA4>)image));
                        break;
                    case Image<IA8>:
                        Format = GXImageFormat.RGBA32;
                        RawImages.Add(((IBlock<IA8>)new IA8Block()).EncodeImage((Image<IA8>)image));
                        break;
                    case Image<RGB565>:
                        Format = GXImageFormat.RGBA32;
                        RawImages.Add(((IBlock<RGB565>)new RGB565Block()).EncodeImage((Image<RGB565>)image));
                        break;
                    case Image<RGB5A3>:
                        Format = GXImageFormat.RGBA32;
                        RawImages.Add(((IBlock<RGB5A3>)new RGB5A3Block()).EncodeImage((Image<RGB5A3>)image));
                        break;
                    case Image<Rgba32>:
                        Format = GXImageFormat.RGBA32;
                        RawImages.Add(((IBlock<Rgba32>)new RGBA32Block()).EncodeImage((Image<Rgba32>)image));
                        break;
                    default:
                        throw new NotImplementedException($"{nameof(TexEntry)} {image}.");
                }
                Hash = HashDepot.XXHash.Hash64(RawImages[0]);
            }

            /// <summary>
            /// Creates an new <see cref="TexEntry"/>.
            /// </summary>
            public TexEntry(byte[] data, GXImageFormat format, int width, int height)
            {
                RawImages.Add(data);
                Format = format;
                Height = height;
                Width = width;
                Hash = HashDepot.XXHash.Hash64(data);
            }

            /// <summary>
            /// Creates an <see cref="TexEntry"/> from a <see cref="Stream"/>,
            /// Automatically reads all pixel data.
            /// </summary>
            public TexEntry(Stream stream, GXImageFormat format, int width, int height, int mipmaps = 0)
            {
                Format = format;
                Height = height;
                Width = width;
                ReadHelper(stream, format, width, height, mipmaps);
                Hash = HashDepot.XXHash.Hash64(RawImages[0]);
            }

            /// <summary>
            /// Can be used for formats that do not correspond to the GXFormat.
            /// </summary>
            /// <exception cref="NotImplementedException"></exception>
            public TexEntry(Stream stream, AImageFormats format, int width, int height, int mipmaps = 0)
            {
                Format = (GXImageFormat)format;
                Height = height;
                Width = width;
                ReadHelper(stream, (GXImageFormat)format, width, height, mipmaps);


                if ((uint)format >> 28 == 0x0C)
                {
                    for (int i = 0; i < RawImages.Count; i++)
                    {
                        switch (format)
                        {
                            case AImageFormats.DXT1:
                                GXImageEX.ConvertDXT1ToCMPR(RawImages[i], width, height);
                                break;
                            case AImageFormats.I4:
                            case AImageFormats.C4:
                                //convert int 4 to int8 to make it easier to work with.
                                SpanBuffer<byte> bufferI8 = new(RawImages[i].Length * 2);
                                for (int w = 0; w < RawImages[i].Length; w++)
                                {
                                    bufferI8[w * 2] = (byte)(RawImages[i][w] & 0xF);
                                    bufferI8[w * 2 + 1] = (byte)(RawImages[i][w] >> 4);
                                }
                                RawImages[i] = ((IBlock<I8>)new I4Block()).EncodePixel(bufferI8, width, height);
                                bufferI8.Dispose();
                                break;
                            case AImageFormats.I8:
                            case AImageFormats.C8:
                                RawImages[i] = ((IBlock<I8>)new I8Block()).EncodePixel(RawImages[i], width, height);
                                break;
                            case AImageFormats.IA4:
                                RawImages[i] = ((IBlock<IA4>)new IA4Block()).EncodePixel(RawImages[i], width, height);
                                break;
                            case AImageFormats.IA8:
                                RawImages[i] = ((IBlock<IA8>)new IA8Block()).EncodePixel(RawImages[i], width, height);
                                break;
                            case AImageFormats.RGB565:
                                RawImages[i] = ((IBlock<RGB565>)new RGB565Block()).EncodePixel(RawImages[i], width, height);
                                break;
                            case AImageFormats.RGB5A3:
                                RawImages[i] = ((IBlock<RGB5A3>)new RGB5A3Block()).EncodePixel(RawImages[i], width, height);
                                break;
                            case AImageFormats.RGBA32:
                                RawImages[i] = ((IBlock<Rgba32>)new RGBA32Block()).EncodePixel(RawImages[i], width, height);
                                break;
                            case AImageFormats.C14X2:
                                throw new NotImplementedException();
                            case AImageFormats.PS2RGBA32:
                                //PS2 Alpha channel must be normalized. Used in Dokapon Kingdom maybe also in other ports but technically not correct.
                                for (int w = 3; w < RawImages[i].Length; w += 4)
                                {
                                    if (RawImages[i][w] == 128) RawImages[i][w] = 255;
                                }
                                RawImages[i] = ((IBlock<Rgba32>)new RGBA32Block()).EncodePixel(RawImages[i], width, height);
                                break;
                        }
                        width >>= 1;
                        height >>= 1;
                    }
                }
                Hash = HashDepot.XXHash.Hash64(RawImages[0]);
            }

            public TexEntry(Stream stream, int width, int height, int mipmaps, uint RBitMask, uint GBitMask, uint BBitMask, uint ABitMask)
            {
                Format = GXImageFormat.RGBA32;
                Height = height;
                Width = width;
                ReadHelper(stream, GXImageFormat.RGBA32, width, height, mipmaps);

                for (int i = 0; i < RawImages.Count; i++)
                {
                    GXImageEX.ToRGBA32(RawImages[i], RBitMask, GBitMask, BBitMask, ABitMask);
                    Span<Rgba32> pixel = MemoryMarshal.Cast<byte, Rgba32>(RawImages[i]);
                    RawImages[i] = ((IBlock<Rgba32>)new RGBA32Block()).EncodePixel(pixel, (int)Width, (int)Height);
                }

                Hash = HashDepot.XXHash.Hash64(RawImages[0]);
            }

            /// <summary>
            /// Creates an <see cref="TexEntry"/> from a <see cref="Stream"/>,
            /// Automatically reads all pixel data and a Palette if present.
            /// </summary>
            public TexEntry(Stream stream, GXImageFormat format, GXPaletteFormat paletteFormat, int width, int height, int mipmaps = 0) : this(stream, format, width, height, mipmaps)
            {
                PaletteFormat = paletteFormat;
                if (format.IsPaletteFormat())
                {
                    int palettesize = format.GetMaxPaletteSize();
                    if (stream.Position + palettesize > stream.Length)
                    {
                        Events.NotificationEvent.Invoke(NotificationType.Info, $"Cannot read Palette, is beyond the end of the stream.");
                    }
                    else
                    {
                        Palettes.Add(stream.Read(palettesize));
                    }
                }
            }

            /// <inheritdoc cref="TexEntry.TexEntry(Stream, GXImageFormat, int, int, int)"/>
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

            private void ReadHelper(Stream stream, GXImageFormat format, int width, int height, int mipmaps = 0)
            {
                for (int i = 0; i <= mipmaps; i++)
                {
                    if (width == 0 || height == 0)
                    {
                        Events.NotificationEvent.Invoke(NotificationType.Info, $"Cannot read mip nummber {i}-{mipmaps} image size would be \"0\".");
                        break;
                    }

                    int imageSize = format.CalculatedDataSize(width, height);

                    if (stream.Position + imageSize > stream.Length)
                    {
                        if (i == 0)
                            throw new EndOfStreamException($"Cannot read {imageSize} bytes of pixel data is beyond the end of the stream.");

                        Events.NotificationEvent.Invoke(NotificationType.Info, $"Cannot read mip nummber {i}-{mipmaps} is beyond the end of the stream.");
                        break;
                    }
                    byte[] imageData = stream.Read(imageSize);
                    RawImages.Add(imageData);

                    width >>= 1;
                    height >>= 1;
                }
            }

            #endregion Constructor

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

            #endregion Dispose
        }
    }
}
