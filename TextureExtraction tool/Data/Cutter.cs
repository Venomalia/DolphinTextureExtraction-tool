using AuroraLip.Archives;
using AuroraLip.Common;
using AuroraLip.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DolphinTextureExtraction_tool
{
    public class Cutter : ScanBase
    {
        private List<byte[]> Pattern;

        internal Cutter(string scanDirectory, string saveDirectory, Options options = null) : base(scanDirectory, saveDirectory, options) { }

        internal Cutter(string scanDirectory, string saveDirectory, List<byte[]> pattern, Options options = null) : base(scanDirectory, saveDirectory, options)
        {
            Pattern = pattern;
        }

        public static Results StartScan(string meindirectory, string savedirectory, List<byte[]> pattern, Options options)
            => StartScan_Async(meindirectory, savedirectory, pattern, options).Result;

        public static async Task<Results> StartScan_Async(string meindirectory, string savedirectory, List<byte[]> pattern, Options options)
        {
            Cutter Extractor = new Cutter(meindirectory, savedirectory, pattern, options);
            return await Task.Run(() => Extractor.StartScan());
        }
        protected override void Scan(Stream Stream, FormatInfo Format, ReadOnlySpan<char> SubPath, int Deep, in string OExtension = "")
        {
#if !DEBUG
            try
            {
#endif
                Archive archive;
                if (Pattern == null)
                    archive = new DataCutter(Stream);
                else
                    archive = new DataCutter(Stream, Pattern);

                if (archive.Root.Count > 0)
                {
                    if (archive.Root.Count == 1)
                        foreach (var item in archive.Root.Items)
                            Save(((ArchiveFile)item.Value).FileData, SubPath.ToString(), Format);
                    else
                        Scan(archive, SubPath.ToString());
                }
#if !DEBUG
            }
            catch (Exception t)
            {
                Log.WriteEX(t, SubPath.ToString() + OExtension);
            }
#endif
        }

        protected override void Scan(Stream stream, string subdirectory, in string Extension = "")
        {
            FormatInfo FFormat = GetFormatTypee(stream, Extension);
            subdirectory = PathEX.WithoutExtension(subdirectory.AsSpan()).ToString();
            Save(stream, subdirectory, FFormat);
        }
    }
}
