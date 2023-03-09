using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AuroraLip.Common
{
    public static class BitmapEx
    {
        /// <summary>
        /// Converts a bitmap to a byte[]
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this Bitmap bitmap)
        {
            BitmapData bmpdata = null;
            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }

        /// <summary>
        /// Creates a bitmap from a byte[].
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="pixelFormat"></param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this byte[] Buffer, int Width, int Height, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
        {
            Bitmap bitmap = new Bitmap(Width, Height, pixelFormat);
            BitmapData ImgData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(Buffer, 0, ImgData.Scan0, Buffer.Length);
            bitmap.UnlockBits(ImgData);
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
        /// Generates a lerped bitmap. Useful for mipmapping
        /// </summary>
        /// <param name="imageA"></param>
        /// <param name="imageB"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="t"></param>
        /// <param name="interpolationMode"></param>
        /// <returns></returns>
        public static Bitmap Lerp(Bitmap imageA, Bitmap imageB, int width, int height, float t, InterpolationMode interpolationMode = InterpolationMode.Bilinear)
        {
            t = t.Clamp(0, 1);
            if (imageA.PixelFormat != imageB.PixelFormat)
                throw new Exception("Pixel format mis-match!");

            Bitmap imageARescale = ResizeImage(imageA, width, height, interpolationMode);
            Bitmap imageBRescale = ResizeImage(imageB, width, height, interpolationMode);

            byte[] imageAbytes = imageARescale.ToByteArray();
            byte[] imageBbytes = imageBRescale.ToByteArray();
            byte[] resultbytes = new byte[imageAbytes.Length];

            for (int i = 0; i < imageAbytes.Length; i++)
            {
                resultbytes[i] = MathEx.Lerp(imageAbytes[i], imageBbytes[i], t);
            }
            imageARescale.Dispose();
            imageBRescale.Dispose();
            return resultbytes.ToBitmap(width, height, imageA.PixelFormat);
        }

        public static bool Compare(this Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null)
                return false;
            if (object.Equals(bmp1, bmp2))
                return true;
            if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
                return false;

            int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

            bool result = true;
            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, bmp2.PixelFormat);

            Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
            Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

            for (int n = 0; n <= bytes - 1; n++)
            {
                if (b1bytes[n] != b2bytes[n])
                {
                    result = false;
                    break;
                }
            }

            bmp1.UnlockBits(bitmapData1);
            bmp2.UnlockBits(bitmapData2);

            return result;
        }
    }
}
