using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace AuroraLib.Common
{
    public static class BitmapEx
    {
        /// <summary>
        /// Returns a span of bytes representing the pixel data of the bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap to convert.</param>
        /// <returns>A span of bytes representing the pixel data of the bitmap.</returns>
        public static unsafe Span<byte> AsSpan(this Bitmap bitmap)
        {
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            try
            {
                byte* pBytes = (byte*)bmpData.Scan0.ToPointer();
                int size = bmpData.Stride * bmpData.Height;
                return new Span<byte>(pBytes, size);
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
        }
        /// <summary>
        /// Converts a span of bytes to a Bitmap object with the specified width, height, and pixel format.
        /// </summary>
        /// <param name="bytes">The span of bytes containing the pixel data.</param>
        /// <param name="width">The width of the bitmap, in pixels.</param>
        /// <param name="height">The height of the bitmap, in pixels.</param>
        /// <param name="pixelFormat">The pixel format of the bitmap.</param>
        /// <returns>A new Bitmap object with the pixel data from the input span.</returns>
        public static unsafe Bitmap ToBitmap(ReadOnlySpan<byte> buffer, int width, int height, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
        {
            Bitmap bitmap = new Bitmap(width, height, pixelFormat);
            BitmapData imgData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            try
            {
                Span<byte> imgDataBytes = new(imgData.Scan0.ToPointer(), imgData.Stride * imgData.Height);
                buffer.CopyTo(imgDataBytes);
            }
            finally
            {
                bitmap.UnlockBits(imgData);
            }

            return bitmap;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="InterpolationMode"></param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height, InterpolationMode InterpolationMode = InterpolationMode.HighQualityBicubic)
        {
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Lerp two Bitmap images together with a specified blending factor.
        /// </summary>
        /// <param name="imageA">The first Bitmap image to multiply.</param>
        /// <param name="imageB">The second Bitmap image to multiply.</param>
        /// <param name="t">The blending factor, between 0 and 1, with 0 representing imageA and 1 representing imageB.</param>
        /// <returns>A new Bitmap image that is the result of multiplying the two input images together.</returns>
        public static Bitmap Lerp(Bitmap imageA, Bitmap imageB, float t)
        {
            Func<byte, byte, byte> multiplyFunc = (a, b) => (byte)MathEx.Lerp(a, b, t.Clamp(0, 1));
            return Blend(imageA, imageB, multiplyFunc);
        }

        /// <summary>
        /// Additive two Bitmap images together with a specified blending factor.
        /// </summary>
        /// <param name="imageA">The first Bitmap image to multiply.</param>
        /// <param name="imageB">The second Bitmap image to multiply.</param>
        /// <param name="t">The blending factor, between 0 and 1, with 0 representing imageA and 1 representing imageB.</param>
        /// <returns>A new Bitmap image that is the result of multiplying the two input images together.</returns>
        public static Bitmap Additive(Bitmap imageA, Bitmap imageB, float t)
        {
            Func<byte, byte, byte> multiplyFunc = (a, b) => (byte)Math.Min(255, a + t * b);
            return Blend(imageA, imageB, multiplyFunc);
        }


        /// <summary>
        /// Subtraktive two Bitmap images together with a specified blending factor.
        /// </summary>
        /// <param name="imageA">The first Bitmap image to multiply.</param>
        /// <param name="imageB">The second Bitmap image to multiply.</param>
        /// <param name="t">The blending factor, between 0 and 1, with 0 representing imageA and 1 representing imageB.</param>
        /// <returns>A new Bitmap image that is the result of multiplying the two input images together.</returns>
        public static Bitmap Subtraktive(Bitmap imageA, Bitmap imageB, float t)
        {
            Func<byte, byte, byte> multiplyFunc = (a, b) => (byte)Math.Max(255, a + t * b);
            return Blend(imageA, imageB, multiplyFunc);
        }

        /// <summary>
        /// Multiplies two Bitmap images together with a specified blending factor.
        /// </summary>
        /// <param name="imageA">The first Bitmap image to multiply.</param>
        /// <param name="imageB">The second Bitmap image to multiply.</param>
        /// <param name="t">The blending factor, between 0 and 1, with 0 representing imageA and 1 representing imageB.</param>
        /// <returns>A new Bitmap image that is the result of multiplying the two input images together.</returns>
        public static Bitmap Multiply(Bitmap imageA, Bitmap imageB, float t)
        {
            Func<byte, byte, byte> multiplyFunc = (a, b) => (byte)(a * (1 - t) + b * t);
            return Blend(imageA, imageB, multiplyFunc);
        }

        /// <summary>
        /// Blends two bitmaps using a given blending function.
        /// </summary>
        /// <param name="bmp1">The first bitmap to blend.</param>
        /// <param name="bmp2">The second bitmap to blend.</param>
        /// <param name="blendFunc">The function to use to blend the two images.</param>
        /// <returns>The blended bitmap.</returns>
        /// <exception cref="ArgumentException">Thrown if the two bitmaps have different sizes or pixel formats.</exception>
        public static Bitmap Blend(Bitmap bmp1, Bitmap bmp2, Func<byte, byte, byte> blendFunc)
        {
            if (bmp1.Size != bmp2.Size)
                throw new ArgumentException("Bitmaps must have the same dimensions.");

            if (bmp1.PixelFormat != bmp2.PixelFormat)
                throw new ArgumentException("Bitmaps must have the same pixel format.");

            ReadOnlySpan<byte> imageAbytes = bmp1.AsSpan();
            ReadOnlySpan<byte> imageBbytes = bmp2.AsSpan();
            Span<byte> resultbytes = new byte[imageAbytes.Length];

            for (int i = 0; i < imageAbytes.Length; i++)
            {
                resultbytes[i] = blendFunc(imageAbytes[i], imageBbytes[i]);
            }
            return ToBitmap(resultbytes, bmp1.Width, bmp1.Height, bmp1.PixelFormat);
        }

        /// <summary>
        /// Compares two Bitmap objects for equality.
        /// </summary>
        /// <param name="bmp1">The first Bitmap to compare.</param>
        /// <param name="bmp2">The second Bitmap to compare.</param>
        /// <returns>True if the Bitmaps are equal, false otherwise.</returns>
        public static unsafe bool Equal(this Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null || !bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
                return false;
            if (object.Equals(bmp1, bmp2))
                return true;

            int width = bmp1.Width;
            int height = bmp1.Height;
            int bytesPerPixel = Image.GetPixelFormatSize(bmp1.PixelFormat) / 8;
            int stride = width * bytesPerPixel;
            int size = stride * height;

            byte* p1 = null, p2 = null;
            BitmapData bitmapData1 = null, bitmapData2 = null;

            try
            {
                bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bmp1.PixelFormat);
                bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bmp2.PixelFormat);

                p1 = (byte*)bitmapData1.Scan0.ToPointer();
                p2 = (byte*)bitmapData2.Scan0.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < stride; x++)
                    {
                        if (p1[x] != p2[x])
                            return false;
                    }

                    p1 += bitmapData1.Stride;
                    p2 += bitmapData2.Stride;
                }

                return true;
            }
            finally
            {
                if (bitmapData1 != null)
                    bmp1.UnlockBits(bitmapData1);
                if (bitmapData2 != null)
                    bmp2.UnlockBits(bitmapData2);
            }
        }

        /// <summary>
        /// Compares the pixel values of two bitmaps and returns a float value representing their difference.
        /// </summary>
        /// <param name="bmp1">The first bitmap to compare.</param>
        /// <param name="bmp2">The second bitmap to compare.</param>
        /// <returns>A float value representing the difference between the pixel values of the two bitmaps.</returns>
        public static unsafe float Compare(this Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null || bmp1.Size != bmp2.Size || bmp1.PixelFormat != bmp2.PixelFormat)
            {
                return float.MaxValue;
            }

            if (object.ReferenceEquals(bmp1, bmp2))
            {
                return 0;
            }
            return GenericEx.Compare(bmp1.AsSpan(), bmp2.AsSpan());
        }

        /// <summary>
        /// Generates a mipmap of the given bitmap with the specified interpolation mode.
        /// </summary>
        /// <param name="source">The source bitmap.</param>
        /// <param name="interpolation">The interpolation mode to use.</param>
        /// <returns>The generated mipmap.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        public static Bitmap GenerateMipMap(this Bitmap source, InterpolationMode interpolation)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Bitmap result = new Bitmap(source.Width / 2, source.Height / 2, source.PixelFormat);

            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = interpolation;
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.PixelOffsetMode = PixelOffsetMode.Default;
                g.DrawImage(source, new Rectangle(0, 0, result.Width, result.Height));
            }

            return result;
        }

        /// <summary>
        /// Generates a mipmap level from the input bitmap by halving its dimensions
        /// </summary>
        /// <param name="source">The input bitmap</param>
        /// <returns>The generated mipmap level</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
        public static Bitmap GenerateMipMap(this Bitmap source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int dstWidth = source.Width / 2;
            int dstHeight = source.Height / 2;

            Bitmap result = source.Resized(new Rectangle(0, 0, dstWidth, dstHeight));

            return result;
        }

        /// <summary>
        /// Resizes a Bitmap to the specified destination rectangle using the lineare interpolation algorithm.
        /// This is fast and preserves the transperent color values.
        /// </summary>
        /// <param name="source">The Bitmap to resize.</param>
        /// <param name="destination">The destination rectangle.</param>
        /// <returns>The resized Bitmap.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the source Bitmap is null.</exception>
        public static Bitmap Resized(this Bitmap source, Rectangle destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Bitmap result = new Bitmap(destination.Width, destination.Height, source.PixelFormat);

            BitmapData srcData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData dstData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, result.PixelFormat);

            try
            {
                unsafe
                {
                    byte* srcScan = (byte*)srcData.Scan0.ToPointer();
                    byte* dstScan = (byte*)dstData.Scan0.ToPointer();

                    int srcStride = srcData.Stride;
                    int dstStride = dstData.Stride;

                    int pixelSize = Image.GetPixelFormatSize(result.PixelFormat) / 8;

                    for (int y = 0; y < dstData.Height; y++)
                    {
                        int srcY = y * srcData.Height / dstData.Height;
                        byte* srcRow = srcScan + (srcY * srcStride);

                        byte* dstRow = dstScan + (y * dstStride);

                        for (int x = 0; x < dstData.Width; x++)
                        {
                            int srcX = x * srcData.Width / dstData.Width;
                            byte* srcPixel = srcRow + (srcX * pixelSize);

                            byte* dstPixel = dstRow + (x * pixelSize);

                            for (int i = 0; i < pixelSize; i++)
                            {
                                dstPixel[i] = srcPixel[i];
                            }
                        }
                    }
                }
            }
            finally
            {
                source.UnlockBits(srcData);
                result.UnlockBits(dstData);
            }

            return result;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsArbitraryMipmap(this Bitmap source, params Bitmap[] mips)
            => source.MipmapCompare(mips) >= 17;

        public static float MipmapCompare(this Bitmap source, params Bitmap[] mips)
        {
            float diff = 0;
            Bitmap sourcemip = source.GenerateMipMap();
            diff += sourcemip.Compare(mips[0]);
            for (int i = 1; i < mips.Length; i++)
            {
                if (i == 1)
                {
                    sourcemip.Dispose();
                    sourcemip = mips[0].GenerateMipMap();
                }
                else
                {
                    using (Bitmap hol = sourcemip)
                    {
                        sourcemip = hol.GenerateMipMap();
                    }
                }
                diff = (sourcemip.Compare(mips[i]) + diff * i) / (1 + i);
            }
            sourcemip.Dispose();

            return diff;
        }

    }
}
