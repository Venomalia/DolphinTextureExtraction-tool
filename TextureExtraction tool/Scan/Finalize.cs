using AuroraLib.Common;
using AuroraLib.Texture;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace DolphinTextureExtraction.Scans
{
    public class Finalize : ScanBase
    {

        private new FinalizeResults Result => (FinalizeResults)base.Result;

        public static ScanResults StartScan(string meindirectory, string savedirectory, ScanOptions options, string logDirectory = null)
            => StartScan_Async(meindirectory, savedirectory, options, logDirectory).Result;

        public static async Task<ScanResults> StartScan_Async(string meindirectory, string savedirectory, ScanOptions options, string logDirectory = null)
        {
            Finalize Extractor = new(meindirectory, savedirectory, options, logDirectory);
            return await Extractor.StartScan_Async();
        }

        internal Finalize(in string scanDirectory, in string saveDirectory, ScanOptions options, string logDirectory = null) : base(scanDirectory, saveDirectory, options, logDirectory)
            => base.Result = new FinalizeResults() { LogFullPath = base.Result.LogFullPath };

        protected override void Scan(ScanObjekt so)
        {
            //in case it's not a supported image format we just copy the file.
            if (!so.Extension.Contains(".png", StringComparison.InvariantCultureIgnoreCase) && !so.Extension.Contains(".tga", StringComparison.InvariantCultureIgnoreCase) && !so.Extension.Contains(".tiff", StringComparison.InvariantCultureIgnoreCase))
            {
                Save(so);
                return;
            }

            ReadOnlySpan<char> name = Path.GetFileName(so.SubPath);

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
                    using Stream fileRG = Save(image, so.SubPath, hashRG, encoder);
                    Log.Write(FileAction.Extract, hashRG.ToString(), $"RGBA Split type:RG");
                    // Save the BA channel image.
                    using Stream fileBA = Save(image, so.SubPath, hashRG, encoder);
                    Log.Write(FileAction.Extract, hashRG.ToString(), $"RGBA Split type:BA");

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
                    //textures a not quantized?
                    if (info.PixelType.BitsPerPixel > 8)
                    {
                        switch (dolphinHash.Format)
                        {
                            case GXImageFormat.I4:
                            case GXImageFormat.I8:
                                if (info.PixelType.BitsPerPixel > 16)
                                {
                                    Image<La16> imageI = Image.Load<La16>(so.Stream);
                                    image = imageI;
                                    FixIntensityTexturs(imageI);
                                    Log.WriteNotification(NotificationType.Info, $"\"{name}\" RGB channel removed.");
                                    encoder = new() { ColorType = PngColorType.GrayscaleWithAlpha };
                                    Result.AddOptimization();
                                }
                                break;
                            case GXImageFormat.IA4:
                            case GXImageFormat.IA8:
                                encoder = new() { ColorType = PngColorType.GrayscaleWithAlpha };
                                if (info.PixelType.BitsPerPixel > 16)
                                {
                                    Log.WriteNotification(NotificationType.Info, $"\"{name}\" RGB channel merged.");
                                    Result.AddOptimization();
                                }
                                break;
                            case GXImageFormat.RGB565:
                                encoder = new() { ColorType = PngColorType.Rgb };
                                if (info.PixelType.BitsPerPixel > 24)
                                {
                                    Log.WriteNotification(NotificationType.Info, $"\"{name}\" Alpha channel removed.");
                                    Result.AddOptimization();
                                }
                                break;
                            case GXImageFormat.C4:
                                IQuantizer quantizer = KnownQuantizers.Wu;
                                quantizer.Options.Dither = KnownDitherings.Stucki;
                                encoder = new() { ColorType = PngColorType.Palette, Quantizer = quantizer };
                                Log.WriteNotification(NotificationType.Info, $"\"{name}\" Quantized.");
                                Result.AddOptimization();
                                break;
                            case GXImageFormat.CMPR:

                                if (info.PixelType.BitsPerPixel > 24)
                                {
                                    Image<Rgba32> imageCMPR = Image.Load<Rgba32>(so.Stream);
                                    image = imageCMPR;

                                    if (IsAlphaNeeded(imageCMPR, 160))
                                    {
                                        if (AlphaThreshold(imageCMPR, 96, 160)) // dolphin use (128,128)
                                        {
                                            Log.WriteNotification(NotificationType.Info, $"\"{name}\" Set alpha Threshold.");
                                            Result.AddOptimization();
                                        }
                                    }
                                    else
                                    {
                                        goto case GXImageFormat.RGB565;
                                    }
                                }
                                break;
                            default:
                                if (info.PixelType.BitsPerPixel > 24)
                                {
                                    Image<Rgba32> imageRGBA = Image.Load<Rgba32>(so.Stream);
                                    image = imageRGBA;

                                    if (!IsAlphaNeeded(imageRGBA))
                                    {
                                        goto case GXImageFormat.RGB565;
                                    }
                                }
                                break;
                        }

                    }

                    image ??= Image.Load(so.Stream);

                    if (FixImageResolutionIfNeeded(image, dolphinHash))
                    {
                        Log.WriteNotification(NotificationType.Info, $"\"{name}\" Resize {info.Size} => {image.Size}.");
                        Result.AddOptimization();
                    }

                    using Stream file = Save(image, so.SubPath, dolphinHash, encoder);
                    Result.AddSize(so.Stream.Length, file.Length);
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

        private Stream Save(Image image, ReadOnlySpan<char> subSavePath, DolphinTextureHashInfo hash, PngEncoder encoder)
        {
            string saveDirectory, savePath;
            bool isDuplicat = Result.AddHashIfNeeded(hash);
            Stream file;

            //In case of a DryRun we do not save the file.
            if (Option.DryRun)
            {
                file = new MemoryPoolStream(512);
            }
            else
            {

                if (isDuplicat)
                {
                    saveDirectory = GetFullSaveDirectory(Path.Join("~Duplicates", Path.GetDirectoryName(subSavePath)));
                }
                else
                {
                    saveDirectory = GetFullSaveDirectory(Path.GetDirectoryName(subSavePath));
                }
                savePath = Path.Combine(saveDirectory, hash.Build() + ".png");
                Directory.CreateDirectory(saveDirectory);
                file = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None); ;
            }

            image.SaveAsPng(file, encoder);

            return file;
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

        private static bool IsAlphaNeeded(Image<Rgba32> image, int min = 252)
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
