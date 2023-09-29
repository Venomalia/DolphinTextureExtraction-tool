using LibCPK;
using System.Collections.Specialized;
using System.Configuration;

namespace DolphinTextureExtraction
{
    internal static class AppSettings
    {
        public static readonly NameValueCollection Config;

        public static readonly bool UseConfig;

        public static bool DryRun = false;

        public static bool Force = false;

        public static uint Deep = 0;

        public static bool Mips = false;

        public static bool ArbitraryMipmapDetection = true;

        public static bool Raw = false;

        public static bool DolphinMipDetection = true;

        public static bool CombinedRGBA = false;

#if DEBUG
        public static ParallelOptions Parallel = new() { MaxDegreeOfParallelism = 1 };
#else
        public static readonly ParallelOptions Parallel = new() { MaxDegreeOfParallelism = 4 };
#endif

        static AppSettings()
        {
            Config = ConfigurationManager.AppSettings;
            UseConfig = Config.HasKeys() && bool.TryParse(Config.Get("UseConfig"), out bool value) && value;

            if (UseConfig)
            {
                if (bool.TryParse(Config.Get("DryRun"), out value))
                    DryRun = value;
                if (bool.TryParse(Config.Get("Force"), out value))
                    Force = value;
                if (uint.TryParse(Config.Get("Deep"), out uint deep))
                    Deep = deep;
                if (int.TryParse(Config.Get("Tasks"), out int thing))
                    Parallel.MaxDegreeOfParallelism = thing <= 0 ? 1 : thing;
                if (bool.TryParse(Config.Get("Mips"), out value))
                    Mips = value;
                if (bool.TryParse(Config.Get("Raw"), out value))
                    Raw = value;
                if (bool.TryParse(Config.Get("DolphinMipDetection"), out value))
                    DolphinMipDetection = value;
                if (bool.TryParse(Config.Get("ArbitraryMipmapDetection"), out value))
                    ArbitraryMipmapDetection = value;
                if (bool.TryParse(Config.Get("CombinedRGBA"), out value))
                    CombinedRGBA = value;
            }
        }
    }
}
