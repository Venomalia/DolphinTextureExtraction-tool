using AuroraLib.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DolphinTextureExtraction.Scans.Helper
{
    internal static class ImageHelper
    {
        public static bool FixImageResolutionIfNeeded(Image image, Size hSize)
        {
            int dividend = image.Width % hSize.Width;
            if (dividend != 0 || image.Height % hSize.Height != 0)
            {
                int width = image.Width;
                if (dividend > hSize.Width / 2)
                {
                    width += hSize.Width - dividend;
                }
                else
                {
                    width -= dividend;
                }
                width = Math.Max(width, hSize.Width);
                int height = hSize.Height * (width / hSize.Width);

                ResizeOptions options = new() { Size = new(width, height), Mode = ResizeMode.Stretch, Sampler = KnownResamplers.Lanczos3 };
                image.Mutate(x => x.Resize(options));

                return true;
            }
            return false;
        }

        public static void FixIntensityTexturs(Image<La16> image)
        {
            var mem = image.GetPixelMemoryGroup();
            for (int i = 0; i < mem.Count; i++)
            {
                Span<La16> pixeldata = mem[i].Span;
                for (int j = 0; j < pixeldata.Length; j++)
                {
                    pixeldata[j].L = pixeldata[j].A;
                }
            }
        }

        public static bool AlphaThreshold(Image<Rgba32> image, int low = 3, int high = 252)
        {
            bool setAlpha = false;
            var mem = image.GetPixelMemoryGroup();
            for (int i = 0; i < mem.Count; i++)
            {
                Span<Rgba32> pixeldata = mem[i].Span;
                for (int j = 0; j < pixeldata.Length; j++)
                {
                    if (pixeldata[j].A < low)
                    {
                        pixeldata[j].A = byte.MinValue;
                        setAlpha = true;
                    }
                    else if (pixeldata[j].A > high && pixeldata[j].A != byte.MaxValue)
                    {
                        pixeldata[j].A = byte.MaxValue;
                        setAlpha = true;
                    }
                }
            }
            return setAlpha;
        }

        public static bool IsAlphaNeeded(Image<Rgba32> image, int min = 252)
        {
            var mem = image.GetPixelMemoryGroup();
            for (int i = 0; i < mem.Count; i++)
            {
                Span<Rgba32> pixeldata = mem[i].Span;
                for (int j = 0; j < pixeldata.Length; j++)
                {
                    if (pixeldata[j].A < min)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
