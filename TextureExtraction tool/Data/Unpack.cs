using AuroraLib.Common;

namespace DolphinTextureExtraction
{
    public class Unpack : ScanBase
    {
        public Unpack(string scanDirectory, string saveDirectory, Options options = null) : base(scanDirectory, saveDirectory, options)
        {
        }

        public static Results StartScan(string meindirectory, string savedirectory, Options options)
            => StartScan_Async(meindirectory, savedirectory, options).Result;

        public static async Task<Results> StartScan_Async(string meindirectory, string savedirectory, Options options)
        {
            Unpack Extractor = new(meindirectory, savedirectory, options);
#if DEBUG
            if (Extractor.Option.Parallel.MaxDegreeOfParallelism == 1)
            {
                Results result = Extractor.StartScan();
                return await Task.Run(() => result);
            }
#endif
            return await Task.Run(() => Extractor.StartScan());
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


                        AddResultUnknown(so.Stream, so.Format, string.Concat(so.SubPath,so.Extension));
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
