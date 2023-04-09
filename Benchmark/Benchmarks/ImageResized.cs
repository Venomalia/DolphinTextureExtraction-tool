using AuroraLib.Common;
using BenchmarkDotNet.Attributes;
using SkiaSharp;
using System.Drawing;

namespace Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class ImageResized
    {
        private const string ImagePath = "";

        [Benchmark]
        public Stream ImageSharp_Resized()
        {
            var output = new MemoryStream();

            using (var input = File.OpenRead(ImagePath))
            {
                var image = SixLabors.ImageSharp.Image.Load(input);
                var width = 2000;
                var height = 2000 * image.Height / image.Width;

                var resizeOptions = new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(width, height),
                    Mode = ResizeMode.Stretch,
                    Sampler = KnownResamplers.Bicubic
                };

                image.Mutate(x => x.Resize(resizeOptions));
                image.SaveAsPng(output);
                return output;
            }
        }

        [Benchmark]
        public Stream SkiaSharp_Resize()
        {
            var stream = new MemoryStream();
            using (var input = File.OpenRead(ImagePath))
            {
                var original = SKBitmap.Decode(input);
                var width = 2000;
                var height = 2000 * original.Height / original.Width;

                var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
                using (var image = SKImage.FromBitmap(resized))
                using (var pixmap = image.PeekPixels())
                using (var data = pixmap.Encode(SKEncodedImageFormat.Png, 100))
                {
                    data.SaveTo(stream);
                    return stream;
                }
            }
        }

        [Benchmark]
        public Stream Bitmap_Resize()
        {
            var stream = new MemoryStream();
            using (var image = System.Drawing.Image.FromFile(ImagePath))
            {
                var width = 2000;
                var height = 2000 * image.Height / image.Width;

                using (var bmp = new System.Drawing.Bitmap(width, height))
                {
                    bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        using (var wrap = new System.Drawing.Imaging.ImageAttributes())
                        {
                            wrap.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                            var rect = new System.Drawing.Rectangle(0, 0, width, height);
                            g.DrawImage(image, rect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, wrap);
                            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            return stream;
                        }
                    }
                }
            }
        }

        [Benchmark]
        public Stream Aurora_BitmapResize()
        {
            var stream = new MemoryStream();
            using (Bitmap bitmap = new(ImagePath))
            {
                var width = 2000;
                var height = 2000 * bitmap.Height / bitmap.Width;
                Bitmap rebitmap = bitmap.Resized(new System.Drawing.Rectangle(0, 0, width, height));

                rebitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream;
            }
        }
    }
}
