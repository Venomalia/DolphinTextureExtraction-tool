using AuroraLib.Texture;
using AuroraLib.Texture.PixelFormats;
using DolphinTextureExtraction.Scans;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace DolphinTextureExtraction.Scan
{
    public class Finalize : ScanBase
    {

        public static ScanResults StartScan(string meindirectory, string savedirectory, ScanOptions options, string logDirectory = null)
            => StartScan_Async(meindirectory, savedirectory, options, logDirectory).Result;

        public static async Task<ScanResults> StartScan_Async(string meindirectory, string savedirectory, ScanOptions options, string logDirectory = null)
        {
            Finalize Extractor = new(meindirectory, savedirectory, options, logDirectory);
            return await Extractor.StartScan_Async();
        }

        internal Finalize(in string scanDirectory, in string saveDirectory, ScanOptions options, string logDirectory = null) : base(scanDirectory, saveDirectory, options, logDirectory)
        { }

        protected override void Scan(ScanObjekt so)
        {
            ReadOnlySpan<char> name = Path.GetFileName(so.SubPath);

            //create a new folder
            string savePath = GetFullSaveDirectory(Path.GetDirectoryName(so.SubPath));
            Directory.CreateDirectory(savePath);

            //if SplitTextureHashInfo?
            if (name.Length > 32 && name[..4].SequenceEqual("RGBA") && SplitTextureHashInfo.TryParse(name.ToString(), out SplitTextureHashInfo splitHash))
            {
                // get Dolphin Texture Hashs
                DolphinTextureHashInfo hashRG, hashBA;
                (hashRG, hashBA) = splitHash.ToDolphinTextureHashInfo();

                //load the RGBA image
                using Image<Rgba32> image = Image.Load<Rgba32>(so.Stream);
                int width = image.Width;
                int height = image.Height;

                // Create two new images.
                using (Image<Rgba32> imageRG = new(width, height))
                using (Image<Rgba32> imageBA = new(width, height))
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Rgba32 pixel = image[x, y];

                            // RRRG channel
                            imageRG[x, y] = new(pixel.R, pixel.R, pixel.R, pixel.G);

                            // BBBA channel
                            imageBA[x, y] = new(pixel.B, pixel.B, pixel.B, pixel.A);
                        }
                    }

                    // Save the images for RG and BA channels
                    imageRG.Save(Path.Combine(savePath, hashRG.Build() + ".png"));
                    imageBA.Save(Path.Combine(savePath, hashBA.Build() + ".png"));
                }
                return;
            }

            //if DolphinTextureHashInfo?
            if (name.Length > 28 && name[..4].SequenceEqual("tex1") && so.Extension.SequenceEqual(".png") && DolphinTextureHashInfo.TryParse(name.ToString(), out DolphinTextureHashInfo dolphinHash))
            {
                using Image<Rgba32> image = Image.Load<Rgba32>(so.Stream);

                //Fixes color imprecision that may have occurred through AI upscaling.
                AlphaThreshold(image);
                switch (dolphinHash.Format)
                {
                    case GXImageFormat.I4:
                    case GXImageFormat.I8:
                        FixIntensityTexturs(image);
                        break;
                    case GXImageFormat.IA4:
                    case GXImageFormat.IA8:
                        FixIntensityAlphaTexturs(image);
                        break;
                }

                image.Save(Path.Combine(savePath, dolphinHash.Build() + ".png"));
                return;
            }
            Save(so);
        }

        private static void FixIntensityTexturs(Image<Rgba32> image)
        {
            I8 pixel = default;
            var mem = image.GetPixelMemoryGroup();
            for (int i = 0; i < mem.Count; i++)
            {
                Span<Rgba32> pixeldata = mem[i].Span;
                for (int j = 0; j < pixeldata.Length; j++)
                {
                    pixel.PackedValue = pixeldata[j].A;
                    pixel.ToRgba32(ref pixeldata[j]);
                }
            }
        }

        private static void FixIntensityAlphaTexturs(Image<Rgba32> image)
        {
            IA8 pixel = default;
            var mem = image.GetPixelMemoryGroup();
            for (int i = 0; i < mem.Count; i++)
            {
                Span<Rgba32> pixeldata = mem[i].Span;
                for (int j = 0; j < pixeldata.Length; j++)
                {
                    pixel.FromRgba32(pixeldata[j]);
                    pixel.ToRgba32(ref pixeldata[j]);
                }
            }
        }


        private static void AlphaThreshold(Image<Rgba32> image, int low = 3, int high = 252)
        {
            var mem = image.GetPixelMemoryGroup();
            for (int i = 0; i < mem.Count; i++)
            {
                Span<Rgba32> pixeldata = mem[i].Span;
                for (int j = 0; j < pixeldata.Length; j++)
                {
                    if (pixeldata[j].A < low)
                    {
                        pixeldata[j].A = byte.MinValue;
                    }
                    else if (pixeldata[j].A > high)
                    {
                        pixeldata[j].A = byte.MaxValue;
                    }
                }
            }
        }
    }
}
