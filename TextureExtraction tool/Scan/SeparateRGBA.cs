using AuroraLib.Texture;
using DolphinTextureExtraction.Scans;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DolphinTextureExtraction.Scan
{
    public class SeparateRGBA : ScanBase
    {

        public static ScanResults StartScan(string meindirectory, string savedirectory, ScanOptions options, string logDirectory = null)
            => StartScan_Async(meindirectory, savedirectory, options, logDirectory).Result;

        public static async Task<ScanResults> StartScan_Async(string meindirectory, string savedirectory, ScanOptions options, string logDirectory = null)
        {
            SeparateRGBA Extractor = new(meindirectory, savedirectory, options, logDirectory);
            return await Extractor.StartScan_Async();
        }

        internal SeparateRGBA(in string scanDirectory, in string saveDirectory, ScanOptions options, string logDirectory = null) : base(scanDirectory, saveDirectory, options, logDirectory)
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
            Save(so);
        }
    }
}
