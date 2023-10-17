using AuroraLib.Texture;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DolphinTextureExtraction.Scans
{
    public class CombineRGBA : ScanBase
    {
        List<int> _hash = new();

        public static ScanResults StartScan(string meindirectory, string savedirectory, ScanOptions options, string logDirectory = null)
            => StartScan_Async(meindirectory, savedirectory, options, logDirectory).Result;

        public static async Task<ScanResults> StartScan_Async(string meindirectory, string savedirectory, ScanOptions options, string logDirectory = null)
        {
            CombineRGBA Extractor = new(meindirectory, savedirectory, options, logDirectory);
            return await Extractor.StartScan_Async();
        }

        public CombineRGBA(in string scanDirectory, in string saveDirectory, ScanOptions options, string logDirectory = null) : base(scanDirectory, saveDirectory, options, logDirectory)
        {
        }

        protected override void Scan(ScanObjekt so)
        {
            try
            {
                //in case it's not a supported image format.
                if (!so.Extension.Contains(".png", StringComparison.InvariantCultureIgnoreCase) && !so.Extension.Contains(".tga", StringComparison.InvariantCultureIgnoreCase) && !so.Extension.Contains(".tiff", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                ReadOnlySpan<char> name = Path.GetFileName(so.SubPath);
                if (name.Length > 28 && name[..4].SequenceEqual("tex1") && DolphinTextureHashInfo.TryParse(name.ToString(), out DolphinTextureHashInfo dolphinHash))
                {
                    if (!dolphinHash.Format.IsPaletteFormat() || !AddHashIfNeeded(dolphinHash))
                    {
                        return;
                    }

                    using Image<Rgba32> image = Image.Load<Rgba32>(so.Stream);

                    if (ImageHelper.IsGrayscale(image) && so.Stream is FileStream file)
                    {
                        string directory = Path.GetDirectoryName(file.Name);
                        string searchPattern = Path.GetFileNameWithoutExtension(file.Name).Replace($"{dolphinHash.TlutHash:x16}", "*");

                        string[] match = Directory.GetFiles(directory, searchPattern + ".*");
                        if (match.Length == 2)
                        {
                            string fullPaht2 = match[0] == file.Name ? match[1] : match[0];
                            DolphinTextureHashInfo.TryParse(Path.GetFileName(fullPaht2), out DolphinTextureHashInfo dolphinHash2);
                            using Image<Rgba32> image2 = Image.Load<Rgba32>(fullPaht2);
                            if (image.Size != image2.Size)
                            {
                                image2.Mutate(x => x.Resize(image.Size));
                            }
                            int trend = AnalyzeImagePairs(image, image2);

                            SplitTextureHashInfo hashRGBA;
                            ReadOnlySpan<char> subPath = so.SubPath;

                            if (trend < 100)
                            {
                                if (trend > 0)
                                    subPath = Path.Join("~Alternatives", subPath);
                                hashRGBA = new(dolphinHash2, dolphinHash);
                                using Image<Rgba32> imageRGBA = CombineImagePairs(image2, image);
                                Save(imageRGBA, subPath, hashRGBA, trend);
                            }
                            if (trend > -100)
                            {
                                if (trend <= 0)
                                    subPath = Path.Join("~Alternatives", subPath);
                                hashRGBA = new(dolphinHash, dolphinHash2);
                                using Image<Rgba32> imageRGBA = CombineImagePairs(image, image2);
                                Save(imageRGBA, subPath, hashRGBA, trend);
                            }
                        }
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

        private void Save(Image image, ReadOnlySpan<char> subSavePath, SplitTextureHashInfo hash, int trend)
        {
            Stream file;
            string fileName = hash.Build() + ".png";

            //In case of a DryRun we do not save the file.
            if (Option.DryRun)
            {
                file = new MemoryPoolStream(512);
            }
            else
            {
                string saveDirectory = GetFullSaveDirectory(Path.GetDirectoryName(subSavePath));
                string savePath = Path.Combine(saveDirectory, fileName);
                Directory.CreateDirectory(saveDirectory);
                file = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            }

            image.SaveAsPng(file);
            file.Dispose();

            fileName = Path.Join(Path.GetDirectoryName(subSavePath), fileName);
            Log.WriteNotification(AuroraLib.Common.NotificationType.Info, $"\"{fileName}\" RGBA tendency {trend}.");
            Option.ListPrintAction?.Invoke(Result, "Combined", fileName, $"RGBA tendency {trend}.");

        }

        private static Image<Rgba32> CombineImagePairs(Image<Rgba32> imageRG, Image<Rgba32> imageBA)
        {
            int width = imageRG.Width;
            int height = imageRG.Height;

            Image<Rgba32> image = new(width, height);

            var memRG = imageRG.GetPixelMemoryGroup();
            var memBA = imageBA.GetPixelMemoryGroup();
            var mem = image.GetPixelMemoryGroup();

            for (int i = 0; i < memRG.Count; i++)
            {
                Span<Rgba32> pRG = memRG[i].Span;
                Span<Rgba32> pBA = memBA[i].Span;
                Span<Rgba32> p = mem[i].Span;

                for (int j = 0; j < pRG.Length; j++)
                {
                    p[j] = new(pRG[j].R, pRG[j].A, pBA[j].R, pBA[j].A);
                }
            }

            return image;
        }

        private static int AnalyzeImagePairs(Image<Rgba32> image1, Image<Rgba32> image2)
        {
            int trend = 0;
            var mem1 = image1.GetPixelMemoryGroup();
            var mem2 = image2.GetPixelMemoryGroup();

            // Last A value for image1 & image2.
            La16 lastP1 = default;
            La16 lastP2 = default;

            // Last trend for A value for image1 & image2.
            int lastTrendA1 = 0;
            int lastTrendA2 = 0;

            for (int i = 0; i < mem1.Count; i++)
            {
                // Retrieve pixel data for both images.
                Span<Rgba32> pd1 = mem1[i].Span;
                Span<Rgba32> pd2 = mem2[i].Span;

                for (int j = 0; j < pd1.Length; j++)
                {
                    La16 p1 = new(pd1[j].R, pd1[j].A);
                    La16 p2 = new(pd2[j].R, pd2[j].A);

                    // Try to detect the behavior of fully trasparent pixels.
                    if (p1.L == p2.L && p1.L == p2.A)
                    {
                        trend -= p1.A == 0 ? 30 : 10;
                    }
                    if (p1.L == p2.L && p1.L == p1.A)
                    {
                        trend += p2.A == 0 ? 30 : 10;
                    }

                    // If only one value does not change it is probably the alpha channel.
                    if (lastP1.L == p1.L && lastP2.L == p2.L)
                    {
                        if (lastP1.A == p1.A && lastP2.A != p2.A)
                        {
                            trend += 5;
                        }
                        else if (lastP2.A == p2.A && lastP1.A != p1.A)
                        {
                            trend -= 5;
                        }
                    }

                    trend += AnalyzeAlphaTrend(p1, ref lastTrendA1, ref lastP1) ? -1 : 3;
                    trend += AnalyzeAlphaTrend(p2, ref lastTrendA2, ref lastP2) ? 1 : -3;
                }
            }
            return trend;

            static bool AnalyzeAlphaTrend(La16 p, ref int trend, ref La16 lp)
            {
                // Determine the trend for A value.
                int currentTrend = lp.A < p.A ? 1 : (lp.A > p.A ? -1 : 0);
                // Analyze the trend of the alpha channel.
                bool result = (trend == 0 || currentTrend == trend);
                // Update last values.
                trend = currentTrend;
                lp = p;
                return result;
            }
        }

        private bool AddHashIfNeeded(DolphinTextureHashInfo dolphinHash)
        {
            int hash = dolphinHash.Hash.GetHashCode() + (dolphinHash.HasMips ? 1 : 0) + dolphinHash.Mipmap;
            lock (_hash)
            {
                if (_hash.Contains(hash))
                    return false;

                _hash.Add(hash);
                return true;
            }
        }

    }
}
