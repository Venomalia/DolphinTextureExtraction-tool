using AuroraLib.Compression;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;

namespace DolphinTextureExtraction.Scans
{
    public class Compress : ScanBase
    {
        private readonly Type algorithm;

        public double CompressionRate { get; private set; } = 0;

        public static ScanResults StartScan(string meindirectory, string savedirectory, Type Algorithm, ScanOptions options)
            => StartScan_Async(meindirectory, savedirectory, Algorithm, options).Result;

        public static async Task<ScanResults> StartScan_Async(string meindirectory, string savedirectory, Type Algorithm, ScanOptions options)
        {
            Compress Extractor = new(meindirectory, savedirectory, Algorithm, options);
            return await Task.Run(() => Extractor.StartScan());
        }

        internal Compress(string scanDirectory, string saveDirectory, Type Algorithm, ScanOptions options = null) : base(scanDirectory, saveDirectory, options)
        {
            algorithm = Algorithm;
        }

        protected override void Scan(ScanObjekt so)
        {
            ICompression algo = (ICompression)Activator.CreateInstance(algorithm);
            using FileStream destination = new(GetFullSaveDirectory(so.SubPath.ToString()), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
            algo.Compress(so.Stream.ToArray(), destination);
            AddResult(so, destination);
        }

        private void AddResult(ScanObjekt so, Stream destination)
        {
            double compressionRate = ((double)destination.Length / so.Stream.Length - 1) * 100;
            lock (Result)
            {
                CompressionRate = (CompressionRate + compressionRate) / 2;
            }
            Log.Write(FileAction.Compress, string.Concat(so.SubPath, so.Extension) + $" ~{PathX.AddSizeSuffix(destination.Length, 2)}", $"Algo:{algorithm.Name} Rate:{compressionRate:+#.##;-#.##;0.00}%");
        }
    }
}
