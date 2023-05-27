namespace AuroraLib.Compression
{
    /// <summary>
    /// Universal class with methods for compressing and decompressing.
    /// </summary>
    public static class Compression<T> where T : ICompression, new()
    {
        /// <summary>
        /// Can be decompress.
        /// </summary>
        public static bool CanDecompress { get => new T().CanRead; }

        /// <summary>
        /// Can be compress.
        /// </summary>
        public static bool CanCompress { get => new T().CanWrite; }

        #region Decompress

        /// <summary>
        /// Data decompression
        /// </summary>
        /// <param name="Data">Compressed data</param>
        /// <returns>Decompress data</returns>
        public static byte[] Decompress(Stream source) => new T().Decompress(source);

        /// <summary>
        /// Decompress a file and save it to a new file
        /// </summary>
        /// <param name="InFilename">Full path to the file</param>
        /// <param name="OutFilename">Full path to the new file</param>
        public static void Decompress(in string InFilename, in string OutFilename) => new T().Decompress(InFilename, OutFilename);

        /// <summary>
        /// Decompress a byte[]
        /// </summary>
        /// <param name="Data">Compressed data</param>
        /// <returns>Decompressed data</returns>
        public static byte[] Decompress(in byte[] Data) => new T().Decompress(in Data);

        #endregion Decompress

        #region Compress

        /// <summary>
        /// Data Compress
        /// </summary>
        /// <param name="Data">Decompress data</param>
        /// <returns>Compressed data</returns>
        public static void Compress(in byte[] source, Stream destination) => new T().Compress(source, destination);

        /// <summary>
        /// Compress a file and save it to a new file
        /// </summary>
        /// <param name="InFilename">Full path to the file</param>
        /// <param name="OutFilename">Full path to the new file</param>
        public static void Compress(in string InFilename, in string OutFilename) => new T().Compress(InFilename, OutFilename);

        /// <summary>
        /// Compress a byte[]
        /// </summary>
        /// <param name="Data">Decompressed data</param>
        /// <returns>Compressed data</returns>
        public static byte[] Compress(in byte[] Data) => new T().Compress(in Data);

        #endregion Compress

        /// <summary>
        /// Checks if the data compressed with this compression method
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>"True" if it corresponds to the compression method.</returns>
        public static bool IsMatch(Stream Data) => new T().IsMatch(Data);
    }
}
