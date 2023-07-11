using AuroraLib.Common;
using AuroraLib.Texture.BlockFormats;
using AuroraLib.Texture.PixelFormats;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture
{
    public static class GXImageEX
    {
        public static IBlock GetBlockDescription(this GXImageFormat Format)
            => Format switch
            {
                GXImageFormat.I4 => new I4Block(),
                GXImageFormat.I8 => new I8Block(),
                GXImageFormat.IA4 => new IA4Block(),
                GXImageFormat.IA8 => new IA8Block(),
                GXImageFormat.RGB565 => new RGB565Block(),
                GXImageFormat.RGB5A3 => new RGB5A3Block(),
                GXImageFormat.RGBA32 => new RGB5A3Block(),
                GXImageFormat.C4 => new I4Block(),
                GXImageFormat.C8 => new I8Block(),
                GXImageFormat.C14X2 => new I14Block(),
                GXImageFormat.CMPR => new CMPRBlock(),
                _ => throw new NotSupportedException("Unsupported GXImageFormat"),
            };

        public static Image DecodeImage(this GXImageFormat format, ReadOnlySpan<byte> data, int width, int height, ReadOnlySpan<byte> palette, GXPaletteFormat paletteFormat)
        {
            if (format.IsPaletteFormat() && palette.Length > 0)
            {
                switch (paletteFormat)
                {
                    case GXPaletteFormat.IA8:
                        Span<IA8> IA8 = stackalloc IA8[palette.Length / 2];
                        new IA8Block().DecodeBlock(palette, IA8);
                        return format.DecodeImage<IA8>(data, width, height, IA8);

                    case GXPaletteFormat.RGB565:
                        Span<RGB565> RGB565 = stackalloc RGB565[palette.Length / 2];
                        new RGB565Block().DecodeBlock(palette, RGB565);
                        return format.DecodeImage<RGB565>(data, width, height, RGB565);

                    case GXPaletteFormat.RGB5A3:
                        Span<RGB5A3> RGB5A3 = stackalloc RGB5A3[palette.Length / 2];
                        new RGB5A3Block().DecodeBlock(palette, RGB5A3);
                        return format.DecodeImage<RGB5A3>(data, width, height, RGB5A3);

                    default:
                        throw new NotSupportedException("Unsupported GXPaletteFormat");
                }
            }
            return format.DecodeImage(data, width, height, ReadOnlySpan<IA8>.Empty);
        }

        public static Image DecodeImage<TPixel>(this GXImageFormat Format, ReadOnlySpan<byte> data, int width, int height, ReadOnlySpan<TPixel> palette) where TPixel : unmanaged, IPixel<TPixel>
            => Format switch
            {
                GXImageFormat.I4 => ((IBlock<I8>)new I4Block()).DecodeImage(data, width, height),
                GXImageFormat.I8 => ((IBlock<I8>)new I8Block()).DecodeImage(data, width, height),
                GXImageFormat.IA4 => ((IBlock<IA4>)new IA4Block()).DecodeImage(data, width, height),
                GXImageFormat.IA8 => ((IBlock<IA8>)new IA8Block()).DecodeImage(data, width, height),
                GXImageFormat.RGB565 => ((IBlock<RGB565>)new RGB565Block()).DecodeImage(data, width, height),
                GXImageFormat.RGB5A3 => ((IBlock<RGB5A3>)new RGB5A3Block()).DecodeImage(data, width, height),
                GXImageFormat.RGBA32 => ((IBlock<Rgba32>)new RGBA32Block()).DecodeImage(data, width, height),
                GXImageFormat.C4 => ((IBlock<I8>)new I4Block()).DecodeImage(data, width, height).ApplyPalette(palette, p => p.PackedValue >> 4),
                GXImageFormat.C8 => ((IBlock<I8>)new I8Block()).DecodeImage(data, width, height).ApplyPalette(palette, p => p.PackedValue),
                GXImageFormat.C14X2 => ((IBlock<I16>)new I14Block()).DecodeImage(data, width, height).ApplyPalette(palette, p => p.PackedValue),
                GXImageFormat.CMPR => ((IBlock<Rgba32>)new CMPRBlock()).DecodeImage(data, width, height),
                _ => throw new NotSupportedException("Unsupported GXImageFormat"),
            };

        public static Image<XPixel> ApplyPalette<TPixel, XPixel>(this Image<TPixel> image, ReadOnlySpan<XPixel> palette, Converter<TPixel, int> pixelToIndex) where TPixel : unmanaged, IPixel<TPixel> where XPixel : unmanaged, IPixel<XPixel>
        {
            if (palette.Length == 0)
                throw new PaletteException("No palette data to apply.");

            Image<XPixel> paletteImage = new(image.Width, image.Height);

            IMemoryGroup<XPixel> pixelsPalette = paletteImage.GetPixelMemoryGroup();
            IMemoryGroup<TPixel> pixels = image.GetPixelMemoryGroup();

            for (int i = 0; i < pixels.Count; i++)
            {
                Span<XPixel> pixelPaletteRow = pixelsPalette[i].Span;
                ReadOnlySpan<TPixel> pixelRow = pixels[i].Span;

                for (int j = 0; j < pixelRow.Length; j++)
                {
                    int index = pixelToIndex.Invoke(pixelRow[j]);
                    pixelPaletteRow[j] = palette[index];
                }
            }
            return paletteImage;
        }

        /// <summary>
        /// Generates a specific Mipmap level of the image by resizing it using the specified <paramref name="resampler"/>.
        /// </summary>
        /// <param name="source">The source image processing context.</param>
        /// <param name="resampler">The resampler to use for resizing.</param>
        /// <param name="level">The desired Mipmap level to generate (default is 1).</param>
        /// <returns>The image processing context with the generated Mipmap level.</returns>
        public static IImageProcessingContext GenerateMipmap(this IImageProcessingContext source, IResampler resampler, in int level = 1)
        {
            SixLabors.ImageSharp.Size Size = source.GetCurrentSize();
            return source.Resize(new ResizeOptions
            {
                Size = new SixLabors.ImageSharp.Size(Math.Max(1, Size.Width >> level), Math.Max(1, Size.Height >> level)),
                Sampler = resampler
            });
        }

        /// <summary>
        /// Compares two <see cref="Image{TPixel}"/> and calculates the difference between their pixels.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type of the images.</typeparam>
        /// <param name="A">The first image to compare.</param>
        /// <param name="B">The second image to compare.</param>
        /// <returns>The calculated difference between the images' pixels.</returns>
        public static float Compare<TPixel>(this Image<TPixel> A, Image<TPixel> B) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (A == null || B == null || A.Size != B.Size)
            {
                return float.MaxValue;
            }

            if (object.ReferenceEquals(A, B))
            {
                return 0f;
            }

            IMemoryGroup<TPixel> pixelMemoryGroupA = A.GetPixelMemoryGroup();
            IMemoryGroup<TPixel> pixelMemoryGroupB = B.GetPixelMemoryGroup();

            float totalDifference = 0f;
            for (int i = 0; i < pixelMemoryGroupA.Count; i++)
            {
                ReadOnlySpan<TPixel> pixelSpanA = pixelMemoryGroupA[i].Span;
                ReadOnlySpan<TPixel> pixelSpanB = pixelMemoryGroupB[i].Span;

                for (int j = 0; j < pixelSpanA.Length; j++)
                {
                    Vector4 difference = Vector4.Abs(pixelSpanA[j].ToVector4() - pixelSpanB[j].ToVector4());
                    totalDifference += Vector4.Distance(Vector4.Zero, difference);
                }
            }

            return totalDifference / (A.Width * A.Height);
        }

        /// <summary>
        /// Compares an <see cref="Image{TPixel}"/> with an array of mipmaps and calculates the average difference between their pixels.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type of the images.</typeparam>
        /// <param name="source">The source image.</param>
        /// <param name="mips">The array of mipmaps to compare with.</param>
        /// <returns>The average difference between the source image and the mipmaps.</returns>
        public static float MipmapCompare<TPixel>(this Image<TPixel> source, ReadOnlySpan<Image<TPixel>> mips) where TPixel : unmanaged, IPixel<TPixel>
        {
            float diff = 0;
            Image<TPixel> mipGenerate = source.Clone(x => x.GenerateMipmap(KnownResamplers.NearestNeighbor));
            diff += mipGenerate.Compare(mips[0]);
            mipGenerate.Dispose();

            for (int i = 1; i < mips.Length; i++)
            {
                if (i == 1)
                {
                    mipGenerate = mips[0].Clone(x => x.GenerateMipmap(KnownResamplers.NearestNeighbor));
                }
                else
                {
                    mipGenerate.Mutate(x => x.GenerateMipmap(KnownResamplers.NearestNeighbor));
                }
                diff = (mipGenerate.Compare(mips[i]) + diff * i) / (1 + i);
            }
            mipGenerate.Dispose();
            return diff;
        }

        public static float MipmapCompare(this Image[] A)
            => A[0] switch
            {
                Image<I8> => MipmapCompare((Image<I8>)A[0], A.Skip(1).Take(2).Cast<Image<I8>>().ToArray()),
                Image<IA4> => MipmapCompare((Image<IA4>)A[0], A.Skip(1).Take(2).Cast<Image<IA4>>().ToArray()),
                Image<IA8> => MipmapCompare((Image<IA8>)A[0], A.Skip(1).Take(2).Cast<Image<IA8>>().ToArray()),
                Image<RGB565> => MipmapCompare((Image<RGB565>)A[0], A.Skip(1).Take(2).Cast<Image<RGB565>>().ToArray()),
                Image<RGB5A3> => MipmapCompare((Image<RGB5A3>)A[0], A.Skip(1).Take(2).Cast<Image<RGB5A3>>().ToArray()),
                Image<Rgba32> => MipmapCompare((Image<Rgba32>)A[0], A.Skip(1).Take(2).Cast<Image<Rgba32>>().ToArray()),
                Image<Rgb24> => MipmapCompare((Image<Rgb24>)A[0], A.Skip(1).Take(2).Cast<Image<Rgb24>>().ToArray()),
                _ => float.MaxValue,
            };

        /// <summary>
        /// Encodes the <paramref name="data"/> by rearranging the elements of <typeparamref name="T"/> as block.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="data">The span of data to be encoded.</param>
        /// <param name="bWidth">The width of the block.</param>
        /// <param name="bHeight">The height of the block.</param>
        public static void EncodeBlock<T>(Span<T> data, int bWidth, int bHeight) where T : unmanaged
        {
            Span<T> linebuffer = stackalloc T[bWidth * 2];
            for (int i = 0; i < bHeight; i += 2)
            {
                int pos = 0;
                Span<T> linedata = data.Slice(i * bWidth, linebuffer.Length);
                for (int bi = 0; bi < bWidth; bi += 2)
                {
                    linebuffer[pos++] = linedata[bi];
                    linebuffer[pos++] = linedata[bi + 1];
                    linebuffer[pos++] = linedata[bi + bWidth];
                    linebuffer[pos++] = linedata[bi + bWidth + 1];
                }
                linebuffer.CopyTo(linedata);
            }
        }

        /// <summary>
        /// Decodes the block <paramref name="data"/> by rearranging the <typeparamref name="T"/> to their original positions.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="data">The span of data to be decoded.</param>
        /// <param name="bWidth">The width of the block.</param>
        /// <param name="bHeight">The height of the block.</param>
        public static void DecodeBlock<T>(Span<T> data, int bWidth, int bHeight) where T : unmanaged
        {
            Span<T> linebuffer = stackalloc T[bWidth * 2];
            for (int i = 0; i < bHeight; i += 2)
            {
                int pos = 0;
                Span<T> linedata = data.Slice(i * bWidth, linebuffer.Length);
                linedata.CopyTo(linebuffer);
                for (int bi = 0; bi < bWidth; bi += 2)
                {
                    linedata[bi] = linebuffer[pos++];
                    linedata[bi + 1] = linebuffer[pos++];
                    linedata[bi + bWidth] = linebuffer[pos++];
                    linedata[bi + bWidth + 1] = linebuffer[pos++];
                }
            }
        }

        /// <summary>
        /// Converts CMPR-compressed data to DXT1 format.
        /// </summary>
        /// <param name="data">The span of data to be converted.</param>
        /// <param name="Width">The width of the image.</param>
        /// <param name="Height">The height of the image.</param>
        public static void ConvertCMPRToDXT1(Span<byte> data, int Width, int Height)
        {
            SwapCMPRColors(data);
            Span<ulong> data64 = MemoryMarshal.Cast<byte, ulong>(data);
            GXImageEX.DecodeBlock(data64, Width / 4, Height / 4);
        }

        /// <summary>
        /// Converts DXT1-compressed data to CMPR format.
        /// </summary>
        /// <param name="data">The span of data to be converted.</param>
        /// <param name="Width">The width of the image.</param>
        /// <param name="Height">The height of the image.</param>
        public static void ConvertDXT1ToCMPR(Span<byte> data, int Width, int Height)
        {
            SwapCMPRColors(data);
            Span<ulong> data64 = MemoryMarshal.Cast<byte, ulong>(data);
            GXImageEX.EncodeBlock(data64, Width / 4, Height / 4);
        }

        private static void SwapCMPRColors(Span<byte> data)
        {
            Span<ushort> data16 = MemoryMarshal.Cast<byte, ushort>(data);
            for (int i = 0; i < data16.Length; i += 4)
            {
                //swap colors
                data16[i] = BitConverterX.Swap(data16[i]);
                data16[i + 1] = BitConverterX.Swap(data16[i + 1]);
                //swap pixel sequence
                data[(i + 2) * 2 + 0] = BitConverterX.SwapAlternateBits(BitConverterX.Swap(data[(i + 2) * 2 + 0]));
                data[(i + 2) * 2 + 1] = BitConverterX.SwapAlternateBits(BitConverterX.Swap(data[(i + 2) * 2 + 1]));
                data[(i + 2) * 2 + 2] = BitConverterX.SwapAlternateBits(BitConverterX.Swap(data[(i + 2) * 2 + 2]));
                data[(i + 2) * 2 + 3] = BitConverterX.SwapAlternateBits(BitConverterX.Swap(data[(i + 2) * 2 + 3]));
            }
        }

        /// <summary>
        /// Formats pixel data to RGBA32 format using the specified color bit masks.
        /// </summary>
        /// <param name="data">The array of pixel data to convert.</param>
        /// <param name="RBitMask">The bitmask for the red color channel.</param>
        /// <param name="GBitMask">The bitmask for the green color channel.</param>
        /// <param name="BBitMask">The bitmask for the blue color channel.</param>
        /// <param name="ABitMask">The bitmask for the alpha channel.</param>
        public static void ToRGBA32(Span<byte> data, uint RBitMask, uint GBitMask, uint BBitMask, uint ABitMask)
        {
            Span<uint> pixel = MemoryMarshal.Cast<byte, uint>(data);
            int RShift = GetShiftCount(RBitMask);
            int GShift = GetShiftCount(GBitMask);
            int BShift = GetShiftCount(BBitMask);
            int AShift = GetShiftCount(ABitMask);

            for (int i = 0; i < pixel.Length; i++)
            {
                (data[i * 4], data[i * 4 + 1], data[i * 4 + 2], data[i * 4 + 3]) =
                    (
                    (byte)((pixel[i] & RBitMask) >> RShift),
                    (byte)((pixel[i] & GBitMask) >> GShift),
                    (byte)((pixel[i] & BBitMask) >> BShift),
                    (byte)((pixel[i] & ABitMask) >> AShift)
                    );
            }
        }

        private static int GetShiftCount(uint bitmask)
        {
            int count = 0;
            while ((bitmask & 1) == 0)
            {
                bitmask >>= 1;
                count++;
            }
            return count;
        }
    }
}
