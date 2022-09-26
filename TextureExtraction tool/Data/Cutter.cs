using AuroraLip.Archives;
using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
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

        protected override void Scan(FileInfo file)
        {
            Stream stream = new FileStream(file.FullName, FileMode.Open);
            FormatInfo FFormat = GetFormatTypee(stream, file.Extension);
            string subdirectory = GetDirectoryWithoutExtension(file.FullName.Replace(ScanPath + Path.DirectorySeparatorChar, ""));

#if !DEBUG
            try
            {
#endif
                Archive archive;
                if (Pattern == null)
                    archive = new DataCutter(stream);
                else
                    archive = new DataCutter(stream, Pattern);

                if (archive.Root.Count > 0)
                {
                    if (archive.Root.Count == 1)
                        foreach (var item in archive.Root.Items)
                            Save(((ArchiveFile)item.Value).FileData, subdirectory, FFormat);
                    else
                        Scan(archive, subdirectory);
                }
#if !DEBUG
            }
            catch (Exception t)
            {
                Log.WriteEX(t, subdirectory + file.Extension);
            }
#endif
            stream.Close();
        }

        protected override void Scan(Stream stream, string subdirectory, in string Extension = "")
        {
            FormatInfo FFormat = GetFormatTypee(stream, Extension);
            subdirectory = GetDirectoryWithoutExtension(subdirectory);
            Save(stream, subdirectory, FFormat);
        }
    }
}
