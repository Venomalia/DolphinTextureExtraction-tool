using AuroraLip.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DolphinTextureExtraction_tool
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
            Unpack Extractor = new Unpack(meindirectory, savedirectory, options);
            return await Task.Run(() => Extractor.StartScan());
        }

        protected override void Scan(Stream Stream, FormatInfo Format, ReadOnlySpan<char> SubPath, int Deep, in string OExtension = "")
        {
#if !DEBUG
            try
            {
#endif

            switch (Format.Typ)
                {
                    case FormatType.Unknown:
                        if (TryForce(Stream, SubPath.ToString(), Format))
                            break;

                        AddResultUnknown(Stream, Format, SubPath.ToString() + OExtension);
                        if (Deep != 0)
                            Save(Stream, SubPath.ToString(), Format);
                        break;
                case FormatType.Rom:
                case FormatType.Archive:
                        if (!TryExtract(Stream, SubPath.ToString(), Format))
                        {
                            Log.Write(FileAction.Unsupported, SubPath.ToString() + OExtension + $" ~{MathEx.SizeSuffix(Stream.Length, 2)}", $"Description: {Format.GetFullDescription()}");
                            Save(Stream, SubPath.ToString(), Format);
                        }
                        break;
                    default:
                        if (Deep != 0)
                            Save(Stream, SubPath.ToString(), Format);
                        break;
                }
#if !DEBUG
            }
            catch (Exception t)
            {
                Log.WriteEX(t, SubPath.ToString() + OExtension);
                if (Deep != 0)
                    Save(Stream, SubPath.ToString(), Format);
            }
#endif
        }

        private void AddResultUnknown(Stream stream, FormatInfo FormatTypee, in string file)
        {
            if (FormatTypee.Header == null || FormatTypee.Header?.Magic.Length <= 3)
            {
                Log.Write(FileAction.Unknown, file + $" ~{MathEx.SizeSuffix(stream.Length, 2)}",
                    $"Bytes32:[{BitConverter.ToString(stream.Read(32))}]");
            }
            else
            {
                Log.Write(FileAction.Unknown, file + $" ~{MathEx.SizeSuffix(stream.Length, 2)}",
                    $"Magic:[{FormatTypee.Header.Magic}] Bytes:[{string.Join(",", FormatTypee.Header.Bytes)}] Offset:{FormatTypee.Header.Offset}");
            }
        }

    }
}
