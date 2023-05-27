namespace AuroraLib.Compression
{
    public static class CompressionEx
    {
        #region Decompress

        /// <summary>
        /// Decompress a file and save it to a new file
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="InFilename">Full path to the file</param>
        /// <param name="OutFilename">Full path to the new file</param>
        public static void Decompress(this ICompression algorithm, in string InFilename, in string OutFilename) => File.WriteAllBytes(OutFilename, algorithm.Decompress(new FileStream(InFilename, FileMode.Open, FileAccess.Read, FileShare.Read)));

        /// <summary>
        /// Decompress a byte[]
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="Data">Compressed data</param>
        /// <returns>Decompressed data</returns>
        public static byte[] Decompress(this ICompression algorithm, in byte[] Data) => algorithm.Decompress(new MemoryStream(Data));

        #endregion Decompress

        #region Compress

        /// <summary>
        /// Compress a file and save it to a new file
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="InFilename">Full path to the file</param>
        /// <param name="OutFilename">Full path to the new file</param>
        public static void Compress(this ICompression algorithm, in string InFilename, in string OutFilename)
        {
            using (var OutFile = new FileStream(OutFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                algorithm.Compress(File.ReadAllBytes(InFilename), OutFile);
            }
        }

        /// <summary>
        /// Compress a byte[]
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="Data">Decompressed data</param>
        /// <returns>Compressed data</returns>
        public static byte[] Compress(this ICompression algorithm, in byte[] Data)
        {
            using (var destination = new MemoryStream())
            {
                algorithm.Compress(in Data, destination);
                return destination.ToArray();
            }
        }

        #endregion Compress
    }
}
