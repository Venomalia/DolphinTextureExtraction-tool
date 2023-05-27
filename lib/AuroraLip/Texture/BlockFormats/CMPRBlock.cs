using AuroraLib.Common;
using AuroraLib.Texture.PixelFormats;
using System.Numerics;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.BlockFormats
{
    /// <summary>
    /// Represents a block of CMPR (Compressed) pixels. Each block has a size of 8x8 pixels.
    /// The 32-byte block is organized into four 4x4 sub-blocks, each containing compressed pixel data.
    /// </summary>
    public readonly struct CMPRBlock : IBlock<Rgba32>
    {
        /// <inheritdoc />
        public int BlockWidth => 8;

        /// <inheritdoc />
        public int BlockHeight => 8;

        /// <inheritdoc />
        public int BitsPerPixel => 4;

        /// <inheritdoc />
        public void DecodeBlock(ReadOnlySpan<byte> data, Span<Rgba32> pixels)
        {
            Span<Rgba32> colors = stackalloc Rgba32[4];
            for (int subblock = 0; subblock < 4; subblock++)
            {
                int subblockX = (subblock & 1) * 4;
                int subblockY = (subblock >> 1) * 4;

                RGB565 color0 = MemoryMarshal.AsRef<RGB565>(data.Slice(subblock * 8, 2));
                RGB565 color1 = MemoryMarshal.AsRef<RGB565>(data.Slice(subblock * 8 + 2, 2));

                // Swap byte order
                color0.PackedValue = color0.PackedValue.Swap();
                color1.PackedValue = color1.PackedValue.Swap();

                GetInterpolatedColours(color0, color1, colors);

                int colorIndexes = MemoryMarshal.AsRef<int>(data.Slice(subblock * 8 + 4, 4)).Swap();

                for (int pixel = 0; pixel < 16; pixel++)
                {
                    int colorIndex = (colorIndexes >> ((15 - pixel) * 2)) & 3;
                    int x = subblockX + (pixel & 3);
                    int y = subblockY + (pixel >> 2);
                    int index = x + y * 8;
                    pixels[index] = colors[colorIndex];
                }
            }
        }

        /// <inheritdoc />
        public void EncodeBlock(Span<Rgba32> pixels, Span<byte> data)
        {
            Span<Rgba32> interpolatedColors = stackalloc Rgba32[4];
            Span<Rgba32> subblockPixels = stackalloc Rgba32[16];

            for (int subblock = 0; subblock < 4; subblock++)
            {
                int subblockX = (subblock & 1) * 4;
                int subblockY = (subblock >> 1) * 4;

                for (int row = 0; row < 4; row++)
                {
                    int sourceIndex = subblockX + (subblockY + row) * 8;

                    var sourceSlice = pixels.Slice(sourceIndex, 4);
                    var targetSlice = subblockPixels.Slice(row * 4, 4);
                    sourceSlice.CopyTo(targetSlice);
                }

                GetDominantColors(subblockPixels, out RGB565 color0, out RGB565 color1);
                GetInterpolatedColours(color0, color1, interpolatedColors);

                // Swap byte order
                color0.PackedValue = color0.PackedValue.Swap();
                color1.PackedValue = color1.PackedValue.Swap();

                MemoryMarshal.Write(data.Slice(subblock * 8, 2), ref color0);
                MemoryMarshal.Write(data.Slice(subblock * 8 + 2, 2), ref color1);

                int colorIndexes = 0;
                for (int pixel = 0; pixel < 16; pixel++)
                {
                    int x = subblockX + (pixel & 3);
                    int y = subblockY + (pixel >> 2);
                    int index = x + y * 8;
                    int colorIndex = GetColorIndex(pixels[index], interpolatedColors);
                    colorIndexes |= colorIndex << ((15 - pixel) * 2);
                }

                colorIndexes = colorIndexes.Swap();
                MemoryMarshal.Write(data.Slice(subblock * 8 + 4, 4), ref colorIndexes);
            }
        }

        /// <summary>
        /// Interpolates colors between the left and right <see cref="RGB565"/> colors and stores them in the specified color span.
        /// </summary>
        /// <param name="left">The left RGB565 color.</param>
        /// <param name="right">The right RGB565 color.</param>
        /// <param name="colors">The span to store the interpolated colors.</param>
        private static void GetInterpolatedColours(in RGB565 left, in RGB565 right, Span<Rgba32> colors)
        {
            left.ToRgba32(ref colors[0]);
            right.ToRgba32(ref colors[1]);

            //Needs Alpha Color?
            if (left.PackedValue > right.PackedValue)
            {
                colors[2] = new((byte)((2 * left.R + right.R) / 3), (byte)((2 * left.G + right.G) / 3), (byte)((2 * left.B + right.B) / 3));
                colors[3] = new((byte)((left.R + 2 * right.R) / 3), (byte)((left.G + 2 * right.G) / 3), (byte)((left.B + 2 * right.B) / 3));
            }
            else
            {
                colors[2] = new((byte)((left.R + right.R) >> 1), (byte)((left.G + right.G) >> 1), (byte)((left.B + right.B) >> 1));
                colors[3] = new((byte)((left.R + 2 * right.R) / 3), (byte)((left.G + 2 * right.G) / 3), (byte)((left.B + 2 * right.B) / 3), 0);
            }
        }

        /// <summary>
        /// Determines the two dominant colors (<paramref name="color0"/> and <paramref name="color1"/>) within a subblock of pixels.
        /// </summary>
        /// <param name="pixels">The span of pixels representing the subblock.</param>
        /// <param name="color0">Output parameter for the first dominant color (<paramref name="color0"/>).</param>
        /// <param name="color1">Output parameter for the second dominant color (<paramref name="color1"/>).</param>
        private static void GetDominantColors(Span<Rgba32> pixels, out RGB565 color0, out RGB565 color1)
        {
            bool NeedsAlphaColor = false;
            Vector3 minVec = new(float.MaxValue);
            Vector3 maxVec = new(float.MinValue);

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].A < 16)
                {
                    NeedsAlphaColor = true;
                    continue;
                }
                Vector3 pixelVec = new(pixels[i].R / 255f, pixels[i].G / 255f, pixels[i].B / 255f);
                minVec = Vector3.Min(minVec, pixelVec);
                maxVec = Vector3.Max(maxVec, pixelVec);
            }

            color0 = new((byte)(minVec.X * 255f), (byte)(minVec.Y * 255f), (byte)(minVec.Z * 255f));
            color1 = new((byte)(maxVec.X * 255f), (byte)(maxVec.Y * 255f), (byte)(maxVec.Z * 255f));

            //Needs Alpha Color?
            if ((NeedsAlphaColor && color0.PackedValue > color1.PackedValue) || (!NeedsAlphaColor && color0.PackedValue < color1.PackedValue))
            {
                (color0, color1) = (color1, color0);
            }
        }

        /// <summary>
        /// Determines the index of the closest color in a given set of colors for a given pixel.
        /// </summary>
        /// <param name="pixel">The RGBA color value of the pixel.</param>
        /// <param name="colors">The span of colors to compare against.</param>
        /// <returns>The index of the closest color in the set of colors.</returns>
        private static int GetColorIndex(in Rgba32 pixel, Span<Rgba32> colors)
        {
            float minDistance = float.MaxValue;
            int colorIndex = 0;

            //Needs Alpha Color?
            if (pixel.A < 16)
                return colors.Length - 1;

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].A == 0)
                    continue;

                float distance = CalculateColorDistance(pixel, colors[i]);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    colorIndex = i;
                }
            }

            return colorIndex;
        }

        /// <summary>
        /// Calculates the distance between two colors in RGBA color space.
        /// </summary>
        /// <param name="first">The first color.</param>
        /// <param name="second">The second color.</param>
        /// <returns>The distance between the two colors.</returns>
        private static float CalculateColorDistance(in IPixel first, in IPixel second)
        {
            Vector4 diff = first.ToVector4() - second.ToVector4();
            diff *= diff;
            return (float)Math.Sqrt(diff.X + diff.Y + diff.Z + diff.W);
        }
    }
}
