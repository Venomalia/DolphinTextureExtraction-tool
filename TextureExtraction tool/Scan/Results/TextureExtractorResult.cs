using AuroraLib.Common;
using DolphinTextureExtraction.Scans.Helper;
using System.Text;

namespace DolphinTextureExtraction.Scans.Results
{
    public class TextureExtractorResult : ScanResults
    {
        public int MinExtractionRate => (int)Math.Round(100d / (ExtractedSize + SkippedSize + UnsupportedSize) * ExtractedSize);

        public int MaxExtractionRate => Extracted > 150 ? (int)Math.Round(100d / (ExtractedSize + SkippedSize / (Extracted / 150) + UnsupportedSize) * ExtractedSize) : MinExtractionRate;

        public int Extracted => Hash.Count;

        public int Unknown { get; private set; } = 0;

        public int Unsupported { get; private set; } = 0;

        internal long ExtractedSize = 0;

        private long UnsupportedSize = 0;

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

        internal void AddUnsupported(ScanObjekt so)
        {
            if (!UnsupportedFormatType.Contains(so.Format))
            {
                UnsupportedFormatType.Add(so.Format);
            }
            Unsupported++;
            UnsupportedSize += so.Stream.Length;
        }

        internal void AddUnknown(ScanObjekt so)
        {
            if (so.Stream.Length > 128)
            {
                if (!UnknownFormatType.Contains(so.Format))
                {
                    UnknownFormatType.Add(so.Format);
                }
            }

            if (so.Deep == 0)
            {
                if (so.Stream.Length > 300)
                    SkippedSize += so.Stream.Length >> 1;
            }
            else
            {
                if (so.Stream.Length > 512)
                    SkippedSize += so.Stream.Length >> 6;
            }
            Unknown++;
        }
    }
}
