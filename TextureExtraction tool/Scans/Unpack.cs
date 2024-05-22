using AuroraLib.Common;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;

namespace DolphinTextureExtraction.Scans
{
    public class Unpack : ScanBase
    {
        public Unpack(string scanDirectory, string saveDirectory, ScanOptions options = null, string logDirectory = null) : base(scanDirectory, saveDirectory, options, logDirectory)
        {
        }

        public static ScanResults StartScan(string meindirectory, string savedirectory, ScanOptions options, string logDirectory = null)
            => StartScan_Async(meindirectory, savedirectory, options, logDirectory).Result;

        public static async Task<ScanResults> StartScan_Async(string meindirectory, string savedirectory, ScanOptions options, string logDirectory = null)
        {
            Unpack Extractor = new(meindirectory, savedirectory, options, logDirectory);
            return await Extractor.StartScan_Async();
        }

        protected override void Scan(ScanObjekt so)
        {
            try
            {
                LogScanObjekt(so);
                if (Option.Deep != 0 && so.Deep >= Option.Deep)
                {
                    Save(so.File.Data, so.File.GetFullPath(), so.Format);
                    return;
                }

                switch (so.Format.Typ)
                {
                    case FormatType.Unknown:
                        if (Option.Force)
                        {
                            if (TryForce(so))
                                break;
                        }
                        else
                        {
                            if (TryExtract(so))
                                break;
                        }


                        LogResultUnknown(so);
                        if (so.Deep != 0)
                            Save(so);
                        break;
                    case FormatType.Iso:
                    case FormatType.Archive:
                        if (!TryExtract(so))
                        {
                            Log.Write(FileAction.Unsupported, so.File.GetFullPath() + $" ~{PathX.AddSizeSuffix(so.File.Data.Length, 2)}", $"Description: {so.Format.GetFullDescription()}");
                            Save(so);
                        }
                        break;
                    default:
                        if (so.Deep != 0)
                            Save(so);
                        break;
                }
            }
            catch (Exception)
            {
                // In case of a processing error, we would still like to save new files.
                if (so.Deep != 0)
                    Save(so);
                throw;
            }
        }
    }
}
