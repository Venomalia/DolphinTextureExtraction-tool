using AuroraLib.Core.Buffers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture
{
    public interface IBlock<TPixel> : IBlock where TPixel : unmanaged, IPixel<TPixel>
    {
        /// <summary>
        /// Decodes a block of encoded data into the corresponding <typeparamref name="TPixel"/> values.
        /// </summary>
        /// <param name="data">The encoded data to decode.</param>
        /// <param name="pixels">The span to store the decoded pixel values.</param>
        void DecodeBlock(ReadOnlySpan<byte> data, Span<TPixel> pixels);

        /// <summary>
        /// Encodes a block of <typeparamref name="TPixel"/> values into the corresponding encoded data.
        /// </summary>
        /// <param name="pixels">The pixel values to encode.</param>
        /// <param name="data">The span to store the encoded data.</param>
        void EncodeBlock(Span<TPixel> pixels, Span<byte> data);

        public byte[] EncodeImage(Image<TPixel> image)
        {
            using SpanBuffer<TPixel> pixel = new(image.Width * image.Height);
            image.CopyPixelDataTo(pixel);
            return EncodePixel(pixel, image.Width, image.Height);
        }

        /// <summary>
        /// Decodes the given <paramref name="data"/> into an <see cref="Image{TPixel}"/> with the specified <paramref name="width"/> and <paramref name="height"/>.
        /// </summary>
        /// <param name="data">The data to decode.</param>
        /// <param name="width">The width of the pixel array.</param>
        /// <param name="height">The height of the pixel array.</param>
        /// <returns>An array of decoded pixels.</returns>
        public Image<TPixel> DecodeImage(ReadOnlySpan<byte> data, int width, int height)
        {
            Image<TPixel> image = new(width, height);

            // Get direct access to the pixels
            MemoryGroupEnumerator<TPixel> pixelGroup = image.GetPixelMemoryGroup().GetEnumerator();
            pixelGroup.MoveNext();
            Span<TPixel> pixel = pixelGroup.Current.Span;

            DecodeImage(data, pixel, width, height);
            return image;
        }

        private void DecodeImage(ReadOnlySpan<byte> data, Span<TPixel> pixel, int width, int height)
        {
            int BPB = BytePerBlock;
            Span<TPixel> blockPixel = stackalloc TPixel[PixelPerBlock];

            int block = 0;

            for (int BlockY = 0; BlockY < height; BlockY += BlockHeight)
            {
                for (int BlockX = 0; BlockX < width; BlockX += BlockWidth)
                {
                    DecodeBlock(data.Slice(block++ * BPB, BPB), blockPixel);

                    for (int i = 0; i < blockPixel.Length; i++)
                    {
                        int pixelX = BlockX + (i % BlockWidth);
                        int pixelY = BlockY + (i / BlockWidth);

                        if (pixelX >= width || pixelY >= height)
                            continue;

                        pixel[(pixelY * width) + pixelX] = blockPixel[i];
                    }
                }
            }
        }

        /// <summary>
        /// Encodes the given pixel data with the specified <paramref name="width"/> and <paramref name="height"/> into a byte array.
        /// </summary>
        /// <param name="pixel">The pixel data to encode.</param>
        /// <param name="width">The width of the pixel data.</param>
        /// <param name="height">The height of the pixel data.</param>
        /// <returns>A byte array containing the encoded data.</returns>
        public byte[] EncodePixel(ReadOnlySpan<TPixel> pixel, int width, int height)
        {
            byte[] encodedData = new byte[CalculatedDataSize(width, height)];
            EncodePixel(pixel, encodedData, width, height);
            return encodedData;
        }

        private void EncodePixel(ReadOnlySpan<TPixel> pixel, Span<byte> data, int width, int height)
        {
            int BPB = BytePerBlock;
            Span<TPixel> blockPixel = stackalloc TPixel[PixelPerBlock];

            int block = 0;

            for (int BlockY = 0; BlockY < height; BlockY += BlockHeight)
            {
                for (int BlockX = 0; BlockX < width; BlockX += BlockWidth)
                {
                    for (int i = 0; i < blockPixel.Length; i++)
                    {
                        int pixelX = BlockX + (i % BlockWidth);
                        int pixelY = BlockY + (i / BlockWidth);

                        if (pixelX >= width || pixelY >= height)
                        {
                            //for pixels outside of the image
                            blockPixel[i].FromVector4(Vector4.Zero);
                        }
                        else
                        {
                            blockPixel[i] = pixel[(pixelY * width) + pixelX];
                        }
                    }

                    EncodeBlock(blockPixel, data.Slice(block++ * BPB, BPB));
                }
            }
        }

        public byte[] EncodePixel(ReadOnlySpan<byte> data, int width, int height)
        {
            ReadOnlySpan<TPixel> pixel = MemoryMarshal.Cast<byte, TPixel>(data);
            return EncodePixel(pixel, width, height);
        }
    }
}
