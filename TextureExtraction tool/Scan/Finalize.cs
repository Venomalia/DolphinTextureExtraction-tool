using AuroraLib.Common;
using AuroraLib.Texture;
using DolphinTextureExtraction.Scans;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Reflection.Emit;
using System.Security.Principal;

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
            //in case it's not a supported image format we just copy the file.
            if (!so.Extension.Contains(".png", StringComparison.InvariantCultureIgnoreCase) && !so.Extension.Contains(".tga", StringComparison.InvariantCultureIgnoreCase) && !so.Extension.Contains(".Tiff", StringComparison.InvariantCultureIgnoreCase))
            {
                Save(so);
                return;
            }

            ReadOnlySpan<char> name = Path.GetFileName(so.SubPath);

            //create a new folder
            string savePath = GetFullSaveDirectory(Path.GetDirectoryName(so.SubPath));
            Directory.CreateDirectory(savePath);

            //if SplitTextureHashInfo?
            if (name.Length > 32 && name[..4].SequenceEqual("RGBA") && SplitTextureHashInfo.TryParse(name.ToString(), out SplitTextureHashInfo splitHash))
            {
                Log.WriteNotification(NotificationType.Info, $"\"{name}\" Detected as RGBA combined texture.");
                // get Dolphin Texture Hashs
                DolphinTextureHashInfo hashRG, hashBA;
                (hashRG, hashBA) = splitHash.ToDolphinTextureHashInfo();

                //load the RGBA image
                using Image<Rgba32> image = Image.Load<Rgba32>(so.Stream);
                AlphaThreshold(image, 2, 253);
                int width = image.Width;
                int height = image.Height;
                PngEncoder encoder = new() { ColorType = PngColorType.GrayscaleWithAlpha };

                if (FixImageResolutionIfNeeded(image, hashRG))
                {
                    Log.WriteNotification(NotificationType.Info, $"\"{name}\" Resize {new Size(width, height)} => {image.Size}.");
                    width = image.Width;
                    height = image.Height;
                }

                // Create two new images.
                using (Image<La16> imageRG = new(width, height))
                using (Image<La16> imageBA = new(width, height))
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Rgba32 pixel = image[x, y];

                            // RG channel
                            imageRG[x, y] = new(pixel.R, pixel.G);

                            // BA channel
                            imageBA[x, y] = new(pixel.B, pixel.A);
                        }
                    }

                    // Save the RG channel image.
                    string hash = hashRG.Build();
                    string imagePath = Path.Combine(savePath, hash + ".png");
                    imageRG.SaveAsPng(imagePath, encoder);
                    Log.Write(FileAction.Extract, hash, $"RGBA Split type:RG");
                    // Save the BA channel image.
                    hash = hashBA.Build();
                    imagePath = Path.Combine(savePath, hashBA.Build() + ".png");
                    imageBA.SaveAsPng(imagePath, encoder);
                    Log.Write(FileAction.Extract, hash, $"RGBA Split type:BA");

                }
                return;
            }

            //if DolphinTextureHashInfo?
            if (name.Length > 28 && name[..4].SequenceEqual("tex1") && DolphinTextureHashInfo.TryParse(name.ToString(), out DolphinTextureHashInfo dolphinHash))
            {

                ImageInfo info = Image.Identify(so.Stream);
                so.Stream.Position = 0;
                Image image = null;
                PngEncoder encoder = null;
                try
                {
                    switch (dolphinHash.Format)
                    {
                        case GXImageFormat.I4:
                        case GXImageFormat.I8:
                            if (info.PixelType.BitsPerPixel > 16)
                            {
                                Image<La16> imageRGBA = Image.Load<La16>(so.Stream);
                                image = imageRGBA;
                                FixIntensityTexturs(imageRGBA);
                                Log.WriteNotification(NotificationType.Info, $"\"{name}\" RGB channel removed.");
                                encoder = new() { ColorType = PngColorType.GrayscaleWithAlpha };
                            }
                            break;
                        case GXImageFormat.IA4:
                        case GXImageFormat.IA8:
                            encoder = new() { ColorType = PngColorType.GrayscaleWithAlpha };
                            if (info.PixelType.BitsPerPixel > 16)
                            {
                                Log.WriteNotification(NotificationType.Info, $"\"{name}\" RGB channel merged.");
                            }
                            break;
                        case GXImageFormat.RGB565:
                            encoder = new() { ColorType = PngColorType.Rgb };
                            if (info.PixelType.BitsPerPixel > 24)
                            {
                                Log.WriteNotification(NotificationType.Info, $"\"{name}\" Alpha channel removed.");
                            }
                            break;
                        case GXImageFormat.CMPR:
                            Image<Rgba32> imageCMPR = Image.Load<Rgba32>(so.Stream);
                            image = imageCMPR;
                            if (AlphaThreshold(imageCMPR, 96, 160)) // dolphin use (128,128)
                            {
                                Log.WriteNotification(NotificationType.Info, $"\"{name}\" Set alpha Threshold.");
                            }
                            break;
                    }

                    image ??= Image.Load(so.Stream);

                    if (FixImageResolutionIfNeeded(image, dolphinHash))
                    {
                        Log.WriteNotification(NotificationType.Info, $"\"{name}\" Resize {info.Size} => {image.Size}.");
                    }

                    string imagePath = Path.Combine(savePath, dolphinHash.Build() + ".png");
                    image.SaveAsPng(imagePath, encoder);
                    return;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    image?.Dispose();
                }
            }

            Save(so);
        }

        public static bool FixImageResolutionIfNeeded(Image image, DolphinTextureHashInfo hash)
        {
            Size hSize = hash.GetImageSize();

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

        private static void FixIntensityTexturs(Image<La16> image)
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

        private static bool AlphaThreshold(Image<Rgba32> image, int low = 3, int high = 252)
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
    }
}
