using AuroraLib.Texture;
using DolphinTextureExtraction.Scans.Results;
using System.Text;

namespace DolphinTextureExtraction.Scans.Options
{
    public class TextureExtractorOptions : ScanOptions
    {
        /// <summary>
        /// Should Mipmaps files be extracted?
        /// </summary>
        public bool Mips = AppSettings.Mips;

        /// <summary>
        /// use Arbitrary Mipmap Detection.
        /// </summary>
        public bool ArbitraryMipmapDetection = AppSettings.ArbitraryMipmapDetection;

        /// <summary>
        /// Extracts all raw images that are found
        /// </summary>
        public bool Raw = AppSettings.Raw;

        /// <summary>
        /// Tries to Imitate dolphin mipmap detection.
        /// </summary>
        public bool DolphinMipDetection = AppSettings.DolphinMipDetection;

        /// <summary>
        /// Combine texture pairs to get an RGBA texture.
        /// </summary>
        public bool CombinedRGBA = AppSettings.CombinedRGBA;

        public override string ToString()
        {
            StringBuilder sb = new();
            ToString(sb);
            sb.Append(", Enable Mips:");
            sb.Append(Mips);
            sb.Append(", Raw:");
            sb.Append(Raw);
            sb.Append(", DolphinMipDetection:");
            sb.Append(DolphinMipDetection);
            sb.Append(", ArbitraryMipmapDetection:");
            sb.Append(ArbitraryMipmapDetection);
            return sb.ToString();
        }
    }
}
