using AuroraLib.Archives;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;

namespace DolphinTextureExtraction.Scans
{
    public class Cutter : ScanBase
    {
        private readonly List<byte[]> Pattern;

        internal Cutter(string scanDirectory, string saveDirectory, ScanOptions options = null, string logDirectory = null) : base(scanDirectory, saveDirectory, options, logDirectory) { }

        internal Cutter(string scanDirectory, string saveDirectory, List<byte[]> pattern, ScanOptions options = null, string logDirectory = null) : base(scanDirectory, saveDirectory, options, logDirectory)
            => Pattern = pattern;

        public static ScanResults StartScan(string meindirectory, string savedirectory, List<byte[]> pattern, ScanOptions options, string logDirectory = null)
            => StartScan_Async(meindirectory, savedirectory, pattern, options, logDirectory).Result;

        public static async Task<ScanResults> StartScan_Async(string meindirectory, string savedirectory, List<byte[]> pattern, ScanOptions options, string logDirectory = null)
        {
            Cutter Extractor = new(meindirectory, savedirectory, pattern, options, logDirectory);
            return await Extractor.StartScan_Async();
        }
        protected override void Scan(ScanObjekt so)
        {

            if (so.Deep != 0)
            {
                Save(so);
            }
            else
            {
                try
                {
                    Archive archive;
                    if (Pattern == null)
                        archive = new DataCutter(so.Stream);
                    else
                        archive = new DataCutter(so.Stream, Pattern);

                    if (archive.Root.Count > 0)
                    {
                        if (archive.Root.Count == 1)
                            foreach (var item in archive.Root.Items)
                                Save(((ArchiveFile)item.Value).FileData, so.SubPath, so.Format);
                        else
                            Scan(archive, so.SubPath, so.Deep + 1);
                    }
                }
                catch (Exception t)
                {
                    Log.WriteEX(t, so.GetFullSubPath());
                }
            }
        }
    }
}
