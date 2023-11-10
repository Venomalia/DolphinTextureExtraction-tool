using AuroraLib.Compression;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Common
{
    public static class Reflection
    {
        public static FileAccessReflection<IFormatRecognition> FileAccess = new();

        public static CompressionReflection Compression = new();
    }
}
