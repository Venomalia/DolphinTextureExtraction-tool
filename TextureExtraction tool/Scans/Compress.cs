using AuroraLib.Compression;
using AuroraLib.Compression.Interfaces;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;

namespace DolphinTextureExtraction.Scans
{
    public class Compress : ScanBase
    {
        private readonly Type algorithm;

        public double CompressionRate { get; private set; } = 0;

        public static ScanResults StartScan(string meindirectory, string savedirectory, Type Algorithm, ScanOptions options, string logDirectory = null)
            => StartScan_Async(meindirectory, savedirectory, Algorithm, options, logDirectory).Result;

        public static async Task<ScanResults> StartScan_Async(string meindirectory, string savedirectory, Type Algorithm, ScanOptions options, string logDirectory = null)
        {
            Compress Extractor = new(meindirectory, savedirectory, Algorithm, options, logDirectory);
            return await Extractor.StartScan_Async();
        }

        internal Compress(string scanDirectory, string saveDirectory, Type Algorithm, ScanOptions options = null, string logDirectory = null) : base(scanDirectory, saveDirectory, options, logDirectory)
            => algorithm = Algorithm;

        protected override void Scan(ScanObjekt so)
        {
            ICompressionAlgorithm algo = (ICompressionAlgorithm)Activator.CreateInstance(algorithm);
            using FileStream destination = new(GetFullSaveDirectory(so.File.GetFullPath()), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
            algo.Compress(so.File.Data.ToArray(), destination);
            AddResult(so, destination);
        }

        private void AddResult(ScanObjekt so, Stream destination)
        {
            double compressionRate = ((double)destination.Length / so.File.Data.Length - 1) * 100;
            lock (Result)
            {
                CompressionRate = (CompressionRate + compressionRate) / 2;
            }
            Log.Write(FileAction.Compress, so.File.GetFullPath() + $" ~{PathX.AddSizeSuffix(destination.Length, 2)}", $"Algo:{algorithm.Name} Rate:{compressionRate:+#.##;-#.##;0.00}%");
        }
    }
}
