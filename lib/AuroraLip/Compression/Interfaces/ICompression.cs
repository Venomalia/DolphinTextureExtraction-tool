namespace AuroraLip.Compression
{
    public interface ICompression
    {
        /// <summary>
        /// Can be compress.
        /// </summary>
        bool CanCompress { get; }

        /// <summary>
        /// Can be decompress.
        /// </summary>
        bool CanDecompress { get; }

        /// <summary>
        /// Data Compress
        /// </summary>
        /// <param name="Data">Decompress data</param>
        /// <returns>Compressed data</returns>
        byte[] Compress(in byte[] Data);

        /// <summary>
        /// Data decompression
        /// </summary>
        /// <param name="Data">Compressed data</param>
        /// <returns>Decompress data</returns>
        byte[] Decompress(in byte[] Data);

        /// <summary>
        /// Checks if the data compressed with this compression method
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>"True" if it corresponds to the compression method.</returns>
        bool IsMatch(in byte[] Data);
    }
}