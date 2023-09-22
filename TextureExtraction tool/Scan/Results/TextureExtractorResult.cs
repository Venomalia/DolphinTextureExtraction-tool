using AuroraLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DolphinTextureExtraction.Scans.Results
{
    public class TextureExtractorResult : ScanResults
    {
        public int MinExtractionRate => (int)Math.Round(100d / (ExtractedSize + SkippedSize + UnsupportedSize) * ExtractedSize);

        public int MaxExtractionRate => Extracted > 150 ? (int)Math.Round(100d / (ExtractedSize + SkippedSize / (Extracted / 150) + UnsupportedSize) * ExtractedSize) : MinExtractionRate;

        public int Extracted => Hash.Count;

        public int Unknown { get; internal set; } = 0;

        public int Unsupported { get; internal set; } = 0;

        public int Skipped { get; internal set; } = 0;

        internal long ExtractedSize = 0;

        internal long UnsupportedSize = 0;

        internal long SkippedSize = 0;

        /// <summary>
        /// List of hash values of the extracted textures
        /// </summary>
        public List<int> Hash = new();

        public List<FormatInfo> UnsupportedFormatType = new();

        public List<FormatInfo> UnknownFormatType = new();

        public string GetExtractionSize() => MinExtractionRate + MinExtractionRate / 10 >= MaxExtractionRate ? $"{(MinExtractionRate + MaxExtractionRate) / 2}%" : $"{MinExtractionRate}% - {MaxExtractionRate}%";

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Extracted textures: {Extracted}");
            sb.AppendLine($"Unsupported files: {Unsupported}");
            if (Unsupported != 0) sb.AppendLine($"Unsupported files Typs: {string.Join(", ", UnsupportedFormatType.Select(x => x.GetFullDescription()))}");
            sb.AppendLine($"Unknown files: {Unknown}");
            if (UnknownFormatType.Count != 0) sb.AppendLine($"Unknown files Typs: {string.Join(", ", UnknownFormatType.Select(x => x.GetTypName()))}");
            sb.AppendLine($"Extraction rate: ~ {GetExtractionSize()}");
            sb.AppendLine($"Scan time: {TotalTime.TotalSeconds:.000}s");
            return sb.ToString();
        }
    }
}
