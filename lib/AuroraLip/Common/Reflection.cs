using AuroraLip.Compression;

namespace AuroraLip.Common
{
    public static class Reflection
    {
        public static FileAccessReflection<IFileAccess> FileAccess = new FileAccessReflection<IFileAccess>();

        public static CompressionReflection Compression = new CompressionReflection();
    }
}
