using AuroraLip.Common;

namespace AuroraLip.Compression
{
    public interface ICompression : IFileAccess
    {

        /// <summary>
        /// Data Compress
        /// </summary>
        /// <param name="Data">Decompress data</param>
        /// <returns>Compressed data</returns>
        void Compress(in byte[] source, Stream destination);

        /// <summary>
        /// Data decompression
        /// </summary>
        /// <param name="Data">Compressed data</param>
        /// <returns>Decompress data</returns>
        byte[] Decompress(Stream source);

    }
}
