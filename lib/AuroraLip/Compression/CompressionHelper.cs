using AuroraLib.Compression;

namespace AuroraLib.Compression
{
    public static class CompressionHelper
    {
        public static System.IO.Compression.CompressionLevel ToSystemIO(this CompressionLevel level) => level switch
        {
            CompressionLevel.NoCompression => System.IO.Compression.CompressionLevel.NoCompression,
            CompressionLevel.Optimal => System.IO.Compression.CompressionLevel.Optimal,
            CompressionLevel.Fastest => System.IO.Compression.CompressionLevel.Fastest,
            CompressionLevel.SmallestSize => System.IO.Compression.CompressionLevel.SmallestSize,
            _ => throw new NotImplementedException(),
        };

        public static CompressionLevel ToAurora(this System.IO.Compression.CompressionLevel level) => level switch
        {
            System.IO.Compression.CompressionLevel.NoCompression => CompressionLevel.NoCompression,
            System.IO.Compression.CompressionLevel.Optimal => CompressionLevel.Optimal,
            System.IO.Compression.CompressionLevel.Fastest => CompressionLevel.Fastest,
            System.IO.Compression.CompressionLevel.SmallestSize => CompressionLevel.SmallestSize,
            _ => throw new NotImplementedException(),
        };
    }
}
