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
                if (!so.Extension.Contains(".png", StringComparison.InvariantCultureIgnoreCase) && !so.Extension.Contains(".tga", StringComparison.InvariantCultureIgnoreCase) && !so.Extension.Contains(".tiff", StringComparison.InvariantCultureIgnoreCase))
                {
                    Save(so);
                    return;
                }

                ReadOnlySpan<char> name = Path.GetFileName(so.SubPath);

                #region SplitTexture
                //if SplitTextureHashInfo?
                if (name.Length > 32 && name[..4].SequenceEqual("RGBA") && SplitTextureHashInfo.TryParse(name.ToString(), out SplitTextureHashInfo splitHash))
                {
                    Log.WriteNotification(NotificationType.Info, $"\"{name}\" Detected as RGBA combined texture.");
                    // get Dolphin Texture Hashs
                    DolphinTextureHashInfo hashRG, hashBA;
                    (hashRG, hashBA) = splitHash.ToDolphinTextureHashInfo();

                    //load the RGBA image
                    using Image<Rgba32> image = Image.Load<Rgba32>(so.Stream);
                    ImageHelper.AlphaThreshold(image, 2, 253);
                    int width = image.Width;
                    int height = image.Height;
                    PngEncoder encoder = new() { ColorType = PngColorType.GrayscaleWithAlpha };

                    if (ImageHelper.FixImageResolutionIfNeeded(image, hashRG.GetImageSize()))
                    {
                        LogOptimization(so.GetFullSubPath(), $"Resize {new Size(width, height)} => {image.Size}.");
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
                        string subPathRG = Path.Join(Path.GetDirectoryName(so.SubPath), hashRG.Build() + ".png");
                        LogDuplicatIfNeeded(ref subPathRG);
                        using Stream fileRG = Save(imageRG, subPathRG, encoder);
                        LogSplit(subPathRG, "RGBA Split type:RG");
                        // Save the BA channel image.
                        string subPathBA = Path.Join(Path.GetDirectoryName(so.SubPath), hashRG.Build() + ".png");
                        LogDuplicatIfNeeded(ref subPathBA);
                        using Stream fileBA = Save(imageBA, subPathBA, encoder);
                        LogSplit(subPathBA, "RGBA Split type:BA");

                    }
                    return;
                }
                #endregion
                else
                {
                    Image image = null;
                    PngEncoder encoder = null;
                    string subPath = so.GetFullSubPath();

                    try
                    {
                        //if DolphinTextureHashInfo?
                        if (name.Length > 28 && name[..4].SequenceEqual("tex1") && DolphinTextureHashInfo.TryParse(name.ToString(), out DolphinTextureHashInfo dolphinHash))
                        {

                            ImageInfo info = Image.Identify(so.Stream);
                            so.Stream.Position = 0;
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
                                            Image<La16> imageI = Image.Load<La16>(so.Stream);
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
                                            Image<Rgba32> imageCMPR = Image.Load<Rgba32>(so.Stream);
                                            image = imageCMPR;

                                            if (ImageHelper.IsAlphaNeeded(imageCMPR, 160))
                                            {
                                                if (ImageHelper.AlphaThreshold(imageCMPR, 96, 160)) // dolphin use (128,128)
                                                {
                                                    LogOptimization(subPath, $"Set alpha Threshold.");
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

                            image ??= Image.Load(so.Stream);

                            if (ImageHelper.FixImageResolutionIfNeeded(image, dolphinHash.GetImageSize()))
                            {
                                LogOptimization(subPath, $"Resize {info.Size} => {image.Size}.");
                            }
                        }

                        image ??= Image.Load(so.Stream);

                        if (encoder == null && image is Image<Rgba32> imageRGBA)
                        {
                            encoder = ImageHelper.GetPngEncoder(imageRGBA);
                            LogOptimization(subPath, encoder);
                        }

                        using Stream file = Save(image, subPath, encoder);
                        Result.AddSize(so.Stream.Length, file.Length);
                    }
                    finally
                    {
                        image?.Dispose();
                    }
                }
            }
            catch (InvalidImageContentException ie)
            {
                Log.WriteEX(ie, so.GetFullSubPath());
                Save(so.Stream, string.Concat(GetFullSaveDirectory(Path.Join("~Corrupt", so.SubPath)), so.Extension));
            }
            catch (Exception e)
            {
                Log.WriteEX(e, so.GetFullSubPath());
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
