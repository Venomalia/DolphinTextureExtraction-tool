using AuroraLib.Common;
using AuroraLib.Core.Extensions;
using AuroraLib.Texture;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;
using SixLabors.ImageSharp;
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
            try
            {
                //in case it's not a supported image format we just copy the file.
                if (!so.File.Extension.Contains(".png", StringComparison.InvariantCultureIgnoreCase) && !so.File.Extension.Contains(".tga", StringComparison.InvariantCultureIgnoreCase) && !so.File.Extension.Contains(".tiff", StringComparison.InvariantCultureIgnoreCase))
                {
                    Save(so);
                    return;
                }

                ReadOnlySpan<char> name = Path.GetFileName(so.File.Name);

                #region SplitTexture
                //if SplitTextureHashInfo?
                if (name.Length > 32 && (name[..4].SequenceEqual("RGBA") || name[..4].SequenceEqual("BGRA")) && SplitTextureHashInfo.TryParse(name.ToString(), out SplitTextureHashInfo splitHash))
                {
                    Log.WriteNotification(NotificationType.Info, $"\"{name}\" Detected as {splitHash.SplitType} combined texture.");
                    // get Dolphin Texture Hashs
                    DolphinTextureHashInfo hashRG, hashBA;
                    (hashRG, hashBA) = splitHash.ToDolphinTextureHashInfo();

                    //load the RGBA image
                    using Image<Rgba32> image = Image.Load<Rgba32>(so.File.Data);
                    ImageHelper.AlphaThreshold(image, 2, 253);
                    int width = image.Width;
                    int height = image.Height;
                    PngEncoder encoder = new() { ColorType = PngColorType.GrayscaleWithAlpha };

                    if (ImageHelper.FixImageResolutionIfNeeded(image, hashRG.GetImageSize()))
                    {
                        LogOptimization(so.File.GetFullPath(), $"Resize {new Size(width, height)} => {image.Size}.");
                        width = image.Width;
                        height = image.Height;
                    }

                    // Create two new images.
                    using Image<La16> imageRG = new(width, height);
                    using Image<La16> imageBA = new(width, height);
                    if (splitHash.SplitType == SplitTextureHashInfo.ChannelSplitType.RGBA)
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
                    }
                    else
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                Rgba32 pixel = image[x, y];
                                // BG channel
                                imageRG[x, y] = new(pixel.B, pixel.G);
                                // RA channel
                                imageBA[x, y] = new(pixel.R, pixel.A);
                            }
                        }
                    }

                    // Save the RG channel image.
                    string subPathRG = Path.Join(Path.GetDirectoryName(so.File.GetFullPath()), hashRG.Build() + ".png");
                    LogDuplicatIfNeeded(ref subPathRG);
                    using Stream fileRG = Save(imageRG, subPathRG, encoder);
                    LogSplit(subPathRG, $"{splitHash.SplitType} Split type:{splitHash.SplitType.ToString()[..2]}");
                    // Save the BA channel image.
                    string subPathBA = Path.Join(Path.GetDirectoryName(so.File.GetFullPath()), hashBA.Build() + ".png");
                    LogDuplicatIfNeeded(ref subPathBA);
                    using Stream fileBA = Save(imageBA, subPathBA, encoder);
                    LogSplit(subPathBA, $"{splitHash.SplitType} Split type:{splitHash.SplitType.ToString()[2..]}");
                    return;
                }
                #endregion
                else
                {
                    ImageInfo info = Image.Identify(so.File.Data);
                    so.File.Data.Position = 0;
                    Image image = null;
                    PngEncoder encoder = null;
                    string subPath = so.File.GetFullPath();

                    try
                    {
                        //if DolphinTextureHashInfo?
                        if (name.Length > 28 && name[..4].SequenceEqual("tex1") && DolphinTextureHashInfo.TryParse(name.ToString(), out DolphinTextureHashInfo dolphinHash))
                        {

                            LogDuplicatIfNeeded(ref subPath);
                            //textures a not quantized?
                            if (info.PixelType.BitsPerPixel > 8)
                            {
                                switch (dolphinHash.Format)
                                {
                                    case GXImageFormat.I4:
                                    case GXImageFormat.I8:
                                        if (info.PixelType.BitsPerPixel > 16)
                                        {
                                            Image<La16> imageI = Image.Load<La16>(so.File.Data);
                                            image = imageI;
                                            ImageHelper.FixIntensityTexturs(imageI);
                                            encoder = new() { ColorType = PngColorType.GrayscaleWithAlpha };
                                            LogOptimization(subPath, $"RGB channel removed.");
                                        }
                                        break;
                                    case GXImageFormat.IA4:
                                    case GXImageFormat.IA8:
                                        encoder = new() { ColorType = PngColorType.GrayscaleWithAlpha };
                                        if (info.PixelType.BitsPerPixel > 16)
                                            LogOptimization(subPath, encoder);
                                        break;
                                    case GXImageFormat.RGB565:
                                        encoder = new() { ColorType = PngColorType.Rgb };
                                        if (info.PixelType.BitsPerPixel > 24)
                                            LogOptimization(subPath, encoder);
                                        break;
                                    case GXImageFormat.C4:
                                        IQuantizer quantizer = KnownQuantizers.Wu;
                                        quantizer.Options.Dither = null;
                                        encoder = new() { ColorType = PngColorType.Palette, Quantizer = quantizer };
                                        LogOptimization(subPath, encoder);
                                        break;
                                    case GXImageFormat.CMPR:
                                        if (info.PixelType.BitsPerPixel > 24)
                                        {
                                            Image<Rgba32> imageCMPR = Image.Load<Rgba32>(so.File.Data);
                                            image = imageCMPR;

                                            if (ImageHelper.IsAlphaNeeded(imageCMPR, 160))
                                            {
                                                if (ImageHelper.AlphaThreshold(imageCMPR, 96, 160)) // dolphin use (128,128)
                                                {
                                                    LogOptimization(subPath, $"Set alpha Threshold.");
                                                    encoder = new() { ColorType = PngColorType.RgbWithAlpha };
                                                }
                                            }
                                            else
                                            {
                                                goto case GXImageFormat.RGB565;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }

                            }

                            if (ImageHelper.ResolutionNeedFix(info.Size, dolphinHash.GetImageSize()))
                            {
                                image ??= Image.Load(so.File.Data);
                                ImageHelper.FixImageResolutionIfNeeded(image, dolphinHash.GetImageSize());
                                LogOptimization(subPath, $"Resize {info.Size} => {image.Size}.");
                            }
                        }

                        // If the texture is quantized we can't improve it anymore.
                        if (image == null && info.PixelType.BitsPerPixel <= 24)
                        {
                            Save(so);
                            Result.AddSize(so.File.Data.Length, so.File.Data.Length);
                        }
                        else
                        {
                            image ??= Image.Load(so.File.Data);

                            if (encoder == null && info.PixelType.BitsPerPixel > 24 && image is Image<Rgba32> imageRGBA)
                            {
                                encoder = ImageHelper.GetPngEncoder(imageRGBA);
                                LogOptimization(subPath, encoder);
                            }

                            using Stream file = Save(image, subPath, encoder);
                            Result.AddSize(so.File.Data.Length, file.Length);
                        }
                    }
                    finally
                    {
                        image?.Dispose();
                    }
                }
            }
            catch (InvalidImageContentException ie)
            {
                Save(so.File.Data, GetFullSaveDirectory(Path.Join("~Corrupt", so.File.GetFullPath())));
                throw;
            }
        }

        private void LogOptimization(string filePath, PngEncoder encoder)
        {
            switch (encoder.ColorType)
            {
                case PngColorType.Grayscale:
                    LogOptimization(filePath, $"Alpha channel removed & RGB channel merged.");
                    break;
                case PngColorType.Rgb:
                    LogOptimization(filePath, $"Alpha channel removed.");
                    break;
                case PngColorType.Palette:
                    LogOptimization(filePath, $"Quantized.");
                    break;
                case PngColorType.GrayscaleWithAlpha:
                    LogOptimization(filePath, $"RGB channel merged.");
                    break;
                default:
                    break;
            }
        }

        private void LogOptimization(string filePath, string info)
        {
            Log.WriteNotification(NotificationType.Info, $"\"{filePath}\" {info}");
            Option.ListPrintAction?.Invoke(Result, "Optimized", filePath, info);
            Result.AddOptimization();
        }

        private void LogSplit(ReadOnlySpan<char> subSavePath, string info)
        {
            Log.WriteNotification(NotificationType.Info, $"\"{subSavePath}\" {info}");
            Option.ListPrintAction?.Invoke(Result, "Split", subSavePath.ToString(), info);
        }

        private bool LogDuplicatIfNeeded(ref string subPath)
        {
            int hash = Path.GetFileNameWithoutExtension(subPath.AsSpan()).GetHashCodeFast();
            bool isDuplicat = Result.AddHashIfNeeded(hash);
            if (isDuplicat)
            {
                subPath = Path.Join("~Duplicates", subPath);

                Log.WriteNotification(NotificationType.Info, $"\"{subPath}\" duplicate found.");
                Option.ListPrintAction?.Invoke(Result, "Duplicate", subPath, "duplicate found.");
            }
            return isDuplicat;
        }

        private Stream Save(Image image, ReadOnlySpan<char> subSavePath, PngEncoder encoder)
        {
            string savePath = GetFullSaveDirectory(subSavePath);
            Stream file;

            //In case of a DryRun we do not save the file.
            if (Option.DryRun)
            {
                file = new MemoryPoolStream(512);
            }
            else
            {
                string saveDirectory = Path.GetDirectoryName(savePath);
                Directory.CreateDirectory(saveDirectory);
                file = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None); ;
            }

            image.SaveAsPng(file, encoder);

            return file;
        }
    }
}
