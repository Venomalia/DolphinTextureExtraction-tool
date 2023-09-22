using AuroraLib.Archives;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;

namespace DolphinTextureExtraction.Scans
{
    public class Cutter : ScanBase
    {
        private List<byte[]> Pattern;

        internal Cutter(string scanDirectory, string saveDirectory, ScanOptions options = null) : base(scanDirectory, saveDirectory, options) { }

        internal Cutter(string scanDirectory, string saveDirectory, List<byte[]> pattern, ScanOptions options = null) : base(scanDirectory, saveDirectory, options)
        {
            Pattern = pattern;
        }

        public static ScanResults StartScan(string meindirectory, string savedirectory, List<byte[]> pattern, ScanOptions options)
            => StartScan_Async(meindirectory, savedirectory, pattern, options).Result;

        public static async Task<ScanResults> StartScan_Async(string meindirectory, string savedirectory, List<byte[]> pattern, ScanOptions options)
        {
            Cutter Extractor = new Cutter(meindirectory, savedirectory, pattern, options);
            return await Task.Run(() => Extractor.StartScan());
        }
        protected override void Scan(ScanObjekt so)
        {
            if (so.Deep != 0)
                Save(so.Stream, so.SubPath.ToString(), so.Format);

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
                            Save(((ArchiveFile)item.Value).FileData, so.SubPath.ToString(), so.Format);
                    else
                        Scan(archive, so.SubPath, so.Deep + 1);
                }
            }
            catch (Exception t)
            {
                Log.WriteEX(t, string.Concat(so.SubPath, so.Extension));
            }
        }
    }
}
