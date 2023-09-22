using AuroraLib.Common;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;

namespace DolphinTextureExtraction.Scans
{
    public class Unpack : ScanBase
    {
        public Unpack(string scanDirectory, string saveDirectory, ScanOptions options = null) : base(scanDirectory, saveDirectory, options)
        {
        }

        public static ScanResults StartScan(string meindirectory, string savedirectory, ScanOptions options)
            => StartScan_Async(meindirectory, savedirectory, options).Result;

        public static async Task<ScanResults> StartScan_Async(string meindirectory, string savedirectory, ScanOptions options)
        {
            Unpack Extractor = new(meindirectory, savedirectory, options);
            return Extractor.StartScan_Async().Result;
        }

        protected override void Scan(ScanObjekt so)
        {
            try
            {
                if (Option.Deep != 0 && so.Deep >= Option.Deep)
                {
                    Save(so.Stream, so.SubPath.ToString(), so.Format);
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


                        AddResultUnknown(so.Stream, so.Format, string.Concat(so.SubPath, so.Extension));
                        if (so.Deep != 0)
                            Save(so.Stream, so.SubPath.ToString(), so.Format);
                        break;
                    case FormatType.Rom:
                    case FormatType.Archive:
                        if (!TryExtract(so))
                        {
                            Log.Write(FileAction.Unsupported, string.Concat(so.SubPath, so.Extension) + $" ~{PathX.AddSizeSuffix(so.Stream.Length, 2)}", $"Description: {so.Format.GetFullDescription()}");
                            Save(so.Stream, so.SubPath.ToString(), so.Format);
                        }
                        break;
                    default:
                        if (so.Deep != 0)
                            Save(so.Stream, so.SubPath.ToString(), so.Format);
                        break;
                }
            }
            catch (Exception t)
            {
                Log.WriteEX(t, string.Concat(so.SubPath, so.Extension));
                if (so.Deep != 0)
                    Save(so.Stream, so.SubPath.ToString(), so.Format);
            }
        }
    }
}
