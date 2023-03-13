
namespace AuroraLib.Compression
{
    public interface ICompressionLevel : ICompression
    {
        /// <summary>
        /// Data Compress
        /// </summary>
        /// <param name="Data">Decompress data</param>
        /// <returns>Compressed data</returns>
        byte[] Compress(byte[] Data, CompressionLevel level);
    }
}